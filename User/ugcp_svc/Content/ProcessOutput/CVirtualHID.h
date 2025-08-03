#pragma once
typedef struct
{
	UINT16  Axes[15]; //X,Y,Z,Rx,Ry,Rz,Sl1,Sl2,Sl3,X,Y,Z,Rx,Ry,Rz
	UCHAR	Hats[4];
	UCHAR	Buttons[16];
} VHID_INPUT_DATA, *PVHID_INPUT_DATA;

class CVirtualHID
{
private:
	typedef struct
	{
		struct
		{
			UCHAR Buttons;
			CHAR X;
			CHAR Y;
			CHAR Wheel;
		} Mouse;
		UCHAR Keyboard[29];
		VHID_INPUT_DATA	DirectX[3];
	} ST_STATUS;
public:
	CVirtualHID();
	~CVirtualHID();

	bool Init();
	ST_STATUS* GetStatus() { return &status; }
	void SendRequestToJoystick(UCHAR joyId);

	void LockMouse() const { WaitForSingleObject(hMutextMouse, INFINITE); }
	void UnlockMouse() const { ReleaseSemaphore(hMutextMouse, 1, NULL); }
private:
	HANDLE hMutextMouse = nullptr;
	bool initialized = false;

	bool available[16];

	ST_STATUS status;

	DWORD Hat2Switch(UCHAR pos);
};

