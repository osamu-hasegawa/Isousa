using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uSCOPE
{
	class STMAUTO
	{
		enum STS {
			_00_NUL_STAT = 0,
			_10_INI_XXXX = 10,
			_11_INI_WAIT,
			_20_NAN_IKAK = 20,
		};
#if false
		static public int MACH(int AUT_STS)
		{
			int NXT_STS = this.AUT_STS + 1;
			int yy, y0, ypos;

#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			this.timer2.Enabled = false;
#endif
			if (G.bCANCEL) {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				G.bCANCEL = false;
			}
#if true//2018.11.13(毛髪中心AF)
			if (this.AUT_STS == 16 && this.FCS_STS != 0) {
			}
			else {
				this.AUT_STS = this.AUT_STS;//FOR BP
			}
#endif
#if DEBUG//2019.01.23(GAIN調整&自動測定)
System.Diagnostics.Debug.WriteLine("{0}:STS={1},DIDX={2}", Environment.TickCount, this.AUT_STS, m_didx);
#endif
			switch (this.AUT_STS) {
			case 0:
				this.timer2.Enabled = false;
				break;
			case 1://中上へ
				/*
					0:透過
					1:透過→反射
					2:透過→反射→赤外
					3:透過→赤外
					4:透過→赤外→反射
					5:反射
					6:反射→透過
					7:反射→透過→赤外
					8:反射→赤外
					9:反射→赤外→透過*/
				//---
				if (false) {
				}
				else if ((G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4) && (G.LED_PWR_STS & 1) == 0) {
					//光源=>白色(透過)
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}
				else if ((G.SS.PLM_AUT_MODE >= 5 && G.SS.PLM_AUT_MODE <= 9) && (G.LED_PWR_STS & 2) == 0) {
					//光源=>白色(反射)
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}/*
				if ((G.LED_PWR_STS & 1) == 0 || (G.LED_PWR_STS & 2) != 0) {
					//光源=>白色
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}*/
				else {
#if true//2019.03.18(AF順序)
					set_af_mode(this.AUT_STS);
#else
#if true//2018.05.17
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[0]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[0];
#if true//2019.02.03(WB調整)
						G.CNT_OFS = G.SS.IMP_AUT_SOFS[0];//透過(表面)
#endif
					}
					else {
						//白色(反射)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[1]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[1];
#if true//2019.02.03(WB調整)
						G.CNT_OFS = G.SS.IMP_AUT_SOFS[1];//反射(表面)
#endif
					}
#endif
#endif
#if true//2018.12.22(測定抜け対応)
					if (m_adat.nuke) {
					}
					else
#endif
					if (m_adat.retry == false) {
						DateTime dt = DateTime.Now;
						string buf = "";
						buf = string.Format("{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
										dt.Year,
										dt.Month,
										dt.Day,
										dt.Hour,
										dt.Minute,
										dt.Second);
						if (!string.IsNullOrEmpty(G.SS.PLM_AUT_TITL)) {
							buf = G.SS.PLM_AUT_TITL + "_" + buf;
						}
						m_adat.fold = G.SS.PLM_AUT_FOLD;
						if (G.SS.PLM_AUT_FOLD.Last() != '\\') {
							m_adat.fold += "\\";
						}
						m_adat.fold += buf;
						m_adat.ext = FLTP2STR(G.SS.PLM_AUT_FLTP);
					}
					if (G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4) {
						m_adat.pref = "CT";//白色(透過)
					}
					else {
						m_adat.pref = "CR";//白色(反射)
					}
#if true//2018.12.22(測定抜け対応)
					if (m_adat.nuke) {
					}
					else
#endif
					if (m_adat.retry == false) {
						try {
							System.IO.Directory.CreateDirectory(m_adat.fold);
							G.SS.AUT_BEF_PATH = m_adat.fold;
							m_adat.log = m_adat.fold + "\\log.csv";
							a_write();
						}
						catch (Exception ex) {
							G.mlog(ex.Message);
							G.CAM_PRC = G.CAM_STS.STS_NONE;
							this.AUT_STS = 0;
							break;
						}
					}
				}
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, m_adat.nuke_st[0] + m_adat.cam_hei_pls);
				}
				else {
#endif
#if true//2018.06.04 赤外同時測定
				MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#endif
#if true//2018.12.22(測定抜け対応)
				}
#endif
				//中上
				if (G.SS.PLM_AUT_HPOS) {
#if false//2018.06.04 赤外同時測定
					MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#endif
					if (NXT_STS != 70) {
						m_retry_cnt_of_hpos = 0;
						NXT_STS = -(5 - 1);//->5
						if (G.SS.PLM_AUT_HMOD == 0) {
							this.SPE_COD = 1;
						}
						else {
							this.SPE_COD = 0;
						}
					}
				}
#if false//2018.06.04 赤外同時測定
				else if (G.bJITAN) {
					//for debug
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, 0);
				}
				else {
					//中上
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, G.SS.PLM_MLIM[1]);
				}
#endif
#if true//2018.07.10
				if (G.SS.PLM_AUT_HPOS) {
					//AF位置探索
				}
				else
#endif
#if true//2018.07.02
				if (
#if true//2018.12.22(測定抜け対応)
					true
#else
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/
#endif
					) {
					if (NXT_STS < 0) {
						m_pre_set[2] = true;
						m_pre_pos[2] = G.SS.PLM_AUT_HP_Z;
					}
					else {
						MOVE_ABS_Z(G.SS.PLM_AUT_HP_Z);//FOCUS/Z軸
					}
				}
				else
#endif
				if (G.SS.PLM_AUT_FINI) {
					if (NXT_STS < 0) {
						m_pre_set[2] = true;
						m_pre_pos[2] = G.SS.PLM_POSF[3];
					}
					else {
						MOVE_ABS_Z(G.SS.PLM_POSF[3]);//FOCUS/Z軸
					}
				}
				if (G.SS.PLM_AUT_ZINI) {
					MOVE_ABS(3, G.SS.PLM_POSZ[3]);//ZOOM軸
				}
				if (NXT_STS != 70 && NXT_STS != -4) {
					NXT_STS = -this.AUT_STS;
				}
				break;
			case 2:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				}
				else
