using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;

namespace LinkedNotes
{

    //Have different classes to hold different forms of a book (Encoded, Decoded, Link, FileAdress)

    [DerrivedList(typeof(NodeBook), typeof(BookMark))]
    public class NodeBook : Node, IPEGI_ListInspect {

        public int firstFree = 0;
        public CountlessSTD<Node> allBookNodes = new CountlessSTD<Node>();

#if PEGI

        public override bool PEGI()
        {
            bool changed = false;

            changed |= base.PEGI();

            return changed;
        }

        string testString;

        public bool PEGI_inList(IList list, int ind, ref int edited)
        {
           var changed = pegi.edit(ref name);

            if (icon.Edit.Click())
                edited = ind;
            return changed;
        }

#endif

        public NodeBook()
        {
            root = this;
        }

        public void Init() => Init(this);

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add("f", firstFree)
            .Add_String("tst", testString);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break; 
                case "f": firstFree = data.ToInt(); break;
                case "tst": testString = data; break;
                default: return false;
            }

            return true;
        }
    

        public override ISTD Decode(string data)
        {
            var ret = data.DecodeTagsFor(this);

            Init(this);

            return ret;
        }

    
    }
}