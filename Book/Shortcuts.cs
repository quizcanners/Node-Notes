using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Story Nodes/Shortcuts", order = 0)]
    public class Shortcuts : STD_ReferancesHolder, IKeepMySTD {

        [NonSerialized]public static List<NodeBook_Base> books = new List<NodeBook_Base>(); 
        public static NodeBook TryGetBook(int index) {
            var book = books.TryGet(index);

            if (book != null && book.GetType() == typeof(NodeBook_OffLoaded))
                book = books.LoadBook(book as NodeBook_OffLoaded);
            
            return book as NodeBook;

        }
        
        public static int playingInBook;
        public static int playingInNode;

        public static Node TryGetCurrentNode() {

            var book = TryGetBook(playingInBook);
            
            if (book != null) {
                var node = book.allBaseNodes[playingInNode];
                if (node == null)
                    Debug.Log("Node is null");
                else
                {
                    var nn = node as Node;
                    if (nn == null)
                        Debug.Log("Node can't hold subnodes");
                    else
                        return nn;
                }
                return null;
            }
            else
                Debug.Log("Book is null");

            return null;
        }

        [HideInInspector]
        [SerializeField]
        string std_Data = "";
        string currentPlayerName = "Unknown";
        public string Config_STD {
            get {
                string val = StuffLoader.LoadFromPersistantPath("Players", currentPlayerName);
                if (val != null)  
                    std_Data = val;
                return std_Data;
            }

            set {
                std_Data = value;
                StuffSaver.SaveToPersistantPath("Players", currentPlayerName, std_Data);
            }
        }
        
#if PEGI
        int inspectedBook = -1;

        public override bool PEGI() {
            bool changed = false;

            "Shortcuts".nl();

            "Active B:{0} N:{1} = {2}".F(playingInBook, playingInNode, TryGetCurrentNode().ToPEGIstring()).nl();

            if (inspectedBook == -1) {
                
                changed |= base.PEGI().nl();
                if (!showDebug)
                    changed |= "Player Name:".edit(ref currentPlayerName).nl();

                if ("Get all book names".Click().nl()) {
                    var lst = StuffLoader.ListFileNamesFromPersistantFolder(BookConversionExtensions.BooksFolder);

                    foreach (var e in lst) {
                        bool contains = false;
                        foreach (var b in books)
                            if (b.name.SameAs(e)) { contains = true; break; }

                        if (!contains) {
                            var off = new NodeBook_OffLoaded();
                            off.name = e;
                            off.IndexForPEGI = books.Count;
                            books.Add(off);
                        }
                    }
                }
            }
            else
                showDebug = false;

            if (!showDebug)
                "Books ".edit_List(books, ref inspectedBook, true);
            
            return changed;
        }
#endif

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("vals", Values.global, this)
            .Add("trigs", TriggerGroup.all)
            .Add("books", books, this)
            .Add("pb", playingInBook)
            .Add("pn", playingInNode);

        public override ISTD Decode(string data)
        {
            var ret = base.Decode(data);

            for (int i = 0; i < books.Count; i++)
                books[i].IndexForPEGI = i;

            return ret;
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "vals": data.DecodeInto(out Values.global, this); break;
                case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "books": data.DecodeInto(out books, this); break;
                case "pb": playingInBook = data.ToInt(); break;
                case "pn": playingInNode = data.ToInt(); break;
                default: return false;
            }
            return true;
        }
    }
}
