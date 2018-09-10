using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;
using STD_Logic;

namespace LinkedNotes
{
    
    public class NodeBook : NodeBook_Base, IPEGI_ListInspect, IPEGI {
        
        public int firstFree = 0;
        public CountlessSTD<Base_Node> allBaseNodes = new CountlessSTD<Base_Node>();
        public List<BookEntryPoint> entryPoints = new List<BookEntryPoint>();
        public Node subNode; 
        
        int inspectedNode = -1;
        int inspectedEntry = -1;

        public override string NameForPEGI { get => subNode.name; set => subNode.name = value; }

#if PEGI
        public static NodeBook inspected;

        public override bool PEGI()  {
            bool changed = false;
            inspected = this;

            if (subNode.inspectedSubnode == -1)
                "Book Entries".edit_List(entryPoints, ref inspectedEntry);

            changed |= subNode.Nested_Inspect();

            inspected = null;
            return changed;
        }
        
        public bool PEGI_inList(IList list, int ind, ref int edited) {
           var changed = pegi.edit(ref subNode.name);

            if (icon.Edit.Click())
                edited = ind;

            if (icon.Save.Click())
                Shortcuts.books.Offload(this);

            return changed;
        }
#endif

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("f", firstFree)
            .Add("sn", subNode)
            .Add_ifNotNegative("in", inspectedNode)
            .Add_ifNotNegative("inE", inspectedEntry)
            .Add("ep", entryPoints);
          
        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": subNode.name = data; break;
                case "f": firstFree = data.ToInt(); break;
                case "sn": data.DecodeInto(out subNode); break;
                case "in": inspectedNode = data.ToInt(); break;
                case "inE": inspectedEntry = data.ToInt(); break;
                case "ep": data.DecodeInto(out entryPoints); break;
                default: return false;
            }
            return true;
        }
    
        public override ISTD Decode(string data) {
            
           var ret = data.DecodeTagsFor(this);

            if (subNode == null)
                subNode = new Node();

            subNode.Init(this, null);
            
            return ret;
        }

    }
}