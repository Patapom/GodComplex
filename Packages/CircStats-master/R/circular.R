###############################################################
#       CircStats package                                     #
###############################################################

###############################################################
#                                                             #
#       Original Splus: Ulric Lund                            #
#       E-mail: ulund@calpoly.edu                             #
#                                                             #
###############################################################

###############################################################
#                                                             #
#       R port: Claudio Agostinelli  <claudio@unive.it>       #
#                                                             #
#       Date: April, 17, 2003                                 #
#       Version: 0.1-7                                        #
#                                                             #
###############################################################

###############################################################
## Modified December 23, 2002
## Modified January 14, 2003

A1 <- function(kappa) {
    result <- besselI(kappa, nu=1, expon.scaled = TRUE)/besselI(kappa, nu=0, expon.scaled = TRUE)
	return(result)
}

###############################################################
## Modified December 2, 2002
## Modified December 23, 2002
## An alias of the besselI(x, 1) function for compatibility with Splus version

I.1 <- function(x) {
    besselI(x=x, nu=1, expon.scaled = FALSE)
}

#I.1 <- function(x) {
#	t <- x/3.75
#	ifelse (x < 3.75,
#            x * (0.5 + 0.87890594 * t^2 + 0.51498869 * t^4 + 
#			0.15084934 * t^6 + 0.02658733 * t^8 + 0.00301532 * t^
#			10 + 0.00032411 * t^12),
#            
#            x^(-0.5) * exp(x) * (0.39894228 - 0.03988024 * t^(-1) -
#			0.00362018 * t^(-2) + 0.00163801 * t^(-3) - 
#			0.01031555 * t^(-4) + 0.02282967 * t^(-5) - 
#			0.02895312 * t^(-6) + 0.01787654 * t^(-7) - 
#			0.00420059 * t^(-8))
#            )
#}

###############################################################
## Modified December 23, 2002
## An alias of the besselI(x, 0) function for compatibility with Splus version

I.0 <- function(x) {
    besselI(x=x, nu=0, expon.scaled = FALSE)
}

#I.0 <- function(x) {
#	p1 <- 1
#	p2 <- 3.5156229
#	p3 <- 3.0899424
#	p4 <- 1.2067492
#	p5 <- 0.2659732
#	p6 <- 0.360768/10
#	p7 <- 0.45813/100
#	q1 <- 0.39894228
#	q2 <- 0.1328592/10
#	q3 <- 0.225319/100
#	q4 <- -0.157565/100
#	q5 <- 0.916281/100
#	q6 <- -0.2057706/10
#	q7 <- 0.2635537/10
#	q8 <- -0.1647633/10
#	q9 <- 0.392377/100
#	y <- (x/3.75)^2
#	ax <- abs(x)
#	z <- 3.75/ax
#	ifelse(abs(x) < 3.75, p1 + y * (p2 + y * (p3 + y * (p4 + y * (p5 + y 
#       * (p6 + y * p7))))), (exp(ax)/sqrt(ax)) * (q1 + z * (q2 + z * (
#		q3 + z * (q4 + z * (q5 + z * (q6 + z * (q7 + z * (q8 + z * 
#		q9)))))))))
#}

###############################################################
## Modified December 2, 2002

A1inv <- function(x) {
	ifelse (0 <= x & x < 0.53,
		    2 * x + x^3 + (5 * x^5)/6,
            
            ifelse (x < 0.85,
		            -0.4 + 1.39 * x + 0.43/(1 - x),
                    
		            1/(x^3 - 4 * x^2 + 3 * x)
                   )
           )
}

###############################################################

change.pt <- function(x) {
	phi <- function(x) {
		arg <- A1inv(x)
		if(besselI(x=arg, nu=0, expon.scaled = FALSE) != Inf)
			result <- x * A1inv(x) - log(besselI(x=arg, nu=0, expon.scaled = FALSE))
		else result <- x * A1inv(x) - (arg + log(1/sqrt(2 * pi * arg) * (1 + 1/(8 * arg) + 9/(128 * arg^2) + 225/(1024 * arg^3))))
		result
	}
	n <- length(x)
	rho <- est.rho(x)
	R1 <- c(1:n)
	R2 <- c(1:n)
	V <- c(1:n)
	for(k in 1:(n - 1)) {
		R1[k] <- est.rho(x[1:k]) * k
		R2[k] <- est.rho(x[(k + 1):n]) * (n - k)
		if(k >= 2 & k <= (n - 2)) {
			V[k] <- k/n * phi(R1[k]/k) + (n - k)/n * phi(R2[k]/(n - k))
		}
	}
	R1[n] <- rho * n
	R2[n] <- 0
	R.diff <- R1 + R2 - rho * n
	rmax <- max(R.diff)
	rave <- mean(R.diff)
	k.r <- (1:n)[R.diff == max(R.diff)]
	V <- V[2:(n - 2)]
	if(n > 3) {
		tmax <- max(V)
		tave <- mean(V)
		k.t <- (1:(n - 3))[V == max(V)] + 1
	}
	else stop("Sample size must be at least 4")
	data.frame(n, rho, rmax, k.r, rave, tmax, k.t, tave)
}

###############################################################

est.rho <- function(x) {
	n <- length(x)
	sinr <- sum(sin(x))
	cosr <- sum(cos(x))
	sqrt(sinr^2 + cosr^2)/n
}

###############################################################

circ.cor <- function(alpha, beta, test = FALSE) {
	n <- length(alpha)
	alpha.bar <- circ.mean(alpha)
	beta.bar <- circ.mean(beta)
	num <- sum(sin(alpha - alpha.bar) * sin(beta - beta.bar))
	den <- sqrt(sum(sin(alpha - alpha.bar)^2) * sum(sin(beta - beta.bar)^2))
	r <- num/den
	result <- data.frame(r)
	if(test == TRUE) {
		l20 <- mean(sin(alpha - alpha.bar)^2)
		l02 <- mean(sin(beta - beta.bar)^2)
		l22 <- mean((sin(alpha - alpha.bar)^2) * (sin(beta - beta.bar)^2))
		test.stat <- sqrt((n * l20 * l02)/l22) * r
		p.value <- 2 * (1 - pnorm(abs(test.stat)))
		result <- data.frame(r, test.stat, p.value)
	}
	result
}

