\name{A1}
\title{Ratio of First and Zeroth Order Bessel Functions}
\alias{A1}

\description{Evaluates the first and zeroth order Bessel functions of the first kind at a specified non-negative real number, and returns the ratio.} 

\usage{
A1(kappa)
}

\arguments{
\item{kappa}{non-negative numeric value at which to evaluate the Bessel functions.}
}

\value{
If I1(kappa) is the first order Bessel function and I0(kappa) is the zeroth order Bessel function, then A1(kappa) returns I1(kappa)/I0(kappa).
}

\details{
The function use \code{\link{besselI}}.
}

\seealso{
 \code{\link{besselI}}, \code{\link{A1inv}}.
}


\keyword{math}

