# using Winston
using Distributions
using Stats
using KernelDensity
using Grid            # for interpolation

# KernelDensity.Winston_init()

function oplot(xs::AbstractArray, f::Function, args...; kwargs...)
  Winston.oplot(xs, map(f, xs), args...;  kwargs...)
end

function oplot(xs::AbstractArray, k::UnivariateKDE, args...; kwargs...)
  Winston.oplot(xs, k.density, args...;  kwargs...)
end

function oplot(k::UnivariateKDE, args...; kwargs...)
  Winston.oplot(k.x, k.density, args...;  kwargs...)
end


gmean(A) = prod(A)^(1/length(A))

function build_bootstrap_distribution(orig_sample, n_samples, statistic_fn, n_observations_per_sample = length(orig_sample))
  type_of_statistic = typeof(statistic_fn(sample(orig_sample, 1)))
  statistics = Array(type_of_statistic, n_samples)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    statistics[i] = statistic_fn(bootstrap_sample)
  end
  statistics
end

function build_bootstrap_distribution_from_normal(orig_sample, n_samples, statistic_fn, sigma = std(orig_sample), n_observations_per_sample = length(orig_sample))
  type_of_statistic = typeof(statistic_fn(sample(orig_sample, 1)))
  statistics = Array(type_of_statistic, n_samples)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    normal_bootstrap_sample = map((x) -> rand(Normal(x, sigma), 1) |> first, bootstrap_sample)
    statistics[i] = statistic_fn(normal_bootstrap_sample)
  end
  statistics
end

function build_distribution(orig_sample, n_samples, n_observations_per_sample, statistic_fn, return_distribution)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    return_distribution[i] = statistic_fn(bootstrap_sample)
  end
  return_distribution
end

# assumes all the statistics take on a Float64 value
# returns:
#   a number_of_statistics X n_bootstrap_samples matrix
function build_bootstrap_distributions(orig_sample, n_bootstrap_samples, multi_statistic_fn, number_of_statistics = length(multi_statistic_fn([1,2,3])))
  n_observations_per_sample = length(orig_sample)
  bootstrap_distributions = Array(Float64, number_of_statistics, n_bootstrap_samples)   # number_of_statistics X n_bootstrap_samples matrix
  for i in 1:n_bootstrap_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    bootstrap_distributions[:, i] = multi_statistic_fn(bootstrap_sample)
  end
  bootstrap_distributions
end

# percentile_sampling_distributions:
#   an array of percentile sampling distributions (i.e. a number_of_statistics X n_bootstrap_samples matrix) computed on the same statistic - one sampling
#   distribution for each percentile, from 0 to 100.
# confidence_level:
#   a percentage indicating how much of the central volume of the sampling distributions to include in the computation of the min and max distributions
#
# returns:
#   a 2 X 101 matrix representing the min and max distributions
function build_min_max_percentile_distributions(percentile_sampling_distributions, confidence_level)
  size(percentile_sampling_distributions, 1) == 101 || error("percentile_sampling_distributions should contain 101 sampling distributions")

  half_confidence_level = confidence_level / 100 / 2
  min_p = max(0.5 - half_confidence_level, 0.0)
  max_p = min(0.5 + half_confidence_level, 1.0)

  min_max_distributions = Array(Float64, 2, 101)   # 2 X 101 matrix

  for i in 1:101
    samp_dist = vec(percentile_sampling_distributions[i, :])
    min_max_distributions[:, i] = quantile(samp_dist, [min_p, max_p])
  end

  min_max_distributions
end

# computes 101 percentiles of the sample: 0.001 0.01 0.02 0.03 0.04 0.05 0.06 0.07 ... 0.95 0.96 0.97 0.98 0.99 0.999
function percentiles(sample)
  # quantile(sample, [0:0.01:1])
  quantile(sample, vcat([0.001], [0.01:0.01:0.99], [0.999]))
end

# returns:
#   a 2 X 101 matrix representing the min and max distributions
function build_min_max_distributions(orig_sample, n_bootstrap_samples, confidence_level)
  percentile_sampling_distributions = build_bootstrap_distributions(orig_sample, n_bootstrap_samples, percentiles)
  build_min_max_percentile_distributions(percentile_sampling_distributions, confidence_level)
end

