using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using System;
using System.IO;
using QcTriggerLogic;
using PlayerAndEditorGUI;

namespace NodeNotes {

    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Story Nodes/Shortcuts", order = 0)]
    public class Shortcuts : CfgReferencesHolder {

        public const string ProjectName = "NodeNotes";

        #region Progress

        static LoopLock loopLock = new LoopLock();

        public static Node CurrentNode
        {

            get { return user.CurrentNode; }

            set   {

                if (Application.isPlaying && visualLayer && loopLock.Unlocked)
                {
                    using (loopLock.Lock()) {
                        expectingLoopCall = true;
                        visualLayer.CurrentNode = value;
                        if (expectingLoopCall) {
                            expectingLoopCall = false;
                            user.CurrentNode = value;
                        }
                    }
                }
                else
                {
                    expectingLoopCall = false;
                    user.CurrentNode = value;
                }

            }
        }

        static bool expectingLoopCall = false;

        public static NodesVisualLayerAbstract visualLayer;

        [NonSerialized] public static Base_Node Cut_Paste;

        #endregion

        #region Users

        [NonSerialized] public static List<string> users = new List<string>();

        public static CurrentUser user = new CurrentUser();

        static readonly string _usersFolder = "Users";

        void LoadUser(string uname) {
            FileLoadUtils.LoadJsonFromPersistentPath(_usersFolder, uname).DecodeInto(out user);
            _tmpUserName = uname;
        }

        void SaveUser() {
            if (!users.Contains(user.Name))
                users.Add(user.Name);
            user.SaveToPersistentPath_Json(_usersFolder, user.Name);
        }

        void DeleteUser() {
            DeleteUser_File(user.Name);
            if (users.Count > 0)
                LoadUser(users[0]);
        }
        
        void DeleteUser_File(string uname) {
            FileDeleteUtils.Delete_PersistentFolder_Json(_usersFolder, uname);
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
                    Name = uname
                };

