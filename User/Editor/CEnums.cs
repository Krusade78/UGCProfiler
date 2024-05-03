namespace Profiler
{
    class CEnums
    {
        public enum ElementType : byte
        {
            AxisX,
            AxisY,
            AxisZ,
            AxisRx,
            AxisRy,
            AxisRz,
            AxisSl,
            Hat = 253,
            Button,
            None
        }

        public enum LedOrder : byte
        {
            Off,
            Constant,
            Blink_Slow,
            Blink_Medium,
            Blink_Fast,
            Flash
        };

        public enum ColorMode : byte
        {
            Color1,
            Color2,
            Color1_2,
            Color2_1,
            Color1y2,
            Color1Add,
            Color2Add
        };
    }
}
