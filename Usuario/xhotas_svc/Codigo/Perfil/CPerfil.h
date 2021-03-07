#pragma once
#include <deque>
#include "../ColaEventos/CPaqueteEventos.h"

class CPerfil
{
public:
	typedef struct {
		UCHAR Cal;
		UCHAR Nulo;
		INT16 Izq;
		INT16 Cen;
		INT16 Der;
	} ST_LIMITES;
	typedef struct {
		UCHAR Antiv;
		UCHAR PosRepetida;
		UCHAR Margen;
		UCHAR Resistencia;
		INT16 PosElegida;
	} ST_JITTER;
	typedef struct
	{
		ST_LIMITES Limites[4][8];
		ST_JITTER Jitter[4][8];
	} CALIBRADO;
	typedef struct
	{
		struct
		{
			UINT16 Indices[15];
			UCHAR TamIndices;
			UCHAR Reservado;//padding
		} MapaBotones[4][2][3][16]; // el ultimo es la rueda
		struct
		{
			UINT16 Indices[15];
			UCHAR TamIndices;
			UCHAR Reservado;//padding
		} MapaSetas[4][2][3][32];
		struct
		{
			UINT16 Indices[16];
			UCHAR SensibilidadRaton;
			UCHAR JoySalida;
			UCHAR TipoEje; 				//Mapeado en bits 0:ninguno, 1:Normal, 10:Invertido, 100:Mini, 1000:Raton, 10000:Incremental, 100000: Bandas
			UCHAR Eje;					//0:X, Y, Z, Rx, Ry, Rz, Sl1, Sl2
			UCHAR Sensibilidad[10];     // Curva de sernsibilidad
			UCHAR Slider;
			UCHAR Bandas[15];
			UCHAR ResistenciaInc;
			UCHAR ResistenciaDec;
		} MapaEjes[4][2][3][8];

		std::deque<CPaqueteEvento*>* Acciones;

		UCHAR TickRaton;

	} PROGRAMADO;
	typedef struct
	{
		UCHAR BotonesDx[4][2];
		struct
		{
			UCHAR PosActual;
		} Botones[4][2][3][16];
		struct
		{
			UCHAR PosActual;
		} Setas[4][2][3][32];
		UCHAR SetasDx[4][4];
		struct
		{
			INT16 PosIncremental;
			UCHAR Banda;
		} Ejes[4][2][3][8];

		char Pinkie;
		char Modos;
	} ESTADO;

	CPerfil();
	~CPerfil();

	void SetModoCalibrado(BYTE modo) { InterlockedExchange16(&modoCalibrado, modo); };
	void SetModoRaw(BYTE modo) { InterlockedExchange16(&modoRaw, modo); };
	void EscribirCalibrado(BYTE* datos);
	void EscribirAntivibracion(BYTE* datos);
	bool HF_IoEscribirComandos(BYTE* datos, DWORD tam);
	bool HF_IoEscribirMapa(BYTE* datos, DWORD tam);

	bool GetModoCalibrado() { return InterlockedCompareExchange16(&modoCalibrado, FALSE, FALSE) == TRUE; };
	bool GetModoRaw() { return InterlockedCompareExchange16(&modoRaw, FALSE, FALSE) == TRUE; };

	void InicioLecturaCal() { WaitForSingleObject(hMutexCalibrado, INFINITE); }
	CALIBRADO* GetCal() { return &calibrado; }
	void FinLecturaCal() { ReleaseMutex(hMutexCalibrado); }

	void InicioLecturaPr() { WaitForSingleObject(hMutexPrograma, INFINITE); }
	PROGRAMADO* GetPr() { return &perfil; };
	void FinLecturaPr() { ReleaseMutex(hMutexPrograma); }

	void LockEstado() { WaitForSingleObject(hMutexEstado, INFINITE); }
	ESTADO* GetEstado() { return &estado; }
	void UnlockEstado() { ReleaseMutex(hMutexEstado); }

private:
	HANDLE hMutexPrograma = nullptr;
	HANDLE hMutexCalibrado = nullptr;
	HANDLE hMutexEstado = nullptr;
	short modoRaw = FALSE;
	short modoCalibrado = FALSE;
	PROGRAMADO perfil;
	CALIBRADO calibrado;
	ESTADO estado;
	bool perfilNuevoOut = false;
	bool perfilNuevoIn = false;
	bool resetComandos = false;
	
	void LimpiarPerfil();
};

