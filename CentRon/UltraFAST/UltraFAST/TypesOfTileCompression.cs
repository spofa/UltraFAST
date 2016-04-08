/*
 * Created by SharpDevelop.
 * User: sagupta
 * Date: 01-Apr-16
 * Time: 10:45 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace UltraFAST
{
	/// <summary>
	/// TypesOfTileCompression.
	/// </summary>
	public enum TypesOfTileCompression
	{
		NONE = -1,
		ZIP = 0,
		GZIP = 1,
		RAR = 2,
		Lz4Net =3,
	}
}
