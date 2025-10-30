namespace IF.Stat
{
    public class EnergyStat : IStat, IRegeneratingStat
    {
        public StatType Type => StatType.Energy;
        // Value는 최대 에너지
        public float Value { get; }
        // 초당 리젠량
        public float RegenPerSecond { get; }

        public EnergyStat(float maxEnergy, float regenPerSecond)
        {
            Value = maxEnergy;
            RegenPerSecond = regenPerSecond;
        }

        public IStat Combine(IStat other)
        {
            if (other == null) return this;
            if (other is EnergyStat e)
            {
                // 최대 에너지와 리젠을 각각 합산
                return new EnergyStat(Value + e.Value, RegenPerSecond + e.RegenPerSecond);
            }
            return this;
        }
    }
}