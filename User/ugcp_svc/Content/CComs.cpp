#include "framework.h"
#include "CComs.h"

CComs::CComs(CProfile& pProfile)
	: pProfile(pProfile)
{
}

CComs::~CComs()
{
	thread.request_stop();
	if (hPipe.valid())
	{
		CancelIoEx(hPipe.get(), nullptr);
	}
	if (thread.joinable())
	{
		thread.join();
	}
}

bool CComs::Init()
{
	char retry = 5;
	while (retry-- > 0)
	{
		hPipe.set(CreateFileW(L"\\\\.\\pipe\\LauncherPipeSvc", GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL));
		if (hPipe.valid())
		{
			DWORD mode = PIPE_READMODE_MESSAGE;
			if (SetNamedPipeHandleState(hPipe.get(), &mode, NULL, NULL))
			{
				if (WriteFile(hPipe.get(), "OK\r\n", 5, NULL, NULL))
				{
					thread = std::jthread([this](std::stop_token st) { ThreadRead(st); });
					if (thread.joinable())
					{
						return true;
					}
				}
			}
			auto release = hPipe.move();
		}
		std::this_thread::sleep_for(std::chrono::milliseconds(1000));
	}

	return false;
}

void CComs::ThreadRead(std::stop_token exit)
{
	SetThreadPriority(GetCurrentThread(), THREAD_MODE_BACKGROUND_BEGIN);

	char retries = 5;
	while (!exit.stop_requested() && (retries >= 0))
	{
		BOOL ok = FALSE;
		std::vector<std::uint8_t> msg;
		do
		{
			const size_t prevSize = msg.size();
			msg.resize(prevSize + 1024);
			DWORD tam = 0;
			ok = ReadFile(hPipe.get(), msg.data() + prevSize, 1024, &tam, nullptr);
			if (!ok && GetLastError() != ERROR_MORE_DATA)
				break;
			msg.resize(prevSize + tam);

		} while (!ok);  // repeat loop if ERROR_MORE_DATA 
		if (ok && !msg.empty())
		{
			ProcessMessage(msg);

			retries = 5;
		}
		else
		{
			retries--;
		}
	}

	if (hWndMessages)
	{
		SendNotifyMessage(hWndMessages, WM_CLOSE, 0, 0);
	}
}

bool CComs::ProcessMessage(std::span<const uint8_t> msg)
{
	switch (static_cast<MsjType>(msg[0]))
	{
		case MsjType::RawMode:
			pProfile.SetRawMode(msg[1] == 1);
			break;
		case MsjType::CalibrationMode:
			pProfile.SetCalibrationMode(msg[1] == 1);
			break;
		case MsjType::Calibration:
			pProfile.WriteCalibration(msg.subspan(1));
			break;
		case MsjType::Antiv:
			pProfile.WriteAntivibration(msg.subspan(1));
			break;
		case MsjType::Map:
			return pProfile.HF_IoWriteMap(msg.subspan(1));
		case MsjType::Commands:
		{
			return (msg.size() > 1)
				? pProfile.HF_IoWriteCommands(msg.subspan(1))
				: pProfile.HF_IoWriteCommands(msg);
		}
		default:
			break;
	}

	return true;
}
