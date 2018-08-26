using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    public class NodeLinkComponent : Base_Node
    {
        public int linkedNodeIndex = 0;
        #if !NO_PEGI

        public override bool PEGI() {
            bool changed = base.PEGI();
            
            changed |= "Link to ".select_iGotIndex_SameClass<Base_Node, Node>(ref linkedNodeIndex, root.allBaseNodes.GetAllObjsNoOrder()).nl();

            return changed;
        }
#endif

        public override void OnMouseOver() {
            if  (Input.GetMouseButtonDown(0) && Conditions_isEnabled()) {
                var node = root.allBaseNodes[linkedNodeIndex] as Node;
                if (node != null)
                Nodes_PEGI.CurrentNode = node;
                results.Apply(Values.global);
            } 
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add("lnk", linkedNodeIndex);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                case "lnk": linkedNodeIndex = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

    }
}