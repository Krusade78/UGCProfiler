#pragma once
#include <deque>
#include "../ProcesarSalida/CVirtualHID.h"

class TipoComando
{
public:
	static const unsigned char Tecla = 1;

	static const unsigned char DxBoton = 2;
	static const unsigned char DxSeta = 3;

	static const unsigned char RatonBt1 = 4;
	static const unsigned char RatonBt2 = 5;
	static const unsigned char RatonBt3 = 6;
	static const unsigned char RatonIzq = 7;
	static const unsigned char RatonDer = 8;
	static const unsigned char RatonArr = 9;
	static const unsigned char RatonAba = 10;
	static const unsigned char RatonWhArr = 11;
	static const unsigned char RatonWhAba = 12;

	static const unsigned char Delay = 20;
	static const unsigned char Hold = 21;
	static const unsigned char Repeat = 22;
	static const unsigned char RepeatN = 23;

	static const unsigned char Modo = 30;
	static const unsigned char Pinkie = 31;

	static const unsigned char MfdLuz = 40;
	static const unsigned char Luz = 41;
	static const unsigned char InfoLuz = 42;
	static const unsigned char MfdPinkie = 43;
	static const unsigned char MfdTextoIni = 44;
	static const unsigned char MfdTexto = 45;
	static const unsigned char MfdTextoFin = 46;
	static const unsigned char MfdHora = 47;
	static const unsigned char MfdHora24 = 48;
	static const unsigned char MfdFecha = 49;

	static const unsigned char Reservado_DxPosicion = 100;
	static const unsigned char Reservado_CheckHold = 101;
	//Reservado_RepeatIni;

	static const unsigned char Soltar = 128;

	TipoComando& operator=(unsigned char v) { valor = v; return *this; }
	bool operator==(unsigned char v) { return valor == v; }
	bool operator!=(unsigned char v) { return valor != v; }
	unsigned char operator&(unsigned char op2) { return (valor & op2); }
	unsigned char Get() { return valor; }
private:
	unsigned char valor = 0;
};
enum class TipoJoy : unsigned char { Pedales, X52_Joy, X52_Ace, NXT, RawPedales = 100, RawX52_Joy, RawX52_Ace, RawNXT};

#pragma warning (disable: 26495)
typedef struct
{
	TipoComando Tipo;
	union
	{
		UCHAR Dato;
		struct
		{
			UCHAR JoyId;
			UCHAR Mapa;
			VHID_INPUT_DATA Datos;
		} VHid;
		struct
		{
			UCHAR JoyId;
			UCHAR Origen;
			UCHAR Modo;
			UCHAR Pinkie;
			USHORT Incremental;
			UCHAR Banda;
		} Extendido;
	};
} EV_COMANDO, *PEV_COMANDO;
#pragma warning (default: 26495)

class CPaqueteEvento
{
public:
	CPaqueteEvento();
	~CPaqueteEvento();

	void AñadirComando(PEV_COMANDO comando);
	std::deque<PEV_COMANDO>* GetColaComandos();
private:
	std::deque<PEV_COMANDO>* cola = nullptr;
};
