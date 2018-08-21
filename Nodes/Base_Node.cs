using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using STD_Logic;

namespace LinkedNotes
{

    [DerrivedList(typeof(Node), typeof(NodeLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognized_STD, INeedAttention, IGotName, IGotIndex {
        public Node parentNode;
        public NodeBook root;

        public ISTD visualRepresentation;
        public string configForVisualRepresentation;
        
        public ConditionBranch condition = new ConditionBranch();
        public List<Result> results = new List<Result>();

        protected static Nodes_PEGI MGMT => Nodes_PEGI.NodeMGMT_inst;

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name;
        public string NameForPEGI {
            get { return name; }
            set { name = value; }
        }

        public virtual void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && parentNode != null)
                parentNode.inspectedSubnode = parentNode.subNotes.IndexOf(this);
            
            if (Input.GetMouseButtonDown(1) && parentNode != null )
                parentNode.SetInspectedUpTheHierarchy(null);

        }

        public override StdEncoder Encode() => new StdEncoder()
        .Add_String("n", name)
        .Add("i", index)
        .Add_ifTrue("ic", editConditions)
        .Add_ifTrue("ir", editResults)
        .Add_ifNotNegative("icr", inspectedResult)
        .Add("cnds", condition)
        .Add("res", results)
        .Add_String("vis", visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "n": name = data; break;
                case "i": index = data.ToInt(); break;
                case "ic": editConditions = data.ToBool(); break;
                case "ir": editResults = data.ToBool(); break;
                case "icr": inspectedResult = data.ToInt(); break;
                case "cnds": data.DecodeInto(out condition); break;
                case "res": data.DecodeInto(out results); break;
                case "vis": configForVisualRepresentation = data; break;
            }
            return true;
        }
      
        int inspectedResult = -1;
        protected bool editConditions = false;
        protected bool editResults = false;
        public override bool PEGI()
        {
            var changed = base.PEGI();

            if (!showDebug)
            {
                "{0}: ".F(index).edit(20, ref name);
                if (icon.Copy.Click("Cut/Paste").nl())
                    Nodes_PEGI.NodeMGMT_inst.Cut_Paste = this;

                if ("Conditions".foldout(ref editConditions).nl()) {
                    editResults = false;
                    condition.PEGI();
                }

                if ("Results".foldout(ref editResults).nl()) {
                    editConditions = false;
                    results.Inspect(Values.global).nl();
                }
                
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
          
            root.firstFree += 1;

            Init(r, null);

            return this;
        }

        public virtual void Init(NodeBook r, Node parent)
        {
            root = r;
            if (parent != null)
                parentNode = parent;
            root.allBaseNodes[index] = this;
        }
    }
}
