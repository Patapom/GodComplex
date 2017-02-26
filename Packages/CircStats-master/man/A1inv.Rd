\name{A1inv}
\title{Inverse of A1}
\alias{A1inv}

\description{
Inverse function of the ratio of the first and zeroth order Bessel functions of the first kind.  This function is used to compute the maximum likelihood estimate of the concentration parameter of a von Mises distribution.
}

\usage{
A1inv(x)
}

\arguments{
\item{x}{numeric value in the interval between 0 and 1.}
}

\value{
Returns the value k, such that A1inv(x) = k, i.e. A1(k) = x.
}

\details{
A1inv(0) = 0 and A1inv(1) = inf.  This function is useful in estimating the concentration parameter of data from a von Mises distribution.
}

\seealso{
\code{\link{est.kappa}}, \code{\link{A1}}, \code{\link{besselI}}.
}

\examples{
#Generate data from a von Mises distribution
data <- rvm(50, pi, 4)
#Estimate the concentration parameter
s <- sum(sin(data))
c <- sum(cos(data))
mean.dir <- atan2(s, c)
kappa <- A1inv(mean(cos(data - mean.dir)))
}

\keyword{math}

