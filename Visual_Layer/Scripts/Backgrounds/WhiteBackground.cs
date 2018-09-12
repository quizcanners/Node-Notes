using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using SharedTools_Stuff;

namespace NodeNotes
{

    public class WhiteBackground : ComponentSTD, IManageFading, IGotDisplayName
    {

        public bool isFading;

        public Color color = Color.white;

#if PEGI
        public override bool PEGI()
        {
            bool changed = false;

            "Background Color".edit(ref color).nl();


            return changed;
        }
#endif


        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "col": color = data.ToColor(); break;
                default: return true;

            }
            return false;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("col", color);

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