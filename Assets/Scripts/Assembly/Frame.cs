using System.Collections.Generic;
using IF.Stat;

namespace IF.Assembly
{
    public class Frame : IAssembly
    {
        public string Id { get; }
        public AssemblyType Type => AssemblyType.Frame;
        public FrameSlotType Slot { get; }
        public IEnumerable<IStat> Stats { get; }

        public Frame(string id, FrameSlotType slot, IEnumerable<IStat> stats)
        {
            Id = id;
            Slot = slot;
            Stats = stats;
        }
    }
}