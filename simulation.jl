using Distributions
using Stats

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
  quantile(sample, cat(1, [0.001], [0.01:0.01:0.99], [0.999]))
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

# function build_sampling_distribution(period_returns, n_samples, n_observations_per_sample, n_periods, statistic_fn)
#   statistics = Array(Float64, n_samples)
#   cumulative_returns = Array(Float64, n_observations_per_sample)
#   for i in 1:n_samples
#
#     for j in 1:n_observations_per_sample
#       n_period_observations = sample(period_returns, n_periods)
#       cumulative_returns[j] = prod(n_period_observations) ^ (52/n_periods)
#     end
#
#     statistics[i] = statistic_fn(cumulative_returns)
#   end
#   statistics
# end

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

  "mean=$mean_of_samp_dist   std=$std_of_samp_dist   range=$range_99th_percentile   0.5%=$q005  1%=$q01  5%=$q05  10%=$q1  20%=$q2  30%=$q3  40%=$q4  50%=$q5  60%=$q6  70%=$q7  80%=$q8  90%=$q9  95%=$q95  99%=$q99  99.5%=$q995"
end

# compute sampling distribution of simulated annual returns
function main()
  n_periods_per_year = 252
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)

  n_return_observations = 251
  n_annualized_returns = 10000
  n_samples = 10000
  n_observations_per_sample = n_return_observations

  annualized_returns = Array(Float64, n_annualized_returns)

  n_iterations = 1
  mean_annualized_returns = Array(Float64, n_iterations)

  # construct original sample of observations
  return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

  for i in 1:n_iterations
    println("$i - $n_return_observations return observations")

    # annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)), n_periods_per_year)
    annualized_returns = build_distribution(return_observations, n_annualized_returns, n_periods_per_year, prod, annualized_returns)

    stats = compute_dist_stats(annualized_returns, annual_return)
    println(compute_samp_dist_stats(annualized_returns))
    mean_annualized_returns[i] = stats[1]

    samp_dist_of_mean_annual_return = build_bootstrap_distribution(annualized_returns, n_samples, mean, n_observations_per_sample)

    # samp_dist_of_mean_annual_return = build_sampling_distribution(return_observations, n_samples, n_observations_per_sample, n_periods, mean)

    mu, sigma, range, q005, q5, q995 = compute_dist_stats(samp_dist_of_mean_annual_return, annual_return)
  end

  println("mean of means=$(mean(mean_annualized_returns))   std of means=$(std(mean_annualized_returns))")
end

