\name{est.kappa}
\title{Estimate Kappa}
\alias{est.kappa}

\description{
Computes the maximum likelihood estimate of kappa, the concentration parameter of a von Mises distribution, given a set of angular measurements. 
}

\usage{
est.kappa(x, bias=FALSE)
}

\arguments{
\item{x}{vector of circular data measured in radians.}
\item{bias}{logical parameter determining whether a bias correction is used in the computation of the MLE.  Default for bias is FALSE - the bias correction is not used.}
}

\value{
Maximum likelihood estimate of kappa, the concentration parameter of a von Mises distribution, given a set of angular measurements.
}

\details{
Best and Fisher (1981) show that the MLE of kappa is seriously biased when both sample size and mean resultant length are small.  They suggest a bias-corrected estimate for kappa when n < 16.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 4.2.1, World Scientific Press, Singapore.

Best, D. and Fisher N. (1981).  The bias of the maximum likelihood estimators of the von Mises-Fisher concentration parameters.  Communications in Statistics - Simulation and Computation, B10(5), 493-502.
}

\seealso{
\link{circ.mean}, \link{circ.disp}, \link{circ.summary}, \link{est.rho}
}

\examples{
data <- rvm(15, 0, 3)
est.kappa(data)
est.kappa(data, bias=TRUE)
}

\keyword{htest}

