#pragma once
#include "IHIDInput.h"
#include <vector>

class CHIDDevices : public IHIDInput
{
public:
	CHIDDevices(UINT32 hardwareId);
	~CHIDDevices();

	//IHIDInput overrides
	virtual bool Prepare() override;
	virtual bool Open() override;
	virtual void Close() override;
	virtual unsigned short Read(void* buff) override;

	typedef struct
	{
		UCHAR ReportId;
		UCHAR Bits;
		UCHAR IsButton;
		UCHAR Skip;
		UINT16 Index;
	} ST_MAP;

	inline std::vector<ST_MAP*>* GetMap() { return &map; }
private:
	UINT32 hardwareId = 0;

	std::vector<ST_MAP*> map;

protected:
	wchar_t* pathInterface = nullptr;
	HANDLE mutex = nullptr;
	PVOID hdev = nullptr;
	unsigned long reportLenght = 0;

	bool GetDeviceMap(void* preparsedData, void* pCaps);
};

