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

        public override bool PEGI()
        {
            bool changed = base.PEGI();

            return changed;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add("rslts", results);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                case "rslts": data.DecodeInto(out results); break;
                default: return false;
            }
            return true;
        }

    }
}