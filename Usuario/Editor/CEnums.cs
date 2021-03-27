namespace Editor
{
    class CEnums
    {
        public enum Tipo
        {
            Eje,
            Boton,
            Seta
        }

        public enum OrdenLed : byte
        {
            Off,
            Constante,
            Inter_Lento,
            Inter_Medio,
            Inter_Rapido,
            Flash
        };

        public enum ModoColor : byte
        {
            Color1,
            Color2,
            Color1_2,
            Color2_1,
            Color1y2,
            Color1Mas,
            Color2Mas
        };
    }
}
