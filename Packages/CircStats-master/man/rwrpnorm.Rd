\name{rwrpnorm}
\title{Random Generation from the Wrapped Normal Distribution}
\alias{rwrpnorm}

\description{
Generates pseudo-random numbers from a wrapped normal distribution.}

\usage{
rwrpnorm(n, mu, rho, sd=1)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{mu}{mean direction in radians of the wrapped normal distribution.}
\item{rho}{concentration parameter.  rho must be between 0 and 1.}
\item{sd}{different way of select \code{rho}, see details below.}
}

\value{
Returns a vector of n independent random variables generated from a wrapped normal distribution.
}

\details{
n random variables are generated from a normal distribution with mean
direction mu, and variance -2*log(rho).  The function returns these
values modulo 2*pi. You can set \code{rho} by using \code{sd} with the following equivalence:
\deqn{
\rho = \exp{- \sigma^2/2}
}
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.5, World Scientific Press, Singapore.
}

\seealso{
\link{dwrpnorm}
}

\keyword{distribution}


