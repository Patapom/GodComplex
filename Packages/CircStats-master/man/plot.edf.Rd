\name{plot.edf}
\title{Plot Empirical Distribution Function}
\alias{plot.edf}

\description{
Plots the empirical distribution function of a data set. 
}

\usage{
\method{plot}{edf}(x, ...)
}

\arguments{
\item{x}{vector of circular data measured in radians.}
\item{...}{optional graphical parameters.  See help section on \link{par}.}
}

\note{
Creates a plot of the empirical distribution function of the vector data.
}

\details{
The vector data is taken modulo 2*pi, and then the linear ranks are used to generate an empirical distribution function.
}

\seealso{
\link{par}
}

\examples{
# Compare the edf's of two simulated sets of data.
data1 <- rvm(10, 0, 3)
data2 <- rvm(10, 0, 1)
plot.edf(data1, xlab="Data", ylab="EDF", main="Plots of Two EDF's")
par(new=TRUE)
plot.edf(data2, axes=FALSE, xlab="", ylab="", lty=2)
}

\keyword{hplot}

