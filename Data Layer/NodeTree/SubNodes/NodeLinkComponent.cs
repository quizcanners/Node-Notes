using System.Collections;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes {

#pragma warning disable IDE0019 // Simplify 'default' expression


    public class NodeLinkComponent : Base_Node, IPEGI_ListInspect {

        public int linkedNodeIndex;

        public override bool ExecuteInteraction() {

            var node = parentBook.allBaseNodes[linkedNodeIndex] as Node;

            if (node != null) {
                Shortcuts.CurrentNode = node;

                results.Apply(Values.global);

                return true;
            }

            return false;
        }

        public override bool OnMouseOver(bool click)
        {
            if (click && Conditions_isEnabled())
                return ExecuteInteraction(); 
            
            return false;
        }

        #region Inspector

        protected override string InspectionHint => "Inspect Node Link";

        protected override string ResultsRole => "On Link Usage";

        protected override icon ExecuteIcon => icon.Link;
        protected override string ExecuteHint => "Execute Transition";

        bool SharedPEGI() => "Node Link ".select_iGotIndex_SameClass<Base_Node, Node>(65, ref linkedNodeIndex, parentBook.allBaseNodes.GetAllObjsNoOrder());

        public override bool InspectInList(IList list, int ind, ref int edited)
        {
            var changes = SharedPEGI();

            base.InspectInList(list, ind, ref edited).changes(ref changes);

            return changes;
        }

        public override bool Inspect()
        {
            bool changed = base.Inspect();

            SharedPEGI().changes(ref changed);

            return changed;
        }
        
        #endregion
        
        #region Encode_Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("lnk", linkedNodeIndex);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "lnk": linkedNodeIndex = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        #endregion

    }
}