# this shows how frequently the 99%-ile confidence interval of the monte-carlo simulated 1-year return distributions include the actual annual return
# for example:
#
# n_periods_per_year = 251
# =================================================================
# 21 observations
# 51.1% accurate
#
# =================================================================
# 63 observations
# 79.3% accurate
#
# =================================================================
# 126 observations
# 94.1% accurate
#
# =================================================================
# 251 observations
# 98.8% accurate
#
# =================================================================
# 502 observations
# 100.0% accurate
#
# =================================================================
# 1255 observations
# 100.0% accurate
#
# =================================================================
# 2510 observations
# 100.0% accurate
#
function main2()
  n_periods_per_year = 52  # 252*13   # every 30 mins
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)
  # return_dist = Uniform(mean_return_per_period - return_std_dev_per_period, mean_return_per_period + return_std_dev_per_period)

  # construct original sample of observations
  # n_return_observations = 63
  # return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

  n_samples = 5000
  mc_samples = 5000

  for n_return_observations in [
        round(n_periods_per_year/12) |> int64,    # 1 month
        round(n_periods_per_year/4) |> int64,     # 1 quarter
        round(n_periods_per_year/2) |> int64,     # half year
        n_periods_per_year,                       # 1 year
        n_periods_per_year*2,
        n_periods_per_year*5,
        n_periods_per_year*10
      ]

    number_accurate_samp_dists_of_geom_mean_annualized = 0
    number_accurate_samp_dists_of_arith_mean_annualized = 0
    number_accurate_annual_return_distributions = 0
    number_accurate_samp_dists_of_mean_annual_return = 0
    # n_samples = n_return_observations

    println("\n=================================================================")
    println("$n_return_observations observations")

    # perform 1000 samples and compute a confidence 99th %-ile confidence interval for each.
    # for each value of <n_return_observations>, we should only only see about 1 CI not contain mu=1.15 due to the definition of the 99th %-ile confidence interval.
    trial_count = 100
    for i in 1:trial_count
      println("")

      # construct original sample of observations
      return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0
      sample_mu, sample_sigma, sample_range, sample_q005, sample_q5, sample_q995 =
        compute_dist_stats(return_observations, mean_return_per_period, print=true, prefix="orig observations ")

      # sampling distribution of geometric mean return (not annualized)
      # samp_dist = build_bootstrap_distribution(return_observations, n_samples, gmean)
      # samp_dist_mu, samp_dist_sigma, samp_dist_range, samp_dist_q005, samp_dist_q5, samp_dist_q995 =
      #   compute_dist_stats(samp_dist, mean_return_per_period, print=true)

      # sampling distribution of geometric mean return (annualized)
      samp_dist2 = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)))
      samp_dist2_mu, samp_dist2_sigma, samp_dist2_range, samp_dist2_q005, samp_dist2_q5, samp_dist2_q995 =
        compute_dist_stats(samp_dist2, annual_return, print=true, prefix="geom (annualized) ")
      if samp_dist2_q005 <= annual_return <= samp_dist2_q995
        number_accurate_samp_dists_of_geom_mean_annualized += 1
      end

      # sampling distribution of arithmetic mean return (annualized)
      samp_dist3 = build_bootstrap_distribution(return_observations, n_samples, (sample) -> mean(sample) ^ n_periods_per_year)
      samp_dist3_mu, samp_dist3_sigma, samp_dist3_range, samp_dist3_q005, samp_dist3_q5, samp_dist3_q995 =
        compute_dist_stats(samp_dist3, annual_return, print=true, prefix="mean (annualized) ")
      if samp_dist3_q005 <= annual_return <= samp_dist3_q995
        number_accurate_samp_dists_of_arith_mean_annualized += 1
      end


      annual_return_dist = build_bootstrap_distribution(return_observations, mc_samples, prod, n_periods_per_year)
      # annual_return_dist = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)))
      # annual_return_dist = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)), max(n_periods_per_year, n_return_observations))
      # annual_return_dist = build_bootstrap_distribution_from_normal(return_observations, n_samples, prod, sample_sigma, n_periods_per_year)
      # println(compute_samp_dist_stats(annual_return_dist))

      ard_mu, ard_sigma, ard_range, ard_q005, ard_q5, ard_q995 = compute_dist_stats(annual_return_dist, annual_return, print=true, prefix="annual returns    ")
      if ard_q005 <= annual_return <= ard_q995
        number_accurate_annual_return_distributions += 1
      end

      # revised_q005, revised_q995 = revise_ci(sample_q005, sample_q5, sample_q995, ard_q5)
      # revised_q005, revised_q995 = revise_ci(samp_dist_q005, samp_dist_q5, samp_dist_q995, ard_q5)
      # is_accurate = revised_q005 <= annual_return <= revised_q995
      # println("accurate=$is_accurate   mean=$ard_mu   std=$ard_sigma   range=$ard_range   $revised_q005 --- $ard_q5 --- $revised_q995")

      # samp_dist_mean_annual_return = build_bootstrap_distribution(annual_return_dist, n_samples, mean)
      samp_dist_mean_annual_return = build_bootstrap_distribution(annual_return_dist, n_samples, mean, n_return_observations)
      # samp_dist_mean_annual_return = build_bootstrap_distribution_from_normal(annual_return_dist, n_samples, mean, annual_std_dev, n_samples)
      sdar_mu, sdar_sigma, sdar_range, sdar_q005, sdar_q5, sdar_q995 = compute_dist_stats(samp_dist_mean_annual_return, annual_return, print=true, prefix="mean annual return")
      if sdar_q005 <= annual_return <= sdar_q995
        number_accurate_samp_dists_of_mean_annual_return += 1
      end

    end

    println("sampling distributions of geom mean return (annualized): $(number_accurate_samp_dists_of_geom_mean_annualized/trial_count * 100)% accurate")
    println("sampling distributions of arith mean return (annualized): $(number_accurate_samp_dists_of_arith_mean_annualized/trial_count * 100)% accurate")
    println("annual return distributions: $(number_accurate_annual_return_distributions/trial_count * 100)% accurate")
    println("sampling distributions of mean annual return: $(number_accurate_samp_dists_of_mean_annual_return/trial_count * 100)% accurate")

  end