###############################################################

circ.disp <- function(x) {
	n <- length(x)
	c <- sum(cos(x))
	s <- sum(sin(x))
	r <- sqrt(c^2 + s^2)
	rbar <- r/n
	var <- 1 - rbar
	data.frame(n, r, rbar, var)
}

###############################################################
# Modified December, 6, 2005

circ.mean <- function(x) {
	sinr <- sum(sin(x))
	cosr <- sum(cos(x))
	circmean <- atan2(sinr, cosr)
	circmean
}

###############################################################
## Modified March 5, 2002
## Modified December 2, 2002
## Modified November 18, 2003

circ.plot <- function(x, main = "", pch = 16, stack = FALSE, bins = 0, cex = 1, dotsep = 40, shrink = 1) {
 x <- x %% (2 * pi)
 if (require(MASS)) {
	eqscplot(x=cos(seq(0, 2 * pi, length = 1000)), y=sin(seq(0, 2 * pi, length = 1000)), axes = FALSE, xlab = "", ylab = "", main = main, type = "l", xlim = shrink * c(-1, 1), ylim = shrink * c(-1, 1), ratio=1, tol=0.04)
	lines(c(0, 0), c(0.9, 1))
	text(0.005, 0.85, "90", cex = 1.5)
	lines(c(0, 0), c(-0.9, -1))
	text(0.005, -0.825, "270", cex = 1.5)
	lines(c(-1, -0.9), c(0, 0))
	text(-0.8, 0, "180", cex = 1.5)
	lines(c(0.9, 1), c(0, 0))
	text(0.82, 0, "0", cex = 1.5)
	text(0, 0, "+", cex = 2)
	n <- length(x)
	z <- cos(x)
	y <- sin(x)
	if(stack == FALSE)
		points(z, y, cex = cex, pch = pch)
	else {
		bins.count <- c(1:bins)
		arc <- (2 * pi)/bins
		for(i in 1:bins) {
			bins.count[i] <- sum(x <= i * arc & x > (i - 1) * arc)
		}
		mids <- seq(arc/2, 2 * pi - pi/bins, length = bins)
		index <- cex/dotsep
		for(i in 1:bins) {
			if(bins.count[i] != 0) {
				for(j in 0:(bins.count[i] - 1)) {
				  r <- 1 + j * index
				  z <- r * cos(mids[i])
				  y <- r * sin(mids[i])
				  points(z, y, cex = cex, pch = pch)
				}
			}
		}
	}

 } else {
    stop("To use this function you have to install the package MASS (VR)\n")
 }


}

###############################################################

circ.range <- function(x, test = FALSE) {
	x <- sort(x %% (2 * pi))
	n <- length(x)
	spacings <- c(diff(x), x[1] - x[n] + 2 * pi)
	range <- 2 * pi - max(spacings)
	result <- data.frame(range)
	if(test == TRUE) {
		stop <- floor(1/(1 - range/(2 * pi)))
		index <- c(1:stop)
		sequence <- ((-1)^(index - 1)) * nCk(n, index) * (1 - index * (1 - range/(2 * pi)))^(n - 1)
		p.value <- sum(sequence)
		result <- data.frame(range, p.value)
	}
	result
}

###############################################################
# Modified December 6, 2005

circ.reg <- function(alpha, theta, order = 1, level = 0.05) {
	n <- length(alpha)
	ctheta <- cos(theta)
	stheta <- sin(theta)
	order.matrix <- t(matrix(rep(c(1:order), n), ncol = n))
	cos.alpha <- cos(alpha * order.matrix)
	sin.alpha <- sin(alpha * order.matrix)
	cos.lm <- lm(ctheta ~ cos.alpha + sin.alpha)
	sin.lm <- lm(stheta ~ cos.alpha + sin.alpha)
	cos.fit <- cos.lm$fitted
	sin.fit <- sin.lm$fitted
	g1.sq <- t(cos.fit) %*% cos.fit
	g2.sq <- t(sin.fit) %*% sin.fit
	rho <- sqrt((g1.sq + g2.sq)/n)
	theta.fitted <- atan2(sin.fit, cos.fit)
	Y1 <- ctheta
	Y2 <- stheta
	ones <- matrix(1, n, 1)
	X <- cbind(ones, cos.alpha, sin.alpha)
	W <- cbind(cos((order + 1) * alpha), sin((order + 1) * alpha))
	M <- X %*% solve(t(X) %*% X) %*% t(X)
	I <- diag(n)
	H <- t(W) %*% (I - M) %*% W
	N <- W %*% solve(H) %*% t(W)
	c <- n - (2 * order + 1)
	N1 <- t(Y1) %*% (I - M) %*% N %*% (I - M) %*% Y1
	D1 <- t(Y1) %*% (I - M) %*% Y1
	T1 <- c * (N1/D1)
	N2 <- t(Y2) %*% (I - M) %*% N %*% (I - M) %*% Y2
	D2 <- t(Y2) %*% (I - M) %*% Y2
	T2 <- c * (N2/D2)
	p1 <- 1 - pchisq(T1, 2)
	p2 <- 1 - pchisq(T2, 2)
	pvalues <- cbind(p1, p2)
	circ.lm <- list()
	circ.lm$rho <- rho
	circ.lm$fitted <- theta.fitted %% (2 * pi)
	circ.lm$x <- cbind(alpha, theta)
	circ.lm$residuals <- (theta - theta.fitted) %% (2 * pi)
	circ.lm$coef <- cbind(cos.lm$coefficients, sin.lm$coefficients)
	circ.lm$pvalues <- pvalues
	circ.lm$A.k <- mean(cos(circ.lm$residuals))
	circ.lm$kappa <- A1inv(circ.lm$A.k)
	if(pvalues[1] < level & pvalues[2] < level)
		circ.lm$message <- paste(
			"Higher order terms are significant at the ", level, 
			" level", sep = "")
	else circ.lm$message <- paste(
			"Higher order terms are not significant at the ", 
			level, " level", sep = "")
	circ.lm
}

