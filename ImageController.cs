using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

using ImageDriver;

namespace GhostFlareChecker
{
    public class ImageController
    {
		static int mergeCount = 0;
		static int CenterCount = 0;
		static string fixFileName = null;
		
		static int smin;
		static int smax;
		static int lmin;
		static int lmax;
		static int cmin;
		static int cmax;
		int[] x;
		double[] y;
		int[] xout;
		double[] yout;
		static int g_max_brightness_x = 0;
		static int g_max_brightness_y = 0;

		public ImageController()
		{
		}

        public void Init()
        {
			x = new int[1024];
			y = new double[1024];
			xout = new int[1024];
			yout = new double[1024];
        }

		public void Release()
		{
			mergeCount = 0;
			CenterCount = 0;
		}

        public void SetImageFile(string imageFile)
        {
			fixFileName = imageFile;
            OpenCV.OCV_TERM_MERGE_IMAGE();
			mergeCount = 0;
			CenterCount = 0;
        }

        public void SetLimitForAnalysis(int argsmin, int argsmax, int arglmin, int arglmax, int argcmin, int argcmax)
        {
			smin = argsmin;
			smax = argsmax;
			lmin = arglmin;
			lmax = arglmax;
			cmin = argcmin;
			cmax = argcmax;
		}

		private void CalcTriangle(int x, int y, int cnt)
		{
			int center_x = x;
			int center_y = y;

			CheckerManager.m_DataController.triInfo[cnt].yokosen = CheckerManager.m_DataController.triInfo[cnt].x - center_x;
			CheckerManager.m_DataController.triInfo[cnt].tatesen = CheckerManager.m_DataController.triInfo[cnt].y - center_y;
			CheckerManager.m_DataController.triInfo[cnt].shasen = (int)Math.Sqrt(Math.Pow(CheckerManager.m_DataController.triInfo[cnt].yokosen, 2) + Math.Pow(CheckerManager.m_DataController.triInfo[cnt].tatesen, 2));

			//���W�A�������߂�
			double rad = Math.Atan2(CheckerManager.m_DataController.triInfo[cnt].tatesen, CheckerManager.m_DataController.triInfo[cnt].yokosen);

			//Radian * �� / Pi �Ŋp�x�����߂�
			CheckerManager.m_DataController.triInfo[cnt].theta = (int)((rad * 180) / Math.PI)*(-1);

		}

		private void GetShasenMaxMin(int total, out int max, out int min)
		{

			int width, height;
			CheckerManager.form3.GetPreviewWindow(out width, out height);//�摜�̉E�[�E���[���W���擾
			width -= 2;//�摜�̍��W����
			height -= 2;//�摜�̍��W����

			max = 0;
			min = 10000;
			for(int i = 0; i < total; i++)
			{
				if(CheckerManager.m_DataController.triInfo[i].x < 2//���[
				|| CheckerManager.m_DataController.triInfo[i].x > width//�E�[
				)
				{
					continue;//�O�`���W���[�ƈ�v���Ă�����̂͏��O����
				}

				if(CheckerManager.m_DataController.triInfo[i].y < 2//��[
				|| CheckerManager.m_DataController.triInfo[i].y > height//���[
				)
				{
					continue;//�O�`���W���[�ƈ�v���Ă�����̂͏��O����
				}

				int mindata, maxdata;
				mindata = maxdata = CheckerManager.m_DataController.triInfo[i].shasen;
				if(min > mindata)
				{
					min = mindata;
				}
				if(max < maxdata)
				{
					max = maxdata;
				}
			}
		}

        public Bitmap ImageAnalysis(int threshMin, int threshMax, Bitmap m_bmpR, out double s, out double l, out double c, out int x, out int y, out double CONTRAST, out int totalContour, bool checkflg)
		{
			Bitmap orgbmp = null;
			if(m_bmpR == null)
			{
				var fs = new System.IO.FileStream(
						fixFileName,
						System.IO.FileMode.Open,
						System.IO.FileAccess.Read);
		        orgbmp = new Bitmap(fs);
				fs.Close();
			}
			
			int width, height;
			CheckerManager.form3.GetPreviewWindow(out width, out height);

			OpenCV.OCV_INITILIZE(width, height, 3);//��3����(3)�F�}�[�W�摜

			if(m_bmpR == null)
			{
				m_bmpR = new Bitmap(width, height);

				Graphics g = Graphics.FromImage(m_bmpR);
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImage(orgbmp, 0, 0, width, height);
				orgbmp.Dispose();
				g.Dispose();
				
				// 32bitRGB -> 24bitRGB
	            Rectangle rtBmp = new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height);
	            Bitmap bmp = m_bmpR.Clone(rtBmp, PixelFormat.Format24bppRgb);
	            m_bmpR.Dispose();
	            m_bmpR = bmp;
			}

