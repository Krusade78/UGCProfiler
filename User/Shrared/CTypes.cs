using System.Runtime.InteropServices;

namespace Shared
{
	public class CTypes
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct STLIMITS
		{
			public byte Cal; //bool
			public byte Null;
			public ushort Left;
			public ushort Center;
			public ushort Right;
			public ushort Range;
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct STJITTER
		{
			public byte Antiv; //bool
			public byte PosRepeated;
			public byte Margin;
			public byte Strength;
			public ushort PosChosen;
			//public ushort PosPosible;
		};

		public enum CommandType : byte
		{
			Key = 1,

			DxButton,
			DxHat,

			MouseBt1,
			MouseBt2,
			MouseBt3,
			MouseLeft,
			MouseRight,
			MouseUp,
			MouseDown,
			MouseWhUp,
			MouseWhDown,

			Delay = 20,
			Hold,
			Repeat,
			RepeatN,

			Mode = 30,
			SubMode,
			PrecisionMode,

			X52MfdLight = 40,
			X52Light,
			X52InfoLight,
			X52MfdPinkie,
			X52MfdTextIni,
			X52MfdText,
			X52MfdTextEnd,
			X52MfdHour,
			X52MfdHour24,
			x52MfdDate,

			VkbGladiatorNxtLeds = 50,

			Reserved_DxPosition = 100,

			Release = 128,
		};
	}
}
