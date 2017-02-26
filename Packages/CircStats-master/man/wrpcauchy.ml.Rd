\name{wrpcauchy.ml}
\title{Wrapped Cauchy Maximum Likelihood Estimates}
\alias{wrpcauchy.ml}

\description{
Computes the maximum likelihood estimates of the location and scale parameters for a wrapped Cauchy distribution.
}

\usage{
wrpcauchy.ml(x, mu0, rho0, acc=1e-015)
}

\arguments{
\item{x}{vector of angular data measured in radians.}
\item{mu0}{initial estimate of the location parameter.}
\item{rho0}{initial estimate of the scale parameter.  rho0 should be between in [0,1).}
\item{acc}{degree of accuracy in the approximation of the estimates.  The default value is 1e-15.  See below for details.}
}

\value{
A dataframe is returned with the variables mu and rho, where mu and rho are the approximations of the MLE of the location  and scale parameter, respectively.
}

\details{
An iterative algorithm due to Kent and Tyler (1988) is used.  Initial values of the MLE's are required.  In the estimation process, estimates of quantities mu1 and mu2 (see Kent and Tyler) are updated iteratively.  When both mu1 and mu2 change by less than acc from one iteration to the next, the process ends, and mu and rho are computed from the final estimates of mu1 and mu2.
}

\references{
Kent, J. and Tyler, D. (1988).  Maximum likelihood estimation for the wrapped Cauchy distribution.  Journal of Applied Statistics, 15, 2, 247-254.

Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 4.4, World Scientific Press, Singapore.
}

\seealso{
\link{dwrpcauchy}, \link{rwrpcauchy}
}

\examples{
# Generate data from a wrapped Cauchy distribution.
data <- rwrpcauchy(50, 0, 0.75)
# Compute the sample mean direction and resultant length.
mu0 <- circ.mean(data)
rho0 <- est.rho(data)
# Estimate the MLE's of the Cauchy distribution parameters.
wrpcauchy.ml(data, mu0, rho0)
}

\keyword{htest}


