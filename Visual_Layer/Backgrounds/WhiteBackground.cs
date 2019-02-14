using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class WhiteBackground : NodesStyleBase {

        const string classTag = "white";

        public override string ClassTag => classTag;

        public bool isFading;

        public Color color = Color.white;

        #region Inspector
#if PEGI
        public string NameForPEGIdisplay => "White Background";

        public override bool Inspect() {
            bool changed = false;

            "Background Color".edit(ref color).nl();

            return changed;
        }
#endif
        #endregion

        #region Encode & Decode

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "col": color = data.ToColor(); break;
                default: return true;

            }
            return false;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("col", color);

        #endregion

        public override void FadeAway() => isFading = true;

        public override bool TryFadeIn() => isFading = false;

        // Update is called once per frame
        void Update()
        {

            if (!isFading && Camera.main) {

                var col = Camera.main.backgroundColor;

                Camera.main.backgroundColor = MyMath.Lerp_bySpeed(col, Color.white, 3);
            }

        }
    }
}