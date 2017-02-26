\name{watson}
\title{Watson's Test }
\alias{watson}

\description{
Performs a Watson's goodness of fit test for the von Mises or circular uniform distribution. 
}

\usage{
watson(x, alpha=0, dist='uniform')
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{alpha}{significance level of the test.  Valid levels are 0.01, 0.05, 0.1.  This argument may be ommited, in which case, a range for the p-value will be returned.}
\item{dist}{distribution to test for.  The default is the uniform distribution. To test for the von Mises distribution, set dist = 'vm'.}
}

\value{
NULL
}

\note{
If dist = 'uniform', Watson's one-sample test for the circular uniform distribution is performed, and the results are printed to the screen.  If alpha is specified and non-zero, the test statistic is printed along with the critical value and decision.  If alpha is omitted, the test statistic is printed and a range for the p-value of the test is given.

If dist = 'vm', estimates of the population parameters are used to evaluate the von Mises distribution function at all data points, thereby arriving at a sample of approximately uniformly distributed data, if the original observations have a von Mises distribution. The one-sample Watson test is then applied to the transformed data as above.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.2, World Scientific Press, Singapore.

Stephens, M. (1970).  Use of the Kolmogorov-Smirnov, Cramer-von Mises and related statistics without extensive tables.  Journal of the Royal Statistical Society, B32, 115-122.
}

\seealso{
\link{circ.range}, \link{kuiper}, \link{rao.spacing}, \link{r.test}, \link{v0.test}
}

\examples{
# Generate data from the uniform distribution on the circle.
data <- runif(100, 0, 2*pi)  
watson(data)
# Generate data from a von Mises distribution.
data <- rvm(50, 0, 4) 
watson(data, 0.05, dist='vm')
}

\keyword{htest}


