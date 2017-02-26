\name{dwrpnorm}    
\title{Wrapped Normal Density Function}
\alias{dwrpnorm}

\description{
Estimate of the wrapped normal density function.
}

\usage{
dwrpnorm(theta, mu, rho, sd=1, acc=1e-5, tol=acc)
}

\arguments{
\item{theta}{value at which to evaluate the density function, measured in radians.}
\item{mu}{mean direction of distribution, measured in radians.}
\item{rho}{mean resultant length of distribution.}
\item{sd}{different way of select \code{rho}, see details below.}
\item{acc}{parameter defining the accuracy of the estimation of the
  density.  Terms are added to the infinite summation that defines the
  density function until successive estimates are within \code{acc} of each
  other.}
\item{tol}{the same as \code{acc}.}
}

\value{
Returns an estimate of the wrapped normal density function.
}

\details{
The form of the wrapped normal density function is an infinite series
with index going from negative infinity to positive infinity.  This
function begins with the zeroth term and adds terms to the series,
corresponding to both the positive and negative index, until the
summation changes by less than the parameter value of \code{acc}. You
can set \code{rho} by using \code{sd} with the following equivalence:
\deqn{
\rho = \exp{- \sigma^2/2}
}
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.6, World Scientific Press, Singapore.
}

\seealso{
\link{rwrpnorm}
}

\examples{
# Values for which to evaluate density
theta <- c(1:500)*2*pi/500
#Compute wrapped normal density function
density <- c(1:500)
for(i in 1:500) density[i] <- dwrpnorm(theta[i], pi, .75)
plot(theta, density)
#Approximate area under density curve
sum(density*2*pi/500)
}

\keyword{distribution}

