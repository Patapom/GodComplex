\name{pp.plot}    
\title{von Mises Probability-Probability Plot}
\alias{pp.plot}

\description{
Plots the empirical distribution of a data set against the best fitting von Mises distribution function.
}

\usage{
pp.plot(x, ref.line=TRUE)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{ref.line}{logical flag: if TRUE a 45 degree reference line is added to the plot. Default is TRUE.}
}

\value{
NULL
}

\note{
A plot is created on the current graphics device.
}

\details{
The maximum likelihood estimates of the parameters of the von Mises distribution are computed from the given data set.  The empirical distribution function is plotted against a von Mises distribution function with parameters given by the MLEs computed. 
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 10.2, World Scientific Press, Singapore.
}

\seealso{
\link{vm.ml}
}

\examples{
data <- rvm(25, 0, 3)
pp.plot(data)
data <- c(rvm(20, 0, 7), rvm(20, pi, 7))
pp.plot(data)
}

\keyword{hplot}