end

# compute sampling distribution of simulated annual returns
function main3()
  n_periods_per_year = 252
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)

  n_return_observations = 251
  n_annualized_returns = 5000

  annualized_returns = Array(Float64, n_annualized_returns)

  n_samples = 5000
  sampling_distributions = Array(Float64, 6, n_samples)    # 6 statistics - mean/sigma/range/q005/q5/q995
  sample_stats = Array(Float64, 6)

  # construct original sample of observations
  return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

  compute_dist_stats(return_observations, mean_return_per_period)

  for i in 1:n_samples
    annualized_returns = build_distribution(return_observations, n_annualized_returns, n_periods_per_year, prod, annualized_returns)

    sample_stats = compute_dist_stats(annualized_returns, annual_return, sample_stats, false)
    sampling_distributions[:, i] = sample_stats
  end

  println("sampling distributions:")
  println("mean \n $(compute_samp_dist_stats(vec(sampling_distributions[1,:])))")
  println("std_dev \n $(compute_samp_dist_stats(vec(sampling_distributions[2,:])))")
  println("99% range \n $(compute_samp_dist_stats(vec(sampling_distributions[3,:])))")
  println("0.5%-ile \n $(compute_samp_dist_stats(vec(sampling_distributions[4,:])))")
  println("50%-ile \n $(compute_samp_dist_stats(vec(sampling_distributions[5,:])))")
  println("99.5%-ile \n $(compute_samp_dist_stats(vec(sampling_distributions[6,:])))")
end

function main4()
  n_periods_per_year = 251

  # construct original sample of observations
  # relative_return_observations = [0.2, 18.9, 15.5, -11.7, 9.2, -11.8, -17.2, 9.8, -20.1, -15.8]
  # return_observations = map((r) -> (r + 100) / 100.0, relative_return_observations)

  # annual returns
  # return_observations = [1.247, 0.802 ,0.809 ,1.098 ,0.917  ,0.882 ,1.092 ,0.883 ,1.155 ,1.189 ,1.036 ,1.013  ,1.091 ,0.832 ,1.05 ,1.129 ,1.082 ,1.144  ,0.867 ,1.351 ,0.891 ,0.979 ,0.982 ,1.158 ,0.961  ,1.262 ,0.863 ,1.195 ,0.945 ,1.225 ,1.16 ,1.064 ,1.265 ,0.745 ,0.972 ,1.371 ,1.05 ,1.309 ,1.14 ,0.87 ,1.197 ,0.696 ,0.492 ,0.874 ,0.972 ,1.076 ,1.25 ,0.992 ,1.024 ,1.152]

  # daily returns since QE3 ended
  return_observations = [0.9952 ,1.0361  ,1.022 ,1.0082 ,0.9969 ,1.012  ,1.0614 ,0.9945 ,1.0373  ,0.9955 ,0.9576 ,1.0342  ,0.9722 ,1.0465 ,1.0593 ,0.8976  ,1.0254 ,0.9132 ,0.9522  ,1.0463 ,0.9726 ,1.0661 ,1.0462  ,1.0088 ,1.0191 ,0.9699 ,0.9808  ,0.972 ,0.951 ,0.962  ,1.0629 ,1.0337 ,0.9756  ,0.9306 ,1.0177 ,0.917  ,0.9759 ,1.0017 ,0.9895  ,1.0066 ,0.9991 ,1.0509  ,1.0051 ,1.0331 ,1.0911  ,0.9498 ,1.0252 ,0.9457  ,0.9356 ,0.8984 ,0.9844 ,0.9603 ,1.0175 ,0.9987 ,1.0157 ,1.052 ,0.9582 ,0.973 ,1.0164 ,1.0062 ,1.0211 ,1.0213 ,1.0101 ,0.9756 ,1.0167 ,1.0024 ,0.9976 ,0.9872 ,0.9871 ,1.0011 ,1.0371 ,1.015 ,1.0247 ,1.0124 ,1.0078]

  n_samples = 5000

  samp_dist = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)))

  println(compute_samp_dist_stats(samp_dist))

  # compute_dist_stats(samp_dist, annual_return)
