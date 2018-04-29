#ifdef _DEBUG

#include "../GodComplex.h"

TimeProfile::TimeProfile() : m_pResult( NULL )
{
	QueryPerformanceFrequency( &m_Frequency );
}

TimeProfile::TimeProfile( double& _Result ) : m_pResult( &_Result )
{
	QueryPerformanceFrequency( &m_Frequency );
	Start();
}
TimeProfile::~TimeProfile()
{
	if ( m_pResult != NULL )
		*m_pResult = Stop();
}

void	TimeProfile::Start()
{
	QueryPerformanceCounter( &m_StartTime );
}
double	TimeProfile::Stop()
{
	QueryPerformanceCounter( &m_StopTime );

	// compute and print the elapsed time in millisec
	double	ElapsedTime = (m_StopTime.QuadPart - m_StartTime.QuadPart) * 1000.0 / m_Frequency.QuadPart;
	return ElapsedTime;
}

#endif