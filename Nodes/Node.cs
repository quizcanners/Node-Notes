using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using PlayerAndEditorGUI;

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
            if (Input.GetMouseButtonDown(0))
            {
                if (this != Nodes_PEGI.CurrentNode)
                    Nodes_PEGI.CurrentNode = this;
                else if (parentNode != null)
                    Nodes_PEGI.CurrentNode = parentNode;
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

        public override bool PEGI() {

            bool changed = false;

            if (inspectedSubnode == -1)
                changed |= base.PEGI();
            else
                showDebug = false;

            if (!showDebug)// && !editConditions && !editResults)
            {

                if (inspectedSubnode == -1)
                {

                    if (this != Nodes_PEGI.CurrentNode && icon.Play.Click())
                        Nodes_PEGI.CurrentNode = this;

                    if (MGMT.Cut_Paste != null) {
                        if (icon.Delete.Click())
                            MGMT.Cut_Paste = null;
                        else
                        {
                            MGMT.Cut_Paste.ToPEGIstring().write();
                            if (icon.Paste.Click())
                            {
                                MGMT.Cut_Paste.MoveTo(this);
                                MGMT.Cut_Paste = null;
                                changed = true;
                            }
                        }

                        pegi.nl();
                    }
                }

                if (inspectedSubnode != -1)
                {
                  

                    var n = subNotes.TryGet(inspectedSubnode);
                    if (n == null || icon.Exit.Click())
                        inspectedSubnode = -1;
                    else
                        n.Try_Nested_Inspect();
                }

                if (inspectedSubnode == -1)
                {

                    var newNode = name.edit_List(subNotes, ref inspectedSubnode, true, ref changed);
                    
                    if (newNode != null)
                    {
                        Debug.Log("Adding new one");
                        newNode.CreatedFor(this);
                    }
                }

            }
            return changed;
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