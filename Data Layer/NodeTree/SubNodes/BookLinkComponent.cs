using System.Collections;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {


    #pragma warning disable IDE0018 // Inline variable declaration
    #pragma warning disable IDE0019 // Simplify 'default' expression

    public class BookLinkComponent : Base_Node, IPEGI_ListInspect {

        public enum BookLinkType { BookLink, BookExit }

        BookLinkType type = BookLinkType.BookLink;

        private string linkedBookAuthor;
        string linkedBookName;
        string bookEntryPoint;

        bool LoadLinkedBook(out NodeBook book) => Shortcuts.books.TryGetLoadedBook(linkedBookName, linkedBookAuthor, out book);

        NodeBook_Base LinkedBook => Shortcuts.books.TryGetBook(linkedBookName, linkedBookAuthor);
        
        Node LinkedNode  {
            get
            {
                NodeBook book;
                if (LoadLinkedBook(out book))   {
                    var ep = book.entryPoints.GetByIGotName(bookEntryPoint);

                    if (ep != null)  
                          return book.allBaseNodes[ep.nodeIndex] as Node;
                }
                return null;
            }
        }
        
        public override bool Conditions_isVisible() {
            if (type == BookLinkType.BookExit && Shortcuts.users.current.bookMarks.Count == 0)
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

                    NodeBook book;
                    if (LoadLinkedBook(out book)) {

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

                    if (Shortcuts.users.current.bookMarks.Count != 0) {

                        Shortcuts.users.current.ExitCurrentBook();
                        executed = true;
                    }

                    break;

                    
            }


            if (executed)
                base.ExecuteInteraction();

            return executed;
        }

        public override bool OnMouseOver(bool click) {
            if (click && Conditions_isEnabled())
                return ExecuteInteraction(); 
            
            return false;
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

                    var linkedBook = Shortcuts.books.TryGetBook(linkedBookName, linkedBookAuthor);

                    if (pegi.select(ref linkedBook, Shortcuts.books.all).changes(ref changed)) {
                        linkedBookName = linkedBook.NameForPEGI;
                        linkedBookAuthor = linkedBook.authorName;
                    }

                    if (linkedBook!= null && linkedBook.Equals(parentBook))
                        "Linking to the same book. Use Node Link.".writeWarning();

                    NodeBook book;

                    if (LoadLinkedBook(out book)) {

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

            var cody = new CfgEncoder()//this.EncodeUnrecognized()
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
                case "b": data.DecodeInto(base.Decode); break; //data.Decode_Base(base.Decode, this); break;
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
