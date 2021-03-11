using UnityEngine;

namespace NodeNotes_Visual
{
    public class GameControllerBase : MonoBehaviour
    {
        public virtual void Initialize()
        {
            gameObject.SetActive(false);

        }

    }
}