###############################################################
# Modified December 6, 2005

circ.summary <- function(x) {
	n <- length(x)
	sinr <- sum(sin(x))
	cosr <- sum(cos(x))
	rho <- sqrt(sinr^2 + cosr^2)/n
	mean.dir <- atan2(sinr, cosr)
	result <- data.frame(n, mean.dir, rho)
	result
}

###############################################################

dcard <- function(theta, mu, r) {
	(1 + 2 * r * cos(theta - mu))/(2 * pi)
}

###############################################################

deg <- function(radian) {
	(radian * 180)/pi
}

##############################################################
# Modified January 14, 2003

dmixedvm <- function(theta, mu1, mu2, kappa1, kappa2, p) {
	p/(2 * pi * besselI(x=kappa1, nu=0, expon.scaled = TRUE)) * (exp(cos(theta - mu1) - 1))^kappa1 + (1 - p)/(2 * pi * besselI(x=kappa2, nu=0, expon.scaled = TRUE)) * (exp(cos(theta - mu2) - 1))^kappa2
}

###############################################################

dtri <- function(theta, r) {
	(4 - pi^2 * r + 2 * pi * r * abs(pi - theta))/(8 * pi)
}

###############################################################
# Modified January 14, 2003

dvm <- function (theta, mu, kappa) {
    1/(2 * pi * besselI(x = kappa, nu = 0, expon.scaled = TRUE)) * 
        (exp(cos(theta - mu) -1))^kappa
}

###############################################################

dwrpcauchy <- function(theta, mu, rho) {
	(1 - rho^2)/((2 * pi) * (1 + rho^2 - 2 * rho * cos(theta - mu)))
}

###############################################################
# Modified December, 31 2002 
# aggiunto il parametro tol che sostituira' acc
# aggiunto sd per poter specificare rho in forma diversa
# controllo sul valore di rho
# rho deve stare nell'intervallo [0,1]

dwrpnorm <- function(theta, mu, rho, sd=1, acc=1e-5, tol=acc) {
        if (missing(rho)) {
            rho <- exp(-sd^2/2)
        }
        if (rho < 0 | rho > 1)
            stop("rho must be between 0 and 1")
	var <- -2 * log(rho)
	term <- function(theta, mu, var, k)	{
		1/sqrt(var * 2 * pi) * exp( - ((theta - mu + 2 * pi * k)^2)/(2 * var))
	}
	k <- 0
	Next <- term(theta, mu, var, k)
	delta <- 1
	while(delta > tol) {
		k <- k + 1
		Last <- Next
		Next <- Last + term(theta, mu, var, k) + term(theta, mu, var, -k)
		delta <- abs(Next - Last)
	}
	Next
}

###############################################################

est.kappa <- function(x, bias = FALSE) {
	mean.dir <- circ.mean(x)
	kappa <- A1inv(mean(cos(x - mean.dir)))
	if(bias == TRUE) {
		kappa.ml <- kappa
		n <- length(x)
		if(kappa.ml < 2)
			kappa <- max(kappa.ml - 2 * (n * kappa.ml)^-1, 0)
		if(kappa.ml >= 2)
			kappa <- ((n - 1)^3 * kappa.ml)/(n^3 + n)
	}
	kappa
}

###############################################################
## Modified December 2, 2002
## Modified December 23, 2002
## An alias of the besselI(x, p) function for compatibility with Splus version

I.p <- function(p, x) {
    besselI(x=x, nu=p, expon.scaled = FALSE)
}

#I.p <- function(p, x) {
#	I.before <- I.0(x)
#	I.curr <- I.1(x)
#	if (p == 0)
#		I.next <- I.before
#	if (p == 1)
#		I.next <- I.curr
#	if (p != 0 && p != 1) {
#		n <- 1
#		I.next <- rep(0, length(x))
#		while(n < p) {
#			  I.next[I.next >=0] <- I.before[I.next >=0] - (2 * n * I.curr[I.next >=0])/x[I.next >=0]
#              I.next[I.next < 0] <- -1
#			  I.before <- I.curr
#			  I.curr <- I.next
#  			  n <- n + 1
#		}
#		I.next[I.next <0] <- 0
#	}
#	return(I.next)
#}


###############################################################

kuiper <- function(x, alpha = 0) {
	cat("\n", "      Kuiper's Test of Uniformity", "\n", "\n")
	kuiper.crits <- cbind(c(0.15, 0.1, 0.05, 0.025, 0.01), c(1.537, 1.62, 
		1.747, 1.862, 2.001))
	x <- sort(x %% (2 * pi))/(2 * pi)
	n <- length(x)
	i <- c(1:n)
	D.P <- max(i/n - x)
	D.M <- max(x - (i - 1)/n)
	V <- (D.P + D.M) * (sqrt(n) + 0.155 + 0.24/sqrt(n))
	cat("Test Statistic:", round(V, 4), "\n")
	if(alpha == 0) {
		if(V < 1.537)
			cat("P-value > 0.15", "\n", "\n")
		else if(V < 1.62)
			cat("0.10 < P-value < 0.15", "\n", "\n")
		else if(V < 1.747)
			cat("0.05 < P-value < 0.10", "\n", "\n")
		else if(V < 1.862)
			cat("0.025 < P-value < 0.05", "\n", "\n")
		else if(V < 2.001)
			cat("0.01 < P-value < 0.025", "\n", "\n")
		else cat("P-value < 0.01", "\n", "\n")
	}
	else {
		Critical <- kuiper.crits[(1:5)[alpha == c(kuiper.crits[, 1])],2]
		cat("Level", alpha, "Critical Value:", round(Critical, 4), "\n")
		if(V > Critical)
			cat("Reject Null Hypothesis", "\n", "\n")
		else cat("Do Not Reject Null Hypothesis", "\n", "\n")
	}
}

