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

		class	Results {

			public delegate void	SettingsChangedEventHandler();
			public delegate void	ResultStateChangedEventHandler( Result _result );

			public class	Settings {

				public enum GUESS_INITIAL_DIRECTION {
					CENTER_OF_MASS,
					REFLECTED_DIRECTION,
					NO_CHANGE,
				}

				public enum GUESS_INITIAL_ROUGHNESS {
					SURFACE,
					CUSTOM,
				}

				public enum GUESS_INITIAL_SCALE {
					FACTOR_CENTER_OF_MASS,
					NO_CHANGE,
				}

				public enum GUESS_INITIAL_FLATTEN {
					CUSTOM,
				}

				public enum GUESS_INITIAL_MASKING {
					CUSTOM,
				}

				public class	Parameter {

					public delegate	void	ValueChangedEventHandler( Parameter _P );

					float	m_rangeMin = 0.0f;
					float	m_rangeMax = 1.0f;

					float	m_min = 0.0f;
					float	m_max = 1.0f;
					int		m_stepsCount = 1;
					bool	m_inclusiveMin = true;
					bool	m_inclusiveMax = false;

					public float	Min	{
						get { return m_min; }
						set {
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
							if ( value == m_inclusiveMax )
								return;

							m_inclusiveMax = value;

							if ( ValueChanged != null )
								ValueChanged( this );
						}
					}

					public event ValueChangedEventHandler	ValueChanged;

					public Parameter( float _rangeMin, float _rangeMax ) {
						m_rangeMin = _rangeMin;
						m_rangeMax = _rangeMax;
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

				// Surface parameters
				public TestForm.SURFACE_TYPE	m_surfaceType = TestForm.SURFACE_TYPE.CONDUCTOR;
				public int						m_rayTracingIterationsCount = 1024;
				public Parameter				m_incomingAngle = new Parameter( 0, 0.5f * (float) Math.PI );
				public Parameter				m_surfaceRoughness = new Parameter( 0, 1.0f );
				public Parameter				m_albedoF0 = new Parameter( 0, 1.0f );
				private Parameter				m_scatteringOrders = new Parameter( 1, 4 );

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
				public GUESS_INITIAL_SCALE		m_initialScale = GUESS_INITIAL_SCALE.FACTOR_CENTER_OF_MASS;
				public bool						m_inheritScale = true;
				public float					m_customScale = 0.05f;
				public GUESS_INITIAL_FLATTEN	m_initialFlatten = GUESS_INITIAL_FLATTEN.CUSTOM;
				public bool						m_inheritFlatten = true;
				public float					m_customFlatten = 0.5f;
				public GUESS_INITIAL_MASKING	m_initialMasking = GUESS_INITIAL_MASKING.CUSTOM;
				public bool						m_inheritMasking = true;
				public float					m_customMasking = 1.0f;


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

				public	Settings() {
				}

				public void		Save( XmlElement _parent ) {
					// Save surface parameters
					XmlElement	ElemSurfaceParms = AppendChild( _parent, "Surface" );
					Attrib( ElemSurfaceParms, "Surface Type", m_surfaceType );
					Attrib( ElemSurfaceParms, "RayTrace Iterations", m_rayTracingIterationsCount );

					XmlElement ElemParm0 = AppendChild( ElemSurfaceParms, "IncomingAngle" );
					m_incomingAngle.Save( ElemParm0 );

					XmlElement ElemParm1 = AppendChild( ElemSurfaceParms, "SurfaceRoughness" );
					m_surfaceRoughness.Save( ElemParm1 );

					XmlElement ElemParm2 = AppendChild( ElemSurfaceParms, "AlbedoF0" );
					m_albedoF0.Save( ElemParm2 );

					XmlElement ElemParm3 = AppendChild( ElemSurfaceParms, "IncomingAngle" );
					m_scatteringOrders.Save( ElemParm3 );

					// Fitter parameters
					XmlElement	ElemFitterParms = AppendChild( _parent, "Fitter" );
					_parent.AppendChild( ElemFitterParms );

					Attrib( ElemFitterParms, "MaxIterations", m_maxIterations );
					Attrib( ElemFitterParms, "logToleranceMinimum", m_logTolerance_Minimum );
					Attrib( ElemFitterParms, "logToleranceGradient", m_logTolerance_Gradient );
					Attrib( ElemFitterParms, "MaxRetries", m_maxRetries );
					Attrib( ElemFitterParms, "OversizeFactor", m_oversizeFactor );

					// Lobe parameters
					XmlElement	ElemLobeParms = AppendChild( _parent, "Lobe" );

					Attrib( ElemLobeParms, "Model", m_lobeModel );
					Attrib( ElemLobeParms, "initialDirection",	m_initialDirection );
					Attrib( ElemLobeParms, "inheritDirection",	m_inheritDirection );
					Attrib( ElemLobeParms, "initialRoughness",	m_initialRoughness );
					Attrib( ElemLobeParms, "inheritRoughness",	m_inheritRoughness );
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

				public class	LobeResults {
					public double	m_theta;
					public double	m_roughness;
					public double	m_scale;
					public double	m_flatten;
					public double	m_masking;

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

				Results			m_owner = null;
				int				m_X;
				int				m_Y;
				int				m_Z;
				float			m_state = 0.0f;
				public string	m_error = null;

				// Input parameters
				public double	m_incomingAngleTheta;
				public double	m_incomingAnglePhi;
				public double	m_surfaceRoughness;
				public double	m_surfaceAlbedoF0;

				// Fitted parameters
				LobeResults		m_reflected = new LobeResults();
				LobeResults		m_refracted = new LobeResults();

				public int		X	{ get { return m_X; } }
				public int		Y	{ get { return m_Y; } }
				public int		Z	{ get { return m_Z; } }

				public float	State {
					get { return m_state; }
					set {
						if ( value == m_state )
							return;

						m_state = value;
						m_owner.ResultStateChanged( this );
					}
				}

				public Result( Results _owner, int _X, int _Y, int _Z ) {
					m_owner = _owner;
					m_X = _X;
					m_Y = _Y;
					m_Z = _Z;
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
					if ( m_owner.m_settings.m_surfaceType == TestForm.SURFACE_TYPE.DIELECTRIC ) {
						XmlElement	ElemRefracted = AppendChild( _parent, "LobeRefracted" );
						m_reflected.Save( ElemReflected );
					}
				}

				public void		Load( XmlElement _parent ) {

				}

			}

			public Settings		m_settings = new Settings();
			public Result[][,,]	m_results = new Result[0][,,];

			public event ResultStateChangedEventHandler	ResultStateChanged;

			public Results() {

			}

			/// <summary>
			/// Initializes the array of results of the correct dimensions using the current settings
			/// </summary>
			public void		InitializeResults() {

				int	orders = m_settings.ScatteringOrdersCount;
				int	dimX = m_settings.m_incomingAngle.StepsCount;
				int	dimY = m_settings.m_surfaceRoughness.StepsCount;
				int	dimZ = m_settings.m_albedoF0.StepsCount;

				double	incomingAnglePhi = 0.0;	//@TODO?? We don't care about anisotropy anyway...

				m_results = new Result[orders][,,];
				for ( int order=0; order < orders; order++ ) {

					m_results[order] = new Result[dimX,dimY,dimZ];

					float	incomingAngleMin, incomingAngleStep;
					m_settings.m_incomingAngle.BuildMinStep( out incomingAngleMin, out incomingAngleStep );
					float	roughnessMin, roughnessStep;
					m_settings.m_incomingAngle.BuildMinStep( out roughnessMin, out roughnessStep );
					float	albedoF0Min, albedoF0Step;
					m_settings.m_albedoF0.BuildMinStep( out albedoF0Min, out albedoF0Step );

					float	albedoF0 = albedoF0Min;
					for ( int Z=0; Z < dimZ; Z++, albedoF0+=albedoF0Step ) {
						float	roughness = roughnessMin;
						for ( int Y=0; Y < dimY; Y++, roughness+=roughnessStep ) {
							float	incomingAngle = incomingAngleMin;
							for ( int X=0; X < dimX; X++, incomingAngle+=incomingAngleStep ) {
								Result	R = new Result( this, X, Y, Z );
								m_results[order][X,Y,Z] = R;

								R.State = 0.0f;			// Not computed yet!
								R.m_error = null;		// No error yet...

								R.m_incomingAngleTheta = incomingAngle;
								R.m_incomingAnglePhi = incomingAnglePhi;
								R.m_surfaceRoughness = roughness;
								R.m_surfaceAlbedoF0 = albedoF0;
							}
						}
					}
				}
			}

			public void		Save( XmlDocument _doc ) {

				XmlElement	Root = _doc.CreateElement( "Root" );

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

// 			/// <summary>
// 			/// Called by our results to notify of a state change
// 			/// </summary>
// 			/// <param name="_result"></param>
// 			void	NotifyResultStateChanged( Result _result ) {
// 
// 			}

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
		WMath.BFGS		m_Fitter = new WMath.BFGS();

		bool			m_computing = false;
		bool			m_isReflectedLobe = true;

		Results			m_results = new Results();

		public new TestForm		Owner {
			get { return m_owner; }
			set { m_owner = value; }
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

			//
			integerTrackbarControlRayCastingIterations.SimulateValueChange();
		}

		void	Simulate() {
			m_owner.RayTraceSurface( );
		}

		void	PerformLobeFitting( float3 _incomingDirection, float _theta, bool _computeInitialThetaUsingCenterOfMass, float _roughness, float _scale, float _flatteningFactor, float _MaskingImportance, float _OversizeFactor, int _scatteringOrder, bool _reflected ) {

			m_isReflectedLobe = _reflected;

			// Read back histogram to CPU for fitting
			Texture2D	Tex_SimulatedLobeHistogram = m_owner.
			m_Tex_LobeHistogram_CPU.CopyFrom( _reflected ? m_Tex_LobeHistogram_Reflected : m_Tex_LobeHistogram_Transmitted );

			// Initialize lobe model
			m_lobeModel.InitTargetData( m_Tex_LobeHistogram_CPU, _scatteringOrder );

			if ( _computeInitialThetaUsingCenterOfMass ) {
				// Optionally override theta to use the direction of the center of mass
				// (quite intuitive to start by aligning our lobe along the main simulated lobe direction!)
				float3	towardCenterOfMass = m_lobeModel.CenterOfMass.Normalized;
				_theta = (float) Math.Acos( towardCenterOfMass.z );
//				_scale = 2.0 * m_centerOfMass.Length;				// Also assume we should match the simulated lobe's length
				_flatteningFactor = 0.5f;							// Start from a semi-flattened shape so it can choose either direction...
				_scale = 0.01f * m_lobeModel.CenterOfMass.Length;	// In fact, I realized the algorithm converged much faster starting from a very small lobe!! (~20 iterations compared to 200 otherwise, because the gradient leads the algorithm in the wrong direction too fast and it takes hell of a time to get back on tracks afterwards if we start from too large a lobe!)
			}

			LobeModel.LOBE_TYPE	lobeType = radioButtonAnalyticalPhong.Checked ? LobeModel.LOBE_TYPE.MODIFIED_PHONG : (radioButtonAnalyticalBeckmann.Checked ? LobeModel.LOBE_TYPE.BECKMANN : LobeModel.LOBE_TYPE.GGX);

			m_lobeModel.InitLobeData( lobeType, _incomingDirection, _theta, _roughness, _scale, _flatteningFactor, _MaskingImportance, _OversizeFactor, checkBoxTest.Checked );

// 			if ( !checkBoxTest.Checked ) {
// 				m_Fitter.SuccessTolerance = 1e-4;
// 				m_Fitter.GradientSuccessTolerance = 1e-4;
// 			}

			// Peform fitting
			m_Fitter.Minimize( m_lobeModel );
		}

		DialogResult	MessageBox( string _Text ) {
			return MessageBox( _Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon ) {
			return System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Automation", _Buttons, _Icon );
		}
		DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon, MessageBoxDefaultButton _defaultButton ) {
			return System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Automation", _Buttons, _Icon, _defaultButton );
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			Visible = false;	// Only hide, don't close!
			e.Cancel = true;
			base.OnFormClosing( e );
		}

		private void checkBoxInit_StartSmall_CheckedChanged( object sender, EventArgs e )
		{
//			floatTrackbarControlInit_StartSmallFactor.Enabled = checkBoxInit_StartSmall.Checked;
		}

		private void radioButtonInit_UseCustomRoughness_CheckedChanged( object sender, EventArgs e )
		{
// 			floatTrackbarControlInit_CustomRoughness.Enabled = radioButtonInit_UseCustomRoughness.Checked;
		}

		private void radioButtonSurfaceType_CheckedChanged( object sender, EventArgs e )
		{
			labelParm2.Text = SurfaceType == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0";
		}

		private void buttonCompute_Click( object sender, EventArgs e )
		{
			if ( m_computing )
				throw new CanceledException();

//				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );

			try {





			} catch ( Exception _e ) {
				bool	canceled = _e is CanceledException;
				string	Text = canceled ? "User canceled...\r\n" : "An error occurred while performing lobe fitting:\r\n" + _e.Message;
//				MessageBox( Text + "\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
				
			} finally {
			}
		}

		void m_lobeModel_ParametersChanged( double[] _parameters ) {
			m_owner.UpdateLobeParameters( _parameters, m_isReflectedLobe );
		}

		private void completionArrayControl_MouseDoubleClick( object sender, MouseEventArgs e ) {
			if ( !completionArrayControl.IsPointValid( e.Location ) )
				return;	// Not a valid candidate for simulation



		}

		private void buttonClearResults_Click( object sender, EventArgs e ) {
			if ( MessageBox( "Are you sure you want to erase current results?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;


		}

		private void newToolStripMenuItem_Click( object sender, EventArgs e )
		{

		}

		private void saveToolStripMenuItem1_Click( object sender, EventArgs e ) {
			try {
				string	FileName = m_AppKey.GetValue( "LastResultsFileName", new System.IO.FileInfo( "results.xml" ).FullName ) as string;
				saveFileDialogResults.FileName = Path.GetFileName( FileName );
				saveFileDialogResults.InitialDirectory = Path.GetDirectoryName( FileName );
				if ( saveFileDialogResults.ShowDialog( this ) != DialogResult.OK )
					return;

				XmlDocument	Doc = new XmlDocument();
				m_results.Save( Doc );

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

				m_results.Load( Doc );

				m_AppKey.SetValue( "LastResultsFileName", openFileDialogResults.FileName );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving results:\r\n" + _e );
			}
		}

		private void integerTrackbarControlRayCastingIterations_ValueChanged(Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue)
		{
			long	count = TestForm.HEIGHTFIELD_SIZE * TestForm.HEIGHTFIELD_SIZE;
					count *= (long) integerTrackbarControlRayCastingIterations.Value;

			labelTotalRaysCount.Text = "Total Simulated Rays: " + count;
		}
	}
}
