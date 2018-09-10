using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;
using System;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    public class SavedProgress: AbstractKeepUnrecognized_STD {

        public string userName = "Unknown";
        public List<BookMark> bookMarks = new List<BookMark>();
        public bool isADeveloper = false;

        void BookMarkOrReturn (NodeBook nextBook) {
            
            var existing = bookMarks.GetByIGotName(nextBook.NameForPEGI);
            
            if (existing == null) {

                var currentBook = Nodes_PEGI.CurrentNode.root;

                if (currentBook != null) {
                    var bm = new BookMark() {
                        bookName = currentBook.NameForPEGI,
                        nodeIndex = Nodes_PEGI.CurrentNode.IndexForPEGI,
                        values = Values.global.Encode().ToString()
                    };
                }
            } else {
                int ind = bookMarks.IndexOf(existing);
                bookMarks = bookMarks.GetRange(0, ind);
            }
        }

        int editedMark = -1;
        public override bool PEGI() {

            bool changed = base.PEGI();

            if (!isADeveloper && "Make A Developer".Click().nl())
                isADeveloper = true;

            if ((isADeveloper || Application.isEditor) && "Make a user".Click().nl())
                isADeveloper = false;

            "Marks ".edit_List(bookMarks,ref editedMark);

            return changed;
        }

        public override StdEncoder Encode() {
            var cody = this.EncodeUnrecognized()
            .Add("bm", bookMarks)
            .Add("vals", Values.global)
            .Add_Bool("dev", isADeveloper)
            .Add_String("n", userName);

            var cur = Nodes_PEGI.CurrentNode;
            if (cur != null) {
                cody.Add_String("curB", cur.root.NameForPEGI)
                .Add("cur", cur.IndexForPEGI);
            }

            return cody;
        }

        public override ISTD Decode(string data)
        {
            var ret = base.Decode(data);

            var b = Shortcuts.TryGetBook(tmpBook);

            if (b != null) 
                Nodes_PEGI.CurrentNode = b.allBaseNodes[tmpNode] as Node;

            return ret;
        }

        static string tmpBook;
        static int tmpNode;

        public override bool Decode(string tag, string data) {
           switch (tag) {
                case "bm": data.DecodeInto(out bookMarks); break;
                case "vals": data.DecodeInto(out Values.global); break;
                case "cur": tmpNode = data.ToInt(); break;
                case "curB": tmpBook = data; break;
                case "dev": isADeveloper = data.ToBool(); break;
                case "n": userName = data; break;
                default: return false;
           }

           return true;
        }



    }
}
