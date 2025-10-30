namespace IF.Stat
{
    public interface IRegeneratingStat
    {
        // 초당 리젠량 반환 (없으면 0 반환)
        float RegenPerSecond { get; }
    }
}