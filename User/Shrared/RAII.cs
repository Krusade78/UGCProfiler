using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Shared.RAII
{
    public class UniqueHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public UniqueHandle() : base(true)  { }
        public UniqueHandle(nint handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return API.Win32.CloseHandle(handle);
        }

        public nint Move()
        {
            nint old = handle;
            SetHandle(nint.Zero);
            return old;
        }
    }

    public class HGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public HGlobalHandle(int size) : base(true)
        {
            SetHandle(Marshal.AllocHGlobal(size));
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
