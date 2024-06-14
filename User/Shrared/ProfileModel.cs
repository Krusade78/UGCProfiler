﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class ProfileModel
    {
        public byte MouseTick { get; set; } = 1;

        public Dictionary<uint, ButtonMapModel> ButtonsMap { get; set; } = []; //JoyId key

        public Dictionary<uint, ButtonMapModel> HatsMap { get; set; } = []; //JoyId key, Hat = button Id / 8, 8 positions per Hat 0-7,8-15,16-23,24-31

        public Dictionary<uint, AxisMapModel> AxesMap { get; set; } = []; //JoyId key

        public List<MacroModel> Macros { get; set; } = [];


        public class ButtonMapModel
        {
            public Dictionary<byte, ModeModel> Modes { get; set; } = []; //Mode as key mmmm_ssss binary Mode/Submode

            public class ModeModel
            {
                public Dictionary<byte, ButtonModel> Buttons { get; set; } = []; //Button Id as key
                public class ButtonModel
                {
                    public byte Type { get; set; } = 0;
                    public List<ushort> Actions { get; set; } = [];
                }
            }
        }

        public class AxisMapModel
        {
            public Dictionary<byte, ModeModel> Modes { get; set; } = []; //Mode as key mmmm_ssss binary Mode/Submode

            public class ModeModel
            {
                public Dictionary<byte, AxisModel> Axes { get; set; } = []; //Axis Id as key
                public class AxisModel
                {
                    public byte Mouse { get; set; } = 1;
                    public byte IdJoyOutput { get; set; }
                    public byte Type {  get; set; } //Mapped in bits 0:none, 1:Normal, 10:Inverted, 100:Mini, 1000:Mouse, 10000:Incremental, 100000: Bands
                    public byte OutputAxis { get; set; }
                    public byte[] Sensibility { get; set; } = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
                    public bool IsSensibilityForSlider { get; set; }
                    public List<byte> Zones { get; set; } = []; //zone position %
                    public List<ushort> Actions { get; set; } = []; //macro index
                    public (byte, byte) Resistance { get; set; } = (1, 1);//increment/decrement type
                }
            }

        }

        public class MacroModel
        {
            [MaxLength(32)]
            public ushort Id { get; set; }
            public string Name { get; set; }
            public List<uint> Commands { get; set; } = [];
        }

        public List<DeviceInfo> DevicesIncluded { get; set; } = [];
        public class DeviceInfo
        {
            public class CUsage
            {
                public ushort ReportId { get; set; }
                public ushort ReportIdx { get; set; }
                public byte Id { get; set; }
                public byte Type { get; set; }
                public byte Bits { get; set; }
                public ushort Range { get; set; }
            }

            public uint Id { get; set; }
            public string Name { get; set; }

            public byte NAxes { get; set; }
            public byte NHats { get; set; }
            public ushort NButtons { get; set; }

            public List<CUsage> Usages { get; set; } = []; //sorted by idx
        }
    }
}
