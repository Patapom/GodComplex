\name{circ.reg}    
\title{Circular-Circular Regression}
\alias{circ.reg}

\description{
Fits a regression model for a circular dependent and circular independent variable.  
}

\usage{
circ.reg(alpha, theta, order=1, level=0.05)
}

\arguments{
\item{alpha}{vector of data for the independent circular variable, measured in radians.}
\item{theta}{vector of data for the dependent circular variable, measured in radians.}
\item{order}{order of trigonometric polynomial to be fit.  order must be an integer value.  By default, order=1.}
\item{level}{level of the test for the significance of higher order trigonometric terms.}
}

\value{
A data frame is returned with the following components. 

\item{rho}{square root of the average of the squares of the estimated conditional concentration parameters of theta given alpha.}
\item{fitted}{fitted values of the model.}
\item{data}{matrix whose columns correspond to alpha and theta.}
\item{residuals}{circular residuals of the model.}
\item{coef}{matrix whose entries are the estimated coefficients of the model. The first column corresponds to the coefficients of the model predicting the cosine of theta, while the second column contains the estimates for the model predicting the sine of theta.  The rows of the matrix correspond to the coefficients according to increasing trigonometric order.}
\item{pvalues}{p-values testing whether the (order + 1) trigonometric terms are significantly different from zero.}  
\item{A.k}{is mean of the cosines of the circular residuals.}
\item{kappa}{assuming the circular residuals come from a von Mises distribution, kappa is the MLE of the concentration parameter.}
}

\details{
A trigonometric polynomial of alpha is fit against the cosine and sine of theta.  The order of trigonometric polynomial is specified by order.  Fitted values of theta are obtained by taking the inverse tangent of the predicted values of sin(theta) devided by the predicted values of cos(theta).  Details of the regression model can be found in Sarma and Jammalamadaka (1993).
}

\references{
Jammalamadaka, S. Rao and SenGupta, A. (2001). Topics in Circular Statistics, Section 8.3, World Scientific Press, Singapore.

Sarma, Y. and Jammalamadaka, S. (1993).  Circular Regression.  Statistical Science and Data Analysis, 109-128.  Proceeding of the Thrid Pacific Area Statistical Conference.  VSP: Utrecht, Netherlands.
}

\examples{
# Generate a data set of dependent circular variables.
data1 <- runif(50, 0, 2*pi)
data2 <- atan2(0.15*cos(data1) + 0.25*sin(data1), 0.35*sin(data1)) + rvm(50, 0, 5)

# Fit a circular regression model.
circ.lm <- circ.reg(data1, data2, order=1)
# Obtain a crude plot a data and fitted regression line.
plot(data1, data2)
circ.lm$fitted[circ.lm$fitted>pi] <- circ.lm$fitted[circ.lm$fitted>pi] - 2*pi 

points(data1[order(data1)], circ.lm$fitted[order(data1)], type='l')
}

\keyword{models}

