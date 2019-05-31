using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QcTriggerLogic;
using QuizCannersUtilities;
using PlayerAndEditorGUI;

namespace NodeNotes
{

    public class NodeButtonComponent : Base_Node
    {

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                results.Apply(Values.global);
        }
        
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b": data.Decode_Base(base.Decode, this); break;
                default: return false;
            }
            return true;
        }



#if !NO_PEGI

        protected override icon InspectorIcon => icon.Done;

        protected override string InspectionHint => "Inspect Button";

        protected override string ResultsRole => "On Click";

        public override bool Inspect()
        {
            "BUTTON".nl();

            bool changed = base.Inspect();

            return changed;
        }
#endif
    }
}