# return_observation_sample:
#   a sample of short-period return observations
# n_periods_per_long_period:
#   the number of short-periods in a long-period
# Example:
#   annualize daily returns:
#     build_monte_carlo_simulated_return_dist(weekly_observations, 1000, 52)
function build_monte_carlo_simulated_return_dist(return_observation_sample, mc_samples, n_periods_per_long_period)
  build_bootstrap_distribution(return_observation_sample, mc_samples, prod, n_periods_per_long_period)
end

function calculate_confidence_interval(dist, confidence_level)
  half_confidence_level = confidence_level / 100 / 2
  min_p = max(0.5 - half_confidence_level, 0.0)
  max_p = min(0.5 + half_confidence_level, 1.0)
  quantile(dist, [min_p, max_p])
end

# returns:
#   an array of (min, max) pairs per statistic calculated by multi_statistic_fn
function calculate_composite_monte_carlo_confidence_intervals(orig_sample, n_bootstrap_samples, confidence_level, mc_samples, n_periods_per_long_period, multi_statistic_fn)
  distributions = build_min_max_distributions(orig_sample, n_bootstrap_samples, confidence_level)
  min_dist_short_period = vec(distributions[1, :])
  max_dist_short_period = vec(distributions[2, :])

  min_dist_long_period = build_monte_carlo_simulated_return_dist(min_dist_short_period, mc_samples, n_periods_per_long_period)
  max_dist_long_period = build_monte_carlo_simulated_return_dist(max_dist_short_period, mc_samples, n_periods_per_long_period)
  println(compute_samp_dist_stats(min_dist_long_period))
  println(compute_samp_dist_stats(max_dist_long_period))

  min_sampling_distributions = build_bootstrap_distributions(min_dist_long_period, n_bootstrap_samples, multi_statistic_fn)
  max_sampling_distributions = build_bootstrap_distributions(max_dist_long_period, n_bootstrap_samples, multi_statistic_fn)

  number_of_statistics = size(min_sampling_distributions, 1)

  confidence_intervals = Array((Float64, Float64), number_of_statistics)
  for i in 1:number_of_statistics
    min_dist = vec(min_sampling_distributions[i, :])
    max_dist = vec(max_sampling_distributions[i, :])
    min_ci_lower_bound, min_ci_upper_bound = calculate_confidence_interval(min_dist, 100)
    max_ci_lower_bound, max_ci_upper_bound = calculate_confidence_interval(max_dist, 100)
    composite_ci_lower_bound = min_ci_lower_bound
    composite_ci_upper_bound = max_ci_upper_bound
    confidence_intervals[i] = (composite_ci_lower_bound, composite_ci_upper_bound)
  end
  confidence_intervals
end

# taken from http://www.johndcook.com/julia_rng.html
function rand_uniform(a, b)
  a + rand()*(b - a)
end

function rand_uniforms(a, b, n)
  map((x) -> rand_uniform(a, b), 1:n)
end

# return a random sample from a normal (Gaussian) distribution
# taken from http://www.johndcook.com/julia_rng.html
function rand_normal(mean, stdev)
  if stdev <= 0.0
      error("standard deviation must be positive")
  end
  u1 = rand()
  u2 = rand()
  r = sqrt( -2.0*log(u1) )
  theta = 2.0*pi*u2
  mean + stdev*r*sin(theta)
end

function rand_normals(mean, stdev, n)
  map((x) -> rand_normal(mean, stdev), 1:n)
end

# returns:
#   an array of MC simulated long-period return observations
function build_randomized_min_max_monte_carlo_return_distribution(orig_sample, n_bootstrap_samples, confidence_level, mc_samples, n_periods_per_long_period)
  distributions = build_min_max_distributions(orig_sample, n_bootstrap_samples, confidence_level)
  min_dist_short_period = vec(distributions[1, :])
  max_dist_short_period = vec(distributions[2, :])

  composite_return_dist_short_period = reduce(
    vcat,
    map(
      (pair) -> begin
        (min, max) = pair
        rand_uniforms(min, max, 1000)
      end,
      zip(min_dist_short_period, max_dist_short_period)
    )
  )

  mc_return_dist_long_period = build_monte_carlo_simulated_return_dist(composite_return_dist_short_period, mc_samples, n_periods_per_long_period)
end

function sample_from_kde_dist(kde_dist, n)
  x = kde_dist.x
  density = kde_dist.density
  # todo: finish this
