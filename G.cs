using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//-----------------------
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;

namespace uSCOPE
{
	public class G
	{
		public delegate void DLG_VOID_VOID();
		public delegate void DLG_VOID_BOOL(bool b);
		public delegate void DLG_VOID_INT(int n);

		public class APPSET:System.ICloneable
		{
			public int TRACE_LEVEL = 0;
			public int DEBUG_MODE = 0;
			public int APP_F01_LFT = 700;
			public int APP_F01_TOP =   5;
			public int APP_F02_LFT =  10;
			public int APP_F02_TOP =   5;
			public int APP_F02_WID = 600;
			public int APP_F02_HEI = 800;
			public string AUT_BEF_PATH = "";
			public string BEFORE_PATH = "";
			//---
			public string PLM_AUT_FOLD = "";
			public string MOZ_CND_FOLD = "";

			public Object Clone()
			{
				APPSET cln = (APPSET)this.MemberwiseClone();
				return (cln);
			}
			public bool load(ref APPSET ss)
			{
				string path = GET_DOC_PATH("uSCOPE.xml");
				bool ret = false;
				try {
					XmlSerializer sz = new XmlSerializer(typeof(APPSET));
					System.IO.StreamReader fs = new System.IO.StreamReader(path, System.Text.Encoding.Default);
					APPSET obj;
					obj = (APPSET)sz.Deserialize(fs);
					fs.Close();
					obj = (APPSET)obj.Clone();
					ss = obj;
					ret = true;
				}
				catch (Exception /*ex*/) {
				}
				return(ret);
			}
			//
			public bool save(APPSET ss)
			{
				string path = GET_DOC_PATH("uSCOPE.xml");
				bool ret = false;
				try {
					XmlSerializer sz = new XmlSerializer(typeof(APPSET));
					System.IO.StreamWriter fs = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);
					sz.Serialize(fs, ss);
					fs.Close();
					ret = true;
				}
				catch (Exception /*ex*/) {
				}
				return (ret);
			}
		}

		public class SYSSET:System.ICloneable
		{
			[XmlIgnoreAttribute]
			public string AUT_BEF_PATH = null;
			[XmlIgnoreAttribute]
			public string BEFORE_PATH = null;
			public int[]	LED_PWM_VAL = { 90, 90, 90};
			public bool		LED_PWM_AUTO = true;
			//[XmlIgnoreAttribute]
			//public bool		LED_STS;
			//---
			public int[] PLM_JSPD = { 300, 300, 300, 300 };
			public int[] PLM_ACCL = { 200, 200, 200, 200 };
			public int[] PLM_HSPD = { 2000, 2000, 2000, 2000 };
			public int[] PLM_LSPD = { 200, 200, 200, 200 };
			public int[] PLM_PLIM = { 3300, 3300, 8500, 9000 };
			public int[] PLM_MLIM = { -3000, -3100, -2800, -500 };
			public int[]	PLM_BSLA = { 0, 0, 0, 0};
			public int[] PLM_OFFS = { 0, 0, 0, 0 };
			public double[] PLM_UMPP = { 2.5, 2.5, 0.35, 0};
			public bool[] PLM_PWSV = { true, true, true, true};
			//---
			public int[]	PLM_POSX = { -3000, 0, 3000, 0 };
			public int[]	PLM_POSY = { -3000, 0, 3000, 0 };
			public int[]	PLM_POSF = { 884, 884, 884,  4500 };
			public int[] PLM_POSZ = { -209, 3088, 6386, 6386 };
			public string[] PLM_POSWT = { "メモ1", "メモ2", "メモ3" };
			public string[] PLM_POSFT = { "メモ1", "メモ1", "メモ3" };
			public string[] PLM_POSZT = { "メモ1", "メモ2", "メモ3" };
			public double ZOM_PLS_A = 6.0653E-04;
			public double ZOM_PLS_B = 4.1268E+00;
			public double ZOM_TST_Y = 4;
			public int	ZOM_TST_X = 0;
			public double CAM_SPE_UMPPX = 2.2;
			//[XmlIgnoreAttribute]
			//public int		PLM_STS = 0;
			//---
			public int[]	PLM_DAT_DIST = { 100, 100, 100, 100 };
			//---
			//[XmlIgnoreAttribute]
			//public int		CAM_PRC_MODE;
			public bool		CAM_PAR_AUTO = true;
			public double[] CAM_PAR_GAMMA = { 1.0, 2.0, 1.0 };
			public double[] CAM_PAR_CONTR = { 0.0, 0.0, 0.0 };
			public double[] CAM_PAR_BRIGH = { 0.3, 0.3, 0.3 };
			public double[] CAM_PAR_SHARP = { 0.0, 0.0, 0.0 };
			//public int      CAM_PAR_EXMOD = 0;
			//public int      CAM_PAR_WBMOD = 0;
			public int[]	CAM_PAR_GAMOD = {1, 1, 1};//自動
			public int[]	CAM_PAR_EXMOD = {1, 1, 1};//自動
			public int[]	CAM_PAR_WBMOD = {1, 1, 1};//自動
			public double[]	CAM_PAR_GA_VL = {1.0, 1.0, 1.0};
			public double[]	CAM_PAR_GA_OF = {0, 0, 0};
			public double[]	CAM_PAR_EX_VL = {1000.0, 1000.0, 1000.0};
			public double[]	CAM_PAR_EX_OF = {0, 0, 0};
			public double[]	CAM_PAR_WB_RV = {1.0, 1.0, 1.0};
			public double[]	CAM_PAR_WB_GV = {1.0, 1.0, 1.0};
			public double[]	CAM_PAR_WB_BV = {1.0, 1.0, 1.0};
			//---
			public int		CAM_HIS_BVAL = 110;
			public int CAM_HIS_PAR1 = 0;
			public int CAM_HIS_METH = 0;
			public int CAM_HIS_OIMG = 0;
			public int CAM_HIS_DISP = 0;
			public bool CAM_HIS_CHK1 = true;
			public bool CAM_HIS_CHK2 = false;
			public int CAM_HIS_RT_X = 100;
			public int CAM_HIS_RT_Y = 100;
			public int CAM_HIS_RT_W = 200;
			public int CAM_HIS_RT_H = 200;
			//---
			public int CAM_CND_MODH = 1;
			public int CAM_CND_MINH = 320;
			public int CAM_CND_MAXH = 70;
			public int CAM_CND_MINS = 30;
			public int CAM_CND_MAXS = 200;
			public int CAM_CND_MINV = 64;
			public int CAM_CND_MAXV = 255;
			//---
			public int CAM_CIR_FILT = 5;//11x11
			//public int CAM_CIR_BVAL = 110;

			public int CAM_CIR_AREA = 10000;
			public int CAM_CIR_LENG = 3000;
			public double CAM_CIR_CVAL = 0.30;

			public int CAM_CIR_AREA_MAX = 1250000;
			public int CAM_CIR_LENG_MAX = 12500;
			public double CAM_CIR_CVAL_MIN = 0.05;
			public double CAM_CIR_MAGN = 8.0;

