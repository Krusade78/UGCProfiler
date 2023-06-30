#include "../framework.h"
#include "CPaqueteHID.h"

CPaqueteHID::CPaqueteHID(UCHAR* buff, DWORD tam)
{
	datos = new UCHAR[tam];
	if (tam == 4)
	{
		tipo = TipoPaquete::Pedales;
	}
	else if (tam == 15)
	{
		tipo = TipoPaquete::X52;
	}
	else
	{
		tipo = TipoPaquete::NXT;
	}
	RtlCopyMemory(datos, buff, tam);
}

CPaqueteHID::~CPaqueteHID()
{
	delete[] datos; datos = nullptr;
}

UCHAR* CPaqueteHID::GetDatos()
{
	return datos;
}

TipoPaquete CPaqueteHID::GetTipo()
{
	return tipo;
}