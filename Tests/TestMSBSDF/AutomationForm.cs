﻿using System;
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
using System.Threading;

using SharpMath;
using Renderer;

namespace TestMSBSDF
{
	public partial class AutomationForm : Form
	{
		const int		AUTO_SAVE_EVERY_N_RUNS = 5;	// Save results every 5 computation
		const int		THREADS_COUNT = 16;			// Multithreading

		#region NESTED TYPES

		public class CanceledException : Exception {}

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

				[System.Diagnostics.DebuggerDisplay( "Range=[{m_min}, {m_max}] Steps={m_stepsCount}" )]
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
						Attrib( _parent, "Min", m_min );
						Attrib( _parent, "Max", m_max );
						Attrib( _parent, "Steps", m_stepsCount );
						Attrib( _parent, "InclusiveMin", m_inclusiveMin );
						Attrib( _parent, "InclusiveMax", m_inclusiveMax );
					}

					public void		Load( XmlElement _parent, int _version ) {
						Attrib( _parent, "Min", ref m_min );
						Attrib( _parent, "Max", ref m_max );
						Attrib( _parent, "Steps", ref m_stepsCount );
						Attrib( _parent, "InclusiveMin", ref m_inclusiveMin );
						Attrib( _parent, "InclusiveMax", ref m_inclusiveMax );
					}
				}

				#endregion

				public int						m_rayTracingIterationsCount = 1024;
				public TestForm.SURFACE_TYPE	m_type = TestForm.SURFACE_TYPE.DIFFUSE;
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
					m_scatteringOrders = new Parameter( this, 1, TestForm.MAX_SCATTERING_ORDER + 1, 1 );
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

				/// <summary>
				/// WARNING: Only use this when results are cleared otherwise the user can lose results!
				/// </summary>
				public void		Unlock() {
					if ( !m_locked )
						throw new Exception( "Surface is not locked!" );

					m_locked = false;

					if ( LockStateChanged != null )
						LockStateChanged();
				}

				public void		Save( XmlElement _parent ) {
					Attrib( _parent, "SurfaceType", m_type );
					Attrib( _parent, "RayTraceIterations", m_rayTracingIterationsCount );
					Attrib( _parent, "Locked", m_locked );

					XmlElement ElemParm0 = AppendChild( _parent, "IncomingAngle" );
					m_incomingAngle.Save( ElemParm0 );

					XmlElement ElemParm1 = AppendChild( _parent, "SurfaceRoughness" );
					m_roughness.Save( ElemParm1 );

					XmlElement ElemParm2 = AppendChild( _parent, "AlbedoF0" );
					m_albedoF0.Save( ElemParm2 );

					XmlElement ElemParm3 = AppendChild( _parent, "ScatteringOrders" );
					m_scatteringOrders.Save( ElemParm3 );
				}

				public void		Load( XmlElement _parent, int _version ) {
					Attrib( _parent, "SurfaceType", ref m_type );
					Attrib( _parent, "RayTraceIterations", ref m_rayTracingIterationsCount );
					Attrib( _parent, "Locked", ref m_locked );

					XmlElement ElemParm0 = _parent["IncomingAngle"];
					m_incomingAngle.Load( ElemParm0, _version );

					XmlElement ElemParm1 = _parent["SurfaceRoughness"];
					m_roughness.Load( ElemParm1, _version );

					XmlElement ElemParm2 = _parent["AlbedoF0"];
					m_albedoF0.Load( ElemParm2, _version );

					XmlElement ElemParm3 = _parent["ScatteringOrders"];
					m_scatteringOrders.Load( ElemParm3, _version );
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
					FIXED,					// Means constrained to specified value
				}

				public enum GUESS_INITIAL_ROUGHNESS {
					SURFACE,
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
					FIXED,					// Means constrained to specified value
					ANALYTICAL,				// Means we use the analytical function we managed to fit with Mathematica so this parameter is not free anymore!
				}

				public enum GUESS_INITIAL_SCALE {
					FACTOR_CENTER_OF_MASS,
					NO_CHANGE,				// Means no change from last computation
					FIXED,					// Means constrained to specified value
					ANALYTICAL,				// Means we use the analytical function we managed to fit with Mathematica so this parameter is not free anymore!
				}

				public enum GUESS_INITIAL_FLATTEN {
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
					FIXED,					// Means constrained to specified value
					ANALYTICAL,				// Means we use the analytical function we managed to fit with Mathematica so this parameter is not free anymore!
				}

				public enum GUESS_INITIAL_MASKING {
					CUSTOM,
					NO_CHANGE,				// Means no change from last computation
					FIXED,					// Means constrained to specified value
				}

				// Fitter parameters
				public bool						m_performFitting = true;		// Patapom (18/04/20) You can now skip fitting and only compute total reflectance 
				public int						m_maxIterations = 200;
				public float					m_logTolerance_Minimum = -6.0f;
				public float					m_logTolerance_Gradient = -6.0f;
				public int						m_maxRetries = 2;
				public float					m_oversizeFactor = 1.0f;

				// Lobe parameters
				public LobeModel.LOBE_TYPE		m_lobeModel = LobeModel.LOBE_TYPE.MODIFIED_PHONG;
				public GUESS_INITIAL_DIRECTION	m_initialDirection = GUESS_INITIAL_DIRECTION.CENTER_OF_MASS;
				public bool						m_inheritDirection_Top = false;
				public bool						m_inheritDirection_Left = false;
				public float					m_fixedTheta = 0.0f;
				public GUESS_INITIAL_ROUGHNESS	m_initialRoughness = GUESS_INITIAL_ROUGHNESS.SURFACE;
				public bool						m_inheritRoughness_Top = false;
				public bool						m_inheritRoughness_Left = false;
				public float					m_customRoughness = 0.8f;
				public float					m_fixedRoughness = 1.0f;
				public GUESS_INITIAL_SCALE		m_initialScale = GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS;
				public bool						m_inheritScale_Top = false;
				public bool						m_inheritScale_Left = false;
				public float					m_customScale = 0.05f;
				public float					m_fixedScale = 1.0f;
				public GUESS_INITIAL_FLATTEN	m_initialFlatten = GUESS_INITIAL_FLATTEN.CUSTOM;
				public bool						m_inheritFlatten_Top = false;
				public bool						m_inheritFlatten_Left = false;
				public float					m_customFlatten = 0.5f;
				public float					m_fixedFlatten = 1.0f;
				public GUESS_INITIAL_MASKING	m_initialMasking = GUESS_INITIAL_MASKING.CUSTOM;
				public bool						m_inheritMasking_Top = false;
				public bool						m_inheritMasking_Left = false;
				public float					m_customMasking = 1.0f;
				public float					m_fixedMasking = 1.0f;


				public	Settings() {
				}

				public void		Save( XmlElement _parent ) {

					// Fitter parameters
					XmlElement	ElemFitterParms = AppendChild( _parent, "FitterParameters" );
					_parent.AppendChild( ElemFitterParms );

					Attrib( ElemFitterParms, "PerformFitting", m_performFitting );
					Attrib( ElemFitterParms, "MaxIterations", m_maxIterations );
					Attrib( ElemFitterParms, "logToleranceMinimum", m_logTolerance_Minimum );
					Attrib( ElemFitterParms, "logToleranceGradient", m_logTolerance_Gradient );
					Attrib( ElemFitterParms, "MaxRetries", m_maxRetries );
					Attrib( ElemFitterParms, "OversizeFactor", m_oversizeFactor );

					// Lobe parameters
					XmlElement	ElemLobeParms = AppendChild( _parent, "LobeParameters" );

					Attrib( ElemLobeParms, "Model",				m_lobeModel );
					Attrib( ElemLobeParms, "initialDirection",	m_initialDirection );
					Attrib( ElemLobeParms, "inheritDirection",	m_inheritDirection_Top );
					Attrib( ElemLobeParms, "inheritDirectionLeft",	m_inheritDirection_Left );
					Attrib( ElemLobeParms, "fixedTheta",		m_fixedTheta );
					Attrib( ElemLobeParms, "initialRoughness",	m_initialRoughness );
					Attrib( ElemLobeParms, "inheritRoughness",	m_inheritRoughness_Top );
					Attrib( ElemLobeParms, "inheritRoughnessLeft",	m_inheritRoughness_Left );
					Attrib( ElemLobeParms, "customRoughness",	m_customRoughness );
					Attrib( ElemLobeParms, "fixedRoughness",	m_fixedRoughness );
					Attrib( ElemLobeParms, "initialScale",		m_initialScale );
					Attrib( ElemLobeParms, "inheritScale",		m_inheritScale_Top );
					Attrib( ElemLobeParms, "inheritScaleLeft",	m_inheritScale_Left );
					Attrib( ElemLobeParms, "customScale",		m_customScale );
					Attrib( ElemLobeParms, "fixedScale",		m_fixedScale );
					Attrib( ElemLobeParms, "initialFlatten",	m_initialFlatten );
					Attrib( ElemLobeParms, "inheritFlatten",	m_inheritFlatten_Top );
					Attrib( ElemLobeParms, "inheritFlattenLeft",m_inheritFlatten_Left );
					Attrib( ElemLobeParms, "customFlatten",		m_customFlatten );
					Attrib( ElemLobeParms, "fixedFlatten",		m_fixedFlatten );
					Attrib( ElemLobeParms, "initialMasking",	m_initialMasking );
					Attrib( ElemLobeParms, "inheritMasking",	m_inheritMasking_Top );
					Attrib( ElemLobeParms, "inheritMaskingLeft",m_inheritMasking_Left );
					Attrib( ElemLobeParms, "customMasking",		m_customMasking );
					Attrib( ElemLobeParms, "fixedMasking",		m_fixedMasking );
				}

				public void		Load( XmlElement _parent, int _version ) {

					// Fitter parameters
					XmlElement	ElemFitterParms = _parent["FitterParameters"];

					Attrib( ElemFitterParms, "PerformFitting", ref m_performFitting );
					Attrib( ElemFitterParms, "MaxIterations", ref m_maxIterations );
					Attrib( ElemFitterParms, "logToleranceMinimum", ref m_logTolerance_Minimum );
					Attrib( ElemFitterParms, "logToleranceGradient", ref m_logTolerance_Gradient );
					Attrib( ElemFitterParms, "MaxRetries", ref m_maxRetries );
					Attrib( ElemFitterParms, "OversizeFactor", ref m_oversizeFactor );

					// Lobe parameters
					XmlElement	ElemLobeParms = _parent["LobeParameters"];

					Attrib( ElemLobeParms, "Model",				ref m_lobeModel );
					Attrib( ElemLobeParms, "initialDirection",	ref m_initialDirection );
					Attrib( ElemLobeParms, "inheritDirection",	ref m_inheritDirection_Top );
					Attrib( ElemLobeParms, "fixedTheta",		ref m_fixedTheta );
					Attrib( ElemLobeParms, "initialRoughness",	ref m_initialRoughness );
					Attrib( ElemLobeParms, "inheritRoughness",	ref m_inheritRoughness_Top );
					Attrib( ElemLobeParms, "customRoughness",	ref m_customRoughness );
					Attrib( ElemLobeParms, "initialScale",		ref m_initialScale );
					Attrib( ElemLobeParms, "inheritScale",		ref m_inheritScale_Top );
					Attrib( ElemLobeParms, "customScale",		ref m_customScale );
					Attrib( ElemLobeParms, "initialFlatten",	ref m_initialFlatten );
					Attrib( ElemLobeParms, "inheritFlatten",	ref m_inheritFlatten_Top );
					Attrib( ElemLobeParms, "customFlatten",		ref m_customFlatten );
					Attrib( ElemLobeParms, "initialMasking",	ref m_initialMasking );
					Attrib( ElemLobeParms, "inheritMasking",	ref m_inheritMasking_Top );
					Attrib( ElemLobeParms, "customMasking",		ref m_customMasking );

					if ( _version >= 1 ) {
						Attrib( ElemLobeParms, "inheritDirectionLeft",	ref m_inheritDirection_Left );
						Attrib( ElemLobeParms, "inheritRoughnessLeft",	ref m_inheritRoughness_Left );
						Attrib( ElemLobeParms, "inheritScaleLeft",	ref m_inheritScale_Left );
						Attrib( ElemLobeParms, "inheritFlattenLeft",ref m_inheritFlatten_Left );
						Attrib( ElemLobeParms, "inheritMaskingLeft",ref m_inheritMasking_Left );
						Attrib( ElemLobeParms, "fixedRoughness",	ref m_fixedRoughness );
						Attrib( ElemLobeParms, "fixedScale",		ref m_fixedScale );
						Attrib( ElemLobeParms, "fixedFlatten",		ref m_fixedFlatten );
						Attrib( ElemLobeParms, "fixedMasking",		ref m_fixedMasking );
					}
				}
			}

			[System.Diagnostics.DebuggerDisplay( "Order={m_order} ({m_X}, {m_Y}, {m_Z}) Theta={m_incomingAngleTheta} Rho={m_surfaceRoughness} F0={m_surfaceAlbedoF0} State={m_state}" )]
			public class	Result {

				#region NESTED TYPES

				[System.Diagnostics.DebuggerDisplay( "Theta={m_theta} Rho={m_roughness} Scale={m_scale} Flatten={m_flatten} Masking={m_masking}" )]
				public class	LobeParameters {
					public double	m_theta = -1.0;		// -1 means invalid
					public double	m_roughness = 0.8;
					public double	m_scale = 0.1;
					public double	m_flatten = 0.5;
					public double	m_masking = 1.0;

					public double	m_totalReflectance = 0.0;	// Patapom (18/04/18) Summation of the total reflectance from the histogram

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

						//////////////////////////////////////////////////////////////////////////
						// Initialize theta
						int				prevParmX = S.m_inheritDirection_Left ? _owner.m_X-1 : (S.m_inheritDirection_Top ? _owner.m_X : -1);
						int				prevParmY = S.m_inheritDirection_Top ? _owner.m_Y-1 : (S.m_inheritDirection_Left ? _owner.m_Y : -1);
						if ( prevParmX > 0 && prevParmY > 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[prevParmX, prevParmY, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						}
						if ( previousParams != null ) {
							// Re-use last fitted direction with the same angle of incidence but different roughness
							m_theta = previousParams.m_theta;
						} else {
							switch ( S.m_initialDirection ) {
								case Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS: {
									// Override theta to use the direction of the center of mass
									// (it's quite intuitive to start by aligning our lobe along the main simulated lobe direction!)
									if ( _lobe.CenterOfMass.Length > 1e-6 ) {
										float3	towardCenterOfMass = _lobe.CenterOfMass.Normalized;
										m_theta = (float) Math.Acos( towardCenterOfMass.z );
									} else {
										m_theta = 0.0f;
									}
									break;
								}
								case Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION:
									m_theta = Math.Acos( _reflectedDirection.z );
									break;
								case Settings.GUESS_INITIAL_DIRECTION.FIXED:
									m_theta = S.m_fixedTheta;
									break;
							}
						}

						_lobe.SetConstraint( 0,	S.m_initialDirection == Settings.GUESS_INITIAL_DIRECTION.FIXED ? S.m_fixedTheta : -0.4999 * Math.PI,
												S.m_initialDirection == Settings.GUESS_INITIAL_DIRECTION.FIXED ? S.m_fixedTheta : 0.4999 * Math.PI );


						//////////////////////////////////////////////////////////////////////////
						// Initialize roughness
						prevParmX = S.m_inheritRoughness_Left ? _owner.m_X-1 : (S.m_inheritRoughness_Top ? _owner.m_X : -1);
						prevParmY = S.m_inheritRoughness_Top ? _owner.m_Y-1 : (S.m_inheritRoughness_Left ? _owner.m_Y : -1);
						if ( prevParmX >= 0 && prevParmY >= 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[prevParmX, prevParmY, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						} else
							previousParams = null;
						if ( previousParams != null ) {
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
								case Settings.GUESS_INITIAL_ROUGHNESS.FIXED:
									m_roughness = S.m_fixedRoughness;
									break;
							}
						}

						if ( S.m_initialRoughness == Settings.GUESS_INITIAL_ROUGHNESS.ANALYTICAL ) {
							// We now have an analytical expression for the roughness parameter!
							float	roughness = ComputeAnalyticalRoughnessParameter( _owner.SurfaceRoughness );
							_lobe.SetConstraint( 1, roughness, roughness );
						} else {
							_lobe.SetConstraint( 1,	S.m_initialRoughness == Settings.GUESS_INITIAL_ROUGHNESS.FIXED ? S.m_fixedRoughness : 1e-4,
													S.m_initialRoughness == Settings.GUESS_INITIAL_ROUGHNESS.FIXED ? S.m_fixedRoughness : 1.0 );
						}


						//////////////////////////////////////////////////////////////////////////
						// Initialize scale
						prevParmX = S.m_inheritScale_Left ? _owner.m_X-1 : (S.m_inheritScale_Top ? _owner.m_X : -1);
						prevParmY = S.m_inheritScale_Top ? _owner.m_Y-1 : (S.m_inheritScale_Left ? _owner.m_Y : -1);
						if ( prevParmX >= 0 && prevParmY >= 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[prevParmX, prevParmY, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						} else
							previousParams = null;
						if ( previousParams != null ) {
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
								case Settings.GUESS_INITIAL_SCALE.FIXED:
									m_scale = S.m_fixedScale;
									break;
							}
						}

						if ( S.m_initialScale == Settings.GUESS_INITIAL_SCALE.ANALYTICAL ) {
							// We now have an analytical expression for the scale parameter!
							float	scale = ComputeAnalyticalScaleParameter( _owner.IncomingAngleTheta, _owner.SurfaceRoughness, _owner.SurfaceAlbedoF0 );
							_lobe.SetConstraint( 2, scale, scale );
						} else {
							_lobe.SetConstraint( 2, S.m_initialScale == Settings.GUESS_INITIAL_SCALE.FIXED ? S.m_fixedScale : 1e-6,
													S.m_initialScale == Settings.GUESS_INITIAL_SCALE.FIXED ? S.m_fixedScale : 10.0 );
						}


						//////////////////////////////////////////////////////////////////////////
						// Initialize flattening factor
						prevParmX = S.m_inheritFlatten_Left ? _owner.m_X-1 : (S.m_inheritFlatten_Top ? _owner.m_X : -1);
						prevParmY = S.m_inheritFlatten_Top ? _owner.m_Y-1 : (S.m_inheritFlatten_Left ? _owner.m_Y : -1);
						if ( prevParmX >= 0 && prevParmY >= 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[prevParmX, prevParmY, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						} else
							previousParams = null;
						if ( previousParams != null ) {
							// Re-use last fitted flatten with the same angle of incidence but different roughness
							m_flatten = previousParams.m_flatten;
						} else {
							switch ( S.m_initialFlatten ) {
								case Settings.GUESS_INITIAL_FLATTEN.CUSTOM:
									m_flatten = S.m_customFlatten;
									break;
								case Settings.GUESS_INITIAL_FLATTEN.FIXED:
									m_flatten = S.m_fixedFlatten;
									break;
							}
						}

						if ( S.m_initialFlatten == Settings.GUESS_INITIAL_FLATTEN.ANALYTICAL ) {
							// We now have an analytical expression for the flattening parameter!
							float	flatten = ComputeAnalyticalFlattenParameter( _owner.IncomingAngleTheta, _owner.SurfaceRoughness );
							_lobe.SetConstraint( 3, flatten, flatten );
						} else {
							_lobe.SetConstraint( 3, S.m_initialFlatten == Settings.GUESS_INITIAL_FLATTEN.FIXED ? S.m_fixedFlatten : 1e-3,
													S.m_initialFlatten == Settings.GUESS_INITIAL_FLATTEN.FIXED ? S.m_fixedFlatten : 2.0 );
						}


						//////////////////////////////////////////////////////////////////////////
						// Initialize masking importance
						prevParmX = S.m_inheritMasking_Left ? _owner.m_X-1 : (S.m_inheritMasking_Top ? _owner.m_X : -1);
						prevParmY = S.m_inheritMasking_Top ? _owner.m_Y-1 : (S.m_inheritMasking_Left ? _owner.m_Y : -1);
						if ( prevParmX >= 0 && prevParmY >= 0 ) {
							Result		previousResult = _owner.m_owner.GetResultsForOrder( _owner.ScatteringOrder )[prevParmX, prevParmY, _owner.m_Z];
							previousParams = reflected ? previousResult.m_reflected : previousResult.m_refracted;
							if ( !previousParams.IsValid )
								previousParams = null;		// Can't use if the results are invalid!
						} else
							previousParams = null;
						if ( previousParams != null ) {
							// Re-use last fitted masking with the same angle of incidence but different roughness
							m_masking = previousParams.m_masking;
						} else {
							switch ( S.m_initialMasking ) {
								case Settings.GUESS_INITIAL_MASKING.CUSTOM:
									m_masking = S.m_customMasking;
									break;
								case Settings.GUESS_INITIAL_MASKING.FIXED:
									m_masking = S.m_fixedMasking;
									break;
							}
						}

						_lobe.SetConstraint( 4, S.m_initialMasking == Settings.GUESS_INITIAL_MASKING.FIXED ? S.m_fixedMasking : 0.0,
												S.m_initialMasking == Settings.GUESS_INITIAL_MASKING.FIXED ? S.m_fixedMasking : 1.0 );
					}

					public void		Save( XmlElement _parent ) {
						Attrib( _parent, "theta", m_theta );
						Attrib( _parent, "roughness", m_roughness );
						Attrib( _parent, "scale", m_scale );
						Attrib( _parent, "flatten", m_flatten );
						Attrib( _parent, "masking", m_masking );
						Attrib( _parent, "totalReflectance", m_totalReflectance );
					}

					public void		Load( XmlElement _parent, int _version ) {
						Attrib( _parent, "theta", ref m_theta );
						Attrib( _parent, "roughness", ref m_roughness );
						Attrib( _parent, "scale", ref m_scale );
						Attrib( _parent, "flatten", ref m_flatten );
						Attrib( _parent, "masking", ref m_masking );
						Attrib( _parent, "totalReflectance", ref m_totalReflectance );
					}

					#region Analytical Expressions

					/// <summary>
					/// Computes the scale parameter analytically using the fitting we found using Mathematica
					/// </summary>
					/// <param name="_theta"></param>
					/// <param name="_roughness"></param>
					/// <returns></returns>
 					float	ComputeAnalyticalScaleParameter( float _theta, float _roughness, float _albedo ) {
						#if true
							// Here are the mathematica expressions giving us the scale parameter as a function of theta, roughness and albedo:
							// 	a(Subscript[\[Alpha], s])= 0.0576265 -1.84307 \[Alpha]+13.2655 \[Alpha]^2-9.1914 \[Alpha]^3
							// 	b(Subscript[\[Alpha], s])= -0.193265+14.4283 \[Alpha]-39.5737 \[Alpha]^2+22.0841 \[Alpha]^3
							// 	c(Subscript[\[Alpha], s])= 0.218714 -21.5808 \[Alpha]+57.0161 \[Alpha]^2-31.3305 \[Alpha]^3
							// 	d(Subscript[\[Alpha], s])=-0.0875285+10.4984 \[Alpha]-27.1654 \[Alpha]^2+14.6968 \[Alpha]^3
							// 
							// k(\[Rho]) = \[Rho]^2
							// 	
							// \[Sigma](\[Mu],Subscript[\[Sigma], s]) =a(Subscript[\[Alpha], s]) + b(Subscript[\[Alpha], s])\[Mu] + c(Subscript[\[Alpha], s]) \[Mu]^2 + d(Subscript[\[Alpha], s]) (\[Mu]^3)
							//
							double	mu = Math.Cos( _theta );
							double	r = _roughness;
							double	r2 = r * r;
							double	r3 = r2 * r;
							double	a =  0.057626522306966195 - 1.8430749623241764 * r + 13.265452228771144 * r2 - 9.1914044613068    * r3;
							double	b = -0.19326518084394056  + 14.428287204401842 * r - 39.57369023420125  * r2 + 22.084117767595018 * r3;
							double	c =  0.21871385093630663  - 21.580810315041887 * r + 57.01607335273474  * r2 - 31.33051654652553  * r3;
							double	d = -0.08752850960291476  + 10.498392018375801 * r - 27.165414679434377 * r2 + 14.696817709205297 * r3;

							double	f = a + b * mu + c * mu*mu + d * mu*mu*mu;
							return (float) f;
						#else
						// Here are the mathematica expressions giving us the scale parameter as a function of theta, roughness and albedo:
						// k(\[Rho]) = 1-2(1-\[Rho])+(1-\[Rho])^2
						// f( \[Sigma], \[Rho] ) = k(\[Rho]) [0.587595 +0.128391 (1-\[Alpha])+0.320232 (1-\[Alpha])^2-1.04001 (1-\[Alpha])^3]
						//
						_roughness = 1.0f - _roughness;
//						_albedo = 1.0f - _albedo;
//						double	k = 1 - 2 * _albedo + _albedo*_albedo;
						double	k = _albedo*_albedo;
						double	f = 0.587595 + 0.128391 * _roughness + 0.320232 * _roughness*_roughness - 1.04001 * _roughness*_roughness*_roughness;
								f *= k;	// Dependence on albedo
						return (float) f;
						#endif
					}

					/// <summary>
					/// Computes the flattening parameter analytically using the fitting we found using Mathematica
					/// </summary>
					/// <param name="_theta"></param>
					/// <param name="_roughness"></param>
					/// <returns></returns>
					float	ComputeAnalyticalFlattenParameter( float _theta, float _roughness ) {
						#if true
							// Here is the new updated mathematica expression for the flattening parameter:
							// a(Subscript[\[Alpha], s])= 0.885056 -1.21098 \[Alpha]+0.225698 \[Alpha]^2+0.449826 \[Alpha]^3
							// b(Subscript[\[Alpha], s])=0.0856807 +0.565903 \[Alpha]
							// c(Subscript[\[Alpha], s])= -0.0770746-1.38461 \[Alpha]+0.856589 \[Alpha]^2
							// d(Subscript[\[Alpha], s])=0.0104231 +0.852559 \[Alpha]-0.684474 \[Alpha]^2
							// fFlat[\[Mu]_, \[Alpha]_] = fA[\[Alpha]] + fB[\[Alpha]] \[Mu] + fC[\[Alpha]] \[Mu]^2 + fD[\[Alpha]] \[Mu]^3
							//
							double	mu = Math.Cos( _theta );
							double	r = _roughness;
							double	r2 = r * r;
							double	r3 = r2 * r;
							double	a = 0.8850557867448499    - 1.2109761138443194 * r + 0.22569832413951335 * r2 + 0.4498256199595464 * r3;
							double	b = 0.0856807009397115    + 0.5659031384072539 * r;
							double	c = -0.07707463071513312  - 1.384614678037336  * r + 0.8565888280926491  * r2;
							double	d = 0.010423083821992304  + 0.8525591060832015 * r - 0.6844738691665317  * r2;

							double	f = a + b * mu + c * mu*mu + d * mu*mu*mu;
							return (float) f;
						#else
							// Here are the mathematica expressions giving us the flattening parameter as a function of theta and roughness:
							// a(\[Rho]) = 0.697462  - 0.479278 (1-\[Rho])
							// b(\[Rho]) = 0.287646  - 0.293594 (1-\[Rho])
							// c(\[Rho]) = 5.69744  + 6.61321 (1-\[Rho])
							// \[Mu] = cos(\[Theta])
							// f( \[Mu], \[Rho] ) = a(\[Rho]) + b(\[Rho]) e^(-c(\[Rho])  \[Mu])
							//
							_roughness = 1.0f - _roughness;
							double	mu = Math.Cos( _theta );
							double	a = 0.697462 - 0.479278 * _roughness;
							double	b = 0.287646 - 0.293594 * _roughness;
							double	c = 5.697440 + 6.613210 * _roughness;
							double	f = a + b * Math.Exp( -c * mu );
							return (float) f;
						#endif
					}

					float	ComputeAnalyticalRoughnessParameter( float _roughness ) {
						#if true
							// Here is the new updated mathematica expression for the exponent parameter that is now only dependent on roughness:
							// roughness[\[Alpha]_] = 1 - 0.2686997865426857` \[Alpha] + 0.1535959296279097` \[Alpha]^2;
							double	roughness = 1.0 - 0.2686997865426857 * _roughness + 0.1535959296279097 * _roughness*_roughness;
							return (float) roughness;
						#else
							// Here are the mathematica expressions giving us the lobe roughness parameter as a function of surface roughness:
							// roughness[\[Alpha]_] = 0.9168937073335606 - 0.013091709358763996 \[Alpha]
							//
							double	roughness = 0.9168937073335606 - 0.013091709358763996 * _roughness;
							return (float) roughness;
						#endif
					}

					float	ComputeAnalyticalExponent( float _roughness ) {
						#if true
							// Here is the new updated mathematica expression for the exponent parameter that is now only dependent on roughness:
							// fEta[\[Alpha]_] = 2.588380909161985` \[Alpha] - 1.3549594389004276` \[Alpha]^2;
							double	exponent = 2.588380909161985 * _roughness - 1.3549594389004276 * _roughness*_roughness;
							return (float) exponent;
						#else
							// Here are the mathematica expressions giving us the exponent parameter as a function of roughness:
							// exponent[\[Alpha]_] = 0.7782894918463 + 0.1683172467667511 \[Alpha]
							//
							double	exponent = 0.7782894918463 + 0.1683172467667511 * _roughness;
							return (float) exponent;
						#endif
					}

					#endregion
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

				public string	UserText {
					get {
						return	"Theta = " + ((int) (180.0 * m_incomingAngleTheta / Math.PI)).ToString( "G3" ) + "°\n"
							+	"Roughness = " + m_surfaceRoughness.ToString( "G4" ) + "\n"
							+	"Albedo/F0 = " + m_surfaceAlbedoF0.ToString( "G4" ) + "\n"
							+	"\n"
							+ m_error;
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

				public bool		IsValid {
					get { return Math.Abs( m_state - 1.0f ) < 1e-4f && m_reflected.IsValid && (m_refracted.IsValid | m_owner.m_surface.m_type != TestForm.SURFACE_TYPE.DIELECTRIC); }
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

				/// <summary>
				/// Clears the result
				/// </summary>
				public void		Clear() {
					State = 0.0f;
					m_reflected.m_theta = -1.0;
					m_refracted.m_theta = -1.0;
				}

				public void		Save( XmlElement _parent ) {
					Attrib( _parent, "Index", "(" + m_X + "," + m_Y + "," + m_Z + ")" );
					Attrib( _parent, "State", m_state );
					Attrib( _parent, "Error", m_error != null ? m_error : "" );

					// Store surface parameters
					Attrib( _parent, "theta", m_incomingAngleTheta );
					Attrib( _parent, "phi", m_incomingAnglePhi );
					Attrib( _parent, "roughness", m_surfaceRoughness );
					Attrib( _parent, "albedoF0", m_surfaceAlbedoF0 );

					XmlElement	ElemReflected = AppendChild( _parent, "LobeReflected" );
					m_reflected.Save( ElemReflected );
					if ( m_owner.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
						XmlElement	ElemRefracted = AppendChild( _parent, "LobeRefracted" );
						m_refracted.Save( ElemRefracted );
					}
				}

				public void		Load( XmlElement _parent, int _version ) {
					Attrib( _parent, "State", ref m_state );
					m_error = Attrib( _parent, "Error" );
					if ( m_error == "" )
						m_error = null;

					// Store surface parameters
					Attrib( _parent, "theta", ref m_incomingAngleTheta );
					Attrib( _parent, "phi", ref m_incomingAnglePhi );
					Attrib( _parent, "roughness", ref m_surfaceRoughness );
					Attrib( _parent, "albedoF0", ref m_surfaceAlbedoF0 );

					XmlElement	ElemReflected = _parent["LobeReflected"];
					m_reflected.Load( ElemReflected, _version );
					if ( m_owner.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
						XmlElement	ElemRefracted = _parent["LobeRefracted"];
						if ( ElemRefracted != null )
							m_refracted.Load( ElemRefracted, _version );
					}
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

				float	incomingAngleMin, incomingAngleStep;
				m_surface.m_incomingAngle.BuildMinStep( out incomingAngleMin, out incomingAngleStep );
				float	roughnessMin, roughnessStep;
				m_surface.m_roughness.BuildMinStep( out roughnessMin, out roughnessStep );
				float	albedoF0Min, albedoF0Step;
				m_surface.m_albedoF0.BuildMinStep( out albedoF0Min, out albedoF0Step );

				m_results = new Result[orders][,,];
				for ( int order=0; order < orders; order++ ) {

					m_results[order] = new Result[dimX,dimY,dimZ];

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

			/// <summary>
			/// Clears the results
			/// </summary>
			public void		Clear() {
				for ( int order=0; order < m_results.Length; order++ ) {
					Result[,,]	orderResults = m_results[order];
					int	W = orderResults.GetLength(0);
					int	H = orderResults.GetLength(1);
					int	D = orderResults.GetLength(2);
					for ( int Z=0; Z < D; Z++ )
						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ ) {
								orderResults[X,Y,Z].Clear();
							}
				}
				m_surface.Unlock();
			}

			public void		Save( XmlDocument _doc ) {

				XmlElement	Root = AppendChild( _doc, "Root" );
				Attrib( Root, "Version", (int) 1 );

				XmlElement	ElmSurface = AppendChild( Root, "SurfaceParameters" );
				m_surface.Save( ElmSurface );

				XmlElement	ElmSettings = AppendChild( Root, "Settings" );
				m_settings.Save( ElmSettings );

				// Save results in an array
				XmlElement	ElmResults = AppendChild( Root, "Results" );

				int	orders = m_results.Length;
				Attrib( ElmResults, "OrdersCount", orders );

				int	minOrder = m_surface.ScatteringOrderMin;

				for ( int order=0; order < orders; order++ ) {
					XmlElement	ElmOrderResults = AppendChild( ElmResults, "Order" );
					Attrib( ElmOrderResults, "Index", minOrder + order );

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

				XmlElement	Root = _doc["Root"];

				int	version = 0;
				Attrib( Root, "Version", ref version );

				XmlElement	ElmSurface = Root["SurfaceParameters"];
				m_surface.Load( ElmSurface, version );
				InitializeResults();

				XmlElement	ElmSettings = Root["Settings"];
				m_settings.Load( ElmSettings, version );

				// Load results from an array
				XmlElement	ElmResults = Root["Results"];

				int	orders = 0;
				Attrib( ElmResults, "OrdersCount", ref orders );
				if ( orders != m_results.Length )
					throw new Exception( "Stored scattering orders count and size of results array mismatch!" );

				XmlElement	ElmOrderResults = ElmResults.FirstChild as XmlElement;
				while ( ElmOrderResults != null ) {
					if ( ElmOrderResults.Name != "Order" )
						throw new Exception( "Unexpected XmlElement: expected \"Order\" element but found \"" + ElmOrderResults.Name + "\" instead!" );

					int	orderIndex = 1;
					if ( !Attrib( ElmOrderResults, "Index", ref orderIndex ) )
						throw new Exception( "Failed to retrieve order index attribute! Can't assign results..." );

					Result[,,]	orderResults = GetResultsForOrder( orderIndex );
					if ( orderResults == null )
						throw new Exception( "Unsupported scattering order " + orderIndex + " (max is " + m_surface.ScatteringOrderMax + ")! Can't assign results..." );

					int	W = orderResults.GetLength(0);
					int	H = orderResults.GetLength(1);
					int	D = orderResults.GetLength(2);
					Attrib( ElmResults, "SizeX", ref W );
					Attrib( ElmResults, "SizeY", ref H );
					Attrib( ElmResults, "SizeZ", ref D );
					if ( W != orderResults.GetLength( 0 ) )
						throw new Exception( "Results length for incoming angle parameter mismatch!" );
					if ( H != orderResults.GetLength( 1 ) )
						throw new Exception( "Results length for roughness parameter mismatch!" );
					if ( D != orderResults.GetLength( 2 ) )
						throw new Exception( "Results length for albedo/F0 parameter mismatch!" );

					XmlElement	ElmResult = ElmOrderResults.FirstChild as XmlElement;
					for ( int Z=0; Z < D; Z++ )
						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ ) {
								if ( ElmResult == null )
									throw new Exception( "Unexpected end of array while reading results!" );
								if ( ElmResult.Name != "Result" )
									throw new Exception( "Unexpected XmlElement: expected \"Result\" element but found \"" + ElmResult.Name + "\" instead!" );

								orderResults[X,Y,Z].Load( ElmResult, version );

								ElmResult = ElmResult.NextSibling as XmlElement;
							}

					ElmOrderResults = ElmOrderResults.NextSibling as XmlElement;	// Next order...
				}
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

			static string		Attrib( XmlElement _parent, string _name ) {
				if ( !_parent.HasAttribute( _name ) )
					return null;

				return _parent.GetAttribute( _name );
			}

			static bool			Attrib( XmlElement _parent, string _name, ref bool _value ) {
				string	value = Attrib( _parent, _name );
				return value != null ? bool.TryParse( value, out _value ) : false;
			}

			static bool			Attrib( XmlElement _parent, string _name, ref int _value ) {
				string	value = Attrib( _parent, _name );
				return value != null ? int.TryParse( value, out _value ) : false;
			}

			static bool			Attrib( XmlElement _parent, string _name, ref float _value ) {
				string	value = Attrib( _parent, _name );
				return value != null ? float.TryParse( value, out _value ) : false;
			}

			static bool			Attrib( XmlElement _parent, string _name, ref double _value ) {
				string	value = Attrib( _parent, _name );
				return value != null ? double.TryParse( value, out _value ) : false;
			}

			static bool			Attrib<TEnum>( XmlElement _parent, string _name, ref TEnum _value ) where TEnum:struct {
				string	value = Attrib( _parent, _name );
				if ( value == null )
					return false;

				return Enum.TryParse( value, out _value );
			}

			#endregion
		}

		#endregion

		#region Multi Threading Nested Type

		/// <summary>
		/// Represents a computation thread working to fit a single lobe
		/// </summary>
		class	ComputationThread {

			AutomationForm					m_owner = null;
			int								m_index = 0;
			Thread							m_thread = null;
			bool							m_done = true;

			LobeModel						m_lobeModel = null;
			BFGS							m_fitter = new BFGS();

			int								m_maxIterations;
			double							m_goalTolerance;
			double							m_gradientTolerance;

			// The data to fit
			bool							m_fitBothLobes;
			double[,]						m_histogramReflected = null;
			double[,]						m_histogramTransmitted = null;

			// Current fitting data
			Document.Result					m_result = null;
			Document.Result.LobeParameters	m_parameters = null;
			bool							m_isReflectedLobe = true;

			// Feedback
			int								m_retriesCount = 0;
			float							m_progressSize = 1.0f;
			float							m_progressOffset = 0.0f;


			/// <summary>
			/// Gets the thread index
			/// </summary>
			public int			Index {
				get { return m_index; }
			}

			/// <summary>
			/// Gets or sets the Done state
			/// </summary>
			public bool			Done {
				get {
					lock( this ) {
						return m_done;
					}	
				}
				set {
					lock( this ) {
						m_done = value;
					}	
				}
			}

			/// <summary>
			/// Gets the lobe model used for the computation
			/// </summary>
			public LobeModel	LobeModel {
				get { return m_lobeModel; }
			}

			/// <summary>
			/// Gets the BFGS fitter used for the computation
			/// </summary>
			public BFGS			Fitter {
				get { return m_fitter; }
			}

			/// <summary>
			/// Gets the amount of retries used for the last fitted lobe
			/// </summary>
			public int			RetriesCount {
				get { return m_retriesCount; }
			}

			Document			Doc {
				get { return m_owner.m_document; }
			}

			/// <summary>
			/// Gets or sets the result we're trying to fit
			/// </summary>
			public Document.Result		Result {
				get { return m_result; }
				set { m_result = value; }
			}

			/// <summary>
			/// Tells if the thread should be used to fit both reflected and transmitted lobes
			/// </summary>
			public bool			FitBothLobes {
				get { return m_fitBothLobes; }
				set { m_fitBothLobes = value; }
			}

			public ComputationThread( AutomationForm _owner, int _index ) {
				m_owner = _owner;
				m_index = _index;

				// Initialize the lobe model
				m_lobeModel = new LobeModel();
				m_lobeModel.ParametersChanged += m_lobeModel_ParametersChanged;
			}

			/// <summary>
			/// Starts the lobe fitting
			/// </summary>
			public void	Start( int _maxIterations, double _goalTolerance, double _gradientTolerance, bool _startThread ) {
				m_maxIterations = _maxIterations;
				m_goalTolerance = _goalTolerance;
				m_gradientTolerance = _gradientTolerance;

				// Create the thread
				if ( _startThread ) {
					m_thread = new Thread( () => {
						Main();
					} );
					m_thread.Name = "Worker" + m_index;
					m_thread.Start();
				} else {
					Main();	// Simply run on the main thread...
				}
			}

			/// <summary>
			/// Main thread function
			/// </summary>
			void	Main() {
				m_progressSize = m_fitBothLobes ? 0.5f : 1.0f;

				try {
					// Fit reflected lobe...
					m_progressOffset = 0.0f;
					PrepareFitter( true, m_maxIterations, m_goalTolerance, m_gradientTolerance );
					PerformLobeFitting();

					m_owner.LogLine( m_index + ">	## Reflected lobe - Fit minimum reached = " + m_fitter.FunctionMinimum + " after " + ((m_retriesCount-1) * m_fitter.MaxIterations + m_fitter.IterationsCount) + " iterations (" + m_retriesCount + " attempts)" );

					// Fit refracted lobe now...
					if ( m_fitBothLobes ) {
						m_progressOffset = 0.5f;
						PrepareFitter( false, m_maxIterations, m_goalTolerance, m_gradientTolerance );
						PerformLobeFitting();

						m_owner.LogLine( m_index + ">	## Refracted lobe - Fit minimum reached = " + m_fitter.FunctionMinimum + " after " + ((m_retriesCount-1) * m_fitter.MaxIterations + m_fitter.IterationsCount) + " iterations (" + m_retriesCount + " attempts)" );
					}

					m_result.State = 1.0f;	// Whatever, we're done now!

				} catch ( CanceledException ) {
					m_result.m_error = "Canceled";
					m_result.State = -1.0f;
				} catch ( Exception _e ) {
					m_result.m_error = _e.Message;
					m_result.State = -1.0f;
				} finally {
					m_thread = null;
					Done = true;
				}
			}

			#region Automation Core

			/// <summary>
			/// Initializes the target data we need to fit from the CPU-readable histogram texture
			/// </summary>
			public void	InitializeLobeTargetData() {
				// Read back histogram to CPU for fitting
				m_histogramReflected = m_owner.m_owner.GetSimulationHistogram( true, (uint) m_result.ScatteringOrder );
				m_histogramTransmitted = m_fitBothLobes ? m_owner.m_owner.GetSimulationHistogram( false, (uint) m_result.ScatteringOrder ) : null;	// Also read back transmitted lobe
			}

			/// <summary>
			/// Prepare the fitter for a new result
			/// </summary>
			public void	PrepareFitter( bool _fitReflectedLobe, int _maxIterations, double _goalTolerance, double _gradientTolerance ) {

				m_isReflectedLobe = _fitReflectedLobe;
				m_parameters = m_isReflectedLobe ? m_result.m_reflected : m_result.m_refracted;

				double	functionMinimumTolerance = Math.Pow( 10.0, Doc.m_settings.m_logTolerance_Minimum );
				double	gradientTolerance = Math.Pow( 10.0, Doc.m_settings.m_logTolerance_Gradient );

				m_fitter.MaxIterations = Doc.m_settings.m_maxIterations;
				m_fitter.SuccessTolerance = functionMinimumTolerance;
				m_fitter.GradientSuccessTolerance = gradientTolerance;

				// Initialize lobe data & compute center of mass
				double[,]	targetHistogram = m_isReflectedLobe ? m_histogramReflected : m_histogramTransmitted;
				m_lobeModel.InitTargetData( targetHistogram );

				// BEGIN: Patapom (18/04/18) Compute total reflectance
				m_parameters.m_totalReflectance = 0.0;
				int	W = targetHistogram.GetLength( 0 );
				int	H = targetHistogram.GetLength( 1 );
				for ( int Y=0; Y < H; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						m_parameters.m_totalReflectance += targetHistogram[X,Y];
					}
				}
				// END: Patapom (18/04/18)
			}

			/// <summary>
			/// Perform lobe fitting of the simulated data (core routine)
			/// </summary>
			/// <param name="_result"></param>
			/// <param name="_reflected"></param>
			public void	PerformLobeFitting() {
				if ( m_maxIterations == 0 )
					return;

				// Initialize preliminary lobe results
				float3				incomingDirection = m_result.IncomingDirection;

				float3				reflectedDirection = incomingDirection;
									reflectedDirection.z = -reflectedDirection.z;	// Mirror against surface

				float3				refractedDirection = TestForm.Refract( -incomingDirection, float3.UnitZ, 1.0f / TestForm.Fresnel_IORFromF0( m_result.SurfaceAlbedoF0 ) );

				m_parameters.Initialize( m_result, m_lobeModel, m_isReflectedLobe ? reflectedDirection : refractedDirection );

				// Initialize lobe model using initialized lobe results
				m_lobeModel.InitLobeData( Doc.m_settings.m_lobeModel, incomingDirection, m_parameters.m_theta, m_parameters.m_roughness, m_parameters.m_scale, m_parameters.m_flatten, m_parameters.m_masking, Doc.m_settings.m_oversizeFactor, true );

				// Peform fitting
				for ( m_retriesCount=0; m_retriesCount < Doc.m_settings.m_maxRetries; m_retriesCount++ ) {
					m_fitter.Minimize( m_lobeModel );
					if ( m_fitter.IterationsCount < Doc.m_settings.m_maxIterations ) {
						// Finished!
						m_retriesCount++;
						break;
					}
				}
			}

			#endregion

			#region EVENT HANDLERS

			void m_lobeModel_ParametersChanged( double[] _parameters ) {
				if ( m_owner.m_canceled ) {
					throw new CanceledException();
				}

				// Store new parameters
				m_parameters.m_theta = _parameters[0];
				m_parameters.m_roughness = _parameters[1];
				m_parameters.m_scale = _parameters[2];
				m_parameters.m_flatten = _parameters[3];
				m_parameters.m_masking = _parameters[4];

				// Update progress
				m_result.State = m_progressOffset + m_progressSize * (m_retriesCount + (float) m_fitter.IterationsCount / m_fitter.MaxIterations) / Doc.m_settings.m_maxRetries;

				if ( m_index == 0 ) {
					m_owner.UpdateUI( m_result, m_isReflectedLobe );	// Only thread 0 can update the UI!
				}
			}

			#endregion
		}

		#endregion

		#region FIELDS

		TestForm			m_owner;

		RegistryKey			m_AppKey;

		ComputationThread[]	m_threads = new ComputationThread[THREADS_COUNT];

		FileInfo			m_documentFileName = null;
		Document			m_document = null;
		Document.Result		m_selectedResult = null;		// Current selection

		#endregion

		#region PROPERTIES

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

				integerTrackbarControlViewScatteringOrder.Value = m_selectedResult.ScatteringOrder;
				integerTrackbarControlViewAlbedoSlice.Value = m_selectedResult.Z;

				// Show lobe parameters in main form
				m_owner.UpdateLobeParameters( m_selectedResult.m_reflected.AsArray, true );
				m_owner.UpdateLobeParameters( m_selectedResult.m_refracted.AsArray, false );
				m_owner.UpdateSurfaceParameters( m_selectedResult.ScatteringOrder, m_selectedResult.IncomingDirection, m_selectedResult.SurfaceRoughness, m_selectedResult.SurfaceAlbedoF0, !m_computing );
			}
		}

		/// <summary>
		/// Use the view trackbar as the official selected scattering order
		/// </summary>
		int				SelectedScatteringOrder {
			get { return integerTrackbarControlViewScatteringOrder.Value; }
			set { integerTrackbarControlViewScatteringOrder.Value = value; }
		}

		FileInfo				DocumentFileName {
			get { return m_documentFileName; }
			set {
				m_documentFileName = value;
				Text = "Automation Form" + (m_documentFileName != null ? " - " + m_documentFileName.FullName : "");
			}
		}

		TestForm.SURFACE_TYPE	SurfaceType {
			get {
				return radioButtonSurfaceTypeConductor.Checked ? TestForm.SURFACE_TYPE.CONDUCTOR : (radioButtonSurfaceTypeDielectric.Checked ? TestForm.SURFACE_TYPE.DIELECTRIC : TestForm.SURFACE_TYPE.DIFFUSE);
			}
		}

		#endregion

		public AutomationForm() {
			InitializeComponent();

			m_AppKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Patapom\MSBRDF" );

			// Create the computation threads
			for ( int i=0; i < THREADS_COUNT; i++ )
				m_threads[i] = new ComputationThread( this, i );

			// Attach a default document
			AttachDocument( new Document() );
		}

		/// <summary>
		/// Recomputes the surface heightfield for the specified roughness
		/// </summary>
		/// <param name="_result"></param>
		void	UpdateSurfaceRoughness( float _surfaceRoughness ) {
			m_owner.BuildBeckmannSurfaceTexture( _surfaceRoughness );
		}

		/// <summary>
		/// Simulates incoming rays on surface (core routine)
		/// </summary>
		void	Simulate( float _theta, float _phi, float _surfaceRoughness, float _albedoF0 ) {
			m_owner.RayTraceSurface( _surfaceRoughness, _albedoF0, m_document.m_surface.m_type, _theta, _phi, m_document.m_surface.m_rayTracingIterationsCount );
		}

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

			TestForm.SURFACE_TYPE	actualType = m_document.m_surface.m_type;
			radioButtonSurfaceTypeConductor.Checked = true;	// Make sure we change it and assign actual value again so main form is notified
			switch ( actualType ) {
				case TestForm.SURFACE_TYPE.CONDUCTOR: radioButtonSurfaceTypeConductor.Checked = true; break;
				case TestForm.SURFACE_TYPE.DIELECTRIC: radioButtonSurfaceTypeDielectric.Checked = true; break;
				case TestForm.SURFACE_TYPE.DIFFUSE: radioButtonSurfaceTypeDiffuse.Checked = true; break;
			}

			groupBoxSimulationParameters.Enabled = !m_document.m_surface.IsLocked;	// Most important!

			floatTrackbarControlParam0_Min.Value = m_document.m_surface.m_incomingAngle.Min;
			floatTrackbarControlParam0_Max.Value = m_document.m_surface.m_incomingAngle.Max;
			integerTrackbarControlParam0_Steps.Value = m_document.m_surface.m_incomingAngle.StepsCount;
			floatTrackbarControlParam1_Min.Value = m_document.m_surface.m_roughness.Min;
			floatTrackbarControlParam1_Max.Value = m_document.m_surface.m_roughness.Max;
			integerTrackbarControlParam1_Steps.Value = m_document.m_surface.m_roughness.StepsCount;
			floatTrackbarControlParam2_Min.Value = m_document.m_surface.m_albedoF0.Min;
			floatTrackbarControlParam2_Max.Value = m_document.m_surface.m_albedoF0.Max;
			integerTrackbarControlParam2_Steps.Value = m_document.m_surface.m_albedoF0.StepsCount;
			integerTrackbarControlRayCastingIterations.Value = m_document.m_surface.m_rayTracingIterationsCount;
			checkBoxParam0_InclusiveStart.Checked = m_document.m_surface.m_incomingAngle.InclusiveMin;
			checkBoxParam0_InclusiveEnd.Checked = m_document.m_surface.m_incomingAngle.InclusiveMax;
			checkBoxParam1_InclusiveStart.Checked = m_document.m_surface.m_roughness.InclusiveMin;
			checkBoxParam1_InclusiveEnd.Checked = m_document.m_surface.m_roughness.InclusiveMax;
			checkBoxParam2_InclusiveStart.Checked = m_document.m_surface.m_albedoF0.InclusiveMin;
			checkBoxParam2_InclusiveEnd.Checked = m_document.m_surface.m_albedoF0.InclusiveMax;

			// Update range sliders
			integerTrackbarControlViewAlbedoSlice.RangeMax = m_document.m_surface.m_albedoF0.StepsCount - 1;
			integerTrackbarControlViewAlbedoSlice.VisibleRangeMax = integerTrackbarControlViewAlbedoSlice.RangeMax;

			integerTrackbarControlScatteringOrder_Min.RangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlScatteringOrder_Min.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlScatteringOrder_Min.RangeMax = m_document.m_surface.ScatteringOrderMax;	// Min scattering can't go higher than this
			integerTrackbarControlScatteringOrder_Min.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlScatteringOrder_Min.Value = m_document.m_surface.ScatteringOrderMin;

			integerTrackbarControlScatteringOrder_Max.RangeMin = m_document.m_surface.ScatteringOrderMin;	// Max scattering can't go lower than this
			integerTrackbarControlScatteringOrder_Max.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlScatteringOrder_Max.RangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlScatteringOrder_Max.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlScatteringOrder_Max.Value = m_document.m_surface.ScatteringOrderMax;

			integerTrackbarControlViewScatteringOrder.RangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlViewScatteringOrder.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
			integerTrackbarControlViewScatteringOrder.RangeMax = m_document.m_surface.ScatteringOrderMax;
			integerTrackbarControlViewScatteringOrder.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
		}

		/// <summary>
		/// Mirrors the document's settings to the UI
		/// </summary>
		void	DocumentSettings2UI() {

			switch ( m_document.m_settings.m_lobeModel ) {
				case LobeModel.LOBE_TYPE.MODIFIED_PHONG: radioButtonLobe_ModifiedPhong.Checked = true; break;
				case LobeModel.LOBE_TYPE.MODIFIED_PHONG_ANISOTROPIC: radioButtonLobe_ModifiedPhongAniso.Checked = true; break;
				case LobeModel.LOBE_TYPE.BECKMANN: radioButtonLobe_Beckmann.Checked = true; break;
				case LobeModel.LOBE_TYPE.GGX: radioButtonLobe_GGX.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialDirection ) {
				case Document.Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS: radioButtonInitDirection_TowardCoM.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION: radioButtonInitDirection_TowardReflected.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_DIRECTION.NO_CHANGE: radioButtonInitDirection_NoChange.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_DIRECTION.FIXED: radioButtonInitDirection_Fixed.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialRoughness ) {
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.SURFACE: radioButtonInitRoughness_UseSurface.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM: radioButtonInitRoughness_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.NO_CHANGE: radioButtonInitRoughness_NoChange.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.FIXED: radioButtonInitRoughness_Fixed.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_ROUGHNESS.ANALYTICAL: radioButtonInitRoughness_Analytical.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialScale ) {
				case Document.Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS: radioButtonInitScale_CoMFactor.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_SCALE.NO_CHANGE: radioButtonInitScale_NoChange.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_SCALE.FIXED: radioButtonInitScale_Fixed.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_SCALE.ANALYTICAL: radioButtonInitScale_Analytical.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialFlatten ) {
				case Document.Settings.GUESS_INITIAL_FLATTEN.CUSTOM: radioButtonInitFlatten_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_FLATTEN.NO_CHANGE: radioButtonInitFlatten_NoChange.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_FLATTEN.FIXED: radioButtonInitFlatten_Fixed.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_FLATTEN.ANALYTICAL: radioButtonInitFlatten_Analytical.Checked = true; break;
			}

			switch ( m_document.m_settings.m_initialMasking ) {
				case Document.Settings.GUESS_INITIAL_MASKING.CUSTOM: radioButtonInitMasking_Custom.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_MASKING.NO_CHANGE: radioButtonInitMasking_NoChange.Checked = true; break;
				case Document.Settings.GUESS_INITIAL_MASKING.FIXED: radioButtonInitMasking_Fixed.Checked = true; break;
			}

			checkBoxInitDirection_Inherit.Checked = m_document.m_settings.m_inheritDirection_Top;
			checkBoxInitScale_Inherit.Checked = m_document.m_settings.m_inheritScale_Top;
			checkBoxInitFlatten_Inherit.Checked = m_document.m_settings.m_inheritFlatten_Top;
			checkBoxInitRoughness_Inherit.Checked = m_document.m_settings.m_inheritRoughness_Top;
			checkBoxInitMasking_Inherit.Checked = m_document.m_settings.m_inheritMasking_Top;
			checkBoxInitDirection_InheritLeft.Checked = m_document.m_settings.m_inheritDirection_Left;
			checkBoxInitScale_InheritLeft.Checked = m_document.m_settings.m_inheritScale_Left;
			checkBoxInitFlatten_InheritLeft.Checked = m_document.m_settings.m_inheritFlatten_Left;
			checkBoxInitRoughness_InheritLeft.Checked = m_document.m_settings.m_inheritRoughness_Left;
			checkBoxInitMasking_InheritLeft.Checked = m_document.m_settings.m_inheritMasking_Left;
			floatTrackbarControlInit_Scale.Value = m_document.m_settings.m_customScale;
			floatTrackbarControlInit_CustomFlatten.Value = m_document.m_settings.m_customFlatten;
			floatTrackbarControlInit_CustomRoughness.Value = m_document.m_settings.m_customRoughness;
			floatTrackbarControlInit_CustomMaskingImportance.Value = m_document.m_settings.m_customMasking;
			floatTrackbarControlInit_FixedDirection.Value = m_document.m_settings.m_fixedTheta * 180.0f / (float) Math.PI;
			floatTrackbarControlInit_FixedRoughness.Value = m_document.m_settings.m_fixedRoughness;
			floatTrackbarControlInit_FixedScale.Value = m_document.m_settings.m_fixedScale;
			floatTrackbarControlInit_FixedFlatten.Value = m_document.m_settings.m_fixedFlatten;
			floatTrackbarControlInit_FixedMasking.Value = m_document.m_settings.m_fixedMasking;
		}

		/// <summary>
		/// Mirrors the document's lobe settings to the UI
		/// </summary>
		void	DocumentLobeSettings2UI() {
			checkBoxSkipSimulation.Checked = !m_document.m_settings.m_performFitting;
			integerTrackbarControlMaxIterations.Value = m_document.m_settings.m_maxIterations;
			floatTrackbarControlGoalTolerance.Value = m_document.m_settings.m_logTolerance_Minimum;
			floatTrackbarControlGradientTolerance.Value = m_document.m_settings.m_logTolerance_Gradient;
			integerTrackbarControlRetries.Value = m_document.m_settings.m_maxRetries;
			floatTrackbarControlFitOversize.Value = m_document.m_settings.m_oversizeFactor;
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

			m_selectedResult = layerResults[0,0,0];

			for ( int Z=0; Z < stepsCountZ; Z++ )
				for ( int Y=0; Y < stepsCountY; Y++ )
					for ( int X=0; X < stepsCountX; X++ ) {
						Document.Result	result = layerResults[X,Y,Z];
						completionArrayControl.SetState( X, Y, Z, result.State, result.UserText );
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

			if ( _result.ScatteringOrder == integerTrackbarControlViewScatteringOrder.Value )
				completionArrayControl.SetState( _result.X, _result.Y, _result.Z, _result.State, _result.UserText );	// Only update UI if it's showing the result's scattering order
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
		void	LogLine( string _text ) {
			Log( _text + "\r\n" );
		}
		void	Log( string _text ) {
			if ( InvokeRequired ) {
				BeginInvoke( (Action) (() => {
					Log( _text );
				} ) );
				return;
			}

			m_log.Add( _text );
			textBoxLog.AppendText( _text );
			textBoxLog.Refresh();
		}

		string	FormatTime( DateTime _time ) {
			return _time.ToString( @"hh\:mm\:ss" );
		}

		string	FormatDuration( TimeSpan _duration ) {
			return _duration.ToString( @"hh\:mm\:ss" ) + ":" + _duration.Milliseconds.ToString( "G03" );
		}

		#endregion

		bool		m_computing = false;
		bool		m_canceled = false;
		void		EnterComputationMode() {
			m_computing = true;
			m_canceled = false;
			m_owner.FittingMode = true;
			buttonCompute.Text = "Cancel";
			buttonCompute.BackColor = Color.IndianRed;

			groupBoxAnalyticalLobeModel.Enabled = false;
			groupBoxLobeFitterConfig.Enabled = false;
			completionArrayControl.Enabled = false;		// Musn't change selection!
			integerTrackbarControlViewScatteringOrder.Enabled = false;
			integerTrackbarControlViewAlbedoSlice.Enabled = false;

			// Lock surface, we can't change dimensions now that we're started!
			if ( !m_document.m_surface.IsLocked )
				m_document.m_surface.Lock();
		}

		void		ExitComputationMode() {
			m_computing = false;
			m_owner.FittingMode = false;
			buttonCompute.Text = "Start";
			buttonCompute.BackColor = Color.MediumAquamarine;

			groupBoxAnalyticalLobeModel.Enabled = true;
			groupBoxLobeFitterConfig.Enabled = true;
			completionArrayControl.Enabled = true;
			integerTrackbarControlViewScatteringOrder.Enabled = true;
			integerTrackbarControlViewAlbedoSlice.Enabled = true;
		}

		/// <summary>
		/// Checks if there is a nested CancelException in the provided exception
		/// </summary>
		/// <param name="_e"></param>
		/// <returns></returns>
		bool	IsNestedCanceledException( Exception _e ) {
			if ( _e is CanceledException )
				return true;

			return _e.InnerException != null ? IsNestedCanceledException( _e.InnerException ) : false;
		}

		/// <summary>
		/// Called by thread 0 when new parameters/states are available
		/// </summary>
		void		UpdateUI( Document.Result _result, bool _isReflectedLobe ) {
			if ( InvokeRequired ) {
				BeginInvoke( (Action) (() => { UpdateUI( _result, _isReflectedLobe ); }) );	// Repost for main thread
				return;
			}

			Document.Result.LobeParameters	parameters = _isReflectedLobe ? _result.m_reflected : _result.m_refracted;
			m_owner.UpdateLobeParameters( parameters.AsArray, _isReflectedLobe );		// Update 3D rendering
			integerTrackbarControlViewScatteringOrder.Value = _result.ScatteringOrder;	// Update scattering order
			completionArrayControl.Select( _result.X, _result.Y, _result.Z );			// Update selected result
		}

		DateTime	m_computationStart;
		DateTime	m_computationEnd;
		DateTime	m_simulationStart;
		DateTime	m_simulationEnd;
		private void buttonCompute_Click( object sender, EventArgs e ) {
			if ( m_computing ) {
				m_canceled = true;	// Raising that flag will throw the exception in the main thread as soon as possible
				return;
			}

			if ( m_documentFileName == null ) {
				string	FileName = AskForFileName();
				if ( FileName == null ) {
					MessageBox( "You cannot start a computation without specifying a filename otherwise auto-save feature won't be available and you might lose your results if a crash occurs during automation!" );
					return;
				}
				m_documentFileName = new FileInfo( FileName );
			}

			ComputeAll( 0, 0, 0, 0, false );
		}

		/// <summary>
		/// Tries to return an idle thread
		/// </summary>
		/// <returns></returns>
		ComputationThread	QueryComputeThread() {
			int	threadsCount = Math.Min( m_threads.Length, integerTrackbarControlThreadsCount.Value );
			for ( int i=0; i < threadsCount; i++ ) {
				if ( m_threads[i].Done ) {
					m_threads[i].Done = false;
					return m_threads[i];
				}
			}

			return null;
		}

		/// <summary>
		/// Waits for all threads to terminate
		/// </summary>
		void	WaitForAllThreads() {
			bool	allThreadsDone = false;
			while ( !allThreadsDone ) {
				allThreadsDone = true;
				for ( int i=0; i < m_threads.Length; i++ ) {
					if ( !m_threads[i].Done ) {
						allThreadsDone = false;
						break;
					}
				}
				if ( !allThreadsDone )
					Thread.Sleep( 50 );
			}
		}


		/// <summary>
		/// Main computation routine
		/// </summary>
		void	ComputeAll( int _startOrder, int _startX, int _startY, int _startZ, bool _singleSlice ) {
			try {
				EnterComputationMode();
				ClearLog();

				int	orderMax = m_document.m_surface.ScatteringOrderMax;
				int	orderMin = _startOrder > 0 && _startOrder <= orderMax ? _startOrder : m_document.m_surface.ScatteringOrderMin;
				if ( _singleSlice )
					orderMax = orderMin;	// Single order if single slice...

				int	dimX = m_document.m_surface.m_incomingAngle.StepsCount;
				int	dimY = m_document.m_surface.m_roughness.StepsCount;
				int	dimZ = m_document.m_surface.m_albedoF0.StepsCount;

				_startX = Math.Max( 0, Math.Min( dimX-1, _startX ) );
				_startY = Math.Max( 0, Math.Min( dimY-1, _startY ) );
				_startZ = Math.Max( 0, Math.Min( dimZ-1, _startZ ) );
				int	endZ = _singleSlice ? _startZ+1 : dimZ;

				double	functionMinimumTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Minimum );
				double	gradientTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Gradient );

				m_computationStart = DateTime.Now;

				LogLine( "########## Computation Started ##########" );
				LogLine( "	• Start Time = " + FormatTime( m_computationStart ) );
				LogLine( "########################################" );
				LogLine( "" );
				LogLine( "" );

				int	runCounter = 0;

				m_owner.SetCurrentScatteringOrder( SelectedScatteringOrder );	// Show the selected order...

				for ( int Z=_startZ; Z < endZ; Z++ ) {
					_startZ = 0;
					for ( int Y=_startY; Y < dimY; Y++ ) {
						_startY = 0;
						for ( int X=_startX; X < dimX; X++ ) {
							_startX = 0;

							// Check if all results from all orders are valid
							bool	allResultsValid = true;
							for ( int order=orderMin; order <= orderMax; order++ ) {
								Document.Result[,,]	results = m_document.GetResultsForOrder( order );

								if ( !results[X,Y,Z].IsValid ) {
									allResultsValid = false;	// That result is not valid! Worth a computation...
									break;
								}
							}
							if ( allResultsValid )
								continue;	// Already valid, no need to recompute...


							//////////////////////////////////////////////////////////////////////////
							// Perform simulation only once
							m_simulationStart = DateTime.Now;

							Document.Result	sampleResult = m_document.m_results[0][X,Y,Z];	// This is just a sample result to retrieve the current parameters we're simulating

							LogLine( " == Starting Simulation (" + X + ", " + Y + ", " + Z + ") ==" );
							LogLine( "	• Angle = " + (sampleResult.IncomingAngleTheta * 180.0 / Math.PI).ToString( "G3" ) + " - Roughness = " + sampleResult.SurfaceRoughness.ToString( "G3" ) + " - " + (m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0") + " = " + sampleResult.SurfaceAlbedoF0.ToString( "G3" ) );
							LogLine( "	• Start Time = " + FormatTime( m_simulationStart ) );

							UpdateSurfaceRoughness( sampleResult.SurfaceRoughness );
							Simulate( sampleResult.IncomingAngleTheta, sampleResult.IncomingAnglePhi, sampleResult.SurfaceRoughness, sampleResult.SurfaceAlbedoF0 );

							m_simulationEnd = DateTime.Now;
							TimeSpan	simulationDuration = m_simulationEnd - m_simulationStart;
							LogLine( "	• Simulation Duration = " + FormatDuration( simulationDuration ) );


							//////////////////////////////////////////////////////////////////////////
							// Fit lobes for all orders
							for ( int order=orderMin; order <= orderMax; order++ ) {
								Document.Result[,,]	results = m_document.GetResultsForOrder( order );
								Document.Result		R = results[X,Y,Z];

								if ( R.IsValid )
									continue;	// No need to recompute that one

								// Reset state
								R.m_error = null;
								R.State = 0.0f;

								// Query a free computation thread for fitting
								ComputationThread	T = null;
								while ( T == null ) {
									T = QueryComputeThread();
									Application.DoEvents();				// Give a chance to the app to process messages!
									if ( m_canceled )
										throw new CanceledException();	// Was the cancel button pushed?
									if ( T == null ) {
										Thread.Sleep( 50 );
									}
								}

								// Perform fitting on working thread
								T.Result = R;
								T.FitBothLobes = m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC;
								T.InitializeLobeTargetData();
								T.Start( checkBoxSkipSimulation.Checked ? 0 : m_document.m_settings.m_maxIterations, functionMinimumTolerance, gradientTolerance, true );

								// Auto-save
								runCounter++;
								if ( runCounter > AUTO_SAVE_EVERY_N_RUNS ) {
									runCounter = 0;
									saveToolStripMenuItem_Click( null, EventArgs.Empty );
								}
							}
						}
					}
				}

			} catch ( CanceledException ) {
				m_canceled = true;
				LogLine( "Canceled!" );
			} catch ( Exception _e ) {
				string	errorText = "An error occurred during fitting: " + _e.Message;
				LogLine( errorText );
				MessageBox( errorText );
			} finally {

				WaitForAllThreads();

				m_computationEnd = DateTime.Now;

				TimeSpan	duration = m_computationEnd - m_computationStart;

				LogLine( "" );
				LogLine( "" );
				LogLine( "########################################" );
				LogLine( "	• End Time = " + FormatTime( m_computationEnd ) + " (Duration: " + FormatDuration( duration ) + ")" );
				LogLine( "########## Computation Ended ##########" );
				LogLine( "" );
				LogLine( "" );

				// Done!
				ExitComputationMode();
			}
		}

		/// <summary>
		/// Computes a single value
		/// </summary>
		void	ComputeSelectedResult() {
			ComputationThread	T = null;
			try {
				EnterComputationMode();

				ClearLog();

				m_simulationStart = DateTime.Now;
				SelectedResult.m_error = null;
				SelectedResult.State = 0.0f;

				T = QueryComputeThread();
				if ( T == null )
					throw new Exception( "No available idle thread for computation!" );

				double	functionMinimumTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Minimum );
				double	gradientTolerance = Math.Pow( 10.0, m_document.m_settings.m_logTolerance_Gradient );

				m_owner.SetCurrentScatteringOrder( SelectedResult.ScatteringOrder );

				LogLine( "== Starting Order " + SelectedResult.ScatteringOrder + " (" + SelectedResult.X + ", " + SelectedResult.Y + ", " + SelectedResult.Z + ") ==" );
				LogLine( "	• Angle = " + (SelectedResult.IncomingAngleTheta * 180.0 / Math.PI).ToString( "G3" ) + " - Roughness = " + SelectedResult.SurfaceRoughness.ToString( "G3" ) + " - " + (m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0") + " = " + SelectedResult.SurfaceAlbedoF0.ToString( "G3" ) );
				LogLine( "	• Start Time = " + FormatTime( m_simulationStart ) );

				UpdateSurfaceRoughness( SelectedResult.SurfaceRoughness );
				Simulate( SelectedResult.IncomingAngleTheta, SelectedResult.IncomingAnglePhi, SelectedResult.SurfaceRoughness, SelectedResult.SurfaceAlbedoF0 );

				DateTime	simulationEnd = DateTime.Now;
				TimeSpan	simulationDuration = simulationEnd - m_simulationStart;
				LogLine( "	• Simulation Duration = " + FormatDuration( simulationDuration ) );


				// Fit reflected lobe...
				T.Result = SelectedResult;
				T.FitBothLobes = m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC;
				T.InitializeLobeTargetData();
				T.Start( checkBoxSkipSimulation.Checked ? 0 : m_document.m_settings.m_maxIterations, functionMinimumTolerance, gradientTolerance, false );	// Start on the main thread here...

			} catch ( CanceledException ) {
				LogLine( "Canceled!" );
				SelectedResult.m_error = "Canceled";
				SelectedResult.State = -1.0f;
			} catch ( Exception _e ) {
				string	errorText = "An error occurred during fitting: " + _e.Message;
				LogLine( errorText );
				SelectedResult.m_error = errorText;
				SelectedResult.State = -1.0f;
				MessageBox( errorText );
			} finally {

				if ( T != null )
					T.Done = true;	// Available again!

				m_simulationEnd = DateTime.Now;

				TimeSpan	duration = m_simulationEnd - m_simulationStart;
				LogLine( "	• End Time = " + FormatTime( m_simulationEnd ) + " (Duration: " + FormatDuration( duration ) + ")" );
				LogLine( "== Fitting Ended ==" );
				LogLine( "" );
				LogLine( "" );

				// Done!
				ExitComputationMode();
			}
		}

		#region Menu

		private void newToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( m_document.m_surface.IsLocked && MessageBox( "Are you sure you want to start a new document and lose existing results?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;

			DocumentFileName = null;
			AttachDocument( new Document() );
		}

		string	AskForFileName() {
			string	fileName = m_AppKey.GetValue( "LastDocFileName", new System.IO.FileInfo( "results.xml" ).FullName ) as string;
			saveFileDialogResults.FileName = Path.GetFileName( fileName );
			saveFileDialogResults.InitialDirectory = Path.GetDirectoryName( fileName );
			if ( saveFileDialogResults.ShowDialog( this ) != DialogResult.OK )
				return null;

			return saveFileDialogResults.FileName;
		}

		private void saveToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( m_documentFileName == null ) {
				string	FileName = AskForFileName();
				if ( FileName == null )
					return;
				DocumentFileName = new FileInfo( FileName );
			}

			try {

				if ( File.Exists( m_documentFileName.FullName ) ) {
					string	BackupFileName = m_documentFileName.FullName + ".bak";
					File.Delete( BackupFileName );
					File.Copy( m_documentFileName.FullName, BackupFileName );	// Backup first...
				}

				XmlDocument	Doc = new XmlDocument();
				m_document.Save( Doc );

				Doc.Save( m_documentFileName.FullName );

				m_AppKey.SetValue( "LastDocFileName", m_documentFileName.FullName );

				if ( !m_computing )
					MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				if ( m_computing )
					throw _e;
				MessageBox( "An error occurred while saving results:\r\n" + _e );
			}
		}

		private void saveAsToolStripMenuItem_Click( object sender, EventArgs e ) {
			string	fileName = AskForFileName();
			if ( fileName == null )
				return;

			DocumentFileName = new FileInfo( fileName );
			saveToolStripMenuItem_Click( sender, e );
		}

		private void openToolStripMenuItem_Click( object sender, EventArgs e ) {
			try {
				string	fileName = m_AppKey.GetValue( "LastDocFileName", new System.IO.FileInfo( "results.xml" ).FullName ) as string;
				openFileDialogResults.FileName = Path.GetFileName( fileName );
				openFileDialogResults.InitialDirectory = Path.GetDirectoryName( fileName );
				if ( openFileDialogResults.ShowDialog( this ) != DialogResult.OK )
					return;

				fileName = openFileDialogResults.FileName;

				XmlDocument	XmlDoc = new XmlDocument();
				XmlDoc.Load( fileName );
				Document	Doc = new Document();
				Doc.Load( XmlDoc );

				DocumentFileName = new FileInfo( fileName );
				AttachDocument( Doc );

				m_AppKey.SetValue( "LastDocFileName", openFileDialogResults.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving results:\r\n" + _e );
			}
		}

		private void exportToolStripMenuItem_Click( object sender, EventArgs e ) {
			string	fileName = m_AppKey.GetValue( "LastExportFileName", new System.IO.FileInfo( "results.bin" ).FullName ) as string;
			saveFileDialogExport.FileName = Path.GetFileName( fileName );
			saveFileDialogExport.InitialDirectory = Path.GetDirectoryName( fileName );
			if ( saveFileDialogExport.ShowDialog( this ) != DialogResult.OK )
				return;

			try {

				fileName = saveFileDialogExport.FileName;
				string	directory = Path.GetDirectoryName( fileName );
				string	fileNameNoExt = Path.GetFileNameWithoutExtension( fileName );
				string	extension = Path.GetExtension( fileName );

				//@TODO: Support extended export formats
				for ( int order=0; order < m_document.m_results.Length; order++ ) {
					Document.Result[,,]	results = m_document.m_results[order];

					int	W = results.GetLength( 0 );
					int	H = results.GetLength( 1 );
					int	slicesCount = results.GetLength( 2 );
					for ( int sliceIndex=0; sliceIndex < slicesCount; sliceIndex++ ) {
						// Write results for reflected lobe
						string	sliceFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + "_slice" + sliceIndex.ToString( "G02" ) + extension );
						using ( FileStream S = new FileInfo( sliceFileName ).Create() )
							using ( BinaryWriter Writer = new BinaryWriter( S ) )
								for ( int Y=0; Y < H; Y++ )
									for ( int X=0; X < W; X++ ) {
										Document.Result	R = results[X,Y,sliceIndex];
										Writer.Write( R.m_reflected.m_theta );
										Writer.Write( R.m_reflected.m_roughness );
										Writer.Write( R.m_reflected.m_scale );
										Writer.Write( R.m_reflected.m_flatten );
										Writer.Write( R.m_reflected.m_masking );
									}

						// Write results for refracted lobe
						if ( m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
							sliceFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + "_slice" + sliceIndex.ToString( "G02" ) + "_refracted" + extension );
							using ( FileStream S = new FileInfo( sliceFileName ).Create() )
								using ( BinaryWriter Writer = new BinaryWriter( S ) )
									for ( int Y=0; Y < H; Y++ )
										for ( int X=0; X < W; X++ ) {
											Document.Result	R = results[X,Y,sliceIndex];
											Writer.Write( R.m_refracted.m_theta );
											Writer.Write( R.m_refracted.m_roughness );
											Writer.Write( R.m_refracted.m_scale );
											Writer.Write( R.m_refracted.m_flatten );
											Writer.Write( R.m_refracted.m_masking );
										}
						}
					}
				}

				m_AppKey.SetValue( "LastExportFileName", saveFileDialogExport.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while exporting results:\r\n" + _e );
			}
		}

		private void importToolStripMenuItem_Click( object sender, EventArgs e ) {
			string	fileName = m_AppKey.GetValue( "LastExportFileName", new System.IO.FileInfo( "results.bin" ).FullName ) as string;
			openFileDialogExport.FileName = Path.GetFileName( fileName );
			openFileDialogExport.InitialDirectory = Path.GetDirectoryName( fileName );
			if ( openFileDialogExport.ShowDialog( this ) != DialogResult.OK )
				return;

			try {

				fileName = openFileDialogExport.FileName;
				string	directory = Path.GetDirectoryName( fileName );
				string	fileNameNoExt = Path.GetFileNameWithoutExtension( fileName );
				string	extension = Path.GetExtension( fileName );

				for ( int order=0; order < m_document.m_results.Length; order++ ) {
					Document.Result[,,]	results = m_document.m_results[order];

					int	W = results.GetLength( 0 );
					int	H = results.GetLength( 1 );
					int	slicesCount = results.GetLength( 2 );
					for ( int sliceIndex=0; sliceIndex < slicesCount; sliceIndex++ ) {
						// Read results for reflected lobe
						string	sliceFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + "_slice" + sliceIndex.ToString( "G02" ) + extension );
						using ( FileStream S = new FileInfo( sliceFileName ).OpenRead() )
							using ( BinaryReader Reader = new BinaryReader( S ) )
								for ( int Y=0; Y < H; Y++ )
									for ( int X=0; X < W; X++ ) {
										Document.Result	R = results[X,Y,sliceIndex];
										R.m_reflected.m_theta = Reader.ReadDouble();
										R.m_reflected.m_roughness = Reader.ReadDouble();
										R.m_reflected.m_scale = Reader.ReadDouble();
										R.m_reflected.m_flatten = Reader.ReadDouble();
										R.m_reflected.m_masking = Reader.ReadDouble();
										R.State = 1;
									}

						// Read results for refracted lobe
						if ( m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
							sliceFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + "_slice" + sliceIndex.ToString( "G02" ) + "_refracted" + extension );
							using ( FileStream S = new FileInfo( sliceFileName ).OpenRead() )
								using ( BinaryReader Reader = new BinaryReader( S ) )
									for ( int Y=0; Y < H; Y++ )
										for ( int X=0; X < W; X++ ) {
											Document.Result	R = results[X,Y,sliceIndex];
											R.m_refracted.m_theta = Reader.ReadDouble();
											R.m_refracted.m_roughness = Reader.ReadDouble();
											R.m_refracted.m_scale = Reader.ReadDouble();
											R.m_refracted.m_flatten = Reader.ReadDouble();
											R.m_refracted.m_masking = Reader.ReadDouble();
											R.State = 1;
										}
						}
					}
				}

				m_AppKey.SetValue( "LastExportFileName", openFileDialogExport.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while importing results:\r\n" + _e );
			}
		}

		private void exportTotalReflectanceToolStripMenuItem_Click( object sender, EventArgs e ) {
			string	fileName = m_AppKey.GetValue( "LastTotalReflectanceExportFileName", new System.IO.FileInfo( "TotalReflectance.bin" ).FullName ) as string;
			saveFileDialogExport.FileName = Path.GetFileName( fileName );
			saveFileDialogExport.InitialDirectory = Path.GetDirectoryName( fileName );
			if ( saveFileDialogExport.ShowDialog( this ) != DialogResult.OK )
				return;

			try {

				fileName = saveFileDialogExport.FileName;
				string	directory = Path.GetDirectoryName( fileName );
				string	fileNameNoExt = Path.GetFileNameWithoutExtension( fileName );
				string	extension = Path.GetExtension( fileName );

				//@TODO: Support extended export formats
				for ( int order=0; order < m_document.m_results.Length; order++ ) {
					Document.Result[,,]	results = m_document.m_results[order];

					string	orderFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + extension );

					int	W = results.GetLength( 0 );
					int	H = results.GetLength( 1 );
					int	slicesCount = results.GetLength( 2 );

					// Write results for reflected lobe
					using ( FileStream S = new FileInfo( orderFileName ).Create() ) {
						using ( BinaryWriter Writer = new BinaryWriter( S ) ) {
							for ( int sliceIndex=0; sliceIndex < slicesCount; sliceIndex++ ) {
								for ( int Y=0; Y < H; Y++ )
									for ( int X=0; X < W; X++ ) {
										Document.Result	R = results[X,Y,sliceIndex];
										Writer.Write( R.m_reflected.m_totalReflectance );
									}
							}
						}
					}

					if ( m_document.m_surface.m_type == TestForm.SURFACE_TYPE.DIELECTRIC ) {
						// Write results for refracted lobe
						orderFileName = Path.Combine( directory, fileNameNoExt + "_order" + (m_document.m_surface.ScatteringOrderMin + order) + "_slice_refracted" + extension );
						using ( FileStream S = new FileInfo( orderFileName ).Create() ) {
							using ( BinaryWriter Writer = new BinaryWriter( S ) ) {
								for ( int sliceIndex=0; sliceIndex < slicesCount; sliceIndex++ ) {
									for ( int Y=0; Y < H; Y++ )
										for ( int X=0; X < W; X++ ) {
											Document.Result	R = results[X,Y,sliceIndex];
											Writer.Write( R.m_refracted.m_totalReflectance );
										}
								}
							}
						}
					}
				}

				m_AppKey.SetValue( "LastTotalReflectanceExportFileName", saveFileDialogExport.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while exporting results:\r\n" + _e );
			}
		}

		#endregion

		#region Context Menu

		private void contextMenuStripSelection_Opening( object sender, CancelEventArgs e ) {
			e.Cancel = SelectedResult == null;
		}

		private void computeToolStripMenuItem_Click( object sender, EventArgs e ) {
			ComputeSelectedResult();	// Same as double-clicking
		}

		private void startFromHereToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( m_documentFileName == null ) {
				string	FileName = AskForFileName();
				if ( FileName == null ) {
					MessageBox( "You cannot start a computation without specifying a filename otherwise auto-save feature won't be available and you might lose your results if a crash occurs during automation!" );
					return;
				}
				DocumentFileName = new FileInfo( FileName );
			}

			ComputeAll( SelectedScatteringOrder, SelectedResult.X, SelectedResult.Y, SelectedResult.Z, true );
		}

		private void clearToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( SelectedResult.IsValid ) {
				if ( MessageBox( "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) != DialogResult.OK )
					return;
			} else {
				MessageBox( "Result already cleared!", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			SelectedResult.Clear();
		}

		private void clearColumnToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) != DialogResult.OK )
				return;

			Document.Result[,,]	results = m_document.GetResultsForOrder( SelectedResult.ScatteringOrder );
			for ( int Y=SelectedResult.Y; Y < m_document.m_surface.m_roughness.StepsCount; Y++ )
				results[SelectedResult.X,Y,SelectedResult.Z].Clear();
		}

		private void clearRowToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) != DialogResult.OK )
				return;

			Document.Result[,,]	results = m_document.GetResultsForOrder( SelectedResult.ScatteringOrder );
			for ( int X=SelectedResult.X; X < m_document.m_surface.m_incomingAngle.StepsCount; X++ )
				results[X,SelectedResult.Y,SelectedResult.Z].Clear();
		}

		private void clearSliceFromHereToolStripMenuItem_Click( object sender, EventArgs e )
		{
			if ( MessageBox( "Are you sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) != DialogResult.OK )
				return;

			Document.Result[,,]	results = m_document.GetResultsForOrder( SelectedResult.ScatteringOrder );
			for ( int Y=SelectedResult.Y; Y < m_document.m_surface.m_roughness.StepsCount; Y++ )
				for ( int X=SelectedResult.X; X < m_document.m_surface.m_incomingAngle.StepsCount; X++ )
					results[X,Y,SelectedResult.Z].Clear();
		}

		#endregion
		
		#region UI => Document Mirroring

		#region Settings

		private void LobeTypeCheckChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_lobeModel =	radioButtonLobe_ModifiedPhong.Checked ?			LobeModel.LOBE_TYPE.MODIFIED_PHONG :
												(radioButtonLobe_ModifiedPhongAniso.Checked ?	LobeModel.LOBE_TYPE.MODIFIED_PHONG_ANISOTROPIC :
												(radioButtonLobe_Beckmann.Checked ?				LobeModel.LOBE_TYPE.BECKMANN :
																								LobeModel.LOBE_TYPE.GGX));

			m_owner.SetLobeType( m_document.m_settings.m_lobeModel );
		}

		private void radioButtonInitDirection_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialDirection = radioButtonInitDirection_TowardCoM.Checked ?		Document.Settings.GUESS_INITIAL_DIRECTION.CENTER_OF_MASS :
													(radioButtonInitDirection_TowardReflected.Checked ?	Document.Settings.GUESS_INITIAL_DIRECTION.REFLECTED_DIRECTION :
													(radioButtonInitDirection_Fixed.Checked ?			Document.Settings.GUESS_INITIAL_DIRECTION.FIXED :
																										Document.Settings.GUESS_INITIAL_DIRECTION.NO_CHANGE));

			floatTrackbarControlInit_FixedDirection.Enabled = m_document.m_settings.m_initialDirection == Document.Settings.GUESS_INITIAL_DIRECTION.FIXED;
		}

		private void radioButtonInitRoughness_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialRoughness = radioButtonInitRoughness_UseSurface.Checked ?	Document.Settings.GUESS_INITIAL_ROUGHNESS.SURFACE :
													(radioButtonInitRoughness_Custom.Checked ?			Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM :
													(radioButtonInitRoughness_NoChange.Checked ?		Document.Settings.GUESS_INITIAL_ROUGHNESS.NO_CHANGE :
													(radioButtonInitRoughness_Fixed.Checked ?			Document.Settings.GUESS_INITIAL_ROUGHNESS.FIXED :
																										Document.Settings.GUESS_INITIAL_ROUGHNESS.ANALYTICAL)));

			floatTrackbarControlInit_CustomRoughness.Enabled = m_document.m_settings.m_initialRoughness == Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM;
			floatTrackbarControlInit_FixedRoughness.Enabled = m_document.m_settings.m_initialRoughness == Document.Settings.GUESS_INITIAL_ROUGHNESS.FIXED;
		}

		private void radioButtonInitScale_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialScale = radioButtonInitScale_CoMFactor.Checked ?		Document.Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS :
													(radioButtonInitScale_NoChange.Checked ?	Document.Settings.GUESS_INITIAL_SCALE.NO_CHANGE :
													(radioButtonInitScale_Fixed.Checked ?		Document.Settings.GUESS_INITIAL_SCALE.FIXED :
																								Document.Settings.GUESS_INITIAL_SCALE.ANALYTICAL));

			floatTrackbarControlInit_Scale.Enabled = m_document.m_settings.m_initialScale == Document.Settings.GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS;
			floatTrackbarControlInit_FixedScale.Enabled = m_document.m_settings.m_initialScale == Document.Settings.GUESS_INITIAL_SCALE.FIXED;
		}

		private void radioButtonInitFlatten_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialFlatten = radioButtonInitFlatten_Custom.Checked ?	Document.Settings.GUESS_INITIAL_FLATTEN.CUSTOM :
													(radioButtonInitFlatten_NoChange.Checked ?	Document.Settings.GUESS_INITIAL_FLATTEN.NO_CHANGE :
													(radioButtonInitFlatten_Fixed.Checked ?		Document.Settings.GUESS_INITIAL_FLATTEN.FIXED :
																								Document.Settings.GUESS_INITIAL_FLATTEN.ANALYTICAL));

			floatTrackbarControlInit_CustomFlatten.Enabled = m_document.m_settings.m_initialFlatten == Document.Settings.GUESS_INITIAL_FLATTEN.CUSTOM;
			floatTrackbarControlInit_FixedFlatten.Enabled = m_document.m_settings.m_initialFlatten == Document.Settings.GUESS_INITIAL_FLATTEN.FIXED;
		}

		private void radioButtonInitMasking_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_initialMasking = radioButtonInitMasking_Custom.Checked ?	Document.Settings.GUESS_INITIAL_MASKING.CUSTOM :
													(radioButtonInitMasking_NoChange.Checked ?	Document.Settings.GUESS_INITIAL_MASKING.NO_CHANGE :
																								Document.Settings.GUESS_INITIAL_MASKING.FIXED);

			floatTrackbarControlInit_CustomMaskingImportance.Enabled = m_document.m_settings.m_initialMasking == Document.Settings.GUESS_INITIAL_MASKING.CUSTOM;
			floatTrackbarControlInit_FixedMasking.Enabled = m_document.m_settings.m_initialMasking == Document.Settings.GUESS_INITIAL_MASKING.FIXED;
		}

		private void checkBoxInitDirection_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritDirection_Top = checkBoxInitDirection_Inherit.Checked;
		}

		private void checkBoxInitScale_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritScale_Top = checkBoxInitScale_Inherit.Checked;
		}

		private void checkBoxInitFlatten_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritFlatten_Top = checkBoxInitFlatten_Inherit.Checked;
		}

		private void checkBoxInitRoughness_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritRoughness_Top = checkBoxInitRoughness_Inherit.Checked;
		}

		private void checkBoxInitMasking_Inherit_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritMasking_Top = checkBoxInitMasking_Inherit.Checked;
		}

		private void checkBoxInitDirection_InheritLeft_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritDirection_Left = checkBoxInitDirection_InheritLeft.Checked;
		}

		private void checkBoxInitRoughness_InheritLeft_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritRoughness_Left = checkBoxInitRoughness_InheritLeft.Checked;
		}

		private void checkBoxInitScale_InheritLeft_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritScale_Left = checkBoxInitScale_InheritLeft.Checked;
		}

		private void checkBoxInitFlatten_InheritLeft_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritFlatten_Left = checkBoxInitFlatten_InheritLeft.Checked;
		}

		private void checkBoxInitMasking_InheritLeft_CheckedChanged( object sender, EventArgs e )
		{
			m_document.m_settings.m_inheritMasking_Left = checkBoxInitMasking_InheritLeft.Checked;
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

		private void floatTrackbarControlInit_FixedDirection_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_fixedTheta = (float) (Math.PI * floatTrackbarControlInit_FixedDirection.Value / 180.0);
		}

		private void floatTrackbarControlInit_FixedRoughness_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_fixedRoughness = floatTrackbarControlInit_FixedRoughness.Value;
		}

		private void floatTrackbarControlInit_FixedScale_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_fixedScale = floatTrackbarControlInit_FixedScale.Value;
		}

		private void floatTrackbarControlInit_FixedFlatten_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_fixedFlatten = floatTrackbarControlInit_FixedFlatten.Value;
		}

		private void floatTrackbarControlInit_FixedMasking_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_document.m_settings.m_fixedMasking = floatTrackbarControlInit_FixedMasking.Value;
		}

		#endregion

		#region Surface

		private void radioButtonSurfaceType_CheckedChanged( object sender, EventArgs e ) {
			m_document.m_surface.m_type = radioButtonSurfaceTypeConductor.Checked ?	TestForm.SURFACE_TYPE.CONDUCTOR : (
										radioButtonSurfaceTypeDielectric.Checked ?	TestForm.SURFACE_TYPE.DIELECTRIC :
																					TestForm.SURFACE_TYPE.DIFFUSE);

			// Update UI
			labelParm2.Text = SurfaceType == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0";

			// Also update surface type in the main form
			if ( m_owner != null )
				m_owner.SetSurfaceType( m_document.m_surface.m_type );

			// Also update initial guesses based on surface type
			if ( !m_internalDocumentChange ) {
				switch ( m_document.m_surface.m_type ) {
					case TestForm.SURFACE_TYPE.DIFFUSE:
						m_document.m_settings.m_lobeModel = LobeModel.LOBE_TYPE.MODIFIED_PHONG;
						m_document.m_settings.m_customScale = 0.05f;
						m_document.m_settings.m_initialRoughness = Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM;
						m_document.m_settings.m_customRoughness = 0.8f;
						m_document.m_settings.m_customFlatten = 0.5f;
						m_document.m_settings.m_customMasking = 0.0f;
						break;

					default:
						m_document.m_settings.m_lobeModel = LobeModel.LOBE_TYPE.MODIFIED_PHONG_ANISOTROPIC;
						m_document.m_settings.m_customScale = 0.1f;
						m_document.m_settings.m_initialRoughness = Document.Settings.GUESS_INITIAL_ROUGHNESS.CUSTOM;
						m_document.m_settings.m_customRoughness = 0.8f;
						m_document.m_settings.m_customFlatten = 1.0f;
						m_document.m_settings.m_customMasking = 1.0f;
						break;
				}
			}
		}

		private void floatTrackbarControlParam0_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_incomingAngle.Min = floatTrackbarControlParam0_Min.Value;
		}

		private void floatTrackbarControlParam0_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_incomingAngle.Max = floatTrackbarControlParam0_Max.Value;
		}

		private void integerTrackbarControlParam0_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			if ( !m_internalDocumentChange ) {
				m_document.m_surface.m_incomingAngle.StepsCount = integerTrackbarControlParam0_Steps.Value;
				DocumentResults2UI();
			}
		}

		private void floatTrackbarControlParam1_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_roughness.Min = floatTrackbarControlParam1_Min.Value;
		}

		private void floatTrackbarControlParam1_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_roughness.Max = floatTrackbarControlParam1_Max.Value;
		}

		private void integerTrackbarControlParam1_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			if ( !m_internalDocumentChange ) {
				m_document.m_surface.m_roughness.StepsCount = integerTrackbarControlParam1_Steps.Value;
				DocumentResults2UI();
			}
		}

		private void floatTrackbarControlParam2_Min_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_albedoF0.Min = floatTrackbarControlParam2_Min.Value;
		}

		private void floatTrackbarControlParam2_Max_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_albedoF0.Max = floatTrackbarControlParam2_Max.Value;
		}

		private void integerTrackbarControlParam2_Steps_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			if ( !m_internalDocumentChange ) {
				m_document.m_surface.m_albedoF0.StepsCount = integerTrackbarControlParam2_Steps.Value;
				DocumentResults2UI();

				integerTrackbarControlViewAlbedoSlice.RangeMax = m_document.m_surface.m_albedoF0.StepsCount - 1;
				integerTrackbarControlViewAlbedoSlice.VisibleRangeMax = integerTrackbarControlViewAlbedoSlice.RangeMax;
			}
		}

		private void integerTrackbarControlScatteringOrder_Min_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			if ( !m_internalDocumentChange ) {
				m_document.m_surface.ScatteringOrderMin = integerTrackbarControlScatteringOrder_Min.Value;

				integerTrackbarControlScatteringOrder_Max.RangeMin = m_document.m_surface.ScatteringOrderMin;	// Max scattering can't go lower than this
				integerTrackbarControlScatteringOrder_Max.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
				integerTrackbarControlViewScatteringOrder.RangeMin = m_document.m_surface.ScatteringOrderMin;
				integerTrackbarControlViewScatteringOrder.VisibleRangeMin = m_document.m_surface.ScatteringOrderMin;
			}
		}

		private void integerTrackbarControlScatteringOrder_Max_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			if ( !m_internalDocumentChange ) {
				m_document.m_surface.ScatteringOrderMax = integerTrackbarControlScatteringOrder_Max.Value;

				integerTrackbarControlScatteringOrder_Min.RangeMax = m_document.m_surface.ScatteringOrderMax;	// Min scattering can't go higher than this
				integerTrackbarControlScatteringOrder_Min.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
				integerTrackbarControlViewScatteringOrder.RangeMax = m_document.m_surface.ScatteringOrderMax;
				integerTrackbarControlViewScatteringOrder.VisibleRangeMax = m_document.m_surface.ScatteringOrderMax;
			}
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
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_incomingAngle.InclusiveMin = checkBoxParam0_InclusiveStart.Checked;
		}

		private void checkBoxParam0_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_incomingAngle.InclusiveMax = checkBoxParam0_InclusiveEnd.Checked;
		}

		private void checkBoxParm1_InclusiveStart_CheckedChanged( object sender, EventArgs e )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_roughness.InclusiveMin = checkBoxParam1_InclusiveStart.Checked;
		}

		private void checkBoxParam1_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_roughness.InclusiveMax = checkBoxParam1_InclusiveEnd.Checked;
		}

		private void checkBoxParm2_InclusiveStart_CheckedChanged( object sender, EventArgs e )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_albedoF0.InclusiveMin = checkBoxParam2_InclusiveStart.Checked;
		}

		private void checkBoxParm2_InclusiveEnd_CheckedChanged( object sender, EventArgs e )
		{
			if ( !m_internalDocumentChange )
				m_document.m_surface.m_albedoF0.InclusiveMax = checkBoxParam2_InclusiveEnd.Checked;
		}

		#endregion

		#region Lobe Fitter

		private void checkBoxSkipSimulation_CheckedChanged( object sender, EventArgs e ) {
			m_document.m_settings.m_performFitting = !checkBoxSkipSimulation.Checked;
		}

		private void integerTrackbarControlMaxIterations_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			m_document.m_settings.m_maxIterations = integerTrackbarControlMaxIterations.Value;
		}

		private void floatTrackbarControlGoalTolerance_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_document.m_settings.m_logTolerance_Minimum = floatTrackbarControlGoalTolerance.Value;
		}

		private void floatTrackbarControlGradientTolerance_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_document.m_settings.m_logTolerance_Gradient = floatTrackbarControlGradientTolerance.Value;
		}

		private void integerTrackbarControlRetries_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			m_document.m_settings.m_maxRetries = integerTrackbarControlRetries.Value;
		}

		private void floatTrackbarControlFitOversize_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_document.m_settings.m_oversizeFactor = floatTrackbarControlFitOversize.Value;
		}

		#endregion

		#endregion

		// Double-clicking builds a single result
		private void completionArrayControl_MouseDoubleClick( object sender, MouseEventArgs e ) {
			if ( !completionArrayControl.IsPointValid( e.Location ) )
				return;	// Not a valid candidate for simulation

			// Change selection to clicked cell
			completionArrayControl.SelectAt( e.Location );

			ComputeSelectedResult();
		}

		private void buttonClearResults_Click( object sender, EventArgs e ) {
			if ( !m_document.m_surface.IsLocked ) {
				MessageBox( "No results are computed yet, there is nothing to clear", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			if ( MessageBox( "Are you sure you want to erase current results?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;

			m_document.Clear();
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
	}
}
