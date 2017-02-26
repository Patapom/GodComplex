\name{circ.plot}
\title{Circular Data Plot}
\alias{circ.plot}

\description{
Creates a plot of circular data points on the current graphics device.  Data points are either plotted as points on the unit circle, or the range of the circle is divided into a specified number of bins, and points are stacked in the bins corresponding to the number of observations in each bin. 
}

\usage{
circ.plot(x, main="", pch=16, stack=FALSE, bins=0, cex=1, dotsep=40, shrink=1)
}

\arguments{
\item{x}{vector of observations to be plotted, measured in radians.}
\item{main}{title of plot.}
\item{pch}{point character to use.  See help on par.}
\item{stack}{logical flag: if TRUE, points are stacked on the perimeter of the circle.  Otherwise, all points are plotted on the perimeter of the circle.  Default is FALSE.}
\item{bins}{if stack = TRUE, bins is the number of arcs to partition the circle with.}
\item{cex}{point character size.  See help on par.}
\item{dotsep}{constant used to specify the distance between stacked points, if stack = TRUE.  Default is 40; larger values will create smaller spaces, while smaller values create larger spaces.  This option can be useful when pch is other than 1, or when shrink is other than 1.}
\item{shrink}{parameter that controls the size of the plotted circle.  Default is 1.  Larger values shrink the circle, while smaller values enlarge the circle.  This option is useful when stack = TRUE.}
}

\details{
When there are many closely distributed observations, stacking is recommended.  Otherwise, much information can be lost due to overplotting of points.  When stacking the points, if there are many points in a particular bin, it may be necessary to shrink the plot of the circle so that all points fit.  This is controlled with the parameter shrink.  Generally the parameter dotsep does not need adjustment, however, when shrinking the plot, or for a very large number of observations, it may be helpful.
}

\note{this function use function \code{\link{eqscplot}} from package MASS.}

\seealso{
\link{rose.diag}.
}

\examples{
# Generate 100 observations from a von Mises distribution.
# with mean direction 0 and concentration 3.
data.vm <- rvm(100, 0, 3) 
# Plot data set. All points do not fit on plot.
circ.plot(data.vm, stack=TRUE, bins=150) 
# Shrink the plot so that all points fit.
circ.plot(data.vm, stack=TRUE, bins=150, shrink=1.5) 
}

\keyword{hplot}

