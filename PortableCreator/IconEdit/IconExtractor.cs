using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TsudaKageyu
{
	public class IconExtractor
	{
		public string FileName { get; private set; }

		public int Count
		{
			get
			{
				return this.iconData.Length;
			}
		}

		public IconExtractor(string fileName)
		{
			this.Initialize(fileName);
		}

		public Icon GetIcon(int index)
		{
			if (index < 0 || this.Count <= index)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Icon icon;
			using (MemoryStream memoryStream = new MemoryStream(this.iconData[index]))
			{
				icon = new Icon(memoryStream);
			}
			return icon;
		}

		public Icon[] GetAllIcons()
		{
			List<Icon> list = new List<Icon>();
			for (int i = 0; i < this.Count; i++)
			{
				list.Add(this.GetIcon(i));
			}
			return list.ToArray();
		}

		public void Save(int index, Stream outputStream)
		{
			if (index < 0 || this.Count <= index)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (outputStream == null)
			{
				throw new ArgumentNullException("outputStream");
			}
			byte[] array = this.iconData[index];
			outputStream.Write(array, 0, array.Length);
		}

		private void Initialize(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			IntPtr hModule = IntPtr.Zero;
			try
			{
				hModule = NativeMethods.LoadLibraryEx(fileName, IntPtr.Zero, 2U);
				if (hModule == IntPtr.Zero)
				{
					throw new Win32Exception();
				}
				this.FileName = this.GetFileName(hModule);
				List<byte[]> tmpData = new List<byte[]>();
				ENUMRESNAMEPROC enumresnameproc = delegate(IntPtr h, IntPtr t, IntPtr name, IntPtr l)
				{
					byte[] dataFromResource = this.GetDataFromResource(hModule, IconExtractor.RT_GROUP_ICON, name);
					int num = (int)BitConverter.ToUInt16(dataFromResource, 4);
					int num2 = 6 + 16 * num;
					for (int i = 0; i < num; i++)
					{
						num2 += BitConverter.ToInt32(dataFromResource, 6 + 14 * i + 8);
					}
					using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(num2)))
					{
						binaryWriter.Write(dataFromResource, 0, 6);
						int num3 = 6 + 16 * num;
						for (int j = 0; j < num; j++)
						{
							ushort num4 = BitConverter.ToUInt16(dataFromResource, 6 + 14 * j + 12);
							byte[] dataFromResource2 = this.GetDataFromResource(hModule, IconExtractor.RT_ICON, (IntPtr)((int)num4));
							binaryWriter.Seek(6 + 16 * j, SeekOrigin.Begin);
							binaryWriter.Write(dataFromResource, 6 + 14 * j, 8);
							binaryWriter.Write(dataFromResource2.Length);
							binaryWriter.Write(num3);
							binaryWriter.Seek(num3, SeekOrigin.Begin);
							binaryWriter.Write(dataFromResource2, 0, dataFromResource2.Length);
							num3 += dataFromResource2.Length;
						}
						tmpData.Add(((MemoryStream)binaryWriter.BaseStream).ToArray());
					}
					return true;
				};
				NativeMethods.EnumResourceNames(hModule, IconExtractor.RT_GROUP_ICON, enumresnameproc, IntPtr.Zero);
				this.iconData = tmpData.ToArray();
			}
			finally
			{
				if (hModule != IntPtr.Zero)
				{
					NativeMethods.FreeLibrary(hModule);
				}
			}
		}

		private byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
		{
			IntPtr intPtr = NativeMethods.FindResource(hModule, name, type);
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			IntPtr intPtr2 = NativeMethods.LoadResource(hModule, intPtr);
			if (intPtr2 == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			IntPtr intPtr3 = NativeMethods.LockResource(intPtr2);
			if (intPtr3 == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			uint num = NativeMethods.SizeofResource(hModule, intPtr);
			if (num == 0U)
			{
				throw new Win32Exception();
			}
			byte[] array = new byte[num];
			Marshal.Copy(intPtr3, array, 0, array.Length);
			return array;
		}

		private string GetFileName(IntPtr hModule)
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			if (NativeMethods.GetMappedFileName(NativeMethods.GetCurrentProcess(), hModule, stringBuilder, stringBuilder.Capacity) == 0)
			{
				throw new Win32Exception();
			}
			string text = stringBuilder.ToString();
			for (char c = 'A'; c <= 'Z'; c += '\u0001')
			{
				string text2 = c.ToString() + ":";
				StringBuilder stringBuilder2 = new StringBuilder(260);
				if (NativeMethods.QueryDosDevice(text2, stringBuilder2, stringBuilder2.Capacity) != 0)
				{
					string text3 = stringBuilder2.ToString();
					if (text.StartsWith(text3))
					{
						return text2 + text.Substring(text3.Length);
					}
				}
			}
			return text;
		}

		private const uint LOAD_LIBRARY_AS_DATAFILE = 2U;
		private static readonly IntPtr RT_ICON = (IntPtr)3;
		private static readonly IntPtr RT_GROUP_ICON = (IntPtr)14;
		private const int MAX_PATH = 260;
		private byte[][] iconData;
	}
}
