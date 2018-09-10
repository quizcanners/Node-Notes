using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;
using STD_Logic;

namespace LinkedNotes
{
    public class Node : Base_Node,  INeedAttention,  IPEGI
    { 

        public List<Base_Node> subNotes = new List<Base_Node>();

        public int inspectedSubnode = -1;

        public override string NeedAttention()
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

            if (root == null)
                return "No root detected";

            return null;
        }

        public override void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled()) {
                if (this != Nodes_PEGI.CurrentNode)
                    Nodes_PEGI.CurrentNode = this;
                else if (parentNode != null)
                    Nodes_PEGI.CurrentNode = parentNode;

                results.Apply(Values.global);
            }

            if (Input.GetMouseButtonDown(1))
                SetInspectedUpTheHierarchy(null);
        }

        public void SetInspectedUpTheHierarchy(Base_Node node) {

            int ind = -1;
            if (node!= null && subNotes.Contains(node))
                ind = subNotes.IndexOf(node);

            inspectedSubnode = ind;

            if (parentNode != null)
                parentNode.SetInspectedUpTheHierarchy(this);
        }
        #if PEGI

        public override bool PEGI() {

            bool changed = false;

            bool onPlayScreen = pegi.paintingPlayAreaGUI;
            
            if (onPlayScreen || inspectedSubnode == -1)
                changed |= base.PEGI();
            else
                showDebug = false;
            
            if ((!showDebug && inspectedSubnode == -1) || onPlayScreen) {

                if (!onPlayScreen && this != Nodes_PEGI.CurrentNode && icon.Play.Click())
                    Nodes_PEGI.CurrentNode = this;
                
                if (Mgmt.Cut_Paste != null)  {

                    bool canPaste = (Mgmt.Cut_Paste.root == root) && (!isOneOfChildrenOf(Mgmt.Cut_Paste as Node)) ;
                 
                        if (icon.Delete.Click("Remove Cut / Paste object"))
                            Mgmt.Cut_Paste = null;
                        else
                        {
                            (Mgmt.Cut_Paste.ToPEGIstring() + (canPaste ? "": " can't paste parent to child")).write();
                            if (canPaste && icon.Paste.Click())
                            {
                                Mgmt.Cut_Paste.MoveTo(this);
                                Mgmt.Cut_Paste = null;
                                changed = true;
                            }
                        }
                    
                    pegi.nl();
                }
            }


            if (!showDebug && !onPlayScreen && !InspectingTriggerStuff) {

                    if (inspectedSubnode != -1)  {
                        var n = subNotes.TryGet(inspectedSubnode);
                        if (n == null || icon.Exit.Click())
                            inspectedSubnode = -1;
                        else
                            n.Try_Nested_Inspect();
                    }

                    if (inspectedSubnode == -1) {

                        pegi.nl();

                        var newNode = name.edit_List(subNotes, ref inspectedSubnode,  ref changed);

                        if (newNode != null)
                        {
                            Debug.Log("Adding new one");
                            newNode.CreatedFor(this);
                        }
                    }
                
            }
            return changed;
        }

#endif

        public T Add<T>() where T: Base_Node, new() {
             var newNode = new T();
             newNode.CreatedFor(this);
             subNotes.Add(newNode);
             return newNode;
        }
        
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("sub", subNotes)
            .Add("b", base.Encode())
            .Add("isn", inspectedSubnode);
    
        public override bool Decode(string tag, string data)
        {
            switch (tag)  {
                case "sub": data.DecodeInto(out subNotes); break;
                case "b": data.DecodeInto(base.Decode); break;
                case "isn": inspectedSubnode = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override void Init (NodeBook nroot, Node parent){
            base.Init(nroot, parent);
          
            foreach (var sn in subNotes)
                sn.Init(nroot, this);
        }
    }
}