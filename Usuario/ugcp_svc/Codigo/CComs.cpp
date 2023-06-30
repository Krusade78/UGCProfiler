#include "framework.h"
#include "CComs.h"
#include <queue>

CComs::CComs(CPerfil* pPerfil)
{
	this->pPerfil = pPerfil;
}

CComs::~CComs()
{
	salir = true;
	if (hPipe != nullptr)
	{
		CancelIo(hPipe);
		CloseHandle(hPipe);
	}
	while (InterlockedCompareExchange16(&hiloCerrado, 0, 0) == FALSE) Sleep(500);
}

bool CComs::Iniciar()
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
					HANDLE hilo = CreateThread(NULL, 0, HiloLectura, this, 0, NULL);
					if (hilo != NULL)
					{
						while (InterlockedCompareExchange16(&hiloCerrado, FALSE, FALSE))
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

DWORD WINAPI CComs::HiloLectura(LPVOID param)
{
	CComs* local = (CComs*)param;
	InterlockedExchange16(&local->hiloCerrado, FALSE);

	SetThreadPriority(GetCurrentThread(), THREAD_MODE_BACKGROUND_BEGIN);

	char intentos = 5;
	while (!local->salir && (intentos >= 0))
	{
		BOOL ok = FALSE;
		DWORD tamMsj = 0;
		std::queue <BYTE*> msj;
		do
		{
			DWORD tam = 0;
			BYTE* buff = new BYTE[1024];
			ok = ReadFile(local->hPipe, buff, 1024, &tam, NULL);
			tamMsj += tam;
			if (!ok && GetLastError() != ERROR_MORE_DATA)
				break;
			msj.push(buff);

		} while (!ok);  // repeat loop if ERROR_MORE_DATA 
		if (ok)
		{
			BYTE* buff = new BYTE[tamMsj];
			DWORD pt = 0;
			while (!msj.empty())
			{
				DWORD tam = ((tamMsj - pt) >= 1024) ? 1024 : tamMsj % 1024;
				RtlCopyMemory(&buff[pt], msj.front(), tam);
				delete[] msj.front();
				msj.pop();
				pt += tam;
			}
			local->ProcesarMensaje(buff, tamMsj);
			delete[] buff;

			intentos = 5;
		}
		else
		{
			intentos--;
		}
		while (!msj.empty())
		{
			delete[] msj.front();
			msj.pop();
		}
	}

	InterlockedExchange16(&local->hiloCerrado, TRUE);
	SendNotifyMessage(local->hWndMensajes, WM_CLOSE, 0, 0);
	return 0;
}

bool CComs::ProcesarMensaje(BYTE* msj, DWORD tam)
{
	switch (static_cast<TipoMsj>(msj[0]))
	{
		case TipoMsj::ModoRaw:
			pPerfil->SetModoRaw(msj[1]);
			break;
		case TipoMsj::ModoCalibrado:
			pPerfil->SetModoCalibrado(msj[1]);
			break;
		case TipoMsj::Calibrado:
			pPerfil->EscribirCalibrado(&msj[1]);
			break;
		case TipoMsj::Antiv:
			pPerfil->EscribirAntivibracion(&msj[1]);
			break;
		case TipoMsj::Mapa:
			return pPerfil->HF_IoEscribirMapa(&msj[1], tam - 1);
		case TipoMsj::Comandos:
			return pPerfil->HF_IoEscribirComandos((tam > 1) ? &msj[1] : msj, tam - 1);
		default:
			break;
	}

	return true;
}
