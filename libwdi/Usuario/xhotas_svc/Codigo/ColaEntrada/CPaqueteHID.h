#pragma once

enum class TipoPaquete : UCHAR { Pedales, X52, NXT };

class CPaqueteHID
{
public:
	CPaqueteHID(UCHAR* buff, DWORD tam);
	~CPaqueteHID();

	UCHAR* GetDatos();
	TipoPaquete GetTipo();
private:
	UCHAR* datos = nullptr;
	TipoPaquete tipo;
};
