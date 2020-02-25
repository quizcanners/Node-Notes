using UnityEngine;

namespace NodeNotes_Visual {

    public class OnMouseOverProxy : MonoBehaviour {

        public NodeCircleController parent;

        public void OnMouseOver() {
            if (parent)
                parent.OnMouseOver();
        }
    }
}