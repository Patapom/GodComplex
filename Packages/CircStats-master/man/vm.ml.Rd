\name{vm.ml}
\title{von Mises Maximum Likelihood Estimates}
\alias{vm.ml}

\description{
Computes the maximum likelihood estimates for the parameters of a von Mises distribution:  the mean direction and the concentration parameter.
}

\usage{
vm.ml(x, bias=FALSE)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{bias}{logical flag: if TRUE, the estimate for kappa is computed with a bias corrected method.  See est.kappa.  Default is FALSE, i.e. no bias correction.}
}

\value{
Returns a data frame with two components:  mu and kappa, the MLES of the respective parameters.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 4.2.1, World Scientific Press, Singapore.
}

\seealso{
\link{circ.mean}, \link{est.kappa}, \link{vm.bootstrap.ci}
}

\keyword{htest}


