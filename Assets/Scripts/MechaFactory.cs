using System.Collections.Generic;
using IF.Assembly;
using UnityEngine;
namespace IF
{
    public class MechaFactory : MonoBehaviour
    {
        public Mecha CreateMecha()
        {
            return CreateMecha(null);
        }

        public Mecha CreateMecha(IEnumerable<Ability> startingAbilities)
        {
            var mechaGameObject = new GameObject("Mecha");
            var mecha = mechaGameObject.AddComponent<Mecha>();

            if (startingAbilities != null)
            {
                foreach (var ability in startingAbilities)
                {
                    if (ability == null) continue;
                    mecha.EquipAbility(ability);
                }
            }

            return mecha;
        }
    }
}