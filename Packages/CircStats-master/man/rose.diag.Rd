\name{rose.diag}
\title{Rose Diagram}
\alias{rose.diag}

\description{
Creates a rose diagram of a circular data set on the current graphics device.    
}

\usage{
rose.diag(x, bins, main="", prop=1, pts=FALSE, cex=1, pch=16, dotsep=40, shrink=1)
}


\arguments{
\item{x}{vector of directional data measured in radians.}
\item{bins}{number of groups to partition the circle with.  This will be the number of petals or sectors in the rose diagram.}
\item{main}{title of plot.  Default is no title.}
\item{prop}{numerical constant determining the radii of the sectors.  By default, prop = 1.}
\item{pts}{logical value.  If TRUE, points will be stacked on the circumference of the circle, according to bins - one point for each observation. The default value is FALSE, no points plotted.}
\item{cex}{size of points, if pts = TRUE.  By default,  cex = 1.  See help on cex in help section for par.}
\item{pch}{if pts = TRUE, pch determines the character used.  See help on pch in help section for par.}
\item{dotsep}{constant used to specify the distance between stacked points, if pts = TRUE.  Default is 40; larger values will create smaller spaces, while smaller values create larger spaces.  This option can be useful when pch is other than 1, or when shrink is other than 1 (see below).}
\item{shrink}{parameter that controls the size of the plotted circle.  Default is 1.  Larger values shrink the circle, while smaller values enlarge the circle.  This option can be useful when pts = TRUE.}
}

\value{
NULL
}

\note{
A rose diagram is plotted on the current graphics device.
}

\details{
The circumference of the circle is split into groups, the number of groups specified by bins.  For each group, a sector is drawn.  The radii of the sectors are by default equal to the square root of the relative frequencies of observations in each group.  This ensures that the area of the sector is proportional to the group frequency.  The length of the radii can be controlled by varying the parameter prop. 
}

\seealso{
\link{circ.plot}
}

\examples{
# Generate uniform data and create several rose diagrams.  
# Some optional parameters may be needed to optimize plots.
data <- runif(50, 0, 2*pi)
rose.diag(data, bins = 18, main = 'Uniform Data')
rose.diag(data, bins = 18, main = 'Stacked Points', pts=TRUE)
# Generate von Mises data and create several rose diagrams.
data <- rvm(25, 0, 5)
rose.diag(data, bins=18, pts=TRUE) # Points fall out of bounds.
rose.diag(data, bins=36, prop=1.5, pts=TRUE, shrink=1.5)
# Adjust optional parameters to fit all points on plot.
}

\keyword{hplot}


