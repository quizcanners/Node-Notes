using PlayerAndEditorGUI;
using QuizCannersUtilities;
using STD_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeNotes {

    [DerivedList(typeof(Node), typeof(NodeLinkComponent), typeof(NodeButtonComponent), typeof(BookLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognizedCfg, INeedAttention, IGotName, IGotIndex, IPEGI, ICanChangeClass, IPEGI_Searchable, IPEGI_ListInspect
    {

        #region Values
        public Node parentNode;
        public NodeBook root;

        public void OnClassTypeChange(object previousInstance) {
            
            if (!(previousInstance is Base_Node other)) return;
            
            parentNode = other.parentNode;
            root = other.root;
            visualRepresentation = other.visualRepresentation;
            previousVisualRepresentation = other.previousVisualRepresentation;
        }

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name = "New Node";
        public string NameForPEGI { get { return name; } set { name = value; } }

        protected static Node CurrentNode { get => Shortcuts.CurrentNode; set => Shortcuts.CurrentNode = value; }

        protected static NodesVisualLayerAbstract VisualLayer => Shortcuts.visualLayer;

        public ICfg visualRepresentation;
        public ICfg previousVisualRepresentation;
        public string configForVisualRepresentation;

        public virtual GameNodeBase AsGameNode => null;
        public virtual Node AsNode => null;
        #endregion

        public virtual void OnMouseOver() {
            if (Input.GetMouseButtonDown(0))
               parentNode?.SetInspectedUpTheHierarchy(this);
        }

        #region Logic

        private int _logicVersion = -1;
        private bool _visConditionsResult = true;
        private bool _enabledConditionResult = true;

        private readonly ConditionBranch _visCondition = new ConditionBranch("Visibility Conditions");
        private readonly ConditionBranch _eblCondition = new ConditionBranch("Activation Conditions");

        public bool TryForceEnabledConditions(Values values, bool to) {
            var done = _eblCondition.TryForceTo(values, to);

            if (to || !done)
                done |= _visCondition.TryForceTo(values, to);

            UpdateLogic();

            return done;
        }
        protected virtual string ResultsRole => "Role Unknown";

        protected List<Result> results = new List<Result>();

        private void UpdateLogic() {
            if (_logicVersion == LogicMGMT.currentLogicVersion) return;

            _logicVersion = LogicMGMT.currentLogicVersion;

            _visConditionsResult = _visCondition.IsTrue;

            _enabledConditionResult = _eblCondition.IsTrue;
        }

        protected bool IsOneOfChildrenOf(Node other) {

            if (other == null)
                return false;

            if (parentNode == null || parentNode == this) return false;

            return parentNode == other || parentNode.IsOneOfChildrenOf(other);

        }

        public virtual bool Conditions_isVisible() {
            UpdateLogic();
            return _visConditionsResult;
        }

        public virtual bool Conditions_isEnabled() {
            UpdateLogic();
            return _visConditionsResult && _enabledConditionResult;
        }

        #endregion
        
        #region Encode_Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
        .Add_String(        "n", name)
        .Add(               "i", index)
        .Add_IfTrue("visL", showLogic)
        .Add_IfNotNegative( "is", inspectedItems)
        .Add_IfNotNegative( "icr", _inspectedResult)
        .Add_IfNotDefault(  "cnds", _eblCondition)
        .Add_IfNotDefault(  "vcnds", _visCondition)
        .Add_IfNotEmpty(    "res",  results)
        .Add_IfNotEmpty(    "vis",  visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "n":       name = data; break;
                case "i":       index = data.ToInt(); break;
                case "visL":    showLogic = data.ToBool(); break;
                case "is":      inspectedItems = data.ToInt(); break;
                case "icr":     _inspectedResult = data.ToInt(); break;
                case "cnds":    _eblCondition.Decode(data); break;
                case "vcnds":   _visCondition.Decode(data); break;
                case "res":     data.Decode_List(out results); break;
                case "vis":     configForVisualRepresentation = data; break;
            }
            return true;
        }

        #endregion

        #region Inspector

        protected virtual icon InspectorIcon => icon.State;

        protected virtual string InspectionHint => "Inspect Node";
        
        public static bool editingNodes = false;

        private int _inspectedResult = -1;
        public bool InspectingTriggerItems => _inspectedResult != -1;

        protected bool showLogic;


#if !NO_PEGI
        
        public virtual bool InspectInList(IList list, int ind, ref int edited)
        {

            index.ToString().write(20);

            var changed = this.inspect_Name();

            if (this.Click_Enter_Attention(InspectorIcon, InspectionHint, false))
                edited = ind;

            return changed;
        }

        public virtual bool String_SearchMatch(string searchString)
        {
            if (_visCondition.SearchMatch_Obj(searchString))
                return true;

            if (_eblCondition.SearchMatch_Obj(searchString))
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

        public readonly LoopLock inspectionLock = new LoopLock();

        protected virtual bool Inspect_AfterNamePart() => false;

        public override bool Inspect() {
      
            if (!inspectionLock.Unlocked) return false;

            var changed = false;
            
            using (inspectionLock.Lock()) {

                var onPlayScreen = pegi.paintingPlayAreaGui;

                if (inspectedItems == -1)
                {
                    if (GetType() == typeof(Node) || onPlayScreen)
                        changed |= this.inspect_Name();
                    if ((this != Shortcuts.Cut_Paste) && icon.Cut.Click("Cut/Paste"))
                        Shortcuts.Cut_Paste = this;
                    if (visualRepresentation != null && icon.Show.Click("Visible. Click To Hide Visual Representation."))
                        Shortcuts.visualLayer.Hide(this);
                    if (visualRepresentation == null && icon.Hide.Click("Hidden. Click to show visual representation."))
                        Shortcuts.visualLayer.Show(this);

                }

                pegi.nl();

                Inspect_AfterNamePart().nl(ref changed);

                "Visual".TryEnter_Inspect(visualRepresentation, ref inspectedItems, 21).nl_ifFolded(ref changed);
                
                if ( inspectedItems != -1 || "Conditions & Results".foldout(ref showLogic).nl()) {
                    
                    _visCondition.enter_Inspect(ref inspectedItems, 1).nl_ifFolded(ref changed);

                    _eblCondition.enter_Inspect(ref inspectedItems, 2).nl_ifFolded(ref changed);

                    ResultsRole.enter_List(ref results, ref _inspectedResult, ref inspectedItems, 3, ref changed)
                        .SetLastUsedTrigger();
                }

                pegi.nl_ifFolded();

                if (changed)
                    _logicVersion = -1;
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

        public virtual void Init(NodeBook nRoot, Node parent)
        {
            root = nRoot;
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
