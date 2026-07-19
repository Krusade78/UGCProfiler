using System.Collections.Generic;

namespace Shared.Calibration.HighLevel
{
    public class Model
    {
        public List<Limits> JoyLimits { get; set; } = [];
        public List<Jitter> JoyJitters { get; set; } = [];
    }
}
