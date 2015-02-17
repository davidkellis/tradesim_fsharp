using Distributions

function main()
  for i in [1:100]
    weekly_return_dist = Normal(0, 0.0064916)             # mu = 0; sigma = 1.4 ^ (1/52) - 1
    weekly_return_sample = rand(weekly_return_dist, 26)   # create half-year sample of weekly observations
    observations = sample(weekly_return_sample, 52*1000*1000)
    m = reshape(observations, 52, 1000*1000)              # returns a 52 row X 1,000,000 column matrix of weekly returns
    products = reducedim(*, m, 1, 1)                      # returns a 1 row X 1,000,00 column matrix of simulated annual returns
    products_sample = vec(products)                       # returns a sample of 1,000,000 simulated annual returns
  end
end

main()