using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace TsudaKageyu
{
	public static class IconUtil
	{
		static IconUtil()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("GetIconData", typeof(byte[]), new Type[] { typeof(Icon) }, typeof(Icon));
			FieldInfo field = typeof(Icon).GetField("iconData", BindingFlags.Instance | BindingFlags.NonPublic);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			ilgenerator.Emit(OpCodes.Ldarg_0);
			ilgenerator.Emit(OpCodes.Ldfld, field);
			ilgenerator.Emit(OpCodes.Ret);
			IconUtil.getIconData = (IconUtil.GetIconDataDelegate)dynamicMethod.CreateDelegate(typeof(IconUtil.GetIconDataDelegate));
		}

		public static Icon[] Split(this Icon icon)
		{
			if (icon == null)
			{
				throw new ArgumentNullException("icon");
			}
			byte[] iconData = IconUtil.GetIconData(icon);
			List<Icon> list = new List<Icon>();
			int num = (int)BitConverter.ToUInt16(iconData, 4);
			for (int i = 0; i < num; i++)
			{
				int num2 = BitConverter.ToInt32(iconData, 6 + 16 * i + 8);
				int num3 = BitConverter.ToInt32(iconData, 6 + 16 * i + 12);
				using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(22 + num2)))
				{
					binaryWriter.Write(iconData, 0, 4);
					binaryWriter.Write(1);
					binaryWriter.Write(iconData, 6 + 16 * i, 12);
					binaryWriter.Write(22);
					binaryWriter.Write(iconData, num3, num2);
					binaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
					list.Add(new Icon(binaryWriter.BaseStream));
				}
			}
			return list.ToArray();
		}

		public static Icon TryGetIcon(this Icon icon, Size size, int bits, bool tryResize, bool tryRedefineBitsCount)
		{
			return icon.Split().TryGetIcon(size, bits, tryResize, tryRedefineBitsCount);
		}

		public static Icon TryGetIcon(this Icon[] icons, Size size, int bits, bool tryResize, bool tryRedefineBitsCount)
		{
			foreach (Icon icon in icons)
			{
				if (icon.Size == size && icon.GetBitCount() == bits)
				{
					return icon;
				}
			}
			if (tryResize || tryRedefineBitsCount)
			{
				Icon icon2 = null;
				foreach (Icon icon3 in icons)
				{
					bool flag = (icon3.Size == size || tryResize) && ((icon3.Size.Height > size.Height && (icon2 == null || icon3.Size.Height > icon2.Size.Height)) || (icon3.Size.Height < size.Height && (icon2 == null || icon3.Size.Height > icon2.Size.Height)));
					if (!flag)
					{
						int bitCount = icon3.GetBitCount();
						int bitCount2 = icon2.GetBitCount();
						flag = (bitCount == bits || tryRedefineBitsCount) && ((bitCount > bits && (icon2 == null || bitCount > bitCount2)) || (bitCount < bits && (icon2 == null || bitCount > bitCount2)));
					}
					if (flag)
					{
						icon2 = icon3;
					}
				}
				return icon2;
			}
			return null;
		}

		public static Bitmap ToBitmap(Icon icon)
		{
			if (icon == null)
			{
				throw new ArgumentNullException("icon");
			}
			Bitmap bitmap2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				icon.Save(memoryStream);
				using (Bitmap bitmap = (Bitmap)Image.FromStream(memoryStream))
				{
					bitmap2 = new Bitmap(bitmap);
				}
			}
			return bitmap2;
		}

		public static int GetBitCount(this Icon icon)
		{
			if (icon == null)
			{
				throw new ArgumentNullException("icon");
			}
			byte[] iconData = IconUtil.GetIconData(icon);
			if (iconData.Length >= 51 && iconData[22] == 137 && iconData[23] == 80 && iconData[24] == 78 && iconData[25] == 71 && iconData[26] == 13 && iconData[27] == 10 && iconData[28] == 26 && iconData[29] == 10 && iconData[30] == 0 && iconData[31] == 0 && iconData[32] == 0 && iconData[33] == 13 && iconData[34] == 73 && iconData[35] == 72 && iconData[36] == 68 && iconData[37] == 82)
			{
				switch (iconData[47])
				{
				case 0:
					return (int)iconData[46];
				case 2:
					return (int)(iconData[46] * 3);
				case 3:
					return (int)iconData[46];
				case 4:
					return (int)(iconData[46] * 2);
				case 6:
					return (int)(iconData[46] * 4);
				}
			}
			else if (iconData.Length >= 22)
			{
				return (int)BitConverter.ToUInt16(iconData, 12);
			}
			throw new ArgumentException("The icon is corrupt. Couldn't read the header.", "icon");
		}

		private static byte[] GetIconData(Icon icon)
		{
			byte[] array = IconUtil.getIconData(icon);
			if (array != null)
			{
				return array;
			}
			byte[] array2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				icon.Save(memoryStream);
				array2 = memoryStream.ToArray();
			}
			return array2;
		}

		private static IconUtil.GetIconDataDelegate getIconData;
		private delegate byte[] GetIconDataDelegate(Icon icon);
	}
}
