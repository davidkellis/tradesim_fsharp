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
  q005, q5, q995 = round(quantile(distribution, [0.005, 0.5, 0.995]), 4)
  mean_of_samp_dist = round(mean(distribution), 4)
  std_of_samp_dist = round(std(distribution), 4)
  range_of_99th_percentile_of_samp_dist = round(abs(q995 - q005), 4)
  
  is_accurate = q005 <= expected_mean <= q995
  println("accurate=$is_accurate   mean=$mean_of_samp_dist   std=$std_of_samp_dist   99th %-ile range=$range_of_99th_percentile_of_samp_dist   $q005 --- $q5 --- $q995")
end

# compute sampling distribution of simulated annual returns
function main()
  n_periods_per_year = 52
  sigma = 1.4 ^ (1/n_periods_per_year) - 1
  # sigma = (1 + 0.82) ^ (1/251) - 1   # estimate of sigma taken from http://blog.iese.edu/jestrada/files/2012/06/DSRSSM.pdf
  return_dist = Normal(1, sigma)     # mu = 1; sigma = 1.4 ^ (1/52) - 1    # sigma of [1.4 ^ (1/52) - 1] simulates a weekly return that when annualized represents a +-40% annual gain 68% of the time and +-80% annual gain 95% of the time

  for i in 1:100
    # construct original sample of observations
    n_return_observations = 26 * i
    return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

    println("$i - $n_return_observations return observations")

    n_annualized_returns = 50000
    n_samples = 5000
    n_observations_per_sample = 1000
    
    # annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample) ^ (52/n_periods_per_year), n_periods_per_year)    # for weekly returns
    # annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample) ^ (251/n_periods_per_year), n_periods_per_year)    # for daily returns
    annualized_returns = build_bootstrap_distribution(return_observations, n_annualized_returns, (sample) -> prod(sample), n_periods_per_year)
    
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
    n_return_observations = 100
    return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0

    n_samples = 5000
    # n_observations_per_sample = 100
    
    # samp_dist = build_bootstrap_distribution(return_observations, n_samples, mean, n_observations_per_sample)
    samp_dist = build_bootstrap_distribution(return_observations, n_samples, mean)
    
    print_dist_stats(samp_dist, 1)
  end
end

main()