#endif
				if (m_adat.retry == false) {
					m_adat.h_idx = 0;//毛髪１本目
					m_adat.h_cnt = 0;
					m_adat.org_pos_x = m_adat.org_pos_y = m_adat.org_pos_z = -0x1000000;
					for (int i = 0; i < m_adat.f_cnt.Length; i++) {
						m_adat.f_cnt[i] = 0;
					}
					m_adat.trace = false;
					m_adat.f_ttl = 0;
					m_adat.f_dum.Clear();
					m_adat.f_nam.Clear();
					m_adat.chk1 = 0;
					m_adat.pos_x.Clear();
					m_adat.pos_y.Clear();
					m_adat.pos_z.Clear();
					//---
					m_adat.z_nam.Clear();
					m_adat.z_pos.Clear();
#if true//2018.06.04 赤外同時測定
					m_adat.y_1st_pos.Clear();
#endif
#if true//2019.01.11(混在対応)
//					m_adat.y_1st_pref.Clear();
#endif
					//---
					if (true) {
						m_adat.z_cnt = 1;
						m_adat.z_idx = 0;
						m_adat.z_nam.Add("ZP00D");
						m_adat.z_pos.Add(0);
#if true//2018.11.13(毛髪中心AF)
						m_adat.k_cnt = 0;
						m_adat.k_idx =-1;
						m_adat.k_nam.Clear();
						m_adat.k_pos.Clear();
#endif
					}
					if (G.SS.PLM_AUT_ZDCK && G.SS.PLM_AUT_ZDEP != null && G.SS.PLM_AUT_ZDEP.Length > 0) {
						for (int i = 0; i < G.SS.PLM_AUT_ZDEP.Length; i++) {
							int pos = G.SS.PLM_AUT_ZDEP[i];
					        m_adat.z_cnt++;
							if (pos >= 0) {
					        m_adat.z_nam.Add(string.Format("ZP{0:00}D", +pos));
							}
							else {
					        m_adat.z_nam.Add(string.Format("ZM{0:00}D", -pos));
							}
					        m_adat.z_pos.Add(pos);
					    }
					}
#if true//2018.11.13(毛髪中心AF)
					if (G.SS.PLM_AUT_ZKCK) {
						m_adat.k_cnt = 1;
						m_adat.k_nam.Add("KP00D");
						m_adat.k_pos.Add(0);
					}
#endif
					if (G.SS.PLM_AUT_ZKCK && G.SS.PLM_AUT_ZKEI != null && G.SS.PLM_AUT_ZKEI.Length > 0) {
						for (int i = 0; i < G.SS.PLM_AUT_ZKEI.Length; i++) {
							int pos = G.SS.PLM_AUT_ZKEI[i];
#if true//2018.11.13(毛髪中心AF)
					        m_adat.k_cnt++;
							if (pos >= 0) {
					        m_adat.k_nam.Add(string.Format("KP{0:00}D", +pos));
							}
							else {
					        m_adat.k_nam.Add(string.Format("KM{0:00}D", -pos));
							}
					        m_adat.k_pos.Add(pos);
#else
					        m_adat.z_cnt++;
							if (pos >= 0) {
					        m_adat.z_nam.Add(string.Format("ZP{0:00}K", +pos));
							}
							else {
					        m_adat.z_nam.Add(string.Format("ZM{0:00}K", -pos));
							}
					        m_adat.z_pos.Add(pos);
#endif
					    }
					}
				}
				NXT_STS = 12;
				break;
			case 5:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke && G.PLM_POS[1] >= (m_adat.nuke_ed[m_adat.nuke_id]-m_adat.cam_hei_pls)) {
					m_adat.nuke_id++;
					if (m_adat.nuke_id >= m_adat.nuke_cnt) {
						NXT_STS = 999;//->終了
					}
					else {
						int nxt_ypos = m_adat.nuke_st[m_adat.nuke_id] + m_adat.cam_hei_pls;
						MOVE_REL_XY(0,nxt_ypos-G.PLM_POS[1]);
						NXT_STS = -(5 - 1);//->5
					}
				}
				else
#endif
#if true//2018.07.30(終了位置指定)
				if (
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					NXT_STS = 999;
				}
				else
#endif
				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
#if true
					if ((NXT_STS = retry_check(NXT_STS)) == 1) {
						break;
					}
#endif
					NXT_STS = 999;
				}
#if false//2019.01.11(混在対応)
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(5 - 1);//->5
				}
#endif
				else {
a_write("AF:開始");
					start_af(3);
				}
			break;
			case 6:
				//AF処理(終了待ち)
				if (this.FCS_STS != 0
#if true//2019.03.02(直線近似)
				 || this.FC2_STS != 0
#endif
					) {
					NXT_STS = this.AUT_STS;
					//m_adat.chk2 = 1;
					//G.mlog("m_adat.chk2参照箇所のチェック");
				}
				else {
a_write("AF:終了");
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					if (G.IR.CIR_CNT <= 0) {
a_write("毛髪判定(AF位置探索):NG");
						m_retry_cnt_of_hpos++;
						if (m_retry_cnt_of_hpos > G.SS.PLM_AUT_HPRT) {
#if true
							if ((NXT_STS = retry_check(NXT_STS)) == 1) {
								break;
							}
#endif
							NXT_STS = 999;
						}
						else {
							MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));
							NXT_STS = -(5 - 1);//->5
a_write("移動:下へ");
						}
					}
					else {
a_write("毛髪判定(AF位置探索):OK");
						this.SPE_COD = 0;
						NXT_STS = 2;//OK
					}
				}
			break;
			case 10:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
					//画面サイズ分↓へ
					MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));
					NXT_STS = -(5 - 1);//->5
					m_adat.h_cnt = m_adat.h_idx;
					break;
				}
#endif
				//a_write("毛髪探索中:LIMIT.CHECK");
#if true//2018.07.30(終了位置指定)
				if (
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					//NXT_STS = 999;
					NXT_STS = 40;
				}
				else
#endif

				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
					NXT_STS = 40;
				}
#if false//2019.01.11(混在対応)
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(10 - 1);//->10
				}
