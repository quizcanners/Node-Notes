using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using SharedTools_Stuff;

namespace NodeNotes_Visual
{

    [TaggedType(classTag)]
    [ExecuteInEditMode]
     public class GradientBackgroundController : NodesStyleBase {

        const string classTag = "grad";

        public override string ClassTag => classTag;

        ColorLerpParameter[] cols = new ColorLerpParameter[3];// { new ColorLerpParameter(), new ColorLerpParameter(), new ColorLerpParameter() }; // = new ColorLerpParameter[3];

            public GameObject backPlane;

            bool isSHowing = false;
            
            const float lerpSpeed = 0.5f;

            public static readonly List<string> globalShaderValues = new List<string> { "_BG_GRAD_COL_1", "_BG_GRAD_COL_2", "_BG_CENTER_COL" };
            
            #region Inspector
#if PEGI
            public override bool Inspect()  {

                bool changed = false;

                if (inspectedStuff == -1 && !Application.isPlaying) 
                    "Back Plane".edit(ref backPlane).nl();
                
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
                    case "cols": data.Decode_Array(out cols); break;
                    default: return false;
                }

                return true;
            }

            public override StdEncoder Encode() => this.EncodeUnrecognized()
                .Add("cols", cols);

            #endregion

            #region Updates
            public override void FadeAway() => isSHowing = false;
            
            public override bool TryFadeIn() {
                isSHowing = true;
                if (backPlane)
                    backPlane.SetActive(true);

                return true;
            }

            float lastPortion = 1;
            // Update is called once per frame
            void Update() {

                if (backPlane && backPlane.activeSelf){
                    
                    lastPortion = 1;

                    int to = globalShaderValues.Count;
                    
                    if (Application.isPlaying)
                        for (int i = 0; i < to; i++)
                            cols[i].MinPortion(lerpSpeed, ref lastPortion);

                    for (int i = 0; i < to; i++)
                        Shader.SetGlobalColor(globalShaderValues[i], cols[i].Lerp(lastPortion));
                    
                    if (!isSHowing && lastPortion == 1)
                        backPlane.SetActive(false);
                    
                }
            }

            void OnEnable() {
                if (!backPlane && transform.childCount > 0)
                    backPlane = transform.GetChild(0).gameObject;

            }

            #endregion
        }

    public struct ColorLerpParameter : IPEGI_ListInspect, ISTD {
        Color current;
        Color target;
        bool lerpFinished;

      /*  public ColorLerpParameter() {
            current = Color.black;
            target = Color.black;
            lerpFinished = false;
        }*/

        public float GetRGBAdistance() {
            if (lerpFinished) return 0;
            var d = current.DistanceRGBA(target);
            if (d == 0)
                lerpFinished = true;
            return d;
        }

        public void MinPortion(float speed, ref float portion) {
            if (!lerpFinished)
                speed.SpeedToMinPortion(GetRGBAdistance(), ref portion);
        }

        public Color LerpTo { set { target = value; lerpFinished = false; } }

        public Color Lerp (float portion) {

            if (!lerpFinished) {
                Color.Lerp(current, target, portion);

                if (portion == 1)
                    lerpFinished = true;
            }

            return current;
        }

#if PEGI
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            bool changed = pegi.edit(ref target);
            if (changed)
                current = target;

            return changed;
        }
#endif

        public StdEncoder Encode() => new StdEncoder().Add("t", target);

        public void Decode(string data)
        {
            current = Color.black;
            target = Color.black;
            data.DecodeTagsFor(this);
        }
        public bool Decode(string tag, string data) {
            switch (tag) {
                case "t": target = data.ToColor(); break;
                default: return false;
            }
            return true;
        }
    }

}