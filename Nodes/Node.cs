using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;
using STD_Logic;

namespace NodeNotes {

    public class Node : Base_Node,  INeedAttention, IPEGI { 

        public List<Base_Node> subNotes = new List<Base_Node>();

        public List<GameNodeBase> gameNodes = new List<GameNodeBase>();

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

        public T Add<T>() where T: Base_Node, new() {
             var newNode = new T();
             newNode.CreatedFor(this);
             subNotes.Add(newNode);
             return newNode;
        }

        LoopLock loopLock = new LoopLock();
        
        public override void Init (NodeBook nroot, Node parent){
            base.Init(nroot, parent);
          
            foreach (var sn in subNotes)
                sn.Init(nroot, this);
        }
        
        #region Inspector
        public int inspectedSubnode = -1;
        public int inspectedGameNode = -1;

        public override string NeedAttention()
        {
            if (loopLock.Unlocked)
            {
                using (loopLock.Lock())
                {
                    foreach (var s in subNotes)
                        if (s == null)
                            return "{0} : {1} Got null sub node".F(IndexForPEGI, name);
                        else
                        {
                            var na = s.NeedAttention();
                            if (na != null)
                                return na;
                        }
                }

                if (root == null)
                    return "No root detected";

                return null;
            }
            else return "Infinite Loop Detected";

        }

        public void SetInspectedUpTheHierarchy(Base_Node node)
        {

            int ind = -1;
            if (node != null && subNotes.Contains(node))
                ind = subNotes.IndexOf(node);

            inspectedSubnode = ind;

            if (parentNode != null)
                parentNode.SetInspectedUpTheHierarchy(this);
        }
#if PEGI

        public override bool PEGI() {

            bool changed = false;

            bool onPlayScreen = pegi.paintingPlayAreaGUI;
            
            if ((!showDebug && inspectedSubnode == -1) && (!onPlayScreen))  {
                pegi.nl();

                    if (this != CurrentNode)  {
                        if (icon.Play.Click())
                            CurrentNode = this;
                    }
                    else if (parentNode != null && icon.Exit.Click())
                        CurrentNode = parentNode;
            }

            if (onPlayScreen || inspectedSubnode == -1)
                changed |= base.PEGI();
            else
                showDebug = false;

            if ((!showDebug && inspectedSubnode == -1) || onPlayScreen) {
                
                var cp = Shortcuts.Cut_Paste;

                if (cp != null)  {

                    bool canPaste = (cp.root == root) && cp != this && !subNotes.Contains(cp) && (!IsOneOfChildrenOf(cp as Node));

                    if (icon.Delete.Click("Remove Cut / Paste object"))
                        Shortcuts.Cut_Paste = null;
                    else {
                        (cp.ToPEGIstring() + (canPaste ? "" : " can't paste parent to child")).write();
                        if (canPaste && icon.Paste.Click())
                        {
                            cp.MoveTo(this);
                            Shortcuts.Cut_Paste = null;
                            changed = true;
                        }
                    }
                    pegi.nl();
                }
            }

            if (inspectedSubnode == -1 && "Game Nodes ".fold_enter_exit_List(gameNodes, ref inspectedGameNode, ref inspectedStuff, 7).nl())
                changed = true;

            if (!showDebug && !onPlayScreen && !InspectingTriggerStuff)  {

                if (inspectedSubnode != -1) {
                    var n = subNotes.TryGet(inspectedSubnode);
                    if (n == null || icon.Exit.Click())
                        inspectedSubnode = -1;
                    else
                        n.Try_Nested_Inspect();
                }

                if (inspectedSubnode == -1) {
                    pegi.nl();
                    var newNode = "Sub Nodes".edit_List(subNotes, ref inspectedSubnode, ref changed);

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
                     .Add_IfNotEmpty("sub", subNotes)
                     .Add("b", base.Encode())
                     .Add_IfNotNegative("isn", inspectedSubnode);
                  //   .Add_IfNotNegative("is", inspectedStuff);
                    
                    foreach (var gn in gameNodes)
                        cody.Add(gn.UniqueTag, gn);

                    return cody;
                }
            }
            else
                Debug.LogError("Infinite loop detected at {0}. Node is probably became a child of itself. ".F(NameForPEGI));

            return new StdEncoder();
        }

        public override bool Decode(string tag, string data) {

            switch (tag)  {

                case "sub": data.DecodeInto(out subNotes); break;
                case "b": data.DecodeInto(base.Decode); break;
                case "isn": inspectedSubnode = data.ToInt(); break;
               // case "is": inspectedStuff = data.ToInt(); break;

                default:
                    Type t;
                    if (GameNodeBase.allGameNodes.TryGetValue(tag, out t)) {
                        gameNodes.Add(data.DecodeInto_Type<GameNodeBase>(t));
                        break;
                    } else
                        return false;
            }
            return true;
        }

        #endregion

    }
}