using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;
using STD_Logic;

namespace NodeNotes {

    public class Node : Base_Node,  INeedAttention, IPEGI {

        #region SubNodes
        List_Data coreNodesMeta = new List_Data("Sub Nodes", keepTypeData: true, enterIcon: icon.StateMachine);

        public List<Base_Node> coreNodes = new List<Base_Node>();

        List_Data gamesNodesMeta = new List_Data("Game Nodes", keepTypeData: true, enterIcon: icon.Discord);

        public List<GameNodeBase> gameNodes = new List<GameNodeBase>();  // Can be entered, but can't have subnodes, can be stored with unrecognized

        public IEnumerator<Base_Node> GetEnumerator()
        {
            foreach (var s in coreNodes)
                yield return s;

            foreach (var g in gameNodes)
                yield return g;
        }
        
        public bool Contains(Base_Node node) {
            if (node != null)
            {
                var gn = node.AsGameNode;
                if (gn != null)
                    return gameNodes.Contains(gn);

                return coreNodes.Contains(node);
            }
            else return false;
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

        public override void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled()) {
                if (this != Shortcuts.CurrentNode)
                    Shortcuts.CurrentNode = this;
                else if (parentNode != null)
                    Shortcuts.CurrentNode = parentNode;

                results.Apply(Values.global);
            }

            if (Input.GetMouseButtonDown(1))
                SetInspectedUpTheHierarchy(null);
        }

        LoopLock loopLock = new LoopLock();
        
        public override void Init (NodeBook nroot, Node parent){
            base.Init(nroot, parent);
          
            foreach (Base_Node sn in this)
                if (sn!= null)
                    sn.Init(nroot, this);

        }

        #region Inspector
        public override void ResetInspector() {
            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    InspectingSubnode = false;

                    foreach (var n in coreNodes)
                        n.ResetInspector();
                    foreach (var g in gameNodes)
                        g.ResetInspector();

                    base.ResetInspector();
                }
        }

        public bool InspectingSubnode { get { return coreNodesMeta.Inspecting; } set { if (value == false) coreNodesMeta.Inspecting  = false; } }
        
        protected override string ResultsRole => "On Enter Results";

        public void SetInspectedUpTheHierarchy(Base_Node node)
        {

            var gn = node.AsGameNode;

            if (gn != null) {
                if (gameNodes.Contains(gn))
                    gamesNodesMeta.inspected = gameNodes.IndexOf(gn);
            }
            else
            {
                if (node != null && coreNodes.Contains(node))
                    coreNodesMeta.inspected = coreNodes.IndexOf(node);
            }

            if (parentNode != null)
                parentNode.SetInspectedUpTheHierarchy(this);
        }

        #if PEGI
        public override string NeedAttention()
        {
            if (loopLock.Unlocked)
            {
                using (loopLock.Lock()) {
                    if (coreNodes.NeedsAttention("Sub Nodes") || gameNodes.NeedsAttention("Game Nodes"))
                        return pegi.LastNeedAttentionMessage;
                }

                if (root == null)
                    return "No root detected";

                return null;
            }
            else return "Infinite Loop Detected";

        }
        
        public override bool Inspect_AfterNamePart() {
            var changed = false;

            if (Application.isPlaying)
                return false;

            if (inspectedStuff == -1 && !InspectingTriggerStuff) {

                if (InspectingSubnode){
                    var n = coreNodes.TryGet(coreNodesMeta);
                    if (n == null)
                        InspectingSubnode = false;
                    else
                    {
                        if (icon.Exit.Click("Exit {0}".F(name)))
                            InspectingSubnode = false;

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

                    if (InspectingSubnode)
                        n.Try_Nested_Inspect();
                }

                if (!InspectingSubnode)  {
                    pegi.nl();
                    var newNode = coreNodesMeta.edit_List(ref coreNodes, ref changed);

                    if (newNode != null)
                        newNode.CreatedFor(this);
                }


            }

            if (!InspectingSubnode) {

                gamesNodesMeta.Inspecting = false;

                var ngn = gamesNodesMeta.enter_List(ref gameNodes, ref inspectedStuff, 7, GameNodeBase.all, ref changed);
                
                pegi.nl_ifFoldedOut();

                if (ngn != null)
                    ngn.CreatedFor(this);
            }

            return changed;

        }

        public override bool Inspect() {

            bool changed = false;
            
            if (InspectingSubnode)
                inspectedStuff = -1;
            
            if (!InspectingSubnode && inspectedStuff ==-1)  {
                if (this != CurrentNode) {
                    if (icon.Play.Click("Enter Node"))
                        CurrentNode = this;
                }
                else if (parentNode != null && icon.Close.Click())
                    CurrentNode = parentNode;
            }

            if (!InspectingSubnode)  {

                var cp = Shortcuts.Cut_Paste;

                if (cp != null) {

                    var gn = cp.AsGameNode;

                    bool canPaste = (cp.root == root) && cp != this && !coreNodes.Contains(cp) && (gn == null || !gameNodes.Contains(gn)) && (!IsOneOfChildrenOf(cp.AsNode));

                    if (icon.Delete.Click("Remove Cut / Paste object"))
                        Shortcuts.Cut_Paste = null;
                    else
                    {
                        (cp.ToPEGIstring() + (canPaste ? "" : " can't paste parent to child")).write();
                        if (canPaste && icon.Paste.Click(ref changed))
                        {
                            cp.MoveTo(this);
                            Shortcuts.Cut_Paste = null;
                            Shortcuts.visualLayer.UpdateVisibility();
                        }
                    }
                    pegi.nl();
                }
                changed |= base.Inspect();
            }
            else
                Inspect_AfterNamePart();
            
            return changed;
        }
        #endif
        #endregion

        #region Encode_Decode

        public override StdEncoder Encode()  {

            if (loopLock.Unlocked)  {
                using (loopLock.Lock()){

                    var cody = this.EncodeUnrecognized()
                        .Add("sub", coreNodes, coreNodesMeta)
                        .Add("b", base.Encode());

                    if (gameNodes.Count > 0)
                        cody.Add("gn", gameNodes, gamesNodesMeta, GameNodeBase.all);
                   
                    return cody;
                }
            }
            else
                Debug.LogError("Infinite loop detected at {0}. Node is probably became a child of itself. ".F(NameForPEGI));

            return new StdEncoder();
        }

        public override bool Decode(string tag, string data) {

            switch (tag)  {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "sub": data.Decode_List(out coreNodes, ref coreNodesMeta); break;
                case "gn":  data.Decode_List(out gameNodes, ref gamesNodesMeta, GameNodeBase.all); break;
                default:  return false;
            }
            return true;
        }

        #endregion

    }
}