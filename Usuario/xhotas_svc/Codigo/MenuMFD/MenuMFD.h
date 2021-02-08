#ifdef _CONTEXT_
typedef struct _MENU_MFD_CONTEXT
{
	WDFTIMER	Timer;
	BOOLEAN		TimerEsperando;
	BOOLEAN		Activado;
	UCHAR		EstadoBotones;

	WDFTIMER	TimerHora;
	BOOLEAN		HoraActivada;
	BOOLEAN		FechaActivada;

	UCHAR		EstadoCursor;
	UCHAR		EstadoPagina;
	struct
	{
		SHORT       Minutos; //horas + minutos (en minutos totales)
		BOOLEAN		_24h;
	} Hora[3];

	UCHAR		LuzGlobal;
	UCHAR		LuzMFD;
} MENU_MFD_CONTEXT, * PMENU_MFD_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
VOID LeerConfiguracion(WDFDEVICE device);
EVT_WDF_TIMER EvtTickMenu;
VOID MenuPulsarBoton(WDFDEVICE device, UCHAR boton);
VOID MenuSoltarBoton(WDFDEVICE device, UCHAR boton);
EVT_WDF_TIMER EvtTickHora;
#endif // _PUBLIC_

#ifdef _PRIVATE_
enum
{
	BotonIntro = 0,
	BotonAbajo,
	BotonArriba
};
VOID CambiarEstado(WDFDEVICE device, UCHAR boton);
VOID MenuCerrar(WDFDEVICE device);
VOID GuardarConfiguracion(WDFDEVICE device);
BOOLEAN VerPantalla1(WDFDEVICE device);
BOOLEAN VerPantallaOnOff(WDFDEVICE device);
BOOLEAN VerPantallaLuz(WDFDEVICE device, UCHAR estado);
BOOLEAN VerPantallaHora(WDFDEVICE device, BOOLEAN sel, CHAR hora, UCHAR minuto, BOOLEAN h24);
#endif // _PRIVATE_


