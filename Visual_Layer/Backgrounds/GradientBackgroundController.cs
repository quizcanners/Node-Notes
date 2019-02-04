using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes_Visual
{

    [TaggedType(classTag)]
    [ExecuteInEditMode]
     public class GradientBackgroundController : NodesStyleBase {

        const string classTag = "grad";

        public override string ClassTag => classTag;

        LinkedLerp.MaterialColor[] cols;

        public GameObject backPlane;

        bool isSHowing = false;

        float lerpSpeed = 0.5f;

        void OnEnable()
        {
            if (cols.IsNullOrEmpty())
            {
                cols = new LinkedLerp.MaterialColor[3];
                cols[0] = new LinkedLerp.MaterialColor("_BG_GRAD_COL_1", Color.white, lerpSpeed);
                cols[0] = new LinkedLerp.MaterialColor("_BG_GRAD_COL_2", Color.white, lerpSpeed);
                cols[0] = new LinkedLerp.MaterialColor("_BG_CENTER_COL", Color.white, lerpSpeed);

            }

            if (!backPlane && transform.childCount > 0)
                backPlane = transform.GetChild(0).gameObject;
        }

        #region Inspector
        #if PEGI
        public override bool Inspect()
        {

            bool changed = false;

            if (inspectedStuff == -1 && !Application.isPlaying)
                "Back Plane".edit(ref backPlane).nl();

            if ("Speed".edit(60, ref lerpSpeed).nl(ref changed)) {
                foreach (var c in cols)
                    c.speedLimit = lerpSpeed;
            }

            return changed;
        }
        #endif
        #endregion

        #region Encoding/Decoding

        public override void Decode(string data)
        {
            backPlane.SetActive(true);
            base.Decode(data);
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "sp": lerpSpeed = data.ToFloat(); break;
                default: return false;
            }

           return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("sp", lerpSpeed);

        #endregion

        #region Updates
        public override void FadeAway() => isSHowing = false;

        public override bool TryFadeIn()
        {
            isSHowing = true;
            if (backPlane)
                backPlane.SetActive(true);

            return true;
        }

        float lastPortion = 1;
        LerpData ld = new LerpData();

        void Update()
        {

            if (backPlane && backPlane.activeSelf)
            {
                
                ld.Reset();

                if (Application.isPlaying) {
                    cols.Portion(ld);
                    cols.Lerp(ld);
                    
                }

                if (!isSHowing && lastPortion == 1)
                    backPlane.SetActive(false);

            }
        }

        #endregion
    }

}