//////////////////////////////////////////////////////////////////////////////////////////////////////////////
// I attempted to realize an idea I had a long time ago: make the triangles "pop out" depending on the actual surface's curvature they're representing
//
// The idea is that flat triangles in low-poly models actually represent a curved surface:
//
//			            -------
//			      ----           ---	<== Actual shape it represents
//	   N0 \	   -                      -     / N1
//		   \ /                           \ /
//		 P0 *-----------------------------* P1	<== Flat triangle
//
// You can refer to the calculus sheets in this folder to have an idea on how I wanted to do that but basically
//	it boiled down to express a tangential sphere at each vertex then interpolating its center and radius along
//	the triangle and try to reproject the triangle's position onto that sphere, exactly like parallax mapping...
//
// My cube example shows that it correctly leave the cube's apparent shape intact but the lighting shows no discontinuities,
//	behaving as if it were a sphere...
//
// All the complicated computations are after all simplified by providing an additional radius of curvature per vertex.
// In the vertex shader you simply output the center of the tangent sphere as:
//
//		C = World Space Position - Radius * World Space Normal
//
// Then in the pixel shader you compute the intersection of the view ray with the interpolated sphere, and recompute the normal
//	as it's orthogonal to the sphere at the new position.
// An interesting fact is that the center and radius of the tangent sphere actually replace the usual normal that we don't need
//	to pass to the pixel shader anymore...
//
// -----------------------------------------------------------------------------------------------------------
// To know how compute the radius associated to each vertex, please refer to the special calculus sheet in the project directory.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//#define USE_SUBDIVIDED_CUBE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;
using Renderer;
using UIUtility;

namespace TriangleCurvature
{
	public partial class TestTriangleCurvatureForm : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
 		struct CB_Main {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_time;
			public uint		_flags;
			public float	_bend;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {

			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		#endregion

		#region FIELDS

		private Device								m_device = new Device();

		private Shader								m_Shader_renderScene = null;
		private Shader								m_Shader_renderSceneFinal = null;

		private ConstantBuffer< CB_Main >			m_CB_Main;
		private ConstantBuffer< CB_Camera >			m_CB_Camera = null;

		private Primitive							m_Prim_Cube = null;
		private Primitive							m_Prim_Torus = null;

		private Camera								m_camera = new Camera();
		private CameraManipulator					m_manipulator = new CameraManipulator();

		#endregion

		public TestTriangleCurvatureForm() {
			InitializeComponent();

			graph = new ImageFile( (uint) panelOutputGraph.Width, (uint) panelOutputGraph.Height, PIXEL_FORMAT.BGRA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			UpdateGraph();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
				
				m_CB_Main = new ConstantBuffer< CB_Main >( m_device, 0 );
				m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );

				#if USE_SUBDIVIDED_CUBE
					m_Prim_Cube = BuildSubdividedCube();
					m_Shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene_Subdiv.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS" );
				#else
					m_Prim_Cube = BuildCube();
					m_Shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS" );
				#endif

				m_Prim_Torus = BuildTorus();
				m_Shader_renderSceneFinal = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene_Final.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS" );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Exception: " + _e.Message, "Curvature Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			m_startTime = DateTime.Now;

			// Initialize camera
			m_camera.CreatePerspectiveCamera( 60.0f * (float) Math.PI / 180.0f, (float) panelOutput.Width / panelOutput.Height, 0.01f, 10.0f );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 0, 4.0f ), float3.Zero, float3.UnitY );
			m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
			m_camera_CameraTransformChanged( null, EventArgs.Empty );

			Application.Idle += Application_Idle;
		}

		protected override void OnClosing( CancelEventArgs e ) {
			base.OnClosing( e );

			Device	D = m_device;
			m_device = null;

			m_Prim_Cube.Dispose();

			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Shader_renderScene.Dispose();

			D.Dispose();
		}

		#region Vertex Tangent Sphere Radius Computation

		ImageFile	graph;

