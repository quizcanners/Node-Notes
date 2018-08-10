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

        int inspectedSubnode = -1;

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

        public override bool PEGI() {

            bool changed = false;

            if (inspectedSubnode == -1)
                changed |= base.PEGI();
            else
                showDebug = false;

            if (!showDebug)
            {

                if (inspectedSubnode == -1 && MGMT.Cut_Paste != null)
                {

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

                var newNode = name.edit_List(subNotes, ref inspectedSubnode, true, ref changed);
                if (newNode != null)
                    newNode.CreatedFor(this);

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