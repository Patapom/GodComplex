\name{circ.summary}
\title{Circular Summary Statistics}
\alias{circ.summary}

\description{
Computes circular summary statistics including the sample size, mean direction and mean resultant length.
}

\usage{
circ.summary(x)
}

\arguments{
\item{x}{vector of data points measured in radians.}
}

\value{
Returns a data frame with variables n, the sample size; mean.dir, the sample mean direction; and rho, the sample mean resultant length.
}

\details{
Each observation is  treated  as  a  unit  vector or a point on the unit circle.  The resultant vector of the observations is found, and the direction of the resultant vector is returned as well as its length divided by the sample size.  
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 1.3, World Scientific Press, Singapore.
}

\seealso{
\link{circ.mean}, \link{circ.disp}, \link{est.kappa}, \link{est.rho}.
}

\examples{
# Compute summary statistics of a random sample of observations. 
data <- runif(50, 0, pi)
circ.summary(data)
}

\keyword{misc}

