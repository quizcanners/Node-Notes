using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

namespace LinkedNotes
{

    [DerrivedList(typeof(Node), typeof(BookMark), typeof(NodeLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognized_STD, INeedAttention, IGotName, IGotIndex {
        public Node parentNode;
        public NodeBook root;

        protected static Nodes_PEGI MGMT => Nodes_PEGI.NodeMGMT_inst;

        int index;

        public string name;

        public string NameForPEGI
        {
            get { return name; }
            set { name = value; }
        }

        public int IndexForPEGI { get { return index; } set { index = value; } }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
        .Add_String("n", name)
        .Add("i", index);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "n": name = data; break;
                case "i": index = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override bool PEGI()
        {
            var changed = base.PEGI();

            if (!showDebug)
            {
                "{0}: ".F(index).edit(20, ref name);
                if (icon.Copy.Click("Cut/Paste"))
                    Nodes_PEGI.NodeMGMT_inst.Cut_Paste = this;

                pegi.nl();
            }

            return changed;
        }
        
        public virtual string NeedAttention()
        {
            if (root == null)
                return "{0} : {1} No root detected".F(IndexForPEGI, name);

            if (parentNode == null)
                return "{0} : {1} No Parent Node detected".F(IndexForPEGI, name);
            return null;
        }

        public virtual void MoveTo(Node node) {
            parentNode.subNotes.Remove(this);
            parentNode = node;
            parentNode.subNotes.Add(this);
        }

        public virtual Base_Node CreatedFor(Node target) {
            parentNode = target;

            return CreatedFor(target.root);
        }

        public virtual Base_Node CreatedFor(NodeBook r) {
            root = r;

            IndexForPEGI = root.firstFree;
            root.allBaseNodes[index] = this;
            root.firstFree += 1;

            return this;
        }

        public virtual void Init(NodeBook r, Node parent)
        {
            root = r;
            parentNode = parent;
        }
    }
}
