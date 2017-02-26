\name{circ.disp}
\title{Circular Dispersion}
\alias{circ.disp}

\description{
Computes measures of dispersion for a directional data set. 
}

\usage{
circ.disp(x)
}

\arguments{
\item{x}{vector of circular data measured in radians.}
}

\value{
Returns a dataframe with the following components. The sample size, n; the resultant length, r; the mean resultant length, rbar= r/n; and the circular variance, var=(1-r/n).
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 1.3, World Scientific Press, Singapore.
}

\seealso{
\link{circ.mean}, \link{circ.summary}, \link{est.kappa}, \link{est.rho}.
}

\keyword{univar}

