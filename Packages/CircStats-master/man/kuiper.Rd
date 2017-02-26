\name{kuiper}
\title{Kuiper's Test}
\alias{kuiper}

\description{
Performs Kuiper's one-sample test of uniformity on the circle. 
}

\usage{
kuiper(x, alpha=0)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{alpha}{significance level of the test.  Possible levels are 0.15, 0.1, 0.05, 0.025, 0.01.  Alpha may be omitted or set to zero, in which case a range for the p-value of the test will be returned.}
}

\value{
NULL
}

\note{
Kuiper's one-sample test of uniformity is performed, and the results are printed to the screen.  If alpha is specified and non-zero, the test statistic is printed along with the critical value and decision.  If alpha is omitted, the test statistic is printed and a range for the p-value of the test is given.
}

\details{
Kuiper's test statistic is a rotation-invariant Kolmogorov-type test statistic.  The critical values of a modified Kuiper's test statistic are used	according to the tabulation given in Stephens (1970). 
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.2, World Scientific Press, Singapore.

Stephens, M. (1970).  Use of the Kolmogorov-Smirnov, Cramer-von Mises and related statistics without extensive tables.  Journal of the Royal Statistical Society, B32, 115-122.
}

\seealso{
\link{circ.range}, \link{rao.spacing}, \link{r.test}, \link{v0.test}, \link{watson}
}

\examples{
# Generate data from the uniform distribution on the circle.
data <- runif(100, 0, 2*pi)
kuiper(data)
# Generate data from the von Mises distribution.
data <- rvm(100, 0, 3)
kuiper(data, alpha=0.01)
}

\keyword{htest}

