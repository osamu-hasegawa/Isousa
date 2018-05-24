using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
//---
using System.Collections;

namespace uSCOPE
{
	class DDV
    {
		static void ThrowErr(Control ctl, string msg)
		{
			if (ctl is TextBox) {
				((TextBox)ctl).SelectAll();
			}
			else if (ctl is NumericUpDown) {
				((NumericUpDown)ctl).Select(0, ctl.Text.Length);
			}
            ctl.Focus();
            throw new Exception(msg);
		}
		static bool ChkNumeric(Control ctl, string txt)
        {
            if (T.IsNumeric(txt) == false) {
				ThrowErr(ctl, "数字を入力してください.");
            }
            return (true);
        }
		static bool ChkNumeric(Control ctl)
        {
            ChkNumeric(ctl, ctl.Text);
            return (true);
        }
		static string ChkMinMax(double val, double min, double max)
        {
			if (min == max) {
                // 範囲チェックなし
				return(null);
            }
            if (val < min || val > max) {
				return(string.Format("{0} ～ {1} の範囲で入力してください.", min, max));
            }
            return (null);
        }
        static public void DDX(bool bUpdate, Control ctl, ref int val)
        {
            if (bUpdate) {
                ctl.Text = val.ToString();
            }
            else {
                ChkNumeric(ctl, ctl.Text);
				if (ctl.GetType().Equals(typeof(NumericUpDown))) {
				val = (int)((NumericUpDown)ctl).Value;
				}
				else {
				val = int.Parse(ctl.Text);
				}
            }
        }
		static public void DDX(bool bUpdate, Control ctl, ref int val, int min, int max)
		{
			if (bUpdate) {
				ctl.Text = val.ToString();
			}
			else {
				ChkNumeric(ctl, ctl.Text);
				val = int.Parse(ctl.Text);
				string msg;
				msg = ChkMinMax(val, min, max);
				if (msg != null) {
					ThrowErr(ctl, msg);
				}
			}
		}
        static public void DDX(bool bUpdate, Control ctl, ref double val)
        {
            if (bUpdate) {
#if true
				if (ctl.GetType().Equals(typeof(NumericUpDown))) {
					int dec = (int)((NumericUpDown)ctl).DecimalPlaces;
					string fmt = "{0:F" + dec.ToString() + "}";

					ctl.Text = string.Format(fmt, val);
				}
				else {
					String	str = String.Format("{0:0.00}", val);
					if (str.Substring(str.Length-3) == ".00") {
						str = str.Substring(0,str.Length-3);
					}
					ctl.Text = str;
				}
#else
				ctl.Text = val.ToString();
#endif
			}
            else {
                ChkNumeric(ctl, ctl.Text);
                val = double.Parse(ctl.Text);
            }
        }
		static public void DDX(bool bUpdate, Control ctl, ref double val, double min, double max)
		{
			if (bUpdate) {
				ctl.Text = val.ToString();
			}
			else {
				ChkNumeric(ctl, ctl.Text);
				val = Double.Parse(ctl.Text);
				string msg = ChkMinMax(val, min, max);
				if (msg != null) {
					ThrowErr(ctl, msg);
				}
			}
		}
		static public void DDX(bool bUpdate, TextBox ctl, ref string str)
        {
           if (bUpdate) {
               ctl.Text = str;
           }
           else {
               str = ctl.Text;
           }
       }
       static public void DDX(bool bUpdate, CheckBox ctl, ref bool chk)
       {
           if (bUpdate) {
               ctl.Checked = chk;
           }
           else {
               chk = ctl.Checked;
           }
       }
       static public void DDX(bool bUpdate, CheckBox ctl, ref int chk)
       {
           if (bUpdate) {
               ctl.Checked = (chk != 0) ? true : false;
           }
           else {
               chk = ctl.Checked ? 1 : 0;
           }
       }
       static public void DDX(bool bUpdate, RadioButton[] ctl, ref int chk)
       {
           if (bUpdate) {
			   for (int i = 0; i < ctl.Length; i++) {
				   ctl[i].Checked = (i == chk);
			   }
           }
           else {
			   chk = -1;
			   for (int i = 0; i < ctl.Length; i++) {
				   if (ctl[i].Checked) {
					   chk = i;
				   }
			   }
           }
		}
		static public void DDX(bool bUpdate, ComboBox ctl, ref string str)
		{
			if (bUpdate) {
				int idx = ctl.FindString(str);
				ctl.SelectedIndex = idx;
			}
			else {
				if (ctl.SelectedItem != null) {
					str = ctl.SelectedItem.ToString();
				}
				else {
					str = "";
				}
			}
		}
		static public void DDX(bool bUpdate, ComboBox ctl, ref int idx)
		{
			if (bUpdate) {
				ctl.SelectedIndex = idx;
			}
			else {
				idx = ctl.SelectedIndex;
			}
		}
		static public void DDX(bool bUpdate, DateTimePicker ctl, ref DateTime tim)
		{
			if (bUpdate) {
				ctl.Value = tim;
			}
			else {
				tim = ctl.Value;
			}
		}
		static public void DDX(bool bUpdate, DateTimePicker ctl1, DateTimePicker ctl2, ref DateTime tim)
		{
			if (bUpdate) {
				/**/
				ctl1.Value = tim;
				/**/
				ctl2.Value = tim;
			}
			else {
				tim = new DateTime(
					ctl1.Value.Year, ctl1.Value.Month, ctl1.Value.Day,
					ctl2.Value.Hour, ctl1.Value.Minute,ctl1.Value.Second);
			}
		}
		static public void DDX(bool bUpdate, TextBox ctl, ref int[] ary, int cnt, int min, int max)
		{
			if (bUpdate) {
				string buf = "";
				if (ary != null && ary.Length > 0) {
					buf += ary[0].ToString();
					for (int i = 1; i < ary.Length; i++) {
						buf += ", ";
						buf += ary[i].ToString();
					}
				}
				ctl.Text = buf;
			}
			else {
				string buf = ctl.Text;
				string[] abuf = null;
				buf = buf.Replace("  ", " ");
				buf = buf.Replace(" ,", ",");
				buf = buf.Replace(", ", ",");
				buf = buf.Replace(" ", ",");
				abuf = buf.Split(',');
				if (abuf.Length <= 0) {
					ary = null;
				}
				else if (abuf.Length == 1 && abuf[0] == "") {
					ary = null;
				}
				else {
					ArrayList ar = new ArrayList();
					for (int i = 0; i < abuf.Length; i++) {
						ChkNumeric(ctl, abuf[i]);
						int val = int.Parse(abuf[i]);
						string msg = ChkMinMax(val, min, max);
						if (msg != null) {
							ThrowErr(ctl, msg);
						}
						ar.Add(val);
					}
					if (ar.Count > cnt) {
						ThrowErr(ctl, string.Format("{0}ヶ以内で入力してください.", cnt));
					}
					ary = (int[])ar.ToArray(typeof(int));
				}
			}
		}
    }
}
