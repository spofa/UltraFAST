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

namespace beeVNC
{
    internal enum SecurityType
    {
        Unknown,
        Invalid, //0
        None, //1
        VNCAuthentication, //2
        RA2, //5
        RA2ne, //6
        Tight, //16
        UltraVNC, //17
        TLS, //18
        VeNCrypt, //19
        GTK_VNC_SASL, //20
        MD5_hash_authentication, //21
        Colin_Dean_xvp //22
    }

    internal enum ServerMessageType
    {
        Unknown,
        FramebufferUpdate, //0
        SetColourMapEntries, //1
        Bell, //2
        ServerCutText, //3
        OLIVE_Call_Control, //249
        Colin_dean_xvp, //250
        tight, //252
        gii, //253
        VMWare, //254/127
        Anthony_Liguori, //255
        Pseudo_DesktopSize, //FF FF FF 21 / -239
        Pseudo_Cursor //FF FF FF 11 / -223
    }

    internal enum ClientMessageType
    {
        Unknown,
        SetPixelFormat, //0
        SetEncodings, //2
        FramebufferUpdateRequest, //3
        KeyEvent, //4
        PointerEvent, //5
        ClientCutText, //6
        OLIVE_Call_Control, //249
        Colin_dean_xvp, //250
        Pierre_Ossman_SetDesktopSize, //251
        tight, //252
        gii, //253
        VMWare, //254/127
        Anthony_Liguori //255
    }

    internal enum RfbEncoding
    {
        ZRLE_ENCODING, //0
		Hextile_ENCODING,  //1
		RRE_ENCODING, //2
		CopyRect_ENCODING, //5
		Raw_ENCODING, //16

        //Inofficials
        CoRRE_ENCODING, //4
        zlib_ENCODING, //6
        tight_ENCODING, //7
        zlibhex_ENCODING, //8
        TRLE_ENCODING, //15
        Hitachi_ZYWRLE_ENCODING, //17
        Adam_Walling_XZ_ENCODING, //18
        Adam_Walling_XZYW_ENCODING, //19
        tight_options_ENCODING, //-240 - -256
        Anthony_Liguori_ENCODING, //-257 - -272
        VMWare_ENCODING, //-273 - -304 + 0x574d5600 - 0x574d56ff
        gii_ENCODING, //-305
        popa_ENCODING, //-306
        Peter_Astrand_DesktopName_ENCODING, //-307
        Pierre_Ossman_ExtendedDesktopSize_ENCODING, //-308
        Colin_dean_xvp_ENCODING, //-309
        OLIVE_Call_Control_ENCODING, //-310
        CursorWithAlpha_ENCODING, //-311
        TurboVNC_fine_grained_quality_level_ENCODING, //-412 - -512
        TurboVNC_subsampling_level_ENCODING, //-763 - -768

        //Pseudo Encodings
        Pseudo_Cursor_ENCODING, //-239
        Pseudo_DesktopSize_ENCODING //-223
    }

    public enum Logtype
    {
        Debug = 6,
        Information = 5,
        Warning = 4,
        Error = 3,
        Critical = 2,
        User = 1,
        None = 0
    }

    public enum KeyCombination
    {
        CtrlAltDel,
        CtrlAltEnd,
        AltTab,
        CtrlEsc,
        AltF4,
        Print,
        NumLock,
        CapsLock,
        Scroll
    }
}
