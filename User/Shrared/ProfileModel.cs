using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class ProfileModel
    {
        public byte MouseTick { get; set; } = 1;

        public Dictionary<uint, ButtonMapModel> ButtonsMap { get; set; } = []; //JoyId key

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
                    public byte Type {  get; set; }
                    public byte OutputAxis { get; set; }
                    public byte[] Sensibility { get; set; } = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
                    public bool IsSensibilityForSlider { get; set; }
                    public List<byte> Bands { get; set; } = []; //band position %
                    public List<ushort> Actions { get; set; } = []; //macro index
                    public (byte, byte) Toughness { get; set; } = (1, 1);//increment/decrement type
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
    }
}
