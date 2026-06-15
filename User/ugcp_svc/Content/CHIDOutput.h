#pragma once
#include "Profile/CProfile.h"
#include "Eventqueue/CEventQueue.h"
#include "ProcessOutput/CVirtualHID.h"
#include "ProcessOutput/CProcessOutput.h"
#include <memory>

class CHIDOutput
{
public:
	explicit CHIDOutput(CProfile& profile, CEventQueue& evQueue, CVirtualHID& vhid);
	~CHIDOutput();

	bool Init();
private:
	unique_handle evExit{};
	unique_handle threadClosed{};

	CProfile& profile;
	CEventQueue& evQueue;
	CVirtualHID& vhid;
	std::unique_ptr<CProcessOutput> output{ nullptr };

	static unsigned _stdcall ThreadRead(void* param);
};

