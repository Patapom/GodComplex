using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestBinaryTreeIntersect
{
	/// <summary>
	/// This is a C# implementation of the code from https://www.shadertoy.com/view/XsfSRn written by Hubw (Huw Bowles from Studio Gobo)
	/// This code is adapted to volume ray-marching but was originally intended for geometric refinement of a view-dependent mesh used
	///	 to render ocean, as described in http://advances.realtimerendering.com/s2013/OceanShoestring_SIGGRAPH2013_Online.pptx presented
	///	 at http://advances.realtimerendering.com/s2013/
	/// 
	/// The idea is to compute a fixed set of sampling distances on the CPU and send them to the GPU to know where to sample the volume.
	/// These points are "static" in the sense that they move along the camera's Z displacement and get split at a constant rate depending
	///  on the distance to the camera: the closer to the camera, the more samples we have.
	/// The important point is that the points must appear to be static in world space to avoid popping.
	/// 
	/// I improved on the idea by using 9 sets of these sampling distances by splitting the camera frustum into 4 distinct parts:
	/// 
	/// 
	/// 
	/// </summary>
	public partial class IntersectForm : Form
	{
		public IntersectForm()
		{
			InitializeComponent();
		}
	}
}
