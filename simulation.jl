using Distributions

function build_bootstrap_distribution(orig_sample, n_samples, statistic_fn)
  orig_sample_size = length(orig_sample)

  # # construct bootstrap samples of weekly returns
  # observations = sample(orig_sample, n_samples*orig_sample_size)
  # bootstrap_samples = reshape(observations, orig_sample_size, n_samples)    # returns a <orig_sample_size> row X <n_samples> column matrix of observations
  #
  # # construct bootstrap distribution of simulated annual returns
  # statistics = statistic_fn(bootstrap_samples, 1) |> vec
  
  statistics = Array(typeof(statistic_fn([1])), n_samples)
  for i in 1:n_samples
    bootstrap_sample = sample(orig_sample, orig_sample_size) |> vec
    statistics[i] = statistic_fn(bootstrap_sample)
  end
  statistics
end

function main()
  for i in 1:100
    samples = 1000
    observations_per_sample = 1000
    sigma = 1.4 ^ (1/52) - 1
    weekly_return_dist = Normal(1, sigma)                             # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time
    
    # construct original sample of observations
    weekly_observations = 52*10
    weekly_return_sample = rand(weekly_return_dist, weekly_observations)            # create sample of weekly observations
    
    # construct bootstrap samples of weekly returns
    observations = sample(weekly_return_sample, weekly_observations*samples*observations_per_sample)
    bootstrap_samples = reshape(observations, weekly_observations, samples*observations_per_sample)     # returns a <weekly_observations> row X <samples * observations_per_sample> column matrix of weekly returns
    
    # construct bootstrap distribution of simulated annual returns
    products = prod(bootstrap_samples, 1)                                                               # a 1 row X <samples * observations_per_sample> column matrix representing the returns of <samples*observations_per_sample> <weekly_observations>-week simulated price paths
    annualized_returns = products .^ (52/weekly_observations)                           # a 1 row X <samples * observations_per_sample> column matrix representing the annualized returns of <samples*observations_per_sample> <weekly_observations>-week simulated price paths
    
    samp_dist_of_mean_annual_return = build_bootstrap_distribution(annualized_returns, samples, mean)
    
    # annual_return_samples = reshape(annualized_returns, observations_per_sample, samples)     # a <observations_per_sample> X <samples> matrix, representing <samples> samples of <observations_per_sample> annual return observations each
    # samp_dist_of_mean_annual_return = mean(annual_return_samples, 1) |> vec             # sampling distribution of mean simulated annual return
    
    # compute quantiles of sampling distribution
    q005, q025, q5, q975, q995 = quantile(samp_dist_of_mean_annual_return, [0.005, 0.025, 0.5, 0.975, 0.995])
    
    is_accurate = q005 <= 1 <= q995
    println("accurate=$is_accurate ; $q005, $q025, $q5, $q975, $q995")
  end
end

main()