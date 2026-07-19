using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Shared
{
	public class CTypes
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct STLIMITS
		{
            [JsonInclude]
            public byte Null;
            [JsonInclude]
            public ushort Left;
            [JsonInclude]
            public ushort Center;
            [JsonInclude]
            public ushort Right;
            [JsonInclude]
            public ushort Range;
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct STJITTER
		{
            [JsonInclude]
            public byte Antiv; //bool
            [JsonInclude]
            public byte PosRepeated;
            [JsonInclude]
            public byte Margin;
            [JsonInclude]
            public byte Strength;
            [JsonInclude]
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

			DxAxis,

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
