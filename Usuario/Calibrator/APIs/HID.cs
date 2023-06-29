using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Calibrator.APIs
{
    internal class HID
    {
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool HidD_GetProductString(IntPtr HidDeviceObject, StringBuilder Buffer, uint BufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetButtonCaps(int ReportType, IntPtr ButtonCaps, ref ushort ButtonCapsLength, IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetValueCaps(int ReportType, IntPtr ValueCaps, ref ushort ValueCapsLength, IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

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
            internal ushort UsagePage;
            internal byte ReportID;
            internal byte IsAlias;
            internal ushort BitField;
            internal ushort LinkCollection;
            internal ushort LinkUsage;
            internal ushort LinkUsagePage;
            internal byte IsRange;
            internal byte IsStringRange;
            internal byte IsDesignatorRange;
            internal byte IsAbsolute;
            internal ushort ReportCount;
            internal ushort Reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            internal uint[] Reserved;
            internal _Anonymous_e__Union Anonymous;

            [StructLayout(LayoutKind.Explicit)]
            internal partial struct _Anonymous_e__Union
            {
                [FieldOffset(0)]
                internal _Range_e__Struct Range;
                [FieldOffset(0)]
                internal _NotRange_e__Struct NotRange;

                internal partial struct _Range_e__Struct
                {
                    internal ushort UsageMin;
                    internal ushort UsageMax;
                    internal ushort StringMin;
                    internal ushort StringMax;
                    internal ushort DesignatorMin;
                    internal ushort DesignatorMax;
                    internal ushort DataIndexMin;
                    internal ushort DataIndexMax;
                }

                internal partial struct _NotRange_e__Struct
                {
                    internal ushort Usage;
                    internal ushort Reserved1;
                    internal ushort StringIndex;
                    internal ushort Reserved2;
                    internal ushort DesignatorIndex;
                    internal ushort Reserved3;
                    internal ushort DataIndex;
                    internal ushort Reserved4;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HIDP_VALUE_CAPS
        {
            internal ushort UsagePage;
            internal byte ReportID;
            internal byte IsAlias;
            internal ushort BitField;
            internal ushort LinkCollection;
            internal ushort LinkUsage;
            internal ushort LinkUsagePage;
            internal byte IsRange;
            internal byte IsStringRange;
            internal byte IsDesignatorRange;
            internal byte IsAbsolute;
            internal byte HasNull;
            internal byte Reserved;
            internal ushort BitSize;
            internal ushort ReportCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            internal ushort[] Reserved2;
            internal uint UnitsExp;
            internal uint Units;
            internal int LogicalMin;
            internal int LogicalMax;
            internal int PhysicalMin;
            internal int PhysicalMax;
            internal _Anonymous_e__Union Anonymous;

            [StructLayout(LayoutKind.Explicit)]
            internal partial struct _Anonymous_e__Union
            {
                [FieldOffset(0)]
                internal _Range_e__Struct Range;
                [FieldOffset(0)]
                internal _NotRange_e__Struct NotRange;

                internal partial struct _Range_e__Struct
                {
                    internal ushort UsageMin;
                    internal ushort UsageMax;
                    internal ushort StringMin;
                    internal ushort StringMax;
                    internal ushort DesignatorMin;
                    internal ushort DesignatorMax;
                    internal ushort DataIndexMin;
                    internal ushort DataIndexMax;
                }

                internal partial struct _NotRange_e__Struct
                {
                    internal ushort Usage;
                    internal ushort Reserved1;
                    internal ushort StringIndex;
                    internal ushort Reserved2;
                    internal ushort DesignatorIndex;
                    internal ushort Reserved3;
                    internal ushort DataIndex;
                    internal ushort Reserved4;
                }
            }
        }
    }
}
