using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace NodeNotes {

    public class BookLinkComponent : Base_Node, IPEGI_ListInspect {

        public enum BookLinkType { BookLink, BookExit }

        BookLinkType type = BookLinkType.BookLink;

        string linkedBookName;
        string bookEntryPoint;

        NodeBook LinkedBook => Shortcuts.TryGetBook(linkedBookName);

        Node LinkedNode  {
            get
            {
                var book = LinkedBook;
                if (book != null)   {
                    var ep = book.entryPoints.GetByIGotName(bookEntryPoint);

                    if (ep != null)  
                          return book.allBaseNodes[ep.nodeIndex] as Node;
                }
                return null;
            }
        }
        
        public override bool Conditions_isVisibile() {
            if (type == BookLinkType.BookExit && Shortcuts.user.bookMarks.Count == 0)
                return false;

            if (type == BookLinkType.BookLink && linkToCurrent) return false;

            return base.Conditions_isVisibile();
        }
        
        bool TryExecuteTransition() {

            bool executed = false;

            switch (type)
            {
                case BookLinkType.BookLink:

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
                case BookLinkType.BookExit:

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

        bool linkToCurrent
        {
            get
            {
                var ln = LinkedNode;
                if (ln != null && ln == CurrentNode)
                    return true;
                return false;
            }
        }

        #region Inspector
#if PEGI

        public override icon InspectorIcon => icon.Book;

        public override string inspectHint => "Inspect Book Link";

        protected override string ResultsRole => "On Transition";

        bool list_PEGI()
        {
            var changed = pegi.editEnum(ref type);

            switch (type)
            {

                case BookLinkType.BookLink:

                    changed |= pegi.select_iGotName(ref linkedBookName, Shortcuts.books);

                    var book = LinkedBook;

                    if (book != null)   {

                        pegi.select_iGotName(ref bookEntryPoint, book.entryPoints);

                        var ep = book.GetEntryPoint(bookEntryPoint);

                        if (ep != null) {
                            if (!linkToCurrent && icon.Play.Click("Transition Condition: {0}".F(Conditions_isEnabled())))
                                TryExecuteTransition();
                        }
                    }
                    break;
                case BookLinkType.BookExit: "Will Exit to previous book".write(); break;
            }

            return changed;
        }

        public override bool PEGI_inList(IList list, int ind, ref int edited)
        {
            bool changed = list_PEGI();

            if (icon.Enter.Click())
                edited = ind;

            return changed;

        }

        public override bool Inspect() {

            bool changed = base.Inspect();

            changed |= list_PEGI().nl();
            
            return changed;
        }
#endif
        #endregion

        #region Encode_Decode

        public override StdEncoder Encode() {

            var cody = this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("t", (int)type);

            if (type == BookLinkType.BookLink) cody
            .Add_String("lnk", linkedBookName)
            .Add_String("ep", bookEntryPoint);

            return cody;
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "b": data.Decode_Base(base.Decode, this); break;
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
