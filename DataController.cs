using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace GhostFlareChecker
{
	public class DataController
	{
		public TriangleInfo[] triInfo;
		public CircleInfo[] circleInfo;
		public SumInfo sumInfo;
		public LogInfo logInfo;
		public AF_INFO[] af_info;
		public LedTable[] ledtable;
		public static int gPos = 0xFF;
		public static int colorIndex = 1;

		static public SYSSET SETDATA = new SYSSET();
		
		public class SYSSET:System.ICloneable
		{
			//Image�֘A start
			public int sMin;
			public int sMax;
			public int lMin;
			public int lMax;
			public double cMin;
			public double cMax;
			public int stepCount;
			public int distanceThresh;
			public int center_x_Thresh;
			public int center_y_Thresh;
			public int center_x_hosei;
			public int center_y_hosei;
			public int FstThreshMin;
			public int FstThreshMax;
			public int FstStepCount;
			public int SndThreshMin;
			public int SndThreshMax;
			public int SndStepCount;
			public int TrdThreshMin;
			public int TrdThreshMax;
			public int TrdStepCount;
			
			public int center_x_zure;
			public int center_y_zure;

			public int[] led_x = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0};
			public int[] led_y = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
								  0, 0, 0};
			public int[] led_area_limit = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
										  0, 0, 0};
			public int lineFlareThresh;

			public int circleArea;


			//Image�֘A end

			//Camera�֘A start
            public int waitTime;
            public bool isMirrorV;
            public bool isMirrorH;
            public int brightness;
            public int contrast;
            public int hue;
            public int saturation;

            public int sharpness;
            public int bayerGainRed;
            public int bayerGainGreen;
            public int bayerGainBlue;
			public bool auto;
            public int gamma;

            public int globalGain;
            public int gainRed;
            public int gainGreen1;
            public int gainGreen2;
            public int colorGainBlue;
            public int exposureTime;
            public int halfClock;

            public int MonitorColorMode;
            public int cameraType;
            public int width;
            public int height;
            public int Fps;
            public long dllVersion;
            public int cameraInfo;
            public int cameraStatus;

			public int af_left;//AF���̋�`�F���[X���W
			public int af_top;//AF���̋�`�F���[Y���W
			public int af_right;//AF���̋�`�F�EX���W
			public int af_buttom;//AF���̋�`�F�EY���W

			public int AfThreshMin;
			public int AfThreshMax;
			//Camera�֘A end


			//Motor�֘A start
			public int[] Unit = { 0, 0, 0, 0 };
			public int[] Axis = { 0, 0, 0, 0 };
			public int[] Origin = { 0, 0, 0, 0 };
			public int[] ORG_drv = { 0, 0, 0, 0 };
			public int[] Jspd = { 0, 0, 0, 0 };
			public int[] Hspd = { 0, 0, 0, 0 };
			public int[] Lspd = { 0, 0, 0, 0 };
			public int[] StdPLS = { 0, 0, 0, 0 };
			public int[] L_LimitPLS = { 0, 0, 0, 0 };
			public int[] U_LimitPLS = { 0, 0, 0, 0 };

			public string MasterSerial;
			public int MasDepth;
			public int AreaDepth;
			public double MasMTF1;
			public double AreaMTF1;
			public double MasMTF2;
			public double AreaMTF2;
			public double MasMTF3;
			public double AreaMTF3;
			public double MasFOV;
			public double AreaFOV;
			public double MasTilt;
			public double AreaTilt;
			public int MasDrct;
			public int AreaDrct;
			public int MasCof;
			public int AreaCof;
			public int MasHens;
			public int AreaHens;
			public int MasHenV;
			public int AreaHenV;
			public int MasHenH;
			public int AreaHenH;
			public int MasFcs;
			public int AreaFcs;

			public double[][] MTFHosei;
			public double[] sp_MTFCent2 = { 0, 0, 0};
			public int sp_DataMatrix;
			public int sp_Depth2;
            public double MasterLastCheckF;
            public string MasterLastCheckS;
            public string model;
            public int gouki;
            public int[] B_OffSet = { 0, 0, 0, 0 };
            public int[] F_OffSet = { 0, 0, 0, 0 };

			public int limit_1;
			public int limit_2;
			public int limit_3;
			public int limit_4;
			public int limit_5;
			public int limit_6;
			public int limit_7;
			public int step_1_2;
			public int step_2_3;
			public int step_3_4;
			public int step_4_5;
			public int step_5_6;
			public int step_6_7;
			public int defaultStep;

			//Motor�֘A end

            public bool load(ref SYSSET ss)
			{
				string path = GET_DOC_PATH("SettingData.xml");
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

			public Object Clone()
			{
				SYSSET cln = (SYSSET)this.MemberwiseClone();
				return (cln);
			}

			public bool save(SYSSET ss)
			{
				string path = GET_DOC_PATH("SettingData.xml");
				bool ret = false;
				try {
					XmlSerializer sz = new XmlSerializer(typeof(SYSSET));
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
		
		static private void ReadDataFromXml()
		{
            SETDATA.load(ref DataController.SETDATA);
		}

		public DataController()
		{
		}

		public void Init()
		{
			ReadDataFromXml();
			InitTriangleInfo();
			InitCircleInfo();
			InitAfInfo();
			InitLedTable();
			CSV_HEADER_WRITE();
		}

		public void Release()
		{
			WriteDataToXml();
		}


		public string GetLedCSVFileName(string str, int ledPos, string lenzeNo)
		{
			//���t�A���Ԃ��擾
			DateTime dt = DateTime.Now;
			string pathstr = string.Format("{0:D04}{1:D02}{2:D02}{3:D02}{4:D02}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
			string pathdst = GET_DOC_PATH(pathstr);

			//S/N���w�b�_�ɕt����
			string lenzID = GetLenzeID(lenzeNo);
			pathdst = pathdst + "_" + lenzID;

			if(!Directory.Exists(pathdst))
			{
				Directory.CreateDirectory(pathdst);
			}

            string pathAndName = pathdst + "\\" + pathstr + "_" + lenzID + "_" + ledPos.ToString() + str;
            return pathAndName;
		}

		public string GetLenzeID(string lenzeNo)
		{
			string	lenzeStr = "NO_NUMBER";
			if(lenzeNo == "")
			{
				return lenzeStr;
			}

			return lenzeNo;
		}

		public string GetCSVFileName(string str)
		{
			//���t���擾
			DateTime dt = DateTime.Now;
			string pathstr = string.Format("{0:D04}{1:D02}{2:D02}", dt.Year, dt.Month, dt.Day);
			string pathdst = GET_DOC_PATH(pathstr);

			if(!Directory.Exists(pathdst))
			{
				Directory.CreateDirectory(pathdst);
			}

            string pathAndName = pathdst + "\\" + pathstr + str;
            return pathAndName;
		}

		public string GetImageFileName(int sign)
		{
			//���t���擾
			DateTime dt = DateTime.Now;

			string pathstr = string.Format("{0:D04}{1:D02}{2:D02}", dt.Year, dt.Month, dt.Day);
			string pathdst = GET_DOC_PATH(pathstr);

			if(!Directory.Exists(pathdst))
			{
				Directory.CreateDirectory(pathdst);
			}

			string signStr = null;
			if(sign == 0)
			{
				signStr = "_ETC";
			}else if(sign == 1)
			{
				signStr = "_RAW";
			}else if(sign == 2)
			{
				signStr = "_ANA";
			}else if(sign == 3)
			{
				signStr = "_CHART_CENTER";
			}else if(sign == 4)
			{
				signStr = "_CHART_RADIAL";
			}
			pathdst += "\\" + signStr;
			if(!Directory.Exists(pathdst))
			{
				Directory.CreateDirectory(pathdst);
			}

			pathstr += "_";
			pathstr += string.Format("{0:D02}{1:D02}{2:D02}{3:D03}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
			pathstr += signStr;
			pathstr += ".png";

            string pathAndName = pathdst + "\\" + pathstr;
            return pathAndName;
		}

		static public void WriteDataToXml()
		{
            SETDATA.save(DataController.SETDATA);
		}

		
		public bool CopySettings(string filename)
		{
			string path;
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
					//backup���쐬
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
                ex.GetType();
				return(false);
			}
			return(true);
		}
		
		public struct TriangleInfo
		{
			public int	x;			//X���W
			public int	y;			//Y���W
			public int	yokosen;	//�����F���
			public int	tatesen;	//�c��
			public int	shasen;		//�ΐ�
			public int	theta;		//�ΐ��ƒ�ӂɋ��܂ꂽ�p�x
		}

		public struct CircleInfo
		{
			public double	s;		//�ʐ�
			public double	l;		//���͒�
			public double	c;		//�~�`�x
			public int	x;		//�~�̒��SX���W
			public int	y;		//�~�̒��SY���W
			public int rgb;		//�`��F
		}

		public struct SumInfo//�J�����P��̎B�e���Ƃ̃f�[�^
		{
			public int n;				//����
            public int sum_x;			//X���W�̍��v
			public int sum_y;			//Y���W�̍��v
			public int sum_xv;			//X���W�̓��̍��v
			public int sum_yv;			//Y���W�̓��̍��v
			public double mean_x;		//X���W�̕���
			public double variance_x;	//X���W�̕��U
			public double stddev_x;	//X���W�̕W���΍�
			public double mean_y;		//Y���W�̕���
			public double variance_y;	//Y���W�̕��U
			public double stddev_y;	//Y���W�̕W���΍�
			public double enkeido;	//�~�`�x(�ŊO�`)
			public double shuicho;	//���͒�(�ŊO�`)
			public double menseki;	//�ʐ�(�ŊO�`)

			public int fst_most_outside_x;			//�ŊO�`��1�Ԗڂ̓���X���W
			public int fst_most_outside_y;			//�ŊO�`��1�Ԗڂ̓���Y���W
			public double fst_most_outside_menseki;	//�ŊO�`��1�Ԗڂ̓��̖ʐ�
			public double fst_most_outside_shuicho;	//�ŊO�`��1�Ԗڂ̓��̎��͒�
			public double fst_most_outside_enkeido;	//�ŊO�`��1�Ԗڂ̓��̉~�`�x

			public int snd_most_outside_x;			//�ŊO�`��2�Ԗڂ̓���X���W
			public int snd_most_outside_y;			//�ŊO�`��2�Ԗڂ̓���Y���W
			public double snd_most_outside_menseki;	//�ŊO�`��2�Ԗڂ̓��̖ʐ�
			public double snd_most_outside_shuicho;	//�ŊO�`��2�ڂ̓��̎��͒�
			public double snd_most_outside_enkeido;	//�ŊO�`��2�Ԗڂ̓��̉~�`�x

			public int trd_most_outside_x;			//�ŊO�`��3�Ԗڂ̓���X���W
			public int trd_most_outside_y;			//�ŊO�`��3�Ԗڂ̓���Y���W
			public double trd_most_outside_menseki; //�ŊO�`��3�Ԗڂ̓��̖ʐ�
			public double trd_most_outside_shuicho; //�ŊO�`��3�ڂ̓��̎��͒�
			public double trd_most_outside_enkeido; //�ŊO�`��3�Ԗڂ̓��̉~�`�x

			public double sum_most_outside_menseki; //�ŊO�`�̖ʐύ��v

			public int fst_most_inside_x;			//1������1�Ԗڂ̓���X���W
			public int fst_most_inside_y;			//1������1�Ԗڂ̓���Y���W
			public double fst_most_inside_menseki;	//1������1�Ԗڂ̓��̖ʐ�
			public double fst_most_inside_shuicho;	//1������1�Ԗڂ̓��̎��͒�
			public double fst_most_inside_enkeido;	//1������1�Ԗڂ̓��̉~�`�x

			public int snd_most_inside_x;			//1������2�Ԗڂ̓���X���W
			public int snd_most_inside_y;			//1������2�Ԗڂ̓���Y���W
			public double snd_most_inside_menseki;	//1������2�Ԗڂ̓��̖ʐ�
			public double snd_most_inside_shuicho;	//1������2�ڂ̓��̎��͒�
			public double snd_most_inside_enkeido;	//1������2�Ԗڂ̓��̉~�`�x

			public int trd_most_inside_x;			//1������3�Ԗڂ̓���X���W
			public int trd_most_inside_y;			//1������3�Ԗڂ̓���Y���W
			public double trd_most_inside_menseki; //1������3�Ԗڂ̓��̖ʐ�
			public double trd_most_inside_shuicho; //1������3�ڂ̓��̎��͒�
			public double trd_most_inside_enkeido; //1������3�Ԗڂ̓��̉~�`�x

			public double sum_most_inside_menseki; //1�����̖ʐύ��v

			public int numberOfoutside;				//�ŊO�`�̓��̐�
			public int numberOfinside;				//1�����̓��̐�

			public int max_brightness_center_x;//�ő�P�x���狁�߂�X���W
			public int max_brightness_center_y;//�ő�P�x���狁�߂�X���W
			public int ledPosition;				//�_������LED�̈ʒu
			public int maxBrightnessCount;		//�ő�P�x�̐�
			
			public int maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
			public int minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����
		}

		public struct LogInfo
		{
			public int FstThreshMin;//(�O��)2�l����臒lMIN
			public int FstThreshMax;//(�O��)2�l����臒lMAX
			public int FstStepCount;//(�O��)���ݕ�
			public int SndThreshMin;//(����)2�l����臒lMIN
			public int SndThreshMax;//(����)2�l����臒lMAX
			public int SndStepCount;//(����)���ݕ�
			public int TrdThreshMin;//(�㔼)2�l����臒lMIN
			public int TrdThreshMax;//(�㔼)2�l����臒lMAX
			public int TrdStepCount;//(�㔼)���ݕ�
			public string z_address;	//Z���̃A�h���X
			public string hankei_kakudoFile;//���a-�p�x�O���t�̃t�@�C����
			public string chu_shintenFile;//���S�_���W�O���t�̃t�@�C����
			public string rawFileName;//���摜�̃t�@�C����
			public string anaFileName;//��͉摜�̃t�@�C����
			public string ghost_or_not;//GHOST�̔���
			public int flare_hankei_avarage;//FLARE�������a�̕��ϒl
			public int flare_thresh;//FLARE��臒l
			public string flare_or_not;//FLARE�̔���
			public double UpperMenseki;//���o����ʐς̏��
			public string lenzeNumber;//�����Y�̃V���A���i���o�[
		}

		public struct AF_INFO
		{
			public int step;
			public int address;
			public int pulse;
			public int whiteCount;
			public double contrast;
			public double menseki;
			public double shuicho;
			public double enkeido;
		}

		//63��LED���̏����A�\���̔z��Ŋi�[
		public struct LedTable
		{
			public int	center_x;			//��ʂ���ݒ肵��X���W
			public int	center_y;			//��ʂ���ݒ肵��Y���W

			public int	outside_center_x;		//�ŊO�`��X���W(�ŏ��Ɍ��o�����ŊO�`�̂���)�FGHOST���o�p
			public int	outside_center_y;		//�ŊO�`��Y���W(�ŏ��Ɍ��o�����ŊO�`�̂���)�FGHOST���o�p

			public int	max_brightness_center_x;//�ő�P�x���狁�߂�X���W
			public int	max_brightness_center_y;//�ő�P�x���狁�߂�X���W

			public bool isPass;					//����LED������ʉ߂�����
			public int  result;					//OK or NG(Ghost or Flare)

			public int fst_most_outside_x;			//�ŊO�`��1�Ԗڂ̓���X���W
			public int fst_most_outside_y;			//�ŊO�`��1�Ԗڂ̓���Y���W
			public double fst_most_outside_menseki;	//�ŊO�`��1�Ԗڂ̓��̖ʐ�
			public double fst_most_outside_shuicho;	//�ŊO�`��1�Ԗڂ̓��̎��͒�
			public double fst_most_outside_enkeido;	//�ŊO�`��1�Ԗڂ̓��̉~�`�x
			public int snd_most_outside_x;			//�ŊO�`��2�Ԗڂ̓���X���W
			public int snd_most_outside_y;			//�ŊO�`��2�Ԗڂ̓���Y���W
			public double snd_most_outside_menseki;	//�ŊO�`��2�Ԗڂ̓��̖ʐ�
			public double snd_most_outside_shuicho;	//�ŊO�`��2�ڂ̓��̎��͒�
			public double snd_most_outside_enkeido;	//�ŊO�`��2�Ԗڂ̓��̉~�`�x
			public int trd_most_outside_x;			//�ŊO�`��3�Ԗڂ̓���X���W
			public int trd_most_outside_y;			//�ŊO�`��3�Ԗڂ̓���Y���W
			public double trd_most_outside_menseki; //�ŊO�`��3�Ԗڂ̓��̖ʐ�
			public double trd_most_outside_shuicho; //�ŊO�`��3�ڂ̓��̎��͒�
			public double trd_most_outside_enkeido; //�ŊO�`��3�Ԗڂ̓��̉~�`�x

			public double sum_most_outside_menseki; //�ŊO�`�̖ʐύ��v
			public int numberOfoutside;				//�ŊO�`�̓��̐�

			public int maxBrightnessCount;			//�ő�P�x�̐�

			public int fst_most_inside_x;			//1������1�Ԗڂ̓���X���W
			public int fst_most_inside_y;			//1������1�Ԗڂ̓���Y���W
			public double fst_most_inside_menseki;	//1������1�Ԗڂ̓��̖ʐ�
			public double fst_most_inside_shuicho;	//1������1�Ԗڂ̓��̎��͒�
			public double fst_most_inside_enkeido;	//1������1�Ԗڂ̓��̉~�`�x
			public int snd_most_inside_x;			//1������2�Ԗڂ̓���X���W
			public int snd_most_inside_y;			//1������2�Ԗڂ̓���Y���W
			public double snd_most_inside_menseki;	//1������2�Ԗڂ̓��̖ʐ�
			public double snd_most_inside_shuicho;	//1������2�ڂ̓��̎��͒�
			public double snd_most_inside_enkeido;	//1������2�Ԗڂ̓��̉~�`�x
			public int trd_most_inside_x;			//1������3�Ԗڂ̓���X���W
			public int trd_most_inside_y;			//1������3�Ԗڂ̓���Y���W
			public double trd_most_inside_menseki; //1������3�Ԗڂ̓��̖ʐ�
			public double trd_most_inside_shuicho; //1������3�ڂ̓��̎��͒�
			public double trd_most_inside_enkeido; //1������3�Ԗڂ̓��̉~�`�x

			public double sum_most_inside_menseki; //1�����̖ʐύ��v
			public int numberOfinside;				//1�����̓��̐�

			public int maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
			public int minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����
		}

		public void InitTriangleInfo()
		{
			triInfo = new TriangleInfo[2048];
		}

		public void InitCircleInfo()
		{
            circleInfo = new CircleInfo[2048];
		}

		public void InitAfInfo()
		{
			af_info = new AF_INFO[1024];
		}

		public void InitLedTable()
		{
			ledtable = new LedTable[63];//63:LED�S��

            for (int i = 0; i < ledtable.Length; i++)
            {
				ledtable[i].center_x = SETDATA.led_x[i];
				ledtable[i].center_y = SETDATA.led_y[i];
			}


		}

		public int MakeDrawColor(int shellColor)
		{
			int rgb = 0;
#if false//�����_���ɐF��ݒ肷��ꍇ
			Random rnd = new System.Random(); // �C���X�^���X�𐶐�
			rgb = rnd.Next(0xFFFFFF);        // RGB�̗������擾
#else//�Œ�F�ɂ���ꍇ
			//��ŕϊ�����̂ŋt�ɂȂ�
			switch(shellColor)
			{
			case 1:
				rgb = 0xFF0000;//��
				break;
			case 2:
				rgb = 0x00FF00;//��
				break;
			case 3:
				rgb = 0x00FFFF;//��
				break;
			case 4:
				rgb = 0xCC00C4;//��
				break;
			case 5:
				rgb = 0x4CB7FF;//�I�����W
				break;
			case 6:
				rgb = 0x7600E5;//�}�[���_
				break;
			case 7:
				rgb = 0xE98743;//�^�[�R�C�Y�u���[
				break;
			case 8:
				rgb = 0x436F88;//�J�[�L
				break;
			case 9:
				rgb = 0x7EEA47;//�G�������h�O���[��
				break;
			default:
				Random rnd = new System.Random(); // �C���X�^���X�𐶐�
				rgb = rnd.Next(0xFFFFFF);        // RGB�̗������擾
				break;
			}
			colorIndex++;
#endif
			rgb.ToString("x4");
			return rgb;
		}

		public void SetCircleColor()
		{
			colorIndex = 1;
		}

		public int GetDrawColor(int i)
		{
			int color = circleInfo[i].rgb;
			
			int r = ((color & 0x0000FF) << 16);
			int g = (color & 0x00FF00);
			int b = ((color & 0xFF0000) >> 16);

			color = r | g | b;
			color.ToString("x4");
			return color;
		}

		public void  SetValueXY(int cnt, int x, int y)
		{
			triInfo[cnt].x = x;
			triInfo[cnt].y = y;
		}

		public void  SetCircleInfo(int i, double s, double l, double c, int x, int y, int rgb)
		{
			circleInfo[i].s = s;
			circleInfo[i].l = l;
			circleInfo[i].c = c;
			circleInfo[i].x = x;
			circleInfo[i].y = y;
			circleInfo[i].rgb = rgb;
		}

		public void InitLogData()
		{
			logInfo.FstThreshMin = 0;
			logInfo.FstThreshMax = 0;
			logInfo.FstStepCount = 0;
			logInfo.SndThreshMin = 0;
			logInfo.SndThreshMax = 0;
			logInfo.SndStepCount = 0;
			logInfo.TrdThreshMin = 0;
			logInfo.TrdThreshMax = 0;
			logInfo.TrdStepCount = 0;
			logInfo.z_address = "";
			logInfo.hankei_kakudoFile = "";
			logInfo.chu_shintenFile = "";
			logInfo.rawFileName = "";
			logInfo.anaFileName = "";
			logInfo.ghost_or_not = "";
			logInfo.flare_hankei_avarage = 0;
			logInfo.flare_thresh = 0;
			logInfo.flare_or_not = "";
			logInfo.UpperMenseki = 0;
			logInfo.lenzeNumber = "";

			sumInfo.n = 0;
			sumInfo.sum_x = 0;
			sumInfo.sum_y = 0;
			sumInfo.sum_xv = 0;
			sumInfo.sum_yv = 0;
			sumInfo.mean_x = 0;
			sumInfo.variance_x = 0;
			sumInfo.stddev_x = 0;
			sumInfo.mean_y = 0;
			sumInfo.variance_y = 0;
			sumInfo.stddev_y = 0;

			sumInfo.numberOfoutside = 0;
			sumInfo.fst_most_outside_x = 0;
			sumInfo.fst_most_outside_y = 0;
			sumInfo.fst_most_outside_menseki = 0;
			sumInfo.fst_most_outside_shuicho = 0;
			sumInfo.fst_most_outside_enkeido = 0;
			sumInfo.snd_most_outside_x = 0;
			sumInfo.snd_most_outside_y = 0;
			sumInfo.snd_most_outside_menseki = 0;
			sumInfo.snd_most_outside_shuicho = 0;
			sumInfo.snd_most_outside_enkeido = 0;

			sumInfo.trd_most_outside_x = 0;
			sumInfo.trd_most_outside_y = 0;			
			sumInfo.trd_most_outside_menseki = 0;
			sumInfo.trd_most_outside_shuicho = 0;
			sumInfo.trd_most_outside_enkeido = 0;

			sumInfo.sum_most_outside_menseki = 0;

			sumInfo.fst_most_inside_x = 0;
			sumInfo.fst_most_inside_y = 0;
			sumInfo.fst_most_inside_menseki = 0;
			sumInfo.fst_most_inside_shuicho = 0;
			sumInfo.fst_most_inside_enkeido = 0;
			sumInfo.snd_most_inside_x = 0;
			sumInfo.snd_most_inside_y = 0;
			sumInfo.snd_most_inside_menseki = 0;
			sumInfo.snd_most_inside_shuicho = 0;
			sumInfo.snd_most_inside_enkeido = 0;
			sumInfo.trd_most_inside_x = 0;
			sumInfo.trd_most_inside_y = 0;
			sumInfo.trd_most_inside_menseki = 0;
			sumInfo.trd_most_inside_shuicho = 0;
			sumInfo.trd_most_inside_enkeido = 0;

			sumInfo.sum_most_inside_menseki = 0;

			sumInfo.numberOfinside = 0;

			sumInfo.max_brightness_center_x = 0;
			sumInfo.max_brightness_center_y = 0;
			sumInfo.ledPosition = 0;
			sumInfo.maxBrightnessCount = 0;

			sumInfo.maxLine = 0;
			sumInfo.minLine = 0;
		}

		public void CSV_HEADER_WRITE()
		{
			string buf = "";
			string csvpath = GetCSVFileName("log.csv");
			StreamWriter wr;
			DateTime dt = DateTime.Now;
			try {
				wr = new StreamWriter(csvpath, true, Encoding.Default);

				buf = string.Format("����");
				buf += string.Format(",�����Y�ԍ�");
				buf += string.Format(",LED�ʒu");
				buf += string.Format(",���E��");
				buf += string.Format(",X���W�̍��v");
				buf += string.Format(",Y���W�̍��v");
				buf += string.Format(",X���W�̓��̍��v");
				buf += string.Format(",Y���W�̓��̍��v");
				buf += string.Format(",X���W�̕���");
				buf += string.Format(",X���W�̕��U");
				buf += string.Format(",X���W�̕W���΍�");
				buf += string.Format(",Y���W�̕���");
				buf += string.Format(",Y���W�̕��U");
				buf += string.Format(",Y���W�̕W���΍�");

				buf += string.Format(",�Ԃ̓���");
				buf += string.Format(",��1�Ԗ�X���W");
				buf += string.Format(",��1�Ԗ�Y���W");
				buf += string.Format(",��1�Ԗږʐ�");
				buf += string.Format(",��1�Ԗڎ��͒�");
				buf += string.Format(",��1�Ԗډ~�`�x");
				buf += string.Format(",��2�Ԗ�X���W");
				buf += string.Format(",��2�Ԗ�Y���W");
				buf += string.Format(",��2�Ԗږʐ�");
				buf += string.Format(",��2�Ԗڎ��͒�");
				buf += string.Format(",��2�Ԗډ~�`�x");
				buf += string.Format(",��3�Ԗ�X���W");
				buf += string.Format(",��3�Ԗ�Y���W");
				buf += string.Format(",��3�Ԗږʐ�");
				buf += string.Format(",��3�Ԗڎ��͒�");
				buf += string.Format(",��3�Ԗډ~�`�x");
				buf += string.Format(",�Ԗʐύ��v");

				buf += string.Format(",�̓���");
				buf += string.Format(",��1�Ԗ�X���W");
				buf += string.Format(",��1�Ԗ�Y���W");
				buf += string.Format(",��1�Ԗږʐ�");
				buf += string.Format(",��1�Ԗڎ��͒�");
				buf += string.Format(",��1�Ԗډ~�`�x");
				buf += string.Format(",��2�Ԗ�X���W");
				buf += string.Format(",��2�Ԗ�Y���W");
				buf += string.Format(",��2�Ԗږʐ�");
				buf += string.Format(",��2�Ԗڎ��͒�");
				buf += string.Format(",��2�Ԗډ~�`�x");
				buf += string.Format(",��3�Ԗ�X���W");
				buf += string.Format(",��3�Ԗ�Y���W");
				buf += string.Format(",��3�Ԗږʐ�");
				buf += string.Format(",��3�Ԗڎ��͒�");
				buf += string.Format(",��3�Ԗډ~�`�x");
				buf += string.Format(",�ʐύ��v");

				buf += string.Format(",�ő�P�x��X���W");
				buf += string.Format(",�ő�P�x��Y���W");
				buf += string.Format(",�ő�P�x��");

				buf += string.Format(",�ő�P�x���S����̍ő勗��");
				buf += string.Format(",�ő�P�x���S����̍ŏ�����");
				buf += string.Format(",�ő勗���\�ŏ�����");
				buf += string.Format(",��������臒l");

				buf += string.Format(",(��)GRAY SCALE��臒lMIN");
				buf += string.Format(",(��)GRAY SCALE��臒lMAX");
				buf += string.Format(",(��)���ݕ�");
				buf += string.Format(",(��)GRAY SCALE��臒lMIN");
				buf += string.Format(",(��)GRAY SCALE��臒lMAX");
				buf += string.Format(",(��)���ݕ�");
				buf += string.Format(",(��)GRAY SCALE��臒lMIN");
				buf += string.Format(",(��)GRAY SCALE��臒lMAX");
				buf += string.Format(",(��)���ݕ�");

				buf += string.Format(",�ʐϏ��");

				buf += string.Format(",���SX���W�̕␳�l");
				buf += string.Format(",���SY���W�̕␳�l");

				buf += string.Format(",Z���̃A�h���X");
				buf += string.Format(",���a-�p�x�O���t�̃t�@�C����");
				buf += string.Format(",���S�_���W�O���t�̃t�@�C����");
				buf += string.Format(",���摜�̃t�@�C����");
				buf += string.Format(",��͉摜�̃t�@�C����");
				buf += string.Format(",GHOST��X���W��臒l");
				buf += string.Format(",GHOST��Y���W��臒l");
				buf += string.Format(",GHOST ?");
				buf += string.Format(",FLARE�������a�̕��ϒl");
				buf += string.Format(",FLARE��臒l");
				buf += string.Format(",FLARE ?");

				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}

		public void CSV_WRITE()
		{
//TODO
			if(sumInfo.ledPosition == 0)
			{
//				return;
			}
			
			string buf = "";
			string csvpath = GetCSVFileName("log.csv");
			StreamWriter wr;
			DateTime dt = DateTime.Now;
			try {
				wr = new StreamWriter(csvpath, true, Encoding.Default);
				buf = string.Format("{0:D02}��{1:D02}��{2:D02}�b{3:D03}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);

				buf += ",";
				buf += logInfo.lenzeNumber;

				buf += string.Format(",{0}", sumInfo.ledPosition);
				buf += string.Format(",{0}", sumInfo.n);
				buf += string.Format(",{0}", sumInfo.sum_x);
				buf += string.Format(",{0}", sumInfo.sum_y);
				buf += string.Format(",{0}", sumInfo.sum_xv);
				buf += string.Format(",{0}", sumInfo.sum_yv);
				buf += string.Format(",{0:F2}", sumInfo.mean_x);
				buf += string.Format(",{0:F2}", sumInfo.variance_x);
				buf += string.Format(",{0:F2}", sumInfo.stddev_x);
				buf += string.Format(",{0:F2}", sumInfo.mean_y);
				buf += string.Format(",{0:F2}", sumInfo.variance_y);
				buf += string.Format(",{0:F2}", sumInfo.stddev_y);

				buf += string.Format(",{0}", sumInfo.numberOfoutside);
				buf += string.Format(",{0}", sumInfo.fst_most_outside_x);
				buf += string.Format(",{0}", sumInfo.fst_most_outside_y);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_outside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_outside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_outside_enkeido);
				buf += string.Format(",{0}", sumInfo.snd_most_outside_x);
				buf += string.Format(",{0}", sumInfo.snd_most_outside_y);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_outside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_outside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_outside_enkeido);
				buf += string.Format(",{0}", sumInfo.trd_most_outside_x);
				buf += string.Format(",{0}", sumInfo.trd_most_outside_y);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_outside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_outside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_outside_enkeido);
				buf += string.Format(",{0:F2}", sumInfo.sum_most_outside_menseki);

				buf += string.Format(",{0}", sumInfo.numberOfinside);
				buf += string.Format(",{0}", sumInfo.fst_most_inside_x);
				buf += string.Format(",{0}", sumInfo.fst_most_inside_y);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_inside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_inside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.fst_most_inside_enkeido);
				buf += string.Format(",{0}", sumInfo.snd_most_inside_x);
				buf += string.Format(",{0}", sumInfo.snd_most_inside_y);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_inside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_inside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.snd_most_inside_enkeido);
				buf += string.Format(",{0}", sumInfo.trd_most_inside_x);
				buf += string.Format(",{0}", sumInfo.trd_most_inside_y);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_inside_menseki);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_inside_shuicho);
				buf += string.Format(",{0:F2}", sumInfo.trd_most_inside_enkeido);
				buf += string.Format(",{0:F2}", sumInfo.sum_most_inside_menseki);

				buf += string.Format(",{0}", sumInfo.max_brightness_center_x);
				buf += string.Format(",{0}", sumInfo.max_brightness_center_y);
				buf += string.Format(",{0}", sumInfo.maxBrightnessCount);

				buf += string.Format(",{0}", sumInfo.maxLine);
				buf += string.Format(",{0}", sumInfo.minLine);
				buf += string.Format(",{0}", (sumInfo.maxLine - sumInfo.minLine));
				buf += string.Format(",{0}", SETDATA.lineFlareThresh);

				buf += string.Format(",{0}", logInfo.FstThreshMin);
				buf += string.Format(",{0}", logInfo.FstThreshMax);
				buf += string.Format(",{0}", logInfo.FstStepCount);
				buf += string.Format(",{0}", logInfo.SndThreshMin);
				buf += string.Format(",{0}", logInfo.SndThreshMax);
				buf += string.Format(",{0}", logInfo.SndStepCount);
				buf += string.Format(",{0}", logInfo.TrdThreshMin);
				buf += string.Format(",{0}", logInfo.TrdThreshMax);
				buf += string.Format(",{0}", logInfo.TrdStepCount);

				buf += string.Format(",{0:F2}", logInfo.UpperMenseki);

				buf += string.Format(",{0}", SETDATA.center_x_hosei);
				buf += string.Format(",{0}", SETDATA.center_y_hosei);

				buf += ",";
				buf += logInfo.z_address;
				buf += ",";
				buf += logInfo.hankei_kakudoFile;
				buf += ",";
				buf += logInfo.chu_shintenFile;
				buf += ",";
				buf += logInfo.rawFileName;
				buf += ",";
				buf += logInfo.anaFileName;

				buf += string.Format(",{0}", SETDATA.center_x_Thresh);
				buf += string.Format(",{0}", SETDATA.center_y_Thresh);

				buf += ",";
				buf += logInfo.ghost_or_not;

				buf += string.Format(",{0}", logInfo.flare_hankei_avarage);
				buf += string.Format(",{0}", SETDATA.distanceThresh);

				buf += ",";
				buf += logInfo.flare_or_not;

				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}

		public void CSV_LED_RESULT_WRITE(string lenzeNo)
		{
			for(int i = 0; i < ledtable.Length; i++)
			{
				string csvpath = GetLedCSVFileName(".csv", (i + 1), lenzeNo);
				string buf = "";
				StreamWriter wr;
				try {
					wr = new StreamWriter(csvpath, true, Encoding.Default);

					buf += string.Format("LED�ԍ�");
					buf += string.Format(",���SX���W�ݒ�l");
					buf += string.Format(",���SY���W�ݒ�l");

					buf += string.Format(",�Ԃ̓���");

					buf += string.Format(",��1�Ԗ�X���W");
					buf += string.Format(",��1�Ԗ�Y���W");
					buf += string.Format(",��1�Ԗږʐ�");
					buf += string.Format(",��1�Ԗڎ��͒�");
					buf += string.Format(",��1�Ԗډ~�`�x");

					buf += string.Format(",��2�Ԗ�X���W");
					buf += string.Format(",��2�Ԗ�Y���W");
					buf += string.Format(",��2�Ԗږʐ�");
					buf += string.Format(",��2�Ԗڎ��͒�");
					buf += string.Format(",��2�Ԗډ~�`�x");

					buf += string.Format(",��3�Ԗ�X���W");
					buf += string.Format(",��3�Ԗ�Y���W");
					buf += string.Format(",��3�Ԗږʐ�");
					buf += string.Format(",��3�Ԗڎ��͒�");
					buf += string.Format(",��3�Ԗډ~�`�x");
					buf += string.Format(",�Ԗʐύ��v");

					buf += string.Format(",�̓���");

					buf += string.Format(",��1�Ԗ�X���W");
					buf += string.Format(",��1�Ԗ�Y���W");
					buf += string.Format(",��1�Ԗږʐ�");
					buf += string.Format(",��1�Ԗڎ��͒�");
					buf += string.Format(",��1�Ԗډ~�`�x");

					buf += string.Format(",��2�Ԗ�X���W");
					buf += string.Format(",��2�Ԗ�Y���W");
					buf += string.Format(",��2�Ԗږʐ�");
					buf += string.Format(",��2�Ԗڎ��͒�");
					buf += string.Format(",��2�Ԗډ~�`�x");

					buf += string.Format(",��3�Ԗ�X���W");
					buf += string.Format(",��3�Ԗ�Y���W");
					buf += string.Format(",��3�Ԗږʐ�");
					buf += string.Format(",��3�Ԗڎ��͒�");
					buf += string.Format(",��3�Ԗډ~�`�x");
					buf += string.Format(",�ʐύ��v");

					buf += string.Format(",�ő�P�x��X���W");
					buf += string.Format(",�ő�P�x��Y���W");
					buf += string.Format(",�ő�P�x��");

					buf += string.Format(",�ő�P�x���S����̍ő勗��");
					buf += string.Format(",�ő�P�x���S����̍ŏ�����");
					buf += string.Format(",�ő勗���\�ŏ�����");

					wr.WriteLine(buf);

					buf = "";
					buf = string.Format("{0}", (i + 1));
					buf += string.Format(",{0}", ledtable[i].center_x);
					buf += string.Format(",{0}", ledtable[i].center_y);

					buf += string.Format(",{0}", ledtable[i].numberOfoutside);

					buf += string.Format(",{0}", ledtable[i].fst_most_outside_x);
					buf += string.Format(",{0}", ledtable[i].fst_most_outside_y);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_outside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_outside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_outside_enkeido);

					buf += string.Format(",{0}", ledtable[i].snd_most_outside_x);
					buf += string.Format(",{0}", ledtable[i].snd_most_outside_y);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_outside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_outside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_outside_enkeido);

					buf += string.Format(",{0}", ledtable[i].trd_most_outside_x);
					buf += string.Format(",{0}", ledtable[i].trd_most_outside_y);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_outside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_outside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_outside_enkeido);
					buf += string.Format(",{0:F2}", ledtable[i].sum_most_outside_menseki);

					buf += string.Format(",{0}", ledtable[i].numberOfinside);

					buf += string.Format(",{0}", ledtable[i].fst_most_inside_x);
					buf += string.Format(",{0}", ledtable[i].fst_most_inside_y);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_inside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_inside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].fst_most_inside_enkeido);

					buf += string.Format(",{0}", ledtable[i].snd_most_inside_x);
					buf += string.Format(",{0}", ledtable[i].snd_most_inside_y);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_inside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_inside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].snd_most_inside_enkeido);

					buf += string.Format(",{0}", ledtable[i].trd_most_inside_x);
					buf += string.Format(",{0}", ledtable[i].trd_most_inside_y);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_inside_menseki);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_inside_shuicho);
					buf += string.Format(",{0:F2}", ledtable[i].trd_most_inside_enkeido);
					buf += string.Format(",{0:F2}", ledtable[i].sum_most_inside_menseki);

					buf += string.Format(",{0}", ledtable[i].max_brightness_center_x);
					buf += string.Format(",{0}", ledtable[i].max_brightness_center_y);
					buf += string.Format(",{0}", ledtable[i].maxBrightnessCount);

					buf += string.Format(",{0}", ledtable[i].maxLine);
					buf += string.Format(",{0}", ledtable[i].minLine);
					buf += string.Format(",{0}", (ledtable[i].maxLine - ledtable[i].minLine));

					wr.WriteLine(buf);

					wr.Close();
				}
				catch (Exception) {
				}
			}
		}


		//����̓_���A�C�ӂ̓_�𒆐S�Ƃ����~�̓����ɂ��邩�B�F���Q�Ԗڂ̊O�`
		public void IsPointInsideArea(int center_x, int center_y, int numOfin, int outer_x, int outer_y)
		{
            // �~�O���t�̕�
            int pie_w = SETDATA.circleArea;
            int pie_h = SETDATA.circleArea;

            int half_pie_w = pie_w / 2;
            int half_pie_h = pie_h / 2;
            double point_val = 0;
            double r2 = 0;
            // �~�O���t��xy���W
            for(int i = 0; i < ledtable.Length; i++)
            {
	            // ���S���W
	            int o_x = ledtable[i].center_x;
	            int o_y = ledtable[i].center_y;

	            // ���a
	            int r = half_pie_w;
	            
	            // �����@(x -a)^2 + (y-b)^2 = r^2
	            point_val = (center_x - o_x) * (center_x - o_x) + (center_y - o_y) * (center_y - o_y);
	            r2 = r * r;

	            // �~�����H
	            if (point_val < r2)
	            {
					sumInfo.numberOfinside = numOfin;

						bool isChange = false;
						if(ledtable[i].maxBrightnessCount <= sumInfo.maxBrightnessCount)//�F�����Ă��Ă��A���̌����ō��P�x�ʐς������̂�����傫���ꍇ�A����������
						{
							isChange = true;
						}
					
						if(numOfin == 1)//GHOST��1�ڂ̓��̏ꍇ
						{
							sumInfo.fst_most_inside_x = outer_x;//���SX���W��ۑ��B
							sumInfo.fst_most_inside_y = outer_y;//���SY���W��ۑ��B

							sumInfo.fst_most_inside_menseki = sumInfo.menseki;
							sumInfo.fst_most_inside_shuicho = sumInfo.shuicho;
							sumInfo.fst_most_inside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_inside_menseki += sumInfo.menseki;//�ʐς̍��v
							if(isChange)
							{
								ledtable[i].numberOfinside = numOfin;
								ledtable[i].sum_most_inside_menseki = sumInfo.sum_most_inside_menseki;

								ledtable[i].fst_most_inside_x = outer_x;//���SX���W��ۑ��B
								ledtable[i].fst_most_inside_y = outer_y;//���SY���W��ۑ��B

								ledtable[i].fst_most_inside_menseki = sumInfo.menseki;
								ledtable[i].fst_most_inside_shuicho = sumInfo.shuicho;
								ledtable[i].fst_most_inside_enkeido = sumInfo.enkeido;

								ledtable[i].snd_most_inside_x = 0;
								ledtable[i].snd_most_inside_y = 0;

								ledtable[i].snd_most_inside_menseki = 0;
								ledtable[i].snd_most_inside_shuicho = 0;
								ledtable[i].snd_most_inside_enkeido = 0;

								ledtable[i].trd_most_inside_x = 0;
								ledtable[i].trd_most_inside_y = 0;

								ledtable[i].trd_most_inside_menseki = 0;
								ledtable[i].trd_most_inside_shuicho = 0;
								ledtable[i].trd_most_inside_enkeido = 0;

							}
						}
						else if(numOfin == 2)
						{
							sumInfo.snd_most_inside_x = outer_x;//���SX���W��ۑ��B
							sumInfo.snd_most_inside_y = outer_y;//���SY���W��ۑ��B

							sumInfo.snd_most_inside_menseki = sumInfo.menseki;
							sumInfo.snd_most_inside_shuicho = sumInfo.shuicho;
							sumInfo.snd_most_inside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_inside_menseki += sumInfo.menseki;//�ʐς̍��v
							if(isChange)
							{
								ledtable[i].numberOfinside = numOfin;
								ledtable[i].sum_most_inside_menseki = sumInfo.sum_most_inside_menseki;

								ledtable[i].snd_most_inside_x = outer_x;//���SX���W��ۑ��B
								ledtable[i].snd_most_inside_y = outer_y;//���SY���W��ۑ��B

								ledtable[i].snd_most_inside_menseki = sumInfo.menseki;
								ledtable[i].snd_most_inside_shuicho = sumInfo.shuicho;
								ledtable[i].snd_most_inside_enkeido = sumInfo.enkeido;
							}
						}
						else if(numOfin == 3)
						{
							sumInfo.trd_most_inside_x = outer_x;//���SX���W��ۑ��B
							sumInfo.trd_most_inside_y = outer_y;//���SY���W��ۑ��B

							sumInfo.trd_most_inside_menseki = sumInfo.menseki;
							sumInfo.trd_most_inside_shuicho = sumInfo.shuicho;
							sumInfo.trd_most_inside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_inside_menseki += sumInfo.menseki;//�ʐς̍��v
							if(isChange)
							{
								ledtable[i].numberOfinside = numOfin;
								ledtable[i].sum_most_inside_menseki = sumInfo.sum_most_inside_menseki;

								ledtable[i].trd_most_inside_x = outer_x;//���SX���W��ۑ��B
								ledtable[i].trd_most_inside_y = outer_y;//���SY���W��ۑ��B

								ledtable[i].trd_most_inside_menseki = sumInfo.menseki;
								ledtable[i].trd_most_inside_shuicho = sumInfo.shuicho;
								ledtable[i].trd_most_inside_enkeido = sumInfo.enkeido;
							}
						}
						else//GHOST�̓���4�ȏ゠��ꍇ
						{
							sumInfo.sum_most_inside_menseki += sumInfo.menseki;//�ʐς̍��v
							if(isChange)
							{
								ledtable[i].numberOfinside = numOfin;
								ledtable[i].sum_most_inside_menseki = sumInfo.sum_most_inside_menseki;
							}
						}
						
						break;
				}
			}
		}
					
					

		//����̓_���A�C�ӂ̓_�𒆐S�Ƃ����~�̓����ɂ��邩�B�ԐF���ŊO�`
		public void IsPointInArea(int center_x, int center_y, bool outside, int numOfout, int most_outer_x, int most_outer_y, int maxLine, int minLine)
		{
            // �~�O���t�̕�
            int pie_w = SETDATA.circleArea;
            int pie_h = SETDATA.circleArea;

            int half_pie_w = pie_w / 2;
            int half_pie_h = pie_h / 2;
            double point_val = 0;
            double r2 = 0;
			gPos = 0xFF;
			bool isChange = false;
            // �~�O���t��xy���W
            for(int i = 0; i < ledtable.Length; i++)
            {
	            // ���S���W
	            int o_x = ledtable[i].center_x;
	            int o_y = ledtable[i].center_y;

	            // ���a
	            int r = half_pie_w;
	            
	            // �����@(x -a)^2 + (y-b)^2 = r^2
	            point_val = (center_x - o_x) * (center_x - o_x) + (center_y - o_y) * (center_y - o_y);
	            r2 = r * r;

	            // �~�����H
	            if (point_val < r2)
	            {
					sumInfo.numberOfoutside = numOfout;
					
					//����LED�������܂��F�����Ă��Ȃ���Βʉ߂Ɣ���
					if(ledtable[i].isPass == false)
					{
						ledtable[i].numberOfoutside = numOfout;
						ledtable[i].isPass = true;//�~��
						gPos = i;
						sumInfo.ledPosition = i + 1;//LED�ʒu��ۑ�

			            ledtable[i].max_brightness_center_x = center_x;//�ő�P�x�̒��S���W��ۑ�
			            ledtable[i].max_brightness_center_y = center_y;//�ő�P�x�̒��S���W��ۑ�
			            sumInfo.max_brightness_center_x = center_x;//�ő�P�x�̒��S���W��ۑ�
			            sumInfo.max_brightness_center_y = center_y;//�ő�P�x�̒��S���W��ۑ�
	
			            ledtable[i].maxBrightnessCount = sumInfo.maxBrightnessCount;//�ő�P�x�̐���ۑ�

			            ledtable[i].outside_center_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
			            ledtable[i].outside_center_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

						sumInfo.fst_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
						sumInfo.fst_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

						sumInfo.fst_most_outside_menseki = sumInfo.menseki;
						sumInfo.fst_most_outside_shuicho = sumInfo.shuicho;
						sumInfo.fst_most_outside_enkeido = sumInfo.enkeido;

						sumInfo.sum_most_outside_menseki += sumInfo.menseki;//�ʐς̍��v
						ledtable[i].sum_most_outside_menseki = sumInfo.sum_most_outside_menseki;

						ledtable[i].fst_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
						ledtable[i].fst_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

						ledtable[i].fst_most_outside_menseki = sumInfo.menseki;
						ledtable[i].fst_most_outside_shuicho = sumInfo.shuicho;
						ledtable[i].fst_most_outside_enkeido = sumInfo.enkeido;

						sumInfo.maxLine = maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
						sumInfo.minLine = minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����
						ledtable[i].maxLine = maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
						ledtable[i].minLine = minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����
			        }
					else if(ledtable[i].isPass == true)
					{
						gPos = i;
						sumInfo.ledPosition = i + 1;//LED�ʒu��ۑ�
			            sumInfo.max_brightness_center_x = center_x;//�ő�P�x�̒��S���W��ۑ�
			            sumInfo.max_brightness_center_y = center_y;//�ő�P�x�̒��S���W��ۑ�
						sumInfo.maxLine = maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
						sumInfo.minLine = minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����

						if(ledtable[i].maxBrightnessCount <= sumInfo.maxBrightnessCount)//�F�����Ă��Ă��A���̌����ō��P�x�ʐς������̂�����傫���ꍇ�A����������
						{
							ledtable[i].numberOfoutside = numOfout;
				            ledtable[i].max_brightness_center_x = center_x;//�ő�P�x�̒��S���W��ۑ�
				            ledtable[i].max_brightness_center_y = center_y;//�ő�P�x�̒��S���W��ۑ�

				            ledtable[i].maxBrightnessCount = sumInfo.maxBrightnessCount;//�ő�P�x�̐���ۑ�

							ledtable[i].maxLine = maxLine;//�ő�P�x���S����O�`�܂ł̍ő勗��
							ledtable[i].minLine = minLine;//�ő�P�x���S����O�`�܂ł̍ŏ�����
							isChange = true;
						}

						if(numOfout == 1)//GHOST��1�ڂ̓��̏ꍇ
						{
							sumInfo.fst_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
							sumInfo.fst_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

							sumInfo.fst_most_outside_menseki = sumInfo.menseki;
							sumInfo.fst_most_outside_shuicho = sumInfo.shuicho;
							sumInfo.fst_most_outside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_outside_menseki += sumInfo.menseki;//�ʐς̍��v

							if(isChange)
							{
								ledtable[i].sum_most_outside_menseki = sumInfo.sum_most_outside_menseki;

								ledtable[i].fst_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
								ledtable[i].fst_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

								ledtable[i].fst_most_outside_menseki = sumInfo.menseki;
								ledtable[i].fst_most_outside_shuicho = sumInfo.shuicho;
								ledtable[i].fst_most_outside_enkeido = sumInfo.enkeido;

								ledtable[i].snd_most_outside_x = 0;
								ledtable[i].snd_most_outside_y = 0;

								ledtable[i].snd_most_outside_menseki = 0;
								ledtable[i].snd_most_outside_shuicho = 0;
								ledtable[i].snd_most_outside_enkeido = 0;

								ledtable[i].trd_most_outside_x = 0;
								ledtable[i].trd_most_outside_y = 0;

								ledtable[i].trd_most_outside_menseki = 0;
								ledtable[i].trd_most_outside_shuicho = 0;
								ledtable[i].trd_most_outside_enkeido = 0;

							}
						}
						else if(numOfout == 2)//GHOST��2�ڂ̓��̏ꍇ
						{
							sumInfo.snd_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
							sumInfo.snd_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

							sumInfo.snd_most_outside_menseki = sumInfo.menseki;
							sumInfo.snd_most_outside_shuicho = sumInfo.shuicho;
							sumInfo.snd_most_outside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_outside_menseki += sumInfo.menseki;//�ʐς̍��v

							if(isChange)
							{
								ledtable[i].sum_most_outside_menseki = sumInfo.sum_most_outside_menseki;

								ledtable[i].snd_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
								ledtable[i].snd_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

								ledtable[i].snd_most_outside_menseki = sumInfo.menseki;
								ledtable[i].snd_most_outside_shuicho = sumInfo.shuicho;
								ledtable[i].snd_most_outside_enkeido = sumInfo.enkeido;
							}
						}
						else if(numOfout == 3)//GHOST��3�ڂ̓��̏ꍇ
						{
							sumInfo.trd_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
							sumInfo.trd_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

							sumInfo.trd_most_outside_menseki = sumInfo.menseki;
							sumInfo.trd_most_outside_shuicho = sumInfo.shuicho;
							sumInfo.trd_most_outside_enkeido = sumInfo.enkeido;

							sumInfo.sum_most_outside_menseki += sumInfo.menseki;//�ʐς̍��v

							if(isChange)
							{
								ledtable[i].sum_most_outside_menseki = sumInfo.sum_most_outside_menseki;

								ledtable[i].trd_most_outside_x = most_outer_x;//���SX���W��ۑ��BGHOST����Ɏg�p
								ledtable[i].trd_most_outside_y = most_outer_y;//���SY���W��ۑ��BGHOST����Ɏg�p

								ledtable[i].trd_most_outside_menseki = sumInfo.menseki;
								ledtable[i].trd_most_outside_shuicho = sumInfo.shuicho;
								ledtable[i].trd_most_outside_enkeido = sumInfo.enkeido;
							}
						}
						else//GHOST�̓���4�ȏ゠��ꍇ
						{
							sumInfo.sum_most_outside_menseki += sumInfo.menseki;//�ʐς̍��v
							if(isChange)
							{
								ledtable[i].sum_most_outside_menseki = sumInfo.sum_most_outside_menseki;
							}
						}
					}


					break;
				}
			}

		}


		//63�S�Ă�LED�����Ƃ��ĔF��������(�������ɓ�������)�B
		public bool IsAllLed()
		{
			bool allLedflag = true;
			for(int i = 0; i < ledtable.Length; i++)
            {
				if(!ledtable[i].isPass)
				{
					allLedflag = false;
					break;
				}
			}
			return allLedflag;
		}

		//63�S�Ă�LED�_����NG��1�ł��܂܂�Ă��邩
		public bool IsGhostFlareCheck()
		{
			bool checkflag = true;
			for(int i = 0; i < ledtable.Length; i++)
            {
				if(ledtable[i].result != 1)//1:OK, 2:Ghost, 3:Flare
				{
					checkflag = false;
					break;
				}
			}
			return checkflag;
		}

		//63�S�Ă�LED�_���ŁA�_�����m�F�ł������AOK�ANG�̐��𐔂��ĕԂ�
		public void GetGhostFlareCount(ref int ok, ref int ng, ref int pass)
		{
			for(int i = 0; i < ledtable.Length; i++)
            {
				//0:�����l, 1:OK, 2:Ghost, 4:Flare, 6:Ghost&Flare
				if(ledtable[i].result == 1)
				{
					ok++;
				}
				else if(ledtable[i].result == 2 || ledtable[i].result == 4 || ledtable[i].result == 6)
				{
					ng++;
				}
				
				if(ledtable[i].isPass)//true:pass
				{
					pass++;
				}
			}
		}

		public void SetLedInit()
		{
			for(int i = 0; i < ledtable.Length; i++)
            {
				ledtable[i].isPass = false;
				ledtable[i].result = 0;
				ledtable[i].outside_center_x = 0;
				ledtable[i].outside_center_y = 0;

				ledtable[i].fst_most_outside_x = 0;
				ledtable[i].fst_most_outside_y = 0;
				ledtable[i].fst_most_outside_menseki = 0;
				ledtable[i].fst_most_outside_shuicho = 0;
				ledtable[i].fst_most_outside_enkeido = 0;

				ledtable[i].snd_most_outside_x = 0;
				ledtable[i].snd_most_outside_y = 0;
				ledtable[i].snd_most_outside_menseki = 0;
				ledtable[i].snd_most_outside_shuicho = 0;
				ledtable[i].snd_most_outside_enkeido = 0;

				ledtable[i].trd_most_outside_x = 0;
				ledtable[i].trd_most_outside_y = 0;
				ledtable[i].trd_most_outside_menseki = 0;
				ledtable[i].trd_most_outside_shuicho = 0;
				ledtable[i].trd_most_outside_enkeido = 0;

				ledtable[i].sum_most_outside_menseki = 0;
				ledtable[i].numberOfoutside = 0;

				ledtable[i].max_brightness_center_x = 0;
				ledtable[i].max_brightness_center_y = 0;
				ledtable[i].maxBrightnessCount = 0;

//
				ledtable[i].fst_most_inside_x = 0;
				ledtable[i].fst_most_inside_y = 0;
				ledtable[i].fst_most_inside_menseki = 0;
				ledtable[i].fst_most_inside_shuicho = 0;
				ledtable[i].fst_most_inside_enkeido = 0;

				ledtable[i].snd_most_inside_x = 0;
				ledtable[i].snd_most_inside_y = 0;
				ledtable[i].snd_most_inside_menseki = 0;
				ledtable[i].snd_most_inside_shuicho = 0;
				ledtable[i].snd_most_inside_enkeido = 0;

				ledtable[i].trd_most_inside_x = 0;
				ledtable[i].trd_most_inside_y = 0;
				ledtable[i].trd_most_inside_menseki = 0;
				ledtable[i].trd_most_inside_shuicho = 0;
				ledtable[i].trd_most_inside_enkeido = 0;

				ledtable[i].sum_most_inside_menseki = 0;
				ledtable[i].numberOfinside = 0;

				ledtable[i].maxLine = 0;
				ledtable[i].minLine = 0;
			}
		}

		public int GetLedPos()
		{
			return gPos;
		}

		public void SetLedPos(int pos, int result)
		{
			if(result == 0x01)
			{
				ledtable[pos].result = result;//OK
			}
			else
			{
				ledtable[pos].result &= 0x06;
				ledtable[pos].result |= result;//GHOST or FLARE
			}
		}

		public int GetLedResult(int pos)
		{
			return ledtable[pos].result;
		}

	}
	
}
