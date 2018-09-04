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

        public string name = "Unknown";
        public List<BookMark> bookMarks = new List<BookMark>();
        public NodeBook currentBook;
        public bool isADeveloper = false;

        void NextBook (NodeBook newBook) {

            if (currentBook != null) {
                var bm = new BookMark()
                {
                    name = currentBook.name,
                    bookData = currentBook.Encode().ToString(),
                    nodeIndex = Nodes_PEGI.CurrentNode.IndexForPEGI
                };
            }

            currentBook = newBook;

        }

        public override bool PEGI()
        {
            bool changed = base.PEGI();

            if (!isADeveloper && "Make A Developer".Click().nl())
                isADeveloper = true;

            if ((isADeveloper || Application.isEditor) && "Make a user".Click().nl())
                isADeveloper = false;


            return changed;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("bm", bookMarks)
            .Add("vals", Values.global)
            .Add("cur", currentBook)
            .Add_String("n", name);

        public override bool Decode(string tag, string data) {
           switch (tag) {
                case "bm": data.DecodeInto(out bookMarks); break;
                case "vals": data.DecodeInto(out Values.global); break;
                case "cur": data.DecodeInto(out currentBook); break;
                case "n": name = data; break;
                default: return false;
           }

           return true;
        }



    }
}
