\name{dcard}   
\title{Cardioid Density Function}
\alias{dcard}

\description{
Returns the cardoid density function evaluated at a particular value. 
}

\usage{
dcard(theta, mu, r)
}

\arguments{
\item{theta}{vector of angles measured in radians at which to evaluate the density function.}
\item{mu}{mean direction of the distribution.}
\item{r}{concentration parameter of the distribution.  Absolute value of r must be less than 0.5.}
}

\value{
Returns the cardoid density function evaluated at theta.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.2, World Scientific Press, Singapore.
}

\seealso{
\link{rcard}
}

\keyword{distribution}

