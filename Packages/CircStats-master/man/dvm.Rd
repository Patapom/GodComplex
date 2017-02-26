\name{dvm}
\title{von Mises Density Function}
\alias{dvm}

\description{
Returns the von Mises density function evaluated at a particular value. 
}

\usage{
dvm(theta, mu, kappa)
}

\arguments{
\item{theta}{vector of angles measured in radians at which to evaluate the density	function.}
\item{mu}{mean direction of the distribution measured in radians.}
\item{kappa}{non-negative numeric value for the concentration parameter of the distribution.}
}

\value{
Returns the von Mises density function evaluated at theta.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.4, World Scientific Press, Singapore.
}

\seealso{
\link{rvm}
}

\keyword{distribution}

