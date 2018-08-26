using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using SharedTools_Stuff;

namespace LinkedNotes
{

    public class WhiteBackground : MonoBehaviour, IManageFading, IGotDisplayName
    {

        public bool isFading;

        public void FadeAway() => isFading = true;

        public string NameForPEGIdisplay() => "White Background";

        public bool TryFadeIn() => isFading = false;




        // Update is called once per frame
        void Update()
        {

            if (!isFading && Camera.main != null) {

                var col = Camera.main.backgroundColor;

                Camera.main.backgroundColor = MyMath.Lerp_RGB(col, Color.white, 3);
            }

        }
    }
}