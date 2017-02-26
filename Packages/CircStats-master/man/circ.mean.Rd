\name{circ.mean}
\title{Mean Direction}
\alias{circ.mean}

\description{
Returns the mean direction of a vector of circular data. 
}

\usage{
circ.mean(x)
}

\arguments{
\item{x}{vector of data points measured in radians.}
}

\value{
Returns the mean direction of the data.
}

\details{
Each observation is treated as a unit vector, or point  on the unit circle.  The resultant vector of the observations is found, and the direction of the resultant vector is returned.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 1.3, World Scientific Press, Singapore.
}

\seealso{
\link{circ.disp}, \link{circ.summary}, \link{est.kappa}, \link{est.rho}.
}

\examples{
# Compute the mean direction of a random sample of observations.
data <- runif(50, 0, pi)
mean.dir <- circ.mean(data)
}

\keyword{univar}

