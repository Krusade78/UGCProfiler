using System.Collections.Generic;

namespace Comunes.Calibrado
{
    public class CCalibrado
    {
        public List<Limites> Limites { get; set; } = new();
        public List<Jitter> Jitters { get; set; } = new();
    }
}
