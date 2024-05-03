using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

namespace Profiler.Dialogs
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class HatEditor : Page
    {
        //private byte pov;
        public HatEditor(/*byte idSeta*/)
        {
            InitializeComponent();
            //pov = idSeta;
        }

        public static async System.Threading.Tasks.Task Show(/*Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData, uint joyId, byte axisId*/)
        {
            HatEditor content = new(/*axisData, joyId, axisId*/);
            ContentDialog dlg = new()
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = ((App)Application.Current).GetMainWindow().Root.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Asignación de setas",// Translate.Get("copy_sensibility_curve"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };

            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                content.Save();
            }
        }

        //     private void Button1_Click(object sender, RoutedEventArgs e)
        //     {
        //         Guardar();
        //         this.DialogResult = true;
        //         this.Close();
        //     }

        private void Save()
        {
            MainWindow parent = ((App)Application.Current).GetMainWindow();
            string[] st = [ Translate.Get("dx_hat_n"), Translate.Get("dx_hat_ne"), Translate.Get("dx_hat_e"), Translate.Get("dx_hat_se"), Translate.Get("dx_hat_s"), Translate.Get("dx_hat_sw"), Translate.Get("dx_hat_w"), Translate.Get("dx_hat_nw")];
            //byte idJ = 0, p = 0, m = 0;

            //padre.GetModos(ref idJ, ref p, ref m);
            //pov /= 8;


            for (byte i = 0; i < 8; i++)
                st[i] = st[i].Replace("%", NumericUpDownJ.Value.ToString()).Replace("&", NumericUpDown1.Value.ToString());

            for (byte i = 0; i < 8; i++)
            {
                ushort idx = 0;
                foreach (Shared.ProfileModel.MacroModel ar in parent.GetData().Profile.Macros)
                {
                    if (ar.Name == st[i])
                    {
                        idx = ar.Id;
                        break;
                    }
                }

                if (idx == 0)
                {
                    idx = 0;
                    foreach (Shared.ProfileModel.MacroModel aar in parent.GetData().Profile.Macros)
                    {
                        if (aar.Id > idx)
                            idx = aar.Id;
                    }
                    idx++; //new Id

                    Shared.ProfileModel.MacroModel ar = new()
                    {
                        Id = idx,
                        Name = st[i],
                    };
                    //'text x52
                    ar.Commands.Add((byte)CommandType.X52MfdTextIni + (3 << 8)); //line
                    byte[] text = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st[i]));
                    for (byte j = 0; j < text.Length; j++)
                    {
                        ar.Commands.Add((ushort)((byte)CommandType.X52MfdText + (text[j] << 8)));
                    }
                    ar.Commands.Add((byte)CommandType.X52MfdTextEnd);
                    //remaining
                    uint v = (uint)(((byte)NumericUpDown1.Value - 1) << 16) | (uint)((i * (byte)(NumericUpDownJ.Value - 1)) << 8);
                    ar.Commands.Add((byte)CommandType.DxHat | v);
                    ar.Commands.Add((byte)CommandType.Hold);
                    ar.Commands.Add(((byte)(CommandType.DxHat | CommandType.Release) | v));
                }

                Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button;

                    if (!padre.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                    {
                        buttonMap = new();
                        padre.GetData().Profile.HatsMap.Add(CurrentSel.Joy, buttonMap);
                    }
                    if (!buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                    {
                        mode = new();
                        buttonMap.Modes.Add(GetMode(), mode);
                    }
                    if (!mode.Buttons.TryGetValue(CurrentSel.Usage.Id, out button))
                    {
                        button = new();
                        mode.Buttons.Add((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), button);
                    }

                    padre.GetData().Profile.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, (byte)(i + (pov * 8))).TamIndices = 0;
                padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, (uint)(i + (pov * 8)), 0).idAccion = idx;
                padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, (uint)(i + (pov * 8)), 1).idAccion = 0;
            }

            parent.GetData().Modified = true;
        }
    }
}
