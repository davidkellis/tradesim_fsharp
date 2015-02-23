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

function build_distribution(orig_sample, n_samples, n_observations_per_sample, statistic_fn, return_distribution)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    return_distribution[i] = statistic_fn(bootstrap_sample)
  end
  return_distribution
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
function compute_dist_stats(samp_dist, expected_mean, return_array = Array(Float64, 6), print = true)
  # compute statistics of sampling distribution
  q005, q01, q05, q1, q15, q2, q25, q3, q35, q4, q45, q5, q55, q6, q65, q7, q75, q8, q85, q9, q95, q99, q995 = round(quantile(samp_dist, [0.005, 0.01, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99, 0.995]), 3)
  mean_of_samp_dist = round(mean(samp_dist), 4)
  std_of_samp_dist = round(std(samp_dist), 4)
  range_99th_percentile = round(abs(q995 - q005), 3)
  
  is_accurate = q005 <= expected_mean <= q995
  
  if print
    println("accurate=$is_accurate   mean=$mean_of_samp_dist   std=$std_of_samp_dist   range=$range_99th_percentile   $q005 --- $q5 --- $q995")
  end
  
  return_array[1] = mean_of_samp_dist
  return_array[2] = std_of_samp_dist
  return_array[3] = range_99th_percentile
  return_array[4] = q005
  return_array[5] = q5
  return_array[6] = q995
  return_array
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
  n_periods_per_year = 251
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = (1 + annual_std_dev) ^ (1/n_periods_per_year) - 1
  # sigma = (1 + 0.82) ^ (1/251) - 1   # estimate of sigma taken from http://blog.iese.edu/jestrada/files/2012/06/DSRSSM.pdf
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)     # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time

  n_return_observations = 251
  n_annualized_returns = 10000
  n_samples = 10000
  n_observations_per_sample = n_return_observations
  
  annualized_returns = Array(Float64, n_annualized_returns)
  
  n_iterations = 30
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

# compute sampling distribution of normally distributed annual returns
function main2()
  n_periods_per_year = 251
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = (1 + annual_std_dev) ^ (1/n_periods_per_year) - 1
  # sigma = (1 + 0.82) ^ (1/251) - 1   # estimate of sigma taken from http://blog.iese.edu/jestrada/files/2012/06/DSRSSM.pdf
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)     # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time

  # construct original sample of observations
  # n_return_observations = 63
  # return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

  n_samples = 5000

  for n_return_observations in [
        round(n_periods_per_year/12) |> int64, 
        round(n_periods_per_year/4) |> int64, 
        round(n_periods_per_year/2) |> int64, 
        n_periods_per_year, 
        n_periods_per_year*2, 
        n_periods_per_year*5, 
        n_periods_per_year*10
      ]
    
    # construct original sample of observations
    return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0
    println("$n_return_observations observations")

    for i in 1:2
      # samp_dist = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)), max(n_periods_per_year, n_return_observations))
      samp_dist = build_bootstrap_distribution(return_observations, n_samples, (sample) -> prod(sample) ^ (n_periods_per_year/length(sample)))
      # println(compute_samp_dist_stats(samp_dist))
    
      compute_dist_stats(samp_dist, annual_return)
    end
  end
end

# compute sampling distribution of simulated annual returns
function main3()
  n_periods_per_year = 251
  annual_return = 1.15
  annual_std_dev = 0.4
  mean_return_per_period = annual_return ^ (1/n_periods_per_year)
  return_std_dev_per_period = (1 + annual_std_dev) ^ (1/n_periods_per_year) - 1
  # sigma = (1 + 0.82) ^ (1/251) - 1   # estimate of sigma taken from http://blog.iese.edu/jestrada/files/2012/06/DSRSSM.pdf
  return_dist = Normal(mean_return_per_period, return_std_dev_per_period)     # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time

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

main2()