#endif
				else if ((G.PLM_POS[1]+(G.FORM02.PX2UM(G.CAM_HEI)/ G.SS.PLM_UMPP[1])) >=  G.SS.PLM_PLIM[1]) {
					if (m_adat.sts_bak == 14) {
						MOVE_REL_XY(0, (G.SS.PLM_PLIM[1] - G.PLM_POS[1]+10));
						NXT_STS = -(12 - 1);//->12
					}
					else {
						NXT_STS = 40;
					}
				}
				if (NXT_STS == 40) {
					m_adat.h_cnt = m_adat.h_idx;
#if true
					if ((NXT_STS = retry_check(NXT_STS)) == 1) {
						//反射の未検出域に対して透過にてリトライする
						break;
					}
#endif
#if true//2018.06.04 赤外同時測定
					if (G.SS.PLM_AUT_IRCK) {
						NXT_STS = 998;//開始位置へ移動後に終了
					}
					else
#endif
					//m_adat.trace = true;
					if (m_adat.f_ttl <= 0 || (G.SS.PLM_AUT_MODE == 0 || G.SS.PLM_AUT_MODE == 5)) {
						NXT_STS = 998;//開始位置へ移動後に終了
					}
					else if (G.SS.PLM_AUT_MODE == 1 || G.SS.PLM_AUT_MODE == 2) {
						NXT_STS = 120;//->反射
						m_adat.trace = true;
					}
					else if (G.SS.PLM_AUT_MODE == 6 || G.SS.PLM_AUT_MODE == 7) {
						NXT_STS = 100;//->透過
						m_adat.trace = true;
					}
					else if (G.SS.PLM_AUT_MODE == 3 || G.SS.PLM_AUT_MODE == 4|| G.SS.PLM_AUT_MODE == 8 || G.SS.PLM_AUT_MODE == 9) {
						NXT_STS = 140;//->赤外
						m_adat.trace = true;
					}
				}
				break;
			case 11:
a_write("移動:下へ");
				//画面サイズ分↓へ
				MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));

				NXT_STS = -this.AUT_STS;
				break;
			case 12:
			case 22:
			case 32:
			case 112:
			case 132:
			case 152:
#if true//2018.07.30(終了位置指定)
				if (this.AUT_STS == 12 &&
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					NXT_STS = 10;
					break;
				}
#endif
				m_dcur = m_didx;
				break;
			case 13:
			case 23:
			case 33:
			case 113:
			case 133:
			case 153:
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 14:
				//測定
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG
a_write("毛髪判定(中心):NG");
					NXT_STS = 10;
				}
				else {
a_write("毛髪判定(中心):OK");
					NXT_STS = NXT_STS;
				}
#if DEBUG//2018.12.22(測定抜け対応)
				// -849,-555,-263, +31,+274,+568,+862,(7本OFFLINE画像,XYリミットを共に±1000に設定)
				if (NXT_STS == 15 && !m_adat.nuke) {
					if (false
					 ||Math.Abs(G.PLM_POS[1]-(-272)) < 20
					 ||Math.Abs(G.PLM_POS[1]-(+ 33)) < 20 
					 ||Math.Abs(G.PLM_POS[1]-(+862)) < 20 
					) {
						DialogResult ret;
						this.timer2.Enabled = false;
						ret = G.mlog("#q[デバッグ用]\r毛髪抜けデバッグのため当該毛髪をスキップさせますか?");
						if (ret == DialogResult.Yes) {
						NXT_STS = 10;//抜けデバッグのため毛髪をスキップさせる
						}
#if false//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
						this.timer2.Enabled = true;
#endif
					}
				}
#endif
				break;
			case 15:
			case 25:
			case 35:
				//毛髪エリアの垂直方向センタリング
				bool flag = true;
				yy = G.IR.CIR_RT.Top + G.IR.CIR_RT.Height/2;
				y0 = G.CAM_HEI/2;
				if (m_adat.chk1 != 0) {
					//OK(左/右移動後毛髪判定にNGのため最後の画像)
				}
				else if (Math.Abs(yy-y0) < (G.CAM_HEI/5)) {
					//OK
					double	TR = 0.03 * G.IR.CIR_RT.Height;
					bool bHitT = (G.IR.CIR_RT.Top  - 0) < TR;
					bool bHitB = (G.CAM_HEI - G.IR.CIR_RT.Bottom) <= TR;
					if (bHitT && bHitB) {
						//画像の上端と下端の両方に接している => 毛髪が縦方向?
					}
					else if (bHitT || bHitB) {
						flag = false;
					}
				}
				else if ((yy - y0) > 0 && (G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					yy = yy;//SOFT.LIMIT(+)
				}
				else if ((yy - y0) < 0 && (G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					yy = yy;//SOFT.LIMIT(-)
				}
				else {
					flag = false;
				}
				if (!flag) {
					int dif = (yy - y0);
					int dst = (int)(G.PLM_POS[1] + (G.FORM02.PX2UM(dif) / G.SS.PLM_UMPP[1]));

					if (dif < 0 && dst <=  G.SS.PLM_MLIM[1]) {
						dif = dif;
					}
					else if (dif > 0 && dst >= G.SS.PLM_PLIM[1]) {
						dif = dif;
					}
					else {
						a_write("センタリング");
						MOVE_PIX_XY(0, dif);
						NXT_STS = -(this.AUT_STS - 3 - 1);
					}
				}
				else {
					flag = flag;
				}
#if true//2019.03.18(AF順序)
				set_af_mode(this.AUT_STS);
#else
#if true//2018.11.13(毛髪中心AF)
				if ((G.LED_PWR_STS & 1) != 0) {
					//白色(透過)
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[0]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[0];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_SOFS[0];//透過(表面)
#endif
				}
				else {
					//白色(反射)
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[1]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[1];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_SOFS[1];//反射(表面)
#endif
				}
#endif
#endif
				if (this.AUT_STS == 15 && NXT_STS == 16) {
					for (int i = 0; i < 2; i++) {
						Console.Beep(1600, 250);
						Thread.Sleep(250);
					}
a_write("AF:開始");
					start_af(1/*1:1st*/);
#if true//2019.01.23(GAIN調整&自動測定)
					m_adat.gai_tune_cl_done = false;
					m_adat.gai_tune_ir_done = false;
#endif
				}
				else if (NXT_STS == (this.AUT_STS + 1)) {
					if (m_adat.chk1 != 0) {
						NXT_STS++;//AF処理をSKIP
					}
					else if (false
					 || (G.SS.PLM_AUT_FCMD == 1)
					 || (G.SS.PLM_AUT_FCMD == 2 && G.IR.CONTRAST <= (m_adat.sta_contrast * (1 - G.SS.PLM_AUT_CTDR / 100.0)))) {
a_write("AF:開始");
						start_af(2/*2:next*/);
					}
					else {
						NXT_STS++;//AF処理をSKIP
					}
				}
				break;
			case 16:
			case 26:
			case 36:
#if true//2018.11.13(毛髪中心AF)
			case 616:
			case 626:
			case 636:
#endif
				//AF処理(終了待ち)
				if (this.FCS_STS != 0
#if true//2019.03.02(直線近似)
				 || this.FC2_STS != 0
#endif
					) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 1;
				}
				else if (m_adat.chk2 == 1) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 0;
#if false//2019.03.18(AF順序)
					if (m_adat.chk3 == 1) {
						m_adat.chk3 = 0;
						G.FORM02.set_size_mode(1, -1, -1);
					}
#endif
					m_dcur = m_didx;
#if true//2018.11.13(毛髪中心AF)
					if (this.AUT_STS > 600) {
a_write("AF:終了(中心)");
					} else {
#endif
a_write("AF:終了");
#if true//2018.11.13(毛髪中心AF)
					}
#endif
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
				}
				else if ((m_didx - m_dcur) < (G.SS.PLM_AUT_SKIP+3)) {
					NXT_STS = this.AUT_STS;
				}
				else {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					//m_adat.chk1 = Environment.TickCount;
					//m_adat.z_pls[m_adat.h_idx] = G.PLM_POS[2];
				}
				break;
			case 17://初回AF後
			case 27://左側探索
			case 37://右側探索