end

# this shows how frequently the 99%-ile confidence interval of the monte-carlo simulated 1-year return distributions include the actual annual return
# when the annual return distribution is computed by randomly drawing a value from a Normal PDF centered at each empirical observation
# NOTE: compare with main2 - the results here are much more accurate
#
# for example:
#
function main5()
  n_periods_per_year = 251   # every 30 mins
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)
  # return_dist = Uniform(mean_return_per_period - return_std_dev_per_period, mean_return_per_period + return_std_dev_per_period)

  # construct original sample of observations
  # n_return_observations = 63
  # return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

  n_samples = 1000
  mc_samples = 1000

  for n_return_observations in [
        round(n_periods_per_year/12) |> int64,    # 1 month
        round(n_periods_per_year/4) |> int64,     # 1 quarter
        round(n_periods_per_year/2) |> int64,     # half year
        n_periods_per_year,                       # 1 year
        n_periods_per_year*2,
        n_periods_per_year*5,
        n_periods_per_year*10
      ]

    number_accurate_samp_dists_of_geom_mean_annualized = 0
    number_accurate_samp_dists_of_arith_mean_annualized = 0
    number_accurate_annual_return_distributions = 0
    number_accurate_samp_dists_of_mean_annual_return = 0
    # n_samples = n_return_observations

    println("\n=================================================================")
    println("$n_return_observations observations ($(n_return_observations / n_periods_per_year) years)")

    # perform 1000 samples and compute a confidence 99th %-ile confidence interval for each.
    # for each value of <n_return_observations>, we should only only see about 1 CI not contain mu=1.15 due to the definition of the 99th %-ile confidence interval.
    trial_count = 100
    for i in 1:trial_count
      println("")

      # construct original sample of observations
      return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0
      sample_mu, sample_sigma, sample_range, sample_q005, sample_q5, sample_q995 =
        compute_dist_stats(return_observations, mean_return_per_period, print=true, prefix="orig observations ")

      # sampling distribution of geometric mean return (not annualized)
      # samp_dist = build_bootstrap_distribution(return_observations, n_samples, gmean)
      # samp_dist_mu, samp_dist_sigma, samp_dist_range, samp_dist_q005, samp_dist_q5, samp_dist_q995 =
      #   compute_dist_stats(samp_dist, mean_return_per_period, print=true)

      # sampling distribution of geometric mean return (annualized)
      samp_dist2 = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)))
      samp_dist2_mu, samp_dist2_sigma, samp_dist2_range, samp_dist2_q005, samp_dist2_q5, samp_dist2_q995 =
        compute_dist_stats(samp_dist2, annual_return, print=true, prefix="geom (annualized) ")
      if samp_dist2_q005 <= annual_return <= samp_dist2_q995
        number_accurate_samp_dists_of_geom_mean_annualized += 1
      end

      # sampling distribution of arithmetic mean return (annualized)
      samp_dist3 = build_bootstrap_distribution(return_observations, n_samples, (sample) -> mean(sample) ^ n_periods_per_year)
      samp_dist3_mu, samp_dist3_sigma, samp_dist3_range, samp_dist3_q005, samp_dist3_q5, samp_dist3_q995 =
        compute_dist_stats(samp_dist3, annual_return, print=true, prefix="mean (annualized) ")
      if samp_dist3_q005 <= annual_return <= samp_dist3_q995
        number_accurate_samp_dists_of_arith_mean_annualized += 1
      end

      confidence_intervals = calculate_composite_monte_carlo_confidence_intervals(return_observations, n_samples, 50, mc_samples, n_periods_per_year, (s) -> [mean(s)])
      (mean_ci_lower, mean_ci_upper) = confidence_intervals[1]

      accurate = false
      if mean_ci_lower <= annual_return <= mean_ci_upper
        accurate = true
        number_accurate_samp_dists_of_mean_annual_return += 1
      end
      println("mean (mc annual)     accurate=$accurate   mean=-.-----   std=-.-----   range=$(round(mean_ci_upper - mean_ci_lower, 4))   $(round(mean_ci_lower, 4)) --------------- $(round(mean_ci_upper, 4))")

    end

    println("sampling distributions of geom mean return (annualized): $(number_accurate_samp_dists_of_geom_mean_annualized/trial_count * 100)% accurate")
    println("sampling distributions of arith mean return (annualized): $(number_accurate_samp_dists_of_arith_mean_annualized/trial_count * 100)% accurate")
    println("sampling distributions of mean annual return: $(number_accurate_samp_dists_of_mean_annual_return/trial_count * 100)% accurate")

  end
