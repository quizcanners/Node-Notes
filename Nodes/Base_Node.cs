using PlayerAndEditorGUI;
using SharedTools_Stuff;
using STD_Logic;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedNotes
{

    [DerrivedList(typeof(Node), typeof(NodeLinkComponent), typeof(NodeButtonComponent))]
    public class Base_Node : AbstractKeepUnrecognized_STD, INeedAttention, IGotName, IGotIndex, IPEGI {
        public Node parentNode;
        public NodeBook root;

        public static bool editingNodes = false;

        public ISTD visualRepresentation;
        public ISTD previousVisualRepresentation;
        public string configForVisualRepresentation;

        int logicVersion = -1;
        bool visConditionsResult = true;
        bool enabledConditionResult = true;
        
        ConditionBranch visCondition = new ConditionBranch();
        ConditionBranch eblCondition = new ConditionBranch();

        void UpdateLogic() {
            if (logicVersion != LogicMGMT.currentLogicVersion) {
                logicVersion = LogicMGMT.currentLogicVersion;

                visConditionsResult = visCondition.TestFor(Values.global);

                enabledConditionResult = eblCondition.TestFor(Values.global);
            }
        }

        public bool Conditions_isVisibile() {
            UpdateLogic();
            return visConditionsResult;
        }

        public bool Conditions_isEnabled() {
            UpdateLogic();
            return visConditionsResult && enabledConditionResult;
        }

        public List<Result> results = new List<Result>();
        
        protected static Nodes_PEGI Mgmt => Nodes_PEGI.NodeMGMT_inst;

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name = "New Node";
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

        public override StdEncoder Encode() => this.EncodeUnrecognized()
        .Add_String("n", name)
        .Add("i", index)
        .Add_ifTrue("ic", editEbl_Conditions)
        .Add_ifTrue("ir", editResults)
        .Add_ifNotNegative("icr", inspectedResult)
        .Add("cnds", eblCondition)
        .Add("vcnds", visCondition)
        .Add("res", results)
        .Add_String("vis", visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": name = data; break;
                case "i": index = data.ToInt(); break;
                case "ic": editEbl_Conditions = data.ToBool(); break;
                case "ir": editResults = data.ToBool(); break;
                case "icr": inspectedResult = data.ToInt(); break;
                case "cnds": data.DecodeInto(out eblCondition); break;
                case "vcnds": data.DecodeInto(out visCondition); break;
                case "res": data.DecodeInto(out results); break;
                case "vis": configForVisualRepresentation = data; break;
            }
            return true;
        }

        int inspectedResult = -1;
        public bool InspectingTriggerStuff => editEbl_Conditions || editResults || editVis_Conditions;
        bool editVis_Conditions = false;
        bool editEbl_Conditions = false;
        bool editResults = false;
        #if !NO_PEGI

        public override bool PEGI()
        {
            var changed = false;
            bool onPlayScreen = pegi.paintingPlayAreaGUI;

            if (!onPlayScreen)
                changed |= base.PEGI();

            if (!showDebug || onPlayScreen)
            {
                if (!InspectingTriggerStuff)
                {
                    changed |= pegi.edit(ref name);
                    if ((this != Mgmt.Cut_Paste) && icon.Copy.Click("Cut/Paste"))
                    {
                        Nodes_PEGI.NodeMGMT_inst.Cut_Paste = this;
                        changed = true;
                    }
                }

                pegi.nl();

                if ("Visibility Conditions".foldout(ref editVis_Conditions).nl())
                {
                    editResults = false;
                    editEbl_Conditions = false;
                    changed |= visCondition.PEGI();
                }

                if ("Enabled Conditions".foldout(ref editEbl_Conditions).nl()) {
                    editResults = false;
                    editVis_Conditions = false;
                    changed |= eblCondition.PEGI();
                }

                if ("Results".foldout(ref editResults).nl()) {
                    editEbl_Conditions = false;
                    editVis_Conditions = false;
                    changed |= results.Inspect(Values.global).nl();
                }
                
                pegi.nl();
            }

            if (changed)
                logicVersion = -1;

            return changed;
        }
#endif


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
