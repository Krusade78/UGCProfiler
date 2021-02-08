/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

MenuMDF.c

Abstract:

Crea el menu de configuración en el MFD del X52.

IRQL:

Todas las funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include <ntstrsafe.h>
#include "context.h"
#define _PUBLIC_
#include "EscribirUSBX52.h"
#define _PRIVATE_
#include "MenuMFD.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, LeerConfiguracion)
#pragma alloc_text (PAGE, GuardarConfiguracion)
#pragma alloc_text (PAGE, EvtTickMenu)
#pragma alloc_text (PAGE, EvtTickHora)
#pragma alloc_text (PAGE, MenuPulsarBoton)
#pragma alloc_text (PAGE, MenuSoltarBoton)
#pragma alloc_text (PAGE, CambiarEstado)
#pragma alloc_text (PAGE, MenuCerrar)
#pragma alloc_text (PAGE, VerPantalla1)
#pragma alloc_text (PAGE, VerPantallaOnOff)
#pragma alloc_text (PAGE, VerPantallaLuz)
#pragma alloc_text (PAGE, VerPantallaHora)
#endif

#pragma region "Funciones públicas"
VOID LeerConfiguracion(WDFDEVICE device)
{
    PAGED_CODE(); 

    WDFKEY key;
    ULONG valor;
    DECLARE_CONST_UNICODE_STRING(stPedales, L"Pedales");
    DECLARE_CONST_UNICODE_STRING(stLuzGlobal, L"LuzGlobal");
    DECLARE_CONST_UNICODE_STRING(stLuzMFD, L"LuzMFD");
    DECLARE_CONST_UNICODE_STRING(stHora1, L"Hora1");
    DECLARE_CONST_UNICODE_STRING(stHora1_24, L"Hora1_24");
    DECLARE_CONST_UNICODE_STRING(stHora2, L"Hora2");
    DECLARE_CONST_UNICODE_STRING(stHora2_24, L"Hora2_24");
    DECLARE_CONST_UNICODE_STRING(stHora3, L"Hora3");
    DECLARE_CONST_UNICODE_STRING(stHora3_24, L"Hora3_24");

    if (NT_SUCCESS(WdfDriverOpenParametersRegistryKey(WdfGetDriver(), KEY_READ, WDF_NO_OBJECT_ATTRIBUTES, &key)))
    {
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stPedales, &valor)))
        {
            GetDeviceContext(device)->Pedales.Activado = (BOOLEAN)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stLuzGlobal, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.LuzGlobal = (UCHAR)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stLuzMFD, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.LuzMFD = (UCHAR)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora1, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[0].Minutos = (SHORT)(LONG)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora2, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[1].Minutos = (SHORT)(LONG)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora3, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[2].Minutos = (SHORT)(LONG)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora1_24, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[0]._24h = (BOOLEAN)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora2_24, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[1]._24h = (BOOLEAN)valor;
        }
        if (NT_SUCCESS(WdfRegistryQueryULong(key, &stHora3_24, &valor)))
        {
            GetDeviceContext(device)->MenuMFD.Hora[2]._24h = (BOOLEAN)valor;
        }
        WdfRegistryClose(key);
    }
}

VOID EvtTickMenu(WDFTIMER timer)
{
	PAGED_CODE();

	UCHAR param = 1;

	WdfTimerStop(timer, FALSE);

	GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.Activado = TRUE;
	GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.TimerEsperando = FALSE;
	Luz_Info(WdfTimerGetParentObject(timer), &param);
    param = 2;
    Luz_MFD(WdfTimerGetParentObject(timer), &param);
    GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.EstadoBotones = 0;
    GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.EstadoCursor = 0;
    GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.EstadoPagina = 0;
    VerPantalla1(WdfTimerGetParentObject(timer));
}

VOID MenuPulsarBoton(WDFDEVICE device, UCHAR boton)
{
	PAGED_CODE();

	if (boton == BotonIntro)
	{
		if (!GetDeviceContext(device)->MenuMFD.TimerEsperando && !GetDeviceContext(device)->MenuMFD.Activado)
		{
			GetDeviceContext(device)->MenuMFD.TimerEsperando = TRUE;
			WdfTimerStart(GetDeviceContext(device)->MenuMFD.Timer, WDF_REL_TIMEOUT_IN_SEC(3));
		}
	}
	if (GetDeviceContext(device)->MenuMFD.Activado)
	{
		GetDeviceContext(device)->MenuMFD.EstadoBotones |= 1 << boton;
	}

}

