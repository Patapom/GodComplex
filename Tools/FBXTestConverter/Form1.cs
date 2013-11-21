using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace FBXTestConverter
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			LoadScene( new FileInfo( @"kiosk.fbx" ) );
		}

		public void	LoadScene( FileInfo _File )
		{
			FBX.SceneLoader.SceneLoader	Loader = new FBX.SceneLoader.SceneLoader();

			FBX.Scene.Scene	Scene = new FBX.Scene.Scene();
			Loader.Load( _File, Scene );
		}
	}
}
