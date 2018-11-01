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
	public partial class Form24 : Form
	{
		static
		private int m_last_idx = 0;
		static
		private Point m_last_loc;

		public Form24()
		{
			InitializeComponent();
		}

		private void Form24_Load(object sender, EventArgs e)
		{
			DDX(true);
			this.tabControl1.SelectedIndex = m_last_idx;
			if (m_last_loc.X != 0 || m_last_loc.Y != 0) {
				this.Location = m_last_loc;
			}
			G.FORM24 = this;
			checkBox1_CheckedChanged(null, null);
		}

		private void Form24_FormClosing(object sender, FormClosingEventArgs e)
		{
			m_last_idx = this.tabControl1.SelectedIndex;
			m_last_loc = this.Location;
			G.FORM24 = null;
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				//キューティクルライン
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton5, this.radioButton6}, ref G.SS.MOZ_CND_CTYP);
				DDV.DDX(bUpdate, this.numericUpDown7 , ref G.SS.MOZ_CND_BPF1);
				DDV.DDX(bUpdate, this.numericUpDown8 , ref G.SS.MOZ_CND_BPF2);
				DDV.DDX(bUpdate, this.comboBox4      , ref G.SS.MOZ_CND_BPSL);
				DDV.DDX(bUpdate, this.numericUpDown1 , ref G.SS.MOZ_CND_NTAP);
				DDV.DDX(bUpdate, this.numericUpDown9 , ref G.SS.MOZ_CND_BPVL);
				DDV.C2V(bUpdate, this.comboBox13     , ref G.SS.MOZ_CND_2DC0);
				DDV.C2V(bUpdate, this.comboBox14     , ref G.SS.MOZ_CND_2DC1);
				DDV.C2V(bUpdate, this.comboBox15     , ref G.SS.MOZ_CND_2DC2);
				DDV.DDX(bUpdate, this.numericUpDown10, ref G.SS.MOZ_CND_2DVL);
				//毛髪径算出
				//DDV.DDX(bUpdate, this.checkBox10     , ref G.SS.MOZ_CND_CKBK);//上下端黒除外
				//DDV.DDX(bUpdate, this.numericUpDown11 , ref G.SS.MOZ_CND_BKVL);//上下端黒除外・閾値

				DDV.DDX(bUpdate, this.comboBox3      , ref G.SS.MOZ_CND_FTCF);//画像・平滑化フィルタ
				DDV.DDX(bUpdate, this.comboBox5      , ref G.SS.MOZ_CND_FTCT);//回数
				DDV.DDX(bUpdate, this.comboBox6      , ref G.SS.MOZ_CND_SMCF);//スムージング・重み係数
				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_CND_CNTR);//コントラスト補正
				DDV.DDX(bUpdate, this.numericUpDown3 , ref G.SS.MOZ_CND_ZVAL);//毛髄判定画素閾値
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.MOZ_CND_HANI);//径方向・毛髄判定範囲
				DDV.DDX(bUpdate, this.numericUpDown19, ref G.SS.MOZ_CND_SLVL);//面積Sl,Sd判定閾値
				DDV.DDX(bUpdate, this.numericUpDown11, ref G.SS.MOZ_CND_OTW1);//外れ値判定:幅  (毛髄長さ)
				DDV.DDX(bUpdate, this.numericUpDown12, ref G.SS.MOZ_CND_OTV1);//外れ値判定:閾値(毛髄長さ)
				DDV.DDX(bUpdate, this.numericUpDown13, ref G.SS.MOZ_CND_OTW2);//外れ値判定:幅  (毛髄中心)
				DDV.DDX(bUpdate, this.numericUpDown14, ref G.SS.MOZ_CND_OTV2);//外れ値判定:閾値(毛髄中心)
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_OTMD);//外れ値判定:補間,1:直線補間
				DDV.DDX(bUpdate, this.numericUpDown15 , ref G.SS.MOZ_CND_SMVL);//除外判定:面積値
				DDV.DDX(bUpdate, this.checkBox4      , ref G.SS.MOZ_CND_CHK1);//有,無効:除外判定:毛髄面積	
				DDV.DDX(bUpdate, this.checkBox5      , ref G.SS.MOZ_CND_CHK2);//有,無効:外れ値判定:毛髄長さ
				DDV.DDX(bUpdate, this.checkBox6      , ref G.SS.MOZ_CND_CHK2);//有,無効:外れ値判定:毛髄中心
#if true//2018.10.30(キューティクル長)
				DDV.DDX(bUpdate, this.numericUpDown2 , ref G.SS.MOZ_CND_CHAN);//
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.MOZ_CND_CMIN);//
#endif
				rc = true;
            }
            catch (Exception e) {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}
		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if ((this.numericUpDown1.Value % 2) == 0) {
				this.numericUpDown1.Value = this.numericUpDown1.Value+1;
			}
		}
		private void button1_Click(object sender, EventArgs e)
		{//キューティクル更新
			if (!DDX(false)) {
				return;
			}
			Cursor.Current = Cursors.WaitCursor;
			//
			G.FORM03.UPDATE_CUTICLE();
			//
			Cursor.Current = Cursors.Default;
			this.textBox2.Text = G.FORM03.m_errstr;
		}

		private void button2_Click(object sender, EventArgs e)
		{//毛髄更新
			if (!DDX(false)) {
				return;
			}
			Cursor.Current = Cursors.WaitCursor;
			//
			G.FORM03.UPDATE_MOUZUI();
			//
			Cursor.Current = Cursors.Default;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDown11.Enabled = (this.checkBox5.Checked == true);
			this.numericUpDown12.Enabled = (this.checkBox5.Checked == true);
			//
			this.numericUpDown13.Enabled = (this.checkBox6.Checked == true);
			this.numericUpDown14.Enabled = (this.checkBox6.Checked == true);
			//
			this.numericUpDown15.Enabled = (this.checkBox4.Checked == true);
		}
	}
}
