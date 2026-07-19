using System.Collections.Generic;

namespace Shared
{
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
