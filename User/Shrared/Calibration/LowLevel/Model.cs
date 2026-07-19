using System.Text.Json.Serialization;

namespace Shared.Calibration.LowLevel
{
    public struct JoyLimits
    {
        [JsonInclude]
        public uint JoyId;
        [JsonInclude]
        public CTypes.STLIMITS[] Limits;
    }

    public struct JoyJitters
    {
        [JsonInclude]
        public uint JoyId;
        [JsonInclude]
        public CTypes.STJITTER[] Jitters;
    }
}
