using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace NodeNotes
{

    public class BookLinkComponent : Base_Node {

        public enum BookLinkType { EntryPoint, Exit }

        BookLinkType type = BookLinkType.EntryPoint;

        string linkedBookName;
        string bookEntryPoint;

        NodeBook LinkedBook => Shortcuts.TryGetBook(linkedBookName);

        public override bool Conditions_isVisibile() {
            if (type == BookLinkType.Exit && Shortcuts.user.bookMarks.Count == 0)
                return false;

            return base.Conditions_isVisibile();
        }
        
        bool TryExecuteTransition() {

            bool executed = false;

            switch (type)
            {
                case BookLinkType.EntryPoint:

                    var book = LinkedBook;
                    if (book != null)
                    {

                        var ep = book.entryPoints.GetByIGotName(bookEntryPoint);

                        if (ep != null)
                        {
                            var n = book.allBaseNodes[ep.nodeIndex];
                            if (n != null)
                            {
                                var node = n as Node;

                                if (node != null)
                                {
                                    Shortcuts.CurrentNode = node;
                                    executed = true;
                                }
                                else Debug.LogError(" Entry {0} was not referancing Node. {1} had type {2} ".F(ep.ToPEGIstring(), n.ToPEGIstring(), n.GetType()));
                            }
                            else
                                Debug.LogError(" Entry {0} was referancing a non-existing node.");
                        }
                    }

                    if (executed)
                        results.Apply(Values.global);

                    return executed;
                case BookLinkType.Exit:

                    if (Shortcuts.user.bookMarks.Count == 0)
                        return false;

                        Shortcuts.user.ExitCurrentBook();

                    break;

                    
            }

            return false;
        }

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                TryExecuteTransition();
        }

        #region Inspector
#if PEGI
        public override bool PEGI() {

            bool changed = base.PEGI();

            changed |= "Type ".editEnum(40, ref type).nl();

            switch (type) {

                case BookLinkType.EntryPoint:

                    changed |= "Link to ".select_iGotName(ref linkedBookName, Shortcuts.books).nl();

                    var book = LinkedBook;

                    if (book != null) {

                        "Entry Point".select_iGotName(ref bookEntryPoint, book.entryPoints).nl();

                        var ep = book.GetEntryPoint(bookEntryPoint);

                        if (ep != null) {

                            "Transition Condition: {0}".F(Conditions_isEnabled()).write();

                            if (icon.Play.Click("Execute Book Transition Test").nl())
                                TryExecuteTransition();

                        }
                    }
                    break;
                case BookLinkType.Exit: "Will Exit to previous book".writeHint(); break;
            }


            return changed;
        }
        #endif
        #endregion
        
        #region Encode_Decode

        public override StdEncoder Encode() {

            var cody = this.EncodeUnrecognized()
            .Add("b", base.Encode())
            .Add("t", (int)type);

            if (type == BookLinkType.EntryPoint) cody
            .Add_String("lnk", linkedBookName)
            .Add_String("ep", bookEntryPoint);

            return cody;
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.DecodeInto(base.Decode); break;
                case "t": type = (BookLinkType)data.ToInt(); break;
                case "lnk": linkedBookName = data; break;
                case "ep": bookEntryPoint = data; break;
                default: return false;
            }
            return true;
        }
        #endregion

    }
}
