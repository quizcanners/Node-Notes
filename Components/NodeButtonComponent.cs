﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STD_Logic;
using SharedTools_Stuff;
using PlayerAndEditorGUI;

namespace LinkedNotes
{

    public class NodeButtonComponent : Base_Node
    {

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                results.Apply(Values.global);
        }
        
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode());

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                default: return false;
            }
            return true;
        }
        #if PEGI

        public override bool PEGI()
        {
            "BUTTON".nl();

            bool changed = base.PEGI();

            return changed;
        }
#endif
    }
}