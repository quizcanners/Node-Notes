using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual {

    [ExecuteAlways]
    public class NodeNotesGradientController : MonoBehaviour, ILinkedLerping {

        public static NodeNotesGradientController instance;

        public BackgroundGradient gradient = new BackgroundGradient();
        
        private LinkedLerp.ShaderColorValueGlobal bgColUp = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_1");
        private LinkedLerp.ShaderColorValueGlobal bgColCnter = new LinkedLerp.ShaderColorValueGlobal("_BG_CENTER_COL");
        private LinkedLerp.ShaderColorValueGlobal bgColDown = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_2");

        public void SetTarget(BackgroundGradient gradient) => this.gradient = gradient;
        
        public void Lerp(LerpData ld, bool canSkipLerp) {

            bgColUp.Lerp(ld);
            bgColCnter.Lerp(ld);
            bgColDown.Lerp(ld);

        }

        public void Portion(LerpData ld) {

            bgColUp.targetValue = gradient.backgroundColorUp;
            bgColCnter.targetValue = gradient.backgroundColorCenter;
            bgColDown.targetValue = gradient.backgroundColorDown;

            bgColUp.Portion(ld);
            bgColCnter.Portion(ld);
            bgColDown.Portion(ld);
        }

        LerpData ld = new LerpData();

        public void Update() {
            ld.Reset();

            Portion(ld);

            Lerp(ld, false);

        }

        public void OnEnable()
        {
            instance = this;
        }

    }
}
