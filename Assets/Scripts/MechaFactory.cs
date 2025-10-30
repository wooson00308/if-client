using UnityEngine;
namespace IF
{
    public class MechaFactory : MonoBehaviour
    {
        public Mecha CreateMecha()
        {
            var mechaGameObject = new GameObject("Mecha");
            var mecha = mechaGameObject.AddComponent<Mecha>();
            return mecha;
        }
    }
}