###############################################################

plot.edf <- function(x, ...) {
	x <- x %% (2 * pi)
	x <- sort(x)
	n <- length(x)
	plot(c(0, x, 2 * pi), c(0, seq(1:n)/n, 1), type = "s", xlim = c(0, 2 * pi), ylim = c(0, 1), ...)
}

###############################################################
## Modified October 12, 2009

pp.plot <- function(x, ref.line = TRUE) {
	n <- length(x)
	x <- sort(x %% (2 * pi))
	z <- c(1:n)/(n + 1)
	mu <- circ.mean(x) %% (2 * pi)
	kappa <- est.kappa(x)
	y <- c(1:n)
	for(i in 1:n) {
		y[i] <- pvm(x[i], mu, kappa)
	}
	plot(z, y, xlab = "von Mises Distribution", ylab = "Empirical Distribution")
	if(ref.line)
		abline(0, 1)
	data.frame(mu, kappa)
}

###############################################################

pvm <- function(theta, mu, kappa, acc = 1e-020) {
	theta <- theta %% (2 * pi)
	mu <- mu %% (2 * pi)
	pvm.mu0 <- function(theta, kappa, acc) {
		flag <- "true"
		p <- 1
		sum <- 0
		while(flag == "true") {
			term <- (besselI(x=kappa, nu=p, expon.scaled = FALSE) * sin(p * theta))/p
			sum <- sum + term
			p <- p + 1
			if(abs(term) < acc)
				flag <- "false"
		}
		theta/(2 * pi) + sum/(pi * besselI(x=kappa, nu=0, expon.scaled = FALSE))
	}
	if(mu == 0) {
		result <- pvm.mu0(theta, kappa, acc)
	}
	else {
		if(theta <= mu) {
			upper <- (theta - mu) %% (2 * pi)
			if(upper == 0)
				upper <- 2 * pi
			lower <- ( - mu) %% (2 * pi)
			result <- pvm.mu0(upper, kappa, acc) - pvm.mu0(lower, kappa, acc)
		}
		else {
			upper <- theta - mu
			lower <- mu %% (2 * pi)
			result <- pvm.mu0(upper, kappa, acc) + pvm.mu0(lower, kappa, acc)
		}
	}
	result
}

###############################################################

r.test <- function(x, degree = FALSE) {
	n <- length(x)
	if(degree)
		x <- ((x * pi)/180)
	ss <- sum(sin(x))
	cc <- sum(cos(x))
	rbar <- (sqrt(ss^2 + cc^2))/n
	z <- (n * rbar^2)
	p.value <- exp( - z)
	if(n < 50)
		temp <- (1 + (2 * z - z^2)/(4 * n) - (24 * z - 132 * z^2 + 76 * z^3 - 9 * z^4)/(288 * n^2))
	else temp <- 1
	result <- list(r.bar = rbar, p.value = p.value * temp)
	result
}

###############################################################

rad <- function(degree) {
	(degree * pi)/180
}

###############################################################

rao.homogeneity <- function(x, alpha = 0) {
	if(!is.list(x))
		stop("Data must be of mode list")
	n <- unlist(lapply(x, length))
	k <- length(x)
	c.data <- lapply(x, cos)
	s.data <- lapply(x, sin)
	x <- unlist(lapply(c.data, mean))
	y <- unlist(lapply(s.data, mean))
	s.co <- unlist(lapply(c.data, var))
	s.ss <- unlist(lapply(s.data, var))
	s.cs <- c(1:k)
	for(i in 1:k) {
		s.cs[i] <- var(c.data[[i]], s.data[[i]])
	}
	s.polar <- 1/n * (s.ss/x^2 + (y^2 * s.co)/x^4 - (2 * y * s.cs)/x^3)
	tan <- y/x
	H.polar <- sum(tan^2/s.polar) - (sum(tan/s.polar))^2/sum(1/s.polar)
	U <- x^2 + y^2
	s.disp <- 4/n * (x^2 * s.co + y^2 * s.ss + 2 * x * y * s.cs)
	H.disp <- sum(U^2/s.disp) - (sum(U/s.disp))^2/sum(1/s.disp)
	cat("\n")
	cat("Rao's Tests for Homogeneity", "\n")
	if(alpha == 0) {
		cat("\n")
		cat("       Test for E`quality of Polar Vectors:", "\n", "\n")
		cat("Test Statistic =", round(H.polar, 5), "\n")
		cat("Degrees of Freedom =", k - 1, "\n")
		cat("P-value of test =", round(1 - pchisq(H.polar, k - 1), 5), "\n", "\n")
		cat("       Test for Equality of Dispersions:", "\n", "\n")
		cat("Test Statistic =", round(H.disp, 5), "\n")
		cat("Degrees of Freedom =", k - 1, "\n")
		cat("P-value of test =", round(1 - pchisq(H.disp, k - 1), 5), "\n", "\n")
	} else {
		cat("\n")
		cat("       Test for Equality of Polar Vectors:", "\n", "\n")
		cat("Test Statistic =", round(H.polar, 5), "\n")
		cat("Degrees of Freedom =", k - 1, "\n")
		cat("Level", alpha, "critical value =", round(qchisq(1 - alpha, k - 1), 5), "\n")
		if(H.polar > qchisq(1 - alpha, k - 1)) {
			cat("Reject null hypothesis of equal polar vectors", "\n", "\n")
		} else { 
                        cat("Do not reject null hypothesis of equal polar vectors", "\n", "\n")
                }
		cat("       Test for Equality of Dispersions:", "\n", "\n")
		cat("Test Statistic =", round(H.disp, 5), "\n")
		cat("Degrees of Freedom =", k - 1, "\n")
		cat("Level", alpha, "critical value =", round(qchisq(1 - alpha, k - 1), 5), "\n")
		if(H.disp > qchisq(1 - alpha, k - 1)) {
			cat("Reject null hypothesis of equal dispersions", "\n", "\n")
		} else {
                        cat("Do not reject null hypothesis of equal dispersions", "\n", "\n")
                }
	}
}

