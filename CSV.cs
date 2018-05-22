using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace uSCOPE
{
	class CSV
	{
		public int		c_max;
		public int		r_max;
		public bool		bTabSep;
		private int		cnt;
		private
		Dictionary <uint, string>
						dict;
//		private int		dummy;
		/**/
		public
		CSV()
		{
			this.bTabSep = false;
			clear();
			//this.dict = new Dictionary<uint, string>();
			//this.c_max = 0;
			//this.r_max = 0;
			//this.cnt = 0;
		}
		public
		CSV(bool bTabSep)
		{
			this.bTabSep = bTabSep;
			clear();
			//this.dict = new Dictionary<uint, string>();
			//this.c_max = 0;
			//this.r_max = 0;
			//this.cnt = 0;
		}
		public
		void clear()
		{
			this.dict = new Dictionary<uint, string>();
			this.c_max = 0;
			this.r_max = 0;
			this.cnt = 0;
		}
		public
		void set(int c, int r, string str)
		{
			uint	key = (uint)((c << 16) | r);

			if (this.c_max < c) {
				this.c_max = c;
			}
			if (this.r_max < r) {
				this.r_max = r;
			}
#if true
			if (str.IndexOf(',') >= 0) {
				str = "\"" + str + "\"";
			}
			if (!this.dict.ContainsKey(key)) {
				this.cnt++;
			}
			else {
				this.dict.Remove(key);
			}
			this.dict.Add(key, str);
#else
			if (!this.dict.ContainsKey(key)) {
				this.cnt++;
			}
			if (str.IndexOf(',') >= 0) {
				str = "\"" + str + "\"";
			}
			this.dict.Add(key, str);
#endif
		}
		public
		String get(int c, int r)
		{
			uint	key = (uint)((c << 16) | r);

			if (!this.dict.ContainsKey(key)) {
				return ("");
			}
			return (this.dict[key]);
		}
		public
		void ToCsvText(out string str)
		{
			StringBuilder buf = new StringBuilder();
			String	sep;
			if (bTabSep) {
				sep = "\t";
			}
			else {
				sep = ",";
			}
			for (int r = 0; r <= this.r_max; r++) {
				for (int c = 0; c <= this.c_max; c++) {
					if (c > 0) {
						buf.Append(sep);
					}
					buf.Append(get(c, r));
				}
				buf.Append("\r\n");
			}
			str = buf.ToString();
		}
		public
		Boolean load(string filename)
		{
			StreamReader
						rd;
			String buf;
			String[] clms;
			char[] sep = { ',', '\t'};
			bool rc = false;
			int		r = 0;

			if (bTabSep) {
				sep = new char[] { '\t'};
			}
			else {
				sep = new char[] { ','};
			}

			try {
/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				rd = new StreamReader(filename, Encoding.Default);
				while ((buf = rd.ReadLine()) != null)
				{
					if (r >= 4720) {
						//r = r;
					}
					buf = buf.Trim();
					clms = buf.Split(sep);
					for (int c = 0; c < clms.Length; c++) {
						set(c, r, clms[c]);
					}
					r++;
				}
				rd.Close();
				rc = true;
			}
			catch (Exception ex) {
				MessageBox.Show(ex.ToString(), "—áŠO”­¶", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return (rc);
		}
		public
		Boolean save(string filename)
		{
			StreamWriter
						wr;
			string buf;
			char sep;
			bool rc = false;

			if (bTabSep) {
				sep = '\t';
			}
			else {
				sep = ',';
			}

			try {
/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				wr = new StreamWriter(filename, false, Encoding.Default);
				for (int r = 0; r <= this.r_max; r++) {
					buf = get(0, r);
					for (int c = 1; c <= this.c_max; c++) {
						buf += sep;
						buf += get(c, r);
					}
					wr.WriteLine(buf);
				}
				wr.Close();
				rc = true;
			}
			catch (Exception ex) {
				MessageBox.Show(ex.ToString(), "—áŠO”­¶", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return (rc);
		}
	}
}
