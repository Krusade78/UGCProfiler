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
        private readonly byte currentModes;
        private readonly uint currentJoy;
        private readonly Shared.ProfileModel.DeviceInfo.CUsage usage;

        public HatEditor(uint joyId, byte mode, Shared.ProfileModel.DeviceInfo.CUsage usage)
        {
            InitializeComponent();
            currentModes = mode;
            currentJoy = joyId;
            this.usage = usage;
            //pov = idSeta;
        }

        public static async System.Threading.Tasks.Task Show(uint joyId, byte mode, Shared.ProfileModel.DeviceInfo.CUsage usage)
        {
            HatEditor content = new(joyId, mode, usage);
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
            string[] st8 = [ Translate.Get("dx_hat_n"), Translate.Get("dx_hat_ne"), Translate.Get("dx_hat_e"), Translate.Get("dx_hat_se"), Translate.Get("dx_hat_s"), Translate.Get("dx_hat_sw"), Translate.Get("dx_hat_w"), Translate.Get("dx_hat_nw")];
            for (byte i = 0; i < 8; i++)
            {
                st8[i] = st8[i].Replace("%", NumericUpDownJ.Value.ToString()).Replace("$", NumericUpDown1.Value.ToString());
            }

            string[] st4 = [st8[0], st8[2], st8[4], st8[6]];


            for (byte i = 0; i <= usage.Range; i++)
            {
                ushort idx = 0;
                foreach (Shared.ProfileModel.MacroModel ar in parent.GetData().Profile.Macros)
                {
                    if (ar.Name == (usage.Range == 3 ? st4[i] : st8[i]))
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
                        Name = usage.Range == 3 ? st4[i] : st8[i],
                    };
                    //'text x52
                    ar.Commands.Add((byte)CommandType.X52MfdTextIni + (3 << 8)); //line
                    byte[] text = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(ar.Name));
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
                    parent.GetData().Profile.Macros.Add(ar);
                }

                if (!parent.GetData().Profile.HatsMap.TryGetValue(currentJoy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetData().Profile.HatsMap.Add(currentJoy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(currentModes, out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(currentModes, mode);
                }
                if (!mode.Buttons.TryGetValue((byte)((usage.Id * 8) + i), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button))
                {
                    button = new();
                    mode.Buttons.Add((byte)((usage.Id * 8) + i), button);
                }
                button.Type = 0;
                button.Actions.Clear();
                button.Actions.Add(idx);
                button.Actions.Add(0);
            }

            parent.GetData().Modified = true;
        }
    }
}
