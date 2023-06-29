#pragma once
#include "../../Perfil/CPerfil.h"
#include "../../ColaEventos/CPaqueteEventos.h"
#include "../../ProcesarSalida/CVirtualHID.h"

class CEjes
{
public:
	static void SensibilidadYMapeado(CPerfil* pPerfil, TipoJoy tipo, PVHID_INPUT_DATA viejo, PVHID_INPUT_DATA entrada);

	static void MoverEje(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx, UINT16 nuevo);

private:
	static void EjeARaton(UCHAR eje, CHAR mov);
	static UCHAR TraducirGiratorio(CPerfil* pPerfil, UCHAR tipoJ, UCHAR eje, UINT16 nueva, UCHAR pinkie, UCHAR modos);
};
