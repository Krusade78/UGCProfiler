#include <wdm.h>

#define _COLA_
#include "cola.h"
#undef _COLA_

PCOLA ColaCrear()
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
	if(cola->principio == NULL)
		return TRUE;
	else
		return FALSE;
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
		cola->principio = primero->link;
		if(primero->link == NULL)
			cola->fin = NULL;
		else
			primero->link->linkp = NULL;

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
	PNODO nodo = (PNODO)ExAllocatePoolWithTag(NonPagedPool, sizeof(NODO), (ULONG)'lcHV');
	if(nodo != NULL)
	{
		nodo->Datos = dato;
		nodo->link = NULL;
		nodo->linkp = cola->fin;
		if(cola->principio == NULL)
			cola->principio = nodo;
		else
			cola->fin->link = nodo;
 
		cola->fin = nodo;
	}
	else
		return FALSE;


	return TRUE;
}

BOOLEAN ColaBorrarNodo(PCOLA cola, PNODO nodo)
{
	if(nodo == cola->principio || nodo->linkp == NULL)
	{
		return ColaBorrarNodo1(cola);
	}
	else
	{
		nodo->linkp->link = nodo->link;
		if(nodo->link != NULL)
			nodo->link->linkp = nodo->linkp;

		if(nodo == cola->fin)
			cola->fin = nodo->linkp;

		if(nodo->Datos != NULL)
			ExFreePool(nodo->Datos);

		ExFreePoolWithTag(nodo, (ULONG)'lcHV');
	}

	return FALSE; // Cola no vacia
}