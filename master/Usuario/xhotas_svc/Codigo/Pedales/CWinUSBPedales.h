#pragma once
#include <winusb.h>
#include "../IEntradaHID.h"

#define HARDWARE_ID_PEDALES  L"\\\\?\\USB#VID_06A3&PID_0763"

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
	GUID guidInterface = { 0xF62B2F21, 0xC152, 0x45FC, {0x8C, 0xC3, 0xA3, 0x0F, 0xFA, 0x9E, 0x79, 0x24} };

	HANDLE mutexOperar = nullptr;
	wchar_t* rutaPedales = nullptr;
	HANDLE usbh = INVALID_HANDLE_VALUE;
	WINUSB_INTERFACE_HANDLE hwusb = nullptr;
	WINUSB_PIPE_INFORMATION pipe{};
};

