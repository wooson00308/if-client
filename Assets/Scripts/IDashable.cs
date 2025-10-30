using UnityEngine;

namespace IF
{
    public interface IDashable : IUseEnergy
    {
        // 대시 에너지 소비량
        public float DashEnergyCost { get; }

        void Dash(Vector3 dir);
    }
}