#pragma once
#include <winusb.h>
#include <queue>
#include <span>
#include <array>

class CX52Write
{
public:
	CX52Write();
	~CX52Write();

	CX52Write(const CX52Write&) = delete;
	CX52Write& operator=(const CX52Write&) = delete;

	static CX52Write& Get() { return *pInstance; }

	void SetWinUSB(std::atomic<WINUSB_INTERFACE_HANDLE>* pawhUSB) { pwhUSB.store(pawhUSB); }

	void Light_MFD(const std::uint8_t SystemBuffer);
	void Light_Global(const std::uint8_t SystemBuffer);
	void Light_Info(const std::uint8_t SystemBuffer);
	void Set_Pinkie(const std::uint8_t SystemBuffer);
	void Set_Text(std::span<const std::uint8_t> SystemBuffer);
	void Set_Hour(const std::array<std::uint8_t, 3>& SystemBuffer);
	void Set_Hour24(const std::array<std::uint8_t, 3>& SystemBuffer);
	void Set_Date(const std::array<std::uint8_t, 2>& SystemBuffer);
private:
	USHORT Date{ 0 };

	struct ORDER
	{
		std::uint16_t value;
		std::uint8_t idx;
	};

	inline static CX52Write* pInstance{ nullptr };

	std::atomic<std::atomic<WINUSB_INTERFACE_HANDLE>*> pwhUSB { nullptr };
	std::queue<ORDER> queue;

	std::mutex mutexQueue;
	std::condition_variable evQueue;
	std::jthread threadWk;

	void SendOrder(std::uint8_t* params, std::uint8_t packets);
	void WkSend(std::stop_token exit);
};
