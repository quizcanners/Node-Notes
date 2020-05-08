using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual {

    [ExecuteAlways]
    public class NodeNotesGradientController : NodeNodesNeedEnableAbstract, ILinkedLerping
    {
        public override string ClassTag => "GradCntrl";

        public static NodeNotesGradientController instance;

        public BackgroundGradient targetGradient = new BackgroundGradient();

        private LinkedLerp.ShaderColorValueGlobal bgColUp = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_1");
        private LinkedLerp.ShaderColorValueGlobal bgColCnter = new LinkedLerp.ShaderColorValueGlobal("_BG_CENTER_COL");
        private LinkedLerp.ShaderColorValueGlobal bgColDown = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_2");

        public ShaderProperty.FloatValue bgTransparency = new ShaderProperty.FloatValue("_NodeNotes_Gradient_Transparency");

        public void SetTarget(BackgroundGradient gradient)
        {
            targetGradient = gradient;
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
            bgColUp.Portion(ld, targetGradient.backgroundColorUp);
            bgColCnter.Portion(ld, targetGradient.backgroundColorCenter);
            bgColDown.Portion(ld, targetGradient.backgroundColorDown);
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

        public bool InspectNode(Base_Node source)
        {
            var changed = false;

            var grds = perNodeConfigs;

            var gradient = grds[source.IndexForPEGI];

            if (gradient == null)
            {
                if ("+ Gradient Cfg".Click().nl(ref changed))
                    grds[source.IndexForPEGI] = targetGradient.Encode().ToString();
            }
            else
            {
                if (icon.Delete.Click("Delete Gradient Cfg"))
                    grds[source.IndexForPEGI] = null;
                else
                {
                    if (source == Shortcuts.CurrentNode)
                    {
                        var crntGrad = targetGradient;

                        if (crntGrad.Nested_Inspect(ref changed))
                        {
                            SetTarget(crntGrad);
                            grds[source.IndexForPEGI] = crntGrad.Encode().ToString();
                        }
                    }
                    else
                        "Enter node to edit it's Background gradient".writeHint();
                }
                pegi.nl();
            }

            if (changed)
                _lerpDone = false;

            return changed;
        }

        #endregion

        #region Encode & Decode
        public override CfgEncoder Encode()
        {
           return new CfgEncoder(); // For Gradient you can edit gradient config while not being inside a node. Maybe should change it at some point.
        }

        public override void Decode(string data)
        {
            targetGradient.Decode(data);
            _lerpDone = false;
        }

        public override bool Decode(string tg, string data)
        {
            return false;
        }
        #endregion

    }
}