VOID MenuSoltarBoton(WDFDEVICE device, UCHAR boton)
{
    PAGED_CODE();

    if (boton == BotonIntro)
	{
		if (GetDeviceContext(device)->MenuMFD.TimerEsperando && !GetDeviceContext(device)->MenuMFD.Activado)
		{
			GetDeviceContext(device)->MenuMFD.TimerEsperando = FALSE;
			WdfTimerStop(GetDeviceContext(device)->MenuMFD.Timer, FALSE);
		}
	}
	if (GetDeviceContext(device)->MenuMFD.Activado)
	{
		if ((GetDeviceContext(device)->MenuMFD.EstadoBotones >> boton) & 1)
		{
			CambiarEstado(device, boton);
		}
		GetDeviceContext(device)->MenuMFD.EstadoBotones &= ~((UCHAR)(1 << boton));
	}
}

VOID EvtTickHora(WDFTIMER timer)
{
    PAGED_CODE();
    LARGE_INTEGER ahoraGMT, ahora;
    TIME_FIELDS campos;
    UCHAR bf[3] = { 0, 0, 0 };
    KeQuerySystemTime(&ahoraGMT);
    ExSystemTimeToLocalTime(&ahoraGMT, &ahora);
    RtlTimeToTimeFields(&ahora, &campos);

    if (GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.FechaActivada)
    {
        bf[0] = 1; bf[1] = (UCHAR)campos.Day;
        Set_Fecha(WdfTimerGetParentObject(timer), bf);
        bf[0] = 2; bf[1] = (UCHAR)campos.Month;
        Set_Fecha(WdfTimerGetParentObject(timer), bf);
        bf[0] = 3; bf[1] = (UCHAR)(campos.Year % 100);
        Set_Fecha(WdfTimerGetParentObject(timer), bf);
    }

    if (GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.HoraActivada)
    {
        LONG hora = GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.Hora[0].Minutos * 60;
        ULONG absHora = ((hora < 0) ? -1 : 1) * hora;
        ahora.QuadPart += ((hora < 0) ? -1 : 1) * WDF_REL_TIMEOUT_IN_SEC(absHora);
        RtlTimeToTimeFields(&ahora, &campos);
        bf[0] = 1;
        bf[1] = (UCHAR)campos.Hour;
        bf[2] = (UCHAR)campos.Minute;
        if (GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.Hora[0]._24h)
        {
            Set_Hora24(WdfTimerGetParentObject(timer), bf);
        }
        else
        {
            Set_Hora(WdfTimerGetParentObject(timer), bf);
        }
    }

    WdfTimerStart(GetDeviceContext(WdfTimerGetParentObject(timer))->MenuMFD.TimerHora, WDF_REL_TIMEOUT_IN_MS(4000));
}
#pragma endregion

