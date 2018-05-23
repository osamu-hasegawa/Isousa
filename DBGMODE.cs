using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Threading;
using System.Threading.Tasks;
//using System.Threading.Thread;
using Basler.Pylon;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace uSCOPE
{
	class DBGMODE
	{
		static private
		Thread m_thrd_of_dev = null;
		static private
		Thread m_thrd_of_cam = null;
		static private
		AutoResetEvent m_event_dev = null;
		static private
		AutoResetEvent m_event_cam = null;
		static private
		bool m_exit_req = false;
		static
		private Bitmap m_bmp_org;
		static
		private Bitmap m_bmp_cam;
		public delegate void DLG_VOID_OBJ_IGEA(Object obj, ImageGrabbedEventArgs e);
		public delegate void DLG_VOID_OBJ_GSEA(Object obj, GrabStopEventArgs e);

		static public Form m_fm = null;
		static public DLG_VOID_OBJ_IGEA m_fg = null;
		static public DLG_VOID_OBJ_GSEA m_fs = null;

	    static private int B4(int l) {
		    return ((byte)((l & 0xff000000) >> 24));
	    }
	    static private int B3(int l) {
		    return ((byte)((l & 0x00ff0000) >> 16));
	    }
	    static private int B2(int l) {
		    return ((byte)((l & 0x0000ff00) >> 8));
	    }
	    static private int B1(int l) {
		    return ((byte)((l & 0x000000ff) >> 0));
	    }
		static private int MAKELONG(int b4, int b3, int b2, int b1) {
		    return ((0xff & b4) << 24 | (0xff & b3) << 16 | (0xff & b2) << 8 | (0xff & b1));
	    }
		static public void INIT()
		{
			if (m_thrd_of_dev == null) {
				m_thrd_of_dev = new Thread( new ThreadStart( THRD_OF_DEVICE ) );
			}
			if (m_event_dev == null) {
				m_event_dev = new AutoResetEvent(false);
			}
			//---
			for (int i = 0; i < PLM_AMS.Length; i++) {
				//ms当たり何pps増えるか
				PLM_AMS[i] = (double)(G.SS.PLM_HSPD[i]-G.SS.PLM_LSPD[i]) / G.SS.PLM_ACCL[i];
				//加速・減速に必要なパルス数(台形部分の積分値)
				PLM_ACT[i] = G.SS.PLM_LSPD[i] * G.SS.PLM_ACCL[i];
				PLM_ACT[i]+=(G.SS.PLM_HSPD[i] - G.SS.PLM_LSPD[i]) * G.SS.PLM_ACCL[i]/2;
				PLM_ACT[i]/=1000;
			}
			m_exit_req = false;
			m_thrd_of_dev.Start();
		}
		static public void INIT_OF_CAM()
		{
			//OFFLINE画像(OFFLINE-IMAGE.PNG)の読込
			m_bmp_org = new Bitmap(G.GET_DOC_PATH("OFFLINE-IMAGE.PNG"));
			m_bmp_cam = new Bitmap(2592, 1944);
			//---
			if (m_thrd_of_cam == null) {
				m_thrd_of_cam = new Thread( new ThreadStart( THRD_OF_CAMERA ) );
			}
			if (m_event_cam == null) {
				m_event_cam = new AutoResetEvent(false);
			}
            m_thrd_of_cam.Start();
		}
		static public void TERM()
		{
			m_exit_req = true;
			if (m_event_dev != null)
			{
				m_event_dev.Set();
			}
			if (m_event_cam != null)
			{
				m_event_cam.Set();
			}
			Thread.Sleep(250); 
			
			if (m_thrd_of_dev != null) {
				m_thrd_of_dev.Abort();
				m_thrd_of_dev = null;
			}
			if (m_thrd_of_cam != null)
			{
				m_thrd_of_cam.Abort();
				m_thrd_of_cam = null;
			}
		}
		static public int HID_ENUM(uint vid, uint pid, out int cnt)
		{
			cnt = 1;
			return(1);
		}
		static public int HID_OPEN(uint vid, uint pid, uint did)
		{
			INIT();
			return(1);
		}
		static public int HID_CLOSE()
		{
			TERM();
			return(1);
		}
		static private
		int[]	PLM_CNT = {0,0,0,0};
		static private
		int[]	PLM_STS = {0,0,0,0};
		static private
		int[]	PLM_MOD = {0,0,0,0};//0:停止中,+1/-1:移動中,+2/-2:JOG移動中,+3/-3:ORG中
		static private
		int[]	PLM_PPS = {0,0,0,0};//移動速度PPS
		static private
		int[]	PLM_SPD = {0,0,0,0};//現在速度(初速→移動速度→初速と変化)
		static private
		int[]	PLM_DIR = {0,0,0,0};//移動方向:(+1:CW, -1:CCW)
		static private
		int[]	PLM_RST = {0,0,0,0};//残り発行予定パルス数
		static private
		int[]	PLM_TIC = {0,0,0,0};//開始TIC
		static private
		int[]	PLM_STQ = {0,0,0,0};//停止要求
		static private
		byte[]	RET_BUF = {0,0,0,0,0,0,0,0};
		static private
		double[]PLM_AMS = {0,0,0,0};//加速・減速に必要なms時間
		static private
		int[]	PLM_ACT = {0,0,0,0};//加速・減速に必要なパルス数(台形部分の積分値)

		static byte STS_BIT(int idx)
		{
			if (PLM_MOD[idx] != 0) {
				PLM_STS[idx] |= (int)G.PLM_STS_BITS.BIT_ONMOV;
			}
			else {
				PLM_STS[idx] &=~(int)G.PLM_STS_BITS.BIT_ONMOV;
			}
			if (PLM_CNT[idx] > 0) {
				PLM_STS[idx] |= (int)G.PLM_STS_BITS.BIT_LMT_H;
			}
			else {
				PLM_STS[idx] &=~(int)G.PLM_STS_BITS.BIT_LMT_H;
			}
			if (PLM_CNT[idx] >= G.SS.PLM_PLIM[idx]) {
				PLM_STS[idx] |= (int)G.PLM_STS_BITS.BIT_LMT_P;
			}
			else {
				PLM_STS[idx] &=~(int)G.PLM_STS_BITS.BIT_LMT_P;
			}
			if (PLM_CNT[idx] <= G.SS.PLM_MLIM[idx]) {
				PLM_STS[idx] |= (int)G.PLM_STS_BITS.BIT_LMT_M;
			}
			else {
				PLM_STS[idx] &=~(int)G.PLM_STS_BITS.BIT_LMT_M;
			}
			
			return((byte)PLM_STS[idx]);
		}
		static public void WRITE_HID(byte[] buf)
		{
			int cmd = buf[0];
			int idx = buf[1];

			switch (cmd) {
			case D.CMD_SET_PLM_PRM:
			case D.CMD_SET_PLM_LMT:
			case D.CMD_SET_PWM_DTY:
			case D.CMD_SET_LED_STS:
			case D.CMD_SET_PWM_STS:
			case D.CMD_SET_PLM_TRQ:				//何もしない
			case D.CMD_GET_BTN_STS:
			break;
			case D.CMD_GET_PLM_POS:
				RET_BUF[0] = (byte)B4(PLM_CNT[idx]);//MSB
				RET_BUF[1] = (byte)B3(PLM_CNT[idx]);
				RET_BUF[2] = (byte)B2(PLM_CNT[idx]);
				RET_BUF[3] = (byte)B1(PLM_CNT[idx]);//LSB
				break;
			case D.CMD_SET_PLM_POS:
				PLM_CNT[idx] = MAKELONG(0x00, buf[2], buf[3], buf[4]);
				break;
			case D.CMD_GET_PLM_STS:
				RET_BUF[0] = STS_BIT(0);//MSB
				RET_BUF[1] = STS_BIT(1);
				RET_BUF[2] = STS_BIT(2);
				RET_BUF[3] = STS_BIT(3);//LSB
				break;
			case D.CMD_SET_PLM_ORG:
				PLM_STS[idx] |= (int)G.PLM_STS_BITS.BIT_ORGOK;
				PLM_CNT[idx]  = 0;
				break;
			case D.CMD_SET_PLM_REL:
				if ((buf[2]&0x80)==0/* CW*/ && PLM_CNT[idx] >= G.SS.PLM_PLIM[idx]) {
					break;
				}
				if ((buf[2]&0x80)!=0/*CCW*/ && PLM_CNT[idx] <= G.SS.PLM_MLIM[idx]) {
					break;
				}
				PLM_TIC[idx] = System.Environment.TickCount;
				PLM_DIR[idx] = (buf[2]&0x80)==0 ? +1:-1;
				PLM_SPD[idx] = G.SS.PLM_LSPD[idx];
				PLM_PPS[idx] = G.SS.PLM_HSPD[idx];
				PLM_MOD[idx] = 1;
				if ((buf[2]&0x80)==0) {
				PLM_RST[idx] = MAKELONG(0x00, buf[2], buf[3], buf[4]);
				}
				else {
				PLM_RST[idx] = MAKELONG(0xFF, buf[2], buf[3], buf[4]);
				PLM_RST[idx]*= -1;
				}
				PLM_STQ[idx] = 0;
				m_event_dev.Set();
				break;
			case D.CMD_SET_PLM_JOG:
				if (buf[2]==1/* CW*/ && PLM_CNT[idx] >= G.SS.PLM_PLIM[idx]) {
					break;
				}
				if (buf[2]!=1/*CCW*/ && PLM_CNT[idx] <= G.SS.PLM_MLIM[idx]) {
					break;
				}
				PLM_TIC[idx] = System.Environment.TickCount;
				PLM_DIR[idx] = (buf[2]==1) ? +1:-1;
				PLM_SPD[idx] = G.SS.PLM_LSPD[idx];
				PLM_PPS[idx] = G.SS.PLM_JSPD[idx];
				PLM_MOD[idx] = 1;
				PLM_RST[idx] = 0x7FFFFF;//最大パルス数
				PLM_STQ[idx] = 0;
				m_event_dev.Set();
				break;
			case D.CMD_SET_PLM_STP:
				PLM_STQ[idx] = 1;	//STOP.REQUEST
				m_event_dev.Set();
				break;
			}
		}
		static public void READ_HID(byte[] cmd)
		{
			if (cmd != null) {
				int l = cmd.Length;
				for (int i = 0; i < l && i < RET_BUF.Length; i++) {
					cmd[i] = RET_BUF[i];
					if (i >= 4) {
						i = i;
						//G.mlog("READ_HID:length >= 4!!!");
					}
				}
			}
		}

		static public void REG_CALLBACK(Form fm, DLG_VOID_OBJ_IGEA fg, DLG_VOID_OBJ_GSEA fs)
		{
			DBGMODE.m_fm = fm;
			DBGMODE.m_fg = fg;
			DBGMODE.m_fs = fs;
		}
		static public void ONE_SHOT()
		{
			//IGrabResult	gr;
			//ImageGrabbedEventArgs	e = new ImageGrabbedEventArgs();

			//XYZステージに位置によってカメラ画像を作成する
			//DEBUG用イメージ大画像から
			//XY->切り出し位置
			//Z ->ぼかし度(4500をAF位置としてno blurとする)
			//透過赤外反射のON/OFFを記憶する
            m_event_cam.Set();
		}
		static public Image GET_IMAGE()
		{
			return (m_bmp_cam);
		}
		static private void THRD_OF_DEVICE()
		{
			while (true) {
				if (PLM_MOD[0] == 0 && PLM_MOD[1] == 0 && PLM_MOD[2] == 0 && PLM_MOD[3] == 0) {
					while (m_event_dev.WaitOne(250) == false) {
						Thread.Sleep(0);
						if (m_exit_req) {
							break;
						}
					}
				}
				else {
					if (true) {
						Thread.Sleep(0);
					}
				}
				if (m_exit_req) {
					break;
				}
				//---
				int tic = Environment.TickCount;
				//---
				for (int i = 0; i < 4; i++) {
					//モータ移動中?
					int ela = tic - PLM_TIC[i];
					int cnt, spd;

					if (PLM_MOD[i] == 0) {
						continue;
					}
					if (false) {
					}
					else if (PLM_STQ[i] != 0 && (PLM_MOD[i] == 1 || PLM_MOD[i] == 2)) {
						PLM_MOD[i] = 3;
						PLM_STQ[i] = 1;
					}
					else if (PLM_DIR[i] > 0 && PLM_CNT[i] >= G.SS.PLM_PLIM[i]) {
						PLM_MOD[i] = 3;
						PLM_STQ[i] = 1;
					}
					else if (PLM_DIR[i] < 0 && PLM_CNT[i] <= G.SS.PLM_MLIM[i]) {
						PLM_MOD[i] = 3;
						PLM_STQ[i] = 1;
					}
					if (PLM_MOD[i] == 1) {//加速中
						cnt = (int)(PLM_SPD[i] * ela / 1000);
						PLM_RST[i] -= cnt;
						cnt*= PLM_DIR[i];
						if ((spd = (int)(PLM_SPD[i] +  ela * PLM_AMS[i])) >= PLM_PPS[i]) {
							PLM_MOD[i] = 2;//→定速
							spd = PLM_PPS[i];
						}
						PLM_CNT[i] += cnt;
						PLM_TIC[i] = tic;
						PLM_SPD[i] = spd;
						if (PLM_RST[i] <= PLM_ACT[i]) {
							PLM_MOD[i] = 3;//→減速
						}
					}
					else if (PLM_MOD[i] == 2) {	//定速中
						cnt = (int)(PLM_SPD[i] * ela / 1000);
						PLM_RST[i] -= cnt;
						cnt*= PLM_DIR[i];
						PLM_CNT[i] += cnt;
						PLM_TIC[i] = tic;
						if (PLM_RST[i] <= PLM_ACT[i]) {
							PLM_MOD[i] = 3;//→減速
						}
					}
					else if (PLM_MOD[i] == 3) {	//減速中
						cnt = (int)(PLM_SPD[i] * ela / 1000);
						if (cnt >= PLM_RST[i]) {
							cnt = PLM_RST[i];
						}
						if (cnt < 0) {
							cnt = cnt;
						}
						PLM_RST[i] -= cnt;
						cnt*= PLM_DIR[i];
						spd = (int)(PLM_SPD[i] -  ela * PLM_AMS[i]);
						if (spd < G.SS.PLM_LSPD[i]) {
							spd = G.SS.PLM_LSPD[i];
						}
						PLM_CNT[i] += cnt;
						PLM_TIC[i] = tic;
						PLM_SPD[i] = spd;
						if (false) {
						}
						else if (PLM_RST[i] == 0) {
							PLM_MOD[i] = 0;	//→停止
							PLM_STQ[i] = 0;
						}
						else if (PLM_STQ[i] != 0 && PLM_SPD[i] <= G.SS.PLM_LSPD[i]) {
							PLM_MOD[i] = 0;	//→停止
							PLM_STQ[i] = 0;
						}
					}
				}
			}
		}
		static private void THRD_OF_CAMERA()
		{
			while (true) {
				while (m_event_cam.WaitOne(2500) == false) {
					Thread.Sleep(0);
					if (m_exit_req) {
						break;
					}
				}
				if (m_exit_req) {
					break;
				}
				//
				const
				int CAM_WID = 2592;
				const
				int CAM_HEI = 1944;
				//const
				//int IMAGE_RATE = 4;
#if true
				double LENS_ZOOM = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
#else
				const
				int LENS_ZOOM = 16;
#endif
				//int x = PLM_CNT[0];
				//int y = PLM_CNT[1];
				double wid_p = G.SS.PLM_PLIM[0] - G.SS.PLM_MLIM[0];
				double hei_p = G.SS.PLM_PLIM[1] - G.SS.PLM_MLIM[1];
				double wid_u = wid_p * G.SS.PLM_UMPP[0];
				double hei_u = hei_p * G.SS.PLM_UMPP[1];
				int wid_i = m_bmp_org.Width;
				int hei_i = m_bmp_org.Height;
				double x_per = (G.PLM_POS[0]- G.SS.PLM_MLIM[0]) / wid_p;
				double y_per = (G.PLM_POS[1]- G.SS.PLM_MLIM[1]) / hei_p;
				//double xum = x * G.SS.PLM_UMPP[0];//ステージのum位置
				//double yum = y * G.SS.PLM_UMPP[1];
				//double xpx = G.UM2PX(xum, G.SS.CAM_SPE_UMPPX, LENS_ZOOM/4);
				//double ypx = G.UM2PX(yum, G.SS.CAM_SPE_UMPPX, LENS_ZOOM/4);
				int ox = (int)(wid_i - wid_i * x_per);
				int oy = (int)(hei_i * y_per);
				int wid = (int)(((CAM_WID *G.SS.CAM_SPE_UMPPX)/wid_u * wid_i)/LENS_ZOOM);
				int	hei = (int)(((CAM_HEI *G.SS.CAM_SPE_UMPPX)/hei_u * hei_i)/LENS_ZOOM);
				//
				Graphics g = Graphics.FromImage(m_bmp_cam);
				Rectangle d_rt = new Rectangle(0,0,CAM_WID,CAM_HEI);
//				Rectangle s_rt = new Rectangle(ox-wid/2, oy-hei/2, wid, hei);
				if ((G.PLM_POS[0]/G.SS.PLM_MLIM[0]) >= 0.95) {
				//-リミットでの繰り返し発生実験
				ox = (int)(wid_i - wid_i * 0.5);
				}
				Rectangle s_rt = new Rectangle(ox-wid/2, oy-hei/2, wid, hei);
				g.Clear(Color.Black/*.Aquamarine*/);
				g.DrawImage(m_bmp_org, d_rt, s_rt, GraphicsUnit.Pixel);
				g.Dispose();
				Thread.Sleep(0);
				//画像取得
				try {
					m_fm.BeginInvoke(m_fg, new object[] { null, null });
					if (m_event_cam.WaitOne(750)) {
						if (m_exit_req) {
							break;
						}
					}
					Thread.Sleep(0);
					m_fm.BeginInvoke(m_fs, new object[] { null, null });
					//if (m_event_cam.WaitOne(250))
					//{
					//    if (m_exit_req)
					//    {
					//        break;
					//    }
					//}
				}
				catch (Exception ex) {
					break;
				}
			}
		}
	}
}
