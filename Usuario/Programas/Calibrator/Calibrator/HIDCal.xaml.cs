using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para HIDCal.xaml
    /// </summary>
    internal partial class HIDCal : UserControl
    {
        private int ejeSel = 0;
        private struct STLIMITES
        {
            internal bool cal;
            internal ushort i;
            internal ushort c;
            internal ushort d;
            internal ushort n;
        };
        private struct STJITTER
        {
            internal bool antiv;
            internal long PosElegida;
            internal byte PosRepetida;
            internal byte Margen;
            internal byte Resistencia;
        };

        STJITTER jitter = new STJITTER();
        STLIMITES limites = new STLIMITES();

        public HIDCal()
        {
            InitializeComponent();
            limites.cal = true;
            jitter.antiv = true;
        }

        public void ActualizarEstado(byte[] hidData)
        {
            int posr = (hidData[ejeSel * 1] << 8) | hidData[(ejeSel * 2) + 1];
            txtPosReal.Text = posr.ToString();
            posReal.Margin = new Thickness(posr - 5, 0, 0, 0);

            // Filtrado de ejes
            long pollEje = (ushort)posr;

            if (jitter.antiv)
            {
                // Antivibraciones
                if ((pollEje < (jitter.PosElegida - jitter.Margen)) || (pollEje > (jitter.PosElegida + jitter.Margen)))
                {
                    jitter.PosRepetida = 0;
                    jitter.PosElegida = pollEje;
                }
                else
                {
                    if (jitter.PosRepetida < jitter.Resistencia)
                    {
                        jitter.PosRepetida++;
                        pollEje = jitter.PosElegida;
                    }
                    else
                    {
                        jitter.PosRepetida = 0;
                        jitter.PosElegida = pollEje;
                    }
                }
            }

            if (limites.cal)
            {
                // Calibrado
                ushort ancho1, ancho2;
                ancho1 = (ushort)((limites.c - limites.n) - limites.i);
                ancho2 = (ushort)(limites.d - (limites.c + limites.n));
                if (((pollEje >= (limites.c - limites.n)) && (pollEje <= (limites.c + limites.n))))
                {
                    //Zona nula
                    pollEje = 1024;
                }
                else
                {
                    if (pollEje < limites.i)
                        pollEje = limites.i;
                    if (pollEje > limites.d)
                        pollEje = limites.d;

                    if (pollEje < limites.c)
                    {
                        pollEje = ((limites.c - limites.n) - pollEje);
                        if (ancho1 > ancho2)
                        {
                            pollEje = (pollEje * ancho2) / ancho1;
                        }
                        pollEje = 0 - pollEje;
                    }
                    else
                    {
                        pollEje -= (limites.c + limites.n);
                        if (ancho2 > ancho1)
                            pollEje = ((pollEje * ancho1) / ancho2);
                    }

                    if (ancho2 > ancho1)
                        pollEje = ((pollEje + ancho1) * (2048)) / (2 * ancho1);
                    else
                        pollEje = ((pollEje + ancho2) * (2048)) / (2 * ancho2);
                }
            }

            txtPosCal.Text = pollEje.ToString();
            posCal.Margin = new Thickness(pollEje - 5, 0, 0, 0);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ushort s = 0;
            ushort.TryParse(txtI.Text, out s);
            limites.i = s;
            ushort.TryParse(txtC.Text, out s);
            limites.c = s;
            ushort.TryParse(txtD.Text, out s);
            limites.d = s;
            ushort.TryParse(txtN.Text, out s);
            limites.n = s;

            ushort.TryParse(txtMargen.Text, out s);
            if (s > 255) s = 255;
            jitter.Margen = (byte)s;
            ushort.TryParse(txtResistencia.Text, out s);
            if (s > 255) s = 255;
            jitter.Resistencia = (byte)s;
        }
    }
}
