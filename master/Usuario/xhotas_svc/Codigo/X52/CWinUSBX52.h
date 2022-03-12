#pragma once
#include <winusb.h>
#include "../IEntradaHID.h"

#define HARDWARE_ID_X52 L"\\\\?\\USB#VID_06A3&PID_0255"

class CX52Entrada : public IEntradaHID
{
public:
	CX52Entrada();
	~CX52Entrada();

	// Heredado vía IEntradaHID
	virtual bool Preparar() override;
	virtual bool Abrir() override;
	virtual void Cerrar() override;
	virtual unsigned short Leer(void* buff) override;
private:
	GUID guidInterface = { 0xA57C1168, 0x7717, 0x4AF0, { 0xB3, 0x0E, 0x6A, 0x4C, 0x62, 0x30, 0xBB, 0x10 } };

	HANDLE mutexOperar = nullptr;
	wchar_t* rutaPedales = nullptr;
	HANDLE usbh = INVALID_HANDLE_VALUE;
	WINUSB_INTERFACE_HANDLE hwusb = nullptr;
	WINUSB_PIPE_INFORMATION pipe{};
};

