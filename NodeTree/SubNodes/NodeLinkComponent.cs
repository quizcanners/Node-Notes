using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace NodeNotes {

    public class NodeLinkComponent : Base_Node, IPEGI_ListInspect {

        public int linkedNodeIndex = 0;
        
        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                TryExecuteLink();
        }

        void TryExecuteLink() {

            var node = root.allBaseNodes[linkedNodeIndex] as Node;

            if (node != null)
                Shortcuts.CurrentNode = node;

            results.Apply(Values.global);
        }

        #region Inspector
        #if PEGI

        protected override string ResultsRole => "On Link Usage";

        bool sharedPEGI() {

            var changed = "Node Link ".select_iGotIndex_SameClass<Base_Node, Node>(65, ref linkedNodeIndex, root.allBaseNodes.GetAllObjsNoOrder());

            if (icon.Play.Click("Execute Transition"))
                TryExecuteLink();

            return changed;
        }

        public bool PEGI_inList(IList list, int ind, ref int edited) => sharedPEGI();

        public override bool Inspect()
        {
            bool changed = base.Inspect();

            changed |= sharedPEGI();

            return changed;
        }

        #endif
        #endregion
        
        #region Encode_Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add("lnk", linkedNodeIndex);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
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