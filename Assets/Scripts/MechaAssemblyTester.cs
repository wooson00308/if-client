using System;
using System.Collections.Generic;
using UnityEngine;
using IF.Assembly;
using IF.Stat;

namespace IF
{
    /// <summary>
    /// 간단한 온-런타임 테스트 도구.
    /// 샘플 프레임/무기/어빌리티를 장착한 메카를 생성하고,
    /// OnGUI 버튼을 통해 파츠를 교체할 수 있다.
    /// 플레이 모드에서 SampleScene을 열고 "Mecha Assembly Tester" 오브젝트를 선택하지 않아도
    /// 좌측 상단 UI 패널을 통해 바로 사용 가능하다.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("IF/Mecha Assembly Tester")]
    public class MechaAssemblyTester : MonoBehaviour
    {
        [SerializeField]
        private Transform mechaParent;

        [SerializeField]
        private Vector3 spawnOffset = new(0f, 0f, 0f);

        [SerializeField]
        private float guiWidth = 340f;

        private Mecha activeMecha;

        private readonly Dictionary<FrameSlotType, List<Frame>> frameOptions = new();
        private readonly Dictionary<WeaponSlotType, List<Weapon>> weaponOptions = new();
        private readonly Dictionary<AbilitySlotType, List<Ability>> abilityOptions = new();

        private readonly Dictionary<FrameSlotType, int> frameSelection = new();
        private readonly Dictionary<WeaponSlotType, int> weaponSelection = new();
        private readonly Dictionary<AbilitySlotType, int> abilitySelection = new();

        private Vector2 scrollPosition;
        private GUIStyle headerStyle;

        private void Awake()
        {
            if (mechaParent == null)
            {
                mechaParent = transform;
            }

            BuildSampleLoadouts();
        }

        private void Start()
        {
            EnsureMecha();
        }

        private void EnsureStyles()
        {
            headerStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
        }

        private void BuildSampleLoadouts()
        {
            frameOptions.Clear();
            weaponOptions.Clear();
            abilityOptions.Clear();

            frameSelection.Clear();
            weaponSelection.Clear();
            abilitySelection.Clear();

            foreach (FrameSlotType slot in Enum.GetValues(typeof(FrameSlotType)))
            {
                frameOptions[slot] = CreateFrameSamples(slot);
                frameSelection[slot] = -1;
            }

            foreach (WeaponSlotType slot in Enum.GetValues(typeof(WeaponSlotType)))
            {
                weaponOptions[slot] = CreateWeaponSamples(slot);
                weaponSelection[slot] = -1;
            }

            foreach (AbilitySlotType slot in Enum.GetValues(typeof(AbilitySlotType)))
            {
                abilityOptions[slot] = CreateAbilitySamples(slot);
                abilitySelection[slot] = -1;
            }
        }

        private void EnsureMecha()
        {
            if (activeMecha != null) return;

            SpawnFreshMecha();
            EquipDefaults();
        }

        private void SpawnFreshMecha()
        {
            DestroyMecha();

            var go = new GameObject("Tester Mecha");
            go.transform.SetParent(mechaParent, false);
            go.transform.localPosition = spawnOffset;

            activeMecha = go.AddComponent<Mecha>();
        }

        private void DestroyMecha()
        {
            if (activeMecha == null) return;

            if (Application.isPlaying)
            {
                Destroy(activeMecha.gameObject);
            }
            else
            {
                DestroyImmediate(activeMecha.gameObject);
            }

            activeMecha = null;

            foreach (var key in frameSelection.Keys)
            {
                frameSelection[key] = -1;
            }

            foreach (var key in weaponSelection.Keys)
            {
                weaponSelection[key] = -1;
            }

            foreach (var key in abilitySelection.Keys)
            {
                abilitySelection[key] = -1;
            }
        }

        private void EquipDefaults()
        {
            if (activeMecha == null) return;

            foreach (var pair in frameOptions)
            {
                if (pair.Value.Count > 0)
                {
                    EquipFrame(pair.Key, 0);
                }
            }

            foreach (var pair in weaponOptions)
            {
                if (pair.Value.Count > 0)
                {
                    EquipWeapon(pair.Key, 0);
                }
            }

            foreach (var pair in abilityOptions)
            {
                if (pair.Value.Count > 0)
                {
                    EquipAbility(pair.Key, 0);
                }
            }
        }

        private List<Frame> CreateFrameSamples(FrameSlotType slot)
        {
            return new List<Frame>
            {
                new Frame($"{slot} Vanguard", slot, new IStat[]
                {
                    new SimpleStat(StatType.Armor, 60f),
                    new SimpleStat(StatType.Mobility, slot == FrameSlotType.Legs ? 8f : 2f)
                }),
                new Frame($"{slot} Fortress", slot, new IStat[]
                {
                    new SimpleStat(StatType.Armor, 90f),
                    new SimpleStat(StatType.Mobility, slot == FrameSlotType.Head ? -1f : -4f)
                })
            };
        }

        private List<Weapon> CreateWeaponSamples(WeaponSlotType slot)
        {
            return new List<Weapon>
            {
                new Weapon($"{slot} Pulse", slot, new IStat[]
                {
                    new SimpleStat(StatType.Mobility, 3f),
                    new SimpleStat(StatType.DashEnergyCost, -2f)
                }),
                new Weapon($"{slot} Cannon", slot, new IStat[]
                {
                    new SimpleStat(StatType.Armor, 25f),
                    new SimpleStat(StatType.DashEnergyCost, 4f)
                })
            };
        }