		/// <summary>
		/// Computes the radius of the sphere tangent to a vertex given a set of neighbor vertices
		/// </summary>
		/// <param name="_P"></param>
		/// <param name="_N"></param>
		/// <param name="_neighbors"></param>
		/// <returns></returns>
		float	ComputeTangentSphereRadius( float3 _P, float3 _N, float3[] _neighbors, bool _debugGraph ) {

			Func<float3,double,float3[],double>	SquareDistance = ( float3 _C, double _R, float3[] _Pns ) => {
				double result = 0.0f;
				foreach ( float3 Pn in _Pns )
					result += (Pn - _C).LengthSquared - _R*_R;
				return result;
			};

float2	rangeX = new float2( -4, 4 ), rangeY = new float2( -50, 50 );
if ( _debugGraph ) {
	graph.Clear( float4.One );
	graph.PlotAxes( float4.UnitW, rangeX, rangeY, 0.5f, 5.0f );
	graph.PlotGraph( float4.UnitW, rangeX, rangeY, ( float x ) => { return (float) SquareDistance( _P - x * _N, x, _neighbors ); } );
}

			const float		eps = 0.01f;
			const double	tol = 1e-3;

			double	previousR = -double.MaxValue;
			double	R = 0.0f;
			float3	C = _P;
			int		iterationsCount = 0;
			double	previousSqDistance = double.MaxValue;
			double	bestSqDistance = double.MaxValue;
			double	bestR = 0.0f;
			int		bestIterationsCount = 0;
			while ( Math.Abs( R ) < 10000.0f && iterationsCount < 1000 ) {
				// Compute gradient
				double	sqDistance = SquareDistance( C, R, _neighbors );
				double	sqDistance2 = SquareDistance( C - eps * _N, R + eps, _neighbors );
				double	grad = (sqDistance2 - sqDistance) / eps;

				// Compute intersection with secant Y=0
				double	t = -sqDistance * (Math.Abs( grad ) > 1e-6f ? 1.0f / grad : (Math.Sign( grad ) * 1e6));
				if ( Math.Abs( t ) < tol )
					break;

				previousR = R;
				R += t;
				C = _P - (float) R * _N;
				iterationsCount++;

				previousSqDistance = sqDistance;
				if ( sqDistance < bestSqDistance ) {
					bestSqDistance = sqDistance;
					bestR = previousR;
					bestIterationsCount = iterationsCount;
				}

if ( _debugGraph ) {
	graph.DrawLine( new float4( 1, 0, 0, 1 ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) previousR, (float) sqDistance ) ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) R, (float) SquareDistance( _P - (float) R * _N, R, _neighbors ) ) ) );
	float	k = 0.1f;
	graph.DrawLine( new float4( 0, 0.5f, 0, 1 ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) previousR, k * (iterationsCount-1) ) ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) R, k * iterationsCount ) ) );
}
			}

if ( _debugGraph ) {
	panelOutputGraph.m_bitmap = graph.AsBitmap;
	panelOutputGraph.Refresh();
	labelResult.Text = "R = " + R + " (" + previousSqDistance + ") in " + iterationsCount + " iterations...\r\nBest = " + bestR + " (" + bestSqDistance + ") in " + bestIterationsCount + " iterations...";
}

