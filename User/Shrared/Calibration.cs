using System.Collections.Generic;

namespace Shared.Calibration
{
    public class CCalibration
    {
        public List<Limits> Limits { get; set; } = [];
        public List<Jitter> Jitters { get; set; } = [];
    }
}