#if true//2018.06.04 赤外同時測定
				if (G.SS.PLM_AUT_IRCK && m_adat.ir_done) {
					//赤外同時測定の赤外測定後
				}
				else {
#endif
				if (m_adat.z_idx == 0) {
#if true//2019.01.23(GAIN調整&自動測定)
					if (this.AUT_STS == 17 && G.SS.PLM_AUT_V_PK && m_adat.gai_tune_cl_done == false) {
						NXT_STS = 700;//GAIN調整
						break;
					}
#endif
					if (this.AUT_STS == 17) {
						//if ((Environment.TickCount - m_adat.chk1) < 2000) {
						//    //フォーカス軸移動直後のため少し待機
						//    NXT_STS = this.AUT_STS;
						//    break;
						//}
						m_adat.sta_contrast = m_contrast;
						m_adat.sta_pos_x = G.PLM_POS[0];
						m_adat.sta_pos_y = G.PLM_POS[1];
						m_adat.sta_pos_z = G.PLM_POS[2];
						if (m_adat.org_pos_x == -0x1000000) {
							m_adat.org_pos_x = m_adat.sta_pos_x;
							m_adat.org_pos_y = m_adat.sta_pos_y;
							m_adat.org_pos_z = m_adat.sta_pos_z;
						}
						m_adat.f_idx = 50;
						//---
#if true//2018.12.22(測定抜け対応)
						if (m_adat.nuke) {
							m_adat.nuke_pos.Add(G.PLM_POS[1]);
#if true//2019.01.11(混在対応)
							m_adat.nuke_pref.Add(m_adat.pref);
#endif
						}
						else
#endif
						if (m_adat.retry == false) {
							//反射での毛髪Ｙ位置を保存して、
							//透過のときはこのＹ座標をスキップするようにする
							m_adat.y_1st_pos.Add(G.PLM_POS[1]);
#if true//2019.01.11(混在対応)
							m_adat.y_1st_pref.Add(m_adat.pref);
#endif
						}
						//--- ONCE
						if (G.SS.PLM_AUT_CNST) {
							if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
	#if true//2018.06.04 赤外同時測定
								set_expo_const();
	#else
								set_expo_mode(/*const*/0);
	#endif
							}
						}
					}
					if (true) {
						m_adat.pos_x.Add(G.PLM_POS[0]);
						m_adat.pos_y.Add(G.PLM_POS[1]);
						m_adat.pos_z.Add(G.PLM_POS[2]);
					}
					//
					System.IO.Directory.CreateDirectory(m_adat.fold);
					//
					m_adat.z_cur = G.PLM_POS[2];
				}
				if (true) {
					string path0, path1, path2, path3;
					path0 = get_aut_path(-1);
					path1 = path0.Replace("@@", m_adat.f_idx.ToString());
					//path1 = get_aut_path(m_adat.f_idx);
					path2 = m_adat.fold + "\\" + path1;
					G.FORM02.save_image(path2);
					if (m_adat.z_idx == 0) {
						m_adat.f_dum.Add(path2);
						path3 = m_adat.fold + "\\" + path0;
						m_adat.f_nam.Add(path3);
					}
					a_write(string.Format("画像保存:{0}", path1));
				}
				//画像保存
				Console.Beep(800, 250);
#if true//2018.06.04 赤外同時測定
				}
				if (G.SS.PLM_AUT_IRCK) {
					if (m_adat.ir_done == false) {
						m_adat.ir_nxst = this.AUT_STS;
						m_adat.ir_lsbk = G.LED_PWR_STS;
						m_adat.ir_chk1 = m_adat.chk1;
						NXT_STS = 440;//赤外に切替
						break;
					}
					else {
						//毛髪判定ステータスを元に戻す
						m_adat.chk1 = m_adat.ir_chk1;
					}
				}
#endif
#if true//2018.11.13(毛髪中心AF)
				//@if (m_adat.k_cnt > 0 && m_adat.k_done) {
				//@}
				//@else