###############################################################

rao.spacing <- function(x, alpha = 0, rad = TRUE) {
        rao.table <- NULL
        data(rao.table, package='CircStats', envir=sys.frame(which=sys.nframe()))
	if(rad == TRUE)
		x <- deg(x)
	x <- sort(x %% 360)
	n <- length(x)
	spacings <- c(diff(x), x[1] - x[n] + 360)
	U <- 1/2 * sum(abs(spacings - 360/n))
	if(n < 4)
		stop("Sample size too small")
	if(n <= 30)
		table.row <- n - 3
	else if(n <= 32)
		table.row <- 27
	else if(n <= 37)
		table.row <- 28
	else if(n <= 42)
		table.row <- 29
	else if(n <= 47)
		table.row <- 30
	else if(n <= 62)
		table.row <- 31
	else if(n <= 87)
		table.row <- 32
	else if(n <= 125)
		table.row <- 33
	else if(n <= 175)
		table.row <- 34
	else if(n <= 250)
		table.row <- 35
	else if(n <= 350)
		table.row <- 36
	else if(n <= 450)
		table.row <- 37
	else if(n <= 550)
		table.row <- 38
	else if(n <= 650)
		table.row <- 39
	else if(n <= 750)
		table.row <- 40
	else if(n <= 850)
		table.row <- 41
	else if(n <= 950)
		table.row <- 42
	else table.row <- 43
	if(alpha == 0) {
		cat("\n")
		cat("       Rao's Spacing Test of Uniformity", "\n", "\n")
		cat("Test Statistic =", round(U, 5), "\n")
		if(U > rao.table[table.row, 1])
			cat("P-value < 0.001", "\n", "\n")
		else if(U > rao.table[table.row, 2])
			cat("0.001 < P-value < 0.01", "\n", "\n")
		else if(U > rao.table[table.row, 3])
			cat("0.01 < P-value < 0.05", "\n", "\n")
		else if(U > rao.table[table.row, 4])
			cat("0.05 < P-value < 0.10", "\n", "\n")
		else cat("P-value > 0.10", "\n", "\n")
	}
	else {
		cat("\n")
		cat("       Rao's Spacing Test of Uniformity", "\n", "\n")
		cat("Test Statistic =", round(U, 5), "\n")
		if(sum(alpha == c(0.001, 0.01, 0.05, 0.1)) == 0)
			stop("Invalid significance level")
		table.col <- (1:4)[alpha == c(0.001, 0.01, 0.05, 0.1)]
		critical <- rao.table[table.row, table.col]
		cat("Level", alpha, "critical value =", critical, "\n")
		if(U > critical)
			cat("Reject null hypothesis of uniformity", "\n", 
				"\n")
		else cat("Do not reject null hypothesis of uniformity", "\n", 
				"\n")
	}
}

###############################################################

rcard <- function(n, mu, r) {
	i <- 1
	result <- c(1:n)
	while(i <= n) {
		x <- runif(1, 0, 2 * pi)
		y <- runif(1, 0, (1 + 2 * r)/(2 * pi))
		f <- (1 + 2 * r * cos(x - mu))/(2 * pi)
		if(y <= f) {
			result[i] <- x
			i <- i + 1
		}
	}
	result
}

###############################################################

rmixedvm <- function(n, mu1, mu2, kappa1, kappa2, p) {
	result <- c(1:n)
	for(i in 1:n) {
		test <- runif(1)
		if(test < p)
			result[i] <- rvm(1, mu1, kappa1)
		else result[i] <- rvm(1, mu2, kappa2)
	}
	result
}

###############################################################
## Modified March 5, 2002
## Modified December, 23, 2003

rose.diag <- function(x, bins, main = "", prop = 1, pts = FALSE, cex = 1, pch = 16, dotsep = 40, shrink = 1) {
	x <- x %% (2 * pi)
    if (require(MASS)) {
	   eqscplot(cos(seq(0, 2 * pi, length = 1000)), sin(seq(0, 2 * pi, length = 1000)), axes = FALSE, xlab = "", ylab = "", main = main, type = "l", xlim = shrink * c(-1, 1), ylim = shrink* c(-1, 1))
	lines(c(0, 0), c(0.9, 1))
	text(0.005, 0.85, "90", cex = 1.5)
	lines(c(0, 0), c(-0.9, -1))
	text(0.005, -0.825, "270", cex = 1.5)
	lines(c(-1, -0.9), c(0, 0))
	text(-0.8, 0, "180", cex = 1.5)
	lines(c(0.9, 1), c(0, 0))
	text(0.82, 0, "0", cex = 1.5)
	points(0, 0, cex = 1)
	n <- length(x)
	freq <- c(1:bins)
	arc <- (2 * pi)/bins
	for(i in 1:bins) {
	    freq[i] <- sum(x <= i * arc & x > (i - 1) * arc)
	}
	rel.freq <- freq/n
	radius <- sqrt(rel.freq) * prop
	sector <- seq(0, 2 * pi - (2 * pi)/bins, length = bins)
	mids <- seq(arc/2, 2 * pi - pi/bins, length = bins)
	index <- cex/dotsep
	for(i in 1:bins) {
		if(rel.freq[i] != 0) {
			lines(c(0, radius[i] * cos(sector[i])), c(0, radius[i] * sin(sector[i])))
			lines(c(0, radius[i] * cos(sector[i] + (2 * pi)/bins)), c(0, radius[i] * sin(sector[i] + (2 * pi)/bins)))
			lines(c(radius[i] * cos(sector[i]), radius[i] * cos(sector[i] + (2 * pi)/bins)), c(radius[i] * sin(sector[i]), radius[i] * sin(sector[i] + (2 * pi)/bins)))
			if(pts == TRUE) {
				for(j in 0:(freq[i] - 1)) {
				  r <- 1 + j * index
				  x <- r * cos(mids[i])
				  y <- r * sin(mids[i])
				  points(x, y, cex = cex, pch = pch)
				}
			}
		}
	}

 } else {
    stop("To use this function you have to install the package MASS (VR)\n")
 }
}

