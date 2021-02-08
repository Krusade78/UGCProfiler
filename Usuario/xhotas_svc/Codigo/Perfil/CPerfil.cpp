#include "../framework.h"
#include "CPerfil.h"
#include "../ColaEventos/CColaEventos.h"
#include "../ProcesarSalida/CProcesarSalida.h"

CPerfil::CPerfil()
{
	hMutexPrograma = CreateMutex(NULL, FALSE, NULL);
	hMutexCalibrado = CreateMutex(NULL, FALSE, NULL);
	hMutexEstado = CreateMutex(NULL, FALSE, NULL);
	RtlZeroMemory(&perfil, sizeof(PROGRAMADO));
	RtlZeroMemory(&calibrado, sizeof(CALIBRADO));
}

CPerfil::~CPerfil()
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlZeroMemory(&calibrado, sizeof(CALIBRADO));
	CloseHandle(hMutexCalibrado);
	LimpiarPerfil();
	CloseHandle(hMutexPrograma);
	CloseHandle(hMutexEstado);
}

void CPerfil::LimpiarPerfil()
{
	WaitForSingleObject(hMutexPrograma, INFINITE);
	{
		if (pColaEv != nullptr)
		{
			static_cast<CColaEventos*>(pColaEv)->Vaciar();
		}
		if (pColaSalida != nullptr)
		{
			static_cast<CProcesarSalida*>(pColaSalida)->LimpiarEventos();
		}

		if (perfil.Acciones != nullptr)
		{
			while (!perfil.Acciones->empty())
			{
				std::deque<PEV_COMANDO>* cola = perfil.Acciones->front();
				perfil.Acciones->pop_front();
				while (!cola->empty())
				{
					delete cola->front();
					cola->pop_front();
				}
				delete cola;
			}
			delete perfil.Acciones;
		}
		RtlZeroMemory(&perfil, sizeof(PROGRAMADO));

		LockEstado();
		{
			RtlZeroMemory(&estado, sizeof(ESTADO));
		}
		UnlockEstado();
		perfilNuevoIn = true;
		perfilNuevoOut = true;
	}
	ReleaseMutex(hMutexPrograma);
}

bool  CPerfil::HF_IoEscribirMapa(BYTE* SystemBuffer, DWORD tam)
{
	if (!resetComandos)
	{
		return false;
	}
	resetComandos = false;
	if (tam == (1 + sizeof(perfil.MapaEjes) + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas)))
	{
		WaitForSingleObject(hMutexPrograma, INFINITE);
		{
			RtlCopyMemory(&perfil.TickRaton, SystemBuffer, 1);
			RtlCopyMemory(perfil.MapaBotones, SystemBuffer + 1, sizeof(perfil.MapaBotones));
			RtlCopyMemory(perfil.MapaSetas, SystemBuffer + 1 + sizeof(perfil.MapaBotones), sizeof(perfil.MapaSetas));
			RtlCopyMemory(perfil.MapaEjes, SystemBuffer + 1 + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas), sizeof(perfil.MapaEjes));
		}
		ReleaseMutex(hMutexPrograma);
	}
	else
	{
		LimpiarPerfil();
		return false;
	}

	return true;
}

bool  CPerfil::HF_IoEscribirComandos(BYTE* SystemBuffer, DWORD InputBufferLength)
{
	BYTE* bufIn;
	size_t tamPrevisto = 0;

	if (InputBufferLength != 0)
	{
		//Comprobar OK
		bufIn = SystemBuffer;
		while (tamPrevisto < InputBufferLength)
		{
			tamPrevisto += (static_cast<size_t>(*bufIn) * 2) + 1;
			bufIn += (static_cast<size_t>(*bufIn) * 2) + 1;
		}
		if (tamPrevisto != InputBufferLength)
		{
			return false;
		}
	}

	LimpiarPerfil();
	resetComandos = true;

	//GetDeviceContext(device)->MenuMFD.HoraActivada = TRUE;
	//GetDeviceContext(device)->MenuMFD.FechaActivada = TRUE;

	WaitForSingleObject(hMutexPrograma, INFINITE);
	{
		perfil.Acciones = new std::deque<std::deque<PEV_COMANDO>*>();
		bufIn = SystemBuffer;
		tamPrevisto = 0;
		while (tamPrevisto < InputBufferLength)
		{
			std::deque<PEV_COMANDO>* colaComandos = new std::deque<PEV_COMANDO>();
			UCHAR tamAccion = *bufIn;
			UCHAR i = 0;

			bufIn++;
			tamPrevisto++;

			for (i = 0; i < tamAccion; i++)
			{
				PEV_COMANDO mem = new EV_COMANDO;
				RtlZeroMemory(mem, sizeof(EV_COMANDO));
				RtlCopyMemory(mem, bufIn, 2);
				{
					if ((((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora) || (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora24))
					{
						//GetDeviceContext(device)->MenuMFD.HoraActivada = FALSE;
					}
					else if (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdFecha)
					{
						//GetDeviceContext(device)->MenuMFD.FechaActivada = FALSE;
					}
					bufIn += 2;
					tamPrevisto += 2;
					colaComandos->push_back(mem);
				}
			}
			perfil.Acciones->push_back(colaComandos);
		}
	}
	ReleaseMutex(hMutexPrograma);

	return true;
}

void CPerfil::EscribirAntivibracion(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlCopyMemory(calibrado.Jitter, datos, sizeof(calibrado.Jitter));
	ReleaseMutex(hMutexCalibrado);
}

void CPerfil::EscribirCalibrado(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlCopyMemory(calibrado.Limites, datos, sizeof(calibrado.Limites));
	ReleaseMutex(hMutexCalibrado);
}