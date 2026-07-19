namespace ugcp_svc.EventQueue
{
	sealed class CommandType
	{
		public const byte Key = 1;

		public const byte DxButton = 2;
		public const byte DxHat = 3;

		public const byte MouseBt1 = 4;
		public const byte MouseBt2 = 5;
		public const byte MouseBt3 = 6;
		public const byte MouseLeft = 7;
		public const byte MouseRight = 8;
		public const byte MouseUp = 9;
		public const byte MouseDown = 10;
		public const byte MouseWhUp = 11;
		public const byte MouseWhDown = 12;

		public const byte DxAxis = 13;

		public const byte Delay = 20;
		public const byte Hold = 21;
		public const byte Repeat = 22;
		public const byte RepeatN = 23;

		public const byte Mode = 30;
		public const byte Submode = 31;
		public const byte PreciseMode = 32;

		public const byte X52MfdLight = 40;
		public const byte X52Light = 41;
		public const byte X52InfoLight = 42;
		public const byte X52MfdPinkie = 43;
		public const byte X52MfdTextIni = 44;
		public const byte X52MfdText = 45;
		public const byte X52MfdTextEnd = 46;
		public const byte X52MfdHour = 47;
		public const byte X52MfdHour24 = 48;
		public const byte MfdDate = 49;

		public const byte NxtLeds = 50;

		public const byte Reserved_DxPosition = 100;
		public const byte Reserved_CheckHold = 101;
		//Reserved_RepeatIni;

		public const byte Release = 128;

        // Conversión implícita desde byte
        public static implicit operator CommandType(byte v)
        {
            return new CommandType(v);
        }

        // Conversión implícita hacia byte
        public static implicit operator byte(CommandType c)
        {
            return c.value;
        }

        public static bool operator ==(CommandType c, byte v) { return c.value == v; }

        public override bool Equals(object? obj)
        {
            if (obj is CommandType other)
                return value == other.value;

            return false;
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static bool operator !=(CommandType c, byte v) { return c.value != v; }

		public static byte operator &(CommandType c, byte op2) { return (byte)(c.value & op2); }
		public byte Get() { return value; }

		private byte value = 0;

        private CommandType(byte value) { this.value = value; }
    }
}
