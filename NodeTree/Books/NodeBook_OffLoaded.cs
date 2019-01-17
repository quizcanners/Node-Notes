using System.Collections;
using System.Collections.Generic;
using QuizCannersUtilities;
using UnityEngine;
using PlayerAndEditorGUI;

namespace NodeNotes {
    public class NodeBook_OffLoaded : NodeBook_Base, IPEGI_ListInspect, IGotDisplayName {

        public override string NameForPEGIdisplay => "{0} [Offloaded]".F(name);

        public string name;

        public override string NameForPEGI { get => name; set => name = value; }

        #region Encode/Decode

        public override bool Decode(string tag, string data) {

            switch (tag) {
                case "n": name = data; break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name);

        #endregion

#if PEGI
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            this.ToPEGIstring().write();

            if (icon.Load.Click())
                Shortcuts.books.LoadBook(this);

            return false;
        }
#endif
    }
    
    public static class BookOffloadConversionExtensions {

    
        
        public static NodeBook_OffLoaded Offload (this List<NodeBook_Base> list, NodeBook book){
            if (book != null && list.Contains(book)) {
                int ind = list.IndexOf(book);
                book.SaveToFile(); 
                var off = new NodeBook_OffLoaded {
                    name = book.NameForPEGI
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
                book.LoadFromPersistantPath(NodeBook_Base.BooksFolder, offloaded.name);
                list[ind] = book;
                return book;
            }
            else Debug.LogError("List does not contain the book you are loading");
            return null;
        }
    }

}
