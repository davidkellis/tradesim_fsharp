using Distributions
using Stats

function build_bootstrap_distribution(orig_sample, n_samples, statistic_fn, n_observations_per_sample = length(orig_sample))
  type_of_statistic = typeof(statistic_fn(sample(orig_sample, 1)))
  statistics = Array(type_of_statistic, n_samples)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, n_observations_per_sample)
    statistics[i] = statistic_fn(bootstrap_sample)
  end
  statistics
end

function build_sampling_distribution(period_returns, n_samples, n_observations_per_sample, n_periods, statistic_fn)
  statistics = Array(Float64, n_samples)
  cumulative_returns = Array(Float64, n_observations_per_sample)
  for i in 1:n_samples
    
    for j in 1:n_observations_per_sample
      n_period_observations = sample(period_returns, n_periods)
      cumulative_returns[j] = prod(n_period_observations) ^ (52/n_periods)
    end

    statistics[i] = statistic_fn(cumulative_returns)
  end
  statistics
end

function print_dist_stats(distribution, expected_mean)
  # compute statistics of sampling distribution
  q005, q025, q5, q975, q995 = round(quantile(distribution, [0.005, 0.025, 0.5, 0.975, 0.995]), 4)
  mean_of_samp_dist = round(mean(distribution), 4)
  std_of_samp_dist = round(std(distribution), 4)
  iqr_of_samp_dist = round(iqr(distribution), 4)
  
  is_accurate = q005 <= expected_mean <= q995
  println("accurate=$is_accurate   mean=$mean_of_samp_dist   std=$std_of_samp_dist   iqr=$iqr_of_samp_dist   $q005 --- $q025 --- $q5 --- $q975 --- $q995")
end

# compute sampling distribution of simulated annual returns
function main()
  # sigma = 1.4 ^ (1/52) - 1
  sigma = 1.4 ^ (1/251) - 1
  return_dist = Normal(1, sigma)                             # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time

  for i in 1:100
    # construct original sample of observations
    n_return_observations = 251
    return_observations = rand(return_dist, n_return_observations)            # create sample of return observations

    n_annualized_returns = 1000
    n_periods = 251
    n_samples = 1000
    n_observations_per_sample = 1000
    
    # annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample) ^ (52/n_periods), n_periods)    # for weekly returns
    # annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample) ^ (251/n_periods), n_periods)    # for daily returns
    annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample), n_periods)    # for daily returns
    
    print_dist_stats(annualized_returns, 1)

    samp_dist_of_mean_annual_return = build_bootstrap_distribution(annualized_returns, n_samples, mean, n_observations_per_sample)
    
    # samp_dist_of_mean_annual_return = build_sampling_distribution(return_observations, n_samples, n_observations_per_sample, n_periods, mean)
    
    print_dist_stats(samp_dist_of_mean_annual_return, 1)
  end
end

# compute sampling distribution of normally distributed annual returns
function main2()
  return_dist = Normal(1, 0.4)

  for i in 1:100
    # construct original sample of observations
    n_return_observations = 1000
    return_observations = rand(return_dist, n_return_observations)            # create sample of return observations

    n_samples = 5000
    n_observations_per_sample = 100
    
    # samp_dist = build_bootstrap_distribution(return_observations, n_samples, mean, n_observations_per_sample)
    samp_dist = build_bootstrap_distribution(return_observations, n_samples, mean)
    
    print_dist_stats(samp_dist, 1)
  end
end

main()