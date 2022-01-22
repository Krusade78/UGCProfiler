#pragma once
#include "../Perfil/CPerfil.h"
#include "../ColaEventos/CPaqueteEventos.h"
#include "../ProcesarSalida/CVirtualHID.h"

//Pedales, X52_Joy, X52_Ace, NXT
constexpr UINT16 MAPA_MINIMOS[4][8]{ {0, 0, 0, 0, 0, 0, 0, 0}, { 0,0,0,0,0,0,0,0 }, {0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0} };
constexpr UINT16 MAPA_CENTROS[4][8]{ {0, 0, 0, 255, 0, 0, 63, 63}, { 1023,1023,0,511,0,0,0,0 }, { 0,0,127,128,128,127,8,8 }, { 2048,2048,1024,2048,0,0,512,512 } };
constexpr UINT16 MAPA_MAXIMOS[4][8]{ {0, 0, 0, 511, 0, 0, 127, 127}, { 2047, 2047, 0, 1023, 0, 0, 0, 0 }, { 0, 0, 255, 255, 255, 255, 15, 15 }, { 4095, 4095, 2047, 4095, 0, 0, 1023, 1023 } };

class CCalibrado
{
public:
	static void Calibrar(CPerfil* pPerfil, TipoJoy tipo, PVHID_INPUT_DATA datosHID);
};

