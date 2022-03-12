#pragma once
#include "../IEntradaHID.h"

#define HARDWARE_ID_NXT L"\\\\?\\HID#VID_231d&PID_0200"

class CNXTEntrada : public IEntradaHID
{
public:
	CNXTEntrada();
	~CNXTEntrada();

	// Heredado vía IEntradaHID
	virtual bool Preparar() override;
	virtual bool Abrir() override;
	virtual void Cerrar() override;
	virtual unsigned short Leer(void* buff) override;
private:
	PVOID hdevNXT = INVALID_HANDLE_VALUE;
	wchar_t* rutaNXT = nullptr;
	HANDLE mutexOperar = nullptr;
};

