\name{rwrpcauchy}
\title{Random Generation from the Wrapped Cauchy Distribution}
\alias{rwrpcauchy}

\description{
Generates pseudo-random numbers from a wrapped Cauchy distribution.}

\usage{
rwrpcauchy(n, location=0, rho=exp(-1))
}

\arguments{
\item{n}{number of random variables to generate.}
\item{location}{modal angle in radians of the wrapped Cauchy distribution.}
\item{rho}{concentration parameter.  rho must be between 0 and 1.}
}

\value{
Returns a vector of n independent random variables generated from a wrapped Cauchy distribution.
}

\details{
n random variables are generated from a Cauchy distribution with location parameter location, and concentration parameter -log(rho).  The function returns these values modulo 2*pi.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.6, World Scientific Press, Singapore.
}

\seealso{
\link{dwrpcauchy}
}

\keyword{distribution}

