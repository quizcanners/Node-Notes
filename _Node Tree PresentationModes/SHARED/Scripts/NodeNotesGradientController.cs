using QuizCannersUtilities;
using RayMarching;
using UnityEngine;

namespace NodeNotes_Visual {

    [ExecuteAlways]
    public class NodeNotesGradientController : NodeNodesNeedEnableAbstract, ILinkedLerping {

        public static NodeNotesGradientController instance;

        public BackgroundGradient gradient = new BackgroundGradient();
        
        private LinkedLerp.ShaderColorValueGlobal bgColUp = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_1");
        private LinkedLerp.ShaderColorValueGlobal bgColCnter = new LinkedLerp.ShaderColorValueGlobal("_BG_CENTER_COL");
        private LinkedLerp.ShaderColorValueGlobal bgColDown = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_2");

        public ShaderProperty.FloatValue bgTransparency = new ShaderProperty.FloatValue("_NodeNotes_Gradient_Transparency");

        public void SetTarget(BackgroundGradient gradient) => this.gradient = gradient;

        #region Linked Lerp
        LerpData ld = new LerpData();
        public void Lerp(LerpData ld, bool canSkipLerp) {

            bgColUp.Lerp(ld);
            bgColCnter.Lerp(ld);
            bgColDown.Lerp(ld);

        }

        public void Portion(LerpData ld) {
            bgColUp.Portion(ld, gradient.backgroundColorUp);
            bgColCnter.Portion(ld, gradient.backgroundColorCenter);
            bgColDown.Portion(ld, gradient.backgroundColorDown);
        }
        #endregion


        public void Update() {
            ld.Reset();

            Portion(ld);

            Lerp(ld, false);

        }


        public override void ManagedOnEnable()
        {
            instance = this;
        }
    }
}
