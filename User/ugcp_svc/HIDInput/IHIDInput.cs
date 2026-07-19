using System;

namespace ugcp_svc.HIDInput
{
    abstract class IHIDInput
    {
        protected abstract bool Prepare();
        protected abstract bool Open();
        protected abstract void Close();
        public abstract ushort Read(Span<byte> buff);

        public static uint GetHardwareId(ref string path)
        {
            uint ret = 0xFFFFFFFF;

            int vIndex = path.IndexOf("VID_", StringComparison.OrdinalIgnoreCase);
            int pIndex = path.IndexOf("PID_", StringComparison.OrdinalIgnoreCase);
            if ((vIndex != -1) && (pIndex != -1))
            {
                ret = uint.Parse(path.Substring(vIndex + 4, 4), System.Globalization.NumberStyles.AllowHexSpecifier) << 16;
                ret |= uint.Parse(path.Substring(pIndex + 4, 4), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return ret;
        }

    }
}
