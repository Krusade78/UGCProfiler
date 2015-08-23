#pragma once

class CTraduce
{
public:
	static int Msg(wchar_t* id1, UINT tipo);
	static wchar_t* Txt(wchar_t* id);
	static void Cerrar();
private:
	typedef struct STLISTA
	{
		wchar_t id[32];
		wchar_t valor[512];
		STLISTA* next;
	} TLISTA;
	static TLISTA* xml;
	static const wchar_t cadenaNula = L'';
	static void Traducir(wchar_t* id, wchar_t** texto);
	static void CogerNombreIdioma(wchar_t* nombre);
	static bool BuscarArchivo(wchar_t* idioma, wchar_t* ruta);
	static bool ExtraerCadena(wchar_t* id, wchar_t** texto);


};

