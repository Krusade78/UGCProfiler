using System;
using System.Runtime.InteropServices;

namespace API
{
    //#pragma warning disable CS0649
    public partial class HID
    {
        public static void HidD_GetHidGuid(ref Guid guid) => guid = new("{4D1E55B2-F16F-11CF-88CB-001111000030}");

        [LibraryImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool HidD_GetProductString(IntPtr HidDeviceObject, IntPtr Buffer, uint BufferLength);

        [LibraryImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

        [LibraryImport("hid.dll", SetLastError = true)]
        public static partial int HidP_GetCaps(IntPtr PreparsedData, IntPtr Capabilities);

        [LibraryImport("hid.dll", SetLastError = true)]
        public static partial int HidP_GetButtonCaps(int ReportType, IntPtr ButtonCaps, ref ushort ButtonCapsLength, IntPtr PreparsedData);

        [LibraryImport("hid.dll", SetLastError = true)]
        public static partial int HidP_GetValueCaps(int ReportType, IntPtr ValueCaps, ref ushort ValueCapsLength, IntPtr PreparsedData);

        [LibraryImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool HidD_FreePreparsedData(IntPtr PreparsedData);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HIDP_CAPS
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort Usage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort UsagePage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort InputReportByteLength;
            [MarshalAs(UnmanagedType.U2)]
            public ushort OutputReportByteLength;
            [MarshalAs(UnmanagedType.U2)]
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberLinkCollectionNodes;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberInputButtonCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberInputValueCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberInputDataIndices;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberOutputButtonCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberOutputValueCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberOutputDataIndices;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberFeatureButtonCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberFeatureValueCaps;
            [MarshalAs(UnmanagedType.U2)]
            public ushort NumberFeatureDataIndices;
        };

        public struct HIDP_BUTTON_CAPS
        {
            public ushort UsagePage;
            public byte ReportID;
            public byte IsAlias;
            public ushort BitField;
            public ushort LinkCollection;
            public ushort LinkUsage;
            public ushort LinkUsagePage;
            public byte IsRange;
            public byte IsStringRange;
            public byte IsDesignatorRange;
            public byte IsAbsolute;
            public ushort ReportCount;
            public ushort Reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public uint[] Reserved;
            public _Anonymous_e__Union Anonymous;

            [StructLayout(LayoutKind.Explicit)]
#pragma warning disable IDE1006 // Naming Styles
            public partial struct _Anonymous_e__Union
#pragma warning restore IDE1006 // Naming Styles
            {
                [FieldOffset(0)]
                public _Range_e__Struct Range;
                [FieldOffset(0)]
                public _NotRange_e__Struct NotRange;

#pragma warning disable IDE1006 // Naming Styles
                public partial struct _Range_e__Struct
#pragma warning restore IDE1006 // Naming Styles
                {
                    public ushort UsageMin;
                    public ushort UsageMax;
                    public ushort StringMin;
                    public ushort StringMax;
                    public ushort DesignatorMin;
                    public ushort DesignatorMax;
                    public ushort DataIndexMin;
                    public ushort DataIndexMax;
                }

#pragma warning disable IDE1006 // Naming Styles
                public partial struct _NotRange_e__Struct
#pragma warning restore IDE1006 // Naming Styles
                {
                    public ushort Usage;
                    public ushort Reserved1;
                    public ushort StringIndex;
                    public ushort Reserved2;
                    public ushort DesignatorIndex;
                    public ushort Reserved3;
                    public ushort DataIndex;
                    public ushort Reserved4;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HIDP_VALUE_CAPS
        {
            public ushort UsagePage;
            public byte ReportID;
            public byte IsAlias;
            public ushort BitField;
            public ushort LinkCollection;
            public ushort LinkUsage;
            public ushort LinkUsagePage;
            public byte IsRange;
            public byte IsStringRange;
            public byte IsDesignatorRange;
            public byte IsAbsolute;
            public byte HasNull;
            public byte Reserved;
            public ushort BitSize;
            public ushort ReportCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ushort[] Reserved2;
            public uint UnitsExp;
            public uint Units;
            public int LogicalMin;
            public int LogicalMax;
            public int PhysicalMin;
            public int PhysicalMax;
            public _Anonymous_e__Union Anonymous;

            [StructLayout(LayoutKind.Explicit)]
#pragma warning disable IDE1006 // Naming Styles
            public partial struct _Anonymous_e__Union
#pragma warning restore IDE1006 // Naming Styles
            {
                [FieldOffset(0)]
                public _Range_e__Struct Range;
                [FieldOffset(0)]
                public _NotRange_e__Struct NotRange;

#pragma warning disable IDE1006 // Naming Styles
                public partial struct _Range_e__Struct
#pragma warning restore IDE1006 // Naming Styles
                {
                    public ushort UsageMin;
                    public ushort UsageMax;
                    public ushort StringMin;
                    public ushort StringMax;
                    public ushort DesignatorMin;
                    public ushort DesignatorMax;
                    public ushort DataIndexMin;
                    public ushort DataIndexMax;
                }

#pragma warning disable IDE1006 // Naming Styles
                public partial struct _NotRange_e__Struct
#pragma warning restore IDE1006 // Naming Styles
                {
                    public ushort Usage;
                    public ushort Reserved1;
                    public ushort StringIndex;
                    public ushort Reserved2;
                    public ushort DesignatorIndex;
                    public ushort Reserved3;
                    public ushort DataIndex;
                    public ushort Reserved4;
                }
            }
        }
    }
    //#pragma warning restore CS0649
}
