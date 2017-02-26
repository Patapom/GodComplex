\name{est.rho}
\title{Mean Resultant Length}
\alias{est.rho}

\description{
Returns the mean resultant length of a vector of circular data. 
}

\usage{
est.rho(x)
}

\arguments{
\item{x}{vector of data points measured in radians.}
}


\value{
Returns the mean resultant length of data.
}

\details{
Each observation is treated as a unit vector, or point  on the unit circle.  The resultant vector of the observations is found, and the length of the resultant vector divided by the sample size is returned.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 1.3, World Scientific Press, Singapore.
}

\seealso{
\link{circ.mean}, \link{circ.disp}, \link{circ.summary}, \link{est.kappa}.
}

\examples{
# Compute the mean resultant length of a random sample of observations.
data <- runif(100, 0, 2*pi)
est.rho(data)
}

\keyword{htest}


