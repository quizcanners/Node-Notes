using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QcTriggerLogic;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {

#pragma warning disable IDE0019 // Use pattern matching

    [DerivedList(typeof(Node), typeof(NodeLinkComponent), typeof(NodeButtonComponent), typeof(BookLinkComponent))]
    public class Base_Node : AbstractKeepUnrecognizedCfg, INeedAttention, IGotName, IGotIndex, IPEGI, ICanChangeClass, IPEGI_Searchable, IPEGI_ListInspect {

        #region Values
        public Node parentNode;
        public NodeBook parentBook;

        public bool IsDirectChildOf(Node node)
        {

            if (parentNode != null)
                return parentNode == node;

            return false;
        }

        public bool IsChildOrSubChildOf(Node node) {

            if (parentNode != null)
                return parentNode == node || parentNode.IsChildOrSubChildOf(node);

            return false;
        }

        public Dictionary<string, string> visualStyleConfigs = new Dictionary<string, string>();

        public void OnClassTypeChange(object previousInstance) {
            
            if (!(previousInstance is Base_Node other)) return;
            
            parentNode = other.parentNode;
            parentBook = other.parentBook;
            visualRepresentation = other.visualRepresentation;
            previousVisualRepresentation = other.previousVisualRepresentation;

            if (!QcUnity.IsNullOrDestroyed_Obj(visualRepresentation))
            {
                pegi.GameView.ShowNotification("Changing Node Type");
                visualRepresentation.OnSourceNodeChange(this);
            }
            else
            {
                pegi.GameView.ShowNotification("Source Node was empty, couldn't change");
            }
        }

        int index;
        public int IndexForPEGI { get { return index; } set { index = value; } }

        public string name = "New Node";
        public string NameForPEGI { get { return name; } set { name = value; } }

        protected static Node CurrentNode { get => Shortcuts.CurrentNode; set => Shortcuts.CurrentNode = value; }

        protected static NodesVisualLayerAbstract VisualLayer => Shortcuts.visualLayer;

        public INodeVisualPresentation visualRepresentation;
        public INodeVisualPresentation previousVisualRepresentation;
        public string configForVisualRepresentation;

        public virtual GameNodeBase AsGameNode => null;
        public virtual Node AsNode => null;
        #endregion

        public virtual bool ExecuteInteraction() {

            results.Apply(Values.global);

            return true;
        }

        public virtual void SetInspected() => parentNode?.SetInspectedUpTheHierarchy(this);

        public virtual bool OnMouseOver(bool click) => false;

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
            if (_logicVersion == LogicMGMT.CurrentLogicVersion) return;

            _logicVersion = LogicMGMT.CurrentLogicVersion;

            _visConditionsResult = _visCondition.IsTrue;

            _enabledConditionResult = _eblCondition.IsTrue && _visConditionsResult;
        }

        protected bool IsOneOfChildrenOf(Node other) {

            if (other == null)
                return false;

            if (parentNode == null || parentNode == this) return false;

            return parentNode == other || parentNode.IsOneOfChildrenOf(other);

        }

        protected bool IsOneOfChildrenOf(Base_Node other) => IsOneOfChildrenOf(other as Node);
        
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
        .Add_String(        "n",    name)
        .Add(               "i",    index)
        .Add_IfNotNegative( "is",   _inspectedItems)
        .Add_IfNotNegative( "icr",  _inspectedResult)
        .Add_IfNotDefault(  "cnds", _eblCondition)
        .Add_IfNotDefault(  "vcnds",_visCondition)
        .Add_IfNotEmpty(    "res",  results)
        .Add_IfNotEmpty(    "bg_cfgs", visualStyleConfigs)
        .Add_IfNotEmpty(    "vis",  visualRepresentation!= null ? visualRepresentation.Encode().ToString() : configForVisualRepresentation);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "n":       name = data; break;
                case "i":       index = data.ToInt(); break;
                case "is":      _inspectedItems = data.ToInt(); break;
                case "icr":     _inspectedResult = data.ToInt(); break;
                case "cnds":    _eblCondition.Decode(data); break;
                case "vcnds":   _visCondition.Decode(data); break;
                case "res":     data.Decode_List(out results); break;
                case "vis":     configForVisualRepresentation = data; break;
                case "bg_cfgs": data.Decode_Dictionary(out visualStyleConfigs); break;
            }
            return true;
        }

        #endregion

        #region Inspector

        protected virtual icon ExecuteIcon => icon.Play;
        protected virtual string ExecuteHint => "Execute Node";

        protected virtual string InspectionHint => "Inspect Node";
        
        private int _inspectedResult = -1;
        private int inspectedLogic = -1;
        public bool InspectingTriggerItems => _inspectedResult != -1;

        public virtual bool InspectInList(IList list, int ind, ref int edited)
        {

            index.ToString().write(20);

            var changed = this.inspect_Name();

            if (NodeBook.inspected != null && (NodeBook.inspected.EditedByCurrentUser() || (CurrentNode != null && (CurrentNode == this || CurrentNode.IsOneOfChildrenOf(this)))))
            {
                if (this.Click_Enter_Attention(icon.Enter, InspectionHint, false))
                    edited = ind;
            }
            else
            {
                var na = NeedAttention();
                if (!na.IsNullOrEmpty())
                    icon.Warning.write(na);
            }

            if (CurrentNode != null && CurrentNode == parentNode)
            {
                if (Conditions_isEnabled())
                {
                    if (ExecuteIcon.Click(ExecuteHint))
                        ExecuteInteraction();
                }
                else (Conditions_isVisible() ? icon.Share : icon.Hide).write(Conditions_isVisible() ? "Visible" : "Hidden Node");
            }


            return changed;
        }

        public virtual bool String_SearchMatch(string searchString)
        {
            if (pegi.Try_SearchMatch_Obj(_visCondition, searchString))
                return true;

            if (pegi.Try_SearchMatch_Obj(_eblCondition, searchString))
                return true;

            if (pegi.Try_SearchMatch_Obj(visualRepresentation, searchString))
                return true;

            if (results.SearchMatch_ObjectList(searchString))
                return true;

            return false;
        }

        public virtual string NeedAttention()
        {
            if (parentBook == null)
                return "{0} : {1} No root detected".F(IndexForPEGI, name);

            if (parentNode == null)
                return "{0} : {1} No Parent Node detected".F(IndexForPEGI, name);

            if (parentNode == this)
                return "Is it's own parent";

            return null;
        }

        public readonly LoopLock inspectionLock = new LoopLock();

        protected virtual bool Inspect_AfterNamePart() => false;

        public bool InspectingVisuals() => _inspectedItems == 21;

        protected enum InspectItems
        {
            Logic = 1,
            Node = 10,
        } 

        public override bool Inspect() {
      

            var changed = false;
            
            if (inspectionLock.Unlocked)
            using (inspectionLock.Lock()) {

                var onPlayScreen = pegi.PaintingGameViewUI;

                if (_inspectedItems == -1) {
                    this.inspect_Name().changes(ref changed);

                    if ((this != Shortcuts.Cut_Paste) && icon.Cut.Click("Cut/Paste"))
                        Shortcuts.Cut_Paste = this;
                }

                pegi.nl();
                    
                Inspect_AfterNamePart().nl(ref changed);

                if (_inspectedItems == -1)
                {
                    if (visualRepresentation == null)
                    {
                        "Node is not currently visualized".write();

                        if ("Show".Click("Hidden. Click to show visual representation."))
                        {
                            Shortcuts.visualLayer.Show(this);
                        }
                    } else if ("Hide".Click("Visible. Click To Hide Visual Representation."))
                        Shortcuts.visualLayer.Hide(this);
                }
            

                "Visual".TryEnter_Inspect(visualRepresentation, ref _inspectedItems, 21).nl_ifFolded(ref changed);
                    
                if ("Conditions & Results".enter(ref _inspectedItems, (int)InspectItems.Logic).nl())
                {

                    _visCondition.enter_Inspect(ref inspectedLogic, 0).nl_ifFolded(ref changed);

                    _eblCondition.enter_Inspect(ref inspectedLogic, 1).nl_ifFolded(ref changed);

                    ResultsRole.enter_List(ref results, ref _inspectedResult, ref inspectedLogic, 2, ref changed).SetLastUsedTrigger();
                }

                pegi.nl_ifFolded();
                    
                if (changed)
                    _logicVersion = -1;
            }
            
            return changed;
        }
        
        #endregion

        #region MGMT

        public virtual string LinkTo(INodeVisualPresentation visualLayer)
        {
            if (visualRepresentation != null)
                Debug.LogError("Visual representation is not null", visualLayer as Object);

            visualRepresentation = visualLayer;
            previousVisualRepresentation = visualLayer;
            return configForVisualRepresentation;

        }

        public virtual void Unlink(CfgEncoder data) {
                configForVisualRepresentation = data.ToString();
                visualRepresentation = null;
        }

        public virtual void MoveTo(Node node) {

            parentNode.Remove(this);
            node.Add(this);
            parentNode = node;

        }

        public virtual Base_Node CreatedFor(Node target) {
            parentNode = target;
            return CreatedFor(target.parentBook);
        }

        public virtual Base_Node CreatedFor(NodeBook r) {
            parentBook = r;

            IndexForPEGI = parentBook.firstFree;
          
            parentBook.firstFree += 1;

            Init(r, null);

            return this;
        }

        public virtual void Init(NodeBook nRoot, Node parent)
        {
            parentBook = nRoot;
            if (parent != null)
                parentNode = parent;
            parentBook.Register(this); //allBaseNodes[index] = this;
        }

        public virtual void Delete() {
            var gn = this as GameNodeBase;
            if (gn != null) 
                parentNode.gameNodes.Remove(gn);
            else
                parentNode.coreNodes.Remove(this);

            parentBook.allBaseNodes[IndexForPEGI] = null;
        }
        
        #endregion

    }
}
