#pragma once
#include <queue>

class CX52Write
{
public:
	CX52Write();
	~CX52Write();
	static CX52Write* Get() { return pLocal; }

	void SetWinUSB(void* ptr) { InterlockedExchangePointer(&wUSB, ptr); }

	void Light_MFD(PUCHAR SystemBuffer);
	void Light_Global(PUCHAR SystemBuffer);
	void Light_Info(PUCHAR SystemBuffer);
	void Set_Pinkie(PUCHAR SystemBuffer);
	void Set_Text(PUCHAR SystemBuffer, BYTE bufferSize);
	void Set_Hour(PUCHAR SystemBuffer);
	void Set_Hour24(PUCHAR SystemBuffer);
	void Set_Date(PUCHAR SystemBuffer);
private:
	USHORT Date = 0;

	typedef struct
	{
		USHORT value;
		BYTE idx;
	} ORDER, *PORDER;

	static CX52Write* pLocal;

	bool exit = false;
	HANDLE semQueue = nullptr;
	void* wUSB = nullptr;
	std::queue<PORDER> queue;
	HANDLE evQueue = nullptr;

	void SendOrder(UCHAR* params, BYTE packets);
	static DWORD WINAPI WkSend(LPVOID param);
};
