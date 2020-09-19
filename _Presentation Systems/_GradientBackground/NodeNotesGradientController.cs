using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual {

    [ExecuteAlways]
    public class NodeNotesGradientController : PresentationSystemsAbstract, ILinkedLerping, ICfgCustom
    {
        public override string ClassTag => "GradCntrl";

        public static NodeNotesGradientController instance;

        private LinkedLerp.ShaderColorValueGlobal bgColUp = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_1");
        private LinkedLerp.ShaderColorValueGlobal bgColCnter = new LinkedLerp.ShaderColorValueGlobal("_BG_CENTER_COL");
        private LinkedLerp.ShaderColorValueGlobal bgColDown = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_2");
        
        public void SetTarget()
        {
            _lerpDone = false;
        }

        #region Linked Lerp
        LerpData ld = new LerpData();

  
        public void Lerp(LerpData ld, bool canSkipLerp) {

            bgColUp.Lerp(ld);
            bgColCnter.Lerp(ld);
            bgColDown.Lerp(ld);

        }

        public void Portion(LerpData ld) {
            bgColUp.Portion(ld);
            bgColCnter.Portion(ld);
            bgColDown.Portion(ld);
        }
        #endregion

        private bool _lerpDone;

        public void Update() {

            if (!_lerpDone)
            {
                ld.Reset();

                Portion(ld);

                Lerp(ld, false);

                if (ld.MinPortion > 0.999f)
                    _lerpDone = true;
            }

        }
        
        public override void ManagedOnEnable()
        {
            instance = this;
        }
        
        #region Inspector

        public override bool Inspect()
        {
            var changed = false;
            
            "Background Up".edit(ref bgColUp.targetValue).nl(ref changed);
            "Background Center".edit(ref bgColCnter.targetValue).nl(ref changed);
            "Background Down".edit(ref bgColDown.targetValue).nl(ref changed);
            
            if (changed)
                SetTarget();
                
            if (changed)
                _lerpDone = false;

            return changed;
        }

        public override string NameForDisplayPEGI() => "Gradient Background";

        #endregion

        #region Encode & Decode
       
        public void Decode(CfgData data)
        {
            this.DecodeTagsFrom(data);
            _lerpDone = false;
        }

        public override void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "bgUp": bgColUp.TargetValue = data.ToColor(); break;
                case "bgc": bgColCnter.TargetValue = data.ToColor(); break;
                case "bgDwn": bgColDown.TargetValue = data.ToColor(); break;
            }
        }

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("bgUp", bgColUp.TargetValue)
            .Add("bgc", bgColCnter.TargetValue)
            .Add("bgDwn", bgColDown.TargetValue);


        #endregion

    }
}
