#pragma once
#include <queue>

class CNXTWrite
{
public:
	CNXTWrite();
	~CNXTWrite();
	static CNXTWrite& Get() { return *pInstance; }

	void SetPath(const std::wstring& path);

	void SetLed(std::uint8_t* params);
private:
	typedef struct
	{
		std::uint8_t buff[4];
	} ORDER;

	inline static CNXTWrite* pInstance{ nullptr };

	std::mutex mutexQueue;
	std::mutex mutexDriver;
	PTP_WORK wkPool = nullptr;
	unique_handle hDriver{};
	std::wstring pathDriver{};
	std::queue<ORDER> queue;

	std::uint8_t hidPacket[0x81]{};

	struct
	{
		std::uint8_t Base{};
		std::uint8_t Old1[4]{};
		std::uint8_t Old2[4]{};
	} statusBaseLed;

	bool OpenDriver();
	void SendOrder(std::uint8_t* params);
	static VOID CALLBACK WkSend(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work);
	std::uint16_t CalculateCRC(std::uint8_t* block);
};

