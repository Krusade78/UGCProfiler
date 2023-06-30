#include "framework.h"
#include "CSalidaHID.h"

CSalidaHID::CSalidaHID(CPerfil* perfil, CColaEventos* colaEv, CVirtualHID* vhid)
{
    this->perfil = perfil;
    this->colaEv = colaEv;
    this->vhid = vhid;
    evSalir = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CSalidaHID::~CSalidaHID()
{
    salir = true;
    SetEvent(evSalir);
    while (InterlockedCompareExchange16(&hiloCerrado, 0, 0) == FALSE) Sleep(500);
    CloseHandle(evSalir);
    if (salida != nullptr) delete salida;
}

bool CSalidaHID::Iniciar()
{
    salida = new CProcesarSalida(perfil, vhid);

    HANDLE hilo = CreateThread(NULL, 0, HiloLectura, this, 0, NULL);
    if (hilo != NULL)
    {
        while (InterlockedCompareExchange16(&hiloCerrado, FALSE, FALSE))
        {
            Sleep(500);
        }
        return true;
    }

	return false;
}

DWORD WINAPI CSalidaHID::HiloLectura(LPVOID param)
{
    CSalidaHID* local = (CSalidaHID*)param;
    InterlockedExchange16(&local->hiloCerrado, FALSE);

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    HANDLE eventos[3]= { local->evSalir, local->colaEv->GetEvCola(), local->salida->GetEvCola()};
    while (!local->salir)
    {

        DWORD ev = WaitForMultipleObjects(3, eventos, FALSE, INFINITE);
        if ((ev - WAIT_OBJECT_0) != 0)
        {
            CPaqueteEvento* paq = nullptr;
            if ((ev - WAIT_OBJECT_0) == 1)
            {
                paq = local->colaEv->Leer();
            }
            local->salida->Procesar(paq);
        }
    }

    InterlockedExchange16(&local->hiloCerrado, TRUE);
    return 0;
}
