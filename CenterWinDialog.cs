using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace uSCOPE
{
	class CenterWinDialog : IDisposable {
		private int mTries = 0;
		private Form mOwner;

		public CenterWinDialog(Form owner) {
			mOwner = owner;
			owner.BeginInvoke(new MethodInvoker(findDialog));
		}

		private void findDialog() {
			// Enumerate windows to find the message box
			if (mTries < 0) return;
			EnumThreadWndProc callback = new EnumThreadWndProc(checkWindow);
			if (EnumThreadWindows(GetCurrentThreadId(), callback, IntPtr.Zero)) {
				if (++mTries < 10) mOwner.BeginInvoke(new MethodInvoker(findDialog));
			}
		}
		private void GetDesktopRect(ref Rectangle rect)
		{
			var scr = System.Windows.Forms.Screen.FromControl(this.mOwner);
			rect = scr.Bounds;
		}
		private bool checkWindow(IntPtr hWnd, IntPtr lp) {
			// Checks if <hWnd> is a dialog
			StringBuilder sb = new StringBuilder(260);
			GetClassName(hWnd, sb, sb.Capacity);
			if (sb.ToString() != "#32770") return true;
			// Got it

			Rectangle frmRect = new Rectangle(mOwner.Location, mOwner.Size);
			RECT dlgRect;
			Rectangle dskRect = new Rectangle();
			GetDesktopRect(ref dskRect);
			GetWindowRect(hWnd, out dlgRect);
			int x, y, w, h;
			const
			int GAP = 10;
			x = frmRect.Left + (frmRect.Width - dlgRect.Right + dlgRect.Left) / 2;
			y = frmRect.Top + (frmRect.Height - dlgRect.Bottom + dlgRect.Top) / 2;
			w = dlgRect.Right - dlgRect.Left;
			h = dlgRect.Bottom - dlgRect.Top;
			if ((x + w) >= dskRect.Width)
			{
				x = dskRect.Width - w - GAP;
			}
			if ((y + h) >= dskRect.Height)
			{
				y = dskRect.Height - h - GAP;
			}
			MoveWindow(hWnd, x, y, w, h, true);
			return false;
		}
		public void Dispose() {
			mTries = -1;
		}

		// P/Invoke declarations
		private delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lp);
		[DllImport("user32.dll")]
		private static extern bool EnumThreadWindows(int tid, EnumThreadWndProc callback, IntPtr lp);
		[DllImport("kernel32.dll")]
		private static extern int GetCurrentThreadId();
		[DllImport("user32.dll")]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder buffer, int buflen);
		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
		[DllImport("user32.dll")]
		private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);
		private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
		}
		/*
			Sample usage:

			private void button1_Click(object sender, EventArgs e) {
				using (new CenterWinDialog(this)) {
					MessageBox.Show("Nobugz waz here");
				}
			}
		 * 
			Note that this code works for any of the Windows dialogs. MessageBox, OpenFormDialog, FolderBrowserDialog, PrintDialog, ColorDialog, FontDialog, PageSetupDialog, SaveFileDialog.
			share|improve this answer 
		*/
}
