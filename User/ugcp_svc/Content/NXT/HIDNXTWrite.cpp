#include "../framework.h"
#include "HIDNXTWrite.h"
#include <hidsdi.h>


CNXTWrite::CNXTWrite()
{
	pInstance = this;
	wkPool = CreateThreadpoolWork(WkSend, this, NULL);

	std::uint8_t packetHeader[8] = { 0x59, 0xA5, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x01 };
	memcpy(hidPacket, packetHeader, 8);
}

CNXTWrite::~CNXTWrite()
{
	pInstance = nullptr;
	{
		std::lock_guard lock(mutexQueue);
		std::queue<ORDER> empty;
		std::swap(queue, empty);
	}
	WaitForThreadpoolWorkCallbacks(wkPool, TRUE);
	CloseThreadpoolWork(wkPool);
	wkPool = nullptr;
}

void CNXTWrite::SetPath(const std::wstring& path)
{
	std::unique_lock<std::mutex> lock(mutexDriver);

	if (hDriver.valid())
	{
		unique_handle release(hDriver.move());
	}

	pathDriver = path;
	if (!path.empty())
	{
		lock.unlock();
		OpenDriver();
		return;
	}
}

bool CNXTWrite::OpenDriver()
{
	std::lock_guard lock(mutexDriver);
	{
		unique_handle release(hDriver.move());
	}
	hDriver.set(CreateFileW(pathDriver.c_str(), GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL));
	if (!hDriver.valid())
	{
		//DWORD err = GetLastError();
		return false;
	}

	return true;
}

void CNXTWrite::SetLed(std::uint8_t* params)
{
	if (params[0] == 0)
	{
		std::uint8_t mode = (params[3] >> 5) & 0x7;
		bool led2 = (mode == 0) || (mode == 5); //blue, color1
		bool led1 = (mode == 1) || (mode == 6); //red, color2
		if ((params[3] & 0b0001'1100) == 0)
		{
			if (led1) statusBaseLed.Base &= 0b0010;
			if (led2) statusBaseLed.Base &= 0b0001;
			if ((statusBaseLed.Base & 0b0011) != 0)
			{
				memcpy(params, (statusBaseLed.Base == 1) ? statusBaseLed.Old1 : statusBaseLed.Old2 , 4);
			}
		}
		else
		{
			statusBaseLed.Base |= (led1) ? 1 : 0;
			statusBaseLed.Base |= (led2) ? 2 : 0;
			if (led1) memcpy(statusBaseLed.Old1, params, 4);
			if (led2) memcpy(statusBaseLed.Old2, params, 4);
		}
		if ((statusBaseLed.Base & 0b0011) == 3)
		{
			params[1] = statusBaseLed.Old2[1];
			params[2] = statusBaseLed.Old2[2] & 0b1;
			params[2] |= statusBaseLed.Old1[2] & 0b1111'1110;
			params[3] &= 0b1111'1100;
			params[3] |= statusBaseLed.Old1[3] & 0b11;
			params[3] = (params[3] & 0b0001'1111) | (4 << 5);
		}
	}
	SendOrder(params);
}

void CNXTWrite::SendOrder(std::uint8_t* buffer)
{
	ORDER order;
	memcpy(&order, buffer, 4);
	{
		std::lock_guard lock(mutexQueue);
		queue.push(order);
	}
	SubmitThreadpoolWork(wkPool);
}

VOID CALLBACK CNXTWrite::WkSend(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work)
{
	if (Context != NULL)
	{
		CNXTWrite* local = static_cast<CNXTWrite*>(Context);
		ORDER order;
		bool process = false;
		{
			std::lock_guard lock(local->mutexQueue);
			if (!local->queue.empty())
			{
				order = local->queue.front();
				local->queue.pop();
				process = true;
			}
		}

		if (process)
		{
			memcpy(&local->hidPacket[8], order.buff, 4);
			auto crc = local->CalculateCRC(&local->hidPacket[5]);
			memcpy(&local->hidPacket[3], &crc, sizeof(crc));
			for (char i = 0; i < 2; i++)
			{
				{
					std::lock_guard lock(local->mutexDriver);
					if (local->hDriver.valid())
					{
						if (HidD_SetFeature(local->hDriver.get(), local->hidPacket, 0x81))
						{
							break;
						}
						//else
						//{
						//	DWORD err = GetLastError();
						//}
					}
				}
				if (!local->OpenDriver())
				{
					break;
				}
			}
		}
	}
}

std::uint16_t CNXTWrite::CalculateCRC(std::uint8_t* block)
{
	std::int16_t result = -1; // 0xffff;

	for (char i = 0; i < 6; i++)
	{
		std::uint8_t v5 = block[i];
		result = v5 ^ result;
		for (char j = 0; j < 8; j++)
		{		
			if ((result & 1) != 0)
			{
				result >>= 1;
				result ^= 0xA001;
			}
			else
			{
				result >>= 1;
			}
		}
	}
	return result;
}
