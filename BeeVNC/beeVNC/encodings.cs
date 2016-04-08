/************************************
 * Developed by Kristian Reukauff
 * License and Project:
 * https://beevnc.codeplex.com/
 * Published under NewBSD-License
 * without any warrenties
 * provided "as is"
 ************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace beeVNC
{
    #region Raw

    /// <summary>
    /// Raw Encoding (see 6.5.1)
    /// </summary>
    public static class RawRectangle
    {
		  //public static byte[, ,] EncodeRawRectangle(int frameHeight, int frameWidth, byte[] frameData)
		  //{
		  //	 byte[,,] retValue = new byte[frameWidth,frameHeight,4];

		  //	 for (int h = 0; h < frameHeight; h++)
		  //	 {
		  //		  for (int w = 0; w < frameWidth; w++)
		  //		  {
		  //				//Update Pixel for Backbuffer (Read every Frame)
		  //				retValue[w, h, 2] = frameData[h * frameWidth * 4 + w * 4]; //blue
		  //				retValue[w, h, 1] = frameData[h * frameWidth * 4 + w * 4 + 1]; //green
		  //				retValue[w, h, 0] = frameData[h * frameWidth * 4 + w * 4 + 2]; //red
		  //				retValue[w, h, 3] = frameData[h * frameWidth * 4 + w * 4 + 3]; //alpha

		  //		  }
		  //	 }

		  //	 return (retValue);
		  //}
    }
    #endregion
}
