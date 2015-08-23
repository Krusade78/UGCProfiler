#pragma once
class CBaseServicio
{
public:
	CBaseServicio();
	~CBaseServicio();

	void Cerrar();
	static void ResumePower();
private:
	HANDLE hSalir;
	static HANDLE hPower;
	static DWORD WINAPI Hilo(LPVOID lpParameter);
};

