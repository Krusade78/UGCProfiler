using System.Runtime.InteropServices;

namespace Comunes
{
    public class CTipos
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct STLIMITES
        {
            public byte Cal; //bool
            public byte Nulo;
            public ushort Izq;
            public ushort Cen;
            public ushort Der;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct STJITTER
        {
            public byte Antiv; //bool
            public byte PosRepetida;
            public byte Margen;
            public byte Resistencia;
            public ushort PosElegida;
        };

        public enum TipoComando : byte
        {
			TipoComando_Tecla = 1,

			TipoComando_DxBoton,
			TipoComando_DxSeta,

			TipoComando_RatonBt1,
			TipoComando_RatonBt2,
			TipoComando_RatonBt3,
			TipoComando_RatonIzq,
			TipoComando_RatonDer,
			TipoComando_RatonArr,
			TipoComando_RatonAba,
			TipoComando_RatonWhArr,
			TipoComando_RatonWhAba,

			TipoComando_Delay = 20,
			TipoComando_Hold,
			TipoComando_Repeat,
            TipoComando_RepeatN,

            TipoComando_Modo = 30,
			TipoComando_Pinkie,

			TipoComando_MfdLuz = 40,
			TipoComando_Luz,
			TipoComando_InfoLuz,
			TipoComando_MfdPinkie,
			TipoComando_MfdTextoIni,
			TipoComando_MfdTexto,
			TipoComando_MfdTextoFin,
			TipoComando_MfdHora,
			TipoComando_MfdHora24,
			TipoComando_MfdFecha,

			TipoComando_Reservado_DxPosicion = 100,

            TipoComando_Soltar = 128,
        };
    }
}
