using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Microsoft.Win32;

using RendererManaged;

namespace TestMSBSDF
{
	public partial class AutomationForm : Form
	{
		#region NESTED TYPES

		class CanceledException : Exception {}

		/// <summary>
		/// The main automation document class
		/// Contains automations settings and results
		/// </summary>
		class	Document {

			#region NESTED TYPES

			public delegate void	SettingsChangedEventHandler();
			public delegate void	ResultStateChangedEventHandler( Result _result );

			/// <summary>
			/// Contains the surface parameters that guide the automation
			/// These settings have a "locked state" that throws an exception if they are
			///  modified after they are locked, this is to ensure a current set of results
			///  doesn't see its dimensions change once the automation has begun...
			/// </summary>
			public class	SurfaceParameters {

				#region NESTED TYPES

				public delegate void	LockStateChangedEventHandler();
				public delegate void	ScatteringOrdersCountChangedEventHandler();

				public class	Parameter {

					public delegate	void	ValueChangedEventHandler( Parameter _P );

					SurfaceParameters	m_owner;

					float				m_rangeMin = 0.0f;
					float				m_rangeMax = 1.0f;

					float				m_min = 0.0f;
					float				m_max = 1.0f;
					int					m_stepsCount = 1;
					bool				m_inclusiveMin = true;
					bool				m_inclusiveMax = false;

