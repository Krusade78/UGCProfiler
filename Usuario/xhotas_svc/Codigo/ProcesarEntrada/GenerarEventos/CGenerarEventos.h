#pragma once
#include "../../Perfil/CPerfil.h"
//#include "../../ColaEventos/CPaqueteEventos.h"
#include "../../ColaEventos/CColaEventos.h"
#include "../../ProcesarSalida/CVirtualHID.h"

class CGenerarEventos
{
public:
	enum class Origen : unsigned char
	{
		Boton = 0,
		Seta,
		Eje
	};

	static void Iniciar(CPerfil* pPerfil, CColaEventos* pColaEv);

	static void Raton(PEV_COMANDO accion);
	static void Comando(TipoJoy id, UINT16 accionId, UCHAR origen, Origen tipoOrigen, PEV_COMANDO datosEje);
	static void DirectX(UCHAR joyId, UCHAR mapa, PVHID_INPUT_DATA inputData);
#ifdef _DEBUG
	static CPerfil* pPerfil;
	static CColaEventos* pColaEv;
#endif // !DEBUG
private:
#ifndef _DEBUG
	static inline CPerfil* pPerfil;
	static inline CColaEventos* pColaEv;
#endif // !DEBUG
};

