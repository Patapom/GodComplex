\name{rvm}
\title{Random Generation from the von Mises Distribution}
\alias{rvm}

\description{
Generates pseudo-random numbers from a von Mises distribution. 
}

\usage{
rvm(n, mean, k)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{mean}{mean direction in radians of the von Mises distribution.}
\item{k}{concentration parameter of the von Mises distribution.}
}

\value{
Returns a vector of n independent random variables generated from a von Mises distribution.  Values are between 0 and 2 pi.
}

\details{
Simulation from the von Mises distribution is done via the algorithm due to Best and Fisher (1979).
}

\references{
Best, D. and Fisher, N. (1979).  Efficient simulation of the von Mises distribution.  Applied Statistics, 24, 152-157.
}

\seealso{
\link{dmixedvm}, \link{dvm}, \link{pvm}, \link{rmixedvm}
}

\keyword{distribution}


