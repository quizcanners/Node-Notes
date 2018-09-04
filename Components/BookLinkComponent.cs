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
        int bookEntryPoint;
#if PEGI
        public override bool PEGI() {
            bool changed = base.PEGI();

            changed |= "Link to ".select_iGotName(ref linkedBookName, root.allBaseNodes.GetAllObjsNoOrder()).nl();

            var book = LinkedBook;

            if (book != null) 
                "Entry Point".select(ref bookEntryPoint, book.entryPoints).nl();

            return changed;
        }
#endif

        NodeBook LinkedBook => Shortcuts.books.GetByIGotName<NodeBook_Base, NodeBook>(linkedBookName);

        public override void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled()) {

                var book = LinkedBook;
                if (book != null)
                {
                    //TODO: Convert book to Book Mark
                    var ep = book.entryPoints.TryGet(bookEntryPoint);

                    if (ep != null) {
                        var node = book.allBaseNodes[ep.nodeIndex] as Node;
                        if (node != null)
                            Nodes_PEGI.CurrentNode = node;
                    }
                }

                results.Apply(Values.global);
            }
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add_String("lnk", linkedBookName);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                case "lnk": linkedBookName = data; break;
                default: return false;
            }
            return true;
        }


    }
}
