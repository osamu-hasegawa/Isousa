using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uSCOPE
{
	class Q
	{
#if false
		/************************************************************/
		public static void S2K(string s, out MODE d)
		{
			if (false) {
			}
			else if (string.Compare(s, "REFERENCE", true) == 0) {
				d = MODE.REFERENCE;
			}
			else if (string.Compare(s, "INTENSITY", true) == 0) {
				d = MODE.INTENSITY;
			}
			else if (string.Compare(s, "TRANSMITTANCE", true) == 0) {
				d = MODE.TRANSMITTANCE;
			}
			else if (string.Compare(s, "ABSORBANCE", true) == 0) {
				d = MODE.ABSORBANCE;
			}
			else if (string.Compare(s, "DARK", true) == 0) {
				d = MODE.DARK;
			}
				/*
			else if (string.Compare(s, "SAMPLE", true) == 0) {
				d = MODE.RESPONSE;
			}
			else if (string.Compare(s, "IRRADIANCE", true) == 0) {
				d = MODE.TRANSMITTANCE;
			}*/
#if true//2016.05.01
			else if (string.Compare(s, "CALIBRATION", true) == 0) {
				d = MODE.CALIBRATION;
			}
#endif
			else {
				d = MODE.NULL;
			}
		}
		/************************************************************/
		public static String K2S(MODE d)
		{
			String buf;
			switch (d) {
			case MODE.REFERENCE    : buf = "REFERENCE"    ; break;
			case MODE.INTENSITY    : buf = "INTENSITY"    ; break;
			case MODE.TRANSMITTANCE: buf = "TRANSMITTANCE"; break;
			case MODE.ABSORBANCE   : buf = "ABSORBANCE"   ; break;
			case MODE.DARK         : buf = "DARK"         ; break;
#if true//2016.05.01
			case MODE.CALIBRATION  : buf = "CALIBRATION"  ; break;
#endif
			default:
				throw new Exception("Internal Error");
			}
			return(buf);
		}
#endif
		/************************************************************************/
		public static bool S2D(string buf, out double f)
		{
			f = 0;
			if (string.IsNullOrEmpty(buf)) {
			}
			else if (!double.TryParse(buf, out f)) {
				return(false);
				//throw new Exception("内容に誤りがあります");
			}
			return(true);
		}
		/************************************************************/
		public static bool S2DT(string buf, out DateTime dt)
		{
			dt = DateTime.Parse(buf);
			return(true);
		}
		/************************************************************/
		public static string DT2S(DateTime dt)
		{
			return(dt.ToString());
		}
		/************************************************************/
		public static bool S2I(string buf, out int i)
		{
			
			return(S2I(buf, out i, 0));
		}
		/************************************************************/
		public static bool S2I(string buf, out int i, int def)
		{
			i = def;
			if (string.IsNullOrEmpty(buf)) {
			}
			else if (!int.TryParse(buf, out i)) {
				return(false);
				//throw new Exception("内容に誤りがあります");
			}
			return(true);
		}
		/************************************************************/
		public static string I2S(int d)
		{
			return(d.ToString());
		}
		/************************************************************************/
		/* 分光放射照度.読み込み */
		/************************************************************************/
		public static string F2S(double f)
		{
			string s;
			
			if (double.IsNaN(f)) {
				s = "";
			}
			else {
				s = f.ToString();
			}
			return(s);
		}
		/************************************************************************/
		public static double S2F(string buf, bool bSpaceOK)
		{
			double	f;

			if (bSpaceOK && string.IsNullOrEmpty(buf)) {
				f = double.NaN;
			}
			else if (!double.TryParse(buf, out f)) {
				throw new Exception("内容に誤りがあります");
			}
			return(f);
		}
		/************************************************************************/
		public static double S2F(string buf)
		{
			return(S2F(buf, false));
		}
		/************************************************************/
		public static bool S2F(string buf, out double f)
		{
			f = double.NaN;
			if (string.IsNullOrEmpty(buf)) {
			}
			else if (!double.TryParse(buf, out f)) {
				return(false);
				//throw new Exception("内容に誤りがあります");
			}
			return(true);
		}
		/************************************************************/
		public static bool S2S(string buf, out string s)
		{
			s = buf;
			return(true);
		}
		/************************************************************/
		public static string S2S(string buf)
		{
			if (buf == null) {
				return("");
			}
			return(buf);
		}
		/************************************************************/
		public static bool S2BL(string buf, out int bl)
		{
			bl = 0;
			if (string.Compare(buf, "ON", true) == 0) {
				bl = 1;
			}
			return(true);
		}
		/************************************************************/
		static public int MAKELONG(int lo, int hi)
		{
			return(lo | (hi << 16));
		}
		static public int HIWORD(int dword)
		{
			return((dword >> 16) & 0xFFFF);
		}
		static public int LOWORD(int dword)
		{
			return(dword & 0xFFFF);
		}
	}
}