                users.Add(uname);
            }
            else Debug.LogError("User {0} already exists".F(uname));
        }

        void RenameUser (string uname) {

            DeleteUser();

            user.Name = uname;

            SaveUser();
        }

        #endregion

        #region Books
        [NonSerialized] public static List<NodeBook_Base> books = new List<NodeBook_Base>();

        public static NodeBook_Base TryGetBook(IBookReference reff) => TryGetBook(reff.BookName, reff.AuthorName);

        public static NodeBook_Base TryGetBook(string bookName, string authorName)
        {
 
            foreach (var b in books)
                if (b.NameForPEGI.Equals(bookName) && b.authorName.Equals(authorName))
                    return b;

            return null;
        }

        public static NodeBook TryGetLoadedBook(IBookReference reff) => TryGetLoadedBook(reff.BookName , reff.AuthorName);

        public static NodeBook TryGetLoadedBook(string bookName, string authorName) {

            var book = TryGetBook(bookName, authorName);
            
            if (book != null && book.GetType() == typeof(NodeBook_OffLoaded))
                book = books.LoadBook(book as NodeBook_OffLoaded);

            return book as NodeBook;
        }

        static readonly string _generalItemsFolder = "General";

        static readonly string _generalItemsFile = "config";

        public bool LoadAll() => this.LoadFromPersistentPath_Json(_generalItemsFolder, _generalItemsFile);

        public void SaveAll()
        {
            SaveUser();
            FileSaveUtils.SaveJsonToPersistentPath(_generalItemsFolder, _generalItemsFile, Encode().ToString());
        }

        public static void AddOrReplace(NodeBook nb) {

            var el = books.GetByIGotName(nb);

            if (el != null) {
                if (CurrentNode != null && CurrentNode.root == el)
                    CurrentNode = null;
                books[books.IndexOf(el)] = nb;
            }
            else
                books.Add(nb);
        }

        #endregion

        #region Inspector
        
        private string _tmpUserName;

        #if !NO_PEGI
        
        private int _inspectedBook = -1;

        public override void ResetInspector() {
            _inspectReplacementOption = false;
            _tmpUserName = "";
            _replaceReceived = null;
            _inspectedBook = -1;
            base.ResetInspector();

            foreach (var b in books)
                b.ResetInspector();
            
             user.ResetInspector();
        }

        private NodeBook _replaceReceived;
        private bool _inspectReplacementOption;

        private EncodedJson json = new EncodedJson();

        public override bool Inspect() {

            var changed = false;


            json.Nested_Inspect().nl();

            if (_inspectedBook == -1) {

                if (inspectedItems == -1) {
                    if (users.Count > 1 && icon.Delete.Click("Delete User"))
                        DeleteUser();

                    string usr = user.Name;
                    if (pegi.select(ref usr, users))
                    {
                        SaveUser();
                        LoadUser(usr);
                    }
                }

                if (icon.Enter.enter(ref inspectedItems, 4, "Inspect user"))
                    changed |= user.Nested_Inspect();

                if (icon.Edit.enter(ref inspectedItems, 6, "Edit or Add user")) {

                    "Name:".edit(60, ref _tmpUserName);

                    if (_tmpUserName.Length <= 3)
                        "Too short".writeHint();
                    else if (users.Contains(_tmpUserName))
                        "Name exists".writeHint();
                    else{
                        if (icon.Add.Click("Add new user"))
                            CreateUser(_tmpUserName);

                        if (icon.Replace.Click("Rename {0}".F(user.Name)))
                            RenameUser(_tmpUserName);
                    }
                }

                if (inspectedItems == -1)
                {

                    pegi.nl();

                    if (Application.isEditor) {

                        if (icon.Folder.Click("Open Save files folder").nl())
                            FileExplorerUtils.OpenPersistentFolder(NodeBook_Base.BooksRootFolder);
                        
                        var authorFolders = FileExplorerUtils.GetFolderNamesFromPersistentFolder(NodeBook_Base.BooksRootFolder);

                        foreach (var authorFolder in authorFolders) {

                            var fls = FileExplorerUtils.GetFileNamesFromPersistentFolder(Path.Combine(NodeBook_Base.BooksRootFolder, authorFolder));

                            foreach (var bookFile in fls) {

                                var loaded = TryGetBook(bookFile, authorFolder);

                                if (loaded == null) {
                                    "{0} by {1}".F(bookFile, authorFolder).write();

                                    if (icon.Insert.Click("Add current book to list"))
                                            books.Add(new NodeBook_OffLoaded(bookFile, authorFolder));

                                    pegi.nl();
                                }

                            }
                        }
                    }

                /*
                if (icon.Refresh.Click("Will populate list with mentions with books in Data folder without loading them")) {
                    
                    var lst = FileExplorerUtils.GetFileNamesFromPersistentFolder(NodeBook_Base.BooksRootFolder);

                    foreach (var e in lst)
                    {
                        var contains = false;
                        foreach (var b in books)
                            if (b.NameForPEGI.SameAs(e)) { contains = true; break; }

                        if (contains) continue;
                        
                        var off = new NodeBook_OffLoaded { name = e };
                        books.Add(off);
                        
                    }
                }*/
                }
            }
            else inspectedItems = -1;

            if (inspectedItems == -1) {

                var newBook = "Books ".edit_List(ref books, ref _inspectedBook, ref changed);

                if (newBook != null)
                    newBook.authorName = user.Name;
                

                if (_inspectedBook == -1)
                {

        #region Paste Options

                    if (_replaceReceived != null)
                    {

                        if (_replaceReceived.NameForPEGI.enter(ref _inspectReplacementOption))
                            _replaceReceived.Nested_Inspect();
                        else
                        {
                            if (icon.Done.ClickUnFocus()) {

                                var el = books.GetByIGotName(_replaceReceived);
                                if (el != null)
                                    books[books.IndexOf(el)] = _replaceReceived;
                                else books.Add(_replaceReceived);
                                
                                _replaceReceived = null;
                            }
                            if (icon.Close.ClickUnFocus())
                                _replaceReceived = null;
                        }
                    }
                    else  {

                        string tmp = "";
                        if ("Paste Messaged Book".edit(140, ref tmp) || StdExtensions.LoadOnDrop(out tmp)) {
                            var book = new NodeBook();
                            book.DecodeFromExternal(tmp);
                            if (books.GetByIGotName(book.NameForPEGI) == null)
                                books.Add(book);
                            else
                                _replaceReceived = book;
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

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("trigs", TriggerGroup.all)
            .Add("books", books, this)
            .Add("us", users)
            .Add_String("curUser", user.Name);
        
        public override bool Decode(string tg, string data)
        {
            switch (tg)  {
                case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "books": data.Decode_List(out books, this); break;
                case "us": data.Decode_List(out users); break;
                case "curUser": LoadUser(data); break;
                default: return false;
            }
            return true;
        }

        #endregion
    }
}
