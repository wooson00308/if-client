namespace IF.Stat
{
    public class SimpleStat : IStat
    {
        public StatType Type { get; }
        public float Value { get; }

        public SimpleStat(StatType type, float value)
        {
            Type = type;
            Value = value;
        }

        public virtual IStat Combine(IStat other)
        {
            if (other == null) return this;
            if (other.Type != Type) return this;
            return new SimpleStat(Type, Value + other.Value);
        }
    }
}
