using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpMath;

namespace CSharpColorExporter
{
	class Program {
		static void Main(string[] args) {

			//////////////////////////////////////////////////////////////////////////
			// Dump C# colors
			string	colorNames = "";
			string	colorValues = "";
			string	colorConcepts = "";

			int	colorIndex = 0;
			foreach ( object colorName in typeof(KnownColor).GetEnumValues() ) {
				Color	C = Color.FromKnownColor( (KnownColor) colorName );
				string	valueString = C.ToArgb().ToString( "X" );
				colorNames += ", \"" + colorName + "\"";
				colorValues += ", 0x" + valueString;
//				colorValues.Add( new Tuple<string, string>( colorName.ToString(), valueString ) );

//				colorConcepts += ", c." + colorName + "( \"0x" + valueString + "\" )";
				colorConcepts += ", " + colorName.ToString().ToLower() + "( \"0x" + valueString + "\" )";

				colorIndex++;
				if ( (colorIndex % 20) == 0 )
					colorConcepts += "\n";
			}

			//////////////////////////////////////////////////////////////////////////
			// Dump wikipedia colors
			string	wikiConcepts = "w = \"détaillées\" => (\n\t";

			string[]	wikiColors = Colors.colors.Split( '\n' );
			for ( int i=0; i < wikiColors.Length; i++ ) {
				wikiConcepts += Colors.ParseColor( wikiColors[i] );
				if ( (i % 20) == 19 )
					wikiConcepts += "\n\t";
			}
			wikiConcepts += "\n)\n";

			//////////////////////////////////////////////////////////////////////////
			// Group colors in general categories
			string[,]	general = new string[,] {
				{ "simples.beige",			"0xf7e7bd", "" },
				{ "simples.bleu",			"0x54a4e5", "" },
				{ "simples.marron",			"0x8d2b24", "" },
				{ "simples.orange",			"0xff6300", "" },
				{ "simples.rouge",			"0xff0000", "" },
				{ "simples.jaune",			"0xffff00", "" },
				{ "simples.vert",			"0x67d000", "" },
				{ "simples.rose",			"0xff84ff", "" },
				{ "simples.turquoise",		"0x00ffff", "" },
			};

			float3[]	xyY = new float3[general.GetLength(0)];
			for ( int i=0; i < xyY.Length; i++ ) {
//				groupements += (i > 0 ? ", " : "") + general[i,0];
				xyY[i] = Colors.HexRGB2xyY( general[i,1] );
			}

			// Associate all wikipédia colors to their closest general color
			HashSet<string>	existingColorNames = new HashSet<string>();
			for ( int i=0; i < wikiColors.Length; i++ ) {
				string	name, hexRGB;
				Colors.ReadNameHexRGB( wikiColors[i], out name, out hexRGB );

				if ( existingColorNames.Contains(name) )
					throw new Exception( "Identical color names!" );
				existingColorNames.Add( name );

				string	wikiColorName = "w." + name;
				float3	wikixyY = Colors.HexRGB2xyY( hexRGB );

				int		closestColorIndex = -1;
				float	closestSqChroma = float.MaxValue;
				for ( int j=0; j < xyY.Length; j++ ) {
					float	Dx = wikixyY.x - xyY[j].x;
					float	Dy = wikixyY.y - xyY[j].y;
					float	sqChroma = Dx*Dx + Dy*Dy;
					if ( sqChroma >= closestSqChroma )
						continue;
					closestSqChroma = sqChroma;
					closestColorIndex = j;
				}

				general[closestColorIndex,2] += (general[closestColorIndex,2] != "" ? ", " : "") + wikiColorName;
			}

//			string	groupements = "w = \"détaillées\"\n\n";
			string	groupements = "";
			for ( int i=0; i < xyY.Length; i++ ) {
				groupements += general[i,0] + " => ( " + general[i,2] + " )\n";
			}
		}
	}
}
