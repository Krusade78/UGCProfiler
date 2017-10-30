typedef struct _ST_NODO_COLA
{
	PVOID Datos;
	struct _ST_NODO_COLA *link;
	struct _ST_NODO_COLA *linkp;
} NODO,*PNODO;

typedef struct _ST_COLA
{
	PNODO principio;
	PNODO fin;
} COLA,*PCOLA;

PCOLA ColaCrear();
BOOLEAN ColaEstaVacia(PCOLA cola);
BOOLEAN ColaPush(PCOLA cola, PVOID dato);
VOID ColaBorrar(PCOLA cola);
BOOLEAN ColaBorrarNodo(PCOLA cola,PNODO nodo);

#ifdef _COLA_

BOOLEAN ColaBorrarNodo1(PCOLA cola);

#endif