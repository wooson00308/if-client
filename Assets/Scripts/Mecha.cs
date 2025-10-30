using UnityEngine;
using System.Collections.Generic;
using IF.Stat;
using IF.Assembly;

namespace IF
{

    public class Mecha : MonoBehaviour, IHitable, IDashable
    {
        #region Fields

        private readonly Dictionary<FrameSlotType, IAssembly> frames = new()
        {
            { FrameSlotType.Head, null },
            { FrameSlotType.Arms, null },
            { FrameSlotType.Body, null },
            { FrameSlotType.Legs, null },
        };

        private readonly Dictionary<WeaponSlotType, IAssembly> weapons = new()
        {
            { WeaponSlotType.L_Arm, null },
            { WeaponSlotType.L_Shoulder, null },
            { WeaponSlotType.R_Arm, null },
            { WeaponSlotType.R_Shoulder, null },
        };

        private readonly Dictionary<AbilitySlotType, IAssembly> abilities = new()
        {
            { AbilitySlotType.Primary, null },
            { AbilitySlotType.Secondary, null },
            { AbilitySlotType.Support, null },
        };

        private readonly List<StatModifier> modifiers = new();

        public float CurrentArmor { get; private set; } = 0f;
        public float CurrentEnergy { get; private set; } = 0f;

        public float DashEnergyCost
        {
            get
            {
                var totals = GetTotalStats();
                if (totals.TryGetValue(StatType.DashEnergyCost, out var dashCostStat))
                {
                    return dashCostStat.Value;
                }
                return 0f;
            }
        }

        #endregion

        #region Unity Methods

        private void Update()
        {
            UpdateModifiers();
            ApplyRegeneration();
        }

        #endregion

        public Dictionary<StatType, IStat> GetTotalStats()
        {
            var map = new Dictionary<StatType, IStat>();

            void Accumulate(IStat stat)
            {
                if (map.TryGetValue(stat.Type, out var existing))
                    map[stat.Type] = existing.Combine(stat);
                else
                    map[stat.Type] = stat;
            }

            void AccumulateAllFrom(IEnumerable<IAssembly> parts)
            {
                foreach (var part in parts)
                {
                    if (part == null) continue;
                    foreach (var s in part.Stats)
                        Accumulate(s);
                }
            }

            AccumulateAllFrom(frames.Values);
            AccumulateAllFrom(weapons.Values);
            AccumulateAllFrom(abilities.Values);

            return map;
        }

        private void ClampCurrentValuesToMax()
        {
            var totals = GetTotalStats();
            if (totals.TryGetValue(StatType.Armor, out var h)) CurrentArmor = Mathf.Clamp(CurrentArmor, 0f, h.Value);
            if (totals.TryGetValue(StatType.Energy, out var e)) CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0f, e.Value);
        }

        private void ApplyRegeneration()
        {
            var totals = GetTotalStats();

            if (totals.TryGetValue(StatType.Energy, out var energyStat))
            {
                float regenE = 0f;
                if (energyStat is IRegeneratingStat eregen) regenE = eregen.RegenPerSecond;
                CurrentEnergy = Mathf.Min(energyStat.Value, CurrentEnergy + regenE * Time.deltaTime);
            }
        }

        public void Hit(float amount)
        {
            CurrentArmor = Mathf.Max(0f, CurrentArmor - amount);
        }

        public void UseEnergy(float amount)
        {
            CurrentEnergy = Mathf.Max(0f, CurrentEnergy - amount);
        }

        public void Dash(Vector3 dir)
        {
            UseEnergy(DashEnergyCost);

            // 대시 구현은 추후에
        }

        #region Stat Modifiers

        /// <summary>
        /// 현재 스탯에 일시적인 수정을 가함.
        /// 버프/디버프 용도로 사용 가능
        /// </summary>
        /// <param name="modifier">적용할 스탯 값</param>
        public void ModifyStat(StatModifier modifier)
        {
            if (modifier == null) return;
            if (modifier.RemainingSeconds <= 0f)
            {
                return; // 유한 시간도 아니고 영구도 아님
            }
            if (modifier.Value == 0f)
            {
                return; // 효과 없음
            }

            modifiers.Add(modifier);
            ClampCurrentValuesToMax();
        }

        public void RemoveAllStatModifiers()
        {
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (!modifiers[i].IsPermanent)
                    modifiers.RemoveAt(i);
            }
            ClampCurrentValuesToMax();
        }

        private void UpdateModifiers()
        {
            bool removed = false;
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                var m = modifiers[i];
                if (m.IsPermanent) continue;
                m.RemainingSeconds -= Time.deltaTime;
                if (m.RemainingSeconds <= 0f)
                {
                    modifiers.RemoveAt(i);
                    removed = true;
                }
            }
            if (removed) ClampCurrentValuesToMax();
        }

        #endregion

        #region Equipment

        public IAssembly EquipFrame(Frame newFrame)
        {
            if (newFrame == null) return null;

            if (frames.TryGetValue(newFrame.Slot, out var existing))
            {
                OnAssemblyUnequipped(existing);
            }
            frames[newFrame.Slot] = newFrame;
            OnAssemblyEquipped(newFrame);
            ClampCurrentValuesToMax();
            return existing;
        }

        public IAssembly UnequipFrame(FrameSlotType slot)
        {
            if (frames.TryGetValue(slot, out var existing))
            {
                frames.Remove(slot);
                OnAssemblyUnequipped(existing);
                ClampCurrentValuesToMax();
                return existing;
            }
            return null;
        }

        public IAssembly EquipWeapon(Weapon newWeapon)
        {
            if (newWeapon == null) return null;

            if (weapons.TryGetValue(newWeapon.Slot, out var existing))
            {
                OnAssemblyUnequipped(existing);
            }
            weapons[newWeapon.Slot] = newWeapon;
            OnAssemblyEquipped(newWeapon);
            ClampCurrentValuesToMax();
            return existing;
        }

        public IAssembly UnequipWeapon(WeaponSlotType slot)
        {
            if (weapons.TryGetValue(slot, out var existing) && existing != null)
            {
                weapons[slot] = null;
                OnAssemblyUnequipped(existing);
                ClampCurrentValuesToMax();
                return existing;
            }
            return null;
        }

        public Ability EquipAbility(Ability newAbility)
        {
            if (newAbility == null || !newAbility.Slot.HasValue) return null;

            var slot = newAbility.Slot.Value;
            Ability replaced = null;

            if (abilities.TryGetValue(slot, out var existing) && existing != null)
            {
                replaced = existing as Ability;
                OnAssemblyUnequipped(existing);
            }

            abilities[slot] = newAbility;
            OnAssemblyEquipped(newAbility);
            ClampCurrentValuesToMax();

            return replaced;
        }

        public Ability UnequipAbility(AbilitySlotType slot)
        {
            if (abilities.TryGetValue(slot, out var existing) && existing != null)
            {
                abilities[slot] = null;
                OnAssemblyUnequipped(existing);
                ClampCurrentValuesToMax();
                return existing as Ability;
            }

            return null;
        }

        private void OnAssemblyEquipped(IAssembly frame)
        {
            // TODO: 이벤트/사운드/UI 처리
        }

        private void OnAssemblyUnequipped(IAssembly frame)
        {
            // TODO: 이벤트/사운드/UI 처리
        }

        #endregion
    }
}
