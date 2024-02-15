#include "framework.h"
#include "CComs.h"
#include <queue>

CComs::CComs(CProfile* pProfile)
{
	this->pProfile = pProfile;
}

CComs::~CComs()
{
	exit = true;
	if (hPipe != nullptr)
	{
		CancelIo(hPipe);
		CloseHandle(hPipe);
	}
	while (InterlockedCompareExchange16(&threadClosed, 0, 0) == FALSE) Sleep(500);
}

bool CComs::Init()
{
	char retry = 5;
	while (retry-- > 0)
	{
		hPipe = CreateFileW(L"\\\\.\\pipe\\LauncherPipeSvc", GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
		if (hPipe != INVALID_HANDLE_VALUE)
		{
			DWORD mode = PIPE_READMODE_MESSAGE;
#pragma warning( push )
#pragma warning( disable : 6001 )
			if (SetNamedPipeHandleState(hPipe, &mode, NULL, NULL))
#pragma warning( pop )
			{
				if (WriteFile(hPipe, "OK\r\n", 5, NULL, NULL))
				{
					HANDLE hilo = CreateThread(NULL, 0, ThreadRead, this, 0, NULL);
					if (hilo != NULL)
					{
						while (InterlockedCompareExchange16(&threadClosed, FALSE, FALSE))
						{
							Sleep(500);
						}
						return true;
					}
				}
			}
			CloseHandle(hPipe);
		}
		hPipe = nullptr;
		Sleep(1000);
	}

	return false;
}

DWORD WINAPI CComs::ThreadRead(LPVOID param)
{
	CComs* local = (CComs*)param;
	InterlockedExchange16(&local->threadClosed, FALSE);

	SetThreadPriority(GetCurrentThread(), THREAD_MODE_BACKGROUND_BEGIN);

	char retries = 5;
	while (!local->exit && (retries >= 0))
	{
		BOOL ok = FALSE;
		DWORD sizeMsg = 0;
		std::queue <BYTE*> msg;
		do
		{
			DWORD tam = 0;
			BYTE* buff = new BYTE[1024];
			ok = ReadFile(local->hPipe, buff, 1024, &tam, NULL);
			sizeMsg += tam;
			if (!ok && GetLastError() != ERROR_MORE_DATA)
				break;
			msg.push(buff);

		} while (!ok);  // repeat loop if ERROR_MORE_DATA 
		if (ok)
		{
			BYTE* buff = new BYTE[sizeMsg];
			DWORD pt = 0;
			while (!msg.empty())
			{
				DWORD size = ((sizeMsg - pt) >= 1024) ? 1024 : sizeMsg % 1024;
				RtlCopyMemory(&buff[pt], msg.front(), size);
				delete[] msg.front();
				msg.pop();
				pt += size;
			}
			local->ProcessMessage(buff, sizeMsg);
			delete[] buff;

			retries = 5;
		}
		else
		{
			retries--;
		}
		while (!msg.empty())
		{
			delete[] msg.front();
			msg.pop();
		}
	}

	InterlockedExchange16(&local->threadClosed, TRUE);
	SendNotifyMessage(local->hWndMessages, WM_CLOSE, 0, 0);
	return 0;
}

bool CComs::ProcessMessage(BYTE* msg, DWORD size)
{
	switch (static_cast<MsjType>(msg[0]))
	{
		case MsjType::RawMode:
			pProfile->SetRawMode(msg[1] == 1);
			break;
		case MsjType::CalibrationMode:
			pProfile->SetCalibrationMode(msg[1] == 1);
			break;
		case MsjType::Calibration:
			pProfile->WriteCalibration(&msg[1]);
			break;
		case MsjType::Antiv:
			pProfile->WriteAntivibration(&msg[1]);
			break;
		case MsjType::Map:
			return pProfile->HF_IoWriteMap(&msg[1], size - 1);
		case MsjType::Commands:
			return pProfile->HF_IoWriteCommands((size > 1) ? &msg[1] : msg, size - 1);
		default:
			break;
	}

	return true;
}
