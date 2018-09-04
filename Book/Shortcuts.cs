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
    public class Shortcuts : STD_ReferancesHolder {

        [NonSerialized]public static List<NodeBook_Base> books = new List<NodeBook_Base>();
        [NonSerialized]public static List<string> users = new List<string>();

        public static SavedProgress user = new SavedProgress();

        public static NodeBook TryGetBook(int index) {
            var book = books.TryGet(index);

            if (book != null && book.GetType() == typeof(NodeBook_OffLoaded))
                book = books.LoadBook(book as NodeBook_OffLoaded);
            
            return book as NodeBook;

        }

        public static NodeBook TryGetBook(string name) {
            var book = books.GetByIGotName(name);

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

        static string FolderForKeepingStuff = "Unknown";
     
        void LoadUser(string uname) => StuffLoader.LoadFromPersistantPath("Users", uname).DecodeInto(out user);
        
        void SaveUser()
        {
            if (!users.Contains(user.userName))
                users.Add(user.userName);
            user.SaveToPersistantPath("Users", user.userName);
        }

        public void LoadAll() => StuffLoader.LoadFromPersistantPath("Players", FolderForKeepingStuff);
        
        public void SaveAll() {
            SaveUser();
            StuffSaver.SaveToPersistantPath("Players", FolderForKeepingStuff, Encode().ToString());
        }
        
#if PEGI
        int inspectedBook = -1;
        string tmpUserName;
        public override bool PEGI() {
            bool changed = false;

            "Shortcuts".nl();

            "Active B:{0} N:{1} = {2}".F(playingInBook, playingInNode, TryGetCurrentNode().ToPEGIstring()).nl();

            if (inspectedBook == -1) {
                
                changed |= base.PEGI().nl();
                if (!showDebug)
                {
                    string usr = user.userName;

                    if ("Profile".select(ref usr, users).nl()) {
                        SaveUser();
                        LoadUser(usr);
                    }

                    "New User:".edit(60, ref tmpUserName);

                    if (!users.Contains(tmpUserName) && icon.Add.Click("Add new user")) {
                        user = new SavedProgress
                        {
                            userName = tmpUserName
                        };
                    }

                    pegi.nl();

                }

                if ("Get all book names".Click().nl()) {
                    var lst = StuffLoader.ListFileNamesFromPersistantFolder(BookOffloadConversionExtensions.BooksFolder);

                    foreach (var e in lst) {
                        bool contains = false;
                        foreach (var b in books)
                            if (b.name.SameAs(e)) { contains = true; break; }

                        if (!contains) {
                            var off = new NodeBook_OffLoaded
                            {
                                name = e,
                                IndexForPEGI = books.Count
                            };

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
            .Add("pn", playingInNode)
            .Add("us", users)
            .Add_String("curUser", user.userName);

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
                case "us": data.DecodeInto(out users); break;
                case "curUser": LoadUser(data); break;
                default: return false;
            }
            return true;
        }
    }
}
