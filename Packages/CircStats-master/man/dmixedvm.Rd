\name{dmixedvm}    
\title{Mixture of von Mises Distributions}
\alias{dmixedvm}

\description{
Evaluates the density function of a mixture of two von Mises distributions. 
}

\usage{
dmixedvm(theta, mu1, mu2, kappa1, kappa2, p)
}

\arguments{
\item{theta}{vector of values at which to evaluate the density function.}
\item{mu1}{mean direction in radians of one of the two von Mises distributions.}
\item{mu2}{mean direction in radians of the other von Mises distribution.}
\item{kappa1}{concentration parameter of one of the two von Mises distributions.}
\item{kappa2}{concentration parameter of the other von Mises distribution.}
\item{p}{mixing proportion.}
}

\value{
Evaluates the density function of a mixture of two von Mises distributions at a given vector of values, theta.
}

\details{
Evaluates the density function \eqn{p*VM(mu1, kappa1) + (1-p)*VM(mu2, kappa2)}, where  VM is the von Mises density function.
}

\seealso{
\link{rmixedvm}
}

\keyword{distribution}

