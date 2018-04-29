#ifdef _DEBUG

class	TimeProfile
{
public:
	LARGE_INTEGER	m_Frequency;	// ticks per second
	LARGE_INTEGER	m_StartTime;
	LARGE_INTEGER	m_StopTime;
	double*			m_pResult;

public:
	// This is the scoped profiler version
	// It will start measuring immediately and assign _Result in the destructor
	// This is useful to write something like:
	//
	//	double	ElapsedTime;
	//	{	// Scope start
	//		TimerProfile( ElapsedTime );
	//
	//		(...)	// Do something
	//
	//	}	// Scope end => ElapsedTime contains scope duration
	//
	TimeProfile( double& _Result );

	// Manual profiling
	TimeProfile();

	~TimeProfile();

	void	Start();
	double	Stop();
};

#endif