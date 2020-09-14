using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {

#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0018 // Inline variable declaration

    public class UsersData : IPEGI
    {
        
        public List<string> all = new List<string>();

        public CurrentUser current = new CurrentUser();

        static readonly string _usersFolder = "Users";
        
        public void LoadUser(string uname)
        {
            Shortcuts.CurrentNode = null;
            current = new CurrentUser();
            current.LoadFromPersistentPath(_usersFolder, uname);
            current.Name = uname;
            _tmpUserName = uname;
        }

        public void SaveCurrentToPersistentPath()
        {
            if (!all.Contains(current.Name))
                all.Add(current.Name);
            current.SaveToPersistentPath(_usersFolder, current.Name);
        }

        public void DeleteUser()
        {
            Shortcuts.CurrentNode = null;

            DeleteUser_File(current.Name);
            if (all.Count > 0)
                LoadUser(all[0]);
        }

        void DeleteUser_File(string uname)
        {
            QcFile.Delete.FromPersistentFolder(_usersFolder, uname, asBytes: true);
            if (all.Contains(uname))
                all.Remove(uname);
        }

        void CreateUser(string uname)
        {
            if (!all.Contains(uname))
            {
                SaveCurrentToPersistentPath();
                Shortcuts.CurrentNode = null;

                current = new CurrentUser
                {
                    Name = uname
                };

                all.Add(uname);
            }
            else Debug.LogError("User {0} already exists".F(uname));
        }

        public void RenameUser(string uname)
        {

            DeleteUser();

            current.Name = uname;

            SaveCurrentToPersistentPath();
        }

        #region Inspector

        public void ResetInspector()
        {
            _tmpUserName = "";
           // current.ResetInspector();
        }

        private string _tmpUserName = "";
        
        private int _inspectedItems = -1;

        public bool Inspect()
        {
            var changed = false;
            
            if (all.Count > 1 && icon.Delete.ClickConfirm("delUsr", "Are you sure you want to delete this User?"))
                DeleteUser();

            string usr = current.Name;
            if (pegi.select(ref usr, all))
            {
                SaveCurrentToPersistentPath();
                Shortcuts.CurrentNode = null;
                LoadUser(usr);
            }
            
            if (icon.Enter.enter(ref _inspectedItems, 0, "Inspect user").nl())
                current.Nested_Inspect().changes(ref changed);

            if ("Edit or Add user".enter(ref _inspectedItems, 1).nl())
            {
                "Name:".edit(60, ref _tmpUserName).nl();

                if (_tmpUserName.Length <= 3)
                    "Too short".writeHint();
                else if (all.Contains(_tmpUserName))
                    "Enter a new name to Add/Rename user".writeHint();
                else
                {
                    pegi.nl();

                    if ("Create New".Click("Add new user"))
                        CreateUser(_tmpUserName);

                    if ("Rename".ClickConfirm("rnUsr", "Do you want to rename a user {0}? This may break links between his books.".F(current.Name)))
                        RenameUser(_tmpUserName);
                }
            }

            return changed;
        }

        #endregion
    }


    public class CurrentUser: ICfg, IGotName, IPEGI, IGotDisplayName {

        //public string startingPoint = "";
        public string Name = "Unknown";
        public List<BookMark> bookMarks = new List<BookMark>();
        ListMetaData marksMeta = new ListMetaData("Book Marks", true, false, false, false);
        public bool isADeveloper;
        
        #region GameNodes
        public Dictionary<string, string> gameNodesData_PerUser = new Dictionary<string, string>();
        #endregion

        #region CurrentState

        static Node _currentNode;

        LoopLock loopLock = new LoopLock();

        public void FinalizeNodeChange(Node value)
        {
            bool bookChanged = _currentNode != null && (value == null || (_currentNode.parentBook != value.parentBook));

            if (isADeveloper && Application.isPlaying && bookChanged)
                CurrentNode.parentBook.UpdatePerBookConfigs();

            if (value == null)
            {
                SaveBookMark();
                _currentNode = null;
                return;
            }

            if (_currentNode == null || bookChanged)
                SetNextBook(value.parentBook);

            _currentNode = value;
        }

        public Node CurrentNode {
            get => _currentNode;
            set  {

                if (loopLock.Unlocked) {
                    using (loopLock.Lock()) {
                        Shortcuts.CurrentNode = value;
                    }
                } else
                    _currentNode = value;
                //Debug.LogError("Shouldn't have Loops here anymore");
            }
        }

        public string NameForPEGI { get => Name;  set => Name = value; }

        public void ExitCurrentBook() {
            if (bookMarks.Count == 0) {
                Debug.LogError("Can't Exit book because it was the Start of the journey.");
                return;
            }

            var lastB = bookMarks.TryGetLast();

            NodeBook b; 

            if (!Shortcuts.books.TryGetLoadedBook(lastB, out b)) {
                Debug.LogError("No {0} book found".F(lastB));
                return;
            }

            SetNextBook(b);
        }

        void SaveBookMark() {

            if (_currentNode != null) {
                
                var currentBook = _currentNode.parentBook;

                if (currentBook != null) {

                    bookMarks.Add(new BookMark
                    {
                        BookName = currentBook.NameForPEGI,
                        AuthorName = currentBook.authorName,
                        nodeIndex = _currentNode.IndexForPEGI,
                        values = Values.global.Encode().ToString(),
                        gameNodesData = gameNodesData_PerUser.Encode().ToString()

                    });
                }
                else Debug.LogError("Current Book was null");
            }
        }

        public void ReturnToBookMark() => ReturnToBookMark(bookMarks.TryGetLast());
        
        public void ReturnToBookMark(BookMark mark) {
            if (mark != null)
            {
                if (bookMarks.Contains(mark))
                    ReturnToBookMark(bookMarks.IndexOf(mark));
                else Debug.LogError("Book Marks don't contain {0}".F(mark));
            }
            else
            {
                Debug.LogError("Bookmark is null");
            }
        }

        void ReturnToBookMark (int ind) {

            if (ind < bookMarks.Count) {

                BookMark bm = bookMarks[ind];

                NodeBook book;

                if (!Shortcuts.books.TryGetLoadedBook(bm, out book))
                    Debug.LogError("No book {0} found".F(bm));
                else {
                    if (TrySetCurrentNode(book, bm.nodeIndex)) {
                        bm.gameNodesData.Decode_Dictionary(out gameNodesData_PerUser);
                        bm.values.DecodeInto(out Values.global);
                        if (bookMarks.Count>ind)
                            bookMarks = bookMarks.GetRange(0, ind);
                        //else
                        //Debug.LogError("Bookmark was set in Try Set current node");
                     
                    }
                    else
                        Debug.LogError("Need to implement default (HUB) node");
                }
            }
            else Debug.LogError("Book Marks don't hav element "+ind);
        }
        
        void SetNextBook (NodeBook nextBook) {

            if (nextBook == null)
                Debug.LogError("Next book is null");
            
            if (_currentNode != null && _currentNode.parentBook == nextBook)
                return;
            
            nextBook.LoadPerBookConfigs();

            var bMarkForNextBook = bookMarks.GetByIGotName(nextBook.NameForPEGI);

            if (bMarkForNextBook == null)  
                SaveBookMark();
            else  {
                int ind = bookMarks.IndexOf(bMarkForNextBook);

                if (TrySetCurrentNode(nextBook, bMarkForNextBook.nodeIndex))
                    bookMarks = bookMarks.GetRange(0, ind);
                else
                    Debug.LogError( "Need to implement default (HUB) node" );
            }
        }

        bool TrySetCurrentNode (NodeBook book, int nodeIndex) {

            var bNode = book.allBaseNodes[nodeIndex];

            if (bNode == null)
                Debug.LogError("No node {0} in {1}".F(nodeIndex, book.NameForPEGI));
            else {
                var n = bNode as Node;
                if (n == null)
                    Debug.LogError("Node_Base {0} is note a Node ({1})".F(nodeIndex, bNode.GetType()));
                else {
                    CurrentNode = n;
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Inspector

        private int _inspectedItems = -1;

        public string NameForDisplayPEGI()=>
            "{0} FROM {1}".F(Name, bookMarks.Count>0 ? "{0} by {1}".F(bookMarks[0].BookName, bookMarks[0].AuthorName) : "NO STORY");
        
        public bool Inspect() {

            bool changed = false; 

            this.GetNameForInspector().nl();

            if (Application.isEditor) 
                "Developer Mode".toggleIcon(ref isADeveloper).nl();
            else if (!isADeveloper && "Turn to Developer".Click().nl())
                isADeveloper = true;
            
            marksMeta.enter_List(ref bookMarks, ref _inspectedItems, 0).nl_ifNotEntered(ref changed);

            "Values ".enter_Inspect(Values.global, ref _inspectedItems, 1).changes(ref changed);

            if (pegi.IsFoldedOut && Values.global.CountForInspector()>0 &&  icon.Delete.ClickConfirm("ResAllTrg","Reset all triggers").nl())
                Values.global.Clear();

            if (bookMarks.Count == 0 && CurrentNode != null && "NULL node (Reset User)".ClickConfirm("ExB"))
            {
                Values.global.Clear();
                CurrentNode = null;
            }

            return changed;
        }

        #endregion

        #region Encoding & Decoding

        private int _tmpNode;
        private string _tmpBookName;
        private string _tmpAuthorName;

        public void Decode(string data) {

            this.DecodeTagsFrom(data);

            NodeBook book;

            if (Shortcuts.books.TryGetLoadedBook(_tmpBookName, _tmpAuthorName, out book))
                Shortcuts.CurrentNode = book.allBaseNodes[_tmpNode] as Node;
            else
            {
                Debug.LogError("Book {0} by {1} not found".F(_tmpBookName, _tmpAuthorName));
                ReturnToBookMark();
            }
        }
        
        public bool Decode(string tg, string data) {
            switch (tg) {
                case "bm": data.Decode_List(out bookMarks); break;
                case "vals": data.DecodeInto(out Values.global); break;
                case "cur": _tmpNode = data.ToInt(); break;
                case "curB": _tmpBookName = data; break;
                case "curA": _tmpAuthorName = data; break;;
                case "dev": isADeveloper = data.ToBool(); break;
                case "n": Name = data; break;
                //case "start": startingPoint = data; break;
                case "pgnd": data.Decode_Dictionary(out gameNodesData_PerUser); break;
                default: return false;
            }
            return true;
        }

        public CfgEncoder Encode() {

            var cody = new CfgEncoder()//this.EncodeUnrecognized()
            .Add_IfNotEmpty("bm", bookMarks)
            .Add("vals", Values.global)
            .Add_Bool("dev", isADeveloper)
            .Add_String("n", Name)
            .Add_IfNotEmpty("pgnd", gameNodesData_PerUser);

            var cur = Shortcuts.CurrentNode;
            if (cur != null) {
                cody.Add_String("curB", cur.parentBook.NameForPEGI)
                    .Add_String("curA", cur.parentBook.AuthorName)
                .Add("cur", cur.IndexForPEGI);
            }

            return cody;
        }

        #endregion
    }
}
