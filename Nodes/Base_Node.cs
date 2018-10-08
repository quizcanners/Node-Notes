using PlayerAndEditorGUI;
using SharedTools_Stuff;
using STD_Logic;
using System.Collections.Generic;
using UnityEngine;

namespace NodeNotes {

    [DerrivedList(typeof(Node), typeof(NodeLinkComponent), typeof(NodeButtonComponent), typeof(BookLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognized_STD, INeedAttention, IGotName, IGotIndex, IPEGI {
        public Node parentNode;
        public NodeBook root;

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name = "New Node";
        public string NameForPEGI {
            get { return name; }
            set { name = value; }
        }

        public static Node CurrentNode { get => Shortcuts.CurrentNode; set => Shortcuts.CurrentNode = value; }
        
        public ISTD visualRepresentation;
        public ISTD previousVisualRepresentation;
        public string configForVisualRepresentation;

        public virtual GameNodeBase AsGameNode => null;

        public virtual void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && parentNode != null)
               parentNode.SetInspectedUpTheHierarchy(this);
        }

        #region Logic

        int logicVersion = -1;
        bool visConditionsResult = true;
        bool enabledConditionResult = true;
        
        ConditionBranch visCondition = new ConditionBranch();
        ConditionBranch eblCondition = new ConditionBranch();

        public List<Result> results = new List<Result>();
        
        void UpdateLogic() {
            if (logicVersion != LogicMGMT.currentLogicVersion) {
                logicVersion = LogicMGMT.currentLogicVersion;

                visConditionsResult = visCondition.IsTrue;

                enabledConditionResult = eblCondition.IsTrue;
            }
        }

        public bool IsOneOfChildrenOf(Node other) {

            if (other == null)
                return false;

            if (parentNode != null && parentNode!= this) {
                if (parentNode == other)
                    return true;
                else
                    return parentNode.IsOneOfChildrenOf(other);
            }

            return false;
        }

        public virtual bool Conditions_isVisibile() {
            UpdateLogic();
            return visConditionsResult;
        }

        public virtual bool Conditions_isEnabled() {
            UpdateLogic();
            return visConditionsResult && enabledConditionResult;
        }

        #endregion
        
        #region Encode_Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
        .Add_String("n", name)
        .Add("i", index)
        .Add("is", inspectedStuff)
        .Add_IfNotNegative("icr", inspectedResult)
        .Add("cnds", eblCondition)
        .Add("vcnds", visCondition)
        .Add("res", results)
        .Add_String("vis", visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": name = data; break;
                case "i": index = data.ToInt(); break;
                case "is": inspectedStuff = data.ToInt(); break;
                case "icr": inspectedResult = data.ToInt(); break;
                case "cnds": data.DecodeInto(out eblCondition); break;
                case "vcnds": data.DecodeInto(out visCondition); break;
                case "res": data.DecodeInto(out results); break;
                case "vis": configForVisualRepresentation = data; break;
            }
            return true;
        }

        #endregion

        #region Inspector


        public virtual string NeedAttention()
        {
            if (root == null)
                return "{0} : {1} No root detected".F(IndexForPEGI, name);

            if (parentNode == null)
                return "{0} : {1} No Parent Node detected".F(IndexForPEGI, name);

            if (parentNode == this)
                return "Is it's own parent";
            return null;
        }

        public static bool editingNodes = false;


        protected int inspectedStuff = -1;
        int inspectedResult = -1;
        public bool InspectingTriggerStuff => inspectedResult != -1;
#if PEGI

      
        
        public override bool Inspect()
        {
            var changed = false;
            bool onPlayScreen = pegi.paintingPlayAreaGUI;

            if (!onPlayScreen)
                changed |= base.Inspect();

            if (!showDebug || onPlayScreen) {
                if (!InspectingTriggerStuff) {
                    changed |= this.inspect_Name();
                    if ((this != Shortcuts.Cut_Paste) && icon.Copy.Click("Cut/Paste"))
                        Shortcuts.Cut_Paste = this;
                }

                pegi.nl();

                if ("Visibility Conditions".fold_enter_exit( ref inspectedStuff, 0).nl())
                    changed |= visCondition.Inspect();
                
                if ("Enabled Conditions".fold_enter_exit(ref inspectedStuff, 1).nl())
                    changed |= eblCondition.Inspect();

                changed |= "Results".fold_enter_exit_List(results, ref inspectedResult, ref inspectedStuff, 2).nl();
            }

            if (changed)
                logicVersion = -1;

            return changed;
        }
#endif
        #endregion

        #region MGMT
        public virtual void MoveTo(Node node) {

            parentNode.Remove(this);
            node.Add(this);
            parentNode = node;

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

        public virtual void Delete() {
            var gn = this as GameNodeBase;
            if (gn != null) 
                parentNode.gameNodes.Remove(gn);
            else
                parentNode.coreNodes.Remove(this);

            root.allBaseNodes[IndexForPEGI] = null;
        }

        #endregion

    }
}