        private List<Ability> CreateAbilitySamples(AbilitySlotType slot)
        {
            return new List<Ability>
            {
                new Ability($"{slot} Booster", slot, new IStat[]
                {
                    new SimpleStat(StatType.Mobility, 6f),
                    new EnergyStat(40f, 5f)
                }),
                new Ability($"{slot} Shield", slot, new IStat[]
                {
                    new SimpleStat(StatType.Armor, 45f),
                    new EnergyStat(20f, 2f)
                })
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            var areaRect = new Rect(10f, 10f, guiWidth, Screen.height - 20f);
            GUILayout.BeginArea(areaRect, GUI.skin.box);

            GUILayout.Label("Mecha Assembly Tester", headerStyle);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            DrawMechaControls();
            DrawFrameControls();
            DrawWeaponControls();
            DrawAbilityControls();
            DrawStatSummary();

            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void DrawMechaControls()
        {
            GUILayout.Space(4f);
            GUILayout.Label("Lifecycle", headerStyle);

            if (activeMecha == null)
            {
                if (GUILayout.Button("Spawn Test Mecha"))
                {
                    EnsureMecha();
                }
            }
            else
            {
                GUILayout.Label($"Active: {activeMecha.name}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Respawn"))
                {
                    SpawnFreshMecha();
                    EquipDefaults();
                }

                if (GUILayout.Button("Destroy"))
                {
                    DestroyMecha();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawFrameControls()
        {
            if (activeMecha == null) return;

            GUILayout.Space(8f);
            GUILayout.Label("Frames", headerStyle);

            foreach (var pair in frameOptions)
            {
                GUILayout.Label(pair.Key.ToString());
                GUILayout.BeginHorizontal();
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    var option = pair.Value[i];
                    var label = frameSelection[pair.Key] == i ? $"● {option.Id}" : option.Id;
                    if (GUILayout.Button(label))
                    {
                        EquipFrame(pair.Key, i);
                    }
                }

                if (GUILayout.Button(frameSelection[pair.Key] >= 0 ? "Unequip" : "None"))
                {
                    UnequipFrame(pair.Key);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawWeaponControls()
        {
            if (activeMecha == null) return;

            GUILayout.Space(8f);
            GUILayout.Label("Weapons", headerStyle);

            foreach (var pair in weaponOptions)
            {
                GUILayout.Label(pair.Key.ToString());
                GUILayout.BeginHorizontal();
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    var option = pair.Value[i];
                    var label = weaponSelection[pair.Key] == i ? $"● {option.Id}" : option.Id;
                    if (GUILayout.Button(label))
                    {
                        EquipWeapon(pair.Key, i);
                    }
                }

                if (GUILayout.Button(weaponSelection[pair.Key] >= 0 ? "Unequip" : "None"))
                {
                    UnequipWeapon(pair.Key);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawAbilityControls()
        {
            if (activeMecha == null) return;

            GUILayout.Space(8f);
            GUILayout.Label("Abilities", headerStyle);

            foreach (var pair in abilityOptions)
            {
                GUILayout.Label(pair.Key.ToString());
                GUILayout.BeginHorizontal();
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    var option = pair.Value[i];
                    var label = abilitySelection[pair.Key] == i ? $"● {option.Id}" : option.Id;
                    if (GUILayout.Button(label))
                    {
                        EquipAbility(pair.Key, i);
                    }
                }

                if (GUILayout.Button(abilitySelection[pair.Key] >= 0 ? "Unequip" : "None"))
                {
                    UnequipAbility(pair.Key);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawStatSummary()
        {
            if (activeMecha == null) return;

            GUILayout.Space(8f);
            GUILayout.Label("Total Stats", headerStyle);

            var totals = activeMecha.GetTotalStats();
            foreach (var pair in totals)
            {
                GUILayout.Label($"{pair.Key}: {pair.Value.Value:0.##}");
            }

            GUILayout.Label($"Current Armor: {activeMecha.CurrentArmor:0.##}");
            GUILayout.Label($"Current Energy: {activeMecha.CurrentEnergy:0.##}");
        }

        private void EquipFrame(FrameSlotType slot, int optionIndex)
        {
            if (activeMecha == null) return;
            if (!frameOptions.TryGetValue(slot, out var options)) return;
            if (optionIndex < 0 || optionIndex >= options.Count) return;

            activeMecha.EquipFrame(options[optionIndex]);
            frameSelection[slot] = optionIndex;
        }

        private void UnequipFrame(FrameSlotType slot)
        {
            if (activeMecha == null) return;
            activeMecha.UnequipFrame(slot);
            frameSelection[slot] = -1;
        }

        private void EquipWeapon(WeaponSlotType slot, int optionIndex)
        {
            if (activeMecha == null) return;
            if (!weaponOptions.TryGetValue(slot, out var options)) return;
            if (optionIndex < 0 || optionIndex >= options.Count) return;

            activeMecha.EquipWeapon(options[optionIndex]);
            weaponSelection[slot] = optionIndex;
        }

        private void UnequipWeapon(WeaponSlotType slot)
        {
            if (activeMecha == null) return;
            activeMecha.UnequipWeapon(slot);
            weaponSelection[slot] = -1;
        }

        private void EquipAbility(AbilitySlotType slot, int optionIndex)
        {
            if (activeMecha == null) return;
            if (!abilityOptions.TryGetValue(slot, out var options)) return;
            if (optionIndex < 0 || optionIndex >= options.Count) return;

            activeMecha.EquipAbility(options[optionIndex]);
            abilitySelection[slot] = optionIndex;
        }

        private void UnequipAbility(AbilitySlotType slot)
        {
            if (activeMecha == null) return;
            activeMecha.UnequipAbility(slot);
            abilitySelection[slot] = -1;
        }
    }
}
