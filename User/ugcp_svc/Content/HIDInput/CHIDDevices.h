#pragma once
#include "IHIDInput.h"
#include <vector>
#include <mutex>
#include <hidsdi.h>

class CHIDDevices : public IHIDInput
{
private:
	typedef struct
	{
		//UCHAR ReportId;
		std::uint8_t Bits;
		std::uint8_t IsButton;
		std::uint8_t IsHat;
		//UCHAR Skip;
		std::uint16_t Index;
	} ST_MAP;
	std::vector<ST_MAP> map;
public:
	CHIDDevices(std::uint32_t hardwareId);
	~CHIDDevices();

	//IHIDInput overrides
	virtual unsigned short Read(void* buff) override;

	const std::vector<ST_MAP>& GetMap() const { return map; }
protected:
	std::uint32_t hardwareId{ 0 };
	std::wstring pathInterface{};
	std::mutex mutex;
	unique_handle hdev{};
	PHIDP_PREPARSED_DATA preparsed{ nullptr };
	std::unique_ptr<CHAR[]> reportBuffer{};
	std::atomic<DWORD> reportLenght{ 0 };

	bool GetDeviceMap(PHIDP_PREPARSED_DATA pData, PHIDP_CAPS pCaps);

	//IHIDInput overrides
	virtual bool Prepare() override;
	virtual bool Open() override;
	virtual void Close(bool exit) override;
};

