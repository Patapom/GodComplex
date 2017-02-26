\name{rao.homogeneity}
\title{Rao's Tests for Homogeneity}
\alias{rao.homogeneity}

\description{
Performs Rao's test for homogeneity on k populations of angular data. 
}

\usage{
rao.homogeneity(x, alpha=0)
}

\arguments{
\item{x}{a list of angular variables measured in radians.  Each component of the list should be a vector corresponding to one of the k samples. The data must be in the form of a list, to allow for samples of varying sizes.}
\item{alpha}{numeric value specifying the significance level of the test. Default is 0, in which case p-values for the test statistic are returned.}
}

\value{
NULL
}

\note{
The test is performed, and the results are written to the screen. Test results are given for both the test of equality of polar vectors, and of dispersions.  If alpha is specified, the test statistic is returned, along with the level alpha critical value.  If alpha is not specified, a p-value for the test is computed.  
}

\details{
Critical values and p-values are determined according to the chi-squared approximation of the the test statistic.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.6.1, World Scientific Press, Singapore.

Rao, J.S. (1967). Large sample tests for the homogeneity of angular data, Sankhya, Ser, B., 28.
}

\keyword{htest}

