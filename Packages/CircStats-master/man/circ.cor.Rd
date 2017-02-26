\name{circ.cor}
\title{Correlation Coefficient for Angular Variables}
\alias{circ.cor}

\description{
Computes a circular version of the Pearson's product moment correlation, and performs a significance test if requested. 
}

\usage{
circ.cor(alpha, beta, test=FALSE)
}

\arguments{
\item{alpha}{vector of circular data measured in radians.}
\item{beta}{vector of circular data measured in radians.}
\item{test}{if test = TRUE, then a significance test for the correlation coefficient is computed.}
}

\value{
Returns a data frame with variables r, a circular version of the Pearson's product moment correlation, test.stat and p.value, the test statistic and p-value respectively, for testing significance of the correlation coefficient. test.stat and p.value are by default not produced, but are given when test=TRUE is specified in the function call.
}

\details{
The correlation coefficient is computed like Pearson's product moment correlation for two linear variables X and Y.  In the computational formula,  however, (xi - xbar) and (yi - ybar) are replaced by sin(xi - xbar) and sin(yi - ybar), where xbar and ybar in the second two expressions are the mean directions of the samples.
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 8.2, World Scientific Press, Singapore.

Jammalamadaka, S. and Sarma, Y. (1988).  A correlation coefficient for angular variables.  Statistical Theory and Data Analysis 2. North Holland:  New York.
}

\examples{
# Generate two circular data sets, and compute their correlation.
data1 <- rvm(50, 0, 3)
data2 <- data1 + pi + rvm(50, 0, 10)
circ.cor(data1, data2, test=TRUE)
}

\keyword{univar}