#endif
				if (m_adat.z_cnt > 1) {
					if (++m_adat.z_idx >= m_adat.z_cnt) {
						m_adat.z_idx = 0;
#if false//true//2018.11.13(毛髪中心AF)
						//@if (m_adat.k_cnt <= 0) {
//@#endif
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
						NXT_STS = -this.AUT_STS;
//@#if true//2018.11.13(毛髪中心AF)
						//@}
#endif
					}
					else {
						NXT_STS = (200+this.AUT_STS);
						break;
					}
				}
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_cnt > 0 && m_adat.k_done == false) {
					m_adat.k_idx = 0;
					if (this.AUT_STS == 17/*初回AF後*/) {
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
					}
					else {
						MOVE_ABS_Z(m_adat.k_pre_pos_z);//Z軸を前位置のAF(中心)位置に戻す
					}
					NXT_STS =-(600-2-1+this.AUT_STS);//移動完了後に615,625,635へ
					break;
				}
				MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
				NXT_STS = -this.AUT_STS;
#endif
				//---
				m_adat.f_cnt[m_adat.h_idx]++;
				m_adat.f_ttl++;
				break;
			case 18:
				m_adat.f_idx--;
				m_adat.chk1 = 0;
				NXT_STS = 20;
				break;
			case 19:
				break;
			case 20:
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 29;
				}
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 29;//こっちを通る
				}
				break;
			case 21:
				//画面サイズ分←へ
				//MOVE_PIX_XY((int)(+G.CAM_WID * 0.9), 0);
				MOVE_PIX_XY((int)(+G.CAM_WID * (1 - G.SS.PLM_AUT_OVLP / 100.0)), 0);

				NXT_STS = -this.AUT_STS;
				a_write("移動:左へ");
				break;
			case 24:
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG
					m_adat.chk1 = 1;
					a_write("毛髪判定(左側):NG");
				}
				else {
					a_write("毛髪判定(左側):OK");
				}
				break;
			//case 26:
			//    break;
			case 28:
				m_adat.f_idx--;
				if (m_adat.chk1 != 0) {
					m_adat.chk1 = 0;
					NXT_STS = 29;
				}
				else {
					NXT_STS = 20;
				}
				break;
			case 29:
				if (true) {
					//毛髪左側の位置順序の入れ替え
					int cnt = m_adat.f_cnt[m_adat.h_idx];
					m_adat.pos_x.Reverse(m_adat.pos_x.Count - cnt, cnt);
					m_adat.pos_y.Reverse(m_adat.pos_y.Count - cnt, cnt);
					m_adat.pos_z.Reverse(m_adat.pos_z.Count - cnt, cnt);
					m_adat.f_dum.Reverse(m_adat.f_dum.Count - cnt, cnt);
				}
				//開始位置へ移動後,右側処理
				MOVE_ABS_XY(m_adat.sta_pos_x, m_adat.sta_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
				m_pre_set[2] = true;
				m_pre_pos[2] = m_adat.sta_pos_z;
#else
				MOVE_ABS_Z(m_adat.sta_pos_z);
#endif
				m_adat.f_idx = 51;
				m_adat.chk1 = 0;
				NXT_STS = -(30 - 1);//->30
				break;
			case 30:
#if true//2018.08.16(右側カット)
				if (G.SS.PLM_AUT_ZNOR) {
					NXT_STS = 39;
				}
#endif

				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
					NXT_STS = 39;
				}
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 39;
				}
				break;
			case 31:
				//画面サイズ分→へ
				//MOVE_PIX_XY((int)(-G.CAM_WID * 0.9), 0);
				MOVE_PIX_XY((int)(-G.CAM_WID * (1-G.SS.PLM_AUT_OVLP/100.0)), 0);
				NXT_STS = -this.AUT_STS;
				a_write("移動:右へ");
				break;
			case 34:
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG時
					m_adat.chk1 = 1;
					a_write("毛髪判定(右側):NG");
				}
				else {
					a_write("毛髪判定(右側):OK");
				}
				break;
			case 38:
				m_adat.f_idx++;
				if (m_adat.chk1 != 0) {
					m_adat.chk1 = 0;
					NXT_STS = 39;
				}
				else {
					NXT_STS = 30;
				}
				break;
			case 39:
				//開始位置へ移動後,次の毛髪処理
				MOVE_ABS_XY(m_adat.sta_pos_x, m_adat.sta_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
				m_pre_set[2] = true;
				m_pre_pos[2] = m_adat.sta_pos_z;
#else
				MOVE_ABS_Z(m_adat.sta_pos_z);
#endif
				NXT_STS = -(10 - 1);//->10
				//---
				rename_aut_files();
				//---
				m_adat.h_idx++;
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				m_adat.n_idx++;
				}
#endif
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					pop_gain_ofs(false);
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[0] + G.SS.CAM_PAR_GA_OF[0]);
					}
					else {
						//白色(反射)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[1] + G.SS.CAM_PAR_GA_OF[1]);
					}
				}
#endif
#if true//2018.08.16(Z軸再原点)
				if (G.SS.PLM_AUT_ZORG) {
					m_pre_set[2] = false;
					NXT_STS = -(500-1);	//500を経由して10へ遷移
				}
#endif
#if true//2019.02.14(Z軸初期位置戻し)
				else if (G.SS.PLM_AUT_ZRET) {//Z軸初期位置戻し
					m_pre_set[2] = true;
					m_pre_pos[2] = G.SS.PLM_POSF[3];
				}
#endif
				break;
			case 100:
			case 400://赤外同時測定
				//光源切り替え(->透過)
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(0, true );//透過
				m_adat.pref = "CT";//白色(透過)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->透過");
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 400) {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					break;
				}
#endif
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 120:
			case 420://赤外同時測定
				//光源切り替え(->反射)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(1, true );//反射
				m_adat.pref = "CR";//白色(反射)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->反射");
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 420) {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					break;
				}
#endif
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 140:
			case 440://赤外同時測定
				//光源切り替え(->赤外)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, true );//赤外
				m_adat.pref = "IR";//赤外
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->赤外");
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 71:
			case 101://透過:トレース
			case 121://反射:トレース
			case 141://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 401://赤外同時測定
			case 421://赤外同時測定
			case 441://赤外同時測定
#endif
//■■■■■■■■■■if (this.AUT_STS == 71 || G.SS.PLM_AUT_EXAT == 1) {
//■■■■■■■■■■		set_expo_mode(/*auto*/1);
//■■■■■■■■■■}
			break;
			case 72:
			case 102://透過:トレース
			case 122://反射:トレース
			case 142://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 402://赤外同時測定
			case 422://赤外同時測定
			case 442://赤外同時測定
