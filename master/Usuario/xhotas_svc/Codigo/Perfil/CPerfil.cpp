#include "../framework.h"
#include "CPerfil.h"
#include "../ColaEventos/CColaEventos.h"
#include "../ProcesarSalida/CProcesarSalida.h"
#include "../X52/EscribirUSBX52.h"
#include "../X52/MenuMFD.h"
#include "../ProcesarSalida/CVirtualHID.h"

CPerfil::CPerfil(void* pVHID)
{
	this->pVHID = pVHID;
	hMutexPrograma = CreateMutex(NULL, FALSE, NULL);
	hMutexCalibrado = CreateMutex(NULL, FALSE, NULL);
	hMutexEstado = CreateMutex(NULL, FALSE, NULL);
	RtlZeroMemory(&perfil, sizeof(PROGRAMADO));
	RtlZeroMemory(&calibrado, sizeof(CALIBRADO));
	RtlZeroMemory(&estado, sizeof(ESTADO));
	modoRaw = FALSE;
	modoCalibrado = FALSE;
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
		if (CColaEventos::Get() != nullptr)
		{
			CColaEventos::Get()->Vaciar();
		}
		if (CProcesarSalida::Get() != nullptr)
		{
			CProcesarSalida::Get()->LimpiarEventos();
		}

		if (perfil.Acciones != nullptr)
		{
			while (!perfil.Acciones->empty())
			{
				delete perfil.Acciones->front();
				perfil.Acciones->pop_front();
			}
			delete perfil.Acciones;
			perfil.Acciones = nullptr;
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
	if (tam == (17 + 1 + sizeof(perfil.MapaEjes) + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas) + sizeof(perfil.RangosEntrada) + sizeof(perfil.RangosSalida)))
	{
		BYTE txt[17];
		RtlCopyMemory(txt, SystemBuffer, 17);
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 17);
		RtlZeroMemory(txt, 17);
		txt[0] = 2;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 2);
		txt[0] = 3;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 2);
		WaitForSingleObject(hMutexPrograma, INFINITE);
		{
			RtlCopyMemory(&perfil.TickRaton, SystemBuffer + 17 , 1);
			RtlCopyMemory(perfil.MapaBotones, SystemBuffer + 17 + 1, sizeof(perfil.MapaBotones));
			RtlCopyMemory(perfil.MapaSetas, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones), sizeof(perfil.MapaSetas));
			RtlCopyMemory(perfil.MapaEjes, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas), sizeof(perfil.MapaEjes));
			RtlCopyMemory(perfil.RangosEntrada, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas) + sizeof(perfil.MapaEjes), sizeof(perfil.RangosEntrada));
			RtlCopyMemory(perfil.RangosSalida, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas) + sizeof(perfil.MapaEjes) + sizeof(perfil.RangosEntrada), sizeof(perfil.RangosSalida));
			
			for (char i = 0; i < 4; i++)
			{
				for (char j = 0; j < 8; j++ )
				{
					if (i < 3) perfil.RangosSalida[i][j] = 0x8000;
					perfil.RangosEntrada[i][j] = 0x8000;
				}
			}
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

	LimpiarPerfil();

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

	resetComandos = true;

	if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetHoraActivada(true);
	if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetFechaActivada(true);

	if (InputBufferLength == 0)
	{
		return true;
	}

	WaitForSingleObject(hMutexPrograma, INFINITE);
	{
		perfil.Acciones = new std::deque<CPaqueteEvento*>();
		bufIn = SystemBuffer;
		tamPrevisto = 0;
		while (tamPrevisto < InputBufferLength)
		{
			CPaqueteEvento* colaComandos = new CPaqueteEvento();
			UCHAR tamAccion = *bufIn;
			UCHAR i = 0;

			bufIn++;
			tamPrevisto++;

			for (i = 0; i < tamAccion; i++)
			{
				PEV_COMANDO mem = new EV_COMANDO;
				RtlZeroMemory(mem, sizeof(EV_COMANDO));
				mem->Tipo = *bufIn;
				mem->Dato = *(bufIn + 1);
				if ((((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora) || (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora24))
				{
					if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetHoraActivada(false);
				}
				else if (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdFecha)
				{
					if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetFechaActivada(false);
				}
				bufIn += 2;
				tamPrevisto += 2;
				colaComandos->AñadirComando(mem);
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