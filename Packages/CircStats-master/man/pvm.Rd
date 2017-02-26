\name{pvm}
\title{Cummulative Probability for the von Mises Distribution}
\alias{pvm}

\description{
Estimates the cummulative probability for a von Mises distribution.} 

\usage{
pvm(theta, mu, kappa, acc=1e-020)
}

\arguments{
\item{theta}{angular value in radians.}
\item{mu}{mean direction of the von Mises distribution.}
\item{kappa}{concentration parameter of the von Mises distribution.}
\item{acc}{parameter relating to the accuracy of the estimated cummulative probability.  See details below.  Default value is 1e-020.}
}

\value{
Returns the probability that a von Mises random variable falls between 0 and theta.
}

\details{
Cummulative probabilities are computed according to the expression for the von Mises cdf given by Gumbel et al. (1953), which gives the cdf as a function of an infinite sum.  The parameter acc specifies the accuracy with which this sum is approximated. Terms greater than acc are included in the summation.  The cummulative probabilities given by pvm coincide with those tabulated	by Mardia (1972), which are given to five decimal values.
}

\references{
Gumbel, E., Greenwood, J. and Durand, D. (1953).  The circular normal distribution: theory and tables.  Journal of the American Statistical Association, 48, 131-152.

Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.4, World Scientific Press, Singapore.
}

\seealso{
\link{dvm}, \link{rvm}
}

\keyword{distribution}

