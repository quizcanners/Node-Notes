using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    public class Node : AbstractKeepUnrecognized_STD , IGotIndex, INeedAttention, IGotName, IPEGI
    {
        protected NodeBook root;
        public string name;
        int index;

        public int IndexForPEGI {get {return index;} set {index = value;}}

        public string NameForPEGI { get { return name; }
             set { name = value; } }

        public List<Node> subNotes = new List<Node>();

        public List<NodeComponent> components = new List<NodeComponent>();

        int inspectedSubnode = -1;
        int inspectedComponent = -1;

        public virtual string NeedAttention()
        {
            if (root == null)
                return "{0} : {1} No root detected".F(index, name);

            foreach (var s in subNotes)
                if (s == null)
                    return "{0} : {1} Got null sub node".F(index, name);
            else
                {
                    var na = s.NeedAttention();
                    if (na != null)
                        return na;
                }
            return null;
        }

        public override bool PEGI() {

            bool changed = base.PEGI();

            var newNode = name.edit_List(subNotes, ref inspectedSubnode, true, ref changed);

            if (newNode != null)
                newNode.Init(root);

            if (inspectedSubnode == -1) { 
                "Components".edit_List(components, ref inspectedComponent, true, ref changed);
            }
            return changed;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("sub", subNotes)
            .Add_String("n", name)
            .Add("i", index);

        public override bool Decode(string tag, string data)
        {
            switch (tag)  {
                case "sub": data.DecodeInto(out subNotes); break;
                case "n": name = data; break;
                case "i": index = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        protected void Init (NodeBook nroot){
            root = nroot;
            root.allBookNodes[index] = this;

            foreach (var sn in subNotes)
            sn.Init(nroot);
        }
        
    }
}