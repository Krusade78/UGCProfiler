#pragma once
#include "IHIDInput.h"
#include <vector>

class CHIDDevices : public IHIDInput
{
public:
	CHIDDevices(UINT32 hardwareId);
	~CHIDDevices();

	//IHIDInput overrides
	virtual unsigned short Read(void* buff) override;

	typedef struct
	{
		//UCHAR ReportId;
		UCHAR Bits;
		UCHAR IsButton;
		UCHAR IsHat;
		//UCHAR Skip;
		UINT16 Index;
	} ST_MAP;

	inline std::vector<ST_MAP*>* GetMap() { return &map; }
private:
	std::vector<ST_MAP*> map;

protected:
	UINT32 hardwareId = 0;
	wchar_t* pathInterface = nullptr;
	HANDLE mutex = nullptr;
	PVOID hdev = nullptr;
	PVOID preparsed = nullptr;
	PCHAR reportBuffer = nullptr;
	unsigned long reportLenght = 0;

	bool GetDeviceMap(void* preparsedData, void* pCaps);

	//IHIDInput overrides
	virtual bool Prepare() override;
	virtual bool Open() override;
	virtual void Close(bool exit) override;
};

