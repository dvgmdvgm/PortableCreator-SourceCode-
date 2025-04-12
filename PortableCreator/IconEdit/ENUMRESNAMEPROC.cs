using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TsudaKageyu
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
	[SuppressUnmanagedCodeSecurity]
	internal delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
}
