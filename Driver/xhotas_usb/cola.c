#include <wdm.h>

#define _COLA_
#include "cola.h"
#undef _COLA_

PCOLA ColaCrearTemporal()
{
	PCOLA cola=(PCOLA)ExAllocatePoolWithTag(NonPagedPool, 
                                sizeof(COLA), 
                                (ULONG)'lcHV'
                                );
	if(cola != NULL)
	{
		cola->fin = NULL;
		cola->principio = NULL;
	}
	return cola;
}

BOOLEAN ColaEstaVacia(PCOLA cola)
{
	return (cola->principio == NULL);
}

VOID ColaBorrar(PCOLA cola)
{
	if(cola->principio != NULL)
	{
		while(!ColaBorrarNodo1(cola));
	}
	ExFreePoolWithTag((PVOID)cola, (ULONG)'lcHV');
}

BOOLEAN ColaBorrarNodo1(PCOLA cola)
{
	PNODO primero = cola->principio;
	if(primero != NULL)
	{
		cola->principio = primero->siguiente;
		if(primero->siguiente == NULL)
			cola->fin = NULL;
		else
			primero->siguiente->anterior = NULL;

		if(primero->Datos != NULL)
			ExFreePool(primero->Datos);

		ExFreePoolWithTag((PVOID)primero, (ULONG)'lcHV');
		if(cola->principio == NULL)
			return TRUE; // Cola vacia
	}
	else
		return TRUE;


	return FALSE; // Cola no vacia
}

BOOLEAN ColaPush(PCOLA cola, PVOID dato)
{
	PNODO nodo;

	if (cola == NULL)
	{
		cola = ColaCrear();
		if (cola == NULL)
			return FALSE;
	}

	nodo = (PNODO)ExAllocatePoolWithTag(NonPagedPool, sizeof(NODO), (ULONG)'lcHV');
	if(nodo != NULL)
	{
		nodo->Datos = dato;
		nodo->siguiente = NULL;
		nodo->anterior = cola->fin;
		if(cola->principio == NULL)
			cola->principio = nodo;
		else
			cola->fin->siguiente = nodo;
 
		cola->fin = nodo;
	}
	else
		return FALSE;


	return TRUE;
}

BOOLEAN ColaBorrarNodo(PCOLA cola, PNODO nodo)
{
	if(nodo == cola->principio || nodo->anterior == NULL)
	{
		return ColaBorrarNodo1(cola);
	}
	else
	{
		nodo->anterior->siguiente = nodo->siguiente;
		if(nodo->siguiente != NULL)
			nodo->siguiente->anterior = nodo->anterior;

		if(nodo == cola->fin)
			cola->fin = nodo->anterior;

		if(nodo->Datos != NULL)
			ExFreePool(nodo->Datos);

		ExFreePoolWithTag(nodo, (ULONG)'lcHV');
	}

	return FALSE; // Cola no vacia
}