#endif
				//カメラ安定待機
				if ((Environment.TickCount - m_adat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				else if (G.SS.PLM_AUT_CNST && this.AUT_STS != 72) {
					if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
#if true//2018.06.04 赤外同時測定
							set_expo_const();
#else
							set_expo_mode(/*const*/0);
#endif
					}
				}
#if true//2019.01.23(GAIN調整&自動測定)
				if (NXT_STS == 443 && G.SS.PLM_AUT_V_PK && m_adat.gai_tune_ir_done == false) {
					NXT_STS = 700;
				}
				else
#endif
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 402 || this.AUT_STS == 422) {
					NXT_STS = m_adat.ir_nxst;
					if (NXT_STS != 17 && NXT_STS != 27 && NXT_STS != 37) {
						NXT_STS = NXT_STS;
					}
				}
#endif
				break;
			case 73:
				NXT_STS = 1;
				break;
			case 103://透過:トレース
			case 123://反射:トレース
			case 143://赤外:トレース
				//m_adat.h_cnt = m_adat.h_idx;
				m_adat.h_idx = 0;
				m_adat.r_idx = 0;
				m_adat.f_idx = 0;
				//MOVE_ABS(2, m_adat.z_pls[0]);
				//NXT_STS = -this.AUT_STS;
				break;
			case 104://透過:トレース
				NXT_STS = 110;
				break;
			case 124://反射:トレース
				NXT_STS = 130;
				break;
			case 144://赤外:トレース
				NXT_STS = 150;
				break;
			case 110://透過:トレース
			case 130://反射:トレース
			case 150://赤外:トレース
				//位置トレース
				if (true) {
					int i = m_adat.r_idx++;
					int x = (int)m_adat.pos_x[i];
					int y = (int)m_adat.pos_y[i];
					int z = (int)m_adat.pos_z[i];
					MOVE_ABS_XY(x, y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
					m_pre_set[2] = true;
					m_pre_pos[2] = z;
#else
					MOVE_ABS_Z(z);
#endif
				}
				NXT_STS = -this.AUT_STS;
a_write("次へ移動");
				break;
			case 111://透過:トレース
			case 131://反射:トレース
			case 151://赤外:トレース
				break;
			case 114://透過:トレース
			case 134://反射:トレース
			case 154://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 443://赤外同時測定
#endif
				if (true) {
					string path0, path1;
					path0 = get_aut_path(m_adat.f_idx);
					path1 = m_adat.fold + "\\" + path0;
//System.Diagnostics.Debug.WriteLine("path0:" + path0);
//System.Diagnostics.Debug.WriteLine("path1:" + path1);
					G.FORM02.save_image(path1);
a_write(string.Format("画像保存:{0}", path0));
				}
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 443) {
					m_adat.ir_done = true;
					if ((m_adat.ir_lsbk & 1)!=0) {
						NXT_STS = 400;//透過に戻す
					}
					else {
						NXT_STS = 420;//反射に戻す
					}
					break;
				}
#endif
				if (m_adat.z_idx == 0) {
					m_adat.z_cur = G.PLM_POS[2];
				}
				if (m_adat.z_cnt > 1) {
					if (++m_adat.z_idx >= m_adat.z_cnt) {
						m_adat.z_idx = 0;
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
						NXT_STS = -this.AUT_STS;
					}
					else {
						NXT_STS = 200+this.AUT_STS;
						break;
					}
				}
				Console.Beep(800, 250);
				break;
			case 115://透過:トレース
			case 135://反射:トレース
			case 155://赤外:トレース
				if (true) {
					int cnt = m_adat.f_cnt[m_adat.h_idx];
					if ((m_adat.f_idx+1) < cnt) {
						//次の画像へ
						m_adat.f_idx++;
						NXT_STS = (this.AUT_STS/10)*10;//->110,130,150
					}
					else {
						//次の毛髪へ
						if (m_adat.f_cnt[m_adat.h_idx+1] <= 0) {//最後の毛髪？
							//次のLEDでトレースを継続
						}
						else {
							m_adat.h_idx++;
							m_adat.f_idx = 0;
							//MOVE_ABS(2, m_adat.z_pls[m_adat.h_idx]);
							NXT_STS = (this.AUT_STS/10)*10;//->110,130,150
						}
					}
				}
				break;
			case 116://透過:トレース
			case 136://反射:トレース
			case 156://赤外:トレース
			case 998:
				//開始位置へ移動
				NXT_STS = -this.AUT_STS;
				if (m_adat.org_pos_x != -0x1000000) {
					//最初の1本目探索位置へ
					MOVE_ABS_XY(m_adat.org_pos_x, m_adat.org_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
					m_pre_set[2] = true;
					m_pre_pos[2] = m_adat.org_pos_z;
#else
					MOVE_ABS_Z(m_adat.org_pos_z);
#endif
				}
				else {
#if true//2018.06.04 赤外同時測定
					MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#else
					//中上
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, G.SS.PLM_MLIM[1]);
#endif
				}
