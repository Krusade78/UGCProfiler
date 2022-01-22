#include "../framework.h"
#include "CVirtualHID.h"
#include "../Perfil/CPerfil.h"

CVirtualHID::CVirtualHID()
{
   RtlZeroMemory(&Estado, sizeof(Estado));
   hMutextRaton = CreateSemaphore(NULL, 1, 1, NULL);
}

CVirtualHID::~CVirtualHID()
{
    CloseHandle(hVHid);
    CloseHandle(hMutextRaton);
}

bool CVirtualHID::Iniciar()
{
    HANDLE hdev = CreateFile(L"\\\\.\\XHOTAS_VHID_Interface", GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
    if (INVALID_HANDLE_VALUE == hdev)
    {
        return false;
    }

    hVHid = hdev;

    return true;
}

bool CVirtualHID::EnviarReportDescriptor(void* vpPerfil)
{
    CPerfil* pPerfil = static_cast<CPerfil*>(vpPerfil);
    DWORD tam = 0;
    UCHAR* buff = new UCHAR[1 + sizeof(size_t) + sizeof(ReportDescriptor0) + (sizeof(ReportDescriptor1) * 3)];
    buff[0] = 0;
    size_t* tamaño = (size_t*)(buff + 1);
    *tamaño = sizeof(ReportDescriptor0) + (sizeof(ReportDescriptor1) * 3);
    RtlCopyMemory(&buff[1 + sizeof(size_t)], ReportDescriptor0, sizeof(ReportDescriptor0));

    UCHAR* buffJ = new UCHAR[sizeof(ReportDescriptor1)];
    for (char i = 0; i < 3; i++)
    {
        RtlCopyMemory(buffJ, ReportDescriptor1, sizeof(ReportDescriptor1));
        buffJ[7] = i + 3;
        *((UINT16*)&buffJ[EJE_X_MAX]) = pPerfil->GetPr()->RangosSalida[i][0];
        *((UINT16*)&buffJ[EJE_Y_MAX]) = pPerfil->GetPr()->RangosSalida[i][1];
        *((UINT16*)&buffJ[EJE_Z_MAX]) = pPerfil->GetPr()->RangosSalida[i][2];
        *((UINT16*)&buffJ[EJE_RX_MAX]) = pPerfil->GetPr()->RangosSalida[i][3];
        *((UINT16*)&buffJ[EJE_RY_MAX]) = pPerfil->GetPr()->RangosSalida[i][4];
        *((UINT16*)&buffJ[EJE_RZ_MAX]) = pPerfil->GetPr()->RangosSalida[i][5];
        *((UINT16*)&buffJ[EJE_SL1_MAX]) = pPerfil->GetPr()->RangosSalida[i][6];
        *((UINT16*)&buffJ[EJE_SL2_MAX]) = pPerfil->GetPr()->RangosSalida[i][7];
        RtlCopyMemory(&buff[1 + sizeof(size_t) + sizeof(ReportDescriptor0) + (sizeof(ReportDescriptor1) * i)], buffJ, sizeof(ReportDescriptor1));
    }
    delete[] buffJ;

    reportOk = WriteFile(hVHid, buff, 1 + sizeof(size_t) + sizeof(ReportDescriptor0) + (sizeof(ReportDescriptor1) * 3), &tam, NULL);
    //DWORD err = GetLastError();
    delete[] buff;
    return reportOk;
}

void CVirtualHID::EnviarRequestRaton(BYTE* inputData)
{
    if (reportOk)
    {
        DWORD tam = 0;
        UCHAR* buff = new UCHAR[sizeof(Estado.Raton) + 1];
        buff[0] = 2;
        RtlCopyMemory(&buff[1], inputData, sizeof(Estado.Raton));
        /*BOOL ok = */WriteFile(hVHid, buff, sizeof(Estado.Raton) + 1, &tam, NULL);
        //DWORD err = GetLastError();
        delete[] buff;
    }
}

void CVirtualHID::EnviarRequestTeclado()
{
    if (reportOk)
    {
        DWORD tam = 0;
        UCHAR* buff = new UCHAR[30];
        buff[0] = 1;
        RtlCopyMemory(&buff[1], Estado.Teclado, 29);
        WriteFile(hVHid, buff, 30, &tam, NULL);
        //DWORD err = GetLastError();
        delete[] buff;
    }

}

void CVirtualHID::EnviarRequestJoystick(UCHAR joyId, PVHID_INPUT_DATA inputData)
{
    if (reportOk)
    {
        DWORD tam = 0;
        UCHAR* buff = new UCHAR[sizeof(VHID_INPUT_DATA) + 1];
        buff[0] = joyId + 3;
        RtlCopyMemory(&buff[1], inputData, sizeof(VHID_INPUT_DATA));
        WriteFile(hVHid, buff, sizeof(VHID_INPUT_DATA) + 1, &tam, NULL);
        //DWORD err = GetLastError();
        delete[] buff;
    }
}
