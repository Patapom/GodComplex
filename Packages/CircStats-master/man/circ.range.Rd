\name{circ.range}
\title{Circular Range}
\alias{circ.range}
\alias{nCk}

\description{
Computes the circular range of a data set and performs a test of uniformity if specified.
}

\usage{
circ.range(x, test=FALSE)
}

\arguments{
\item{x}{vector of directional data measured in radians.}
\item{test}{logical flag:  if TRUE then the test of uniformity is performed; otherwise the test is not performed.  Default is FALSE.}
}

\value{
Returns a dataframe with the circular range, r.  If the significance test is requested the p-value of the test is returned as p.value.
}

\details{
The circular range is the shortest arc on the circle containing the entire set of data.  The p-value is computed using the exact distribution of the circular range under the hypothesis of uniformity.
}

\note{nCk(n,k) evaluate combinatoric, it should not be called by the users.}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 7.4, World Scientific Press, Singapore.
}

\seealso{
\link{kuiper}, \link{rao.spacing}, \link{r.test}, \link{v0.test}, \link{watson}.
}

\examples{
data <- rvm(50, 0, 2)
circ.range(data, test=TRUE)
data <- runif(50, 0, 2*pi)
circ.range(data, test=TRUE)
}

\keyword{univar}
\keyword{htest}


