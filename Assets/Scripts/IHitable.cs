namespace IF
{
    public interface IHitable
    {
        public float CurrentArmor { get; }
        void Hit(float amount);
    }
}
