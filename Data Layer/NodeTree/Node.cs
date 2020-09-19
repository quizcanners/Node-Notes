using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {

#pragma warning disable IDE0018 // Inline variable declaration

    public class Node : Base_Node {

        public string visualStyleTag = "";

        #region SubNodes

        private ListMetaData _coreNodesMeta = new ListMetaData("Sub Nodes", keepTypeData: true);

        public List<Base_Node> coreNodes = new List<Base_Node>();

        private ListMetaData _gamesNodesMeta = new ListMetaData("Game Nodes", keepTypeData: true);

        public List<GameNodeBase> gameNodes = new List<GameNodeBase>();  // Can be entered, but can't have subnodes, can be stored with unrecognized

        public IEnumerator<Base_Node> GetEnumerator()
        {
            foreach (var s in coreNodes)
                yield return s;

            foreach (var g in gameNodes)
                yield return g;
        }

        public override void SetInspected()
        {
            _coreNodesMeta.inspected = -1;
            _gamesNodesMeta.inspected = -1;
            parentNode?.SetInspectedUpTheHierarchy(this);
        }

        public bool Contains(Base_Node node) {

            if (node == null)
                return false;
            
            var gn = node.AsGameNode;
                
            return gn != null ? gameNodes.Contains(gn) : coreNodes.Contains(node);
        }

        public void Add(Base_Node node)
        {
            var gn = node.AsGameNode;

            if (gn!= null)
                gameNodes.Add(gn);
            else
                coreNodes.Add(node);

        }

        public void Remove(Base_Node node) {

            var gn = node.AsGameNode;

            if (gn != null)
                gameNodes.Remove(gn);
            else
                coreNodes.Remove(node);

        }

        public T Add<T>() where T : Base_Node, new()
        {
            var newNode = new T();
            newNode.CreatedFor(this);
            coreNodes.Add(newNode);
            return newNode;
        }
        #endregion

        public override Node AsNode => this;

        public override bool ExecuteInteraction() {
            Shortcuts.CurrentNode = this;
            base.ExecuteInteraction();

            if (parentNode != null)
                parentNode.InspectedCoreNode = this;
            return true;
        }

        public override bool OnMouseOver(bool click) {

            if (click && Conditions_isEnabled()) {
                if (this != Shortcuts.CurrentNode)
                    ExecuteInteraction();
                else if (parentNode != null)
                    Shortcuts.CurrentNode = parentNode;

                return true;
            }

            if (Input.GetMouseButtonDown(1))
                SetInspectedUpTheHierarchy(null);

            return false;
        }

        private readonly LoopLock _loopLock = new LoopLock();
        
        public override void Init (NodeBook nRoot, Node parent){
            base.Init(nRoot, parent);
          
            foreach (var sn in this)
                sn?.Init(nRoot, this);
            
        }

        #region Inspector
        public override void ResetInspector()
        {
            if (!_loopLock.Unlocked) return;

            using (_loopLock.Lock()) {

                InspectingSubNode = false;

                foreach (var n in coreNodes)
                    n.ResetInspector();
                foreach (var g in gameNodes)
                    g.ResetInspector();

                base.ResetInspector();
            }
        }

        public bool InspectingSubNode
        {
            get { return _coreNodesMeta.Inspecting; }
            set { if (value == false)
                    _coreNodesMeta.Inspecting  = false;
            }
        }
        
        protected override string ResultsRole => "On Enter Results";

        private Base_Node InspectedCoreNode {
            set
            {
                if (value == null)
                    _coreNodesMeta.inspected = -1;
                else if (coreNodes.Contains(value))
                    _coreNodesMeta.inspected = coreNodes.IndexOf(value);
            }
            get { return coreNodes.TryGet(_coreNodesMeta.inspected); }
        } 

        public void SetInspectedUpTheHierarchy(Base_Node node)
        {

            if (node == null)
                return;

            var gn = node.AsGameNode;

            if (gn != null) {
                if (gameNodes.Contains(gn))
                    _gamesNodesMeta.inspected = gameNodes.IndexOf(gn);
            }
            else
            {
                if (coreNodes.Contains(node))
                    _coreNodesMeta.inspected = coreNodes.IndexOf(node);
            }

            parentNode?.SetInspectedUpTheHierarchy(this);
        }
        
        protected override icon ExecuteIcon => icon.Next;
        protected override string ExecuteHint => "Enter Node";

        public override string NeedAttention()
        {
            if (!_loopLock.Unlocked) return "Infinite Loop Detected";

            using (_loopLock.Lock())
            {
                string msg;
                if (pegi.NeedsAttention(coreNodes, out msg, "Sub Nodes") || pegi.NeedsAttention(gameNodes, out msg, "Game Nodes"))
                    return msg;
            }

            return parentBook == null ? "No root detected" : null;

        }

        protected override bool Inspect_AfterNamePart() {
    
            var changed = false;
            
            if (_inspectedItems == -1 && InspectingSubNode) {

                    var node = coreNodes.TryGet(_coreNodesMeta);

                    if (node == null)
                        InspectingSubNode = false;
                    else if (!Application.isPlaying || (Shortcuts.CurrentNode == this || IsChildOrSubChildOf(Shortcuts.CurrentNode))) {

                        if (icon.Exit.Click("Exit {0}".F(name)))
                            InspectingSubNode = false;

                        if (this == Shortcuts.CurrentNode)
                        {
                            name.write(PEGI_Styles.EnterLabel);
                            if (parentNode != null && icon.Active.Click("Is Active. Click to exit."))
                                Shortcuts.CurrentNode = parentNode;
                          
                        }
                        else
                            name.write();
                        
                        pegi.nl();
                    }

                    if (InspectingSubNode)
                        pegi.Try_Nested_Inspect(node).changes(ref changed);
            }

            if (!InspectingSubNode) {
                
                if (_inspectedItems == -1)
                {
                    pegi.nl();
                    var newNode = _coreNodesMeta.edit_List(ref coreNodes, ref changed);

                    newNode?.CreatedFor(this);
                }

                _gamesNodesMeta.Inspecting = false;

                var ngn = _gamesNodesMeta.enter_List(ref gameNodes, ref _inspectedItems, 7, GameNodeBase.all, ref changed);
                
                pegi.nl_ifFoldedOut();

                ngn?.CreatedFor(this);

                pegi.nl();

            }

            return changed;

        }

        public override bool Inspect() {

            var changed = false;
            
            if (InspectingSubNode)
                _inspectedItems = -1;
            
            if (!InspectingSubNode && _inspectedItems ==-1)  {
                if (this != CurrentNode) {
                    if (parentBook.EditedByCurrentUser()) {
                        if (parentNode != null && icon.Play.Click("Enter Node"))
                            CurrentNode = this;
                    } else if (CurrentNode != null && CurrentNode == parentNode) {
                        if (Conditions_isEnabled()) {
                            if (icon.Play.Click("Enter node"))
                                ExecuteInteraction();
                        } else (Conditions_isVisible() ? icon.Share : icon.Hide).write(Conditions_isVisible() ? "Visible" : "Hidden Node");
                    }

                }
                else if (parentNode != null && icon.Back.Click("Exit Node"))
                {
                    CurrentNode = parentNode;
                    parentNode.InspectedCoreNode = null;
                }
            }

            if (!InspectingSubNode)  {

                var cp = Shortcuts.Cut_Paste;

                if (cp != null) {

                    var gn = cp.AsGameNode;

                    var canPaste = (cp.parentBook == parentBook) && cp != this && !coreNodes.Contains(cp) && (gn == null || !gameNodes.Contains(gn)) && (!IsOneOfChildrenOf(cp.AsNode));

                    if (icon.Delete.Click("Remove Cut / Paste object"))
                        Shortcuts.Cut_Paste = null;
                    else
                    {
                        (cp.GetNameForInspector() + (canPaste ? "" : " can't paste parent to child")).write();
                        if (canPaste && icon.Paste.Click(ref changed))
                        {
                            cp.MoveTo(this);
                            Shortcuts.Cut_Paste = null;
                            Shortcuts.visualLayer.OnLogicVersionChange();
                        }
                    }
                    pegi.nl();
                }

                base.Inspect().changes(ref changed);
            }
            else
                Inspect_AfterNamePart().changes(ref changed);
            


            return changed;
        }
      
        #endregion

        #region Encode_Decode

        public override CfgEncoder Encode()  {

            if (_loopLock.Unlocked)  {
                using (_loopLock.Lock()){

                    var cody = new CfgEncoder() //this.EncodeUnrecognized()
                        .Add_IfNotEmpty("sub", coreNodes)
                        .Add_IfNotEmpty("bg", visualStyleTag)
                        .Add("b", base.Encode);

                    if (gameNodes.Count > 0)
                        cody.Add_IfNotEmpty("gn", gameNodes, GameNodeBase.all);
                   
                    return cody;
                }
            }

            Debug.LogError("Infinite loop detected at {0}. Node is probably became a child of itself. ".F(NameForPEGI));

            return new CfgEncoder();
        }

        public override void Decode(string tg, CfgData data) {

            switch (tg)  {
                case "b": data.Decode(base.Decode); break;//data.Decode_Base(base.Decode, this); break;
                case "bg": visualStyleTag = data.ToString(); break;
                case "sub": data.ToList(out coreNodes); break;
                case "gn":  data.ToList(out gameNodes, GameNodeBase.all); break;
            }
        }

        #endregion

    }
}