EXTERN_C_START

NTSTATUS IniciarX52(_In_ WDFDEVICE device);
EVT_WDF_IO_QUEUE_IO_INTERNAL_DEVICE_CONTROL EvtX52InternalIOCtl;
EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL EvtX52IOCtl;
VOID LeerX52ConPedales(PDEVICE_CONTEXT devExt);

#ifdef _PRIVATE_

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)
//#define IOCTL_PEDALES		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0108, METHOD_BUFFERED, FILE_WRITE_ACCESS)

EVT_WDF_REQUEST_COMPLETION_ROUTINE EvtCompletionConfigDescriptor;
EVT_WDF_REQUEST_COMPLETION_ROUTINE EvtCompletionX52Data;

CONST UCHAR reportDescriptor[] = {
	// Keyboard descriptor
	0x05,   0x01,       // Usage Page (Generic Desktop),
	0x09,   0x06,       // Usage (Keyboard),
	0xA1,   0x01,       // Collection (Application),
		0x85, 0x03,   // Report ID.
		0x05,   0x07,       //  Usage Page (Key Codes);
		0x19,   0x00,       //  Usage Minimum (0),
		0x29,   0xe7,       //  Usage Maximum (231),
		0x15,   0x00,       //  Logical Minimum (0),
		0x25,   0x01,       //  Logical Maximum (1),
		0x75,   0x01,       //  Report Size (1),
		0x95,   0xe8,       //  Report Count (232),
		0x81,   0x02,       //  Input (Data, Variable, Absolute),;Modifier byte
	0xC0,                // End Collection

	// Mouse Descriptor
	0x05, 0x01, // Usage Page (Generic Desktop),
	0x09, 0x02, // Usage (Mouse),
	0xA1, 0x01, // Collection (Application),
		0x85, 0x02,   // Report ID 02.
		0x09, 0x01, // Usage (Pointer),
		0xA1, 0x00, // Collection (Physical),
			0x05, 0x09, // Usage Page (Buttons),
			0x19, 0x01, // Usage Minimum (01),
			0x29, 0x03, // Usage Maximun (03),
			0x15, 0x00, // Logical Minimum (0),
			0x25, 0x01, // Logical Maximum (1),
			0x65, 0x00, //unit none
			0x75, 0x01, // Report Size (1),
			0x95, 0x03, // Report Count (3),
			0x81, 0x02, // Input (Data, Variable, Absolute), ;3 button bits
			0x95, 0x01, // Report Count (1),
			0x75, 0x05, // Report Size (5),
			0x81, 0x01, // Input (Constant), ;5 bit padding
			0x05, 0x01, // Usage Page (Generic Desktop),
			0x09, 0x30, // Usage (X),
			0x09, 0x31, // Usage (Y),
			0x09, 0x38,	//Wheel
			0x15, 0x81, // Logical Minimum (-127),
			0x25, 0x7f, // Logical Maximum (127),
			0x75, 0x08, // Report Size (8),
			0x95, 0x03, // Report Count (2),
			0x81, 0x06, // Input (Data, Variable, Relative), ;2 position bytes (X & Y)
		0xC0, // End Collection,
	0xC0, // End Collection

	0x05, 0x01,
	0x09, 0x04,  //usage joystick
	0xa1, 0x01, //collection application
		0x85, 0x01,   // Report ID.
		0x09, 0x01, //usage pointer
		0xa1, 0x00,  // collection physical
			0x09, 0x30,  //X
			0x09, 0x31,  //Y
			0x15, 0x00,   // logical min
			0x26, 0xff, 0x10, //logical max
			0x75, 0x10,  //report size
			0x95, 0x02,  //report count 2
			0x81, 0x02,
		0xc0,

		0x09, 0x35,	//Rudder(Rz)
		0x09, 0x32,  //throttle
		0x09, 0x33,  //Rx
		0x09, 0x34,  //Ry
		0x09, 0x36,  //Slider 1
		0x09, 0x36,  //Slider 2
		0x95, 0x06,  //report count 6
		0x81, 0x02,

		0x09, 0x39, //Hat1
		0x09, 0x39, //Hat2
		0x09, 0x39, //Hat3
		0x09, 0x39, //Hat4
		0x15, 0x01, //logical min 1
		0x25, 0x08, //logical max 8
		0x35, 0x00, //physical min
		0x46, 0x3b, 0x01, //physical max
		0x65, 0x14, //unit measure
		0x55, 0x00, //unit exponent
		0x75, 0x08,  //report size 8
		0x95, 0x04,  //report count
		0x81, 0x42,

		0x05,0x09,	//botones
		0x19,0x01,
		0x29,0x1a,
		0x15,0x00, //logical min 0
		0x25,0x01, //logical max 1
		0x45,0x01, //physical max 1
		0x75,0x01, //report size 1
		0x95,0x1a, //report count 26
		0x65,0x00, //unit none
		0x81,0x02,

		0x95, 0x01, // Report Count (1),
		0x75, 0x06, // Report Size (6),
		0x81, 0x01, // Input (Constant), ;6 bit padding

		0x05, 0x05, //usage page gaming controls
		0xa1, 0x00,  // collection physical
			0x09, 0x24, //Move right/left
			0x09, 0x26, //Move up/down
			0x25, 0x0f, // logical max 15
			0x45, 0x0f, //physical max 15
			0x75, 0x04, //report size 4
			0x95, 0x02,  //report count 2
			//0x66,0x11,0xf0, //unit velicity
			0x81, 0x02,
		0xc0,
	0xc0
};
#endif

EXTERN_C_END

