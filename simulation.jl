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
    
    # perform 100 samples and compute a confidence 99th %-ile confidence interval for each.
    # for each value of <n_return_observations>, we should only only see about 1 CI not contain mu=1.15 due to the definition of the 99th %-ile confidence interval.
    for i in 1:100
      # construct original sample of observations
      return_observations = max(rand(return_dist, n_return_observations), 0)            # create sample of return observations; all values are >= 0
      println("$n_return_observations observations")


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

main4()