###############################################################

rtri <- function(n, r) {
	u <- matrix(c(runif(n)), ncol = 1)
	get.theta <- function(u, r)
	{
		if(u < 0.5) {
			a <- pi * r
			b <-  - (4 + pi^2 * r)
			c <- 8 * pi * u
			theta1 <- ( - b + sqrt(b^2 - 4 * a * c))/(2 * a)
			theta2 <- ( - b - sqrt(b^2 - 4 * a * c))/(2 * a)
			min(theta1, theta2)
		}
		else {
			a <- pi * r
			b <- 4 - 3 * pi^2 * r
			c <- (2 * pi^3 * r) - (8 * pi * u)
			theta1 <- ( - b + sqrt(b^2 - 4 * a * c))/(2 * a)
			theta2 <- ( - b - sqrt(b^2 - 4 * a * c))/(2 * a)
			max(theta1, theta2)
		}
	}
	theta <- apply(u, 1, get.theta, r)
	theta[theta > pi] <- theta[theta > pi] - 2 * pi
	theta
}

###############################################################

rvm <- function(n, mean, k) {
	vm <- c(1:n)
	a <- 1 + (1 + 4 * (k^2))^0.5
	b <- (a - (2 * a)^0.5)/(2 * k)
	r <- (1 + b^2)/(2 * b)
	obs <- 1
	while(obs <= n) {
		U1 <- runif(1, 0, 1)
		z <- cos(pi * U1)
		f <- (1 + r * z)/(r + z)
		c <- k * (r - f)
		U2 <- runif(1, 0, 1)
		if(c * (2 - c) - U2 > 0) {
			U3 <- runif(1, 0, 1)
			vm[obs] <- sign(U3 - 0.5) * acos(f) + mean
			vm[obs] <- vm[obs] %% (2 * pi)
			obs <- obs + 1
		}
		else {
			if(log(c/U2) + 1 - c >= 0) {
				U3 <- runif(1, 0, 1)
				vm[obs] <- sign(U3 - 0.5) * acos(f) + mean
				vm[obs] <- vm[obs] %% (2 * pi)
				obs <- obs + 1
			}
		}
	}
	vm
}

###############################################################

rwrpcauchy <- function(n, location = 0, rho = exp(-1)) {
	if(rho == 0)
		result <- runif(n, 0, 2 * pi)
	else if(rho == 1)
		result <- rep(location, n)
	else {
		scale <-  - log(rho)
		result <- rcauchy(n, location, scale) %% (2 * pi)
	}
	result
}

###############################################################
# Modified December 31, 2002
# aggiunto parametro sd
# controllo sul valore di rho

rwrpnorm <- function(n, mu, rho, sd=1) {
        if (missing(rho)) {
            rho <- exp(-sd^2/2)
        }
        if (rho < 0 | rho > 1)
            stop("rho must be between 0 and 1")        
	if (rho == 0)
		result <- runif(n, 0, 2 * pi)
	else if (rho == 1)
		result <- rep(mu, n)
	else {
		sd <- sqrt(-2 * log(rho))
		result <- rnorm(n, mu, sd) %% (2 * pi)
	}
	result
}

###############################################################

rwrpstab <- function(n, index, skewness, scale=1) {
	rstable(n, index, skewness, scale=scale) %% (2 * pi)
}

###############################################################
# Modified December 6, 2005

trig.moment <- function(x, p = 1, center = FALSE) {
	n <- length(x)
	sinr <- sum(sin(x))
	cosr <- sum(cos(x))
	circmean <- atan2(sinr, cosr)
	sin.p <- sum(sin(p * (x - circmean * center)))/n
	cos.p <- sum(cos(p * (x - circmean * center)))/n
	mu.p <- atan2(sin.p, cos.p)
	rho.p <- sqrt(sin.p^2 + cos.p^2)
	data.frame(mu.p, rho.p, cos.p, sin.p)
}

###############################################################

v0.test <- function(x, mu0 = 0, degree = FALSE) {
	n <- length(x)
	if(degree) {
		x <- ((x * pi)/180)
		mu0 <- ((mu0 * pi)/180)
	}
	r0.bar <- (sum(cos(x - mu0)))/n
	z0 <- sqrt(2 * n) * r0.bar
	pz <- pnorm(z0)
	fz <- dnorm(z0)
	p.value <- 1 - pz + fz * ((3 * z0 - z0^3)/(16 * n) + (15 * z0 + 305 * z0^3 - 125 * z0^5 + 9 * z0^7)/(4608 * n^2))
	result <- list(r0.bar = r0.bar, p.value = p.value)
	result
}

###############################################################
# Modified April 17, 2003