a_write("開始位置へ移動");
				break;
			case 117://透過:トレース
				if (true) {
					G.FORM10.LED_SET(0, false);//透過OFF
				}
				if (G.SS.PLM_AUT_MODE == 6 || G.SS.PLM_AUT_MODE == 9) {
					//6:反射→透過
					//9:反射→赤外→透過
					G.FORM10.LED_SET(1, true );//反射に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 7) {
					//7:反射→透過→赤外
					NXT_STS = 140;//赤外に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
				m_adat.chk1 = Environment.TickCount;
			break;
			case 137://反射:トレース
				if (true) {
					G.FORM10.LED_SET(1, false);//反射OFF
				}
				if (G.SS.PLM_AUT_MODE == 1 || G.SS.PLM_AUT_MODE == 4) {
					//1:透過→反射
					//4:透過→赤外→反射
					G.FORM10.LED_SET(0, true );//透過に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 2) {
					//2:透過→反射→赤外
					NXT_STS = 140;//赤外に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
			break;
			case 157://赤外:トレース
				//光源切り替え
				if (true) {
					G.FORM10.LED_SET(2, false);//赤外OFF
				}
				if (G.SS.PLM_AUT_MODE == 2 || G.SS.PLM_AUT_MODE == 3) {
					//2:透過→反射→赤外
					//3:透過→赤外
					G.FORM10.LED_SET(0, true );//透過に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 7 || G.SS.PLM_AUT_MODE == 8) {
					//7:反射→透過→赤外
					//8:反射→赤外
					G.FORM10.LED_SET(1, true );//反射に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 4) {
					//4:透過→赤外→反射
					NXT_STS = 120;//反射に切り替えて継続
				}
				else if (G.SS.PLM_AUT_MODE == 9) {
					//9:反射→赤外→透過
					NXT_STS = 100;//透過に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
			break;
			case 70:
				//光源切り替え(開始時)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外

				if ((G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4)) {
					G.FORM10.LED_SET(0, true);//透過
a_write("光源切替:->透過");
				}
				else {
					G.FORM10.LED_SET(1, true);//反射
a_write("光源切替:->反射");
				}
				m_adat.chk1 = Environment.TickCount;
				break;
			case 118://透過:トレース
			case 138://反射:トレース
			case 158://赤外:トレース
				break;
			case 119://透過:トレース
			case 139://反射:トレース
			case 159://赤外:トレース
				break;
			case 61:
				NXT_STS = 999;//自動測定:終了
				break;
			case 217:
			case 227:
			case 237:
			case 314:
			case 334:
			case 354:
				//Z軸移動
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_idx >= 0) {
					int zpos = (int)(m_adat.k_pos[m_adat.k_idx]);
					MOVE_ABS_Z(m_adat.k_pre_pos_z + zpos);
					NXT_STS = -this.AUT_STS;
				} else
#endif
				if (true) {
					int zpos = (int)(m_adat.z_pos[m_adat.z_idx]);
					MOVE_ABS_Z(m_adat.z_cur + zpos);
					NXT_STS = -this.AUT_STS;
				}
				break;
			case 218:
			case 228:
			case 238:
			case 315:
			case 335:
			case 355:
				m_dcur = m_didx;
				break;
			case 219:
			case 229:
			case 239:
			case 316:
			case 336:
			case 356:
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 220:
			case 230:
			case 240:
			case 317:
			case 337:
			case 357:
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_idx >= 0) {
				NXT_STS = -3-200+this.AUT_STS+600;
				break;
				}
#endif
				NXT_STS = -3-200+this.AUT_STS;
				break;
#if true//2018.08.16(Z軸再原点)
			case 500:
				D.SET_STG_ORG(2);
				G.PLM_STS |= (1 << 2);
				NXT_STS = -this.AUT_STS;
			break;
			case 501:
#if true//2019.02.14(Z軸初期位置戻し)
				if (G.SS.PLM_AUT_ZRET) {//Z軸初期位置戻し
					MOVE_ABS_Z(G.SS.PLM_POSF[3]);//FOCUS/Z軸
					NXT_STS = -(10 - 1);//->10
					break;
				}
#endif
				MOVE_ABS_Z(m_adat.sta_pos_z);
				NXT_STS = -(10 - 1);//->10
			break;
#endif
#if true//2018.11.13(毛髪中心AF)
			case 615:
			case 625:
			case 635:
#if true//2019.03.18(AF順序)
				set_af_mode(this.AUT_STS);
#else
#if true//2018.05.17
				if ((G.LED_PWR_STS & 1) != 0) {
					//白色(透過):中心用
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[2]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[2];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_COFS[0];//透過(中心)
#endif
				}
				else {
					//白色(反射):中心用
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[3]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[3];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_COFS[1];//反射(中心)
#endif
				}
#endif
#endif
				if (this.AUT_STS == 615 && NXT_STS == 616) {
a_write("AF:開始(中心)");
					start_af(1/*1:1st*/);
				}
				else if (NXT_STS == (this.AUT_STS + 1)) {
					if (m_adat.chk1 != 0) {
						NXT_STS++;//AF処理をSKIP
					}
					else if (false
					 || (G.SS.PLM_AUT_FCMD == 1)
					 || (G.SS.PLM_AUT_FCMD == 2 && G.IR.CONTRAST <= (m_adat.k_sta_contrast * (1 - G.SS.PLM_AUT_CTDR / 100.0)))) {
a_write("AF:開始(中心)");
						start_af(2/*2:next*/);
					}
					else {
						NXT_STS++;//AF処理をSKIP
					}
				}
			break;
			case 617://初回AF後
			case 627://左側探索
			case 637://右側探索
#if true//2018.06.04 赤外同時測定
				if (G.SS.PLM_AUT_IRCK && m_adat.ir_done) {
					//赤外同時測定の赤外測定後
				}
				else {
#endif
				if (m_adat.k_idx == 0) {
					if (this.AUT_STS == 617) {
						m_adat.k_sta_contrast = m_contrast;
						//---
						if (G.SS.PLM_AUT_CNST) {
							if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
	#if true//2018.06.04 赤外同時測定
								set_expo_const();
	#else
								set_expo_mode(/*const*/0);
	#endif
							}
						}
					}
					//@if (true) {
					//@    m_adat.pos_x.Add(G.PLM_POS[0]);
					//@    m_adat.pos_y.Add(G.PLM_POS[1]);
					//@    m_adat.pos_z.Add(G.PLM_POS[2]);
					//@}
					//
					//@System.IO.Directory.CreateDirectory(m_adat.fold);
					//
					//@m_adat.z_cur = G.PLM_POS[2];
					m_adat.k_pre_pos_z = G.PLM_POS[2];
				}
				if (true) {
					string path0, path1, path2, path3;
					path0 = get_aut_path(-1);
					path1 = path0.Replace("@@", m_adat.f_idx.ToString());
					//path1 = get_aut_path(m_adat.f_idx);
					path2 = m_adat.fold + "\\" + path1;
					G.FORM02.save_image(path2);
					//@if (m_adat.z_idx == 0) {
					//@    m_adat.f_dum.Add(path2);
					//@    path3 = m_adat.fold + "\\" + path0;
					//@    m_adat.f_nam.Add(path3);
					//@}
					a_write(string.Format("画像保存:{0}", path1));
				}
				//画像保存
				Console.Beep(800, 250);
#if true//2018.06.04 赤外同時測定
				}
				if (G.SS.PLM_AUT_IRCK) {
					if (m_adat.ir_done == false) {
						m_adat.ir_nxst = this.AUT_STS;
						m_adat.ir_lsbk = G.LED_PWR_STS;
						m_adat.ir_chk1 = m_adat.chk1;
						NXT_STS = 440;//赤外に切替
						break;
					}
					else {
						//毛髪判定ステータスを元に戻す
						m_adat.chk1 = m_adat.ir_chk1;
					}
				}
