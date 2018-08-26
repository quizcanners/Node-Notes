using System.Collections;
using System.Collections.Generic;
using SharedTools_Stuff;
using UnityEngine;
using PlayerAndEditorGUI;

namespace LinkedNotes {
    public class NodeBook_OffLoaded : NodeBook_Base, IPEGI_ListInspect, IGotDisplayName {

        public override string NameForPEGIdisplay() => "Offloaded {0}".F(name);

        public override bool Decode(string tag, string data) {

            switch (tag) {
                case "n": name = data; break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name);

        public bool PEGI_inList(IList list, int ind, ref int edited) {

            this.ToPEGIstring().write();

            if (icon.Load.Click())
                Shortcuts.books.LoadBook(this);

            return false;
        }
    }
    
    public static class BookConversionExtensions {

        public const string BooksFolder = "Books";

        public static NodeBook_OffLoaded Offload (this List<NodeBook_Base> list, NodeBook book){
            if (book != null && list.Contains(book)) {
                int ind = list.IndexOf(book);
                book.SaveToPersistantPath(BooksFolder, book.name);
                var off = new NodeBook_OffLoaded {
                    name = book.name
                };
                list[ind] = off;
                return off;
            }
            else Debug.LogError("List does not contain the book you are unloading");
            return null;
        }

        public static NodeBook LoadBook (this List<NodeBook_Base> list, NodeBook_OffLoaded offloaded) {

            if (offloaded != null && list.Contains(offloaded)) {
                int ind = list.IndexOf(offloaded);
                var book = new NodeBook();
                book.LoadFromPersistantPath(BooksFolder, offloaded.name);
                list[ind] = book;
                //Debug.Log("{0} book loaded".F(book.name));
                return book;
            }
            else Debug.LogError("List does not contain the book you are loading");
            return null;
        }

    }

}