            BitmapData bmpData = m_bmpR.LockBits(new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height), ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
            int ret = OpenCV.OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			m_bmpR.UnlockBits(bmpData);

			OpenCV.OCV_GRAY_THRESH_MERGE(threshMin, threshMax);

            int numContour;
            IntPtr contour = OpenCV.OCV_FIND_FIRST(2, out numContour);//��1����(2)�F���摜�ƌ`����}�[�W

			totalContour = numContour;

            IntPtr pos = (IntPtr)0;
            IntPtr retpos = (IntPtr)0;

			CONTRAST = 0;
			s = l = c = x = y = 0;//�R���p�C���G���[����̒u��

			int flg = 0;
			if(checkflg)
			{
				flg = 1;
			}

			int r = 0;
			int hosei_x = 0;
			int hosei_y = 0;
			int hosei_xx = 0;
			int hosei_yy = 0;
			int bSIGNE = 0;
			int[] px = new int[1024];
			int[] py = new int[1024];
			int n = 0;
			int cnt = 0;
			int numOfout = 1;
			int numOfin = 1;
			for(int i = 0; i < numContour; i++)
			{
	            retpos = OpenCV.OCV_FIND_NEXT_ALL(pos, smax, smin, lmax, lmin, cmax, cmin,
	                    out s, out l, out c, out x, out y, ref i, flg);

                pos = retpos;

				if (pos == (IntPtr)0) {
					break;
				}

				if(mergeCount == 0)
				{
					r = 0x0000FF;
	            	OpenCV.OCV_DRAW_CONTOURS(0, pos, r, 0x0000FF);
	            }else
	            {
					r = CheckerManager.m_DataController.MakeDrawColor(mergeCount);
	            	OpenCV.OCV_DRAW_CONTOURS(50, pos, r, 0x0000FF);//��1����(50)�F�O�`�F�����J��Ԃ����摜�̏d�ˍ��킹
				}

				hosei_xx = DataController.SETDATA.center_x_hosei;
				hosei_yy = DataController.SETDATA.center_y_hosei;

				CheckerManager.m_DataController.sumInfo.shuicho = l;//���͒������O�ɕۑ�
				CheckerManager.m_DataController.sumInfo.enkeido = c;//�~�`�x�����O�ɕۑ�
				CheckerManager.m_DataController.sumInfo.menseki = s;//�ʐς����O�ɕۑ�

				if(mergeCount == 0)//�ŊO�`�̂� �ԐF
				{
					bSIGNE = (s < 0) ? 1 : 0;
#if false
					int n = OpenCV.OCV_APPROX_PTS(pos, bSIGNE, 1);//��3�����͋ߎ����x(0-100?)
#else
					OpenCV.OCV_APPROX_PTS_XY(pos, bSIGNE, 3, out n, ref px[0], ref py[0]);//��3�����͋ߎ����x(0-100?)
#endif
					for(cnt = 0; cnt < n; cnt++)
					{
						CheckerManager.m_DataController.SetValueXY(cnt, px[cnt], py[cnt]);

	                    hosei_x = x - hosei_xx;
	                    hosei_y = y - hosei_yy;
						CalcTriangle(g_max_brightness_x, g_max_brightness_y, cnt);
					}
					int maxLine, minLine;
					GetShasenMaxMin(n, out maxLine, out minLine);
					x = hosei_x;
					y = hosei_y;

					if(n > 0)
					{
						CheckerManager.m_DataController.IsPointInArea(g_max_brightness_x, g_max_brightness_y, true, numOfout, x, y, maxLine, minLine);//GHOST���o�p
						CheckerManager.form1.DrawShasenChart(n);
					}

					numOfout++;
				}
				else if(mergeCount == 1)//2�Ԗڂ̊O�` �F
				{
					CheckerManager.m_DataController.IsPointInsideArea(g_max_brightness_x, g_max_brightness_y, numOfin, x, y);
					numOfin++;
				}
				else
				{
				}

				CheckerManager.m_DataController.SetCircleInfo(CenterCount, s, l, c, x, y, r);
				CenterCount++;
            }

