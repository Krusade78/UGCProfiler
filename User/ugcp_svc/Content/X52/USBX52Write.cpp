#include "../framework.h"
#include "USBX52Write.h"
#include <thread>

CX52Write::CX52Write()
{
	pInstance = this;
	threadWk = std::jthread([this](std::stop_token st) { CX52Write::WkSend(st); });
}

CX52Write::~CX52Write()
{
	pInstance = nullptr;

	threadWk.request_stop();
	evQueue.notify_all();
}

#pragma region "Orders"
void CX52Write::Light_MFD(const std::uint8_t SystemBuffer)
{
	std::uint8_t params[3] = { SystemBuffer ,0 , 0xb1};
	SendOrder(params, 1);
}

void CX52Write::Light_Global(const std::uint8_t SystemBuffer)
{
	std::uint8_t params[3] = { SystemBuffer, 0 ,0xb2 };
	SendOrder(params, 1);
}

void CX52Write::Light_Info(const std::uint8_t SystemBuffer)
{
	std::uint8_t params[3] = { std::uint8_t(SystemBuffer + 0x50),0 , 0xb4 };
	SendOrder(params, 1);
}

void CX52Write::Set_Pinkie(const std::uint8_t SystemBuffer)
{
	std::uint8_t params[3] = { std::uint8_t(SystemBuffer + 0x50), 0 , 0xfd };
	SendOrder(params, 1);
}

void CX52Write::Set_Text(std::span<const std::uint8_t> SystemBuffer)
{
	std::uint8_t params[3 * 17]{};
	auto text = SystemBuffer.subspan(1);
	std::uint8_t nparams = 1;
	std::uint8_t paramIdx = 0;

	if ((SystemBuffer.size() - 1) > 16)
		return;

	params[0] = 0x0; params[1] = 0;
	switch (SystemBuffer[0]) //line
	{
	case 1:
		params[2] = 0xd9;
		paramIdx = 0xd1;
		break;
	case 2:
		params[2] = 0xda;
		paramIdx = 0xd2;
		break;
	case 3:
		params[2] = 0xdc;
		paramIdx = 0xd4;
	}
	for (char i = 0; i < 16; i += 2)
	{
		if (text[i] == 0)
			break;
		params[0 + (3 * nparams)] = text[i];
		params[1 + (3 * nparams)] = text[i + 1];
		params[2 + (3 * nparams)] = paramIdx;
		nparams++;
	}

	SendOrder(params, nparams);
}

void CX52Write::Set_Hour(const std::array<std::uint8_t, 3>& SystemBuffer)
{
	std::uint8_t params[3] = { (SystemBuffer)[2] , (SystemBuffer)[1], std::uint8_t(SystemBuffer[0] + 0xbf) };
	SendOrder(params, 1);
}

void CX52Write::Set_Hour24(const std::array<std::uint8_t, 3>& SystemBuffer)
{
	std::uint8_t params[3] = { SystemBuffer[2] , std::uint8_t(SystemBuffer[1] + 0x80), std::uint8_t(SystemBuffer[0] + 0xbf) };
	SendOrder(params, 1);
}

void CX52Write::Set_Date(const std::array<std::uint8_t, 2>& SystemBuffer)
{
	std::uint8_t params[3] = { 0, 0, 0 };

	switch (SystemBuffer[0])
	{
	case 1:
		params[2] = 0xc4;
		params[1] = static_cast<std::uint8_t>(Date >> 8);
		params[0] = SystemBuffer[1];
		Date = *(reinterpret_cast<std::uint16_t*>(params));
		break;
	case 2:
		params[2] = 0xc4;
		params[1] = SystemBuffer[1];
		params[0] = static_cast<std::uint8_t>(Date & 0xff);
		Date = *(reinterpret_cast<std::uint16_t*>(params));
		break;
	case 3:
		params[2] = 0xc8;
		params[1] = 0;
		params[0] = SystemBuffer[1];
	}
	SendOrder(params, 1);
}
#pragma endregion

void CX52Write::SendOrder(std::uint8_t* buffer, std::uint8_t packets)
{
	for (std::uint8_t processed = 0; processed < packets; processed++)
	{
		ORDER order{
			.value = *(reinterpret_cast<std::uint16_t*>(&buffer[processed * 3])),
			.idx = buffer[2 + (processed * 3)]
		};
		{
			std::lock_guard<std::mutex> lock(mutexQueue);
			queue.push(order);
		}
		evQueue.notify_one();
	}
}

void CX52Write::WkSend(std::stop_token exit)
{	
	while (true)
	{
		ORDER order;
		{
			std::unique_lock<std::mutex> lock(mutexQueue);
			evQueue.wait(lock, [&] { return exit.stop_requested() || !queue.empty(); });

			if (exit.stop_requested())
			{
				break;
			}

			order = queue.front();
			queue.pop();
		}

		if (!exit.stop_requested())
		{
			WINUSB_SETUP_PACKET controlSetupPacket
			{
				controlSetupPacket.RequestType = 0b01000000,
				controlSetupPacket.Request = 0x91, // Request
				controlSetupPacket.Value = order.value, // Value
				controlSetupPacket.Index = order.idx, // Index  
				controlSetupPacket.Length = 0
			};

			auto pUSB = pwhUSB.load();
			if (pUSB != nullptr)
			{
				auto hUSB = pUSB->load();
				if (hUSB != nullptr)
				{
					WinUsb_ControlTransfer(hUSB, controlSetupPacket, nullptr, 0, nullptr, nullptr);
				}
			}
		}
	}
}