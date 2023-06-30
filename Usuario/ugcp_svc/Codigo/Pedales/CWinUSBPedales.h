#pragma once
#include "../IEntradaHID.h"

#define HARDWARE_ID_PEDALES  L"\\\\?\\HID#VID_06A3&PID_0763"

class CPedalesEntrada : public IEntradaHID
{
public:
	CPedalesEntrada();
	~CPedalesEntrada();

	// Heredado vía IEntradaHID
	virtual bool Preparar() override;
	virtual bool Abrir() override;
	virtual void Cerrar() override;
	virtual unsigned short Leer(void* buff) override;
private:
	HANDLE hdevPedales = INVALID_HANDLE_VALUE;
	wchar_t* rutaPedales = nullptr;
	HANDLE mutexOperar = nullptr;

	const unsigned char READ_TAM = 4;
};