          	mergeCount++;

            OpenCV.OCV_FIND_TERM();

			Bitmap m_bmpZ = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			BitmapData bmpDataZ = m_bmpZ.LockBits(new Rectangle(0, 0, m_bmpZ.Width, m_bmpZ.Height), ImageLockMode.ReadWrite, m_bmpZ.PixelFormat);
			int retZ = OpenCV.OCV_GET_IMG(bmpDataZ.Scan0, bmpDataZ.Width, bmpDataZ.Height, bmpDataZ.Stride, PF2BPP(bmpDataZ.PixelFormat));
			m_bmpZ.UnlockBits(bmpDataZ);

			return m_bmpZ;
		}

        public Bitmap GetShapeLineInfo(int threshMin, int threshMax, Bitmap m_bmpR, out double s, out double l, out double c, out int x, out int y, bool checkflg)
		{
			int width, height;
			CheckerManager.form2.GetPreviewWindow(out width, out height);

			OpenCV.OCV_INITILIZE(width, height, 3);

            BitmapData bmpData = m_bmpR.LockBits(new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height), ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
            int ret = OpenCV.OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			m_bmpR.UnlockBits(bmpData);

			OpenCV.OCV_GRAY_THRESH_MERGE(threshMin, threshMax);

            int numContour;
            IntPtr contour = OpenCV.OCV_FIND_FIRST(2, out numContour);

            IntPtr pos = (IntPtr)0;
            IntPtr retpos = (IntPtr)0;

			s = l = c = x = y = 0;//�R���p�C���G���[����̒u��

			int flg = 0;
			if(checkflg)
			{
				flg = 1;
			}
			int r = 0;
			for(int i = 0; i < numContour; i++)
			{
	            retpos = OpenCV.OCV_FIND_NEXT_ALL(pos, smax, smin, lmax, lmin, cmax, cmin,
	                    out s, out l, out c, out x, out y, ref i, flg);

                pos = retpos;
				if (pos == (IntPtr)0) {
					break;
				}

				r = 0x0000FF;
            	OpenCV.OCV_DRAW_CONTOURS(0, pos, r, 0x0000FF);
            }

            OpenCV.OCV_FIND_TERM();

			Bitmap m_bmpZ = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			BitmapData bmpDataZ = m_bmpZ.LockBits(new Rectangle(0, 0, m_bmpZ.Width, m_bmpZ.Height), ImageLockMode.ReadWrite, m_bmpZ.PixelFormat);
			int retZ = OpenCV.OCV_GET_IMG(bmpDataZ.Scan0, bmpDataZ.Width, bmpDataZ.Height, bmpDataZ.Stride, PF2BPP(bmpDataZ.PixelFormat));
			m_bmpZ.UnlockBits(bmpDataZ);

			return m_bmpZ;
		}

		public double GetImageContrast(int threshMin, int threshMax, int resizeWidth, int resizeHeight, Bitmap m_bmpR, int left, int top, int right, int buttom)
		{
			OpenCV.OCV_INITILIZE(resizeWidth, resizeHeight, 3);

            BitmapData bmpData = m_bmpR.LockBits(new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height), ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
            int ret = OpenCV.OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			m_bmpR.UnlockBits(bmpData);

			OpenCV.OCV_GRAY_THRESH_MERGE(threshMin, threshMax);

			//CONTRAST�v�Z
			init_mask_rect(left, top, right, buttom);
			double[] HISTVALY = new double[256];
			Array.Clear(HISTVALY, 0, 256);

			double	HIST_MIN;
			double	HIST_MAX;
			double	HIST_AVG;

			OpenCV.OCV_CAL_HIST(1, 1, ref HISTVALY[0], out HIST_MIN, out HIST_MAX, out HIST_AVG);

			double CONTRAST = (HIST_MAX - HIST_MIN) / (HIST_MAX + HIST_MIN);

            OpenCV.OCV_FIND_TERM();

			return CONTRAST;
		}