			public int CAM_DIR_PREC = 15;
			public int CAM_CIR_DISP = 0;
			public bool CAM_CIR_CHK1 = true;
			public bool CAM_CIR_CHK2 = false;
			public bool CAM_CIR_CHK3 = true;
			public bool CAM_CIR_CHK4 = false;
			//---
			public int CAM_FCS_LMIN = -2500;
			public int CAM_FCS_LMAX = +8500;
			public int CAM_FCS_DISL = 500;
			public int CAM_FCS_DISM = 50;
			public int CAM_FCS_DISS = 5;
			public int CAM_FCS_PAR1 = 0;
			public int CAM_FCS_DISP = 0;
			//public bool CAM_FCS_CHK1 = false;
			public bool CAM_FCS_CHK2 = false;
			public int CAM_FCS_SKIP = 1;
			public int CAM_FCS_FAVG = 1;
			public bool CAM_FCS_USSD = false;
			//---
#if true//2018.09.27(20本対応と解析用パラメータ追加)
			public int[] IMP_FLT_COEF = {5,5,5,5,5,5};
			public int[] IMP_BIN_MODE = {1,1,0,1,1,0};
			public int[] IMP_BIN_BVAL = {128,128,1,128,128,1};
			public int[] IMP_HUE_LOWR = {300,300,300,300,300,300};
			public int[] IMP_HUE_UPPR = {60,60,60,60,60,60};
			public int[] IMP_SAT_LOWR = {30,30,30,30,30,30};
			public int[] IMP_SAT_UPPR = {200,200,200,200,200,200};
			public int[] IMP_VAL_LOWR = {64,64,64,64,64,64};
			public int[] IMP_VAL_UPPR = {255,255,255,255,255,255};
			public int[] IMP_SUM_LOWR = {  10000,  10000,  10000,  10000,  10000,  10000};
			public int[] IMP_SUM_UPPR = {2500000,2500000,2500000,2500000,2500000,2500000};
			public int[] IMP_LEN_LOWR = {  1000,  1000,  1000,  1000,  1000,  1000};
			public int[] IMP_LEN_UPPR = {100000,100000,100000,100000,100000,100000};
			public double[] IMP_CIR_LOWR = {0.0,0.0,0.0,0.0,0.0,0.0};
			public double[] IMP_CIR_UPPR = {0.3,0.3,0.3,0.3,0.3,0.3};

			public int[] IMP_CUV_LOWR = {0,0,0,0,0,0};
			public int[] IMP_CUV_UPPR = {0,0,0,0,0,0};
			public double[] IMP_GIZ_LOWR = {0,0,0,0,0,0};
			public double[] IMP_GIZ_UPPR = {0,0,0,0,0,0};

			public int[] IMP_POL_PREC = {28,28,28,28,28,28};
			public double[] IMP_OPT_MAGN = {8.0,8.0,8.0,8.0,8.0,8.0};
			public int[] IMP_AUT_AFMD = {0, 0, 0, 0, 0, 0};
#else
			public int[] IMP_FLT_COEF = {5,5,5,5};
			public int[] IMP_BIN_MODE = {1,1,0,0};
			public int[] IMP_BIN_BVAL = {128,128,1,125};
			public int[] IMP_HUE_LOWR = {300,300,300,300};
			public int[] IMP_HUE_UPPR = {60,60,60,60};
			public int[] IMP_SAT_LOWR = {30,30,30,30};
			public int[] IMP_SAT_UPPR = {200,200,200,200};
			public int[] IMP_VAL_LOWR = {64,64,64,64};
			public int[] IMP_VAL_UPPR = {255,255,255,255};
			public int[] IMP_SUM_LOWR = {  10000,  10000,  10000,  35000};
			public int[] IMP_SUM_UPPR = {2500000,2500000,2500000,1000000};
			public int[] IMP_LEN_LOWR = {  1000,  1000,  1000,  2500};
			public int[] IMP_LEN_UPPR = {100000,100000,100000,100000};
			public double[] IMP_CIR_LOWR = {0.0,0.0,0.0,0.0};
			public double[] IMP_CIR_UPPR = {0.3,0.3,0.3,0.04};

			public int[] IMP_CUV_LOWR = {0,0,0,0};
			public int[] IMP_CUV_UPPR = {0,0,0,10000};
			public double[] IMP_GIZ_LOWR = {0,0,0,0};
			public double[] IMP_GIZ_UPPR = {0,0,0,0.25};

