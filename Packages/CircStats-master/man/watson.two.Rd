\name{watson.two}
\title{Watson's Two-Sample Test of Homogeneity}
\alias{watson.two}

\description{
Performs Watson's test for homogeneity on two samples of circular data. 
}

\usage{
watson.two(x, y, alpha=0, plot=FALSE)
}

\arguments{
\item{x}{vector of circular data measured in radians.}
\item{y}{vector of circular data measured in radians.}
\item{alpha}{significance level of the test.  Valid levels are 0.001, 0.01, 0.05, 0.1.  This argument may be ommited, in which case, a range for the p-value will be returned.}
\item{plot}{logical value.  If TRUE, an overlayed plot of both empirical distribution functions will be sent to the current graphics device.  The default value if FALSE.}
}

\value{
NULL
}

\note{
Watson's two-sample test of homogeneity is performed, and the results are printed to the screen.  If alpha is specified and non-zero, the test statistic is printed along with the critical value and decision.  If alpha is omitted, the test statistic is printed and a range for the p-value of the test is given.  If plot=TRUE, an overlayed plot of both empirical distribution functions will be sent to the current graphics device.
}

\details{
Critical values for the test statistic are obtained using the asymptotic distribution of the test statistic.  It is recommended to use the obtained critical values and ranges for p-values only for combined sample sizes in excess of 17.  Tables are available for smaller sample sizes and can be found in Mardia (1972) for instance.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.5, World Scientific Press, Singapore.
}

\examples{
# Perform a two-sample test of homogeneity on two
# simulated data sets.
data1 <- rvm(20, 0, 3)
data2 <- rvm(20, pi, 2)
watson.two(data1, data2, alpha=0.05, plot=TRUE)
watson.two(data1, data2)
}

\keyword{htest}


