using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace uSCOPE
{
    class D
    {
#if true
		[DllImport("USBHIDHELPER32.DLL", EntryPoint = "HID_ENUM")]
		static extern int _HID_ENUM(uint vid, uint pid, IntPtr pcnt);

		[DllImport("USBHIDHELPER32.DLL")]
		static extern int HID_OPEN(uint vid, uint pid, uint did);
    
		[DllImport("USBHIDHELPER32.DLL")]
		static extern int HID_CLOSE();
    
		[DllImport("USBHIDHELPER32.DLL")]
		static extern int WRITE_HID(byte[] pbuf, int size);
    
		[DllImport("USBHIDHELPER32.DLL")]
		static extern int READ_HID(byte[] pbuf, int size);
		/*
		BOOL APIENTRY HID_ENUM(DWORD vid, DWORD pid, LPDWORD pcnt);
		BOOL APIENTRY HID_OPEN(DWORD vid, DWORD pid, DWORD did);
		BOOL APIENTRY HID_CLOSE(void);
		BOOL APIENTRY WRITE_HID(LPBYTE pbuf, DWORD size);
		BOOL APIENTRY READ_HID(LPBYTE pbuf, DWORD size);
		*/
#endif
	    /**/
	    public const int CMD_GET_ECHOBAC				= 0x01;
	    public const int CMD_GET_BTN_STS			 	= 0x10;
	    public const int CMD_SET_LED_STS			 	= 0x11;
		//public const int CMD_SET_HUB_PWR			 	= 0x12;
	    public const int CMD_SET_PWM_STS				= 0x20;
	    public const int CMD_SET_PWM_DTY				= 0x13;
		//public const int CMD_SET_PWM_FRQ				= 0x14;
		//public const int CMD_GET_AFC_ADS				= 0x15;	// AD0, AD1
		//public const int CMD_SET_AFC_C_A				= 0x16;	// AD0 * A + AD1 * B + C => 0-1023?  
		//public const int CMD_SET_AFC_C_B				= 0x17;
		//public const int CMD_SET_AFC_C_C				= 0x18;
		//public const int CMD_SET_AFC_C_I				= 0x19;	// AT.FOCUS UPDATE INTERVAL (ms)
		//public const int CMD_SET_AFC_C_T				= 0x1A;	// TORELANCE(%) == AT.END.CONDITION
		//public const int CMD_SET_AFC_ENB				= 0x1B;	// ENABLE/DISABLE
		//public const int CMD_SET_AFC_POS				= 0x21;	// ENABLE/DISABLE
		//public const int CMD_SET_VCM_POS			 	= 0x1C;
	    public const int CMD_SET_PLM_PRM			 	= 0x1D;	// PULSE.MOTOR.PARAM
	    public const int CMD_GET_PLM_STS			 	= 0x1F;	// PULSE.MOTOR.PARAM,COUNTER,LIMIT
	    public const int CMD_GET_PLM_POS			 	= 0x22;	// PULSE.MOTOR.PARAM,COUNTER,LIMIT
	    public const int CMD_SET_PLM_REL				= 0x23;	// PULSE.MOTOR.REL.MOV
	    public const int CMD_SET_PLM_ABS				= 0x24;	// PULSE.MOTOR.ABS.MOV
	    public const int CMD_SET_PLM_STP				= 0x25;	// PULSE.MOTOR.STOP
	    public const int CMD_SET_PLM_JOG				= 0x32;	// PULSE.MOTOR.JOG.MODE
	    public const int CMD_SET_PLM_ORG				= 0x26;	// PULSE.MOTOR.ORG
	    public const int CMD_SET_PLM_TRQ				= 0x27;	// PULSE.MOTOR.TRQ
	    public const int CMD_SET_PLM_ENB				= 0x28;	// PULSE.MOTOR.STAND.BY
	    public const int CMD_SET_PLM_STB				= 0x33;	// PULSE.MOTOR.STAND.BY
	    public const int CMD_SET_PLM_POS				= 0x29;	// PULSE.MOTOR.COUNTER
	    public const int CMD_SET_PLM_LMT				= 0x30;	// PULSE.MOTOR.SOFT.LIMIT
		//public const int CMD_SET_DAC_PWD				= 0x31;	// DAC.POWER.DOWN
	    /* 2012.02.17 m.araya*/
		//public const int CMD_SET_PID_C_P				= 0x40;// 
		//public const int CMD_SET_PID_C_I				= 0x41;// 
		//public const int CMD_SET_PID_C_D				= 0x42;// 
		//public const int CMD_SET_PID_C_T				= 0x43;// TARGET

	    /**/
	    private static bool m_access = false;
	    private static bool m_bPresetDone = false;
	    /**/
	
	    static private int MAKELONG(int w1, int w2) {
	    // TODO 自動生成されたメソッド・スタブ
		    return((0xffff & w1) << 16 | (0xffff & w2));
	    }
	    static private int MAKELONG(int b4, int b3, int b2, int b1) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((0xff & b4) << 24 | (0xff & b3) << 16 | (0xff & b2) << 8 | (0xff & b1));
	    }
	    static private int WH(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((short)((l & 0xffff0000) >> 16));
	    }
	    /*private int WL(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((short)((l & 0x0000ffff) >> 0));
	    }*/
	    static private int B4(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((byte)((l & 0xff000000) >> 24));
	    }
	    static private int B3(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((byte)((l & 0x00ff0000) >> 16));
	    }
	    static private int B2(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((byte)((l & 0x0000ff00) >> 8));
	    }
	    static private int B1(int l) {
	    // TODO 自動生成されたメソッド・スタブ
		    return ((byte)((l & 0x000000ff) >> 0));
	    }
	    static public void PRESET_PARAM() {
		    if (m_bPresetDone) {
			    return;
		    }
			//int[]	STG_HSPD = {500, 500, 500};//PULSE/SEC
			//int[]	STG_LSPD = { 50,  50,  50};//PULSE/SEC
			//int[]	STG_JSPD = {100, 100, 100};//PULSE/SEC
			//int[]	STG_ACCL = {100, 100, 100};//ms
			//int[]	STG_LIMT = {950, 950,250};//PULSE
		    int		HSPD, LSPD, JSPD, ACCL, LIMT;
			//int		ia, ib, ic, it, tr;
		    for (int i = 0; i < 4; i++){
			    HSPD = G.SS.PLM_HSPD[i];
				LSPD = G.SS.PLM_LSPD[i];
				JSPD = G.SS.PLM_JSPD[i];
				ACCL = G.SS.PLM_ACCL[i];
				LIMT = G.SS.PLM_PLIM[i];
			    if (!CMDOUT(CMD_SET_PLM_PRM, /*AXIS=*/i, /*HISPD*/0, B2(HSPD), B1(HSPD), null)) {
				    return;
			    }
			    if (!CMDOUT(CMD_SET_PLM_PRM, /*AXIS=*/i, /*LOSPD*/1, B2(LSPD), B1(LSPD), null)) {
				    return;
			    }
			    if (!CMDOUT(CMD_SET_PLM_PRM, /*AXIS=*/i, /*JGSPD*/2, B2(JSPD), B1(JSPD), null)) {
				    return;
			    }
			    if (!CMDOUT(CMD_SET_PLM_PRM, /*AXIS=*/i, /*ACCEL*/3, B2(ACCL), B1(ACCL), null)) {
				    return;
			    }
			    if (!CMDOUT(CMD_SET_PLM_LMT, /*AXIS=*/i, B3(LIMT)  , B2(LIMT), B1(LIMT), null)) {
				    return;
			    }
			    if (true) {//2012.02.10 m.araya
					//int[]	STG_LMTM = {-950,-950,-5050};	//CCW側ソフトリミット値[PULSE]
				    //
					int LMTM = G.SS.PLM_MLIM[i];
				    if (!CMDOUT(CMD_SET_PLM_LMT, /*AXIS=*/i+4, B3(LMTM)  , B2(LMTM), B1(LMTM), null)) {
					    return;
				    }
			    }
		    }
			//再計算
			if (!CMDOUT(CMD_SET_PLM_PRM, -1, 0, 0, 0, null))
			{
				return;
			}

			//if (true) {//2012.02.10
			//    tr = 0;		// 許容範囲 0とする=>後ほどこの係数はカットします
			//    it = 50;	// 更新インターバル[ms] 40ms以上の値を指定すること
			//    it = 5 ;//2012.02.17　最小1msに設定
			//    CMDOUT(CMD_SET_AFC_C_T, B2(tr), B1(tr), null);
			//    CMDOUT(CMD_SET_AFC_C_I, B2(it), B1(it), null);
			//}
			//if (true) {//2012.02.11
			//    // デフォルトは800Hzでしたが、とりあえず 3200Hz としました.
			//    CMD_SET_PWM_FRQ(32767);	// 245-32767Hzの範囲で指定してください
			//}
			//if (true) {//2012.02.17 m.araya PID係数の設定
			//    SET_PID_COEFF_P(0.10f);
			//    SET_PID_COEFF_I(0.f);
			//    SET_PID_COEFF_D(0.f);
			//}
			//m_bPresetDone = true;
	    }
		static private int HID_ENUM(uint vid, uint pid, out int pcnt)
		{
#if true
			if ((G.AS.DEBUG_MODE & 1) != 0) {
				DBGMODE.HID_ENUM(vid, pid, out pcnt);
				return(1);
			}
#endif
			int		ret;
			IntPtr	buf = new IntPtr();

			buf = Marshal.AllocHGlobal(4);
			ret = _HID_ENUM(vid, pid, buf); ;
			pcnt = Marshal.ReadInt32(buf);
			Marshal.FreeHGlobal(buf);

			return (ret);
		}

		static public bool INIT() {
			int cnt;
			if (HID_ENUM(0x04D8, 0x003F, out cnt) != 0) {
				if (cnt <= 0) {
					//G.mlog("CAN NOT DETECT USB HID DEVICE");
					G.mlog("#s顕微鏡装置に接続できません.");
					return(false);
				}
#if true
				if ((G.AS.DEBUG_MODE & 1) != 0) {
					if (DBGMODE.HID_OPEN(0x04D8, 0x003F, 0) == 0) {
						return(false);
					}
					m_access = true;
					return (true);
				}
#endif
				if (HID_OPEN(0x04D8, 0x003F, 0) == 0) {
					G.mlog("ERROR @ HID_OPEN");
					return (false);
				}
			}
			m_access = true;
			return (true);
		}
		static public void SET_LED_STS(int idx, int on_off) {
  			if (true) {
				CMDOUT(CMD_SET_PWM_STS, idx == 0 ? idx: (3-idx), on_off, null);//1chと2chの入れ替え
				if (on_off != 0) {
					G.LED_PWR_STS |= (0x01 << idx);
				}
				else {
					G.LED_PWR_STS &=~(0x01 << idx);
				}
	  		}
	/*    	else {
				Message msg = Message.obtain(handler, MAKELONG(idx, on_off, 0, CMD_SET_PWM_STS));
				msg.sendToTarget();
			}*/
		}
		static public void SET_LED_DUTY(int idx, int duty) {
			CMDOUT(CMD_SET_PWM_DTY, idx == 0 ? idx: (3-idx), duty, null);//1chと2chの入れ替え
		}
		static public int GET_SW_STS() {
			byte[] buf = new byte[5];
			CMDOUT(CMD_GET_BTN_STS, 0, buf);
			return(buf[0]);
		}
		static public void TERM() {
			if (m_access) {
  	 			for (int q = 0; q < 2; q++) {
  	 	 			SET_LED_STS(q, 0);
 	 			}
  	 			for (int q = 0; q < 4; q++) {
  	 				SET_STG_TRQ(q, 0);
  	 			}
#if true
				if ((G.AS.DEBUG_MODE & 1) != 0) {
					DBGMODE.HID_CLOSE();
					m_access = false;
					return;
				}
#endif

				HID_CLOSE();
				m_access = false;
			}
		}
		//static public String GET_DEV_STR() {
		//    return(m_access.m_devstr);    	
		//}
		static public void SET_ILED_STS(int sts) {
			CMDOUT(CMD_SET_LED_STS, sts, null);
		}
		//static public void SET_HUB_PWR(int sts) {
		//    sts = ~sts;
		//    CMDOUT(CMD_SET_HUB_PWR, sts, null);
		//}
		static public bool isCONNECTED() {
			return(m_access);
		}
		static public void SET_STG_ORG(int idx) {
			SET_STG_TRQ(idx, 1);
			CMDOUT(CMD_SET_PLM_ORG, idx, null);
		}
		static public void SET_STG_ABS(int idx, int pos) {
			if (true) {
				byte[] buf = new byte[5];
				CMDOUT(CMD_GET_PLM_POS, idx, buf);
				int	cur_pos = MAKELONG(buf[0], buf[1], buf[2], buf[3]);
				int	dif = pos - cur_pos;
				if (dif != 0) {
#if true//2018.05.18
					SET_STG_TRQ(idx, 1);
#endif
					CMDOUT(CMD_SET_PLM_REL, idx, B3(dif), B2(dif), B1(dif), null);
				}
			}
			//else {
			//    SET_STG_TRQ(idx, 1);
			//    CMDOUT(CMD_SET_PLM_ABS, idx, B3(pos), B2(pos), B1(pos), null);
			//}
		}
		static public void SET_STG_REL(int idx, int cnt) {
			SET_STG_TRQ(idx, 1);
			CMDOUT(CMD_SET_PLM_REL, idx, B3(cnt), B2(cnt), B1(cnt), null);
		}
		static public void SET_STG_JOG(int idx, int dir) {
			SET_STG_TRQ(idx, 1);
			CMDOUT(CMD_SET_PLM_JOG, idx, dir, null);
		}
		static public int GET_STG_POS(int idx) {
			byte[] buf = new byte[5];
			CMDOUT(CMD_GET_PLM_POS, idx, buf);
			return(MAKELONG(buf[0], buf[1], buf[2], buf[3]));
		}
		static public void SET_STG_POS(int idx, int cnt) {
			byte[] buf = new byte[5];
			CMDOUT(CMD_SET_PLM_POS, idx, B3(cnt), B2(cnt), B1(cnt), null);
		}
		static public int GET_STG_STS() {
			byte[] buf = new byte[5];
			CMDOUT(CMD_GET_PLM_STS, 0, buf);
			return(MAKELONG(0, buf[2], buf[1], buf[0]));
		}
		static public int GET_STG_STS(byte[] buf)
		{
			return(CMDOUT(CMD_GET_PLM_STS, 0, buf) ? 1:0);
		}
		static public void SET_STG_STOP(int idx) {
			CMDOUT(CMD_SET_PLM_STP, idx, null);
		}
		static public void SET_STG_TRQ(int idx, int hi_lo) {
			CMDOUT(CMD_SET_PLM_TRQ, idx, hi_lo, null);
			if (hi_lo == 1) {
				G.STG_TRQ_STS |= (1<<idx);//TRQ.HIセット
			}
			else {
				G.STG_TRQ_STS &= ~(1<<idx);//TRQ.HIクリア
			}
		}
		//static public void SET_VCM_POS(int idx, int sts) {
		//    CMDOUT(CMD_SET_VCM_POS, idx, sts, null);
		//}
		/************************************************************/
		/* 2012.02.11 追加 */
		/************************************************************/
		//static public void SET_PWM_FRQ(int freq) {
		//    if (freq < 245) {
		//        freq = 245;
		//    }
		//    else
		//    if (freq > 32767) {
		//        freq = 32767;
		//    }
		//    CMDOUT(CMD_SET_PWM_FRQ, B2(freq), B1(freq), null);
		//}
		/************************************************************/
		static public bool CMDOUT(int cmd, int par1, byte[] buf)
		{
			return(CMDOUT(cmd, par1, 0, 0, 0, buf));
		}
		/************************************************************/
		static public bool CMDOUT(int cmd, int par1, int par2, byte[] buf)
		{
			return(CMDOUT(cmd, par1, par2, 0, 0, buf));
		}
		/************************************************************/
		static public bool CMDOUT(int cmd, int par1, int par2, int par3, byte[] buf)
		{
			return(CMDOUT(cmd, par1, par2, par3, 0, buf));
		}
		/************************************************************/
		static public bool CMDOUT(int cmd, int par1, int par2, int par3, int par4, byte[] buf)
		{
			byte[] commandPacket = new byte[16];
			commandPacket[0] = (byte)cmd;
			commandPacket[1] = (byte)par1;
			commandPacket[2] = (byte)par2;
			commandPacket[3] = (byte)par3;
			commandPacket[4] = (byte)par4;
#if true
			if ((G.AS.DEBUG_MODE & 1) != 0) {
				DBGMODE.WRITE_HID(commandPacket);
				if (buf != null) {
				DBGMODE.READ_HID(buf);
				}
				return(true);
			}
#endif
			if (WRITE_HID(commandPacket, commandPacket.Length) == 0) {
		//			Toast.makeText(TEST24Activity.this, "USB COMMUNICATION ERROR!!!", Toast.LENGTH_SHORT).show();
			//	return(false);
			}
			if (buf != null) {
				if (READ_HID(commandPacket, commandPacket.Length) == 0)
				{
					return(false);
				}
				for (int i = 0; i < commandPacket.Length; i++)
				{
					if (i >= buf.Length)
					{
						break;
					}
					buf[i] = commandPacket[i];
				}
			}
			return(true);
		}
		static public void CLEAR_PRESET_FLAG() {
			m_bPresetDone = false;
		}
    }
}
