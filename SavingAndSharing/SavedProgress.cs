using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;
using System;
using PlayerAndEditorGUI;

namespace NodeNotes
{
    public class CurrentUser: AbstractKeepUnrecognized_STD, IGotName, IPEGI, IGotDisplayName {

        public string startingPoint = "";
        public string userName = "Unknown";
        public List<BookMark> bookMarks = new List<BookMark>();
        public bool isADeveloper = false;

        #region GameNodes

        string preGameNodeSTD;

        public Dictionary<string, string> gameNodeTypeData = new Dictionary<string, string>();

        public void SaveCurrentNode() => preGameNodeSTD = Encode().ToString();

        public void LoadCurrentNode() => Decode(preGameNodeSTD);

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
                        using (loopLockLocal.Lock()) {

                            if (value == null) {
                                SaveCurrentBook();
                                _currentNode = null;
                                return;
                            }

                            if (_currentNode == null || _currentNode.root != value.root)
                                SetNextBook(value.root);
                        }
                    }

                    _currentNode = value;
                }
            }
        }

        public string NameForPEGI { get => userName; set => userName = value; }

        public void ExitCurrentBook() {
            if (bookMarks.Count == 0) {
                Debug.LogError("Can't Exit book because it was the Start of the journey.");
                return;
            }

            var lastB = bookMarks.Last().bookName;

            var b = Shortcuts.TryGetBook(lastB);

            if (b == null) {
                Debug.LogError("No {0} book found".F(lastB));
                return;
            }

            SetNextBook(b);
        }

        void SaveCurrentBook() {

            if (_currentNode != null) {

                var currentBook = _currentNode.root;

                if (currentBook != null) {
                    bookMarks.Add(new BookMark()
                    {
                        bookName = currentBook.NameForPEGI,
                        nodeIndex = _currentNode.IndexForPEGI,
                        values = Values.global.Encode().ToString()
                    });
                }
            }
        }

        public void ReturnToMark(BookMark mark) {
            if (bookMarks.Contains(mark))
                ReturnToMark(bookMarks.IndexOf(mark));
            else Debug.LogError("Book Marks don't contain {0}".F(mark.ToPEGIstring()));
        }

        void ReturnToMark (int ind) {

            if (ind < bookMarks.Count) {

                BookMark bm = bookMarks[ind];

                var book = Shortcuts.TryGetBook(bm.bookName);

                if (book == null)
                    Debug.LogError("No book {0} found".F(bm.bookName));
                else {
                    if (TrySetCurrentNode(book, ind))
                    {
                        bm.values.DecodeInto(out Values.global);
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
            
            if (bookMarks.Count == 0)
            {
                if (_currentNode != null)
                    startingPoint = _currentNode.root.NameForPEGI;
                else 
                    startingPoint = nextBook.NameForPEGI;
            }

            if (_currentNode != null && _currentNode.root == nextBook)
                return;

            var bMarkForNextBook = bookMarks.GetByIGotName(nextBook.NameForPEGI);

            if (bMarkForNextBook == null)  
                SaveCurrentBook();
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

        public string NameForPEGIdisplay =>
            "{0} FROM {1}".F(userName, startingPoint);

        #if PEGI
        public override void ResetInspector() {
            editedMark = -1;
            base.ResetInspector();
        }

        int editedMark = -1;
        public override bool Inspect() {

            bool changed = false; 

            this.ToPEGIstring().nl();

            if (Application.isEditor) 
                "Is A Developer ".toggleIcon(ref isADeveloper, true).nl();
            else if (!isADeveloper && "Turn to Developer".Click().nl())
                isADeveloper = true;
            
            changed |= "Marks ".enter_List(ref bookMarks,ref editedMark, ref inspectedStuff, 0).nl_ifNotEntered();

            changed |= "Values ".enter_Inspect(Values.global, ref inspectedStuff, 1).nl_ifNotEntered();
            
            return changed;
        }
        #endif

        #endregion

        #region Encoding_Decoding

        public override ISTD Decode(string data) {
  
            var ret = base.Decode(data);

            var b = Shortcuts.TryGetBook(tmpBook);

            if (b != null)
                Shortcuts.CurrentNode = b.allBaseNodes[tmpNode] as Node;

            return ret;
        }

        static string tmpBook;
        static int tmpNode;

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "bm": data.Decode_List(out bookMarks); break;
                case "vals": data.DecodeInto(out Values.global); break;
                case "cur": tmpNode = data.ToInt(); break;
                case "curB": tmpBook = data; break;
                case "dev": isADeveloper = data.ToBool(); break;
                case "n": userName = data; break;
                case "start": startingPoint = data; break;
                case "pgnd": data.Decode_Dictionary(out gameNodeTypeData); break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() {

            var cody = this.EncodeUnrecognized()
            .Add_IfNotEmpty("bm", bookMarks)
            .Add("vals", Values.global)
            .Add_Bool("dev", isADeveloper)
            .Add_String("n", userName)
            .Add_String("start", startingPoint)
            .Add_IfNotEmpty("pgnd", gameNodeTypeData);

            var cur = Shortcuts.CurrentNode;
            if (cur != null) {
                cody.Add_String("curB", cur.root.NameForPEGI)
                .Add("cur", cur.IndexForPEGI);
            }

            return cody;
        }

        #endregion
    }
}
