using PlayerAndEditorGUI;
using QuizCannersUtilities;
using STD_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeNotes {

    [DerrivedList(typeof(Node), typeof(NodeLinkComponent), typeof(NodeButtonComponent), typeof(BookLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognized_STD, INeedAttention, IGotName, IGotIndex, IPEGI, ICanChangeClass, IPEGI_Searchable, IPEGI_ListInspect
    {

        #region Values
        public Node parentNode;
        public NodeBook root;

        public void Copy_NonSTDdata_From_PreviousInstance(object previous) {
            var other = previous as Base_Node;
            if (other != null) {
                parentNode = other.parentNode;
                root = other.root;
                visualRepresentation = other.visualRepresentation;
                previousVisualRepresentation = other.previousVisualRepresentation;
            }
        }

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name = "New Node";
        public string NameForPEGI { get { return name; } set { name = value; } }

        public static Node CurrentNode { get => Shortcuts.CurrentNode; set => Shortcuts.CurrentNode = value; }
        
        public static NodesVisualLayerAbstract VisualLayer { get => Shortcuts.visualLayer;  }

        public ISTD visualRepresentation;
        public ISTD previousVisualRepresentation;
        public string configForVisualRepresentation;

        public virtual GameNodeBase AsGameNode => null;
        public virtual Node AsNode => null;
        #endregion

        public virtual void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && parentNode != null)
               parentNode.SetInspectedUpTheHierarchy(this);
        }

        #region Logic

        int logicVersion = -1;
        bool visConditionsResult = true;
        bool enabledConditionResult = true;
        
        ConditionBranch visCondition = new ConditionBranch("Visibe");
        ConditionBranch eblCondition = new ConditionBranch("Enabled");

        public bool TryForceEnabledConditions(bool to) {
            bool done = eblCondition.TryForceTo(to);

            if (to || (!to && !done))
                done |= visCondition.TryForceTo(to);

            UpdateLogic();

            return done;
        }
        protected virtual string ResultsRole => "Role Unknown";

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
        .Add_String(        "n", name)
        .Add(               "i", index)
        .Add_IfNotNegative( "is", inspectedStuff)
        .Add_IfNotNegative( "icr", inspectedResult)
        .Add_IfNotDefault(  "cnds", eblCondition)
        .Add_IfNotDefault(  "vcnds", visCondition)
        .Add_IfNotEmpty(    "res",  results)
        .Add_IfNotEmpty(    "vis",  visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n":       name = data; break;
                case "i":       index = data.ToInt(); break;
                case "is":      inspectedStuff = data.ToInt(); break;
                case "icr":     inspectedResult = data.ToInt(); break;
                case "cnds":    eblCondition.Decode(data); break;
                case "vcnds":   visCondition.Decode(data); break;
                case "res":     data.Decode_List(out results); break;
                case "vis":     configForVisualRepresentation = data; break;
            }
            return true;
        }

        #endregion

        #region Inspector

        public virtual icon InspectorIcon => icon.State;

        public virtual string inspectHint => "Inspect Node";

        #if PEGI

        public virtual bool PEGI_inList(IList list, int ind, ref int edited)
        {

            index.ToString().write(20);

            var changed = this.inspect_Name();

            if (this.Click_Enter_Attention(InspectorIcon, inspectHint, false))
                edited = ind;

            return changed;
        }

        public virtual bool String_SearchMatch(string searchString)
        {
            if (visCondition.SearchMatch_Obj(searchString))
                return true;

            if (eblCondition.SearchMatch_Obj(searchString))
                return true;

            if (visualRepresentation.SearchMatch_Obj(searchString))
                return true;

            if (results.SearchMatch(searchString))
                return true;
            
            return false;
        }
        
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

        #endif



        public static bool editingNodes = false;

        int inspectedResult = -1;
        public bool InspectingTriggerStuff => inspectedResult != -1;
#if PEGI
        LoopLock inspectLock = new LoopLock();

        public virtual bool Inspect_AfterNamePart() => false;
        
        public override bool Inspect() {
            var changed = false;

            if (inspectLock.Unlocked)
                using (inspectLock.Lock()) {

                    bool onPlayScreen = pegi.paintingPlayAreaGUI;

                    if (!onPlayScreen)
                        changed |= base.Inspect();

                    if (inspectedStuff == -1)
                    {
                        if (GetType() == typeof(Node) || onPlayScreen)
                            changed |= this.inspect_Name();
                        if ((this != Shortcuts.Cut_Paste) && icon.Copy.Click("Cut/Paste"))
                            Shortcuts.Cut_Paste = this;
                        if (visualRepresentation != null && icon.Show.Click("Visible. Click To Hide Visual Representation."))
                            Shortcuts.visualLayer.Hide(this);
                        if (visualRepresentation == null && icon.Hide.Click("Hidden. Click to show visual representation."))
                            Shortcuts.visualLayer.Show(this);

                    }

                    pegi.nl();

                    Inspect_AfterNamePart().changes(ref changed);

                    changed |= "Visual".TryEnter_Inspect(visualRepresentation, ref inspectedStuff, 4).nl_ifFolded();

                    changed |= visCondition.enter_Inspect(ref inspectedStuff, 1).nl_ifFolded();

                    changed |= eblCondition.enter_Inspect(ref inspectedStuff, 2).nl_ifFolded();

                    ResultsRole.enter_List(ref results, ref inspectedResult, ref inspectedStuff, 3, ref changed).SetLastUsedTrigger();

                    pegi.nl_ifFolded();

                    if (changed)
                        logicVersion = -1;
                }

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
            root.Register(this); //allBaseNodes[index] = this;
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
