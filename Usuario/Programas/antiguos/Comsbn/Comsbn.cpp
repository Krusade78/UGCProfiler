// Comsbn.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include <winioctl.h>



BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
    return TRUE;
}
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)

#define IOCTL_USR_MAPA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_COMANDOS		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
__declspec(dllexport) char CargarMapa(wchar_t* archivo)
{
	struct _EJES{
		UCHAR Mouse;
		UCHAR nEje;				//Mapeado 0:nada <20 normal >20 invertido
		UCHAR Sensibilidad[10];
		UCHAR Bandas[15];
		UINT16 Indices[16];
	}MapaEjes[2][3][3][4];
	struct _PEQUE{
		UCHAR Mouse;
		UCHAR nEje;
		UCHAR Bandas[15];
		UINT16 Indices[16];
	}MapaEjesPeque[2][3][3][3];
	struct _MINI{
		UCHAR Mouse;
		UCHAR nEje;
	}MapaEjesMini[2][3][3][2];
	struct _BOTONES{
		UCHAR Estado;	// 4 bit idc 4 bit total posiciones
		UINT16 Indices[15];
	}MapaBotones[2][3][3][26]; // el ultimo es la rueda
	struct _SETAS{
		UCHAR Estado;
		UINT16 Indices[15];
	} MapaSetas[2][3][3][32];

	UCHAR tipoJoy;
	UCHAR TickRaton;

	DWORD ret;
	HANDLE f=CreateFile(archivo, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, NULL, NULL);
	if(f == INVALID_HANDLE_VALUE)
		return 1;

	if( !ReadFile(f, &tipoJoy, 1, &ret, 0) || (ret !=1 ) )
	{
		CloseHandle(f);
		return 2;
	}
	if( !ReadFile(f, &TickRaton, 1, &ret, 0) || (ret != 1) )
	{
		CloseHandle(f);
		return 2;
	}
	
	if(SetFilePointer(f, 113, 0, FILE_CURRENT) != 115)
	{
		CloseHandle(f);
		return 2;
	}

	if( !ReadFile(f, MapaEjes, sizeof(MapaEjes), &ret, 0) || (ret != sizeof(MapaEjes)) )
	{
		CloseHandle(f);
		return 2;
	}

	if( !ReadFile(f, MapaEjesPeque, sizeof(MapaEjesPeque), &ret, 0) || (ret != sizeof(MapaEjesPeque)) )
	{
		CloseHandle(f);
		return 2;
	}
	if( !ReadFile(f, MapaEjesMini, sizeof(MapaEjesMini), &ret, 0) || (ret != sizeof(MapaEjesMini)) )
	{
		CloseHandle(f);
		return 2;
	}
	if( !ReadFile(f, MapaBotones, sizeof(MapaBotones), &ret, 0) || (ret != sizeof(MapaBotones)) )
	{
		CloseHandle(f);
		return 2;
	}
	if( !ReadFile(f, MapaSetas, sizeof(MapaSetas), &ret, 0) || (ret !=sizeof(MapaSetas)) )
	{
		CloseHandle(f);
		return 2;
	}

	UINT16 total;
	ULONG tamBuffer;

	if( !ReadFile(f, &total, 2, &ret, 0) || (ret != 2) )
	{
		CloseHandle(f);
		return 2;
	}

	tamBuffer = 2;
	char horaAct = 1;
	char fechaAct = 1;
	PUCHAR* lista  =new PUCHAR[total];
	RtlZeroMemory(lista, sizeof(PUCHAR)  *total);

	for(int i = 0; i < total; i++)
	{
		UCHAR tam;
		DWORD seek;
		if( !ReadFile(f, &tam, 1, &ret, 0) || (ret != 1) )
			goto mal;
		seek = SetFilePointer(f, 0, 0, FILE_CURRENT);
		if(seek == INVALID_SET_FILE_POINTER)
			goto mal;
		if(SetFilePointer(f, tam * 2, 0, FILE_CURRENT) != (seek + (tam * 2)))
			goto mal;
		if( !ReadFile(f, &tam, 1, &ret, 0) || (ret != 1) )
			goto mal;
		lista[i] = new UCHAR[(tam * 2) + 1];
		lista[i][0] = tam;
		if( !ReadFile(f, &lista[i][1], tam * 2, &ret, 0) || (ret != (tam * 2)) )
			goto mal;
		if(horaAct || fechaAct)
		{
			for(int j = 0; j < tam; j++)
			{ // buscar modificacion hora o fecha
				if((lista[i][1 + (j * 2)] == 25) || (lista[i][1 + (j * 2)] == 26))
					horaAct = 0;
				if(lista[i][1 + (j * 2)] == 27)
					fechaAct = 0;
			}
		}
		tamBuffer += (tam * 2) + 1;
	}
	CloseHandle(f);

	char* buffer = new char[tamBuffer];
	RtlCopyMemory(buffer, &total, 2);
	tamBuffer = 2;
	for(int i = 0; i < total; i++)
	{
		RtlCopyMemory(&buffer[tamBuffer], lista[i], (lista[i][0] * 2) + 1);
		tamBuffer += (lista[i][0] * 2) + 1;
		delete[] lista[i]; lista[i] = NULL;
	}
	delete[] lista; lista = NULL;

	HANDLE driver = CreateFile(	L"\\\\.\\XHOTASHidInterface",
								GENERIC_WRITE,
								FILE_SHARE_WRITE,
								NULL,
								OPEN_EXISTING,
								0,
								NULL);
	if(driver == INVALID_HANDLE_VALUE)
	{
		delete[] buffer; buffer = NULL;
		return 3;
	}

	if(!DeviceIoControl(driver, IOCTL_USR_COMANDOS, buffer, tamBuffer, NULL, 0, &ret, NULL))
	{
		delete[] buffer; buffer = NULL;
		CloseHandle(driver);
		return 4;
	}

	delete[] buffer; buffer = NULL;

	buffer=new char[sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini) + 2 + sizeof(MapaBotones) + sizeof(MapaSetas)];
	RtlCopyMemory(buffer, MapaEjes, sizeof(MapaEjes));
	RtlCopyMemory(buffer+sizeof(MapaEjes), MapaEjesPeque,sizeof(MapaEjesPeque));
	RtlCopyMemory(buffer+sizeof(MapaEjes) + sizeof(MapaEjesPeque), MapaEjesMini ,sizeof(MapaEjesMini));
	RtlCopyMemory(buffer+sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini),&tipoJoy,1);
	RtlCopyMemory(buffer+sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini)+1,&TickRaton,1);
	RtlCopyMemory(buffer+sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini)+2,MapaBotones,sizeof(MapaBotones));
	RtlCopyMemory(buffer+sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini)+2+sizeof(MapaBotones),MapaSetas,sizeof(MapaSetas));

	if(!DeviceIoControl(driver, IOCTL_USR_MAPA, buffer, sizeof(MapaEjes) + sizeof(MapaEjesPeque) + sizeof(MapaEjesMini) + 2 + sizeof(MapaBotones) + sizeof(MapaSetas), NULL, 0, &ret,NULL))
	{
		delete[] buffer; buffer = NULL;
		CloseHandle(driver);
		return 4;
	}

	CloseHandle(driver);
	delete[] buffer; buffer = NULL;

	HANDLE h = NULL;
	h = OpenEvent(EVENT_MODIFY_STATE, FALSE, L"Global\\eXHOTASHora");
	if(h != NULL)
	{
		if(horaAct) SetEvent(h); else ResetEvent(h);
		CloseHandle(h);
	}
	h = OpenEvent(EVENT_MODIFY_STATE, FALSE, L"Global\\eXHOTASFecha");
	if(h != NULL)
	{
		if(fechaAct) SetEvent(h); else ResetEvent(h);
		CloseHandle(h);
	}
	h = OpenEvent(EVENT_MODIFY_STATE, FALSE, L"Global\\eXHOTASTimer");
	if(h != NULL)
	{
		SetEvent(h);
		CloseHandle(h);
	}
	driver = CreateFile(L"\\\\.\\XUSBInterface",
						GENERIC_WRITE,
						FILE_SHARE_WRITE,
						NULL,
						OPEN_EXISTING,
						0,
						NULL);
	if(driver != INVALID_HANDLE_VALUE)
	{
		char texto[18];
		const char linea[] = "";
		RtlZeroMemory(texto, 18);
		int p1 = -1, p2 = -1, p = wcslen(archivo) - 1;
		wchar_t* pc = &archivo[wcslen(archivo) - 1];
		while( p > -1 )
		{
			if((wcsncmp(pc, L".", 1) == 0) && (p2 == -1))
				p2 = p;
			if( wcsncmp(pc, L"\\", 1) == 0 )
			{
				p1 = p;
				break;
			}
			p--;
			pc = CharPrev(archivo, pc);
		}
		if( p1 != -1 && p2 != -1)
			WideCharToMultiByte(20127, 0, &archivo[p1 + 1], p2 - p1 - 1, &texto[1], 16, NULL, NULL);
		else if( p1 == -1 && p2 != -1)
			WideCharToMultiByte(20127, 0, archivo, p2, &texto[1], 16, NULL, NULL);
		else
			RtlCopyMemory(&texto[1], linea, sizeof(linea));

		texto[0] = 1;
		DeviceIoControl(driver, IOCTL_TEXTO, texto, strlen(texto), NULL, 0, &ret, NULL);
		//siguientes lineas en blanco
		RtlCopyMemory(&texto[1], linea, sizeof(linea));
		texto[0] = 2;
		DeviceIoControl(driver, IOCTL_TEXTO, texto, strlen(texto), NULL, 0, &ret, NULL);
		texto[0] = 3;
		DeviceIoControl(driver, IOCTL_TEXTO, texto, strlen(texto), NULL, 0, &ret, NULL);

		CloseHandle(driver);
	}

	return 0;

mal:
	for(int i = 0; i < total; i++)
	{
		if(lista[i] == NULL)
			break;
		else
		{
			delete[] lista[i]; lista[i] = NULL;
		}
	}
	delete[] lista; lista = NULL;
	CloseHandle(f);
	return 2;

}