end


function build_distribution(n_observations, build_observation_fn)
  type_of_observation = typeof(build_observation_fn())
  observations = Array(type_of_observation, n_observations)
  for i in 1:n_observations
    observations[i] = build_observation_fn()
  end
  observations
end

function main6()
  n_periods_per_year = 252
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = sqrt((annual_std_dev^2 + (mean_return_per_period^2)^n_periods_per_year)^(1/n_periods_per_year)-mean_return_per_period^2)       # What’s Wrong with Multiplying by the Square Root of Twelve http://corporate.morningstar.com/US/documents/MethodologyDocuments/MethodologyPapers/SquareRootofTwelve.pdf; how to annualize volatility - http://investexcel.net/how-to-annualize-volatility/ (only applies to log returns)
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)

  n = 10000

  println("single normal")
  dist = build_distribution(n, () -> rand(return_dist))
  compute_dist_stats(dist, annual_return)

  println("multiply 2 normals")
  dist = build_distribution(n, () -> rand(return_dist) * rand(return_dist))
  compute_dist_stats(dist, annual_return)

  println("multiply 3 normals")
  dist = build_distribution(n, () -> rand(return_dist) * rand(return_dist) * rand(return_dist))
  compute_dist_stats(dist, annual_return)

  println("multiply 4 normals")
  dist = build_distribution(n, () -> rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist))
  compute_dist_stats(dist, annual_return)

  println("multiply 10 normals")
  dist = build_distribution(n, () -> rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist))
  compute_dist_stats(dist, annual_return)

  println("multiply 100 normals")
  dist = build_distribution(
    n,
    () -> rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist)
  )
  compute_dist_stats(dist, annual_return)

  println("multiply 252 normals")
  dist = build_distribution(
    n,
    () -> rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
          rand(return_dist) * rand(return_dist)
  )
  compute_dist_stats(dist, annual_return)

  println("----")

  println("single normal (max)")
  dist = build_distribution(n, () -> max(rand(return_dist), 0))
  compute_dist_stats(dist, annual_return)

  println("multiply 2 normals (max)")
  dist = build_distribution(n, () -> max(rand(return_dist) * rand(return_dist), 0))
  compute_dist_stats(dist, annual_return)

  println("multiply 3 normals (max)")
  dist = build_distribution(n, () -> max(rand(return_dist) * rand(return_dist) * rand(return_dist), 0))
  compute_dist_stats(dist, annual_return)

  println("multiply 4 normals (max)")
  dist = build_distribution(n, () -> max(rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist), 0))
  compute_dist_stats(dist, annual_return)

  println("multiply 10 normals (max)")
  dist = build_distribution(n, () -> max(rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist), 0))
  compute_dist_stats(dist, annual_return)

  println("multiply 100 normals (max)")
  dist = build_distribution(
    n,
    () -> max(
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist)
            , 0
          )
  )
  compute_dist_stats(dist, annual_return)

  println("multiply 252 normals (max)")
  dist = build_distribution(
    n,
    () -> max(
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) * rand(return_dist) *
            rand(return_dist) * rand(return_dist)
            , 0
          )
  )
  compute_dist_stats(dist, annual_return)

end

main5()
