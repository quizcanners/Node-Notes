using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes
{
    public class NodeStoryBooks : ICfg, IPEGI
    {

        public List<NodeBook_Base> all = new List<NodeBook_Base>();

        public NodeBook_Base TryGetBook(IBookReference reff) => TryGetBook(reff.BookName, reff.AuthorName);

        public NodeBook_Base TryGetBook(string bookName, string authorName)
        {
            foreach (var b in all)
                if (b.NameForPEGI.Equals(bookName) && b.authorName.Equals(authorName))
                    return b;

            return null;
        }

        public bool TryGetLoadedBook(IBookReference reff, out NodeBook nodeBook) => TryGetLoadedBook(reff.BookName, reff.AuthorName, out nodeBook);

        public bool TryGetLoadedBook(string bookName, string authorName, out NodeBook nodeBook)
        {
            var book = TryGetBook(bookName, authorName);

            if (book == null)
            {
                nodeBook = null;
                return false;
            }

            if (book.GetType() == typeof(NodeBook_OffLoaded))
                book = (book as NodeBook_OffLoaded).LoadBook();

            nodeBook = book as NodeBook;

            return nodeBook != null;
        }

        public void AddOrReplace(NodeBook nb)
        {
            var el = all.GetByIGotName(nb);

            if (el != null)
            {
                if (Shortcuts.CurrentNode != null && Shortcuts.CurrentNode.parentBook == el)
                    Shortcuts.CurrentNode = null;
                all[all.IndexOf(el)] = nb;
            }
            else
                all.Add(nb);
        }

        #region Encode & Decode
        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);
        
        public bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "books": data.Decode_List(out all); break;
                default: return false;
            }
            return true;
        }

        public CfgEncoder Encode()
        {
            var cody = new CfgEncoder();

            cody.Add("books", all);

            return cody;
        }
        #endregion

        #region Inspector

        public void ResetInspector()
        {
            _inspectReplacementOption = false;
            _inspectedBook = -1;
            _replaceReceived = null;

            foreach (var b in all)
                b.ResetInspector();
        }

        private NodeBook _replaceReceived;
        private bool _inspectReplacementOption;
        public int _inspectedBook = -1;

     

        public bool Inspect()
        {
            var changed = false;

            var newBook = "Books ".edit_List(ref all, ref _inspectedBook, ref changed);

            if (newBook != null)
                newBook.authorName = Shortcuts.users.current.Name;

    
            if (_inspectedBook == -1)
            {

                #region Paste Options

                if (_replaceReceived != null)
                {

                    if (_replaceReceived.NameForPEGI.enter(ref _inspectReplacementOption))
                        _replaceReceived.Nested_Inspect();
                    else
                    {
                        if (icon.Done.ClickUnFocus())
                        {

                            var el = all.GetByIGotName(_replaceReceived);
                            if (el != null)
                                all[all.IndexOf(el)] = _replaceReceived;
                            else all.Add(_replaceReceived);

                            _replaceReceived = null;
                        }
                        if (icon.Close.ClickUnFocus())
                            _replaceReceived = null;
                    }
                }
                else
                {

                    string tmp = "";
                    if ("Paste Messaged Book".edit(140, ref tmp) || StdExtensions.DropStringObject(out tmp))
                    {
                        var book = new NodeBook();
                        book.DecodeFromExternal(tmp);
                        if (all.GetByIGotName(book.NameForPEGI) == null)
                            all.Add(book);
                        else
                            _replaceReceived = book;
                    }
                }
                pegi.nl();

                #endregion

            }


            return changed;
        }

        #endregion
    }
}