#pragma once
typedef struct
{
	UINT16  Ejes[8]; //X,Y,Z,Rx,Ry,Rz,Sl1,Sl2
	UCHAR	Setas[4];
	UCHAR	Botones[4];
} VHID_INPUT_DATA, *PVHID_INPUT_DATA;

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
	void EnviarRequestJoystick(UCHAR joyId, PVHID_INPUT_DATA inputData);

	void LockRaton() { WaitForSingleObject(hMutextRaton, INFINITE); }
	void UnlockRaton() { ReleaseSemaphore(hMutextRaton, 1, NULL); }
private:
	HANDLE hMutextRaton = nullptr;

	DWORD Hat2Switch(UCHAR pos);
};