#endif
				if (true/*m_adat.k_cnt > 0*/) {
					if (++m_adat.k_idx >= m_adat.k_cnt) {
						m_adat.k_idx =-1;
					}
					else {
						NXT_STS = 200+this.AUT_STS-600;
						break;
					}
				}
				if (true) {
					MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
					NXT_STS = -(this.AUT_STS-600);
				}
				//---
				m_adat.f_cnt[m_adat.h_idx]++;
				m_adat.f_ttl++;
				break;
#endif
#if true//2019.01.23(GAIN調整&自動測定)
			case 700:
				if (G.LED_PWR_STS == 1) {
					this.timer4.Tag = 0;//透過
				}
				else if (G.LED_PWR_STS == 2) {
					this.timer4.Tag = 1;//反射
				}
				else if (G.LED_PWR_BAK == 1/*反射*/) {
					this.timer4.Tag = 3;//赤外(<-反射)
				}
				else {
					this.timer4.Tag = 2;//赤外(<-透過)
				}
				G.CNT_MOD = 0;//0:画面全体
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_VPK = 1;
				this.GAI_STS = 1;
				this.timer4.Enabled = true;
a_write("GAIN調整:開始");
				break;
			case 701:
				//GAIN調整-終了待ち
				if (this.GAI_STS != 0) {
					NXT_STS = this.AUT_STS;
				}
				else {
a_write(string.Format("GAIN調整:終了(OFFSET={0})", G.SS.CAM_PAR_GA_OF[(int)this.timer4.Tag]));
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					if (m_adat.gai_tune_cl_done == false) {
						m_adat.gai_tune_cl_done = true;
						NXT_STS = 17;//初回AF後
					}
					else {
						m_adat.gai_tune_ir_done = true;
						NXT_STS = 443;//IRの保存へ
					}
				}
				break;
			case 702:
				break;
#endif
			case 999:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
					//抜けチェック測定後
					rename_nuke_files();
					m_adat.nuke = false;
#if false//2019.01.11(混在対応) -> rename内でソートしてコピーするように変更
					for (int i = 0; i < m_adat.nuke_pos.Count; i++) {
						//透過リトライ用にコピーしておく
						m_adat.y_1st_pos.Add(m_adat.nuke_pos[i]);
					}
#endif
				}
				else if (G.SS.PLM_AUT_NUKE/* && G.SS.PLM_AUT_HPOS*/) {
					if (check_nuke()) {
						m_adat.nuke = true;
						NXT_STS = 1;
						break;
					}
				}
#endif
#if true//2019.01.11(混在対応)
				if (G.SS.PLM_AUT_RTRY && (G.SS.PLM_AUT_MODE == 5 || G.SS.PLM_AUT_MODE == 8)) {
					if (check_touka_retry()) {
						m_adat.nuke = true;
						//5:反射
						//8:反射→赤外
						//反射で毛髪検出できないときは透過にてリトライする
						G.SS.PLM_AUT_MODE -= 5;
						//0:透過
						//3:透過→赤外
						NXT_STS = 1;
						break;
					}
				}
#else
				if (m_adat.h_cnt == 0 && G.SS.PLM_AUT_RTRY) {
					if (G.SS.PLM_AUT_MODE == 5 || G.SS.PLM_AUT_MODE == 8) {
						//5:反射
						//8:反射→赤外
						//反射で毛髪検出できないときは透過にてリトライする
						G.SS.PLM_AUT_MODE -= 5;
						//0:透過
						//3:透過→赤外
						NXT_STS = 1;
						break;
					}
				}
#endif
//■■■■■■■set_expo_mode(/*auto*/1);
				a_write(string.Format("終了:毛髪{0}本", m_adat.h_cnt));
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				timer2.Enabled = false;
				UPDSTS();
				for (int i = 0; i < 3; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
				G.mlog(string.Format("#i測定が終了しました.\r毛髪:{0}本", m_adat.h_cnt));
				break;
			default:
				if (!(this.AUT_STS < 0)) {
					G.mlog("kakunin suru koto!!!");
				}
				else {
					//f軸停止待ち
#if true//2018.06.04 赤外同時測定
					m_adat.ir_done = false;
#endif
					if ((G.PLM_STS & (1|2|4)) == 0) {
						if (m_bsla[0] != 0 || m_bsla[1] != 0) {
#if true//2018.05.23(毛髪右端での繰り返し発生対応)
							if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
								NXT_STS = NXT_STS;//リミットステータスが消えてしまうのでバックラッシュ制御はスキップする
							}
							else if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
								NXT_STS = NXT_STS;//リミットステータスが消えてしまうのでバックラッシュ制御はスキップする
							}
							else {
#endif
							MOVE_REL_XY(m_bsla[0], m_bsla[1]);
#if true//2018.05.23(毛髪右端での繰り返し発生対応)
							}
#endif
							m_bsla[0] = m_bsla[1] = 0;
							NXT_STS = this.AUT_STS;
						}
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
						else if (m_pre_set[2]) {
							m_pre_set[2] = false;
							MOVE_ABS_Z(m_pre_pos[2]);
							NXT_STS = this.AUT_STS;
						}
#endif
						else if (m_bsla[2] != 0) {
							Thread.Sleep(1000/G.SS.PLM_LSPD[2]);//2018.05.21
							MOVE_REL_Z(m_bsla[2]);
							m_bsla[2] = 0;
							NXT_STS = this.AUT_STS;
						}
						else {
							NXT_STS = (-this.AUT_STS) + 1;
						}
					}
					else {
						NXT_STS = this.AUT_STS;
					}
				}
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (this.AUT_STS > 0) {
				m_adat.sts_bak = this.AUT_STS;
			}
			if (this.AUT_STS != 0) {
				this.AUT_STS = NXT_STS;
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				this.timer2.Enabled = true;
#endif
			}
			return(AUT_STS);
		}
#endif
	}
}
