\name{rtri}
\title{Random Generation from the Triangular Distribution}
\alias{rtri}

\description{
Generates pseudo-random numbers from a symmetric triangular distribution.
}

\usage{
rtri(n, r)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{r}{concentration parameter of the distribution.  r must be between 0 and \eqn{4/pi^2}.}
}

\value{
Returns a vector of n independent random variables generated from a symmetric triangular distribution with mean direction 0 and concentration parameter r.
}

\details{
The triangular cdf is inverted to obtain the random numbers.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.3.3, World Scientific Press, Singapore.
}

\seealso{
\link{dtri}
}

\keyword{distribution}