// 			if ( R < 1000.0 )
// 				throw new Exception( "Maybe R should be Math.Abs()'ed? (neighbors are all lying on a flat plane or in a ?" );
			R = Math.Max( -10000.0, Math.Min( 10000.0, R ) );

			return (float) R;
		}

		/// <summary>
		/// Computes the radius of the sphere tangent to a vertex given a set of neighbor vertices
		/// </summary>
		/// <param name="_P"></param>
		/// <param name="_N"></param>
		/// <param name="_neighbors"></param>
		/// <returns></returns>
		float	ComputeTangentSphereRadius_SUPER_SLOW( float3 _P, float3 _N, float3[] _neighbors ) {

ImageFile	graph = new ImageFile( (uint) panelOutputGraph.Width, (uint) panelOutputGraph.Height, PIXEL_FORMAT.BGRA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
graph.Clear( float4.One );

			Func<float3,float,float3[],float>	SquareDistance = ( float3 _C, float _R, float3[] _Pns ) => { float result = 0.0f; foreach ( float3 Pn in _Pns ) result += (Pn - _C).LengthSquared - _R*_R; return Math.Abs( result ); };
//			Func<float3,float3[],float>		SquareDistance = ( float3 _C, float3[] _Pns ) => { float result = 0.0f; foreach ( float3 Pn in _Pns ) result += (Pn - _C).Length; return result / _Pns.Length; };

float2	rangeX = new float2( 0, 4 ), rangeY = new float2( -50, 50 );
graph.PlotAxes( float4.UnitW, rangeX, rangeY, 0.5f, 5.0f );
graph.PlotGraph( float4.UnitW, rangeX, rangeY, ( float x ) => { return SquareDistance( _P - x * _N, x, _neighbors ); } );

			const float	eps = 0.01f;
			const float	tol = 1e-3f;

			float	previousR = -float.MaxValue;
			float	R = 0.0f;
			float	step = 0.1f;
			float3	C = _P;
			int		iterationsCount = 0;
			float	previousSqDistance = float.MaxValue;
			float	bestSqDistance = float.MaxValue;
			float	bestR = 0.0f;
			int		bestIterationsCount = 0;
			while ( step > tol && R < 10000.0f && iterationsCount < 1000 ) {
				// Compute gradient
				float	sqDistance = SquareDistance( C, R, _neighbors );
				float	sqDistance2 = SquareDistance( C - eps * _N, R + eps, _neighbors );
				float	grad = (sqDistance2 - sqDistance) / eps;

				if ( previousSqDistance < sqDistance ) {
					step *= 0.95f;	// Climbing up again, roll down slower...
				}

				// Follow opposite gradient direction toward minimum
				previousR = R;
				R -= step * grad;
				C = _P - R * _N;
				iterationsCount++;

				previousSqDistance = sqDistance;
				if ( sqDistance < bestSqDistance ) {
					bestSqDistance = sqDistance;
					bestR = previousR;
					bestIterationsCount = iterationsCount;
				}

graph.DrawLine( new float4( 1, 0, 0, 1 ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( previousR, sqDistance ) ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( R, SquareDistance( _P - R * _N, R, _neighbors ) ) ) );

float	k = 0.1f;
graph.DrawLine( new float4( 0, 0.5f, 0, 1 ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( previousR, k * (iterationsCount-1) ) ), graph.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( R, k * iterationsCount ) ) );
			}

			// Since we crossed the minimum, take the average for a better result
			R = 0.5f * (R + previousR);

panelOutputGraph.m_bitmap = graph.AsBitmap;
panelOutputGraph.Refresh();
labelResult.Text = "R = " + R + " (" + previousSqDistance + ") in " + iterationsCount + " iterations...\r\nBest = " + bestR + " (" + bestSqDistance + ") in " + bestIterationsCount + " iterations...";

			return R;
		}

		#endregion

		#region Tangent Surface Radii Computation

		/// <summary>
		/// Computes the 2 radii of the surface tangent to a vertex given a set of neighbor vertices
		/// </summary>
		/// <param name="_P"></param>
		/// <param name="_N"></param>
		/// <param name="_neighbors"></param>
		/// <returns></returns>
		float2	ComputeTangentCurvatureRadii( float3 _P, float3 _N, float3 _T, float3[] _neighbors, bool _debugGraph ) {

			// Compute the square distance between the local neighbors and the double curvature surface of equation f(x,y) = Rt * x² + Rb * y²
			Func<double,double,float3[],double>	SquareDistance = ( double _Rt, double _Rb, float3[] _Pns ) => {
				double result = 0.0;
				foreach ( float3 Pn in _Pns ) {
					double	Fxy = _Rt * Pn.x * Pn.x + _Rb * Pn.y * Pn.y;
					double	Dz = Fxy - Pn.z;
					result += Dz * Dz;
//					result += Dz;
				}
				return result;
			};

			// Transform neighbors into local space
			float3		B = _T.Cross( _N ).Normalized;
			float3[]	neighbors = new float3[_neighbors.Length];
			for ( int neighborIndex=0; neighborIndex < _neighbors.Length; neighborIndex++ ) {
				float3	D = _neighbors[neighborIndex] - _P;
				neighbors[neighborIndex].Set( D.Dot( _T ), D.Dot( B ), D.Dot( _N ) );
			}

float2	rangeX = new float2( -100, 100 ), rangeY = new float2( -1000, 1000 );
float4	pipo = new float4( 1, 0, 0, 1 );
if ( _debugGraph ) {
	graph.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
		float	R0 = rangeX.x + X * (rangeX.y - rangeX.x) / graph.Width;
		float	R1 = rangeY.y + Y * (rangeY.x - rangeY.y) / graph.Height;
		float	sqDistance = (float) SquareDistance( R0, R1, _neighbors );
		float	V = Math.Abs( sqDistance / 200.0f );
		_color.Set( V, V, V, 1 );
	} );
	graph.DrawLine( new float4( 0, 0, 1, 1 ), new float2( 0, 0.5f * graph.Height ), new float2( graph.Width, 0.5f * graph.Height ) );
	graph.DrawLine( new float4( 0, 0, 1, 1 ), new float2( 0.5f * graph.Width, 0 ), new float2( 0.5f * graph.Width, graph.Height ) );
}

			const float		eps = 0.01f;
			const double	tol = 1e-3;

			// Compute gradient descent
			double	previousRt = -double.MaxValue;
			double	previousRb = -double.MaxValue;
			double	Rt = 0.0, Rb = 0.0;
			int		iterationsCount = 0;
			double	previousSqDistance = double.MaxValue;
			double	bestSqDistance = double.MaxValue;
			double	bestRt = 0.0;
			double	bestRb = 0.0;
			double	stepSize = 10.0;
			int		bestIterationsCount = 0;
			while ( Math.Abs( Rt ) < 10000.0f && Math.Abs( Rb ) < 10000.0f && iterationsCount < 1000 ) {
				// Compute gradient
				double	sqDistance = SquareDistance( Rt, Rb, neighbors );	// Central value
				if ( Math.Abs( sqDistance - previousSqDistance ) / sqDistance < tol ) {
					break;	// Relative error is low enough
				}

				double	sqDistance_dT = SquareDistance( Rt + eps, Rb, neighbors );
				double	sqDistance_dB = SquareDistance( Rt, Rb + eps, neighbors );
//				double	sqDistance_dTdB = SquareDistance( Rt + eps, Rb + eps, neighbors );
				double	grad_dT = (sqDistance_dT - sqDistance) / eps;
				double	grad_dB = (sqDistance_dB - sqDistance) / eps;
// 				double	grad_dTdB = (sqDistance_dTdB - sqDistance) / eps;
// 						grad_dT = 0.5 * (grad_dT + grad_dTdB);
// 						grad_dB = 0.5 * (grad_dB + grad_dTdB);

				// Compute intersection with secant Y=0
// 				double	t_T = -sqDistance * (Math.Abs( grad_dT ) > 1e-6 ? 1.0 / grad_dT : (Math.Sign( grad_dT ) * 1e6));
// 				double	t_B = -sqDistance * (Math.Abs( grad_dB ) > 1e-6 ? 1.0 / grad_dB : (Math.Sign( grad_dB ) * 1e6));
				double	t_T = -stepSize * eps * grad_dT;
				double	t_B = -stepSize * eps * grad_dB;
				if ( t_T*t_T + t_B*t_B < tol*tol )
					break;

				previousRt = Rt;
				previousRb = Rb;
				Rt += t_T;
				Rb += t_B;
				iterationsCount++;

if ( _debugGraph ) {
	graph.DrawLine( pipo, new float2( ((float) previousRt - rangeX.x) *  graph.Width / (rangeX.y - rangeX.x), ((float) previousRb - rangeY.y) * graph.Height / (rangeY.x - rangeY.y) )
						, new float2( ((float) Rt - rangeX.x) *  graph.Width / (rangeX.y - rangeX.x), ((float) Rb - rangeY.y) * graph.Height / (rangeY.x - rangeY.y) ) );
}

				// Keep best results
				previousSqDistance = sqDistance;
				if ( sqDistance < bestSqDistance ) {
					bestSqDistance = sqDistance;
					bestRt = previousRt;
					bestRb = previousRb;
					bestIterationsCount = iterationsCount;
				}
			}

			Rt = Math.Max( -10000.0, Math.Min( 10000.0, Rt ) );
			Rb = Math.Max( -10000.0, Math.Min( 10000.0, Rb ) );

