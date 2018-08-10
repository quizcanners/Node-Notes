using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;

namespace LinkedNotes
{
    
    public class NodeBook : AbstractKeepUnrecognized_STD, IPEGI_ListInspect, IPEGI {

        public string name;
        public int firstFree = 0;
        public CountlessSTD<Base_Node> allBaseNodes = new CountlessSTD<Base_Node>();
        public List<Node> subNodes = new List<Node>();

#if PEGI

        int inspectedNode = -1;

        public override bool PEGI()  {
            bool changed = false;

            "Nodes".edit_List(subNodes, ref inspectedNode, true);

            //if (icon.Add.Click())
              //  subNodes.Add((new Node()).CreatedFor(this));
                   
            return changed;
        }
        
        public bool PEGI_inList(IList list, int ind, ref int edited) {
           var changed = pegi.edit(ref name);

            if (icon.Edit.Click())
                edited = ind;
            return changed;
        }
#endif

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("f", firstFree)
            .Add("s", subNodes)
            .Add_String("n", name)
            .Add("in", inspectedNode);

        public override bool Decode(string tag, string data) {
            switch (tag)
            {
                case "f": firstFree = data.ToInt(); break;
                case "s": data.DecodeInto(out subNodes); break;
                case "n": name = data; break;
                case "in": inspectedNode = data.ToInt(); break;
                default: return false;
            }
            return true;
        }
    
        public override ISTD Decode(string data) {
            var ret = data.DecodeTagsFor(this);

            foreach (var s in subNodes)
                s.Init(this, null);

            return ret;
        }

    }
}