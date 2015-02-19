using Distributions

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

function main()
  for i in 1:100
    sigma = 1.4 ^ (1/52) - 1
    weekly_return_dist = Normal(1, sigma)                             # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time
    
    # construct original sample of observations
    n_weekly_returns = 52*10000
    weekly_returns = rand(weekly_return_dist, n_weekly_returns)            # create sample of weekly observations
    
    n_samples = 1000
    n_observations_per_sample = 1000
    n_periods = 1000
    # # annualized_returns = build_bootstrap_distribution(weekly_returns, n_samples, (sample) -> prod(sample) ^ (52/n_observations_per_sample), n_observations_per_sample)
    # n_annualized_returns = 2000
    # # annualized_returns = build_bootstrap_distribution(weekly_returns, n_annualized_returns, prod, 52)
    # annualized_returns = build_bootstrap_distribution(weekly_returns, n_annualized_returns, (sample) -> prod(sample) ^ (52/n_observations_per_sample), n_observations_per_sample)
    #
    # samp_dist_of_mean_annual_return = build_bootstrap_distribution(annualized_returns, n_samples, mean)
    
    samp_dist_of_mean_annual_return = build_sampling_distribution(weekly_returns, n_samples, n_observations_per_sample, n_periods, mean)
    
    # compute quantiles of sampling distribution
    q005, q025, q5, q975, q995 = quantile(samp_dist_of_mean_annual_return, [0.005, 0.025, 0.5, 0.975, 0.995])
    
    is_accurate = q005 <= 1 <= q995
    println("accurate=$is_accurate ; $q005, $q025, $q5, $q975, $q995")
  end
end

main()