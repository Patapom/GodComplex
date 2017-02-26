\name{rwrpstab}
\title{Random Generation from the Wrapped Stable Distribution}
\alias{rwrpstab}

\description{
Generates pseudo-random numbers from a wrapped stable distribution.
}

\usage{
rwrpstab(n, index, skewness, scale=1)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{index}{number from the interval (0, 2]. An index of 2 corresponds to the normal, 1 to the Cauchy.  Smaller values mean longer tails.}
\item{skewness}{number giving the modified skewness. Negative values correspond to skewness to the left (the median is smaller than the mean, if it exists), and positive values correspond to skewness to the right (the median is larger than the mean). The absolute value of skewness should not exceed 1.}
\item{scale}{the scale of the distribution}
}

\value{
Returns a vector of n independent random variables generated from a wrapped stable distribution.
}

\details{
n random variables are generated from a stable distribution with with parameters index, skewness and scale. The function returns these values modulo 2*pi.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 2.2.8, World Scientific Press, Singapore.
}

\keyword{distribution}


