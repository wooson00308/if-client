namespace IF.Stat
{
    public interface IStat
    {
        StatType Type { get; }
        float Value { get; }
        IStat Combine(IStat other); // 같은 타입끼리 합침
    }
}