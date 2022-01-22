#pragma once
typedef struct
{
	UINT16  Ejes[8]; //X,Y,Z,Rx,Ry,Rz,Sl1,Sl2
	UCHAR	Setas[4];
	UCHAR	Botones[4];
} VHID_INPUT_DATA, *PVHID_INPUT_DATA;

//
// This is the default report descriptor for the virtual Hid device returned
// by the mini driver in response to IOCTL_HID_GET_REPORT_DESCRIPTOR.
//
static const UCHAR ReportDescriptor0[] =
{
    // Keyboard(STD101)
    0x05,   0x01,       // Usage Page(Generic Desktop),
    0x09,   0x06,       // Usage(Keyboard),
    0xA1,   0x01,       // Collection(Application),
        0x85,   0x01,       //  Report Id(1)
        0x05,   0x07,       //  usage page key codes
        0x19,   0x00,       //  Usage Minimum (0),
        0x29,   0xe7,       //  Usage Maximum (231),
        0x15,   0x00,       //  Logical Minimum (0),
        0x25,   0x01,       //  Logical Maximum (1),
        0x75,   0x01,       //  Report Size (1),
        0x95,   0xe8,       //  Report Count (232),
        0x81,   0x02,       //  input(Variable)
    0xC0,				// End Collection,

    // Mouse
    0x05, 0x01,                    // USAGE_PAGE (Generic Desktop)
    0x09, 0x02,                    // USAGE (Mouse)
    0xa1, 0x01,                    // COLLECTION (Application)
        0x85, 0x02,                    //  Report Id(2)
        0x09, 0x01,                    //   USAGE (Pointer)
        0xa1, 0x00,                    //   COLLECTION (Physical)
            0x05, 0x09,                    //     USAGE_PAGE (Button)
            0x19, 0x01,                    //     USAGE_MINIMUM (Button 1)
            0x29, 0x03,                    //     USAGE_MAXIMUM (Button 3)
            0x15, 0x00,                    //     LOGICAL_MINIMUM (0)
            0x25, 0x01,                    //     LOGICAL_MAXIMUM (1)
            0x65, 0x00,              //     Unit(None)
            0x95, 0x03,                    //     REPORT_COUNT (3)
            0x75, 0x01,                    //     REPORT_SIZE (1)
            0x81, 0x02,                    //     INPUT (Data,Var,Abs)
            0x95, 0x01,                    //     REPORT_COUNT (1)
            0x75, 0x05,                    //     REPORT_SIZE (5)
            0x81, 0x03,                    //     INPUT (Cnst,Var,Abs)
            0x05, 0x01,                    //     USAGE_PAGE (Generic Desktop)
            0x09, 0x30,                    //     USAGE (X)
            0x09, 0x31,                    //     USAGE (Y)
            0x09, 0x38,	//Wheel
            0x15, 0x81, // Logical Minimum (-127),
            0x25, 0x7f, // Logical Maximum (127),
            0x35, 0x81,               //     Physical Minimum
            0x45, 0x7F,              //     Physical Maximum
            0x75, 0x08,                    //     Report Size(8)
            0x95, 0x03,                    //     Report Count(3)
            0x81, 0x06, // Input (Data, Variable, Relative), ;2 position bytes (X & Y)
        0xc0,                          //   END_COLLECTION
    0xc0,                          // END_COLLECTION
};
static const UCHAR ReportDescriptor1[] = {
    0x05, 0x01,
    0x09, 0x04,  //usage joystick
    0xa1, 0x01, //collection application
        0x85, 0x03,   // Report ID.
        0x16, 0x00, 0x00, // logical min
        //0x36, 0x01, 0x80, // Physical min
        //0x46, 0xff, 0x7f, //Physical max
        0x09, 0x01, //usage pointer
        0xa1, 0x00,  // collection physical
            0x75, 0x10,  //report size
            0x95, 0x01,  //report count 1
            0x26, 0xff, 0x7f, //logical max
            0x09, 0x30,  //X
            0x81, 0x02,
            0x26, 0xff, 0x7f, //logical max
            0x09, 0x31,  //Y
            0x81, 0x02,
        0xc0,

        0x95, 0x01,  //report count 1
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x32,  //throttle
        0x81, 0x02,
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x33,  //Rx
        0x81, 0x02,
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x34,  //Ry
        0x81, 0x02,
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x35,  //Rz
        0x81, 0x02,
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x36,  //Slider 1
        0x81, 0x02,
        0x16, 0x01, 0x80, // logical min
        0x26, 0xff, 0x7f, //logical max
        0x09, 0x36,  //Slider 2
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
        0x95,0x20, //report count 32
        0x65,0x00, //unit none
        0x81,0x02,
    0xc0
};

constexpr auto EJE_X_MAX = 20;
constexpr auto EJE_Y_MAX = 27;
constexpr auto EJE_Z_MAX = 37;
constexpr auto EJE_RX_MAX = 44;
constexpr auto EJE_RY_MAX = 51;
constexpr auto EJE_RZ_MAX = 58;
constexpr auto EJE_SL1_MAX = 65;
constexpr auto EJE_SL2_MAX = 72;

class CVirtualHID
{
public :
	CVirtualHID();
	~CVirtualHID();
	struct
	{
		struct
		{
			UCHAR Botones;
			CHAR X;
			CHAR Y;
			CHAR Rueda;
		} Raton;
		UCHAR Teclado[29];
		VHID_INPUT_DATA	DirectX[3];
	} Estado;

	bool Iniciar();
	void EnviarRequestTeclado();
	void EnviarRequestRaton(BYTE* inputData);
	void EnviarRequestJoystick(UCHAR joyId, PVHID_INPUT_DATA inputData);

    bool EnviarReportDescriptor(void* pPerfil);

	void LockRaton() { WaitForSingleObject(hMutextRaton, INFINITE); }
	void UnlockRaton() { ReleaseSemaphore(hMutextRaton, 1, NULL); }
private:
	HANDLE hVHid = nullptr;
	HANDLE hMutextRaton = nullptr;
    bool reportOk = false;
};

