using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uSCOPE
{
	public partial class Form35 : Form
	{
		public G.SYSSET	m_ss;
		//
		private NumericUpDown[] numFSPD;
		private NumericUpDown[] numDPLS;
		private NumericUpDown[] numCNDA;
		private NumericUpDown[] numCNDB;
		private NumericUpDown[] numSKIP;
		private NumericUpDown[] numFAVG;
		private NumericUpDown[] numBPLS;
		private ComboBox[]      cmbDTYP;
		private NumericUpDown[] numDROP;
		private NumericUpDown[] numDCNT;
		//
		public Form35()
		{
			InitializeComponent();
		}

		private void Form35_Load(object sender, EventArgs e)
		{
			//
			//(透過:表面, 反射:表面, 透過:中心, 反射:中心)
			numFSPD = new NumericUpDown[] { this.numericUpDown10 , this.numericUpDown28, this.numericUpDown19, this.numericUpDown37};
			numDPLS = new NumericUpDown[] { this.numericUpDown11 , this.numericUpDown29, this.numericUpDown20, this.numericUpDown38};
			numCNDA = new NumericUpDown[] { this.numericUpDown12 , this.numericUpDown30, this.numericUpDown21, this.numericUpDown39};
			numCNDB = new NumericUpDown[] { this.numericUpDown13 , this.numericUpDown31, this.numericUpDown22, this.numericUpDown40};
			numSKIP = new NumericUpDown[] { this.numericUpDown14 , this.numericUpDown32, this.numericUpDown23, this.numericUpDown41};
			numFAVG = new NumericUpDown[] { this.numericUpDown15 , this.numericUpDown33, this.numericUpDown24, this.numericUpDown42};
			numBPLS = new NumericUpDown[] { this.numericUpDown16 , this.numericUpDown34, this.numericUpDown25, this.numericUpDown43};
			numDROP = new NumericUpDown[] { this.numericUpDown17 , this.numericUpDown35, this.numericUpDown26, this.numericUpDown44};
			numDCNT = new NumericUpDown[] { this.numericUpDown18 , this.numericUpDown36, this.numericUpDown27, this.numericUpDown45};
			//
			cmbDTYP = new ComboBox[]      { this.comboBox1, this.comboBox3, this.comboBox2, this.comboBox4};
			//
			DDX(true);

			//checkBox1_CheckedChanged(null, null);
		}

		private bool DDX(bool bUpdate)
        {
            bool rc=false;
			//
			try {
				for (int i = 0; i < 4; i++) {
					//---
					DDV.DDX(bUpdate, numFSPD[i], ref m_ss.IMP_FC2_FSPD[i]);
					DDV.DDX(bUpdate, numDPLS[i], ref m_ss.IMP_FC2_DPLS[i]);
					DDV.DDX(bUpdate, numCNDA[i], ref m_ss.IMP_FC2_CNDA[i]);
					DDV.DDX(bUpdate, numCNDB[i], ref m_ss.IMP_FC2_CNDB[i]);
					DDV.DDX(bUpdate, numSKIP[i], ref m_ss.IMP_FC2_SKIP[i]);
					DDV.DDX(bUpdate, numFAVG[i], ref m_ss.IMP_FC2_FAVG[i]);
					DDV.DDX(bUpdate, numBPLS[i], ref m_ss.IMP_FC2_BPLS[i]);
					DDV.DDX(bUpdate, cmbDTYP[i], ref m_ss.IMP_FC2_DTYP[i]);
					DDV.DDX(bUpdate, numDROP[i], ref m_ss.IMP_FC2_DROP[i]);
					DDV.DDX(bUpdate, numDCNT[i], ref m_ss.IMP_FC2_DCNT[i]);
				}
				//-----
                rc = true;
            }
            catch (Exception e)
            {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}

		private void Form35_Validating(object sender, CancelEventArgs e)
		{
			if (DDX(false) == false) {
				e.Cancel = true;
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < 4; i++) {
				if (cmbDTYP[i].SelectedIndex == 0) {
					numDROP[i].Enabled = true;
					numDCNT[i].Enabled = false;
				}
				else {
					numDROP[i].Enabled = false;
					numDCNT[i].Enabled = true;
				}
			}
		}
	}
}
