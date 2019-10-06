using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuizCannersUtilities;

namespace NodeNotes_Visual
{

    [ExecuteInEditMode]
    public class CreateNodeButton : MonoBehaviour, ILinkedLerping
    {

        public static bool showCreateButtons = false;
        
        public bool toLeft;
        public RectTransform rectTranform;

        Vector2 startPosition = Vector3.zero;

        private void OnEnable() {

          

            if (!rectTranform)
                rectTranform = GetComponent<RectTransform>();

            startPosition = rectTranform.anchoredPosition;

            startPosition.x = 0;

        }

        LinkedLerp.RectTransformVector2Value position; 
        
        Vector2 Destination => startPosition + (showCreateButtons ? Vector2.zero :  Vector2.right * rectTranform.rect.width * (toLeft ? -1 : 1));

        public void Portion(LerpData ld)
        {
            if (position == null)
                position = new LinkedLerp.RectangleTransformAnchoredPositionValue(rectTranform, 800);
            
            if (showCreateButtons && !gameObject.activeSelf)
                   gameObject.SetActive(true);
            
            position.TargetValue = Destination;

            position.Portion(ld);
        }

        public void Lerp(LerpData ld, bool canSkipLerp)
        {
           position.Lerp(ld);

           if (Application.isPlaying && ld.Portion() == 1 && !showCreateButtons && !showCreateButtons)
                   gameObject.SetActive(false);
                 
        }
    }
}