			public int[] IMP_POL_PREC = {28,28,28,28};
			public double[] IMP_OPT_MAGN = {8.0,8.0,8.0,8.0};
			public int[] IMP_AUT_AFMD = {0, 0, 0, 0};
#endif
			//---
			public bool PLM_AUT_FINI = true;
			public bool PLM_AUT_ZINI = true;
			public string PLM_AUT_TITL = "";
			//public int PLM_AUT_SPOS = 0;
			public int PLM_AUT_OVLP = 5;
			public int PLM_AUT_FLTP = 1;
			[XmlIgnoreAttribute]
			public string PLM_AUT_FOLD = "";
			public int PLM_AUT_MODE = 1;
			public int PLM_AUT_SKIP = 2;
			public int PLM_AUT_HANI = 200;
			public int PLM_AUT_DISL = 50;
			public int PLM_AUT_DISS = 5;
			public int PLM_AUT_AFMD = 0;
			//---
			public int PLM_AUT_FCMD = 1;
			public int PLM_AUT_CTDR = 10;
			public int PLM_AUT_2HAN = 100;
			public int PLM_AUT_2DSL = 25;
			public int PLM_AUT_2DSS = 5;
			public bool PLM_AUT_2FST = false;
			//---
			public bool PLM_AUT_HPOS = false;
			public int PLM_AUT_HMOD = 0;
			public int PLM_AUT_HP_X = 0;
			public int PLM_AUT_HP_Y = -3000;
#if true//2018.07.02
			public int PLM_AUT_HP_Z = 0;
#endif
#if true//2018.07.30
            public int PLM_AUT_ED_Y = +3000;
#endif
			public int PLM_AUT_HPRT = 5;
			public int PLM_AUT_HPMN = 3000;
			public int PLM_AUT_HPMX = 6000;
			public int PLM_AUT_HPSL = 200;
			public int PLM_AUT_HPSS = 200;
			//---
			public bool PLM_AUT_CNST = false;
			public bool PLM_AUT_RTRY = false;
#if true//2018.12.22(測定抜け対応)
			public bool PLM_AUT_NUKE = true;
#endif
			//---
			//public bool PLM_AUT_ZMUL = false;
			//public int PLM_AUT_ZHAN = 200;
			//public int PLM_AUT_ZSTP =  50;
			//---
			public bool PLM_AUT_ZDCK = false;//Ｚ測定:深度合成用
			public int[] PLM_AUT_ZDEP = null;
			//---
			public bool PLM_AUT_ZKCK = false;//Ｚ測定:毛髪径判定用
			public int[] PLM_AUT_ZKEI = null;
			//---
			public bool PLM_AUT_IRCK = false;
#if true//2018.08.16
			public bool PLM_AUT_ZORG = false;
			public bool PLM_AUT_ZNOR = false;
#endif
			//---
			public int ETC_LED_WAIT = 18;
			public int ETC_UIF_LEVL =  0;
			public int ETC_UIF_BACK =  0;
			public bool ETC_UIF_CUTI = false;
			public bool ETC_LED_IRGR = true;
#if false//2018.06.07
			public int ETC_CLF_CTCR = 1;
#endif
			//---
			public int ETC_DAN_MODE = 0;
			public int ETC_HIS_MODE = 0;
			public int ETC_SPE_CD01 = 0;//一度だけ実行(時期を見て削除すること)
			public int ETC_SPE_CD02 = 0;//一度だけ実行(時期を見て削除すること)
			[XmlIgnoreAttribute]
			public Color ETC_BAK_COLOR = Color.FromArgb(198, 3, 85);
			[XmlElement("ETC_BAK_COLOR")]
			public string ETC_BAK_COLOR_STR {
				get { return ColorTranslator.ToHtml(this.ETC_BAK_COLOR); }
				set { this.ETC_BAK_COLOR = ColorTranslator.FromHtml(value); }
			}
#if true
			[XmlIgnoreAttribute]
			public Color ETC_CRS_COLOR = Color.FromArgb(0, 255, 255);
			[XmlElement("ETC_CRS_COLOR")]
			public string ETC_CRS_COLOR_STR {
				get { return ColorTranslator.ToHtml(this.ETC_CRS_COLOR); }
				set { this.ETC_CRS_COLOR = ColorTranslator.FromHtml(value); }
			}
			public int ETC_CRS_LENGTH = 50;
#endif
#if true//2018.11.10(保存機能)
			public int MOZ_SAV_DMOD = 0;
			public string MOZ_SAV_FOLD = "";
			public int MOZ_SAV_FMOD = 0;
			public string MOZ_SAV_NAME = "";
#endif
			//---
			public int MOZ_CND_FMOD = 0;
			[XmlIgnoreAttribute]
			public string MOZ_CND_FOLD = "";
			public int MOZ_CND_CUTE = 5;
			public int MOZ_CND_ZVAL = 50;
#if true//2018.09.29(キューティクルライン検出)
			public double MOZ_CND_DSUM = 1;
#else
			public int MOZ_CND_DSUM = 3;
#endif
			public int MOZ_CND_FTCF = 5;//11x11
			public int MOZ_CND_FTCT = 0;//1回
			public int MOZ_CND_SMCF = 9;//重み係数=21
#if true//2018.10.10(毛髪径算出・改造)
			public int MOZ_CND_OTW1 = 21;		//外れ値判定:幅  (毛髄長さ)
			public double MOZ_CND_OTV1 = 1.6;	//外れ値判定:閾値(毛髄長さ)

