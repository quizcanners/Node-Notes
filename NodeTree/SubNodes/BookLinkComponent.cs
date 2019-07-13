using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using System;
using QcTriggerLogic;
using PlayerAndEditorGUI;

namespace NodeNotes {


    #pragma warning disable IDE0019 // Simplify 'default' expression

    public class BookLinkComponent : Base_Node, IPEGI_ListInspect {

        public enum BookLinkType { BookLink, BookExit }

        BookLinkType type = BookLinkType.BookLink;

        private string linkedBookAuthor;
        string linkedBookName;
        string bookEntryPoint;

        NodeBook LoadLinkedBook => Shortcuts.TryGetLoadedBook(linkedBookName, linkedBookAuthor);

        NodeBook_Base LinkedBook => Shortcuts.TryGetBook(linkedBookName, linkedBookAuthor);
        
        Node LinkedNode  {
            get
            {
                var book = LoadLinkedBook;
                if (book != null)   {
                    var ep = book.entryPoints.GetByIGotName(bookEntryPoint);

                    if (ep != null)  
                          return book.allBaseNodes[ep.nodeIndex] as Node;
                }
                return null;
            }
        }
        
        public override bool Conditions_isVisible() {
            if (type == BookLinkType.BookExit && Shortcuts.user.bookMarks.Count == 0)
                return false;

            if (type == BookLinkType.BookLink && LinkToCurrent) return false;

            return base.Conditions_isVisible();
        }

        public override bool ExecuteInteraction()
        {

            bool executed = false;

            switch (type)
            {
                case BookLinkType.BookLink:

                    var book = LoadLinkedBook;
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
                                else Debug.LogError(" Entry {0} was not referancing Node. {1} had type {2} ".F(ep.GetNameForInspector(), n.GetNameForInspector(), n.GetType()));
                            }
                            else
                                Debug.LogError(" Entry {0} was referancing a non-existing node.");
                        }
                    }

                 

                    return executed;
                case BookLinkType.BookExit:

                    if (Shortcuts.user.bookMarks.Count != 0)
                    {
                        Shortcuts.user.ExitCurrentBook();
                        executed = true;
                    }

                    break;

                    
            }


            if (executed)
                base.ExecuteInteraction();

            return executed;
        }

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && Conditions_isEnabled())
                ExecuteInteraction();
        }

        bool LinkToCurrent
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
 
        protected override string InspectionHint => "Inspect Book Link";

        protected override string ResultsRole => "On Transition";

        bool List_PEGI()
        {
            var changed = pegi.editEnum(ref type);

            switch (type)
            {

                case BookLinkType.BookLink:

                    var linkedBook = Shortcuts.TryGetBook(linkedBookName, linkedBookAuthor);

                    if (pegi.select(ref linkedBook, Shortcuts.books).changes(ref changed)) {
                        linkedBookName = linkedBook.NameForPEGI;
                        linkedBookAuthor = linkedBook.authorName;
                    }

                    var book = LoadLinkedBook;

                    if (book != null) {

                        pegi.select_iGotName(ref bookEntryPoint, book.entryPoints);

                        var ep = book.GetEntryPoint(bookEntryPoint);

                        if (ep != null) {
                            if (!LinkToCurrent && icon.Play.Click("Transition Condition: {0}".F(Conditions_isEnabled())))
                                ExecuteInteraction();
                        }
                    }
                    break;
                case BookLinkType.BookExit: "Will Exit to previous book".write(); break;
            }

            return changed;
        }

        public override bool InspectInList(IList list, int ind, ref int edited)
        {
            bool changed = List_PEGI();

            if (icon.Enter.Click())
                edited = ind;

            return changed;

        }

        public override bool Inspect() {

            bool changed = base.Inspect();

            changed |= List_PEGI().nl();
            
            return changed;
        }

        #endregion

        #region Encode_Decode

        public override CfgEncoder Encode() {

            var cody = this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("t", (int)type);

            if (type == BookLinkType.BookLink)
                cody.Add_String("lnk", linkedBookName)
                    .Add_String("au", linkedBookAuthor)
                    .Add_String("ep", bookEntryPoint);

            return cody;
        }

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "t": type = (BookLinkType)data.ToInt(); break;
                case "lnk": linkedBookName = data; break;
                case "au": linkedBookAuthor = data; break;
                case "ep": bookEntryPoint = data; break;
                default: return false;
            }
            return true;
        }

        #endregion

    }
}
