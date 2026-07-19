using System;
using System.Collections.Generic;

namespace ugcp_svc.Profile
{
    sealed class CProfile : IDisposable
    {
        private readonly CCalibration calibration = new();
        private CProgramming profile = new();
        private CStatus status = new();
        private byte _rawMode = 0;
        //private byte _calibrationMode = 0;
        private Action<HashSet<uint>>? RefreshHidInputDevice = null;
        private Action<bool>? PauseWinUSB = null;

        void IDisposable.Dispose()
        {
            ((IDisposable)calibration).Dispose();
            ClearProfile();
        }

        private void ClearProfile()
        {
            CProgramming closeProfile = new();
            System.Threading.Interlocked.Exchange(ref profile, closeProfile);
            CStatus closeStatus = new();
            System.Threading.Interlocked.Exchange(ref status, closeStatus);
            ProcessOutput.CProcessOutput.Get()?.ClearEvents();
        }

        //public bool CalibrationMode
        //{
        //    get => System.Threading.Interlocked.CompareExchange(ref _calibrationMode, 0, 0) == 1;
        //    set
        //    {
        //        System.Threading.Interlocked.Exchange(ref _calibrationMode, (byte)(value ? 1 : 0));
        //        PauseWinUSB?.Invoke(value);
        //    }
        //}

        public bool RawMode
        {
            get => System.Threading.Interlocked.CompareExchange(ref _rawMode, 0, 0) == 1;
            set
            {
                System.Threading.Interlocked.Exchange(ref _rawMode, (byte)(value ? 1 : 0));
                PauseWinUSB?.Invoke(value);
            }
        }

        public CProgramming GetProfile() { return profile; }

        public CCalibration GetCalibration() { return calibration; }

        public CStatus GetStatus() { return status; }

        public void SetRefreshDevicesCallback(Action<HashSet<uint>> FnRefreshHidInputDevice)
        {
            RefreshHidInputDevice = FnRefreshHidInputDevice;
        }

        public void SetPauseWinUSBCallback(Action<bool> FnPauseWinUSB)
        {
            PauseWinUSB = FnPauseWinUSB;
        }

        public bool WriteProfile(ReadOnlySpan<char> SystemBuffer)
        {
            ClearProfile();
            CProgramming newProfile = new();

            if (SystemBuffer.Length < (17))
            {
                return false;
            }
            else
            {
                byte[] txtBuffer = new byte[17];
                txtBuffer[0] = (byte)SystemBuffer[0];
                string pfName = SystemBuffer.Slice(1, 16).ToString().TrimEnd('\0');
                pfName = pfName.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] text = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(pfName));
                for (byte i = 0; i < 16; i++)
                {
                    if (text.Length >= (i + 1))
                        txtBuffer[i + 1] = text[i];
                    else
                        txtBuffer[i + 1] = 0;
                }

                txtBuffer[0] = 1;
                X52.CX52Write.Get()?.Set_Text(txtBuffer);
                ReadOnlySpan<byte> cmd2 = [2, 0];
                X52.CX52Write.Get()?.Set_Text(cmd2);
                ReadOnlySpan<byte> cmd3 = [3, 0];
                X52.CX52Write.Get()?.Set_Text(cmd3);
            }
            var jsonObject = System.Text.Json.JsonSerializer.Deserialize<Shared.ProfileModel>(SystemBuffer[17..]);
            if (jsonObject == null)
            {
                return false;
            }

            HF_IoWriteCommands(jsonObject.Macros, newProfile);
            HF_IoWriteMap(jsonObject, newProfile);
            System.Threading.Interlocked.Exchange(ref profile, newProfile);
            status.Reset(jsonObject);

            HashSet<uint> newDevices = [];
            foreach (uint d in jsonObject.ButtonsMap.Keys) { newDevices.Add(d); }
            foreach (uint d in jsonObject.HatsMap.Keys) { newDevices.Add(d); }
            foreach (uint d in jsonObject.AxesMap.Keys) { newDevices.Add(d); }

            RefreshHidInputDevice?.Invoke(newDevices);
            return true;
        }

        private static void HF_IoWriteMap(Shared.ProfileModel jsonObject, CProgramming newProfile)
        {
            newProfile.MouseTick = jsonObject.MouseTick;
            newProfile.ButtonsMap.LoadProfile(jsonObject.ButtonsMap);
            newProfile.HatsMap.LoadProfile(jsonObject.HatsMap);
            newProfile.AxesMap.LoadProfile(jsonObject.AxesMap);
        }

        private static void HF_IoWriteCommands(List<Shared.ProfileModel.MacroModel> jsonObject, CProgramming newProfile)
        {
            X52.CMFDMenu.Get()?.SetHourActivated(true);
            X52.CMFDMenu.Get()?.SetDateActivated(true);

            for (int m = 0; m < jsonObject.Count; m++)
            {
                var macro = jsonObject[m];
                EventQueue.EV_COMMAND[] cmds = new EventQueue.EV_COMMAND[macro.Commands.Count];

                for (int i = 0; i < cmds.LongLength; i++)
                {
                    ref var mem = ref cmds[i];
                    mem.Type = (byte)(macro.Commands[i] & 0xff);
                    mem.Data.Basic.Data1 = (byte)((macro.Commands[i] >> 8) & 0xff);
                    mem.Data.Basic.Data2 = (byte)((macro.Commands[i] >> 16) & 0xff);

                    byte extra = (byte)((macro.Commands[i] >> 24) & 0xff);
                    mem.Data.Basic.Extra = (byte)(extra & 0xf);
                    mem.Data.Basic.OutputJoy = (byte)(extra >> 4);
                    if ((mem.Type == EventQueue.CommandType.X52MfdHour) || (mem.Type == EventQueue.CommandType.X52MfdHour24))
                    {
                        X52.CMFDMenu.Get()?.SetHourActivated(false);
                    }
                    else if (mem.Type == EventQueue.CommandType.MfdDate)
                    {
                        X52.CMFDMenu.Get()?.SetDateActivated(false);
                    }
                }
                newProfile.WriteActions().Add(cmds);
            }
        }

        public void WriteAntivibration(ReadOnlySpan<char> ptr)
        {
            calibration.BeginCalibrationRead();
            {
                var jsonObject = System.Text.Json.JsonSerializer.Deserialize<List<Shared.Calibration.LowLevel.JoyJitters>>(ptr);
                if (jsonObject != null)
                {
                    calibration.Jitters = jsonObject;
                }
            }
            calibration.EndCalibrationRead();
        }

        public void WriteCalibration(ReadOnlySpan<char> ptr)
        {
            calibration.BeginCalibrationRead();
            {
                var jsonObject = System.Text.Json.JsonSerializer.Deserialize<List<Shared.Calibration.LowLevel.JoyLimits>>(ptr);
                if (jsonObject != null)
                {
                    calibration.Limits = jsonObject;
                }
            }
            calibration.EndCalibrationRead();
        }
    }
}
