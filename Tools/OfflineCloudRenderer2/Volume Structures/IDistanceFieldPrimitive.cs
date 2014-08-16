using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace OfflineCloudRenderer2
{
	/// <summary>
	/// This interface is implemented by any distance field primitive
	/// </summary>
	public interface IDistanceFieldPrimitive
	{
		/// <summary>
		/// Gets the minimum dimensions of the primitive
		/// </summary>
		float3	Min		{ get; }

		/// <summary>
		/// Gets the maximum dimensions of the primitive
		/// </summary>
		float3	Max		{ get; }

		/// <summary>
		/// Reurns the distance from the primitive at this position
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		float	Eval( float3 _Position );
	}
}