end

function build_kde_mc_return_distribution(orig_sample, n_bootstrap_samples, mc_samples, n_periods_per_long_period)
  kde_dist = kde(orig_sample)
  return_dist_short_period = sample_from_kde_dist(kde_dist, 5000)
  mc_return_dist_long_period = build_monte_carlo_simulated_return_dist(return_dist_short_period, mc_samples, n_periods_per_long_period)
end

# Silverman's rule of thumb for KDE bandwidth selection
function silverman_bandwidth(data, alpha::Float64 = 0.9)
  # Determine length of data
  ndata = length(data)
  ndata <= 1 && return alpha

  # Calculate width using variance and IQR
  var_width = std(data)
  q25, q75 = quantile(data, [0.25, 0.75])
  quantile_width = (q75 - q25) / 1.34

  # Deal with edge cases with 0 IQR or variance
  width = min(var_width, quantile_width)
  if width == 0.0
    if var_width == 0.0
      width = 1.0
    else
      width = var_width
    end
  end

  # Set bandwidth using Silverman's rule of thumb
  return alpha * width * ndata ^ (-0.2)
end

function build_kde_distribution(orig_sample, n)
  h = silverman_bandwidth(orig_sample)
  map(x -> rand_normal(x, h), sample(orig_sample, n))
end

function build_sampling_distribution(distribution, n_samples, n_observations_per_sample, statistic_fn)
  statistics = Array(Float64, n_samples)
  for i in 1:n_samples
    sample_observations = sample(distribution, n_observations_per_sample)
    statistics[i] = statistic_fn(sample_observations)
  end
  statistics
end

# returns (mean, std_dev, 99%-range, 0.005%-ile, 0.5%-ile, 0.995%-ile)
function compute_dist_stats(samp_dist, expected_mean, return_array = Array(Float64, 6); print = true, prefix = "")
  # compute statistics of sampling distribution
  q005, q01, q05, q1, q15, q2, q25, q3, q35, q4, q45, q5, q55, q6, q65, q7, q75, q8, q85, q9, q95, q99, q995 = round(quantile(samp_dist, [0.005, 0.01, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99, 0.995]), 5)
  mean_of_samp_dist = round(mean(samp_dist), 5)
  std_of_samp_dist = round(std(samp_dist), 5)
  range_99th_percentile = round(abs(q995 - q005), 3)

  is_accurate = q005 <= expected_mean <= q995

  if print
    println("$prefix   accurate=$is_accurate   mean=$mean_of_samp_dist   std=$std_of_samp_dist   range=$range_99th_percentile   $q005 --- $q5 --- $q995")
  end

  return_array[1] = mean_of_samp_dist
  return_array[2] = std_of_samp_dist
  return_array[3] = range_99th_percentile
  return_array[4] = q005
  return_array[5] = q5
  return_array[6] = q995
  return_array
end

# q005, q5, q995 are the quantiles of the distribution of observed [daily/weekly/monthly] returns
# median_simulated_annual_return is the 50th percentile of the monte carlo simulated 1-year return distribution
function revise_ci(q005, q5, q995, median_simulated_annual_return)
  revised_q005 = (q005 / q5) * median_simulated_annual_return
  revised_q995 = (q995 / q5) * median_simulated_annual_return
  [revised_q005, revised_q995]
end

function compute_samp_dist_stats(samp_dist)
  # compute statistics of sampling distribution
  q005, q01, q05, q1, q15, q2, q25, q3, q35, q4, q45, q5, q55, q6, q65, q7, q75, q8, q85, q9, q95, q99, q995 = round(quantile(samp_dist, [0.005, 0.01, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99, 0.995]), 3)
  mean_of_samp_dist = round(mean(samp_dist), 4)
  std_of_samp_dist = round(std(samp_dist), 4)
  range_99th_percentile = round(abs(q995 - q005), 3)

  report = "mean=$mean_of_samp_dist   std=$std_of_samp_dist   range=$range_99th_percentile   0.5%=$q005  1%=$q01  5%=$q05  10%=$q1  20%=$q2  30%=$q3  40%=$q4  50%=$q5  60%=$q6  70%=$q7  80%=$q8  90%=$q9  95%=$q95  99%=$q99  99.5%=$q995"

  mean_of_samp_dist, std_of_samp_dist, range_99th_percentile, q005, q5, q995, report
