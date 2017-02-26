\name{v0.test}
\title{Rayleigh Test of Uniformity:  Alternative with Specified Mean Direction}
\alias{v0.test}

\description{
Performs a Rayleigh test of uniformity, assessing the significance of the mean resultant length.  The alternative hypothesis is a unimodal distribution with a specified mean direction and unknown mean resultant length.  
}

\usage{
v0.test(x, mu0=0, degree=FALSE)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{mu0}{Specified mean direction in alternative hypothesis, measured in the same units (degrees or radians) as the data.}
\item{degree}{logical flag:  if TRUE, data is assumed to be measured in degrees rather than radians. Default is FALSE.}
}

\value{
Returns a list with two components:  the mean resultant length, r0.bar, and the p-value of the test statistic, p.value.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Sections 3.3.3 and 3.4.1, World Scientific Press, Singapore.
}

\seealso{
\link{circ.range}, \link{kuiper}, \link{rao.spacing}, \link{r.test}, \link{watson}
}

\examples{
data <- rvm(25, pi, 2)
v0.test(data, mu0=pi)
}

\keyword{htest}

