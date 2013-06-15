//////////////////////////////////////////////////////////////////////////
// This little application parses a directory for .FXBIN files and concatenates identical prefixed files into a single blob
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConcatenateShaders
{
	class Program
	{
		static void Main( string[] args )
		{
			if ( args.Length != 1 )
				throw new Exception( "You must provide the path to the fxbin files as the only argument!" );

			DirectoryInfo	TargetDirectory = new DirectoryInfo( Path.GetFullPath( args[0] ) );
			if ( !TargetDirectory.Exists )
				throw new Exception( "Specified target directory \"" + TargetDirectory.FullName + "\" does not exist!" );

			// Build the shader files with common names
			Dictionary<string,List<string>>	CommonFiles = new Dictionary<string,List<string>>();

			string[]	Files = Directory.GetFiles( args[0], "*.fxbin" );
			foreach ( string File in Files )
			{
				string	FileNameWithoutExtension = Path.GetFileNameWithoutExtension( File );
						FileNameWithoutExtension = Path.GetFileNameWithoutExtension( FileNameWithoutExtension );	// Double tap as the file has 2 extensions

				if ( !CommonFiles.ContainsKey( FileNameWithoutExtension ) )
					CommonFiles[FileNameWithoutExtension] = new List<string>();
				CommonFiles[FileNameWithoutExtension].Add( File );
			}
			if ( CommonFiles.Count == 0 )
				throw new Exception( "Didn't find any file matching \"" + (args[0] + "\\*.fxbin") + "\"!" );

			// Concatenate all the blobs into a single compact shader file
			foreach ( string ShaderFile in CommonFiles.Keys )
			{
				List<string>	ShaderFiles = CommonFiles[ShaderFile];

				// Build the concatenation infos
				byte[][]	EntryPoints = new byte[ShaderFiles.Count][];
				byte[][]	Blobs = new byte[ShaderFiles.Count][];
				int[]		BlobOffsets = new int[ShaderFiles.Count];

				int			SumEntryPointsLength = 0;
				int			CurrentBlobOffset = 0;
				for ( int FileIndex=0; FileIndex < ShaderFiles.Count; FileIndex++ )
				{
					using ( FileStream S = new FileInfo( ShaderFiles[FileIndex] ).OpenRead() )
					{
						if ( S.Length == 0 )
							throw new Exception( "Shader file is empty!" );

						using ( BinaryReader Reader = new BinaryReader( S ) )
						{
							int	EntryPointLength = Reader.ReadInt32();
							EntryPoints[FileIndex] = Reader.ReadBytes( EntryPointLength );

//							int	BlobLength = (int) (S.Length - S.Position);
							int	BlobLength = Reader.ReadInt32();
							if ( BlobLength > 65535 )
								throw new Exception( "Shader size doesn't fit on 16 bits!" );

							Blobs[FileIndex] = Reader.ReadBytes( BlobLength );
							BlobOffsets[FileIndex] = CurrentBlobOffset;
							CurrentBlobOffset += BlobLength;
							CurrentBlobOffset += 2;	// Account for blob length
							if ( CurrentBlobOffset > 65535 )
								throw new Exception( "Shader offset doesn't fit on 16 bits!" );

							SumEntryPointsLength += EntryPointLength;
						}
					}
				}
				SumEntryPointsLength += 2*ShaderFiles.Count;	// Account for the jump offsets

				// Concatenate into a single binary file
				FileInfo	TargetFile = new FileInfo( Path.Combine( TargetDirectory.FullName, ShaderFile + ".hlsl" ) );
				using ( FileStream S = TargetFile.Create() )
				{
					using ( BinaryWriter Writer = new BinaryWriter( S ) )
					{
						// Write the amount of blobs in the file
						Writer.Write( (ushort) ShaderFiles.Count );

						// Write the "header"
						int	CurrentPosition = 0;
						for ( int FileIndex=0; FileIndex < ShaderFiles.Count; FileIndex++ )
						{
							// Write the entry point
							Writer.Write( EntryPoints[FileIndex] );
							CurrentPosition += EntryPoints[FileIndex].Length;

							// Write the jump offset
							CurrentPosition += 2;	// Account for jump offset
							int	JumpOffset = SumEntryPointsLength - CurrentPosition;
								JumpOffset += BlobOffsets[FileIndex];
							Writer.Write( (ushort) JumpOffset );
						}

						// Write the blobs
						for ( int FileIndex=0; FileIndex < ShaderFiles.Count; FileIndex++ )
						{
							// Write the blob size
							Writer.Write( (ushort) Blobs[FileIndex].Length );

							// Write the blob
							Writer.Write( Blobs[FileIndex] );
						}
					}
				}
			}

			System.Windows.Forms.MessageBox.Show( "Successfully concatenated " + CommonFiles.Count + " files!" );
		}
	}
}
