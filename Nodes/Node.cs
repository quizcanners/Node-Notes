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

        public List<Base_Node> coreNodes = new List<Base_Node>();

        List_Data gamesNodesMeta = new List_Data("Game Nodes", true);

        public List<GameNodeBase> gameNodes = new List<GameNodeBase>();  // Can be entered, but can't have subnodes, can be stored with unrecognized

        public IEnumerator<Base_Node> GetEnumerator()
        {
            foreach (var s in coreNodes)
                yield return s;

            foreach (var g in gameNodes)
                yield return g;
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
        public int inspectedSubnode = -1;

        public override string NeedAttention()
        {
            if (loopLock.Unlocked)
            {
                using (loopLock.Lock()) {

                    var na = coreNodes.needsAttention(false, "Sub Nodes");

                    if (na != null)
                        return na;
                    
                    var gna = gameNodes.needsAttention(false, "Game Nodes");

                    if (gna != null)
                        return gna;
                }

                if (root == null)
                    return "No root detected";

                return null;
            }
            else return "Infinite Loop Detected";

        }

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
                    inspectedSubnode = coreNodes.IndexOf(node);
            }

            if (parentNode != null)
                parentNode.SetInspectedUpTheHierarchy(this);
        }

#if PEGI
        public override bool Inspect() {

            bool changed = false;

            if (inspectedSubnode != -1)
                inspectedStuff = -1;

            if (inspectedSubnode == -1 && inspectedStuff ==-1)  {
                if (this != CurrentNode) {
                    if (icon.Play.Click("Enter Node"))
                        CurrentNode = this;
                }
                else if (parentNode != null && icon.Close.Click())
                    CurrentNode = parentNode;
            }

            if (inspectedSubnode == -1) {

                var cp = Shortcuts.Cut_Paste;

                if (cp != null)
                {

                    var gn = cp.AsGameNode;

                    bool canPaste = (cp.root == root) && cp != this && !coreNodes.Contains(cp) && (gn == null || !gameNodes.Contains(gn)) && (!IsOneOfChildrenOf(cp.AsNode));

                    if (icon.Delete.Click("Remove Cut / Paste object"))
                        Shortcuts.Cut_Paste = null;
                    else {
                        (cp.ToPEGIstring() + (canPaste ? "" : " can't paste parent to child")).write();
                        if (canPaste && icon.Paste.Click()){
                            cp.MoveTo(this);
                            Shortcuts.Cut_Paste = null;
                            changed = true;
                            Shortcuts.visualLayer.UpdateVisibility();
                        }
                    }
                    pegi.nl();
                }
  
                changed |= base.Inspect();

                var ngn = gamesNodesMeta.enter_List(ref gameNodes, ref inspectedStuff, 7, GameNodeBase.all, ref changed);

                pegi.nl_ifFoldedOut();

                if (ngn != null)
                    ngn.CreatedFor(this);
            }
            
                if (inspectedStuff == -1 && !InspectingTriggerStuff) {

                    if (inspectedSubnode != -1) {
                        var n = coreNodes.TryGet(inspectedSubnode);
                        if (n == null)
                            inspectedSubnode = -1;
                        else {
                            if (icon.Exit.Click(name))
                                inspectedSubnode = -1;

                                if (this == Shortcuts.CurrentNode && parentNode != null && icon.Active.Click())
                                    Shortcuts.CurrentNode = parentNode;
                                
                            name.nl();
                        }

                        if (inspectedSubnode != -1)
                            n.Try_Nested_Inspect();
                    }

                    if (inspectedSubnode == -1)
                    {
                        pegi.nl();
                        var newNode = "Sub Nodes".edit_List(ref coreNodes, ref inspectedSubnode, ref changed);

                        if (newNode != null)
                            newNode.CreatedFor(this);
                    }

                }
            

            

            return changed;
        }

#endif
        #endregion
        
        #region Encode_Decode

        public override StdEncoder Encode()  {

            if (loopLock.Unlocked)  {
                using (loopLock.Lock()){

                    var cody = this.EncodeUnrecognized()
                        .Add_IfNotEmpty("sub", coreNodes)
                        .Add("b", base.Encode())
                        .Add_IfNotNegative("isn", inspectedSubnode);

                        if (gameNodes.Count>0)
                        cody.Add("gnMeta", gamesNodesMeta)
                            .Add_Abstract("gn", gameNodes, gamesNodesMeta);
                   
                    return cody;
                }
            }
            else
                Debug.LogError("Infinite loop detected at {0}. Node is probably became a child of itself. ".F(NameForPEGI));

            return new StdEncoder();
        }

        public override bool Decode(string tag, string data) {

            switch (tag)  {

                case "sub": data.DecodeInto_List(out coreNodes); break;
                case "b": data.DecodeInto(base.Decode); break;
                case "isn": inspectedSubnode = data.ToInt(); break;
                case "gnMeta": data.DecodeInto(out gamesNodesMeta); break;
                case "gn":  data.DecodeInto_List(out gameNodes, GameNodeBase.all, gamesNodesMeta); break;
                   
                default:  return false;
            }
            return true;
        }

        #endregion

    }
}