		public void init_mask_rect(int left, int top, int right, int buttom)
		{
			OpenCV.RECT	rt;
			rt.Left   = left;
			rt.Top    = top;
			rt.Right  = right;
			rt.Bottom = buttom;
			OpenCV.OCV_ZERO(3);
			OpenCV.OCV_DRAW_RECT(3, ref rt, 0xFFFFFF, -1);
		}

		public int GetCenterCount()
		{
			return CenterCount;
		}

        static private int PF2BPP(PixelFormat pf)
		{
			int bpp;
			switch (pf) {
			case PixelFormat.Indexed:
			case PixelFormat.Format8bppIndexed:
				bpp = 8;
				break;
			case PixelFormat.Format24bppRgb:
				bpp = 24;
				break;
			case PixelFormat.Format32bppRgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format32bppArgb:
				bpp = 32;
				break;
			default:
				bpp = 8;
				break;
			}
			return(bpp);
		}

		//�摜�̒��S��`���̔������𐔂���
		public int GetDataInScopeArea(Bitmap src, int x, int y, int wide, int height)
		{
			var roi = new Rectangle(x, y, wide, height);
		    // srcRect��roi�̏d�Ȃ����̈���擾�i�摜���͂ݏo�����̈��؂���j
		    // �摜�̗̈�
		    var imgRect = new Rectangle(0, 0, src.Width, src.Height);
		    // �͂ݏo����������؂���(�d�Ȃ����̈���擾)
		    var roiTrim = Rectangle.Intersect(imgRect, roi);
		    // �摜�̊O�̗̈���w�肵���ꍇ
		    if (roiTrim.IsEmpty == true) return 0;
		 
		    //////////////////////////////////////////////////////////////////////
		    // �摜�̐؂�o��
		 
		    // �؂�o���傫���Ɠ����T�C�Y��Bitmap�I�u�W�F�N�g���쐬
		    var dst = new Bitmap(roiTrim.Width, roiTrim.Height, src.PixelFormat);
		    // Bitmap�I�u�W�F�N�g����Graphics�I�u�W�F�N�g�̍쐬
		    var g = Graphics.FromImage(dst);
		    // �`���
		    var dstRect = new Rectangle(0, 0, roiTrim.Width, roiTrim.Height);
		    // �`��
		    g.DrawImage(src, dstRect, roiTrim, GraphicsUnit.Pixel);
		    // ���
		    g.Dispose();

			//Bitmap��byte�ɕϊ�
			byte[] buffer = new byte[wide * height * 3];
			System.Drawing.Imaging.BitmapData bmpData = 
			    dst.LockBits(dstRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, 
			    PixelFormat.Format24bppRgb); 

			// Bitmap�̐擪�A�h���X���擾 
			IntPtr ptr = bmpData.Scan0; 
			// Bitmap�փR�s�[ 
			Marshal.Copy(ptr, buffer, 0, (dst.Width * dst.Height * 3));
			dst.UnlockBits(bmpData);

			//byte�z�񂩂�0xFF�𐔂���
			int count = 0;
			for(int i = 0; i < buffer.Length; i++)
			{
				if(buffer[i] == 0xFF)
				{
					count++;
				}
			}

			return count;
		}


		//�摜�̍ő�P�x�̒��S���W�����߂�
		public void GetCenterFromMaxBrightness(Bitmap org)
		{
			Bitmap src = (Bitmap)org.Clone();

			Rectangle rect = new Rectangle(0, 0, src.Width, src.Height); 
			System.Drawing.Imaging.BitmapData bmpData = 
			    src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, 
			    PixelFormat.Format24bppRgb); 

			// Bitmap�̐擪�A�h���X���擾 
			IntPtr ptr = bmpData.Scan0; 
			// Bitmap�փR�s�[ 
			byte[] pixels = new byte[bmpData.Stride * src.Height];

//			Marshal.Copy(pixels, 0, ptr, pixels.Length); 
			Marshal.Copy(ptr, pixels, 0, pixels.Length); 

			int pixelSize = 3;//24bit bmp
			int maxBrightnessCount = 0;
			int total_xpos = 0;
			int total_ypos = 0;

