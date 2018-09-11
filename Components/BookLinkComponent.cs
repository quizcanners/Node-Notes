using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;


namespace LinkedNotes
{
    public class BookLinkComponent : Base_Node
    {
        string linkedBookName;
        string bookEntryPoint;

#if PEGI
        public override bool PEGI() {
            bool changed = base.PEGI();

            changed |= "Link to ".select_iGotName(ref linkedBookName, Shortcuts.books).nl();

            var book = LinkedBook;

            if (book != null) 
                "Entry Point".select_iGotName(ref bookEntryPoint, book.entryPoints).nl();

            return changed;
        }
#endif

        NodeBook LinkedBook => Shortcuts.TryGetBook(linkedBookName); 

        public override void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled()) {

                var book = LinkedBook;
                if (book != null) {
                    
                    var ep = book.entryPoints.GetByIGotName(bookEntryPoint);

                    if (ep != null) {
                        var n = book.allBaseNodes[ep.nodeIndex];
                        if (n!= null)
                        {
                            var node = n as Node;

                            if (node != null)
                                Nodes_PEGI.CurrentNode = node;
                            else Debug.LogError(" Entry {0} was not referancing Node. {1} had type {2} ".F(ep.ToPEGIstring(), n.ToPEGIstring(),n.GetType()));
                        } 
                        else
                            Debug.LogError(" Entry {0} was referancing a non-existing node.");
                    }
                }

                results.Apply(Values.global);
            }
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add_String("lnk", linkedBookName)
            .Add_String("ep", bookEntryPoint);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                case "lnk": linkedBookName = data; break;
                case "ep": bookEntryPoint = data; break;
                default: return false;
            }
            return true;
        }


    }
}
