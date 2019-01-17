using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STD_Logic;
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
        
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.Decode_Base(base.Decode, this); break;
                default: return false;
            }
            return true;
        }
#if PEGI

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
