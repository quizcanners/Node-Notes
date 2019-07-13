using System.Collections;
using System.Collections.Generic;
using QuizCannersUtilities;
using UnityEngine;
using PlayerAndEditorGUI;

namespace NodeNotes {
    public class NodeBook_OffLoaded : NodeBook_Base, IPEGI_ListInspect, IGotDisplayName {

        public override string NameForDisplayPEGI() => "{0} by {1}".F(name, authorName);
        
        public string name;

        public override string NameForPEGI { get => name; set => name = value; }

        #region Encode/Decode

        public override bool Decode(string tg, string data) {

            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "n": name = data; break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("n", name);

        #endregion
        
        public bool InspectInList(IList list, int ind, ref int edited) {
            this.GetNameForInspector().write();

            if (icon.Load.Click())
                Shortcuts.books.LoadBook(this);

            return false;
        }

        public NodeBook_OffLoaded() {

        }

        public NodeBook_OffLoaded(IBookReference reff)
        {
            NameForPEGI = reff.BookName;
            AuthorName = reff.AuthorName;
        }

        public NodeBook_OffLoaded(string name, string author)
        {
            NameForPEGI = name;
            AuthorName = author;
        }

    }
    
    public static class BookOffloadConversionExtensions {
        
        public static NodeBook_OffLoaded Offload (this List<NodeBook_Base> list, NodeBook book){
            if (book != null && list.Contains(book)) {
                int ind = list.IndexOf(book);
                book.SaveToFile();
                var off = new NodeBook_OffLoaded(book);
               
                list[ind] = off;
                return off;
            }
            else Debug.LogError("List does not contain the book you are unloading");
            return null;
        }

        public static NodeBook LoadBook (this List<NodeBook_Base> list, NodeBook_OffLoaded offloaded) {

            if (offloaded != null && list.Contains(offloaded)) {
                var ind = list.IndexOf(offloaded);
                var book = new NodeBook();

                if (book.TryLoad(offloaded)) {
                    list[ind] = book;
                    return book;
                }

            }
            else Debug.LogError("List does not contain the book you are loading");
            return null;
        }
    }

}
