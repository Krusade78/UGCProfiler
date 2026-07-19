using System.Runtime.CompilerServices;

namespace ugcp_svc.HIDInput
{
    struct HID_INPUT_DATA
    {
        [InlineArray(24)]
        public struct AxisArray
        {
            private ushort item;
        }
        [InlineArray(2)]
        public struct ButtonArray
        {
            private ulong item;
        }
        [InlineArray(4)]
        public struct HatArray
        {
            private byte item;
        }

        public AxisArray Axis;
        public ButtonArray Buttons;
        public HatArray Hats;
    }
}
