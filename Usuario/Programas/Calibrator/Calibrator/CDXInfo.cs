using System;

namespace Calibrator
{
    class CDXInfo
    {
        private byte[] botones = new byte[54];
        public int X = 0;
        public int Y = 0;
        public int mjX = 0;
        public int mjY = 0;
        public int Z = 0;
        public int R = 0;
        public int Rx = 0;
        public int Ry = 0;
        public int Slider = 0;
        public int Slider2 = 0;
        public int Rudder = 0;
        public int[] POVs = new int[] { -1, -1, -1, -1 };

        public int limiteR = 255;
        public int limitexy = 4095;

        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern byte AbrirDirectInput(IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern void CerrarDirectInput();
        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern byte GetTipoDirectInput();
        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern byte PollDirectInput(byte[] buf);
        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern byte CalibrarDirectInput(byte b);

        public CDXInfo(System.Windows.Window wnd)
        {
            byte t = GetTipoDirectInput();
            if (t == 0) return;
            if (t == 5)
            {
                limitexy = 2047;
                limiteR = 1023;
            }
            Apertura(new System.Windows.Interop.WindowInteropHelper(wnd).Handle);
        }
        private void Apertura(IntPtr h)
        {
            if (AbrirDirectInput(h) == 0)
                System.Windows.MessageBox.Show("Error al abrir DirectX", "Erroe", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        }

        public void Cerrar()
        {
            CerrarDirectInput();
        }

        public void Poll()
        {
            byte[] b = new byte[272];
            if (PollDirectInput(b) == 1)
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(272);
                System.Runtime.InteropServices.Marshal.Copy(b, 0, ptr, 272);
                this.X = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 0); //js.lX
                this.Y = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 4); //js.lY
                this.Z = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 8); //js.lZ
                this.Rx = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 12); //js.lRx
                this.Ry = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 16); //js.lRy
                this.R = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 20); //js.lRz
                this.Slider = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 24); //js.rglSlider(0)
                this.Slider2 = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 28); //js.rglSlider(1)
                this.POVs[0] = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 32); //js.rgdwPOV(0)
                this.POVs[1] = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 36); //js.rgdwPOV(1)
                this.POVs[2] = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 40); //js.rgdwPOV(2)
                this.POVs[3] = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 44); //js.rgdwPOV(3)
                for (byte i = 0; i < 56; i++)
                {
                    botones[i] = System.Runtime.InteropServices.Marshal.ReadByte(ptr, 48 + i); //js.rgbButtons(i)
                }
                this.mjX = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 176);
                this.mjY = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 180);
                this.Rudder = System.Runtime.InteropServices.Marshal.ReadInt32(ptr, 184);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            }
        }
        public bool GetBoton(int b)
        {
            return (botones[b] == 1) ? true : false;
        }

        public byte Calibrar()
        {
            if (limitexy == 2047)
                return CalibrarDirectInput(1);
            else
                return CalibrarDirectInput(0);
        }
    }
}