VOID CambiarEstado(WDFDEVICE device, UCHAR boton)
{
	PAGED_CODE();

	MENU_MFD_CONTEXT* mfd = &GetDeviceContext(device)->MenuMFD;
    BOOLEAN ok = FALSE;

	#pragma region "Boton intro"
    if (boton == BotonIntro)
    {
        switch (mfd->EstadoPagina)
        {
            case 0: //principal
                switch (mfd->EstadoCursor)
                {
                    case 0:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 1;
                        ok = VerPantallaOnOff(device);
                        break;
                    case 1:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 2;
                        ok = VerPantallaLuz(device, mfd->LuzGlobal);
                        break;
                    case 2:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 3;
                        ok = VerPantallaLuz(device, mfd->LuzMFD);
                        break;
                    case 3:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 4;
                        ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[0].Minutos / 60), (((mfd->Hora[0].Minutos < 0) ? -1 : 1) * mfd->Hora[0].Minutos) % 60, mfd->Hora[0]._24h);
                        break;
                    case 4:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 5;
                        ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[1].Minutos / 60), (((mfd->Hora[1].Minutos < 0) ? -1 : 1) * mfd->Hora[1].Minutos) % 60, mfd->Hora[1]._24h);
                        break;
                    case 5:
                        mfd->EstadoCursor = 0;
                        mfd->EstadoPagina = 6;
                        ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[0].Minutos / 60), (((mfd->Hora[2].Minutos < 0) ? -1 : 1) * mfd->Hora[2].Minutos) % 60, mfd->Hora[2]._24h);
                        break;
                    case 6:
                        MenuCerrar(device);
                        break;
                }
                break;
            case 1: //pedales
                GetDeviceContext(device)->Pedales.Activado = (mfd->EstadoCursor == 0);
                mfd->EstadoCursor = 0;
                mfd->EstadoPagina = 0;
                ok = VerPantalla1(device);
                break;
            case 2: // luz global
                mfd->LuzGlobal = mfd->EstadoCursor;
                Luz_Global(device, &mfd->LuzGlobal);
                mfd->EstadoCursor = 1;
                mfd->EstadoPagina = 0;
                ok = VerPantalla1(device);
                break;
            case 3: //luz mfd
                mfd->LuzMFD = mfd->EstadoCursor;
                mfd->EstadoCursor = 2;
                mfd->EstadoPagina = 0;
                ok = VerPantalla1(device);
                break;
            case 4: //hora 1
            case 5: //hora 2
            case 6: //hora 3
                if (mfd->EstadoCursor == 3)
                {
                    mfd->EstadoCursor = mfd->EstadoPagina - 1;
                    mfd->EstadoPagina = 0;
                    ok = VerPantalla1(device);
                }
                else
                {
                    mfd->EstadoPagina = (mfd->EstadoPagina * 10 + mfd->EstadoCursor);
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[0].Minutos / 60), (((mfd->Hora[0].Minutos < 0) ? -1 : 1) * mfd->Hora[0].Minutos) % 60, mfd->Hora[0]._24h);
                }
                break;
            case 40:
            case 41:
            case 42:
                mfd->EstadoPagina = 4;
                ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[0].Minutos / 60), (((mfd->Hora[0].Minutos < 0) ? -1 : 1) * mfd->Hora[0].Minutos) % 60, mfd->Hora[0]._24h);
                break;
            case 50:
            case 51:
            case 52:
                {
                    UCHAR buffer[3] = { 2, 0, 0 };
                    if (mfd->Hora[1].Minutos < 0)
                    {
                        buffer[1] = (UCHAR)(((-mfd->Hora[1].Minutos) >> 8) + 4);
                        buffer[2] = (UCHAR)((-mfd->Hora[1].Minutos) & 0xff);
                    }
                    else
                    {
                        buffer[1] = (UCHAR)(mfd->Hora[1].Minutos >> 8);
                        buffer[2] = (UCHAR)(mfd->Hora[1].Minutos & 0xff);
                    }
                    if (mfd->Hora[1]._24h)
                    {
                        Set_Hora24(device, buffer);
                    }
                    else
                    {
                        Set_Hora24(device, buffer);
                    }

                    mfd->EstadoPagina = 5;
                    ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[1].Minutos / 60), (((mfd->Hora[1].Minutos < 0) ? -1 : 1) * mfd->Hora[1].Minutos) % 60, mfd->Hora[1]._24h);
                }
                break;
            case 60:
            case 61:
            case 62:
                {
                    UCHAR buffer[3] = { 3, 0, 0 };
                    if (mfd->Hora[2].Minutos < 0)
                    {
                        buffer[1] = (UCHAR)(((-mfd->Hora[2].Minutos) >> 8) + 4);
                        buffer[2] = (UCHAR)((-mfd->Hora[2].Minutos) & 0xff);
                    }
                    else
                    {
                        buffer[1] = (UCHAR)(mfd->Hora[2].Minutos >> 8);
                        buffer[2] = (UCHAR)(mfd->Hora[2].Minutos & 0xff);
                    }
                    if (mfd->Hora[2]._24h)
                    {
                        Set_Hora24(device, buffer);
                    }
                    else
                    {
                        Set_Hora24(device, buffer);
                    }
                    mfd->EstadoPagina = 6;
                    ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[2].Minutos / 60), (((mfd->Hora[2].Minutos < 0) ? -1 : 1) * mfd->Hora[2].Minutos) % 60, mfd->Hora[2]._24h);
                }
                break;
        }
    }
    #pragma endregion

    #pragma region "botón arriba"
    else if (boton == BotonArriba)
    {
        switch (mfd->EstadoPagina)
        {
            case 0:
                if (mfd->EstadoCursor != 0)
                {
                    mfd->EstadoCursor--;
                }
                ok = VerPantalla1(device);
                break;
            case 1:
                if (mfd->EstadoCursor != 0)
                {
                    mfd->EstadoCursor--;
                }
                ok = VerPantallaOnOff(device);
                break;
            case 2:
            case 3:
                if (mfd->EstadoCursor != 0)
                {
                    mfd->EstadoCursor--;
                }
                ok = VerPantallaLuz(device, mfd->EstadoCursor);
                break;
            case 4:
            case 5:
            case 6:
                if (mfd->EstadoCursor != 0)
                {
                    mfd->EstadoCursor--;
                }
                ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[mfd->EstadoPagina - 4].Minutos / 60), (((mfd->Hora[mfd->EstadoPagina - 4].Minutos < 0) ? -1 : 1) * mfd->Hora[mfd->EstadoPagina - 4].Minutos) % 60, mfd->Hora[mfd->EstadoPagina - 4]._24h);
                break;
            case 40:
            case 50:
            case 60:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    if ((mfd->Hora[0].Minutos / 60) != 23)
                    {
                        mfd->Hora[idx].Minutos += 60;
                    }
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
            case 41:
            case 51:
            case 61:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    if ((mfd->Hora[0].Minutos % 60) != 59)
                    {
                        mfd->Hora[idx].Minutos++;
                    }
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
            case 42:
            case 52:
            case 62:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    mfd->Hora[idx]._24h = !mfd->Hora[idx]._24h;
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
        }
    }
    #pragma endregion

    #pragma region "botón abajo"
    else if (boton == BotonAbajo)
    {
		switch (mfd->EstadoPagina)
		{
            case 0:
                if (mfd->EstadoCursor != 6)
                {
                    mfd->EstadoCursor++;
                }
                ok = VerPantalla1(device);
                break;
            case 1:
                if (mfd->EstadoCursor != 1)
                {
                    mfd->EstadoCursor++;
                }
                ok = VerPantallaOnOff(device);
                break;
            case 2:
            case 3:
                if (mfd->EstadoCursor != 2)
                {
                    mfd->EstadoCursor++;
                }
                ok = VerPantallaLuz(device, (mfd->EstadoPagina == 2) ? mfd->LuzGlobal :  mfd->LuzMFD);
                break;
            case 4:
            case 5:
            case 6:
                if (mfd->EstadoCursor != 3)
                {
                    mfd->EstadoCursor++;
                }
                ok = VerPantallaHora(device, FALSE, (CHAR)(mfd->Hora[mfd->EstadoPagina - 4].Minutos / 60), (((mfd->Hora[mfd->EstadoPagina - 4].Minutos < 0) ? -1 : 1) * mfd->Hora[mfd->EstadoPagina - 4].Minutos) % 60, mfd->Hora[mfd->EstadoPagina - 4]._24h);
                break;
            case 40:
            case 50:
            case 60:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    if ((mfd->Hora[0].Minutos / 60) != -23)
                    {
                        mfd->Hora[idx].Minutos -= 60;
                    }
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
            case 41:
            case 51:
            case 61:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    if ((mfd->Hora[0].Minutos % 60) != 0)
                    {
                        mfd->Hora[idx].Minutos--;
                    }
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
            case 42:
            case 52:
            case 62:
                {
                    CHAR idx = (mfd->EstadoPagina / 10) - 4;
                    mfd->Hora[idx]._24h = !mfd->Hora[idx]._24h;
                    ok = VerPantallaHora(device, TRUE, (CHAR)(mfd->Hora[idx].Minutos / 60), (((mfd->Hora[idx].Minutos < 0) ? -1 : 1) * mfd->Hora[idx].Minutos) % 60, mfd->Hora[idx]._24h);
                }
                break;
        }
    }
    #pragma endregion

    if (!ok)
    {
        MenuCerrar(device);
    }
}