end

function build_simple_distribution(n_observations, build_observation_fn)
  type_of_observation = typeof(build_observation_fn())
  observations = Array(type_of_observation, n_observations)
  for i in 1:n_observations
    observations[i] = build_observation_fn()
  end
  observations
end

# see http://en.wikipedia.org/wiki/Kernel_(statistics)
# function normal_kernel(x)
#   1 / sqrt(2pi) * exp(-(x ^ 2 / 2))
# end

# naive implementation of a kernel density estimate
# see http://en.wikipedia.org/wiki/Kernel_density_estimation
# function kde_pdf(xs, kernel = normal_kernel, h = silverman_bandwidth(xs))
#   n = length(xs)
#   function(x)
#     sum(map(x_i -> kernel((x - x_i) / h), xs)) / (n * h)
#   end
# end

# convert UnivariateKDE to InterpKDE-based pdf
function kde_to_pdf(kde::UnivariateKDE)
  ikde = InterpKDE(kde)
  function(x)
    max(pdf(ikde, x), 0)
  end
end

# kde with interpolation
function kde_pdf(xs)
  kde(xs) |> kde_to_pdf
end

# returns the MSE indicating how closely a density estimate represents a reference density
function mean_squared_error(xs::AbstractArray{Float64}, reference_pdf::Function, estimated_pdf::Function)
  mean((estimated_pdf(xs) - reference_pdf(xs)) .^ 2)
end

function span_of_kdes(kde1::UnivariateKDE, kde2::UnivariateKDE, increment = 0.1)
  min(kde1.x |> first, kde2.x |> first):increment:max(kde1.x |> last, kde2.x |> last)
end

function mean_absolute_error(xs::AbstractArray{Float64}, reference_pdf::Function, estimated_pdf::Function)
  mean(abs(estimated_pdf(xs) - reference_pdf(xs)))
end