if ( _debugGraph ) {
	panelOutputGraph.m_bitmap = graph.AsBitmap;
	panelOutputGraph.Refresh();
	labelResult.Text = "R = " + Rt.ToString( "G4" ) + ", " + Rb.ToString( "G4" ) + " (" + previousSqDistance.ToString( "G4" ) + ") in " + iterationsCount + " iterations...\r\nBest = " + bestRt.ToString( "G4" ) + ", " + bestRb.ToString( "G4" ) + " (" + bestSqDistance.ToString( "G4" ) + ") in " + bestIterationsCount + " iterations...";
}

			return new float2( (float) Rt, (float) Rb );
		}

		void	UpdateGraph() {
// 			float3	P = new float3( 1, 1, 1 );
// 			float3	N = P.Normalized;
// 			float	R = ComputeTangentSphereRadius( P, N, new float3[] { new float3( 1, 1, -1 ), new float3( -1, 1, 1 ), new float3( 1, -1, 1 ) } );
// //			float	R = ComputeTangentSphereRadius( P, N, new float3[] { new float3( 1, 1, 0 ), new float3( 0, 1, 1 ), new float3( 1, 0, 1 ) } );
// 
// // 			float3	P = new float3( 1, 1, 0 );
// // 			float3	N = P.Normalized;
// // 			float	R = ComputeTangentSphereRadius( P, N, new float3[] { new float3( -1, 1, 0 ), new float3( 1, -1, 0 ) } );

			float3	V0 = float3.One;
			float3	N0 = V0.Normalized;
			float3	V1 = new float3( 1, 1, -1 );
			float3	V2 = new float3( -1, 1, 1 );
			float3	V3 = new float3( 1, -1, 1 );
			float3	D0 = V1 - V0;
			float3	D1 = V2 - V0;
			float3	D2 = V3 - V0;
			float	L = D0.Length;
			float	L_ = (V1 - D0.Dot( N0 ) * N0 - V0).Length;

			float3	P = float3.Zero;
			float3	N = float3.UnitZ;
			float	A = floatTrackbarControlA.Value;
			float	B = floatTrackbarControlB.Value;
			float	C = floatTrackbarControlC.Value;
			float	a = (float) (2 * Math.Sqrt( 2.0 / 3 ));
			float	b = a * (float) Math.Sqrt( 3 ) / 2.0f;
// 			float	L1 = (new float3( 0, a, A ) - P).Length;
// 			float	L2 = (new float3( -b, -0.5f * a, A ) - P).Length;
// 			float	L3 = (new float3( b, -0.5f * a, A ) - P).Length;
//			float	R = ComputeTangentSphereRadius( P, N, new float3[] { new float3( 0, a, A ), new float3( -b, -0.5f * a, B ), new float3( b, -0.5f * a, C ) }, true );

			float	D = floatTrackbarControlD.Value;
// 			float2	curvature = ComputeTangentCurvatureRadii( P, N, float3.UnitX, new float3[] { new float3( -1, 0, A ), new float3( +1, 0, B ), new float3( 0, -1, C ), new float3( 0, +1, D ) }, true );

			// Torus inner vertex
					P = new float3( 0.5f, 0.0f, 0.0f );
					N = new float3( -1.0f, 0.0f, 0.0f );
			float3	T = new float3( 0.0f, 1.0f, 0.0f );
			float3[]	neighbors = new float3[] {
				new float3( 0.566346049f, -0.184016943f, 0.2938926f ),
				new float3( 0.595491469f, 0.0f, 0.2938926f ),
				new float3( 0.566346049f, 0.184016988f, 0.2938926f ),
				new float3( 0.47552827f, -0.154508471f, 0.0f ),
				new float3( 0.47552827f, 0.1545085f, 0.0f ),
				new float3( 0.5663461f, -0.184016973f, -0.293892682f ),
				new float3( 0.5954915f, 0.0f, -0.293892682f ),
				new float3( 0.5663461f, 0.184017f, -0.293892682f )
			};
			float2	curvature2 = ComputeTangentCurvatureRadii( P, N, T, neighbors, true );

					P = new float3( 1.5f, 0.0f, 0.0f );
					N = new float3( 1.0f, 0.0f, 0.0f );
					T = new float3( 0.0f, 1.0f, 0.0f );
			neighbors = new float3[] {
				new float3( 1.335767f, -0.4340169f, -0.293892652f ),
				new float3( 1.40450847f, 0.0f, -0.293892652f ),
				new float3( 1.335767f, 0.434017f, -0.293892652f ),
				new float3( 1.42658484f, -0.4635254f, 0.0f ),
				new float3( 1.42658484f, 0.4635255f, 0.0f ),
				new float3( 1.335767f, -0.4340169f, 0.293892622f ),
				new float3( 1.40450847f, 0.0f, 0.293892622f ),
				new float3( 1.335767f, 0.434017f, 0.293892622f ),
			};
			float2	curvature3 = ComputeTangentCurvatureRadii( P, N, T, neighbors, true );
		}

		#endregion

		#region Primitive Building

		const float	BEND = 1.0f;

		Primitive	BuildCube() {
			// Default example face where B is used to stored the triangle's center and 
// 			float3	C0 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3( -1.0f, -1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f )) / 3.0f;
// 			float3	C1 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f ) + new float3(  1.0f,  1.0f, 1.0f )) / 3.0f;

			float	R = (float) Math.Sqrt( 3.0f ) / BEND;
			float3	C0 = new float3( R, 0, 0 );
			float3	C1 = new float3( R, 0, 0 );

			VertexP3N3G3B3T2[]	defaultFace = new VertexP3N3G3B3T2[6] {
				// First triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3( -1.0f, -1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 1, 1 ) },

				// 2nd triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f,  1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 0 ) },
			};

			float3[]	faceNormals = new float3[6] {
				-float3.UnitX,
				 float3.UnitX,
				-float3.UnitY,
				 float3.UnitY,
				-float3.UnitZ,
				 float3.UnitZ,
			};
			float3[]	faceTangents = new float3[6] {
				 float3.UnitZ,
				-float3.UnitZ,
				 float3.UnitX,
				 float3.UnitX,
				-float3.UnitX,
				 float3.UnitX,
			};

			Func<float3,float3,float3,float3,float3>	lambdaTransform = ( float3 _P, float3 _T, float3 _B, float3 _N ) => _P.x * _T + _P.y * _B + _P.z * _N;

			VertexP3N3G3B3T2[]	vertices = new VertexP3N3G3B3T2[6*2*3];
			for ( int faceIndex=0; faceIndex < 6; faceIndex++ ) {
				float3	N = faceNormals[faceIndex];
				float3	T = faceTangents[faceIndex];
				float3	B = N.Cross( T );

				for ( int i=0; i < 6; i++ ) {
					VertexP3N3G3B3T2	V = defaultFace[i];
					vertices[6*faceIndex+i].P = lambdaTransform( V.P, T, B, N );
					vertices[6*faceIndex+i].N = lambdaTransform( V.N, T, B, N );
					vertices[6*faceIndex+i].T = lambdaTransform( V.T, T, B, N );
//					vertices[6*faceIndex+i].B = lambdaTransform( V.B, T, B, N );
vertices[6*faceIndex+i].B = V.B;
					vertices[6*faceIndex+i].UV = V.UV;
				}
			}
			uint[]				indices = new uint[6 * 2*3] {
// 				0, 1, 2, 0, 2, 3,
// 				4, 5, 6, 4, 6, 7,
// 				8, 9, 10, 8, 10, 11,
// 				12, 13, 14, 12, 14, 15,
// 				16, 17, 18, 16, 18, 19,
// 				20, 21, 22, 20, 22, 23,
				0, 1, 2, 3, 4, 5,
				6, 7, 8, 9, 10, 11,
				12, 13, 14, 15, 16, 17,
				18, 19, 20, 21, 22, 23,
				24, 25, 26, 27, 28, 29,
				30, 31, 32, 33, 34, 35
			};

			return new Primitive( m_device, (uint) vertices.Length, VertexP3N3G3B3T2.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
		}

		Primitive	BuildSubdividedCube() {
			// Default example face where B is used to stored the triangle's center and 
			float3	C0 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3( -1.0f, -1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f )) / 3.0f;
			float3	C1 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f ) + new float3(  1.0f,  1.0f, 1.0f )) / 3.0f;

			VertexP3N3G3B3T2[]	defaultFace = new VertexP3N3G3B3T2[6] {
				// First triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3( -1.0f, -1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 1, 1 ) },

				// 2nd triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( BEND * -1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f,  1.0f, 1.0f ), N = new float3( BEND *  1.0f, BEND *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 0 ) },
			};

			float3[]	faceNormals = new float3[6] {
				-float3.UnitX,
				 float3.UnitX,
				-float3.UnitY,
				 float3.UnitY,
				-float3.UnitZ,
				 float3.UnitZ,
			};
			float3[]	faceTangents = new float3[6] {
				 float3.UnitZ,
				-float3.UnitZ,
				 float3.UnitX,
				 float3.UnitX,
				-float3.UnitX,
				 float3.UnitX,
			};

			Func<float3,float3,float3,float3,float3>	lambdaTransform = ( float3 _P, float3 _T, float3 _B, float3 _N ) => _P.x * _T + _P.y * _B + _P.z * _N;

			List< VertexP3N3G3B3T2 >	vertices = new List<VertexP3N3G3B3T2>();
			List< uint >				indices = new List<uint>();

			const uint	SUBVIVS_COUNT = 5;

			VertexP3N3G3B3T2[]	temp = new VertexP3N3G3B3T2[3];
