#pragma once

class CMenuMFD
{
public:
	CMenuMFD();
	~CMenuMFD();

	static CMenuMFD* Get() { return pLocal; }

	void SetTextoInicio();
	void MenuPulsarBoton(UCHAR boton);
	void MenuSoltarBoton(UCHAR boton);
	void SetHoraActivada(bool onoff) { menuMFD.HoraActivada = onoff; }
	void SetFechaActivada(bool onoff) { menuMFD.FechaActivada = onoff; }
	bool EstaActivado() { return menuMFD.Activado; }
	bool X52Joy() { return !menuMFD.NXTActivado; }

private:
	enum class Boton : unsigned char
	{
		Intro = 0,
		Abajo,
		Arriba
	};
	struct
	{
		PTP_TIMER	TimerMenu;
		bool		TimerEsperando;
		bool		Activado;
		UCHAR		EstadoBotones;

		PTP_TIMER	TimerHora;
		bool		HoraActivada;
		bool		FechaActivada;

		UCHAR		EstadoCursor;
		UCHAR		EstadoPagina;

		bool		NXTActivado;
		struct
		{
			SHORT       Minutos; //horas + minutos (en minutos totales)
			BOOLEAN		_24h;
		} Hora[3];

		UCHAR		LuzGlobal;
		UCHAR		LuzMFD;
	} menuMFD;

	static CMenuMFD* pLocal;

	bool horaActiva = false;
	bool fechaActiva = false;

	void LeerConfiguracion();
	void GuardarConfiguracion();
	static void CALLBACK EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
	static void CALLBACK EvtTickHora(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);

	void CambiarEstado(UCHAR boton);
	void MenuCerrar();
	void VerPantalla1();
	void VerPantallaOnOff();
	void VerPantallaLuz(UCHAR estado);
	void VerPantallaHora(bool sel, CHAR hora, UCHAR minuto, bool h24);
};


