﻿using System;
using System.Runtime.InteropServices;

namespace API
{
	public partial class CRawInput
	{
		/// <summary>Enumeration containing flags for a raw input device.</summary>
		[Flags()]
		public enum RawInputDeviceFlags
		{
			/// <summary>No flags.</summary>
			None = 0,
			/// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
			Remove = 0x00000001,
			/// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
			Exclude = 0x00000010,
			/// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
			PageOnly = 0x00000020,
			/// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
			NoLegacy = 0x00000030,
			/// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
			InputSink = 0x00000100,
			/// <summary>If set, the mouse button click does not activate the other window.</summary>
			CaptureMouse = 0x00000200,
			/// <summary>If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
			NoHotKeys = 0x00000200,
#pragma warning restore CA1069 // Enums values should not be duplicated
			/// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
			AppKeys = 0x00000400,
			RIDEV_EXINPUTSINK = 0x00001000
			/// <summary>If set, this enables the caller to receive input in the background only if the foreground application does not process it.</summary>
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RAWINPUTDEVICE
		{
			/// <summary>Top level collection Usage page for the raw input device.</summary>
			public ushort UsagePage;
			/// <summary>Top level collection Usage for the raw input device. </summary>
			public ushort Usage;
			/// <summary>Mode flag that specifies how to interpret the information provided by UsagePage and Usage.</summary>
			public RawInputDeviceFlags Flags;
			/// <summary>Handle to the target device. If NULL, it follows the keyboard focus.</summary>
			public IntPtr WindowHandle;
		}

		//[LibraryImport("user32", SetLastError = true)]
		//      [return: MarshalAs(UnmanagedType.Bool)]
		//      public static partial bool RegisterRawInputDevices([In] RAWINPUTDEVICE[] pRawInputDevice, uint numberDevices, uint size);

		/// <summary>
		/// Value type for raw input from a HID.
		/// </summary>    
		[StructLayout(LayoutKind.Sequential)]
		public struct RAWINPUTHID
		{
			/// <summary>Size of the HID data in bytes.</summary>
			public int Size;
			/// <summary>Number of HID in Data.</summary>
			public int Count;
			///// <summary>Data for the HID.</summary>
			//public IntPtr Data;
		}

		[Flags()]
		public enum RawKeyboardFlags : ushort
		{
			/// <summary></summary>
			KeyMake = 0,
			/// <summary></summary>
			KeyBreak = 1,
			/// <summary></summary>
			KeyE0 = 2,
			/// <summary></summary>
			KeyE1 = 4,
			/// <summary></summary>
			TerminalServerSetLED = 8,
			/// <summary></summary>
			TerminalServerShadow = 0x10,
			/// <summary></summary>
			TerminalServerVKPACKET = 0x20
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct RAWINPUTKEYBOARD
		{
			public ushort MakeCode;
			public RawKeyboardFlags Flags;
			public ushort Reserved;
			public ushort VKey;
			public uint Message;
			public uint ExtraInformation;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct RAWINPUTMOUSE
		{
			[FieldOffset(0)]
			public ushort usFlags;
			[FieldOffset(4)]
			public ushort usButtonFlags;
			[FieldOffset(6)]
			public short usButtonData;
			[FieldOffset(8)]
			public uint ulRawButtons;
			[FieldOffset(12)]
			public int lLastX;
			[FieldOffset(16)]
			public int lLastY;
			[FieldOffset(20)]
			public uint ulExtraInformation;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RAWINPUTHEADER
		{
			public uint dwType;                     // Type of raw input (RIM_TYPEHID 2, RIM_TYPEKEYBOARD 1, RIM_TYPEMOUSE 0)
			public uint dwSize;                     // Size in bytes of the entire input packet of data. This includes RAWINPUT plus possible extra input reports in the RAWHID variable length array. 
			public IntPtr hDevice;                  // A handle to the device generating the raw input data. 
			public IntPtr wParam;                   // RIM_INPUT 0 if input occurred while application was in the foreground else RIM_INPUTSINK 1 if it was not.
		}

		[LibraryImport("user32.dll")]
		public static partial int GetRawInputData(IntPtr hRawInput, int uiCommand, [Out] byte[] pData, ref int pcbSize, int cbSizeHeader);

		public enum RawInputDeviceInfoCommand : uint
		{
			PreparsedData = 0x20000005,
			DeviceName = 0x20000007,
			DeviceInfo = 0x2000000b,
		}

		[LibraryImport("user32.dll", SetLastError = true)]
		public static partial uint GetRawInputDeviceInfoW(
			IntPtr hDevice,
			RawInputDeviceInfoCommand uiCommand,
			IntPtr data,
			ref uint size);
	}
}