VOID MenuCerrar(WDFDEVICE device)
{
    PAGED_CODE();

    //Limpiar pantalla
    UCHAR fila[2] = { 1, 0 };
    Set_Texto(device, fila, 2);
    fila[0] = 2;
    Set_Texto(device, fila, 2);
    fila[0] = 3;
    Set_Texto(device, fila, 2);

    //Apagar luz
    fila[0] = 0;
    Luz_Info(device, &fila[0]);

    Luz_MFD(device, &GetDeviceContext(device)->MenuMFD.LuzMFD);

    GuardarConfiguracion(device);
    GetDeviceContext(device)->MenuMFD.Activado = FALSE;
}

VOID GuardarConfiguracion(WDFDEVICE device)
{
    PAGED_CODE();

    WDFKEY key;
    DECLARE_CONST_UNICODE_STRING(stPedales, L"Pedales");
    DECLARE_CONST_UNICODE_STRING(stLuzGlobal, L"LuzGlobal");
    DECLARE_CONST_UNICODE_STRING(stLuzMFD, L"LuzMFD");
    DECLARE_CONST_UNICODE_STRING(stHora1, L"Hora1");
    DECLARE_CONST_UNICODE_STRING(stHora1_24, L"Hora1_24");
    DECLARE_CONST_UNICODE_STRING(stHora2, L"Hora2");
    DECLARE_CONST_UNICODE_STRING(stHora2_24, L"Hora2_24");
    DECLARE_CONST_UNICODE_STRING(stHora3, L"Hora3");
    DECLARE_CONST_UNICODE_STRING(stHora3_24, L"Hora3_24");

    if (NT_SUCCESS(WdfDriverOpenParametersRegistryKey(WdfGetDriver(), KEY_WRITE, WDF_NO_OBJECT_ATTRIBUTES, &key)))
    {
        WdfRegistryAssignULong(key, &stPedales, (ULONG)GetDeviceContext(device)->Pedales.Activado);
        WdfRegistryAssignULong(key, &stLuzGlobal, (ULONG)GetDeviceContext(device)->MenuMFD.LuzGlobal);
        WdfRegistryAssignULong(key, &stLuzMFD, (ULONG)GetDeviceContext(device)->MenuMFD.LuzMFD);
        WdfRegistryAssignULong(key, &stHora1, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[0].Minutos);
        WdfRegistryAssignULong(key, &stHora2, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[1].Minutos);
        WdfRegistryAssignULong(key, &stHora3, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[2].Minutos);
        WdfRegistryAssignULong(key, &stHora1_24, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[0]._24h);
        WdfRegistryAssignULong(key, &stHora2_24, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[1]._24h);
        WdfRegistryAssignULong(key, &stHora3_24, (ULONG)GetDeviceContext(device)->MenuMFD.Hora[2]._24h);
        WdfRegistryClose(key);
    }
}

