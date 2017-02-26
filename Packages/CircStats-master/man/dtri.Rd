\name{dtri}    
\title{Triangular Density Function}
\alias{dtri}

\description{
Returns the triangular density function evaluated at a particular value. 
}

\usage{
dtri(theta, r)
}

\arguments{
\item{theta}{vector of angles measured in radians at which to evaluate the density function.}
\item{r}{concentration parameter of the distribution.  r must be between 0 and \eqn{4/pi^2}.}
}

\value{
Returns the triangular density function evaluated at theta.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.3, World Scientific Press, Singapore.
}

\seealso{
\link{rtri}
}

\keyword{distribution}

