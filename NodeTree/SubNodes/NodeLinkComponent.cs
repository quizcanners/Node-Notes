using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using System;
using QcTriggerLogic;
using PlayerAndEditorGUI;

namespace NodeNotes {

    public class NodeLinkComponent : Base_Node, IPEGI_ListInspect {

        public int linkedNodeIndex = 0;

        public override bool ExecuteInteraction() {

            var node = root.allBaseNodes[linkedNodeIndex] as Node;

            if (node != null) {
                Shortcuts.CurrentNode = node;

                results.Apply(Values.global);

                return true;
            }

            return false;
        }

        public override void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                ExecuteInteraction();
        }

        #region Inspector
        #if !NO_PEGI

        protected override string InspectionHint => "Inspect Node Link";

        protected override string ResultsRole => "On Link Usage";

        protected override icon ExecuteIcon => icon.Link;
        protected override string ExecuteHint => "Execute Transition";

        bool SharedPEGI() => "Node Link ".select_iGotIndex_SameClass<Base_Node, Node>(65, ref linkedNodeIndex, root.allBaseNodes.GetAllObjsNoOrder());

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

        #endif
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