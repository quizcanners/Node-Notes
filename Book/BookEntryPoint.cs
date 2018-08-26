using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;
using STD_Logic;

namespace LinkedNotes
{

    public class BookEntryPoint : AbstractKeepUnrecognized_STD, IPEGI {

        int nodeIndex = -1;

        bool startPoint;

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "s": startPoint = data.ToBool(); break;
                case "ind": nodeIndex = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_Bool("s", startPoint)
            .Add("ind", nodeIndex);


        public override bool PEGI()
        {
            bool changed = false;

            "On Node".select_iGotIndex_SameClass<Base_Node, Node>(ref nodeIndex, NodeBook.inspected.allBaseNodes.GetAllObjsNoOrder()).nl();

            "Game Start Point".toggle(ref startPoint).nl();

            return changed;
        }
    }
}
