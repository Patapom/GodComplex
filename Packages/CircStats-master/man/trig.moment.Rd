\name{trig.moment}    
\title{Trigonometric Moments}
\alias{trig.moment}  

\description{
Computes the specified order trigonometric moment for a set of directional data points. 
}

\usage{
trig.moment(x, p=1, center=FALSE)
}

\arguments{
\item{x}{directional data set measured in radians.}
\item{p}{order of trigonometric moment to be computed. Default is for a first order trigonometric moment.}
\item{center}{logical flag whether to compute centered moments or not.  Default is to compute an uncentered moment.}
}

\value{
Returns a data frame with variables mu.p, rho.p, cos.p, and sin.p, respectively the pth trigonometric moment's direction, resultant length, and real and imaginary components.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 1.3, World Scientific Press, Singapore.
}

\seealso{
\link{circ.disp}, \link{circ.mean}, \link{circ.summary}, \link{est.kappa}, \link{est.rho}
}

\keyword{math}

