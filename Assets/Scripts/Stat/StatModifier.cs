namespace IF.Stat
{
    public class StatModifier
    {
        public StatType Type;
        public float Value;

        /// <summary>
        /// 유한 시간: >= 0,
        /// 영구: float.PositiveInfinity
        /// </summary>
        public float RemainingSeconds;
        public bool IsPermanent => float.IsPositiveInfinity(RemainingSeconds);
    }
}