//			for ( int faceIndex=3; faceIndex < 4; faceIndex++ ) {
			for ( int faceIndex=0; faceIndex < 6; faceIndex++ ) {
				float3	N = faceNormals[faceIndex];
				float3	T = faceTangents[faceIndex];
				float3	B = N.Cross( T );

				for ( uint triIndex=0; triIndex < 2; triIndex++ ) {
					for ( int i=0; i < 3; i++ ) {
						VertexP3N3G3B3T2	V = defaultFace[3*triIndex+i];
						temp[i].P = lambdaTransform( V.P, T, B, N );
						temp[i].N = lambdaTransform( V.N, T, B, N );
						temp[i].T = lambdaTransform( V.T, T, B, N );
						temp[i].B = lambdaTransform( V.B, T, B, N );
						temp[i].UV = V.UV;
					}

					SubdivTriangle( temp, SUBVIVS_COUNT, vertices, indices );
				}
			}

			return new Primitive( m_device, (uint) vertices.Count, VertexP3N3G3B3T2.FromArray( vertices.ToArray() ), indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
		}

		void		SubdivTriangle( VertexP3N3G3B3T2[] _triangle, uint _count, List< VertexP3N3G3B3T2 > _vertices, List< uint > _indices ) {
			if ( _count == 0 ) {
				// Push triangle
// _triangle[0].B.x = (float) _vertices.Count;
// _triangle[1].B.x = (float) _vertices.Count+1;
// _triangle[2].B.x = (float) _vertices.Count+2;
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[0] );
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[1] );
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[2] );
				return;
			}

			VertexP3N3G3B3T2[]	insideTriangle = new VertexP3N3G3B3T2[3] {
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[0].P + _triangle[1].P), N = 0.5f * (_triangle[0].N + _triangle[1].N), UV = 0.5f * (_triangle[0].UV + _triangle[1].UV), T = 0.5f * (_triangle[0].T + _triangle[1].T), B = 0.5f * (_triangle[0].B + _triangle[1].B) },
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[1].P + _triangle[2].P), N = 0.5f * (_triangle[1].N + _triangle[2].N), UV = 0.5f * (_triangle[1].UV + _triangle[2].UV), T = 0.5f * (_triangle[1].T + _triangle[2].T), B = 0.5f * (_triangle[1].B + _triangle[2].B) },
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[2].P + _triangle[0].P), N = 0.5f * (_triangle[2].N + _triangle[0].N), UV = 0.5f * (_triangle[2].UV + _triangle[0].UV), T = 0.5f * (_triangle[2].T + _triangle[0].T), B = 0.5f * (_triangle[2].B + _triangle[0].B) },
			};

			VertexP3N3G3B3T2[]	temp = new VertexP3N3G3B3T2[3];
			temp[0] = _triangle[0];
			temp[1] = insideTriangle[0];
			temp[2] = insideTriangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[0];
			temp[1] = _triangle[1];
			temp[2] = insideTriangle[1];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[0];
			temp[1] = insideTriangle[1];
			temp[2] = insideTriangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[2];
			temp[1] = insideTriangle[1];
			temp[2] = _triangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
		}