			for (int y = 0; y < bmpData.Height; y++)
			{
				for (int x = 0; x < bmpData.Width; x++)
				{
					//(x,y)�̃f�[�^�ʒu
					int pos = y * bmpData.Stride + x * pixelSize;
					// RGB
					byte b = pixels[pos] , g = pixels[pos + 1], r = pixels[pos + 2];

					if(b == 0xFF && g == 0xFF && r == 0xFF)
					{
						total_xpos += x;
						total_ypos += y;
						maxBrightnessCount++;
					}

				}
			}
			src.UnlockBits(bmpData);

			CheckerManager.m_DataController.sumInfo.maxBrightnessCount = maxBrightnessCount;//�ő�P�x�̐���ۑ�

			g_max_brightness_x = g_max_brightness_y = 0;
			if(maxBrightnessCount > 0)
			{
				g_max_brightness_x = total_xpos / maxBrightnessCount;
				g_max_brightness_y = total_ypos / maxBrightnessCount;
			}

		}



		public int GetAfBestAddress(int AfCount, int type, out double minValue, out int minTotal, out bool isAf)
		{
			for(int i = 0; i < 1024; i++)
			{
				x[i] = 0;
				y[i] = 0;
				xout[i] = 0;
				yout[i] = 0;
			}

			for(int i = 0; i < AfCount; i++)
			{
				x[i] = CheckerManager.m_DataController.af_info[i].address;
				if(type == 1)//�ŊO�`�̍ŏ��ʐ�
				{
					y[i] = CheckerManager.m_DataController.af_info[i].menseki;
				}
				else if(type == 2)//�����̔��F��
				{
					y[i] = CheckerManager.m_DataController.af_info[i].whiteCount;
				}
			}

			//�z����ɓ��l�������SPLINE�␳�����s����ׁA���l������ΏȂ�
			//�ő�̃A�h���X�l��T��
			int MaxAddress = 0;
			int IndexMax = 0;
			for(int i = 0; i < AfCount; i++)
			{
				if(i == 0)
				{
					MaxAddress = x[i];
					continue;
				}

				if(MaxAddress < x[i])
				{
					MaxAddress = x[i];
					IndexMax = i;
				}
			}

			//���l�̏���l������΍폜
			if(IndexMax < (AfCount - 1))
			{
				for(int i = (IndexMax + 1); i < AfCount; i++)
				{
					x[i] = 0;
					y[i] = 0;
				}
				AfCount -= (AfCount - (IndexMax + 1));
			}

			//�ŏ��̃A�h���X�l��T��
			int MinAddress = 0;
			int IndexMin = 0;
			for(int i = (AfCount - 1); i >= 0; i--)
			{
				if(i == (AfCount - 1))
				{
					MinAddress = x[i];
					continue;
				}

				if(MinAddress > x[i])
				{
					MinAddress = x[i];
					IndexMin = i;
				}
			}

			//���l�̉����l������Έړ�����
			if(IndexMin > 0)
			{
				for(int i = 0; i < (AfCount - IndexMin); i++)
				{
					x[i] = x[i + IndexMin];
					y[i] = y[i + IndexMin];
				}

				for(int i = (AfCount - IndexMin); i < AfCount; i++)
				{
					x[i] = 0;
					y[i] = 0;
				}

				AfCount -= IndexMin;
			}

			
			int out_count = 0;

			int x_sabun = x[AfCount - 1] - x[0];
			int af_div_size = AfCount * 20;//1��Ԃ��ו����B���͔C��
			double f_x_sabun = (double)x_sabun;
			double f_af_div_size = (double)af_div_size;
			double inc_size = (double)(f_x_sabun / f_af_div_size);
#if false
			//�����_�ȉ��؂�グ
			int increment = (int)Math.Ceiling(inc_size);
#else
			//�����_�ȉ��؂艺��
			int increment = (int)inc_size;
#endif
			//SPLINE�␳
			for(int xx = x[0]; xx <= x[AfCount - 1]; xx += increment)
			{
				double yyout = 0;
				xout[out_count] = xx;
				GetSplineData(AfCount, xx, out yyout);
				yout[out_count] = yyout;
				out_count++;
			}

			isAf = true;
			//AF���� �␳�������ʁA�}�C�i�X�l���܂܂��ꍇ�G���[�Ƃ݂Ȃ�
			for(int i = 0; i < yout.Length; i++)
			{
				if(yout[i] < 0)
				{
					minValue = minTotal = -1;
					isAf = false;


					//debug start
					string csvpath = CheckerManager.m_DataController.GetCSVFileName("af_error.csv");
					string buf = "";
					StreamWriter wr;
		            for (int ii = 0; ii < yout.Length; ii++)
				    {
						try {
							wr = new StreamWriter(csvpath, true, Encoding.Default);
							buf = "";

							buf = string.Format("{0}", ii);
							buf += string.Format(",{0}", x[ii]);
							buf += string.Format(",{0}", (int)y[ii]);
							buf += string.Format(",{0}", xout[ii]);
							buf += string.Format(",{0}", (int)yout[ii]);

							wr.WriteLine(buf);
							wr.Close();
						}
						catch (Exception) {
						}

					}
					//debug end

					return -1;
				}
			}

			int minIndex = 0;
			minTotal = 0;
			minValue = 0;
			if(type == 1)//�ŊO�`���ŏ��ʐςƂȂ�ʒu�����߂�ꍇ
			{
				minValue = 10000;//�C��
				for(int i = 0; i < yout.Length; i++)//1��Ԃ��ו����B���͔C��
				{
					if(xout[i] < -1000 || 1000 < xout[i])//���[�^�[�A�h���X��-1000~1000�̊ԂŔ��肷��悤����
					{
						continue;
					}
					
					if(yout[i] > 0 && minValue > yout[i])
					{
						minValue = yout[i];
						minIndex = i;
					}
				}
			}
			else if(type == 2)//�����̔��F�����ŏ��ƂȂ�ʒu�����߂�ꍇ
			{
				minTotal = 10000;//�C��
				for(int i = 0; i < yout.Length; i++)//1��Ԃ��ו����B���͔C��
				{
					if(xout[i] < -1000 || 1000 < xout[i])//���[�^�[�A�h���X��-1000~1000�̊ԂŔ��肷��悤����
					{
						continue;
					}
					
					if(yout[i] > 0 && minTotal > yout[i])
					{
						minTotal = (int)yout[i];
						minIndex = i;
					}
				}
			}

			//�f�o�b�O�p ����������
			DateTime dt = DateTime.Now;
			string pathstr = string.Format("{0:D04}{1:D02}{2:D02}{3:D02}{4:D02}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
			if(type == 1)
			{
				string csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
				string buf = "";
				StreamWriter wr;

				try {
					wr = new StreamWriter(csvpath, true, Encoding.Default);

					buf = string.Format("'{0}", pathstr);
					buf += string.Format(",�␳�O�@Address");
					buf += string.Format(",�␳�O�@�O�`�ʐ�");
					buf += string.Format(",�␳��@Address");
					buf += string.Format(",�␳��@�O�`�ʐ�");

					wr.WriteLine(buf);
					wr.Close();
				}
				catch (Exception) {
				}

	            for (int i = 0; i < yout.Length; i++)
			    {
					Console.WriteLine("index={0},address={1},menseki={2:f2}",
					i,
					xout[i],
					yout[i]
					);

					csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
					buf = "";
					try {
						wr = new StreamWriter(csvpath, true, Encoding.Default);

						buf = string.Format("{0}", i);
						buf += string.Format(",{0}", x[i]);
						buf += string.Format(",{0:f2}", y[i]);
						buf += string.Format(",{0}", xout[i]);
						buf += string.Format(",{0:f2}", yout[i]);

						wr.WriteLine(buf);
						wr.Close();
					}
					catch (Exception) {
					}
				}

				csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
				buf = "";
				try {
					wr = new StreamWriter(csvpath, true, Encoding.Default);

					buf = string.Format("best={0}", minIndex);
					buf += string.Format(",{0}", xout[minIndex]);
					buf += string.Format(",{0:f2}", yout[minIndex]);

					wr.WriteLine(buf);
					wr.Close();
				}
				catch (Exception) {
				}

			}
			else if(type == 2)
			{
				string csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
				string buf = "";
				StreamWriter wr;
				try {
					wr = new StreamWriter(csvpath, true, Encoding.Default);

					buf = string.Format("'{0}", pathstr);
					buf += string.Format(",�␳�O�@Address");
					buf += string.Format(",�␳�O�@�O�`�ʐ�");
					buf += string.Format(",�␳��@Address");
					buf += string.Format(",�␳��@���F��");

					wr.WriteLine(buf);
					wr.Close();
				}
				catch (Exception) {
				}

	            for (int i = 0; i < yout.Length; i++)
			    {
					Console.WriteLine("index={0},address={1},whitecount={2}",
					i,
					xout[i],
					(int)yout[i]
					);

					csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
					buf = "";
					try {
						wr = new StreamWriter(csvpath, true, Encoding.Default);

						buf = string.Format("{0}", i);
						buf += string.Format(",{0}", x[i]);
						buf += string.Format(",{0}", (int)y[i]);
						buf += string.Format(",{0}", xout[i]);
						buf += string.Format(",{0}", (int)yout[i]);

						wr.WriteLine(buf);
						wr.Close();
					}
					catch (Exception) {
					}

				}

				csvpath = CheckerManager.m_DataController.GetCSVFileName("af.csv");
				buf = "";
				try {
					wr = new StreamWriter(csvpath, true, Encoding.Default);

					buf = string.Format("best={0}", minIndex);
					buf += string.Format(",{0}", xout[minIndex]);
					buf += string.Format(",{0}", (int)yout[minIndex]);

					wr.WriteLine(buf);
					wr.Close();
				}
				catch (Exception) {
				}

            }
			//�f�o�b�O�p ����������
			
			return xout[minIndex];
		}

		public void test(int type)
		{
		}

		public void GetSplineData(int n, int x1, out double yout)
		{
		    int shift = 0;
		    //��x[]�͏����ł��邱�ƁI
		    int i=-1, i1, k;
		    double y1, qi, si, xx;
		    x1-=shift; //�V�t�g����

		    double[] h = new double [n];
		    double[] b = new double [n];
		    double[] d = new double [n];
		    double[] g = new double [n];
		    double[] u = new double [n];
		    double[] r = new double [n+1];

		    //��Ԃ̌���
		    for(i1=1; i1<n && i<0 ;i1++){
		        if(x1<x[i1])
		            i=i1-1;
		    }
		    if(i<0)
		        i=n-1;

		    //�X�e�b�v�P
		    for(i1=0;i1<n;i1++){
		        h[i1] = x[i1+1]-x[i1];
		        if(h[i1]==0) h[i1]=0.000000001;
		    }
		    for(i1=1;i1<n;i1++){
		        b[i1] = 2.0*(h[i1]+h[i1-1]);
		        d[i1] = 3.0*((y[i1+1]-y[i1])/h[i1]-(y[i1]-y[i1-1])/h[i1-1]);
		    }

		    //�X�e�b�v�Q
		    g[1]=h[1]/b[1];
		    for(i1=2;i1<n-1;i1++)
		        g[i1] = h[i1]/(b[i1]-h[i1-1]*g[i1-1]);
		    u[1]=d[1]/b[1];
		    for(i1= 2;i1<n;i1++)
		        u[i1] = (d[i1]-h[i1-1]*u[i1-1])/(b[i1]-h[i1-1]*g[i1-1]);

		    //�X�e�b�v�R
		    k      = (i>1) ? i : 1;
		    r[0]   = 0.0;
		    r[n]   = 0.0;
		    r[n-1] = u[n-1];
		    for(i1=n-2;i1>=k;i1--)
		        r[i1] = u[i1]-g[i1]*r[i1+1];

		    //�X�e�b�v�S
		    xx = x1-x[i];
		    qi = (y[i+1]-y[i])/h[i]-h[i]*(r[i+1]+2.0*r[i])/3.0;
		    si = (r[i+1]-r[i])/(3.0*h[i]);
		    y1 = y[i]+xx*(qi+xx*(r[i]+si*xx));
		    if(y1<0.1 && shift!=0){
//		        int a=0;
		    }

		    if(System.Math.Abs(y1)>10000){
		        yout = -9999; //
		    }

		    yout = y1;
		}


	}
}