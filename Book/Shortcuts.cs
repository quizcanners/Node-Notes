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

        public static NodeBook TryGetBook(string name) {
            var book = books.GetByIGotName(name);

            if (book != null && book.GetType() == typeof(NodeBook_OffLoaded))
                book = books.LoadBook(book as NodeBook_OffLoaded);

            return book as NodeBook;
        }
        
        static readonly string _usersFolder = "Users";
      
        void LoadUser(string uname) => StuffLoader.LoadFromPersistantPath(_usersFolder, uname).DecodeInto(out user);
        
        void SaveUser() {

            if (!users.Contains(user.userName))
                users.Add(user.userName);
            user.SaveToPersistantPath(_usersFolder, user.userName);
        }

        void DelteUserFile(string uname) {
            StuffDeleter.DeleteFile_PersistantFolder(_usersFolder, uname);
        }

        static readonly string _generalStuffFolder = "General";

        static readonly string _generalStuffFile = "config";

        public void LoadAll() => this.LoadFromPersistantPath(_generalStuffFolder, _generalStuffFile);
        
        public void SaveAll() {
            SaveUser();
            StuffSaver.SaveToPersistantPath(_generalStuffFolder, _generalStuffFile, Encode().ToString());
        }
        
#if PEGI
        int inspectedBook = -1;
        string tmpUserName;
        public override bool PEGI() {
            bool changed = false;

            "Shortcuts".nl();

            if (inspectedBook == -1) {
                
                changed |= base.PEGI().nl();
                if (!showDebug) {
                    string usr = user.userName;

                    if ("Profile".select(ref usr, users).nl()) {
                        SaveUser();
                        LoadUser(usr);
                    }

                    pegi.nl();

                    "New User:".edit(60, ref tmpUserName);

                    if (tmpUserName.Length>3 && !users.Contains(tmpUserName))
                    {
                        if (icon.Add.Click("Add new user"))
                        {
                            user = new SavedProgress
                            {
                                userName = tmpUserName
                            };
                        }

                        if (icon.Refresh.Click("Rename {0}".F(user.userName)))
                        {
                            users.Remove(user.userName);

                            DelteUserFile(user.userName);

                            user.userName = tmpUserName;

                            SaveUser();
                        }
                    }

                    pegi.nl();
                }

                if ("Get all book names".Click().nl()) {
                    var lst = StuffLoader.ListFileNamesFromPersistantFolder(BookOffloadConversionExtensions.BooksFolder);

                    foreach (var e in lst) {
                        bool contains = false;
                        foreach (var b in books)
                            if (b.NameForPEGI.SameAs(e)) { contains = true; break; }

                        if (!contains) {
                            var off = new NodeBook_OffLoaded
                            {
                                name = e
                            };

                            books.Add(off);
                        }
                    }
                }
            }
            else
                showDebug = false;

            if (!showDebug)
                "Books ".edit_List(books, ref inspectedBook);
            
            return changed;
        }
#endif

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("trigs", TriggerGroup.all)
            .Add("books", books, this)
            .Add("us", users)
            .Add_String("curUser", user.userName);

        public override ISTD Decode(string data)
        {
            var ret = base.Decode(data);

            return ret;
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)  {
                case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "books": data.DecodeInto(out books, this); break;
                case "us": data.DecodeInto(out users); break;
                case "curUser": LoadUser(data); break;
                default: return false;
            }
            return true;
        }
    }
}
