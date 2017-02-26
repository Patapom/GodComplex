\name{dwrpcauchy}
\title{Wrapped Cauchy Density Function}
\alias{dwrpcauchy}

\description{
Returns the wrapped Cauchy density function evaluated at a particular value. 
}

\usage{
dwrpcauchy(theta, mu, rho)
}

\arguments{
\item{theta}{vector of angles measured in radians at which to evaluate the density	function.}
\item{mu}{mean direction of the distribution measured in radians.}
\item{rho}{concentration parameter of the distribution.  rho must be in the interval from 0 to 1.}
}

\value{
Returns the wrapped Cauchy density function evaluated at theta.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.7, World Scientific Press, Singapore.
}

\seealso{
\link{rwrpcauchy}
}

\keyword{distribution}