// 		const uint	SUBDIVS0 = 40;
// 		const uint	SUBDIVS1 = 20;
		const uint	SUBDIVS0 = 20;
		const uint	SUBDIVS1 = 10;
		const float	RADIUS0 = 1.0f;
		const float	RADIUS1 = 0.5f;

		Primitive	BuildTorus() {
			List< VertexP3N3G3B3T2 >	vertices = new List<VertexP3N3G3B3T2>();
			List< uint >				indices = new List<uint>();

			// Build vertices
			VertexP3N3G3B3T2	V = new VertexP3N3G3B3T2();
			for ( uint i=0; i < SUBDIVS0; i++ ) {
				float	a0 = 2.0f * (float) Math.PI * i / SUBDIVS0;
				float3	X = new float3( (float) Math.Cos( a0 ), (float) Math.Sin( a0 ), 0.0f );
				float3	Y = new float3( -X.y, X.x, 0.0f );
				float3	Z = X.Cross( Y );
				float3	C = RADIUS0 * X;	// Center of little ring, around large ring

				for ( uint j=0; j < SUBDIVS1; j++ ) {
					float	a1 = 2.0f * (float) Math.PI * j / SUBDIVS1;
					float3	lsN = new float3( (float) Math.Cos( a1 ), (float) Math.Sin( a1 ), 0.0f );
//					float3	lsN2 = new float3( -lsN.y, lsN.x, 0.0f );
					float3	N = lsN.x * X + lsN.y * Z;
//					float3	N2 = lsN2.x * X + lsN2.y * Z;

					V.P = C + RADIUS1 * N;
					V.N = N;
					V.T = Y;
					V.UV = new float2( 4.0f * i / SUBDIVS0, 1.0f * j / SUBDIVS1 );

					vertices.Add( V );
				}
			}

			// Build indices and curvature
			uint[,]			neighborIndices = new uint[3,3];
			float3[]		neighbors = new float3[8];
			List< float2 >	curvatures = new List<float2>();
			float2			minCurvature = new float2( float.MaxValue, float.MaxValue );
			float2			maxCurvature = new float2( -float.MaxValue, -float.MaxValue );
			float2			avgCurvature = float2.Zero;
			int				count = 0;
			for ( uint i=0; i < SUBDIVS0; i++ ) {
				uint	Pi = (i+SUBDIVS0-1) % SUBDIVS0;
				uint	Ni = (i+1) % SUBDIVS0;

				uint	ringCurrent = SUBDIVS1 * i;
				uint	ringPrevious = SUBDIVS1 * Pi;
				uint	ringNext = SUBDIVS1 * Ni;

				for ( uint j=0; j < SUBDIVS1; j++ ) {
					uint	Pj = (j+SUBDIVS1-1) % SUBDIVS1;
					uint	Nj = (j+1) % SUBDIVS1;

					neighborIndices[0,0] = ringPrevious + Pj;
					neighborIndices[0,1] = ringPrevious + j;
					neighborIndices[0,2] = ringPrevious + Nj;
					neighborIndices[1,0] = ringCurrent + Pj;
					neighborIndices[1,1] = ringCurrent + j;
					neighborIndices[1,2] = ringCurrent + Nj;
					neighborIndices[2,0] = ringNext + Pj;
					neighborIndices[2,1] = ringNext + j;
					neighborIndices[2,2] = ringNext + Nj;

					// Build 2 triangles
					indices.Add( neighborIndices[1,2] );
					indices.Add( neighborIndices[1,1] );
					indices.Add( neighborIndices[2,1] );
					indices.Add( neighborIndices[1,2] );
					indices.Add( neighborIndices[2,1] );
					indices.Add( neighborIndices[2,2] );

					// Compute central vertex's curvature
					VertexP3N3G3B3T2	centerVertex = vertices[(int) neighborIndices[1,1]];
					neighbors[0] = vertices[(int) neighborIndices[0,0]].P;
					neighbors[1] = vertices[(int) neighborIndices[1,0]].P;
					neighbors[2] = vertices[(int) neighborIndices[2,0]].P;
					neighbors[3] = vertices[(int) neighborIndices[0,1]].P;
//					neighbors[] = vertices[(int) neighborIndices[1,1]].P;
					neighbors[4] = vertices[(int) neighborIndices[2,1]].P;
					neighbors[5] = vertices[(int) neighborIndices[0,2]].P;
					neighbors[6] = vertices[(int) neighborIndices[1,2]].P;
					neighbors[7] = vertices[(int) neighborIndices[2,2]].P;

					// Single curvature
// 					float	curvature = ComputeTangentSphereRadius( centerVertex.P, centerVertex.N, neighbors, false );
// 					centerVertex.B.x = curvature;
				
					// Double curvature
					float2	curvature = ComputeTangentCurvatureRadii( centerVertex.P, centerVertex.N, centerVertex.T, neighbors, false );
					centerVertex.B.x = curvature.x;
					centerVertex.B.y = curvature.y;

					vertices[(int) neighborIndices[1,1]] = centerVertex;

					curvatures.Add( curvature );
					minCurvature.Min( curvature );
					maxCurvature.Max( curvature );
					avgCurvature += curvature;
					count++;
				}
			}
			avgCurvature /= count;

			labelMeshInfo.Text = "Curvature Avg. = " + avgCurvature + " - Min = " + minCurvature + " - Max = " + maxCurvature;

			return new Primitive( m_device, (uint) vertices.Count, VertexP3N3G3B3T2.FromArray( vertices.ToArray() ), indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
		}

		#endregion

		void m_camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_camera.Proj2Camera;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;
//			m_CB_Camera.m._Proj2World = m_CB_Camera.m._World2Proj.Inverse;

			m_CB_Camera.UpdateData();
		}

		DateTime	m_startTime;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			m_CB_Main.m._resolutionX = (uint) panelOutput.Width;
			m_CB_Main.m._resolutionY = (uint) panelOutput.Height;
			m_CB_Main.m._time = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_Main.m._flags = (uint) (	(checkBoxShowNormal.Checked ? 1U : 0U)
										  | (checkBoxEnableCorrection.Checked ? 2U : 0U)
										);
			m_CB_Main.m._bend = floatTrackbarControlCurvatureStrength.Value;
			m_CB_Main.UpdateData();

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
			m_device.Clear( float4.Zero );
			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, (byte) 0U, true, false );

// 			if ( m_Shader_renderScene.Use() ) {
// 				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );
// 				m_Prim_Cube.Render( m_Shader_renderScene );
// 			}
			if ( m_Shader_renderSceneFinal.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );
				m_Prim_Torus.Render( m_Shader_renderSceneFinal );
			}

			m_device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		private void floatTrackbarControlA_ValueChanged(FloatTrackbarControl _Sender, float _fFormerValue) {
			UpdateGraph();
		}
	}
}