			public int MOZ_CND_OTW2 = 31;		//外れ値判定:幅  (毛髄中心)
			public double MOZ_CND_OTV2 = 1.2;	//外れ値判定:閾値(毛髄中心)
			public int MOZ_CND_OTMD = 1;		//外れ値判定:補間,1:直線補間
			public int MOZ_CND_SLVL = 128;		//毛髄面積:D/L区分閾値
			public double MOZ_CND_SMVL = 5;		//除外判定:面積値
			public bool MOZ_CND_CHK1 = true;	//有,無効:除外判定:毛髄面積
			public bool MOZ_CND_CHK2 = true;	//有,無効:外れ値判定:毛髄長さ
			public bool MOZ_CND_CHK3 = true;	//有,無効:外れ値判定:毛髄中心
			//0:無し
			//1:毛髪上下端全範囲
			//2:毛髄判定範囲
			//3:毛髄判定範囲で上下別々に(等面積)
			//4:毛髄判定範囲で上下別々に(等最大値)
			public int MOZ_CND_CNTR = 1;//コントラスト補正
#else
			public bool MOZ_CND_CTRA = true;//コントラスト補正
#endif
			public int MOZ_CND_HANI = 75;//毛髄判定範囲[%]
			//---
			public int MOZ_CND_PDFL = 0;//位置検出
			public int MOZ_CND_DMFL = 0;//断面画像
#if true//2018.08.21
			public string MOZ_CND_ZPCT;//Z位置:キューティクル
			public string MOZ_CND_ZPHL;//Z位置:毛髪径(位置検出)
			public string MOZ_CND_ZPML;//Z位置:毛髄径
#else
			public string MOZ_CND_ZPOS;
#endif
			public int MOZ_CND_ZCNT;
			public bool MOZ_CND_NOMZ;//画像表示のみ
			//public int MOZ_IRC_FILT = 5;
			//public int MOZ_IRC_ZVAL = 135-10;
			//public int MOZ_IRC_SMAX = 1000000;
			//public int MOZ_IRC_SMIN = 35000;
			//public int MOZ_IRC_LMAX = 1000000;
			//public int MOZ_IRC_LMIN = 2500;
			//public double MOZ_IRC_CMAX = 0.04;
			//public double MOZ_IRC_CMIN = 0;
			//public int MOZ_IRC_KMAX = 10000;
			//public int MOZ_IRC_KMIN = 0;
			//public double MOZ_IRC_UMAX = 0.25;
			//public double MOZ_IRC_UMIN = 0;
#if false//2018.08.21
			public bool MOZ_IRC_NOMZ = true;
			public bool MOZ_IRC_SAVE = true;
#endif
#if true//2018.09.29(キューティクルライン検出)
			public int MOZ_CND_CTYP = 0;//キューティクル(0:BPF,1:2d)
			public double MOZ_CND_BPF1 = 0.047;
			public double MOZ_CND_BPF2 = 0.300;
			public int MOZ_CND_BPSL = 1;//1:スロープ=普通
			public int MOZ_CND_NTAP = 11;//フィルタタップ数
			public double MOZ_CND_BPVL = 25;
			public int MOZ_CND_2DC0 = 7;
			public int MOZ_CND_2DC1 = 0;
			public int MOZ_CND_2DC2 = 0;
			public double MOZ_CND_2DVL = 4.0;
			//---
			public int MOZ_CND_HWID = 10;
			public int MOZ_CND_HMAX = 70;
			[XmlIgnoreAttribute]
			public int MOZ_CND_HCNT = (70/10);
			[XmlIgnoreAttribute]
			public double[] MOZ_CND_FCOF = null;
#endif
#if true//2018.10.30(キューティクル長)
			public int MOZ_CND_CHAN = 65;
			public int MOZ_CND_CMIN = 2;
			public int MOZ_CND_CNEI = 1;
#endif
#if true//2018.11.02(HSVグラフ)
			public int MOZ_CND_HIST = 65;
#endif
#if true//2018.08.21
			public bool MOZ_IRC_CK00 = false;
			public bool MOZ_IRC_CK01 = false;
			public bool MOZ_IRC_CK02 = false;
			public bool MOZ_IRC_CK03 = false;
#else
			public bool MOZ_IRC_CK00 = true;
			public bool MOZ_IRC_CK01 = true;
			public bool MOZ_IRC_CK02 = true;
			public bool MOZ_IRC_CK03 = true;
#endif
			public int MOZ_IRC_DISP = 0;
			//---
			public bool MOZ_FST_CK00 = false;
#if true//2018.07.02
			public bool MOZ_FST_CK01 = false;
#endif
			public int MOZ_FST_RCNT = 3;
			public int MOZ_FST_CCNT = 3;
			public int MOZ_FST_MODE = 0;
			public int MOZ_FST_FCOF = 0;
#if true//2018.11.13(毛髪中心AF)
			public int MOZ_FST_IMTP = 0;
#endif
			//---
			public int TST_PAR_GAUS = 0;
			public bool TST_PAR_CHK1 = true;
			public bool TST_PAR_CHK2 = false;
			public bool TST_PAR_CHK3 = false;
			public int TST_PAR_VAL1 = 12;		//半径1(カーネルサイズ)
			public int TST_PAR_VAL2 = 24;		//半径2(カーネルサイズ)
			public int TST_PAR_VAL3 = 14;		//カーネルサイズ
			public int TST_PAR_VAL4 = 3;		//二値化閾値
			//public int TST_PAR_VAL5 = 50;		//th1:50
			//public int TST_PAR_VAL6 = 200;	//th2:200
			//public int TST_PAR_VAL7 = 3;		//apert_size:3
			public double TST_PAR_DBL1 = 3.5;	//σ1
			public double TST_PAR_DBL2 = 11.1;	//σ2
			public int TST_PAR_DISP = 0;
			//
			public int TST_PAR_ORDR = 0;		//処理手順
			public int TST_PAR_EROD = 6;		//収縮
			public int TST_PAR_DILA = 8;		//膨張
			public int TST_PAR_THIN = 0;		//細線
			public int TST_PAR_SMIN = 7000;		//面積:MIN
			public int TST_PAR_SMAX = 30000;	//面積:MAX
			public int TST_PAR_LMIN = 0;		//周囲長:MIN
			public int TST_PAR_LMAX = 200000;	//周囲長:MAX
			public int TST_PAR_PREC = 0;		//多曲線近似精度
			//
			//public double TST_PAR_DBL3 = 50;
			//public double TST_PAR_DBL4 = 200;
			[XmlIgnoreAttribute]
			public int EUI_XYA_MODE = 0;
			[XmlIgnoreAttribute]
			public int EUI_ZFC_MODE = 0;
			[XmlIgnoreAttribute]
			public int EUI_ZOM_MODE = 0;
			public int[] EUI_XYA_PCNT = {-1, 8, 16};
			public int[] EUI_ZFC_PCNT = {-1, 8, 16};
			public int[] EUI_ZOM_PCNT = {-1, 8, 16};
			public string[] EUI_XYA_TEXT = {"連続", "x 8", "x 16"};
			public string[] EUI_ZFC_TEXT = {"連続", "x 8", "x 16"};
			public string[] EUI_ZOM_TEXT = {"連続", "x 8", "x 16"};
			public int[] EUI_ZOM_PSET = {3088, 6386};
			public string[] EUI_ZOM_LABL = {"8倍", "16倍"};
			//---
			public void RESIZE_ARRAY(ref int[] dst, ref int[] src)
			{
				int slen = src.Length;
				int dlen = dst.Length;
				if (dlen != slen) {
					System.Array.Resize(ref dst, slen);
					if (slen > dlen) {
						System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
					}
				}
			}
			public void RESIZE_ARRAY(ref double[] dst, ref double[] src)
			{
				int slen = src.Length;
				int dlen = dst.Length;
				if (dlen != slen) {
					System.Array.Resize(ref dst, slen);
					if (slen > dlen) {
						System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
					}
				}
			}
			public void RESIZE_ARRAY(ref string[] dst, ref string[] src)
			{
				int slen = src.Length;
				int dlen = dst.Length;
				if (dlen != slen) {
					System.Array.Resize(ref dst, slen);
					if (slen > dlen) {
						System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
					}
				}
			}
			//---
			public Object Clone()
			{
				SYSSET cln = (SYSSET)this.MemberwiseClone();
				cln.LED_PWM_VAL  =(int[])this.LED_PWM_VAL.Clone();
				cln.PLM_JSPD = (int[])this.PLM_JSPD.Clone();
				cln.PLM_ACCL = (int[])this.PLM_ACCL.Clone();
				cln.PLM_HSPD = (int[])this.PLM_HSPD.Clone();
				cln.PLM_LSPD = (int[])this.PLM_LSPD.Clone();
				cln.PLM_PLIM = (int[])this.PLM_PLIM.Clone();
				cln.PLM_MLIM = (int[])this.PLM_MLIM.Clone();
				cln.PLM_BSLA = (int[])this.PLM_BSLA.Clone();
				cln.PLM_OFFS = (int[])this.PLM_OFFS.Clone();
				cln.PLM_UMPP = (double[])this.PLM_UMPP.Clone();
				cln.PLM_POSX = (int[])this.PLM_POSX.Clone();
				cln.PLM_POSY = (int[])this.PLM_POSY.Clone();
				cln.PLM_POSF = (int[])this.PLM_POSF.Clone();
				cln.PLM_POSZ = (int[])this.PLM_POSZ.Clone();
				cln.PLM_POSWT = (string[])this.PLM_POSWT.Clone();
				cln.PLM_POSFT = (string[])this.PLM_POSFT.Clone();
				cln.PLM_POSZT = (string[])this.PLM_POSZT.Clone();
				cln.PLM_DAT_DIST = (int[])this.PLM_DAT_DIST.Clone();
				cln.CAM_PAR_GAMMA = (double[])this.CAM_PAR_GAMMA.Clone();
				cln.CAM_PAR_CONTR = (double[])this.CAM_PAR_CONTR.Clone();
				cln.CAM_PAR_BRIGH = (double[])this.CAM_PAR_BRIGH.Clone();
				cln.CAM_PAR_SHARP = (double[])this.CAM_PAR_SHARP.Clone();
#if true
				//設定ファイルにて新規のサイズ4がサイズ3に上書きされてしまうため...
				RESIZE_ARRAY(ref cln.PLM_POSX, ref G.SS.PLM_POSX);
				RESIZE_ARRAY(ref cln.PLM_POSY, ref G.SS.PLM_POSY);
				RESIZE_ARRAY(ref cln.PLM_POSF, ref G.SS.PLM_POSF);
				RESIZE_ARRAY(ref cln.PLM_POSZ, ref G.SS.PLM_POSZ);
				//---
				RESIZE_ARRAY(ref cln.LED_PWM_VAL, ref G.SS.LED_PWM_VAL);
				//---
				RESIZE_ARRAY(ref cln.CAM_PAR_GAMMA, ref G.SS.CAM_PAR_GAMMA);
				RESIZE_ARRAY(ref cln.CAM_PAR_CONTR, ref G.SS.CAM_PAR_CONTR);
				RESIZE_ARRAY(ref cln.CAM_PAR_BRIGH, ref G.SS.CAM_PAR_BRIGH);
				RESIZE_ARRAY(ref cln.CAM_PAR_SHARP, ref G.SS.CAM_PAR_SHARP);
				//---
				RESIZE_ARRAY(ref cln.CAM_PAR_EXMOD, ref G.SS.CAM_PAR_EXMOD);
				RESIZE_ARRAY(ref cln.CAM_PAR_WBMOD, ref G.SS.CAM_PAR_WBMOD);
#else
				// 以下のようにやってもサイズ変更がcln側に反映されない...
				object[] cur = {
					cln.LED_PWM_VAL,	cln.PLM_JSPD,		cln.PLM_ACCL,
					cln.PLM_HSPD,		cln.PLM_LSPD,		cln.PLM_PLIM,
					cln.PLM_MLIM,		cln.PLM_BSLA,		cln.PLM_OFFS,
					cln.PLM_UMPP,		cln.PLM_POSX,		cln.PLM_POSY,
					cln.PLM_POSF,		cln.PLM_POSZ,		cln.PLM_POSWT,
					cln.PLM_POSFT,		cln.PLM_POSZT,		cln.PLM_DAT_DIST,
					cln.CAM_PAR_GAMMA,	cln.CAM_PAR_CONTR,	cln.CAM_PAR_BRIGH,
					cln.CAM_PAR_SHARP
				};
				object[] org = {
					G.SS.LED_PWM_VAL,	G.SS.PLM_JSPD,		G.SS.PLM_ACCL,
					G.SS.PLM_HSPD,		G.SS.PLM_LSPD,		G.SS.PLM_PLIM,
					G.SS.PLM_MLIM,		G.SS.PLM_BSLA,		G.SS.PLM_OFFS,
					G.SS.PLM_UMPP,		G.SS.PLM_POSX,		G.SS.PLM_POSY,
					G.SS.PLM_POSF,		G.SS.PLM_POSZ,		G.SS.PLM_POSWT,
					G.SS.PLM_POSFT,		G.SS.PLM_POSZT,		G.SS.PLM_DAT_DIST,
					G.SS.CAM_PAR_GAMMA,	G.SS.CAM_PAR_CONTR,	G.SS.CAM_PAR_BRIGH,
					G.SS.CAM_PAR_SHARP
				};
				for (int i = 0; i < org.Length; i++) {
					Type tp = cur[i].GetType();
					if (false) {
					}
					else if (tp.Equals(typeof(int[]))) {
						int[] src = (int[])org[i];
						int[] dst = (int[])cur[i];
						int slen = src.Length;
						int dlen = dst.Length;
						if (dlen != slen) {
							System.Array.Resize(ref dst, slen);
							if (slen > dlen) {
								System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
							}
							//System.Array.Resize(ref (int[])(cur[i]), slen);
							cur[i] = dst;
						}
					}
					else if (tp.Equals(typeof(double[]))) {
						double[] src = (double[])org[i];
						double[] dst = (double[])cur[i];
						int slen = src.Length;
						int dlen = dst.Length;
						if (dlen != slen) {
							System.Array.Resize(ref dst, slen);
							if (slen > dlen) {
								System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
							}
						}
					}
					else if (tp.Equals(typeof(string[]))) {
						string[] src = (string[])org[i];
						string[] dst = (string[])cur[i];
						int slen = src.Length;
						int dlen = dst.Length;
						if (dlen != slen) {
							System.Array.Resize(ref dst, slen);
							if (slen > dlen) {
								System.Array.Copy(src, dlen, dst, dlen, (slen - dlen));
							}
						}
					}
				}
#endif
#if true//2018.09.27(20本対応と解析用パラメータ追加)
				//設定ファイルにて新規のサイズ6が旧サイズ4に上書きされてしまうため...
				RESIZE_ARRAY(ref cln.IMP_FLT_COEF, ref G.SS.IMP_FLT_COEF);
				RESIZE_ARRAY(ref cln.IMP_BIN_MODE, ref G.SS.IMP_BIN_MODE);
				RESIZE_ARRAY(ref cln.IMP_BIN_BVAL, ref G.SS.IMP_BIN_BVAL);
				RESIZE_ARRAY(ref cln.IMP_HUE_LOWR, ref G.SS.IMP_HUE_LOWR);
				RESIZE_ARRAY(ref cln.IMP_HUE_UPPR, ref G.SS.IMP_HUE_UPPR);
				RESIZE_ARRAY(ref cln.IMP_SAT_LOWR, ref G.SS.IMP_SAT_LOWR);
				RESIZE_ARRAY(ref cln.IMP_SAT_UPPR, ref G.SS.IMP_SAT_UPPR);
				RESIZE_ARRAY(ref cln.IMP_VAL_LOWR, ref G.SS.IMP_VAL_LOWR);
				RESIZE_ARRAY(ref cln.IMP_VAL_UPPR, ref G.SS.IMP_VAL_UPPR);
				RESIZE_ARRAY(ref cln.IMP_SUM_LOWR, ref G.SS.IMP_SUM_LOWR);
				RESIZE_ARRAY(ref cln.IMP_SUM_UPPR, ref G.SS.IMP_SUM_UPPR);
				RESIZE_ARRAY(ref cln.IMP_LEN_LOWR, ref G.SS.IMP_LEN_LOWR);
				RESIZE_ARRAY(ref cln.IMP_LEN_UPPR, ref G.SS.IMP_LEN_UPPR);
				RESIZE_ARRAY(ref cln.IMP_CIR_LOWR, ref G.SS.IMP_CIR_LOWR);
				RESIZE_ARRAY(ref cln.IMP_CIR_UPPR, ref G.SS.IMP_CIR_UPPR);
				RESIZE_ARRAY(ref cln.IMP_CUV_LOWR, ref G.SS.IMP_CUV_LOWR);
				RESIZE_ARRAY(ref cln.IMP_CUV_UPPR, ref G.SS.IMP_CUV_UPPR);
				RESIZE_ARRAY(ref cln.IMP_GIZ_LOWR, ref G.SS.IMP_GIZ_LOWR);
				RESIZE_ARRAY(ref cln.IMP_GIZ_UPPR, ref G.SS.IMP_GIZ_UPPR);
				RESIZE_ARRAY(ref cln.IMP_POL_PREC, ref G.SS.IMP_POL_PREC);
				RESIZE_ARRAY(ref cln.IMP_OPT_MAGN, ref G.SS.IMP_OPT_MAGN);
				RESIZE_ARRAY(ref cln.IMP_AUT_AFMD, ref G.SS.IMP_AUT_AFMD);
#endif
				return (cln);
			}
			public void preset_for_2018_MAR()
			{
				/*
				DDV.DDX(bUpdate, this.comboBox5     , ref  G.SS.IMP_FLT_COEF[3] = G.SS.MOZ_IRC_FILT);
				G.SS.IMP_BIN_MODE[3] = 1;//GRAY-SCALE
				IMP_HUE_LOWR[i]
				IMP_HUE_UPPR[i]
				IMP_SAT_LOWR[i]
				IMP_SAT_UPPR[i]
				IMP_VAL_LOWR[i]
				IMP_VAL_UPPR[i]
				DDV.DDX(bUpdate, this.numericUpDown30, ref G.SS.IMP_BIN_BVAL[3] = G.SS.MOZ_IRC_ZVAL);
				DDV.DDX(bUpdate, this.numericUpDown31, ref G.SS.IMP_SUM_UPPR[3] = G.SS.MOZ_IRC_SMAX);
				DDV.DDX(bUpdate, this.numericUpDown32, ref G.SS.IMP_SUM_LOWR[3] = G.SS.MOZ_IRC_SMIN);
				DDV.DDX(bUpdate, this.numericUpDown33, ref G.SS.IMP_LEN_UPPR[3] = G.SS.MOZ_IRC_LMAX);
				DDV.DDX(bUpdate, this.numericUpDown34, ref G.SS.IMP_LEN_LOWR[3] = G.SS.MOZ_IRC_LMIN);
				DDV.DDX(bUpdate, this.numericUpDown35, ref G.SS.IMP_CIR_UPPR[3] = G.SS.MOZ_IRC_CMAX);
				DDV.DDX(bUpdate, this.numericUpDown36, ref G.SS.IMP_CIR_LOWR[3] = G.SS.MOZ_IRC_CMIN);
				DDV.DDX(bUpdate, this.numericUpDown37, ref G.SS.IMP_CUV_UPPR[3] = G.SS.MOZ_IRC_KMAX);
				DDV.DDX(bUpdate, this.numericUpDown38, ref G.SS.IMP_CUV_LOWR[3] = G.SS.MOZ_IRC_KMIN);
				DDV.DDX(bUpdate, this.numericUpDown39, ref G.SS.IMP_GIZ_UPPR[3] = G.SS.MOZ_IRC_UMAX);
				DDV.DDX(bUpdate, this.numericUpDown40, ref G.SS.IMP_GIZ_LOWR[3] = G.SS.MOZ_IRC_UMIN);
				 */
			}
			//
			public bool load(ref SYSSET ss)
			{
				string path = GET_DOC_PATH("settings.xml");
				bool ret = false;
				try {
					XmlSerializer sz = new XmlSerializer(typeof(SYSSET));
					System.IO.StreamReader fs = new System.IO.StreamReader(path, System.Text.Encoding.Default);
					SYSSET obj;
					obj = (SYSSET)sz.Deserialize(fs);
					fs.Close();
					obj = (SYSSET)obj.Clone();
					ss = obj;
					ret = true;
				}
				catch (Exception /*ex*/) {
				}
				return(ret);
			}
			//
			public bool save(SYSSET ss)
			{
				string path = GET_DOC_PATH("settings.xml");
				bool ret = false;
				try {
					XmlSerializer sz = new XmlSerializer(typeof(SYSSET));
					System.IO.StreamWriter fs = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);
					sz.Serialize(fs, ss);
					fs.Close();
					ret = true;
				}
#if true//2018.10.10(毛髪径算出・改造)
				catch (Exception ex) {
					G.mlog(ex.Message);
				}
#else
				catch (Exception /*ex*/) {
				}
#endif
				return (ret);
			}
		};

		public class IP_RESULT:System.ICloneable
		{
			public bool HIST_RECT;
			public bool HIST_ALL;
			public double[] HISTVALY = new double[256];
			public double	HIST_MIN;
			public double	HIST_MAX;
			public double	HIST_AVG;
			public double	CONTRAST;
			//---
			public double[] HISTVALR = new double[256];
			public double[] HISTVALG = new double[256];
			public double[] HISTVALB = new double[256];
			public double[] HISTVALH = new double[256];
			public double[] HISTVALS = new double[256];
			public double[] HISTVALV = new double[256];
			public double[] HISTVALD = new double[256];
			//---
			public int	  CIR_CNT;
			public double CIR_S;
			public double CIR_L;
			public double CIR_C;
			public double CIR_P;
			public double CIR_U;
			public Rectangle CIR_RT;
			//---
			public Point[] DIA_TOP = new Point[16];
			public Point[] DIA_BTM = new Point[16];
			public int     DIA_CNT;	//DIA_TOP,DIT_BTMの有効数
			public Point[] MSK_PLY = new Point[16*2];
			public int     MSK_PLY_CNT;	//MSK_PLYの有効数
			public Point[] MSK_PLY_IMG = new Point[16*2];
			//---
			public Point[] PLY_PTS = new Point[256];
			public int	   PLY_CNT;
			public int     PLY_XMIN, PLY_XMAX;
			public int     PLY_YMIN, PLY_YMAX;
			//public PointF[] CIRCUM_PTF = new PointF[4];
			//public Point[]	CIRCUM_PTS = new Point[4];
			//public PointF[] DIAM_PTF = new PointF[2];
			//public Point[]	DIAM_PTS = new Point[2];
			//public double	DIAM_LEN;
			//---
			public int	EDG_CNT;
			public PointF[] EDG_LFT = new PointF[16];
			public PointF[] EDG_RGT = new PointF[16];
			//---
			public int WIDTH;
			public int HEIGHT;
			//---
			public Object Clone()
			{
				SYSSET cln = (SYSSET)this.MemberwiseClone();
				return (cln);
			}
			public void clear()
			{
				for (int i = 0; i < this.HISTVALY.Length; i++) {
					this.HISTVALY[i] = 0;
				}
				this.HIST_MIN = double.NaN;
				this.HIST_MAX = double.NaN;
				this.HIST_AVG = double.NaN;
				this.CONTRAST = double.NaN;
				//---
				this.CIR_CNT = 0;
				this.CIR_S = double.NaN;
				this.CIR_L = double.NaN;
				this.CIR_C = double.NaN;
				this.CIR_P = double.NaN;
				this.CIR_U = double.NaN;
				this.CIR_RT = new Rectangle(0, 0, 0, 0);
				//---
				this.DIA_CNT = 0;
				//---
				this.EDG_CNT = 0;
			}
		}
		public enum PLM_STS_BITS
		{
			BIT_ONMOV = 0x0001,	//移動中
			BIT______ = 0x0002,	//
			BIT_LMT_M = 0x0004,	//現在LIMIT.MがON(SOFT.LIMIT.CCW側)
			BIT_END_N = 0x0008,	//正常動作にて終了
			BIT_END_A = 0x0010,	//リミット or STOP.REQ or原点未検出にて終了
			BIT_LMT_H = 0x0020,	//現在LIMIT.HがON
			BIT_LMT_P = 0x0040,	//現在LIMIT.PがON(SOFT.LIMIT.CW側)
			BIT_ORGOK = 0x0080,	//ORG検出実行済み
			//	BIT_START = 0x0080	//マイコン側処理用
			BIT_ACCEL = 0x0100,	//加速中
			BIT_SLOWS = 0x0200,	//減速中
		};
		public enum CAM_STS
		{
			STS_NONE = 0,
			STS_HIST = 1,
			STS_HAIR = 2,
			STS_FCUS = 4,
			STS_AUTO = 5,
			STS_ATIR = 6,
			STS_CUTI = 7,
		};
		//---	
		static public APPSET AS = new APPSET();
		static public SYSSET SS = new SYSSET();
		static public IP_RESULT	IR = new IP_RESULT();
		//---	
		//static public bool		bDEBUG=true;
		//static public bool		bONLINE=false;
		//static public bool		bONLINE_OF_NI=false;
		static public int		UIF_LEVL=0;
		static public bool		bCANCEL=false;
		static public Form01	FORM01 = null;
		static public Form02	FORM02 = null;
		static public Form03	FORM03 = null;
		static public Form10	FORM10 = null;
		static public Form11	FORM11 = null;
		static public Form12	FORM12 = null;
		static public Form13	FORM13 = null;
