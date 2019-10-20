using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using QcTriggerLogic;
using System;
using PlayerAndEditorGUI;

namespace NodeNotes {
    
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0018 // Inline variable declaration

    public class CurrentUser: AbstractKeepUnrecognizedCfg, IGotName, IPEGI, IGotDisplayName {

        public string startingPoint = "";
        public string Name = "Unknown";
        public List<BookMark> bookMarks = new List<BookMark>();
        ListMetaData marksMeta = new ListMetaData("Book Marks", true, false, false, false);
        public bool isADeveloper = false;

        private static int tmpNode;
        private string tmpBookName;
        private string tmpAuthorName;
        
        #region GameNodes
        public Dictionary<string, string> gameNodesData_PerUser = new Dictionary<string, string>();
        #endregion

        #region CurrentState

        static Node _currentNode;

        LoopLock loopLock = new LoopLock();
        LoopLock loopLockLocal = new LoopLock();

        public Node CurrentNode {
            get => _currentNode;
            set  {

                if (loopLock.Unlocked) {
                    using (loopLock.Lock()) {
                        Shortcuts.CurrentNode = value;
                    }

                }  else  {

                    if (loopLockLocal.Unlocked) {
                        using (loopLockLocal.Lock())
                        {

                            bool bookChanged = _currentNode != null && (value == null || (_currentNode.parentBook != value.parentBook));

                            if (Application.isPlaying && bookChanged)
                                CurrentNode.parentBook.UpdatePerBookVisualBackgroundConfigs();

                            if (value == null) {
                                SaveBookMark();
                                _currentNode = null;
                                return;
                            }

                            if (_currentNode == null || bookChanged)
                                SetNextBook(value.parentBook);
                        }
                    }

                    _currentNode = value;
                }
            }
        }

        public string NameForPEGI { get => Name;  set => Name = value; }

        public void ExitCurrentBook() {
            if (bookMarks.Count == 0) {
                Debug.LogError("Can't Exit book because it was the Start of the journey.");
                return;
            }

            var lastB = bookMarks.Last();

            NodeBook b; 

            if (!Shortcuts.TryGetLoadedBook(lastB, out b)) {
                Debug.LogError("No {0} book found".F(lastB));
                return;
            }

            SetNextBook(b);
        }

        void SaveBookMark() {

            if (_currentNode != null) {
                
                var currentBook = _currentNode.parentBook;

                if (currentBook != null) {

                    bookMarks.Add(new BookMark()
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
            else Debug.LogError("Bookmark is null");
        }

        void ReturnToBookMark (int ind) {

            if (ind < bookMarks.Count) {

                BookMark bm = bookMarks[ind];

                NodeBook book;

                if (!Shortcuts.TryGetLoadedBook(bm, out book))
                    Debug.LogError("No book {0} found".F(bm));
                else {
                    if (TrySetCurrentNode(book, bm.nodeIndex)) {
                        bm.gameNodesData.Decode_Dictionary(out gameNodesData_PerUser);
                        bm.values.DecodeInto(out Values.global);
                        if (bookMarks.Count<=ind)
                            Debug.LogError("Bookmark was set in Try Set current node");
                        else 
                            bookMarks = bookMarks.GetRange(0, ind);
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
            
            if (bookMarks.Count == 0) {
                if (_currentNode != null)
                    startingPoint = _currentNode.parentBook.NameForPEGI;
                else 
                    startingPoint = nextBook.NameForPEGI;
            }

            if (_currentNode != null && _currentNode.parentBook == nextBook)
                return;
            
            nextBook.LoadPerBookBackgroundConfigs();

            var bMarkForNextBook = bookMarks.GetByIGotName(nextBook.NameForPEGI);

            if (bMarkForNextBook == null)  
                SaveBookMark();
            else  {
                int ind = bookMarks.IndexOf(bMarkForNextBook);

                if (TrySetCurrentNode(nextBook, bMarkForNextBook.nodeIndex))
                {
                    bookMarks = bookMarks.GetRange(0, ind);
                } else
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

        public string NameForDisplayPEGI()=>
            "{0} FROM {1}".F(Name, startingPoint);
        
        public override bool Inspect() {

            bool changed = false; 

            this.GetNameForInspector().nl();

            if (Application.isEditor) 
                "Developer Mode".toggleIcon(ref isADeveloper).nl();
            else if (!isADeveloper && "Turn to Developer".Click().nl())
                isADeveloper = true;
            
            marksMeta.enter_List(ref bookMarks, ref inspectedItems, 0).nl_ifNotEntered(ref changed);

            "Values ".enter_Inspect(Values.global, ref inspectedItems, 1).changes(ref changed);

            if (pegi.IsFoldedOut && Values.global.CountForInspector()>0 &&  icon.Delete.Click("Reset all triggers"))
                Values.global.Clear();


            return changed;
        }
   
        #endregion

        #region Encoding_Decoding

        public override void Decode(string data) {
  
            base.Decode(data);

            NodeBook book;

            if (Shortcuts.TryGetLoadedBook(tmpBookName, tmpAuthorName, out book))
                Shortcuts.CurrentNode = book.allBaseNodes[tmpNode] as Node;
            else
                ReturnToBookMark();
        }
        
        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "bm": data.Decode_List(out bookMarks); break;
                case "vals": data.DecodeInto(out Values.global); break;
                case "cur": tmpNode = data.ToInt(); break;
                case "curB": tmpBookName = data; break;
                case "curA": tmpAuthorName = data; break;;
                case "dev": isADeveloper = data.ToBool(); break;
                case "n": Name = data; break;
                case "start": startingPoint = data; break;
                case "pgnd": data.Decode_Dictionary(out gameNodesData_PerUser); break;
                default: return false;
            }
            return true;
        }

        public override CfgEncoder Encode() {

            var cody = this.EncodeUnrecognized()
            .Add_IfNotEmpty("bm", bookMarks)
            .Add("vals", Values.global)
            .Add_Bool("dev", isADeveloper)
            .Add_String("n", Name)
            .Add_String("start", startingPoint)
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