vm.bootstrap.ci <- function(x, bias = FALSE, alpha = 0.05, reps = 1000, print = TRUE) {

    if (require(boot)) {
        circ.mean.local <- function(x, i) {
            circ.mean(x[i])
        }
	mean.bs <- boot(data = x, statistic = circ.mean.local, R = reps, stype="i")
#	mean.bs <- bootstrap(x=x, theta=circ.mean, nboot = reps)
	mean.reps <- mean.bs$t
	mean.reps <- sort(mean.reps %% (2 * pi))
	B <- reps
	spacings <- c(diff(mean.reps), mean.reps[1] - mean.reps[B] + 2 * pi)
	max.spacing <- (1:B)[spacings == max(spacings)]
	off.set <- 2 * pi - mean.reps[max.spacing + 1]
	if(max.spacing != B)
		mean.reps2 <- mean.reps + off.set
	else mean.reps2 <- mean.reps
	mean.reps2 <- sort(mean.reps2 %% (2 * pi))
	mean.ci <- quantile(mean.reps2, c(alpha/2, 1 - alpha/2))
	if(max.spacing != B)
		mean.ci <- mean.ci - off.set
        est.kappa.local <- function(x, i, bias) {
            est.kappa(x[i], bias=bias)
        }   

	kappa.bs <- boot(data = x, statistic = est.kappa.local, R = reps, stype="i", bias = bias)
	kappa.reps <- kappa.bs$t

	kappa.ci <- quantile(kappa.reps, c(alpha/2, 1 - alpha/2))
	result <- list(mean.ci, kappa.ci, c(mean.reps), c(kappa.reps))
	names(result) <- c("mu.ci", "kappa.ci", "mu.reps", "kappa.reps")
	cat("Bootstrap Confidence Intervals for Mean Direction and Concentration", "\n")
	cat("Confidence Level:  ", 100 * (1 - alpha), "%", "\n")
	cat("Mean Direction:           ", "Low =", round(mean.ci[1], 2), "  High =", round(mean.ci[2], 2), "\n")
	cat("Concentration Parameter:  ", "Low =", round(kappa.ci[1], 2), "  High =", round(kappa.ci[2], 2), "\n")
	result

        } else {
             stop("To use this function you have to install the package bootstrap \n")
        }

}

###############################################################

vm.ml <- function(x, bias = FALSE) {
	mu <- circ.mean(x)
	kappa <- A1inv(mean(cos(x - mu)))
	if(bias == TRUE) {
		kappa.ml <- kappa
		n <- length(x)
		if(kappa.ml < 2)
			kappa <- max(kappa.ml - 2 * (n * kappa.ml)^-1, 0)
		if(kappa.ml >= 2)
			kappa <- ((n - 1)^3 * kappa.ml)/(n^3 + n)
	}
	data.frame(mu, kappa)
}

###############################################################

watson <- function(x, alpha = 0, dist = "uniform") {
	if(dist == "uniform") {
		cat("\n", "      Watson's Test for Circular Uniformity", "\n", "\n")
		n <- length(x)
		u <- sort(x)/(2 * pi)
		u.bar <- mean(u)
		i <- seq(1:n)
		sum.terms <- (u - u.bar - (2 * i - 1)/(2 * n) + 0.5)^2
		u2 <- sum(sum.terms) + 1/(12 * n)
		u2 <- (u2 - 0.1/n + 0.1/(n^2)) * (1 + 0.8/n)
		crits <- c(99, 0.267, 0.221, 0.187, 0.152)
		if(n < 8) {
			cat("Total Sample Size < 8:  Results are not valid", "\n", "\n")
		}
		cat("Test Statistic:", round(u2, 4), "\n")
		if(sum(alpha == c(0, 0.01, 0.025, 0.05, 0.1)) == 0)
			stop("Invalid input for alpha")
		if(alpha == 0) {
			if(u2 > 0.267)
				cat("P-value < 0.01", "\n", "\n")
			else if(u2 > 0.221)
				cat("0.01 < P-value < 0.025", "\n", "\n")
			else if(u2 > 0.187)
				cat("0.025 < P-value < 0.05", "\n", "\n")
			else if(u2 > 0.152)
				cat("0.05 < P-value < 0.10", "\n", "\n")
			else cat("P-value > 0.10", "\n", "\n")
		}
		else {
			index <- (1:5)[alpha == c(0, 0.01, 0.025, 0.05, 0.1)]
			Critical <- crits[index]
			if(u2 > Critical)
				Reject <- "Reject Null Hypothesis"
			else Reject <- "Do Not Reject Null Hypothesis"
			cat("Level", alpha, "Critical Value:", round(Critical, 4), "\n")
			cat(Reject, "\n", "\n")
		}
	}
	else if(dist == "vm") {
		cat("\n", "      Watson's Test for the von Mises Distribution"
, "\n", "\n")
		u2.crits <- cbind(c(0, 0.5, 1, 1.5, 2, 4, 100), c(0.052, 
			0.056, 0.066, 0.077, 0.084, 0.093, 0.096), c(0.061, 
			0.066, 0.079, 0.092, 0.101, 0.113, 0.117), c(0.081, 
			0.09, 0.11, 0.128, 0.142, 0.158, 0.164))
		n <- length(x)
		mu.hat <- circ.mean(x)
		kappa.hat <- est.kappa(x)
		x <- (x - mu.hat) %% (2 * pi)
		x <- matrix(x, ncol = 1)
		z <- apply(x, 1, pvm, 0, kappa.hat)
		z <- sort(z)
		z.bar <- mean(z)
		i <- c(1:n)
		sum.terms <- (z - (2 * i - 1)/(2 * n))^2
		Value <- sum(sum.terms) - n * (z.bar - 0.5)^2 + 1/(12 * n)
		if(kappa.hat < 0.25)
			row <- 1
		else if(kappa.hat < 0.75)
			row <- 2
		else if(kappa.hat < 1.25)
			row <- 3
		else if(kappa.hat < 1.75)
			row <- 4
		else if(kappa.hat < 3)
			row <- 5
		else if(kappa.hat < 5)
			row <- 6
		else row <- 7
		if(alpha != 0) {
			if(alpha == 0.1)
				col <- 2
			else if(alpha == 0.05)
				col <- 3
			else if(alpha == 0.01)
				col <- 4
			else {
				cat("Invalid input for alpha", "\n", "\n")
				break
			}
			Critical <- u2.crits[row, col]
			if(Value > Critical)
				Reject <- "Reject Null Hypothesis"
			else Reject <- "Do Not Reject Null Hypothesis"
			cat("Test Statistic:", round(Value, 4), "\n")
			cat("Level", alpha, "Critical Value:", round(Critical, 4), "\n")
			cat(Reject, "\n", "\n")
		}
		else {
			cat("Test Statistic:", round(Value, 4), "\n")
			if(Value < u2.crits[row, 2])
				cat("P-value > 0.10", "\n", "\n")
			else if((Value >= u2.crits[row, 2]) && (Value < 
				u2.crits[row, 3]))
				cat("0.05 < P-value > 0.10", "\n", "\n")
			else if((Value >= u2.crits[row, 3]) && (Value < u2.crits[row, 4]))
				cat("0.01 < P-value > 0.05", "\n", "\n")
			else cat("P-value < 0.01", "\n", "\n")
		}
	}
	else stop("Distribution must be either uniform or von Mises")
}

