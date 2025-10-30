using System.Collections.Generic;
using IF.Stat;

namespace IF.Assembly
{
    public interface IAssembly
    {
        string Id { get; }
        AssemblyType Type { get; }
        public IEnumerable<IStat> Stats { get; }
    }
}