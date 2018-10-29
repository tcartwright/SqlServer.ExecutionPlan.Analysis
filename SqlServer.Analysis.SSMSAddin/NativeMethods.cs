using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace SqlServer.ExecutionPlan.Analysis.SSMSAddin
{
	#pragma warning disable 0649
	internal static class NativeMethods
	{
		public struct COPYDATASTRUCT
		{
			public IntPtr dwData;

			public int cbData;

			public IntPtr lpData;
		}

		public const int SW_RESTORE = 9;

		public const uint WM_APP = 32768u;

		public const uint WM_APP_RELOAD_XML = 32769u;

		public const int WM_COPYDATA = 74;

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetFocus();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}
	#pragma warning restore 0649

}
