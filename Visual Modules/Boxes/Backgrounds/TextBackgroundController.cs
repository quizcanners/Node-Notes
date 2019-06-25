using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class TextBackgroundController : NodesStyleBase, IPointerClickHandler
    {
        const string classTag = "textRead";

        public TextMeshProUGUI pTextMeshPro;

        public override bool TryFadeIn()
        {

            return true;
        }

        public override void FadeAway() {

        }

        public void OnPointerClick(PointerEventData eventData)
        {

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                Debug.Log("Clicked " + linkIndex);

                //Application.OpenURL(linkInfo.GetLinkID());
            }
        }

       
    }
}