#pragma region "Pantallas"
BOOLEAN  VerPantalla1(WDFDEVICE device)
{
	PAGED_CODE();
	UCHAR cursor = GetDeviceContext(device)->MenuMFD.EstadoCursor % 3;
	UCHAR pagina = GetDeviceContext(device)->MenuMFD.EstadoCursor / 3;
    CHAR f1[16], f2[16], f3[16], f4[16], f5[16], f6[16], f7[16], f8[16];
    CHAR* filas[9];

    RtlCopyMemory(f1, " Pedales        ", 16);
    RtlCopyMemory(f2, " Luz botones    ", 16);
    RtlCopyMemory(f3, " Luz MFD        ", 16);
    RtlCopyMemory(f4, " Hora 1         ", 16);
    RtlCopyMemory(f5, " Hora 2         ", 16);
    RtlCopyMemory(f6, " Hora 3         ", 16); //251
    RtlCopyMemory(f7, " Salir          ", 16); //252
    RtlCopyMemory(f8, "                ", 16);
    filas[0] = f1;
    filas[1] = f2;
    filas[2] = f3;
    filas[3] = f4;
    filas[4] = f5;
    filas[5] = f6;
    filas[6] = f7;
    filas[7] = f8;
    filas[8] = f8;

	filas[cursor + (pagina * 3)][0] = '>';

    for (UCHAR i = 0; i < 3; i++)
    {
        CHAR* texto = filas[i + (pagina * 3)];
        CHAR buffer[17];
		for (UCHAR c = 0; c < 16; c++)
		{
			buffer[c + 1] = texto[c];
		}

        buffer[0] = i + 1;

		if (!NT_SUCCESS(Set_Texto(device, (UCHAR*)buffer, 17)))
		{
			return FALSE;
		}
    }

	return TRUE;
}

