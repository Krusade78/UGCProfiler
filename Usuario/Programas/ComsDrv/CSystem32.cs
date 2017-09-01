using System;
using System.Runtime.InteropServices;

namespace ComsDrv
{
    internal static class CSystem32
    {
        public enum TipoComando
        {
            TipoComando_Tecla = 0,
            TipoComando_RatonBt1,
            TipoComando_RatonBt2,
            TipoComando_RatonBt3,
            TipoComando_RatonIzq,
            TipoComando_RatonDer,
            TipoComando_RatonArr,
            TipoComando_RatonAba,
            TipoComando_RatonWhArr,
            TipoComando_RatonWhAba,
            TipoComando_Delay,
            TipoComando_Hold,
            TipoComando_Repeat,
            TipoComando_RepeatN,
            TipoComando_Modo,
            TipoComando_Pinkie = 16,
            TipoComando_RepeatIni,
            TipoComando_DxBoton,
            TipoComando_DxSeta,
            TipoComando_MfdLuz,
            TipoComando_Luz,
            TipoComando_InfoLuz,
            TipoComando_MfdPinkie,
            TipoComando_MfdTexto,
            TipoComando_MfdHora,
            TipoComando_MfdHora24,
            TipoComando_MfdFecha,
            TipoComando_RepeatFin = 44,
            TipoComando_RepeatNFin,
            TipoComando_MfdTextoFin = 56
        };

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr pSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle handle, UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped);
    }
}