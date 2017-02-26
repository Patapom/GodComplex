\name{rmixedvm}
\title{Random Generation from the Mixed von Mises Distribution}
\alias{rmixedvm}

\description{
Generates pseudo-random numbers from a mixture of two von Mises distributions.
}

\usage{
rmixedvm(n, mu1, mu2, kappa1, kappa2, p)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{mu1}{mean direction in radians of one of the two von Mises distributions.}
\item{mu2}{mean direction in radians of the other von Mises distribution.}
\item{kappa1}{concentration parameter of one of the two von Mises distributions.}
\item{kappa2}{concentration parameter of the other von Mises distribution.}
\item{p}{mixing proportion.}
}

\value{
Returns a vector of n independent random variables generated from a mixture of two von Mises distributions.
}

\details{
Simulates random variables with the density function p*VM(mu1, kappa1) + (1-p)*VM(mu2, kappa2), where  VM is the von Mises density function.  

A uniform random variable on (0,1) is generated.  If it is less than p, then a variable is generated from VM(mu1, kappa1), else a variable is generated from VM(mu2, kappa2).  Simulation from the von Mises distribution is done via the algorithm due to Best and Fisher (1979).
}

\references{
Best, D. and Fisher, N. (1979).  Efficient simulation of the von Mises distribution.  Applied Statistics, 24, 152-157.
}

\seealso{
\link{dmixedvm}, \link{rvm}
}

\keyword{distribution}


