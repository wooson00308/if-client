using System.Collections.Generic;
using IF.Stat;

namespace IF.Assembly
{
    public class Weapon : IAssembly
    {
        public string Id { get; }
        public AssemblyType Type => AssemblyType.Weapon;
        public WeaponSlotType Slot { get; }
        public IEnumerable<IStat> Stats { get; }

        public Weapon(string id, WeaponSlotType slot, IEnumerable<IStat> stats)
        {
            Id = id;
            Slot = slot;
            Stats = stats;
        }
    }
}