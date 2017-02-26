\name{r.test}
\title{Rayleigh Test of Uniformity:  General Unimodal Alternative}
\alias{r.test}

\description{
Performs a Rayleigh test of uniformity, assessing the significance of the mean resultant length.  The alternative hypothesis is a unimodal distribution with unknown mean direction and unknown mean resultant length.  
}

\usage{
r.test(x, degree=FALSE)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{degree}{logical flag:  if TRUE, data is assumed to be measured in degrees rather than radians. Default is FALSE.}
}

\value{
Returns a list with two components:  the mean resultant length, r.bar, and the p-value of the test statistic, p.value.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Sections 3.3.2 and 3.4.1, World Scientific Press, Singapore.
}

\seealso{
\link{circ.range}, \link{kuiper}, \link{rao.spacing}, \link{v0.test}, \link{watson}
}


\examples{
data <- rvm(25, pi, 2)
r.test(data)
}

\keyword{htest}

