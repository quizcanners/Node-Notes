using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace NodeNotes {

    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Story Nodes/Shortcuts", order = 0)]
    public class Shortcuts : STD_ReferencesHolder {

        #region Progress
     
        static LoopLock loopLock = new LoopLock();

        public static Node CurrentNode
        {

            get { return user.CurrentNode; }

            set
            {
                if (Application.isPlaying && visualLayer && loopLock.Unlocked)
                {
                    using (loopLock.Lock())
                    {
                        visualLayer.CurrentNode = value;
                    }
                }

                user.CurrentNode = value;
            }
        }

        public static NodesVisualLayerAbstract visualLayer;

        [NonSerialized] public static Base_Node Cut_Paste;

        #endregion

        #region Users

        [NonSerialized] public static List<string> users = new List<string>();

        public static CurrentUser user = new CurrentUser();

        static readonly string _usersFolder = "Users";

        void LoadUser(string uname) {
            StuffLoader.LoadFromPersistantPath(_usersFolder, uname).DecodeInto(out user);
            tmpUserName = uname;
        }

        void SaveUser() {
            if (!users.Contains(user.userName))
                users.Add(user.userName);
            user.SaveToPersistantPath(_usersFolder, user.userName);
        }

        void DeleteUser() {
            DeleteUser_File(user.userName);
            if (users.Count > 0)
                LoadUser(users[0]);
        }
        
        void DeleteUser_File(string uname) {
            StuffDeleter.DeleteFile_PersistantFolder(_usersFolder, uname);
            if (users.Contains(uname))
                users.Remove(uname);
        }

        void CreateUser(string uname)
        {
            if (!users.Contains(uname))
            {
                SaveUser();

                user = new CurrentUser
                {
                    userName = uname
                };

                users.Add(uname);
            }
            else Debug.LogError("User {0} already exists".F(uname));
        }

        void RenameUser (string uname) {

            DeleteUser();

            user.userName = uname;

            SaveUser();
        }

        #endregion

        #region Books
        [NonSerialized] public static List<NodeBook_Base> books = new List<NodeBook_Base>();

        public static NodeBook TryGetBook(string name)
        {
            var book = books.GetByIGotName(name);

            if (book != null && book.GetType() == typeof(NodeBook_OffLoaded))
                book = books.LoadBook(book as NodeBook_OffLoaded);

            return book as NodeBook;
        }

        static readonly string _generalStuffFolder = "General";

        static readonly string _generalStuffFile = "config";

        public void LoadAll() => this.LoadFromPersistantPath(_generalStuffFolder, _generalStuffFile);

        public void SaveAll()
        {
            SaveUser();
            StuffSaver.SaveToPersistantPath(_generalStuffFolder, _generalStuffFile, Encode().ToString());
        }

        #endregion

        #region Inspector
        int inspectedBook = -1;
        string tmpUserName;

#if PEGI

        NodeBook replaceRecieved;
        bool inspectReplacementOption;

        public override bool Inspect() {

            bool changed = false;

            if (inspectedBook == -1)
            {

                changed |= base.Inspect();

                if (inspectedStuff == -1)
                {
                    if (users.Count > 1 && icon.Delete.Click())
                        DeleteUser();

                    string usr = user.userName;
                    if (pegi.select(ref usr, users))
                    {
                        SaveUser();
                        LoadUser(usr);
                    }
                }

                if (icon.Enter.enter(ref inspectedStuff, 4))
                    changed |= user.Nested_Inspect();

                if (icon.Add.enter(ref inspectedStuff, 6))
                {
                    "New User:".edit(60, ref tmpUserName);

                    if (tmpUserName.Length > 3 && !users.Contains(tmpUserName))
                    {

                        if (icon.Add.Click("Add new user"))
                            CreateUser(tmpUserName);

                        if (icon.Replace.Click("Rename {0}".F(user.userName)))
                            RenameUser(tmpUserName);
                    }
                }

                if (inspectedStuff == -1)
                {

                   

                    pegi.nl();

                    if (Application.isEditor && icon.Folder.Click("Open Save files folder"))
                        StuffExplorer.OpenPersistantFolder(NodeBook_Base.BooksFolder);

                    if (icon.Refresh.Click("Will populate list with mentiones with books in Data folder without loading them")) {

                        var lst = StuffLoader.ListFileNamesFromPersistantFolder(NodeBook_Base.BooksFolder);

                        foreach (var e in lst)
                        {
                            bool contains = false;
                            foreach (var b in books)
                                if (b.NameForPEGI.SameAs(e)) { contains = true; break; }

                            if (!contains)
                            {
                                var off = new NodeBook_OffLoaded { name = e };
                                books.Add(off);
                            }
                        }
                    }
                }
            }
            else inspectedStuff = -1;

            if (inspectedStuff == -1) {

                "Books ".edit_List(books, ref inspectedBook);

                if (inspectedBook == -1)
                {

                    #region Paste Options

                    if (replaceRecieved != null)
                    {

                        if (replaceRecieved.NameForPEGI.enter(ref inspectReplacementOption))
                            replaceRecieved.Nested_Inspect();
                        else
                        {
                            if (icon.Done.ClickUnfocus()) {

                                var el = books.GetByIGotName(replaceRecieved);
                                if (el != null)
                                    books[books.IndexOf(el)] = replaceRecieved;
                                else books.Add(replaceRecieved);
                                
                                replaceRecieved = null;
                            }
                            if (icon.Close.ClickUnfocus())
                                replaceRecieved = null;
                        }
                    }
                    else  {

                        string tmp = "";
                        if ("Paste Messaged Book".edit(140, ref tmp) || STDExtensions.LoadOnDrop(out tmp)) {
                            var book = new NodeBook();
                            book.DecodeFromExternal(tmp);
                            if (books.GetByIGotName(book.NameForPEGI) == null)
                                books.Add(book);
                            else
                                replaceRecieved = book;
                        }
                    }
                    pegi.nl();

                    #endregion

                }

            }
            return changed;
            }
#endif
        #endregion

        #region Encode_Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("trigs", TriggerGroup.all)
            .Add("books", books, this)
            .Add("us", users)
            .Add_String("curUser", user.userName);

        public override ISTD Decode(string data) {

            var ret = base.Decode(data);

            return ret;
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)  {
                case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "books": data.DecodeInto_List(out books, this); break;
                case "us": data.DecodeInto(out users); break;
                case "curUser": LoadUser(data); break;
                default: return false;
            }
            return true;
        }

        #endregion
    }
}
