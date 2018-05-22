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
	public partial class Form10:Form
	{
		public Form10()
		{
			InitializeComponent();
		}
		private void init()
		{
			this.numericUpDown1.Value = G.SS.LED_PWM_VAL[0];
			this.numericUpDown3.Value = G.SS.LED_PWM_VAL[1];
			this.numericUpDown2.Value = G.SS.LED_PWM_VAL[2];
		}
		private void Form10_Load(object sender, EventArgs e)
		{
			init();
		}
		//public void LED_CL(bool on)
		//{
		//    LED_SET(/*透過*/0, on);
		//}
		//public void LED_IR(bool on)
		//{
		//    LED_SET(/*赤外*/1, on);
		//}
		//
		// il=0:LED白色(透過), il=1:LED白色(反射), il=2:LED赤外
		//
		public void LED_SET(int il, bool on)
		{
			CheckBox[] chks = {
				this.checkBox1, this.checkBox2, this.checkBox3
			};
			NumericUpDown[] nums = {
				this.numericUpDown1, this.numericUpDown2, this.numericUpDown3
			};
			if (on) {
				D.SET_LED_DUTY(il, (int)nums[il].Value);
				D.SET_LED_STS(il, 1);//ON
				chks[il].Checked = true;

				if (G.SS.LED_PWM_AUTO) {
					for (int i = 0; i < chks.Length; i++) {
						if (i != il && chks[i].Checked) {
							D.SET_LED_STS(i, 0);//AUTO-OFF
							chks[i].Checked = false;
						}
					}
				}
				if (G.SS.CAM_PAR_AUTO /*&& G.FORM02 != null && G.FORM02.isCONNECTED()*/) {
					G.FORM12.set_param_auto(il);
				}
				if (G.SS.CAM_PAR_AUTO) {
					G.FORM12.set_imp_param(il, -1);
				}
			}
			else {
				D.SET_LED_STS(il, 0);//OFF
				chks[il].Checked = false;
			}
			if (on && G.FORM13 != null) {
				G.FORM13.LED_SET(il);
			}
		}
		public void UPDSTS()
		{
			if (D.isCONNECTED()) {
				this.checkBox1.Enabled = true;
				this.checkBox3.Enabled = true;
				this.checkBox2.Enabled = true;
			}
			else {
				this.checkBox1.Enabled = false;
				this.checkBox3.Enabled = false;
				this.checkBox2.Enabled = false;
			}
		}
		private void OnClicks(object sender, EventArgs e)
		{
			if (false) {
			}
			else if (sender == this.checkBox1) {
				LED_SET(0, this.checkBox1.Checked==true);//白色LED(透過)
			}
			else if (sender == this.checkBox2) {
				LED_SET(1, this.checkBox2.Checked==true);//白色LED(反射)
			}
			else if (sender == this.checkBox3) {
				LED_SET(2, this.checkBox3.Checked==true);//赤外LED
			}
		}
		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (sender == this.numericUpDown1) {
				if (D.isCONNECTED()) {//白色LED(透過)
					D.SET_LED_DUTY(0, (int)this.numericUpDown1.Value);
				}
			}
			else if (sender == this.numericUpDown2) {
				if (D.isCONNECTED()) {//白色LED(反射)
					D.SET_LED_DUTY(1, (int)this.numericUpDown2.Value);
				}
			}
			else if (sender == this.numericUpDown3) {
				if (D.isCONNECTED()) {//赤外LED
					D.SET_LED_DUTY(2, (int)this.numericUpDown3.Value);
				}
			}
		}
	}
}