#if true//2018.10.10(毛髪径算出・改造)
		static public Form24	FORM24 = null;
#endif
		static public int PLM_STS = 0;
		static public int[] PLM_POS = { 0, 0, 0, 0 };
		static public byte[] PLM_STS_BIT = new byte[16];
		//static public int PLM_JOG = -1;
		static public int[] PLM_BSL = {0,0,0,0};
		//static public int CAM_STS;
		static public CAM_STS CAM_PRC = CAM_STS.STS_NONE;

		//static public int AUT_STS;
		//static public int MOK_STS;
		static public int CAM_WID;
		static public int CAM_HEI;
		static public int LED_PWR_STS;
		static public int STG_TRQ_STS = 0;//ビットON:TRQ-HI, OFF:TRQ-LO
		static public int CAM_GAI_STS=2;//0:固定, 1:自動, 2:不定
		static public int CAM_EXP_STS=2;//0:固定, 1:自動, 2:不定
		static public int CAM_WBL_STS=2;//0:固定, 1:自動, 2:不定
		static public bool bJITAN=false;
		static public int CNT_MOD;
		//-----------------------
		static public DialogResult mlog(string str)
		{
			return(mlog(str, Form.ActiveForm));
		}
		//-----------------------
		static public DialogResult mlog(string str, Form frm)
        {
			MessageBoxIcon	icons = MessageBoxIcon.Exclamation;
			MessageBoxButtons
							butns = MessageBoxButtons.OK;
			DialogResult	rc;
			/**/
			 
			if (frm == null) {
				frm = G.FORM01;
			}
			/**/
			if (str.Length > 0 && str[0]== '#') {
				switch (char.ToLower(str[1])) {
				case 's':
					icons = MessageBoxIcon.Stop;
				break;
				case 'q':
					icons = MessageBoxIcon.Question;
					if (str[1] == 'q') {// small q
						butns = MessageBoxButtons.YesNo;
					}
					else {				// large Q
						butns = MessageBoxButtons.OKCancel;
					}
				break;
				case 'c':
					icons = MessageBoxIcon.Question;
					butns = MessageBoxButtons.YesNoCancel;
				break;
				case 'i':
					icons = MessageBoxIcon.Information;
				break;
				case 'e':
					icons = MessageBoxIcon.Exclamation;
				break;
				default:
				break;
				}
				str = str.Substring(2);
			}
			using (new CenterWinDialog(frm)) {
				rc = MessageBox.Show(G.FORM01, str, Application.ProductName, butns, icons);
			}
			return(rc);
        }
		static public void lerr(string str)
		{
			mlog("internal error %s %d");//(, __FILE__, __LINE__);
		}
		static public string filter_string()
		{
			string buf = "";
			buf += "bmp (*.bmp)|*.bmp|";
			buf += "png (*.png)|*.png|";
			buf += "jpg (*.jpg)|*.jpg|";
			buf += "All files (*.*)|*.*";
			return (buf);
		}
		static public int to_fidx(string ext)
		{
			int idx = 0;
			if (false) {
			}
			else if (ext.Contains("bmp")) {
				idx = 1;//1 start
			}
			else if (ext.Contains("png")) {
				idx = 2;
			}
			else if (ext.Contains("jpg") || ext.Contains("jpeg")) {
				idx = 3;
			}
			return (idx);
		}
		static public string GET_DOC_PATH(string file)
		{
			string path;
			path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			path += @"\KOP";
			if (!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			}
			path += @"\" + Application.ProductName;
			if (!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			}
			if (!string.IsNullOrEmpty(file)) {
				if (file[0] != '\\') {
					path += "\\";
				}
				path += file;
			}

			return (path);
		}
		// filename:setting.xml
		static public bool COPY_SETTINGS(string filename)
		{
			string path;
			// path:C:\\Users\\araya320\\AppData\\Roaming
			path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path += @"\KOP";
			if (!System.IO.Directory.Exists(path)) {
				return(false);
			}
			path += @"\" + Application.ProductName;
			if (!System.IO.Directory.Exists(path)) {
				return(false);
			}
			path += @"\";
			path += filename;
			if (!System.IO.File.Exists(path)) {
				return(false);
			}
			try {
				string path_dst = GET_DOC_PATH(filename);
				if (System.IO.File.Exists(path_dst)) {
					//backupを作成
					DateTime dt = System.IO.File.GetLastWriteTime(path_dst);
					string file_base = System.IO.Path.GetFileNameWithoutExtension(filename);
					string file_ext = System.IO.Path.GetExtension(filename);
					string path_bak = file_base;
					path_bak += string.Format("-{0:D04}{1:D02}{2:D02}", dt.Year, dt.Month, dt.Day);
					path_bak += string.Format("-{0:D02}h{1:D02}m{2:D02}s", dt.Hour, dt.Minute, dt.Second);
					path_bak += file_ext;
					path_bak = GET_DOC_PATH(path_bak);
					System.IO.File.Copy(path_dst, path_bak, true);
				}
				System.IO.File.Copy(path, path_dst, true);
				System.IO.File.Delete(path);
			}
			catch (Exception ex) {
				return(false);
			}
			return(true);
		}
		static public double PX2UM(double px)
		{
			double	zoom = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
			double	um = PX2UM(px, G.SS.CAM_SPE_UMPPX, zoom);
			return (um);
		}
		static public double PX2UM(double px, double umppx, double zoom)
		{
			double um = px * umppx/zoom;	//[um]=[px]*[um/px]
			return (um);
		}
		static public double UM2PX(double um)
		{
			double	zoom = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
		    double	px = UM2PX(um, G.SS.CAM_SPE_UMPPX, zoom);
		    return (px);
		}
		static public double UM2PX(double um, double umppx, double zoom)
		{
			double	px = um / (umppx/zoom);	//[px] = [um]/[um/px]
			return (px);
		}
		static public double diff(PointF p1, PointF p2)
		{
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			double df = Math.Sqrt(dx*dx + dy*dy);
			return(df);
		}
		static public double diff(Point p1, Point p2)
		{
			int dx = p2.X - p1.X;
			int dy = p2.Y - p1.Y;
			double df = Math.Sqrt(dx*dx + dy*dy);
			return(df);
		}
		static public double diff(int x1, int y1, int x2, int y2)
		{
			int dx = x2 - x1;
			int dy = y2 - y1;
			double df = Math.Sqrt(dx*dx + dy*dy);
			return(df);
		}
		private static bool
			TMP_IMP_PUSHED = false;
		private static
		int TMP_FLT_COEF,
			TMP_BIN_MODE,
			TMP_BIN_BVAL,
			TMP_HUE_LOWR,
			TMP_HUE_UPPR,
			TMP_SAT_LOWR,
			TMP_SAT_UPPR,
			TMP_VAL_LOWR,
			TMP_VAL_UPPR;
		private static
		int TMP_SUM_LOWR,
			TMP_SUM_UPPR,
			TMP_LEN_LOWR,
			TMP_LEN_UPPR;
		private static double
			TMP_CIR_LOWR,
			TMP_CIR_UPPR,
			TMP_OPT_MAGN;
		private static
		int TMP_POL_PREC;

		static public void push_imp_para()
		{
			TMP_FLT_COEF = G.SS.CAM_CIR_FILT;
			TMP_BIN_MODE = G.SS.CAM_CND_MODH;
			TMP_BIN_BVAL = G.SS.CAM_HIS_BVAL;
			TMP_HUE_LOWR = G.SS.CAM_CND_MINH;
			TMP_HUE_UPPR = G.SS.CAM_CND_MAXH;
			TMP_SAT_LOWR = G.SS.CAM_CND_MINS;
			TMP_SAT_UPPR = G.SS.CAM_CND_MAXS;
			TMP_VAL_LOWR = G.SS.CAM_CND_MINV;
			TMP_VAL_UPPR = G.SS.CAM_CND_MAXV;
			//---
			TMP_SUM_LOWR = G.SS.CAM_CIR_AREA    ;
			TMP_SUM_UPPR = G.SS.CAM_CIR_AREA_MAX;
			TMP_LEN_LOWR = G.SS.CAM_CIR_LENG    ;
			TMP_LEN_UPPR = G.SS.CAM_CIR_LENG_MAX;
			TMP_CIR_LOWR = G.SS.CAM_CIR_CVAL_MIN;
			TMP_CIR_UPPR = G.SS.CAM_CIR_CVAL    ;
			//---
			TMP_OPT_MAGN = G.SS.CAM_CIR_MAGN;
			TMP_POL_PREC = G.SS.CAM_DIR_PREC;
			//---
			TMP_IMP_PUSHED = true;
		}
		static public void pop_imp_para()
		{
			if (!TMP_IMP_PUSHED) {
				G.mlog("over pop!!!");
			}
			G.SS.CAM_CIR_FILT     = TMP_FLT_COEF;
			G.SS.CAM_CND_MODH     = TMP_BIN_MODE;
			G.SS.CAM_HIS_BVAL     = TMP_BIN_BVAL;
			G.SS.CAM_CND_MINH     = TMP_HUE_LOWR;
			G.SS.CAM_CND_MAXH     = TMP_HUE_UPPR;
			G.SS.CAM_CND_MINS     = TMP_SAT_LOWR;
			G.SS.CAM_CND_MAXS     = TMP_SAT_UPPR;
			G.SS.CAM_CND_MINV     = TMP_VAL_LOWR;
			G.SS.CAM_CND_MAXV     = TMP_VAL_UPPR;
			//---
			G.SS.CAM_CIR_AREA     = TMP_SUM_LOWR;
			G.SS.CAM_CIR_AREA_MAX = TMP_SUM_UPPR;
			G.SS.CAM_CIR_LENG     = TMP_LEN_LOWR;
			G.SS.CAM_CIR_LENG_MAX = TMP_LEN_UPPR;
			G.SS.CAM_CIR_CVAL_MIN = TMP_CIR_LOWR;
			G.SS.CAM_CIR_CVAL     = TMP_CIR_UPPR;
			//---
			G.SS.CAM_CIR_MAGN     = TMP_OPT_MAGN;
			G.SS.CAM_DIR_PREC     = TMP_POL_PREC;
			//---
			TMP_IMP_PUSHED = false;
		}
		//ch=0:白色LED用(透過), ch=1:白色LED用(反射), ch=2:赤外LED用
		static public void set_imp_param(int i, int mask)
		{
			if ((mask & 2) != 0) {
			G.SS.CAM_CIR_FILT = G.SS.IMP_FLT_COEF[i];
			G.SS.CAM_CND_MODH = G.SS.IMP_BIN_MODE[i];
			G.SS.CAM_HIS_BVAL = G.SS.IMP_BIN_BVAL[i];//, 1, 254);
			G.SS.CAM_CND_MINH = G.SS.IMP_HUE_LOWR[i];
			G.SS.CAM_CND_MAXH = G.SS.IMP_HUE_UPPR[i];
			G.SS.CAM_CND_MINS = G.SS.IMP_SAT_LOWR[i];
			G.SS.CAM_CND_MAXS = G.SS.IMP_SAT_UPPR[i];
			G.SS.CAM_CND_MINV = G.SS.IMP_VAL_LOWR[i];
			G.SS.CAM_CND_MAXV = G.SS.IMP_VAL_UPPR[i];
			}
			//---
			if ((mask & 1) != 0) {
			G.SS.CAM_CIR_AREA     = G.SS.IMP_SUM_LOWR[i];//, 1000, 2500000);
			G.SS.CAM_CIR_AREA_MAX = G.SS.IMP_SUM_UPPR[i];//, 1000, 2500000);
			G.SS.CAM_CIR_LENG     = G.SS.IMP_LEN_LOWR[i];//, 1, 10000);
			G.SS.CAM_CIR_LENG_MAX = G.SS.IMP_LEN_UPPR[i];//, 1, 100000);
			G.SS.CAM_CIR_CVAL_MIN = G.SS.IMP_CIR_LOWR[i];//, 0.0, 1);
			G.SS.CAM_CIR_CVAL     = G.SS.IMP_CIR_UPPR[i];//, 0.0, 1);
			//---
			G.SS.CAM_CIR_MAGN = G.SS.IMP_OPT_MAGN[i];
			G.SS.CAM_DIR_PREC = G.SS.IMP_POL_PREC[i];//, 5, 100);
			}
		}
	}
}
