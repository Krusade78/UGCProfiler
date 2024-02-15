#pragma once
#include <queue>

class CNXTWrite
{
public:
	CNXTWrite();
	~CNXTWrite();
	static CNXTWrite* Get() { return pLocal; }

	void SetPath(wchar_t* path);

	void SetLed(UCHAR* params);
private:
	typedef struct
	{
		PUCHAR buff;
	} ORDEN, * PORDEN;

	static CNXTWrite* pLocal;

	HANDLE semQueue = nullptr;
	HANDLE semDriver = nullptr;
	PTP_WORK wkPool = nullptr;
	HANDLE hDriver = nullptr;
	wchar_t* pathDriver = nullptr;
	std::queue<PORDEN> queue;

	UCHAR hidPacket[0x81];

	struct
	{
		UCHAR Base;
		UCHAR Old1[4];
		UCHAR Old2[4];
	} statusBaseLed;

	bool OpenDriver();
	void SendOrder(UCHAR* params);
	static VOID CALLBACK WkSend(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work);
	WORD CalculateCRC(UCHAR* block);
};

