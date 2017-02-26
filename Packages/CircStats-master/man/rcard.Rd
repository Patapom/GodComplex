\name{rcard}
\title{Random Generation from the Cardoid Distribution}
\alias{rcard}

\description{
Generates pseudo-random numbers from a cardoid distribution. 
}

\usage{
rcard(n, mu, r)
}

\arguments{
\item{n}{number of random variables to generate.}
\item{mu}{mean direction of the distribution in radians.}
\item{r}{concentration parameter of the distribution.  The absolute value of r must be less than 0.5.}
}

\value{
Returns a vector of n independent random variables generated from a cardoid distribution.
}

\details{
An acceptance/rejection method of simulation is employed.
}

\seealso{
\link{dcard}
}

\keyword{distribution}


