\name{vm.bootstrap.ci}
\title{Bootstrap Confidence Intervals}
\alias{vm.bootstrap.ci}

\description{
Generates simple bootstrap confidence intervals for the parameters of a von Mises distribtution: the mean direction mu, and the concentration parameter kappa.
}


\usage{
vm.bootstrap.ci(x, bias=FALSE, alpha=0.05, reps=1000, print=TRUE)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
\item{bias}{logical flag: if TRUE, the replication estimates for kappa are computed with a bias corrected method.  See est.kappa.  Default is FALSE, i.e. no bias correction.}
\item{alpha}{parameter determining level of confidence intervals. 1-alpha confidence intervals for mu and kappa are computed.  By default, 95\% confidence intervals are generated.}
\item{reps}{number of resampled data sets to use.  Default is 1000.}
\item{print}{logical flag indicating whether the algorithm should print a message indicating which set of replicates is currently being drawn.  Default is TRUE.}
}



\value{
A list is returned with the following components:  mu.ci and kappa.ci contain the limits of the confidence intervals for mu and kappa respectively.  mu.reps and kappa.reps contain the estimates of mu and kappa for each resampled data set.
}

\note{
The confidence intervals are printed to the screen.
}

\details{
Percentile confidence intervals are computed by resampling from the original data set B times.  For each resampled data set, the MLE's of mu and kappa are computed.  The bootstrap confidence intervals are the alpha/2 and 1-alpha/2 percentiles of the B MLE's computed for each resampled data set.
}

\seealso{
\link{vm.ml}, \link{est.kappa}
}

\examples{
x <- rvm(25, 0, 3)
x.bs <- vm.bootstrap.ci(x, alpha=.10)
hist(x.bs$kappa.reps)
}



\keyword{htest}

