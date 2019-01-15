using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
//---
using System.Linq;
using System.Collections;

namespace uSCOPE
{
	partial class frmPassword : Form
	{
		public frmPassword()
		{
			InitializeComponent();
		}

		private void frmPassword_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != System.Windows.Forms.DialogResult.OK) {
				return;
			}
			byte[] hash_inp = G.comp_hash(this.textBox1.Text);
			byte[] hash_set;
			if (G.SS.ETC_CPH_HASH == null || G.SS.ETC_CPH_HASH.Length == 0) {
				hash_set = G.comp_hash("miyazaki");
			}
			else {
				hash_set = G.SS.ETC_CPH_HASH;
			}
			if (!hash_inp.SequenceEqual(hash_set)) {
				G.mlog("パスワードが異なります.");
				e.Cancel = true;
			}
		}
	}
}