					public float	Min	{
						get { return m_min; }
						set {
							if ( m_owner.m_locked )
								throw new Exception( "Parameter space settings are locked and can't be changed anymore!" );

							value = Math.Max( m_rangeMin, Math.Min( m_rangeMax, value ) );
							if ( value == m_min )
								return;

							m_min = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public float	Max	{
						get { return m_max; }
						set {
							if ( m_owner.m_locked )
								throw new Exception( "Parameter space settings are locked and can't be changed anymore!" );

							value = Math.Max( m_rangeMin, Math.Min( m_rangeMax, value ) );
							if ( value == m_max )
								return;

							m_max = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public int		StepsCount	{
						get { return m_stepsCount; }
						set {
							if ( m_owner.m_locked )
								throw new Exception( "Parameter space settings are locked and can't be changed anymore!" );

							value = Math.Max( 1, value );
							if ( value == m_stepsCount )
								return;

							m_stepsCount = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public bool		InclusiveMin	{
						get { return m_inclusiveMin; }
						set {
							if ( m_owner.m_locked )
								throw new Exception( "Parameter space settings are locked and can't be changed anymore!" );

							if ( value == m_inclusiveMin )
								return;

							m_inclusiveMin = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public bool		InclusiveMax	{
						get { return m_inclusiveMax; }
						set {
							if ( m_owner.m_locked )
								throw new Exception( "Parameter space settings are locked and can't be changed anymore!" );

							if ( value == m_inclusiveMax )
								return;

							m_inclusiveMax = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public event ValueChangedEventHandler	ValueChanged;

					public Parameter( SurfaceParameters _owner, float _rangeMin, float _rangeMax, int _stepsCount ) {
						m_owner = _owner;
						m_rangeMin = _rangeMin;
						m_min = m_rangeMin;
						m_rangeMax = _rangeMax;
						m_max = m_rangeMax;
						m_stepsCount = _stepsCount;
					}

					/// <summary>
					/// Builds the min/step values to correctly interpolate that parameter using a loop going from 0 to stepsCount-1
					/// </summary>
					/// <param name="_min"></param>
					/// <param name="_step"></param>
					public void		BuildMinStep( out float _min, out float _step ) {
						_step = m_max - m_min;
						if ( !m_inclusiveMin && !m_inclusiveMax ) {
							_step /= m_stepsCount+1;						// 0 => min+step, stepsCount-1 => max-step
						} else if ( m_inclusiveMin && m_inclusiveMax ) {
							_step /= m_stepsCount > 1 ? m_stepsCount-1 : 1;	// 0 => min, stepsCount-1 => max
						} else {
							_step /= m_stepsCount;							// 0 => min, stepsCount-1 => max-step
						}
						_min = m_min + (m_inclusiveMin ? 0 : _step);
					}

					public void		Save( XmlElement _parent ) {
						_parent.SetAttribute( "Min", m_min.ToString() );
						_parent.SetAttribute( "Max", m_max.ToString() );
						_parent.SetAttribute( "Steps", m_stepsCount.ToString() );
						_parent.SetAttribute( "InclusiveMin", m_inclusiveMin.ToString() );
						_parent.SetAttribute( "InclusiveMax", m_inclusiveMax.ToString() );
					}

					public void		Load( XmlElement _parent ) {
						float.TryParse( _parent.GetAttribute( "Min" ), out m_min );
						float.TryParse( _parent.GetAttribute( "Max" ), out m_max );
						int.TryParse( _parent.GetAttribute( "Steps" ), out m_stepsCount );
						bool.TryParse( _parent.GetAttribute( "InclusiveMin" ), out m_inclusiveMin );
						bool.TryParse( _parent.GetAttribute( "InclusiveMax" ), out m_inclusiveMax );
					}
				}

				#endregion

				public int						m_rayTracingIterationsCount = 1024;
				public TestForm.SURFACE_TYPE	m_type = TestForm.SURFACE_TYPE.CONDUCTOR;
				private bool					m_locked = false;	// Once computations have started, we can't touch the settings anymore!
				public Parameter				m_incomingAngle = null;
				public Parameter				m_roughness = null;
				public Parameter				m_albedoF0 = null;
				private Parameter				m_scatteringOrders = null;	// Actually an integer parameter, must be accessed through properties below

				public event LockStateChangedEventHandler				LockStateChanged;
				public event ScatteringOrdersCountChangedEventHandler	ScatteringOrdersCountChanged;

				public bool			IsLocked {
					get { return m_locked; }
				}

				public int			ScatteringOrderMin {
					get { return (int) m_scatteringOrders.Min; }
					set { m_scatteringOrders.Min = value; }
				}

				public int			ScatteringOrderMax {
					get { return (int) m_scatteringOrders.Max; }
					set { m_scatteringOrders.Max = value; }
				}

				public int			ScatteringOrdersCount {
					get { return 1 + ScatteringOrderMax - ScatteringOrderMin; }
				}

				public	SurfaceParameters() {
					m_incomingAngle = new Parameter( this, 0, 90.0f, 20 );
					m_roughness = new Parameter( this, 0, 1.0f, 10 );
					m_albedoF0 = new Parameter( this, 0, 1.0f, 4 );
					m_scatteringOrders = new Parameter( this, 1, 4, 1 );
					m_scatteringOrders.ValueChanged += m_scatteringOrders_ValueChanged;

					m_roughness.Min = 1.0f;
					m_roughness.Max = 0.0f;
					m_roughness.InclusiveMax = true;	// Roughness must be simulated all the way!
					m_albedoF0.Min = 1.0f;
					m_albedoF0.Max = 0.0f;
				}

				public void		Lock() {
					if ( m_locked )
						throw new Exception( "Surface is already locked!" );

					m_locked = true;

					if ( LockStateChanged != null )
						LockStateChanged();
				}

				public void		Save( XmlElement _parent ) {
					Attrib( _parent, "Surface Type", m_type );
					Attrib( _parent, "RayTrace Iterations", m_rayTracingIterationsCount );
					Attrib( _parent, "Locked", m_locked );

					XmlElement ElemParm0 = AppendChild( _parent, "IncomingAngle" );
					m_incomingAngle.Save( ElemParm0 );

					XmlElement ElemParm1 = AppendChild( _parent, "SurfaceRoughness" );
					m_roughness.Save( ElemParm1 );

					XmlElement ElemParm2 = AppendChild( _parent, "AlbedoF0" );
					m_albedoF0.Save( ElemParm2 );

					XmlElement ElemParm3 = AppendChild( _parent, "IncomingAngle" );
					m_scatteringOrders.Save( ElemParm3 );
				}

				public void		Load( XmlElement _parent ) {

				}

				// Simple forward
				void m_scatteringOrders_ValueChanged( SurfaceParameters.Parameter _P ) {
					if ( ScatteringOrdersCountChanged != null )
						ScatteringOrdersCountChanged();
				}
			}

			/// <summary>
			/// This class contains the simulation settings
			/// Although discouraged, they can be changed even when the simulation has already begun
			/// </summary>
			public class	Settings {

				public enum GUESS_INITIAL_DIRECTION {
					CENTER_OF_MASS,
					REFLECTED_DIRECTION,
					NO_CHANGE,				// Means no change from last computation
				}

				public enum GUESS_INITIAL_ROUGHNESS {
					SURFACE,
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
				}

				public enum GUESS_INITIAL_SCALE {
					FACTOR_CENTER_OF_MASS,
					NO_CHANGE,				// Means no change from last computation
				}

				public enum GUESS_INITIAL_FLATTEN {
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
				}

				public enum GUESS_INITIAL_MASKING {
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
				}

				// Fitter parameters
				public int						m_maxIterations = 200;
				public float					m_logTolerance_Minimum = -6.0f;
				public float					m_logTolerance_Gradient = -6.0f;
				public int						m_maxRetries = 2;
				public float					m_oversizeFactor = 1.0f;

				// Lobe parameters
				public LobeModel.LOBE_TYPE		m_lobeModel = LobeModel.LOBE_TYPE.MODIFIED_PHONG;
				public GUESS_INITIAL_DIRECTION	m_initialDirection = GUESS_INITIAL_DIRECTION.CENTER_OF_MASS;
				public bool						m_inheritDirection = true;
				public GUESS_INITIAL_ROUGHNESS	m_initialRoughness = GUESS_INITIAL_ROUGHNESS.SURFACE;
				public bool						m_inheritRoughness = true;
				public float					m_customRoughness = 0.8f;
				public GUESS_INITIAL_SCALE		m_initialScale = GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS;
				public bool						m_inheritScale = true;
				public float					m_customScale = 0.05f;
				public GUESS_INITIAL_FLATTEN	m_initialFlatten = GUESS_INITIAL_FLATTEN.CUSTOM;
				public bool						m_inheritFlatten = true;
				public float					m_customFlatten = 0.5f;
				public GUESS_INITIAL_MASKING	m_initialMasking = GUESS_INITIAL_MASKING.CUSTOM;
				public bool						m_inheritMasking = true;
				public float					m_customMasking = 1.0f;


				public	Settings() {
				}

				public void		Save( XmlElement _parent ) {

					// Fitter parameters
					XmlElement	ElemFitterParms = AppendChild( _parent, "FitterParameters" );
					_parent.AppendChild( ElemFitterParms );

					Attrib( ElemFitterParms, "MaxIterations", m_maxIterations );
					Attrib( ElemFitterParms, "logToleranceMinimum", m_logTolerance_Minimum );
					Attrib( ElemFitterParms, "logToleranceGradient", m_logTolerance_Gradient );
					Attrib( ElemFitterParms, "MaxRetries", m_maxRetries );
					Attrib( ElemFitterParms, "OversizeFactor", m_oversizeFactor );

					// Lobe parameters
					XmlElement	ElemLobeParms = AppendChild( _parent, "LobeParameters" );

					Attrib( ElemLobeParms, "Model", m_lobeModel );
					Attrib( ElemLobeParms, "initialDirection",	m_initialDirection );
					Attrib( ElemLobeParms, "inheritDirection",	m_inheritDirection );
					Attrib( ElemLobeParms, "initialRoughness",	m_initialRoughness );
					Attrib( ElemLobeParms, "inheritRoughness",	m_inheritRoughness );
					Attrib( ElemLobeParms, "customRoughness",	m_customRoughness );
					Attrib( ElemLobeParms, "initialScale",		m_initialScale );
					Attrib( ElemLobeParms, "inheritScale",		m_inheritScale );
					Attrib( ElemLobeParms, "customScale",		m_customScale );
					Attrib( ElemLobeParms, "initialFlatten",	m_initialFlatten );
					Attrib( ElemLobeParms, "inheritFlatten",	m_inheritFlatten );
					Attrib( ElemLobeParms, "customFlatten",		m_customFlatten );
					Attrib( ElemLobeParms, "initialMasking",	m_initialMasking );
					Attrib( ElemLobeParms, "inheritMasking",	m_inheritMasking );
					Attrib( ElemLobeParms, "customMasking",		m_customMasking );
				}

				public void		Load( XmlElement _parent ) {

				}
			}

			public class	Result {
				#region NESTED TYPES

				public class	LobeParameters {
					public double	m_theta = -1.0;		// -1 means invalid
					public double	m_roughness = 0.8;
					public double	m_scale = 0.1;
					public double	m_flatten = 0.5;
					public double	m_masking = 1.0;

					/// <summary>
					/// Tells if the parameters are valid
					/// </summary>
					public bool		IsValid {
						get { return m_theta >= 0.0; }
					}

					public double[]	AsArray {
						get { return new double[] {	m_theta,
													m_roughness,
													m_scale,
													m_flatten,
													m_masking
												  };
						}
					}

					/// <summary>
					/// Initializes the lobe results based on current settings
					/// </summary>
					/// <param name="_owner">Our owner result</param>
					/// <param name="_lobe"></param>
					/// <param name="_reflectedDirection"></param>
					public void		Initialize( Result _owner, LobeModel _lobe, float3 _reflectedDirection ) {
						Settings		S = _owner.m_owner.m_settings;

						bool			reflected = this == _owner.m_reflected;	// Are we the reflected or refracted lobe result?
						LobeParameters	previousParams = null;
						if ( _owner.m_Y > 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[_owner.m_X, _owner.m_Y-1, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						}

						//////////////////////////////////////////////////////////////////////////
						// Initialize theta
						if ( S.m_inheritDirection && previousParams != null ) {
							// Re-use last fitted direction with the same angle of incidence but different roughness
							m_theta = previousParams.m_theta;
						} else {
							switch ( S.m_initialDirection ) {
								case Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS: {
									// Override theta to use the direction of the center of mass
									// (it's quite intuitive to start by aligning our lobe along the main simulated lobe direction!)
									float3	towardCenterOfMass = _lobe.CenterOfMass.Normalized;
									m_theta = (float) Math.Acos( towardCenterOfMass.z );
									break;
								}
								case Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION:
									m_theta = Math.Acos( _reflectedDirection.z );
									break;
							}
						}

						//////////////////////////////////////////////////////////////////////////
						// Initialize roughness
						if ( S.m_inheritRoughness && previousParams != null ) {
							// Re-use last fitted roughness with the same angle of incidence but different roughness
							m_roughness = previousParams.m_roughness;
						} else {
							switch ( S.m_initialRoughness ) {
								case Settings.GUESS_INITIAL_ROUGHNESS.SURFACE:
									m_roughness = _owner.m_surfaceRoughness;
									break;
								case Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM:
									m_roughness = S.m_customRoughness;
									break;
							}
						}

						//////////////////////////////////////////////////////////////////////////
						// Initialize scale
						if ( S.m_inheritScale && previousParams != null ) {
							// Re-use last fitted scale with the same angle of incidence but different roughness
							m_scale = previousParams.m_scale;
						} else {
							switch ( S.m_initialScale ) {
								case Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS: {
									m_scale = _lobe.CenterOfMass.Length;
									m_scale *= S.m_customScale;	// In fact it's better to have a very small scale as I realized the algorithm converged much faster starting from a very small lobe!!
																// (~20 iterations compared to 200 otherwise, because the gradient leads the algorithm in the wrong direction too fast and it takes hell
																//	of a time to get back on tracks afterwards if we start from too large a lobe!)
									break;
								}
							}
						}

						//////////////////////////////////////////////////////////////////////////
						// Initialize flattening factor
						if ( S.m_inheritFlatten && previousParams != null ) {
							// Re-use last fitted flatten with the same angle of incidence but different roughness
							m_flatten = previousParams.m_flatten;
						} else {
							switch ( S.m_initialFlatten ) {
								case Settings.GUESS_INITIAL_FLATTEN.CUSTOM:
									m_flatten = S.m_customFlatten;
									break;
							}
						}

						//////////////////////////////////////////////////////////////////////////
						// Initialize masking importance
						if ( S.m_inheritMasking && previousParams != null ) {
							// Re-use last fitted masking with the same angle of incidence but different roughness
							m_masking = previousParams.m_masking;
						} else {
							switch ( S.m_initialMasking ) {
								case Settings.GUESS_INITIAL_MASKING.CUSTOM:
									m_masking = S.m_customMasking;
									break;
							}
						}
					}

					public void		Save( XmlElement _parent ) {
						Attrib( _parent, "theta", m_theta );
						Attrib( _parent, "roughness", m_roughness );
						Attrib( _parent, "scale", m_scale );
						Attrib( _parent, "flatten", m_flatten );
						Attrib( _parent, "masking", m_masking );
					}

					public void		Load( XmlElement _parent ) {

					}
				}

				#endregion

				Document		m_owner = null;
				int				m_order;
				int				m_X;
				int				m_Y;
				int				m_Z;
				float			m_state = 0.0f;		// Not computed yet!
				public string	m_error = null;		// No error yet...

				// Input parameters
				float			m_incomingAngleTheta;
				float			m_incomingAnglePhi;
				float			m_surfaceRoughness;
				float			m_surfaceAlbedoF0;

				// Fitted parameters
				public LobeParameters		m_reflected = new LobeParameters();
				public LobeParameters		m_refracted = new LobeParameters();

				public int		ScatteringOrder		{ get { return m_order; } }
				public int		X					{ get { return m_X; } }
				public int		Y					{ get { return m_Y; } }
				public int		Z					{ get { return m_Z; } }

				public float	IncomingAngleTheta	{ get { return m_incomingAngleTheta; } }
				public float	IncomingAnglePhi	{ get { return m_incomingAnglePhi; } }
				public float	SurfaceRoughness	{ get { return m_surfaceRoughness; } }
				public float	SurfaceAlbedoF0		{ get { return m_surfaceAlbedoF0; } }

				public float	State {
					get { return m_state; }
					set {
						if ( value == m_state )
							return;

						m_state = value;
						m_owner.ResultStateChanged( this );
					}
				}

				/// <summary>
				/// Gets the incoming direction, pointing toward the surface
				/// </summary>
				public float3	IncomingDirection {
					get {
						double	cosTheta = Math.Cos( m_incomingAngleTheta );
						double	sinTheta = Math.Sin( m_incomingAngleTheta );
						double	cosPhi = Math.Cos( m_incomingAnglePhi );
						double	sinPhi = Math.Sin( m_incomingAnglePhi );
						float3	result = new float3( -(float) (cosPhi * sinTheta), -(float) (sinPhi * sinTheta), -(float) cosTheta );
						return result;
					}
				}

				public Result( Document _owner, int _order, int _X, int _Y, int _Z, float _theta, float _phi, float _roughness, float _albedoF0 ) {
					m_owner = _owner;
					m_order = _order;
					m_X = _X;
					m_Y = _Y;
					m_Z = _Z;
					m_incomingAngleTheta = _theta;
					m_incomingAnglePhi = _phi;
					m_surfaceRoughness = _roughness;
					m_surfaceAlbedoF0 = _albedoF0;
				}

				public void		Save( XmlElement _parent ) {
					Attrib( _parent, "Index", "(" + m_X + "," + m_Y + ", " + m_Z + ")" );
					Attrib( _parent, "State", m_state );
					Attrib( _parent, "Error", m_error != null ? m_error : "" );

					// Store surface parameters
					Attrib( _parent, "incomingTheta", m_incomingAngleTheta );
					Attrib( _parent, "incomingPhi", m_incomingAnglePhi );
					Attrib( _parent, "surfaceRoughness", m_surfaceRoughness );
					Attrib( _parent, "albedoF0", m_surfaceAlbedoF0 );

					XmlElement	ElemReflected = AppendChild( _parent, "LobeReflected" );
					m_reflected.Save( ElemReflected );
					if ( m_owner.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
						XmlElement	ElemRefracted = AppendChild( _parent, "LobeRefracted" );
						m_reflected.Save( ElemReflected );
					}
				}

				public void		Load( XmlElement _parent ) {

				}

			}

			#endregion

			public SurfaceParameters	m_surface = new SurfaceParameters();
			public Settings				m_settings = new Settings();
			public Result[][,,]			m_results = new Result[0][,,];

			public event ResultStateChangedEventHandler	ResultStateChanged;

			public Document() {
				InitializeResults();
			}

			/// <summary>
			/// Initializes the array of results of the correct dimensions using the current settings
			/// </summary>
			public void		InitializeResults() {
				int	orders = m_surface.ScatteringOrdersCount;
				int	dimX = m_surface.m_incomingAngle.StepsCount;
				int	dimY = m_surface.m_roughness.StepsCount;
				int	dimZ = m_surface.m_albedoF0.StepsCount;

				int	minOrder = m_surface.ScatteringOrderMin;

				float	incomingAnglePhi = 0.0f;	//@TODO?? We don't care about anisotropy anyway...

				m_results = new Result[orders][,,];
				for ( int order=0; order < orders; order++ ) {

					m_results[order] = new Result[dimX,dimY,dimZ];

					float	incomingAngleMin, incomingAngleStep;
					m_surface.m_incomingAngle.BuildMinStep( out incomingAngleMin, out incomingAngleStep );
					float	roughnessMin, roughnessStep;
					m_surface.m_roughness.BuildMinStep( out roughnessMin, out roughnessStep );
					float	albedoF0Min, albedoF0Step;
					m_surface.m_albedoF0.BuildMinStep( out albedoF0Min, out albedoF0Step );

					float	albedoF0 = albedoF0Min;
					for ( int Z=0; Z < dimZ; Z++, albedoF0+=albedoF0Step ) {
						float	roughness = roughnessMin;
						for ( int Y=0; Y < dimY; Y++, roughness+=roughnessStep ) {
							float	incomingAngle = incomingAngleMin;
							for ( int X=0; X < dimX; X++, incomingAngle+=incomingAngleStep ) {
								Result	R = new Result( this, minOrder + order, X, Y, Z, incomingAngle * (float) Math.PI / 180.0f, incomingAnglePhi, roughness, albedoF0 );
								m_results[order][X,Y,Z] = R;
							}
						}
					}
				}
			}

			/// <summary>
			/// Gets the array of results for the specific scattering order
			/// </summary>
			/// <param name="_order"></param>
			/// <returns></returns>
			public Result[,,]	GetResultsForOrder( int _order ) {
				if ( _order < m_surface.ScatteringOrderMin || _order > m_surface.ScatteringOrderMax )
					return null;

				return m_results[_order - m_surface.ScatteringOrderMin];
			}

			public void		Save( XmlDocument _doc ) {

				XmlElement	Root = _doc.CreateElement( "Root" );

				XmlElement	ElmSurface = AppendChild( Root, "SurfaceParameters" );
				m_surface.Save( ElmSurface );

				XmlElement	ElmSettings = AppendChild( Root, "Settings" );
				m_settings.Save( ElmSettings );

				// Save results in an array
				XmlElement	ElmResults = AppendChild( Root, "Results" );

				int	orders = m_results.Length;
				Attrib( ElmResults, "OrdersCount", orders );

				for ( int order=0; order < orders; order++ ) {
					XmlElement	ElmOrderResults = AppendChild( ElmResults, "Order" );
					Attrib( ElmOrderResults, "Index", order );

					Result[,,]	orderResults = m_results[order];

					int	W = orderResults.GetLength(0);
					int	H = orderResults.GetLength(1);
					int	D = orderResults.GetLength(2);
					Attrib( ElmResults, "SizeX", W );
					Attrib( ElmResults, "SizeY", H );
					Attrib( ElmResults, "SizeZ", D );
					for ( int Z=0; Z < D; Z++ )
						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ ) {
								XmlElement	ElmResult = AppendChild( ElmOrderResults, "Result" );
								orderResults[X,Y,Z].Save( ElmResult );
							}
				}
			}

			public void		Load( XmlDocument _doc ) {

			}

			#region XML Helpers

			static XmlElement	AppendChild( XmlDocument _doc, string _name ) {
				XmlElement	element = _doc.CreateElement( _name );
				_doc.AppendChild( element );
				return element;
			}

			static XmlElement	AppendChild( XmlElement _parent, string _name ) {
				XmlElement	element = _parent.OwnerDocument.CreateElement( _name );
				_parent.AppendChild( element );
				return element;
			}

			static void			Attrib( XmlElement _parent, string _name, object _value ) {
				_parent.SetAttribute( _name, _value.ToString() );
			}

			#endregion
		}

		#endregion

		TestForm		m_owner;

		RegistryKey		m_AppKey;

		LobeModel		m_lobeModel = null;
		WMath.BFGS		m_fitter = new WMath.BFGS();

		bool			m_computing = false;
		bool			m_isReflectedLobe = true;
		Document.Result.LobeParameters	m_currentFittedResult = null;

		FileInfo		m_documentFileName = null;
		Document		m_document = null;
		Document.Result	m_selectedResult = null;		// Current selection

		public new TestForm		Owner {
			get { return m_owner; }
			set { m_owner = value; }
		}

		/// <summary>
		/// Gets or sets the result currently selected in the completion control
		/// </summary>
		Document.Result	SelectedResult {
			get { return m_selectedResult; }
			set {
				if ( value == m_selectedResult )
					return;

				m_selectedResult = value;

				if ( m_selectedResult == null )
					return;

				completionArrayControl.Select( m_selectedResult.X, m_selectedResult.Y, m_selectedResult.Z );

				// Show lobe parameters in main form
				m_owner.UpdateLobeParameters( m_selectedResult.m_reflected.AsArray, true );
				m_owner.UpdateLobeParameters( m_selectedResult.m_refracted.AsArray, false );
			}
		}

		/// <summary>
		/// Use the view trackbar as the official selected scattering order
		/// </summary>
		int				SelectedScatteringOrder {
			get { return integerTrackbarControlViewScatteringOrder.Value; }
			set { integerTrackbarControlViewScatteringOrder.Value = value; }
		}

		TestForm.SURFACE_TYPE	SurfaceType {
			get {
				return radioButtonSurfaceTypeConductor.Checked ? TestForm.SURFACE_TYPE.CONDUCTOR : (radioButtonSurfaceTypeDielectric.Checked ? TestForm.SURFACE_TYPE.DIELECTRIC : TestForm.SURFACE_TYPE.DIFFUSE);
			}
		}

		public AutomationForm() {
			InitializeComponent();

			m_AppKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Patapom\MSBRDF" );

			// Initialize the lobe model
			m_lobeModel = new LobeModel();
			m_lobeModel.ParametersChanged += m_lobeModel_ParametersChanged;

			// Attach a default document
			AttachDocument( new Document() );
		}

		#region Automation Core

		/// <summary>
		/// Prepare the fitter using current settings
		/// </summary>
		void	PrepareFitter() {

			double	functionMinimumTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Minimum );
			double	gradientTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Gradient );

			m_fitter.MaxIterations = m_document.m_settings.m_maxIterations;
			m_fitter.SuccessTolerance = functionMinimumTolerance;
			m_fitter.GradientSuccessTolerance = gradientTolerance;
		}

		/// <summary>
		/// Recomputes the surface heightfield for the specified roughness
		/// </summary>
		/// <param name="_result"></param>
		void	UpdateSurfaceRoughness( Document.Result _result ) {
			m_owner.BuildBeckmannSurfaceTexture( (float) _result.SurfaceRoughness );
		}

		/// <summary>
		/// Simulates incoming rays on surface (core routine)
		/// </summary>
		void	Simulate( Document.Result _result ) {

			float	albedo = _result.SurfaceAlbedoF0;
			float	F0 = _result.SurfaceAlbedoF0;

			m_owner.RayTraceSurface( _result.SurfaceRoughness, albedo, F0, m_document.m_surface.m_type, _result.IncomingAngleTheta, _result.IncomingAnglePhi, m_document.m_surface.m_rayTracingIterationsCount );
		}

		/// <summary>
		/// Perform lobe fitting of the simulated data
		/// </summary>
		/// <param name="_result"></param>
		/// <param name="_reflected"></param>
		int		m_retriesCount = 0;
		void	PerformLobeFitting( Document.Result _result, bool _reflected ) {

			Document.Result.LobeParameters	lobeResults = _reflected ? _result.m_reflected : _result.m_refracted;

			m_isReflectedLobe = _reflected;	// Global flag for current fitting
			m_currentFittedResult = lobeResults;

			// Read back histogram to CPU for fitting
			Texture2D	Tex_SimulatedLobeHistogram = m_owner.GetSimulationHistogram( _reflected );

			// Initialize lobe data & compute center of mass
			m_lobeModel.InitTargetData( Tex_SimulatedLobeHistogram, _result.ScatteringOrder );

			// Initialize preliminary lobe results
			float3				incomingDirection = _result.IncomingDirection;

			float3				reflectedDirection = incomingDirection;
								reflectedDirection.z = -reflectedDirection.z;	// Mirror against surface

			float3				refractedDirection = TestForm.Refract( -incomingDirection, float3.UnitZ, 1.0f / TestForm.Fresnel_IORFromF0( _result.SurfaceAlbedoF0 ) );

			LobeModel.LOBE_TYPE	lobeType = m_document.m_settings.m_lobeModel;

			lobeResults.Initialize( _result, m_lobeModel, _reflected ? reflectedDirection : refractedDirection );

			// Initialize lobe model using initialized lobe results
			m_lobeModel.InitLobeData( lobeType, incomingDirection, lobeResults.m_theta, lobeResults.m_roughness, lobeResults.m_scale, lobeResults.m_flatten, lobeResults.m_masking, m_document.m_settings.m_oversizeFactor, true );

			// Peform fitting
			for ( m_retriesCount=0; m_retriesCount < m_document.m_settings.m_maxRetries; m_retriesCount++ ) {
				m_fitter.Minimize( m_lobeModel );
				if ( m_fitter.IterationsCount < m_document.m_settings.m_maxIterations ) {
					// Finished!
					m_retriesCount++;
					break;
				}
			}
		}

		#endregion

		#region Document Management

		bool		m_internalDocumentChange = false;
		void		AttachDocument( Document _doc ) {
			// Detach existing doc first
			DetachDocument( m_document );

			m_internalDocumentChange = true;
			m_document = _doc;

			// Subscribe to the document's events
			m_document.ResultStateChanged += m_results_ResultStateChanged;
			m_document.m_surface.LockStateChanged += m_surface_LockStateChanged;
			m_document.m_surface.ScatteringOrdersCountChanged += m_surface_ScatteringOrdersCountChanged;
			m_document.m_surface.m_incomingAngle.ValueChanged += m_simulationParameter_ValueChanged;
			m_document.m_surface.m_roughness.ValueChanged += m_simulationParameter_ValueChanged;
			m_document.m_surface.m_albedoF0.ValueChanged += m_simulationParameter_ValueChanged;

			// Mirror
			Document2UI();

			m_internalDocumentChange = false;
		}

		void		DetachDocument( Document _doc ) {
			if ( _doc == null )
				return;

			m_internalDocumentChange = true;

			// Unsubscribe from the document's events
			m_document.m_surface.m_incomingAngle.ValueChanged -= m_simulationParameter_ValueChanged;
			m_document.m_surface.m_roughness.ValueChanged -= m_simulationParameter_ValueChanged;
			m_document.m_surface.m_albedoF0.ValueChanged -= m_simulationParameter_ValueChanged;
			m_document.m_surface.ScatteringOrdersCountChanged -= m_surface_ScatteringOrdersCountChanged;
			m_document.m_surface.LockStateChanged -= m_surface_LockStateChanged;
			m_document.ResultStateChanged -= m_results_ResultStateChanged;

			SelectedResult = null;	// Clear selection
			m_document = null;

			m_internalDocumentChange = false;
		}

		/// <summary>
		/// Mirrors the document's values to the UI
		/// </summary>
		void	Document2UI() {
			DocumentSurface2UI();
			DocumentLobeSettings2UI();
			DocumentSettings2UI();
			DocumentResults2UI();
		}

		/// <summary>
		/// Mirrors the document's surface parameters to the UI
		/// </summary>
		void	DocumentSurface2UI() {

			switch ( m_document.m_surface.m_type ) {
				case TestForm.SURFACE_TYPE.CONDUCTOR: radioButtonSurfaceTypeConductor.Checked = true; break;
				case TestForm.SURFACE_TYPE.DIELECTRIC: radioButtonSurfaceTypeDielectric.Checked = true; break;
				case TestForm.SURFACE_TYPE.DIFFUSE: radioButtonSurfaceTypeDiffuse.Checked = true; break;
			}

			floatTrackbarControlParam0_Min.Value = m_document.m_surface.m_incomingAngle.Min;
			floatTrackbarControlParam0_Max.Value = m_document.m_surface.m_incomingAngle.Max;
			integerTrackbarControlParam0_Steps.Value = m_document.m_surface.m_incomingAngle.StepsCount;
			floatTrackbarControlParam1_Min.Value = m_document.m_surface.m_roughness.Min;
			floatTrackbarControlParam1_Max.Value = m_document.m_surface.m_roughness.Max;
			integerTrackbarControlParam1_Steps.Value = m_document.m_surface.m_roughness.StepsCount;
			floatTrackbarControlParam2_Min.Value = m_document.m_surface.m_albedoF0.Min;
			floatTrackbarControlParam2_Max.Value = m_document.m_surface.m_albedoF0.Max;
			integerTrackbarControlViewAlbedoSlice.RangeMax = m_document.m_surface.m_albedoF0.StepsCount-1;
			integerTrackbarControlParam2_Steps.Value = m_document.m_surface.m_albedoF0.StepsCount;
			integerTrackbarControlScatteringOrder_Min.Value = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlScatteringOrder_Min.RangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlScatteringOrder_Max.Value = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlScatteringOrder_Max.RangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlRayCastingIterations.Value = m_document.m_surface.m_rayTracingIterationsCount;
			checkBoxParam0_InclusiveStart.Checked = m_document.m_surface.m_incomingAngle.InclusiveMin;
			checkBoxParam0_InclusiveEnd.Checked = m_document.m_surface.m_incomingAngle.InclusiveMax;
			checkBoxParam1_InclusiveStart.Checked = m_document.m_surface.m_roughness.InclusiveMin;
			checkBoxParam1_InclusiveEnd.Checked = m_document.m_surface.m_roughness.InclusiveMax;
			checkBoxParam2_InclusiveStart.Checked = m_document.m_surface.m_albedoF0.InclusiveMin;
			checkBoxParam2_InclusiveEnd.Checked = m_document.m_surface.m_albedoF0.InclusiveMax;
		}

		/// <summary>
		/// Mirrors the document's settings to the UI
		/// </summary>
		void	DocumentSettings2UI() {

			switch ( m_document.m_settings.m_lobeModel ) {
				case LobeModel.LOBE_TYPE.MODIFIED_PHONG: radioButtonLobe_ModifiedPhong.Checked = true; break;
				case LobeModel.LOBE_TYPE.BECKMANN: radioButtonLobe_Beckmann.Checked = true; break;
				case LobeModel.LOBE_TYPE.GGX: radioButtonLobe_GGX.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialRoughness ) {
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.SURFACE: radioButtonInitRoughness_UseSurface.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM: radioButtonInitRoughness_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.NO_CHANGE: radioButtonInitRoughness_NoChange.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialMasking ) {
				case Document.Settings.GUESS_INITIAL_MASKING.CUSTOM: radioButtonInitMasking_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_MASKING.NO_CHANGE: radioButtonInitMasking_NoChange.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialFlatten ) {
				case Document.Settings.GUESS_INITIAL_FLATTEN.CUSTOM: radioButtonInitFlatten_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_FLATTEN.NO_CHANGE: radioButtonInitFlatten_NoChange.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialScale ) {
				case Document.Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS: radioButtonInitScale_CoMFactor.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_SCALE.NO_CHANGE: radioButtonInitScale_NoChange.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialDirection ) {
				case Document.Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS: radioButtonInitDirection_TowardCoM.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION: radioButtonInitDirection_TowardReflected.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_DIRECTION.NO_CHANGE: radioButtonInitDirection_NoChange.Checked = true; break;
			}

			checkBoxInitDirection_Inherit.Checked = m_document.m_settings.m_inheritDirection;
			checkBoxInitScale_Inherit.Checked = m_document.m_settings.m_inheritScale;
			checkBoxInitFlatten_Inherit.Checked = m_document.m_settings.m_inheritFlatten;
			checkBoxInitScale_Inherit.Checked = m_document.m_settings.m_inheritRoughness;
			checkBoxInitMasking_Inherit.Checked = m_document.m_settings.m_inheritMasking;
			floatTrackbarControlInit_Scale.Value = m_document.m_settings.m_customScale;
			floatTrackbarControlInit_Flatten.Value = m_document.m_settings.m_customFlatten;
			floatTrackbarControlInit_CustomRoughness.Value = m_document.m_settings.m_customRoughness;
			floatTrackbarControlInit_MaskingImportance.Value = m_document.m_settings.m_customMasking;
		}

		/// <summary>
		/// Mirrors the document's lobe settings to the UI
		/// </summary>
		void	DocumentLobeSettings2UI() {
			integerTrackbarControlMaxIterations.Value = m_document.m_settings.m_maxIterations;
			floatTrackbarControlGoalTolerance.Value = m_document.m_settings.m_logTolerance_Minimum;
			floatTrackbarControlGradientTolerance.Value = m_document.m_settings.m_logTolerance_Gradient;
			integerTrackbarControlRetries.Value = m_document.m_settings.m_maxRetries;
		}

		/// <summary>
		/// Mirrors the document's results to the UI
		/// </summary>
		void	DocumentResults2UI() {
			int	stepsCountX = m_document.m_surface.m_incomingAngle.StepsCount;
			int	stepsCountY = m_document.m_surface.m_roughness.StepsCount;
			int	stepsCountZ = m_document.m_surface.m_albedoF0.StepsCount;
			completionArrayControl.Init( stepsCountX, stepsCountY, stepsCountZ );

			Document.Result[,,]	layerResults = m_document.GetResultsForOrder( SelectedScatteringOrder );

			for ( int Z=0; Z < stepsCountZ; Z++ )
				for ( int Y=0; Y < stepsCountY; Y++ )
					for ( int X=0; X < stepsCountX; X++ ) {
						Document.Result	result = layerResults[X,Y,Z];
						completionArrayControl.SetState( X, Y, Z, result.State, result.m_error );
					}
		}

		void m_surface_ScatteringOrdersCountChanged() {
			if ( m_internalDocumentChange )
				return;

			// Rebuild results
			m_document.InitializeResults();

			// Update UI
			integerTrackbarControlViewScatteringOrder.RangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlViewScatteringOrder.RangeMax = m_document.m_surface.ScatteringOrderMax;

			DocumentResults2UI();
		}

		void m_simulationParameter_ValueChanged( AutomationForm.Document.SurfaceParameters.Parameter _P ) {
			if ( m_internalDocumentChange )
				return;

// Must be updated nonetheless since individual results' data may have changed
// 			int	dimX = m_document.m_surface.m_incomingAngle.StepsCount;
// 			int	dimY = m_document.m_surface.m_roughness.StepsCount;
// 			int	dimZ = m_document.m_surface.m_albedoF0.StepsCount;
// 
// 			if (	m_document.m_results[0].GetLength( 0 ) == dimX
// 				&&	m_document.m_results[0].GetLength( 1 ) == dimY
// 				&&	m_document.m_results[0].GetLength( 2 ) == dimZ )
// 				return;	// No need to resize results!

			m_document.InitializeResults();
			DocumentResults2UI();
		}

		void m_surface_LockStateChanged() {
			groupBoxSimulationParameters.Enabled = !m_document.m_surface.IsLocked;
		}

		void m_results_ResultStateChanged( AutomationForm.Document.Result _result ) {
			if ( m_internalDocumentChange )
				return;

			if ( _result.ScatteringOrder == SelectedScatteringOrder )
				completionArrayControl.SetState( _result.X, _result.Y, _result.Z, _result.State, _result.m_error );	// Only update UI if it's showing the result's scattering order
		}

		#endregion

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			Visible = false;	// Only hide, don't close!
			e.Cancel = true;
			base.OnFormClosing( e );
		}

		#region User Information

		DialogResult	MessageBox( string _Text ) {
			return MessageBox( _Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon ) {
			return System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Automation", _Buttons, _Icon );
		}
		DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon, MessageBoxDefaultButton _defaultButton ) {
			return System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Automation", _Buttons, _Icon, _defaultButton );
		}

		List< string >	m_log = new List< string >();
		void	ClearLog() {
			m_log = new List< string >();
			textBoxLog.Text = "";
			textBoxLog.Refresh();
		}
		void	Log( string _text ) {
			m_log.Add( _text );
			textBoxLog.AppendText( _text );
			textBoxLog.Refresh();
		}
		void	LogLine( string _text ) {
			_text += "\r\n";
			m_log.Add( _text );
			textBoxLog.AppendText( _text );
			textBoxLog.Refresh();
		}

		string	FormatTime( DateTime _time ) {
			return _time.ToString( @"hh\:mm\:ss" );
		}

		string	FormatDuration( TimeSpan _duration ) {
			return _duration.ToString( @"hh\:mm\:ss\" ) + ":" + _duration.Milliseconds.ToString( "G03" );
		}

		#endregion

		DateTime	m_simulationStart;
		DateTime	m_simulationEnd;
		private void buttonCompute_Click( object sender, EventArgs e )
		{
			if ( m_computing )
				throw new CanceledException();

//				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );

			// Lock surface, we can't change dimensions now that we're started!
			if ( !m_document.m_surface.IsLocked )
				m_document.m_surface.Lock();

			// @TODO
			throw new Exception( );

			try {





			} catch ( Exception _e ) {
				bool	canceled = _e is CanceledException;
				string	Text = canceled ? "User canceled...\r\n" : "An error occurred while performing lobe fitting:\r\n" + _e.Message;
//				MessageBox( Text + "\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
				
			} finally {
			}
		}


		private void completionArrayControl_MouseDoubleClick( object sender, MouseEventArgs e ) {
			if ( !completionArrayControl.IsPointValid( e.Location ) )
				return;	// Not a valid candidate for simulation

			// Change selection to clicked
			completionArrayControl.SelectAt( e.Location );

			// Lock surface, we can't change dimensions now that we're started!
			if ( !m_document.m_surface.IsLocked )
				m_document.m_surface.Lock();

			m_simulationStart = DateTime.Now;
			SelectedResult.m_error = null;
			SelectedResult.State = 0.0f;

			// Compute a single value
			try {
				LogLine( "== Starting Order " + SelectedResult.ScatteringOrder + " (" + SelectedResult.X + ", " + SelectedResult.Y + ", " + SelectedResult.Z + ") ==" );
				LogLine( "	• Angle = " + (SelectedResult.IncomingAngleTheta * 180.0 / Math.PI).ToString( "G3" ) + " - Roughness = " + SelectedResult.SurfaceRoughness.ToString( "G3" ) + " - " + (m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0") + " = " + SelectedResult.SurfaceAlbedoF0.ToString( "G3" ) );
				LogLine( "	• Start Time = " + FormatTime( m_simulationStart ) );

				PrepareFitter();
				UpdateSurfaceRoughness( SelectedResult );
				Simulate( SelectedResult );

				DateTime	simulationEnd = DateTime.Now;
				TimeSpan	simulationDuration = simulationEnd - m_simulationStart;
				LogLine( "	• Simulation Duration = " + FormatDuration( simulationDuration ) );

				// Fit reflected lobe...
				PerformLobeFitting( SelectedResult, true );
				LogLine( "	## Reflected lobe - Fit minimum reached = " + m_fitter.FunctionMinimum + " after " + m_retriesCount + " attempts" );

				if ( m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
					// Fit refracted lobe now...
					SelectedResult.State = 0.5f;

					PerformLobeFitting( SelectedResult, false );
					LogLine( "	## Refracted lobe - Fit minimum reached = " + m_fitter.FunctionMinimum + " after " + m_retriesCount + " attempts" );
				}

				SelectedResult.State = 1.0f;

			} catch ( CanceledException ) {
				LogLine( "Canceled" );
				SelectedResult.m_error = "Canceled";
				SelectedResult.State = -1.0f;
			} catch ( Exception _e ) {
				string	errorText = "An error occurred during fitting: " + _e.Message;
				LogLine( errorText );
				SelectedResult.m_error = errorText;
				SelectedResult.State = -1.0f;
			} finally {

				m_simulationEnd = DateTime.Now;

				TimeSpan	duration = m_simulationEnd - m_simulationStart;
				LogLine( "	• End Time = " + FormatTime( m_simulationEnd ) + " (Duration: " + FormatDuration( duration ) + ")" );
				LogLine( "== Fitting Ended ==" );
				LogLine( "" );
				LogLine( "" );
			}
		}

		void m_lobeModel_ParametersChanged( double[] _parameters ) {

			// Store new parameters
			m_currentFittedResult.m_theta = _parameters[0];
			m_currentFittedResult.m_roughness = _parameters[1];
			m_currentFittedResult.m_scale = _parameters[2];
			m_currentFittedResult.m_flatten = _parameters[3];
			m_currentFittedResult.m_masking = _parameters[4];

			// Update 3D rendering
			m_owner.UpdateLobeParameters( _parameters, m_isReflectedLobe );
		}

		private void buttonClearResults_Click( object sender, EventArgs e ) {
			if ( !m_document.m_surface.IsLocked ) {
				MessageBox( "No results are computed yet, there is nothing to clear", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			if ( MessageBox( "Are you sure you want to erase current results?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;

			//@TODO
			throw new Exception();
		}

		private void newToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure you want to start a new document and lose existing results?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;

			AttachDocument( new Document() );
		}

		private void saveToolStripMenuItem1_Click( object sender, EventArgs e ) {
			try {
				string	FileName = m_AppKey.GetValue( "LastResultsFileName", new System.IO.FileInfo( "results.xml" ).FullName ) as string;
				saveFileDialogResults.FileName = Path.GetFileName( FileName );
				saveFileDialogResults.InitialDirectory = Path.GetDirectoryName( FileName );
				if ( saveFileDialogResults.ShowDialog( this ) != DialogResult.OK )
					return;

				XmlDocument	Doc = new XmlDocument();
				m_document.Save( Doc );

				Doc.Save( FileName );

				m_AppKey.SetValue( "LastResultsFileName", saveFileDialogResults.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving results:\r\n" + _e );
			}
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e) {
			try {
				string	FileName = m_AppKey.GetValue( "LastResultsFileName", new System.IO.FileInfo( "results.xml" ).FullName ) as string;
				openFileDialogResults.FileName = Path.GetFileName( FileName );
				openFileDialogResults.InitialDirectory = Path.GetDirectoryName( FileName );
				if ( openFileDialogResults.ShowDialog( this ) != DialogResult.OK )
					return;

				XmlDocument	Doc = new XmlDocument();
				Doc.Load( FileName );

				m_document.Load( Doc );

				m_AppKey.SetValue( "LastResultsFileName", openFileDialogResults.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving results:\r\n" + _e );
			}
		}

		private void completionArrayControl_SelectionChanged( CompletionArrayControl _Sender ) {
			Document.Result[,,]	layerResults = m_document.GetResultsForOrder( integerTrackbarControlViewScatteringOrder.Value );
			SelectedResult = layerResults[_Sender.SelectedX,_Sender.SelectedY, _Sender.SelectedZ];
		}

		private void integerTrackbarControlViewScatteringOrder_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			// Update UI & selection
			DocumentResults2UI();

			Document.Result[,,]	layerResults = m_document.GetResultsForOrder( _Sender.Value );

			SelectedResult = layerResults[completionArrayControl.SelectedX, completionArrayControl.SelectedY, completionArrayControl.SelectedZ];
		}

		private void integerTrackbarControlViewAlbedoSlice_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			completionArrayControl.SelectedZ = _Sender.Value;
		}

		private void contextMenuStripSelection_Opening( object sender, CancelEventArgs e )
		{
			if ( SelectedResult == null )
				e.Cancel = true;

			//@TODO: Handle selection
		}

		
		#region UI => Document Mirroring

		#region Settings

		private void LobeTypeCheckChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_lobeModel =	radioButtonLobe_ModifiedPhong.Checked ? LobeModel.LOBE_TYPE.MODIFIED_PHONG :
												(radioButtonLobe_Beckmann.Checked ? LobeModel.LOBE_TYPE.BECKMANN :
												LobeModel.LOBE_TYPE.GGX);
		}

		private void radioButtonInitRoughness_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialRoughness = radioButtonInitRoughness_UseSurface.Checked ?	Document.Settings.GUESS_INITIAL_ROUGHNESS.SURFACE :
													(radioButtonInitRoughness_Custom.Checked ?		Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM :
																									Document.Settings.GUESS_INITIAL_ROUGHNESS.NO_CHANGE);
		}

		private void radioButtonInitMasking_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialMasking = radioButtonInitMasking_Custom.Checked ?	Document.Settings.GUESS_INITIAL_MASKING.CUSTOM :
																							Document.Settings.GUESS_INITIAL_MASKING.NO_CHANGE;
		}

		private void radioButtonInitFlatten_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialFlatten = radioButtonInitFlatten_Custom.Checked ?	Document.Settings.GUESS_INITIAL_FLATTEN.CUSTOM :
																							Document.Settings.GUESS_INITIAL_FLATTEN.NO_CHANGE;
		}

		private void radioButtonInitScale_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialScale = radioButtonInitScale_CoMFactor.Checked ?	Document.Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS :
																							Document.Settings.GUESS_INITIAL_SCALE.NO_CHANGE;
		}

		private void radioButtonInitDirection_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialDirection = radioButtonInitDirection_TowardCoM.Checked ?		Document.Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS :
													(radioButtonInitDirection_TowardReflected.Checked ?	Document.Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION :
																										Document.Settings.GUESS_INITIAL_DIRECTION.NO_CHANGE);
		}

		private void checkBoxInitDirection_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritDirection = checkBoxInitDirection_Inherit.Checked;
		}

		private void checkBoxInitScale_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritScale = checkBoxInitScale_Inherit.Checked;
		}

		private void checkBoxInitFlatten_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritFlatten = checkBoxInitFlatten_Inherit.Checked;
		}

		private void checkBoxInitRoughness_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritRoughness = checkBoxInitScale_Inherit.Checked;
		}

