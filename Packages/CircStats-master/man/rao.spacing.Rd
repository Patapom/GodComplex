\name{rao.spacing}
\title{Rao's Spacing Test of Uniformity}
\alias{rao.spacing}

\description{
Performs Rao's Spacing Test of uniformity. 
}

\usage{
rao.spacing(x, alpha=0, rad=TRUE)
}

\arguments{
\item{x}{numeric vector of angular data measured in degrees.}
\item{alpha}{numeric value specifying the significance level of the test. The default value is 0, in which case, a range for the p-value will be returned.  Valid significance levels are 0.10, 0.05, 0.01 and 0.001.}
\item{rad}{logical value.  If true, data will be assumed to be measured radians.  If false, data will be assumed to be measured in degrees.}
}

\value{
NULL
}

\note{
The Rao's Spacing Test is performed, and the results are written to the screen.
}

\details{
If alpha is specified, critical values are determined from a table of simulated critical points (see reference below).  If alpha is not specified, a range for the p-value is determined using the table of simulated critical points.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.4, World Scientific Press, Singapore.

Rao, J.S. (1976). Some tests based on arc-lengths for the circle. Sankhya, The Indian Journal of Statistics, Serial B(4), 38, 329-338.

Russell, G.S. and Levitin, D.J. (1995).  An expanded table of probability values for Rao's Spacing Test.  Communications in Statistics - Simulation and Computation, 24, 4, 879-888.
}

\seealso{
\link{circ.range}, \link{kuiper}, \link{r.test}, \link{v0.test}, \link{watson}
}

\keyword{htest}


