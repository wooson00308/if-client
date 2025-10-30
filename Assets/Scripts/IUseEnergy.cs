namespace IF
{
    public interface IUseEnergy
    {
        public float CurrentEnergy { get; }

        void UseEnergy(float amount);
    }
}