###############################################################

watson.two <- function(x, y, alpha = 0, plot = FALSE) {
	n1 <- length(x)
	n2 <- length(y)
	n <- n1 + n2
	if(n < 18)
		cat("Total Sample Size < 18:  Consult tabulated critical values", "\n", "\n")
	if(plot == TRUE) {
		x <- sort(x %% (2 * pi))
		y <- sort(y %% (2 * pi))
		plot.edf(x, main = "Comparison of Empirical CDFs", xlab= "", ylab = "")
		par(new = TRUE)
		plot.edf(y, xlab = "", ylab = "", axes = FALSE, lty = 2)
	}
	cat("\n", "      Watson's Two-Sample Test of Homogeneity", "\n", "\n")
	x <- cbind(sort(x %% (2 * pi)), rep(1, n1))
	y <- cbind(sort(y %% (2 * pi)), rep(2, n2))
	xx <- rbind(x, y)
	rank <- order(xx[, 1])
	xx <- cbind(xx[rank,  ], seq(1:n))
	a <- c(1:n)
	b <- c(1:n)
	for(i in 1:n) {
		a[i] <- sum(xx[1:i, 2] == 1)
		b[i] <- sum(xx[1:i, 2] == 2)
	}
	d <- b/n2 - a/n1
	dbar <- mean(d)
	u2 <- (n1 * n2)/n^2 * sum((d - dbar)^2)
	crits <- c(99, 0.385, 0.268, 0.187, 0.152)
	cat("Test Statistic:", round(u2, 4), "\n")
	if(sum(alpha == c(0, 0.001, 0.01, 0.05, 0.1)) == 0)
		stop("Invalid input for alpha")
	else if(alpha == 0) {
		if(u2 > 0.385)
			cat("P-value < 0.001", "\n", "\n")
		else if(u2 > 0.268)
			cat("0.001 < P-value < 0.01", "\n", "\n")
		else if(u2 > 0.187)
			cat("0.01 < P-value < 0.05", "\n", "\n")
		else if(u2 > 0.152)
			cat("0.05 < P-value < 0.10", "\n", "\n")
		else cat("P-value > 0.10", "\n", "\n")
	}
	else {
		index <- (1:5)[alpha == c(0, 0.001, 0.01, 0.05, 0.1)]
		Critical <- crits[index]
		if(u2 > Critical)
			Reject <- "Reject Null Hypothesis"
		else Reject <- "Do Not Reject Null Hypothesis"
		cat("Level", alpha, "Critical Value:", round(Critical, 4), 
			"\n")
		cat(Reject, "\n", "\n")
	}
}

###############################################################
# Modified December 6, 2005

wrpcauchy.ml <- function(x, mu0, rho0, acc = 1e-015) {
	mu1.old <- (2 * rho0 * cos(mu0))/(1 + rho0^2)
	mu2.old <- (2 * rho0 * sin(mu0))/(1 + rho0^2)
	w.old <- 1/(1 - mu1.old * cos(x) - mu2.old * sin(x))
	flag <- 0
	while(flag == 0) {
		mu1.new <- sum(w.old * cos(x))/sum(w.old)
		mu2.new <- sum(w.old * sin(x))/sum(w.old)
		diff1 <- abs(mu1.new - mu1.old)
		diff2 <- abs(mu2.new - mu2.old)
		if((diff1 < acc) && (diff2 < acc))
			flag <- 1
		else {
			mu1.old <- mu1.new
			mu2.old <- mu2.new
			w.old <- 1/(1 - mu1.old * cos(x) - mu2.old * sin(
				x))
		}
	}
	mu.const <- sqrt(mu1.new^2 + mu2.new^2)
	rho <- (1 - sqrt(1 - mu.const^2))/mu.const
	mu <- atan2(mu2.new, mu1.new) %% (2 * pi)
	data.frame(mu, rho)
}

###############################################################

rvm <- function(n, mean, k) {
	vm <- c(1:n)
	a <- 1 + (1 + 4 * (k^2))^0.5
	b <- (a - (2 * a)^0.5)/(2 * k)
	r <- (1 + b^2)/(2 * b)
	obs <- 1
	while(obs <= n) {
		U1 <- runif(1, 0, 1)
		z <- cos(pi * U1)
		f <- (1 + r * z)/(r + z)
		c <- k * (r - f)
		U2 <- runif(1, 0, 1)
		if(c * (2 - c) - U2 > 0) {
			U3 <- runif(1, 0, 1)
			vm[obs] <- sign(U3 - 0.5) * acos(f) + mean
			vm[obs] <- vm[obs] %% (2 * pi)
			obs <- obs + 1
		}
		else {
			if(log(c/U2) + 1 - c >= 0) {
				U3 <- runif(1, 0, 1)
				vm[obs] <- sign(U3 - 0.5) * acos(f) + mean
				vm[obs] <- vm[obs] %% (2 * pi)
				obs <- obs + 1
			}
		}
	}
	vm
}

###############################################################

nCk <- function(n, k) {
	result <- exp(log(gamma(n + 1)) - log(gamma(k + 1)) - log(gamma(n - k 
+ 1)))
	result
}

###############################################################