BOOLEAN VerPantallaOnOff(WDFDEVICE device)
{
    PAGED_CODE();
    UCHAR cursor = GetDeviceContext(device)->MenuMFD.EstadoCursor;
    CHAR f1[8], f2[8];
    CHAR* filas[2];

    RtlCopyMemory(f1, " On     ", 8);
    RtlCopyMemory(f2, " Off    ", 8);

    filas[0] = f1;
    filas[1] = f2;

    filas[cursor][0] = '>';

    filas[(GetDeviceContext(device)->Pedales.Activado) ? 0 : 1][5] = '(';
    filas[(GetDeviceContext(device)->Pedales.Activado) ? 0 : 1][6] = '*';
    filas[(GetDeviceContext(device)->Pedales.Activado) ? 0 : 1][7] = ')';

    for (UCHAR i = 0; i < 2; i++)
    {
        CHAR* texto = filas[i];
        CHAR buffer[9];
        for (UCHAR c = 0; c < 8; c++)
        {
            buffer[c + 1] = texto[c];
        }
        buffer[0] = i + 1;

        if (!NT_SUCCESS(Set_Texto(device, (UCHAR*)buffer, 9)))
        {
            return FALSE;
        }
    }
    f1[0] = 3; //file 3 vacía
    if (!NT_SUCCESS(Set_Texto(device, (UCHAR*)f1, 1)))
    {
        return FALSE;
    }

    return TRUE;
}

BOOLEAN VerPantallaLuz(WDFDEVICE device, UCHAR estado)
{
    PAGED_CODE();
    UCHAR cursor = GetDeviceContext(device)->MenuMFD.EstadoCursor;
    CHAR f1[10], f2[10], f3[10];
    CHAR* filas[3];

    RtlCopyMemory(f1, " Bajo     ", 10);
    RtlCopyMemory(f2, " Medio    ", 10);
    RtlCopyMemory(f3, " Alto     ", 10);

    filas[0] = f1;
    filas[1] = f2;
    filas[2] = f3;

    filas[cursor][0] = '>';

    filas[estado][7] = '(';
    filas[estado][8] = '*';
    filas[estado][9] = ')';

    for (UCHAR i = 0; i < 3; i++)
    {
        CHAR* texto = filas[i];
        CHAR buffer[11];
        for (UCHAR c = 0; c < 10; c++)
        {
            buffer[c + 1] = texto[c];
        }
        buffer[0] = (i + 1);

        if (!NT_SUCCESS(Set_Texto(device, (UCHAR*)buffer, 11)))
        {
            return FALSE;
        }
    }

    return TRUE;
}

BOOLEAN VerPantallaHora(WDFDEVICE device, BOOLEAN sel, CHAR hora, UCHAR minuto, BOOLEAN h24)
{
    PAGED_CODE();
    UCHAR cursor = GetDeviceContext(device)->MenuMFD.EstadoCursor % 3;
    UCHAR pagina = GetDeviceContext(device)->MenuMFD.EstadoCursor / 3;
    CHAR f1[11], f2[11], f3[11], f4[11], f5[11];
    CHAR* filas[6];

    RtlCopyMemory(f1, " Hora:     ", 11);
    RtlCopyMemory(f2, " Minuto:   ", 11);
    RtlCopyMemory(f3, " AM/PM:    ", 11);
    RtlCopyMemory(f4, " Volver    ", 11);
    RtlCopyMemory(f5, "           ", 11);
    filas[0] = f1;
    filas[1] = f2;
    filas[2] = f3;
    filas[3] = f4;
    filas[4] = f5;
    filas[5] = f5;

    filas[cursor + (pagina * 3)][0] = (sel) ? '*' : '>';

    RtlStringCbPrintfA(&f1[7], 2, "%02d", hora);
    RtlStringCbPrintfA(&f2[9], 2, "%02u", minuto);
    RtlCopyMemory(&f3[8], (h24) ? "No" : "Si", 2);

    for (UCHAR i = 0; i < 3; i++)
    {
        CHAR* texto = filas[i + (pagina * 3)];
        CHAR buffer[12];
        for (UCHAR c = 0; c < 11; c++)
        {
            buffer[c + 1] = texto[c];
        }

        buffer[0] = i + 1;

        if (!NT_SUCCESS(Set_Texto(device, (UCHAR*)buffer, 12)))
        {
            return FALSE;
        }
    }

    return TRUE;
}
#pragma endregion
