using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    public class Node : AbstractKeepUnrecognized_STD , IGotIndex
    {

        NodeBook root;
        int index;

        public int IndexForPEGI {get {return index;} set {index = value;}}

        public List<Node> subNotes = new List<Node>();

        public override bool PEGI()
        {
            bool changed = base.PEGI();

            "Sub Notes".edit_List(subNotes, true);

            return changed;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("sub", subNotes);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "sub": data.DecodeInto(out subNotes); break;
                default: return false;
            }

            return true;
        }

        protected void Init (NodeBook nroot){
            root = nroot;
            nroot.allBookNodes[index] = this;

            foreach (var sn in subNotes)
            sn.Init(nroot);
        }

    }
}