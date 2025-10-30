using System.Collections.Generic;
using IF.Stat;

namespace IF.Assembly
{
    public class Ability : IAssembly
    {
        public string Id { get; }
        public AssemblyType Type => AssemblyType.Ability;
        public AbilitySlotType? Slot { get; }
        public IEnumerable<IStat> Stats { get; }

        public Ability(string id, IEnumerable<IStat> stats)
            : this(id, null, stats)
        {
        }

        public Ability(string id, AbilitySlotType? slot, IEnumerable<IStat> stats)
        {
            Id = id;
            Slot = slot;
            Stats = stats ?? new List<IStat>();
        }
    }
}