# trial runs:
# 12 periods per year:
# sampling distribution of mean daily mae=mean=0.5046    std=0.1402   range=0.687   0.5%=0.329  1%=0.337  5%=0.357  10%=0.364  20%=0.397  30%=0.423  40%=0.445  50%=0.474  60%=0.487  70%=0.531  80%=0.591  90%=0.686  95%=0.766  99%=0.984  99.5%=1.016
# sampling distribution of mean annual mae=mean=0.1579   std=0.047   range=0.231   0.5%=0.084  1%=0.084  5%=0.105  10%=0.113  20%=0.122  30%=0.132  40%=0.136  50%=0.149  60%=0.158  70%=0.163  80%=0.183  90%=0.229  95%=0.244  99%=0.304  99.5%=0.315
# 52 periods:
# sampling distribution of mean daily mae=mean=0.497     std=0.1176   range=0.5   0.5%=0.329  1%=0.34  5%=0.354  10%=0.367  20%=0.381  30%=0.42  40%=0.445  50%=0.483  60%=0.512  70%=0.548  80%=0.584  90%=0.654  95%=0.7  99%=0.822  99.5%=0.829
# sampling distribution of mean annual mae=mean=0.1471   std=0.0481   range=0.223   0.5%=0.095  1%=0.097  5%=0.1  10%=0.106  20%=0.114  30%=0.117  40%=0.121  50%=0.131  60%=0.136  70%=0.156  80%=0.182  90%=0.206  95%=0.244  99%=0.297  99.5%=0.318
# 126 periods:
# sampling distribution of mean daily mae=mean=0.6228    std=0.1563   range=0.662   0.5%=0.393  1%=0.402  5%=0.44  10%=0.447  20%=0.486  30%=0.524  40%=0.56  50%=0.585  60%=0.627  70%=0.665  80%=0.733  90%=0.864  95%=0.969  99%=1.025  99.5%=1.055
# sampling distribution of mean annual mae=mean=0.143    std=0.0496   range=0.205   0.5%=0.092  1%=0.095  5%=0.1  10%=0.101  20%=0.109  30%=0.115  40%=0.118  50%=0.124  60%=0.132  70%=0.144  80%=0.168  90%=0.235  95%=0.259  99%=0.294  99.5%=0.297
# 251 periods:
# sampling distribution of mean daily mae=mean=0.8795    std=0.3085   range=1.263   0.5%=0.494  1%=0.497  5%=0.54  10%=0.564  20%=0.617  30%=0.679  40%=0.735  50%=0.791  60%=0.874  70%=0.961  80%=1.114  90%=1.312  95%=1.528  99%=1.727  99.5%=1.757
# sampling distribution of mean annual mae=mean=0.1341   std=0.0421   range=0.203   0.5%=0.088  1%=0.091  5%=0.096  10%=0.099  20%=0.104  30%=0.109  40%=0.114  50%=0.119  60%=0.128  70%=0.136  80%=0.162  90%=0.195  95%=0.232  99%=0.256  99.5%=0.291
function main()
  for n_periods_per_year in [12, 52, 126, 251]

    # n_periods_per_year = 251
    annual_return = 1.15
    annual_std_dev = 0.4
    mean_return_per_period = annual_return ^ (1/n_periods_per_year)
    return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)

    n = 100000
    xs = 0:0.001:3

    non_negative_rand_normals(n) = max(rand_normals(mean_return_per_period, return_std_dev_per_period, n), 0)
    prod_of_non_negative_rand_normals(n) = non_negative_rand_normals(n) |> prod

    println("single normal")
    true_short_period_return_dist = build_simple_distribution(n, () -> prod_of_non_negative_rand_normals(1))
    ref_daily_kde = kde(true_short_period_return_dist)
    ref_daily_kde_pdf = ref_daily_kde |> kde_to_pdf
    true_short_period_mu, true_short_period_sigma, true_short_period_range, true_short_period_low, true_short_period_median, true_short_period_high, true_short_period_report = compute_samp_dist_stats(true_short_period_return_dist)
    println(true_short_period_report)
    # p = oplot(xs, kde(dist), "k-")

    println("multiply $n_periods_per_year normals")
    true_annual_return_dist = build_simple_distribution(n, () -> prod_of_non_negative_rand_normals(n_periods_per_year))
    ref_annual_kde = kde(true_annual_return_dist)
    ref_annual_kde_pdf = ref_annual_kde |> kde_to_pdf
    true_annual_mu, true_annual_sigma, true_annual_range, true_annual_low, true_annual_median, true_annual_high, true_annual_report = compute_samp_dist_stats(true_annual_return_dist)
    println(true_annual_report)
    # p = oplot(xs, kde(dist), "b-")
    # p = oplot(xs, kde_lscv(dist), "y-")

    # sampling distribution of arithmetic mean return (annualized)
    # println("sampling dist of mean annual return")
    # samp_dist = build_sampling_distribution(dist, 10000, 10000, mean)
    # println(compute_samp_dist_stats(samp_dist))

    short_period_kde_dist_observation_count = 10000
    long_period_mc_dist_observation_count = 10000
    number_of_years_worth_of_short_period_data = 3
    short_period_sample_size = n_periods_per_year * number_of_years_worth_of_short_period_data

    # samp_dist_of_mean_daily_mse = Float64[]
    # samp_dist_of_mean_annual_mse = Float64[]
    number_of_accurate_samp_dist_of_mean_short_period_returns = 0
    number_of_accurate_samp_dist_of_mean_annual_returns = 0
    number_of_accurate_samp_dist_of_median_annual_returns = 0
    ranges_of_samp_dist_of_mean_annual_return = Float64[]


    trial_count = 100
    bootstrap_sample_count = 100

    for i in 1:trial_count
      println("trial $i:")

      short_period_sample = non_negative_rand_normals(short_period_sample_size)

      # daily_mses = Float64[]
      # annual_mses = Float64[]
      samp_dist_mean_short_period_return = Float64[]
      samp_dist_mean_annual_return = Float64[]
      samp_dist_median_annual_return = Float64[]

      # construct bootstrap samples from the short period sample
      for j in 1:bootstrap_sample_count
        # println("\nbootstrap $j:")


        # generate bootstrap daily sample
        bootstrap_short_period_sample = sample(short_period_sample, short_period_sample_size)


        ################# generate bootstrapped daily kde #################

        # println("bootstrap daily kde dist (sample of $short_period_sample_size)")
        short_period_dist = build_kde_distribution(bootstrap_short_period_sample, short_period_kde_dist_observation_count)

        mean_short_period_return = mean(short_period_dist)
        push!(samp_dist_mean_short_period_return, mean_short_period_return)

        # est_daily_kde = kde(short_period_dist)
        # est_daily_kde_pdf = est_daily_kde |> kde_to_pdf
        # # daily_mse = mean_squared_error(span_of_kdes(ref_daily_kde, est_daily_kde), ref_daily_kde_pdf, est_daily_kde_pdf)
        # daily_mse = mean_absolute_error(span_of_kdes(ref_daily_kde, est_daily_kde), ref_daily_kde_pdf, est_daily_kde_pdf)
        # push!(daily_mses, daily_mse)

        # compute_dist_stats(short_period_dist, mean_return_per_period)
        # p = oplot(xs, kde(bootstrap_short_period_sample), "c--")      # should nearly overlap the "single normal"
        # p = oplot(xs, kde_lscv(bootstrap_short_period_sample), "r-")      # should nearly overlap the "single normal"


        ################# generate MC annual kde from bootstrapped daily kde #################

        # println("multiply $n_periods_per_year random observations from kde-estimated daily dist")
        annual_dist = build_monte_carlo_simulated_return_dist(short_period_dist, long_period_mc_dist_observation_count, n_periods_per_year)

        mean_annual_return = mean(annual_dist)
        push!(samp_dist_mean_annual_return, mean_annual_return)

        median_annual_return = median(annual_dist)
        push!(samp_dist_median_annual_return, median_annual_return)

        # est_annual_kde = kde(annual_dist)
        # est_annual_kde_pdf = est_annual_kde |> kde_to_pdf
        # # annual_mse = mean_squared_error(span_of_kdes(ref_annual_kde, est_annual_kde), ref_annual_kde_pdf, est_annual_kde_pdf)
        # annual_mse = mean_absolute_error(span_of_kdes(ref_annual_kde, est_annual_kde), ref_annual_kde_pdf, est_annual_kde_pdf)
        # push!(annual_mses, annual_mse)

        # println(compute_samp_dist_stats(annual_dist))
        # p = oplot(xs, kde(annual_dist), "m:")        # should nearly overlap the "multiply 251 normals"
      end


      println("------")
      mu, sigma, range, low, mid, high, report = compute_samp_dist_stats(samp_dist_mean_short_period_return)
      println("samp dist mean short period return=$report")
      if low <= mean_return_per_period <= high
        number_of_accurate_samp_dist_of_mean_short_period_returns += 1
      end

      mu, sigma, range, low, mid, high, report = compute_samp_dist_stats(samp_dist_mean_annual_return)
      println("samp dist mean annual return=$report")
      if low <= true_annual_mu <= high
        number_of_accurate_samp_dist_of_mean_annual_returns += 1
      end
      push!(ranges_of_samp_dist_of_mean_annual_return, range)

      mu, sigma, range, low, mid, high, report = compute_samp_dist_stats(samp_dist_median_annual_return)
      println("samp dist median annual return=$report")
      if low <= true_annual_median <= high
        number_of_accurate_samp_dist_of_median_annual_returns += 1
      end

      # mean_daily_mse = mean(daily_mses)
      # println("mean daily mse=$mean_daily_mse")
      # push!(samp_dist_of_mean_daily_mse, mean_daily_mse)

      # mean_annual_mse = mean(annual_mses)
      # println("mean annual mse=$mean_annual_mse")
      # push!(samp_dist_of_mean_annual_mse, mean_annual_mse)
      println("------")


      # display(p)
      # read(STDIN, Char)
      # exit()

    end


    println("\n======")
    println("%age accurate sampling distribution of mean short period return=$(number_of_accurate_samp_dist_of_mean_short_period_returns / trial_count))")
    println("%age accurate sampling distribution of mean annual return=$(number_of_accurate_samp_dist_of_mean_annual_returns / trial_count))")
    println("%age accurate sampling distribution of median annual return=$(number_of_accurate_samp_dist_of_median_annual_returns / trial_count))")
    println("mean of 99th percentile ranges of samp dist of mean annual return=$(mean(ranges_of_samp_dist_of_mean_annual_return))")
    # println("sampling distribution of mean daily mse=$(compute_samp_dist_stats(samp_dist_of_mean_daily_mse))")
    # println("sampling distribution of mean annual mse=$(compute_samp_dist_stats(samp_dist_of_mean_annual_mse))")
    println("======\n")

  end
end

main()
