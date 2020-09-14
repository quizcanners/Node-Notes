using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes {

    public class NodeButtonComponent : Base_Node {

        public override bool OnMouseOver(bool click) {
            if (click && Conditions_isEnabled())
            {
                ExecuteInteraction();
                return true;
            }

            return false;
        }
        
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("b", base.Encode);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b": data.DecodeInto(base.Decode); break;
                default: return false;
            }
            return true;
        }

        protected override string InspectionHint => "Inspect Button";

        protected override string ResultsRole => "On Click";

        public override bool Inspect()
        {
            "BUTTON: {0}".F(NameForPEGI).nl();

            bool changed = base.Inspect();

            return changed;
        }

    }
}