		private void checkBoxInitMasking_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritMasking = checkBoxInitMasking_Inherit.Checked;
		}

		private void floatTrackbarControlInit_Scale_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_customScale = _Sender.Value;
		}

		private void floatTrackbarControlInit_Flatten_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_customFlatten = _Sender.Value;
		}

		private void floatTrackbarControlInit_CustomRoughness_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_customRoughness = _Sender.Value;
		}

		private void floatTrackbarControlInit_MaskingImportance_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_customMasking = _Sender.Value;
		}

		#endregion

		#region Surface

		private void radioButtonSurfaceType_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_type = radioButtonSurfaceTypeConductor.Checked ?	TestForm.SURFACE_TYPE.CONDUCTOR : (
										radioButtonSurfaceTypeDielectric.Checked ?	TestForm.SURFACE_TYPE.DIELECTRIC :
																					TestForm.SURFACE_TYPE.DIFFUSE);

			// Update UI
			labelParm2.Text = SurfaceType == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0";

			// Also update surface type in the main form
			if ( m_owner != null )
				m_owner.SetSurfaceType( m_document.m_surface.m_type );
		}

		private void floatTrackbarControlParam0_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_incomingAngle.Min = floatTrackbarControlParam0_Min.Value;
		}

		private void floatTrackbarControlParam0_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_incomingAngle.Max = floatTrackbarControlParam0_Max.Value;
		}

		private void integerTrackbarControlParam0_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.m_incomingAngle.StepsCount = integerTrackbarControlParam0_Steps.Value;
			DocumentResults2UI();
		}

		private void floatTrackbarControlParam1_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_roughness.Min = floatTrackbarControlParam1_Min.Value;
		}

		private void floatTrackbarControlParam1_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_roughness.Max = floatTrackbarControlParam1_Max.Value;
		}

		private void integerTrackbarControlParam1_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.m_roughness.StepsCount = integerTrackbarControlParam1_Steps.Value;
			DocumentResults2UI();
		}

		private void floatTrackbarControlParam2_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_albedoF0.Min = floatTrackbarControlParam2_Min.Value;
		}

		private void floatTrackbarControlParam2_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_surface.m_albedoF0.Max = floatTrackbarControlParam2_Max.Value;
		}

		private void integerTrackbarControlParam2_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.m_albedoF0.StepsCount = integerTrackbarControlParam2_Steps.Value;
			DocumentResults2UI();

			integerTrackbarControlViewAlbedoSlice.RangeMax = m_document.m_surface.m_albedoF0.StepsCount - 1;
			integerTrackbarControlViewAlbedoSlice.VisibleRangeMax = integerTrackbarControlViewAlbedoSlice.RangeMax;
		}

		private void integerTrackbarControlScatteringOrder_Min_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.ScatteringOrderMin = integerTrackbarControlScatteringOrder_Min.Value;

			integerTrackbarControlScatteringOrder_Max.RangeMin = m_document.m_surface.ScatteringOrderMin;	// Max scattering can't go lower than this
			integerTrackbarControlScatteringOrder_Max.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlViewScatteringOrder.RangeMin = _Sender.Value;
			integerTrackbarControlViewScatteringOrder.VisibleRangeMin = _Sender.Value;
		}

		private void integerTrackbarControlScatteringOrder_Max_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.ScatteringOrderMax = integerTrackbarControlScatteringOrder_Max.Value;

			integerTrackbarControlScatteringOrder_Min.RangeMax = m_document.m_surface.ScatteringOrderMax;	// Min scattering can't go higher than this
			integerTrackbarControlScatteringOrder_Min.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlViewScatteringOrder.RangeMax = _Sender.Value;
			integerTrackbarControlViewScatteringOrder.VisibleRangeMax = _Sender.Value;
		}

		private void integerTrackbarControlRayCastingIterations_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_surface.m_rayTracingIterationsCount = integerTrackbarControlRayCastingIterations.Value;

			long	count = TestForm.HEIGHTFIELD_SIZE * TestForm.HEIGHTFIELD_SIZE;
					count *= (long) integerTrackbarControlRayCastingIterations.Value;

			string	readableCount = "";
			while ( count > 0 ) {
				long	mod = count % 1000;
				count /= 1000;
				readableCount = (count > 0 ? "," + mod.ToString( "G03" ) : "" + mod.ToString( "G3" )) + readableCount;
			}

			labelTotalRaysCount.Text = "Total Simulated Rays: " + readableCount;
		}

		private void checkBoxParam0_InclusiveStart_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_incomingAngle.InclusiveMin = checkBoxParam0_InclusiveStart.Checked;
		}

		private void checkBoxParam0_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_incomingAngle.InclusiveMax = checkBoxParam0_InclusiveEnd.Checked;
		}

		private void checkBoxParm1_InclusiveStart_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_roughness.InclusiveMin = checkBoxParam1_InclusiveStart.Checked;
		}

		private void checkBoxParam1_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_roughness.InclusiveMax = checkBoxParam1_InclusiveEnd.Checked;
		}

		private void checkBoxParm2_InclusiveStart_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_albedoF0.InclusiveMin = checkBoxParam2_InclusiveStart.Checked;
		}

		private void checkBoxParm2_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_surface.m_albedoF0.InclusiveMax = checkBoxParam2_InclusiveEnd.Checked;
		}

		#endregion

		#region Lobe Fitter

		private void integerTrackbarControlMaxIterations_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_settings.m_maxIterations = integerTrackbarControlMaxIterations.Value;
		}

		private void floatTrackbarControlGoalTolerance_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_logTolerance_Minimum = floatTrackbarControlGoalTolerance.Value;
		}

		private void floatTrackbarControlGradientTolerance_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_logTolerance_Gradient = floatTrackbarControlGradientTolerance.Value;
		}

		private void integerTrackbarControlRetries_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_document.m_settings.m_maxRetries = integerTrackbarControlRetries.Value;
		}

		private void floatTrackbarControlFitOversize_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_oversizeFactor = floatTrackbarControlFitOversize.Value;
		}

		#endregion

		#endregion
	}
}
