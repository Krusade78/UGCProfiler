#pragma once
#include "Profile/CProfile.h"
#include "Eventqueue/CEventQueue.h"
#include "ProcessOutput/CVirtualHID.h"
#include "ProcessOutput/CProcessOutput.h"

class CHIDOutput
{
public:
	CHIDOutput(CProfile* profile, CEventQueue* evQueue, CVirtualHID* vhid);
	~CHIDOutput();

	bool Init();
private:
	HANDLE evExit = nullptr;
	bool exit = false;
	short threadClosed = TRUE;

	CProfile* profile = nullptr;
	CEventQueue* evQueue = nullptr;
	CVirtualHID* vhid = nullptr;
	CProcessOutput* output = nullptr;

	static DWORD WINAPI ThreadRead(LPVOID param);
};

