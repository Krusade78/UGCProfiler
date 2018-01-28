/*--
Copyright (c) 2007 Alfredo Costalago

Module Name:

    serviciox36.h
--*/

typedef	struct _CALIBRADO {
	UINT16 i;
	UINT16 c;
	UINT16 d;
	UCHAR n;
	UCHAR Margen;
	UCHAR Resistencia;
	BOOLEAN cal;
	BOOLEAN antiv;
} CALIBRADO;

class CServicio
{
public:
	CServicio();

	bool horaActiva;
	bool fechaActiva;

	void IniciarServicio();
	void Tick();
	void ClearX52();
	void CargarConfiguracion();
private:
	char luzMFD;
	char luces;
	bool hora24[3];
	WORD hora[3];

	void SetTextoInicio();
	void CargarCalibrado();
	void LeerRegistro(CALIBRADO* ejes);
};