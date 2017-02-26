\name{change.pt}
\title{Change Point Test}
\alias{change.pt}

\description{
Tests for a change in mean direction, concentration, or both, given a set of directional data points. 
}

\usage{
change.pt(x)
}

\arguments{
\item{x}{vector of angular measurements in radians.}
}

\value{
Returns a data frame with variables n, rho=r/n, rmax, k.r, rave, tmax, k.t, and tave. The first of these is the sample size, followed by the overall mean resultant length. Both of these are needed to enter any of the tables or nomograms(see under Details). The other values represent the change point test statistics.  While rmax and rave test for a change in mean direction (with unknown concentration), tmax and tave are useful in the context of testing more generally, for a change in mean direction and/or concentration. k.r and k.t are the observation numbers  for which rmax and tmax attain their maximum value and indicate the observation at which the change is most likely to have occurred, when the tables or nomograms indicate significance. 
}

\details{
In either context, the user can choose which statistic (max or ave) to use, and then consult the appropriate table provided in the book referenced below. The critical values for these 4 statistics are to be found in Table 11.3 (or Figure 11.3) for rmax, Table 11.4 (or Figure 11.4) for rave, Figure 11.5 for tmax and Figure 11.6 for tave.
}

\seealso{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Chapter 11, World Scientific Press, Singapore.  
}

\keyword{htest}

