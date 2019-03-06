using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using System;
using STD_Logic;

namespace NodeNotes
{
    
    public class NodeBook : NodeBook_Base, IPEGI_ListInspect, IPEGI, IPEGI_Searchable {

        #region Values
        public int firstFree = 1;
        public CountlessStd<Base_Node> allBaseNodes = new CountlessStd<Base_Node>();

        private Base_Node this[int ind] => allBaseNodes[ind];

        public void Register(Base_Node node) {
            if (node != null) {

                var cur = allBaseNodes[node.IndexForPEGI];

                if (cur != null && cur != node) {
                  
                    while (this[firstFree] != null) firstFree++;
                    Debug.LogError("Trying to replace {0} with {1}. Assigning index {2}".F(cur, node, firstFree));
                    node.IndexForPEGI = firstFree;
                    allBaseNodes[firstFree] = node;
                    firstFree++;
                }
                else
                    allBaseNodes[node.IndexForPEGI] = node;

            } else Debug.LogError("Registering NULL node");
        }

        public List<BookEntryPoint> entryPoints = new List<BookEntryPoint>();
        public Dictionary<string, string> gameNodesData = new Dictionary<string, string>();
        public Node subNode = new Node();
        #endregion

        #region Inspector


        public override void ResetInspector()
        {
            _inspectedNode = -1;
            _inspectedEntry = -1;

            subNode.ResetInspector();

            foreach (var ep in entryPoints)
                ep.ResetInspector();

            base.ResetInspector();
        }

        private int _inspectedNode = -1;
        private int _inspectedEntry = -1;

        public override string NameForPEGI { get => subNode.name; set => subNode.name = value; }

        #if PEGI
        public bool String_SearchMatch(string searchString) =>  subNode.SearchMatch_Obj(searchString);
        
        public BookEntryPoint GetEntryPoint(string name) => entryPoints.GetByIGotName(name);

        public static NodeBook inspected;

        private bool _showShareOptions;

        public override bool Inspect()  {

            inspected = this;

            pegi.nl();

            var changed = false;
            
            if (subNode.inspectedItems == -1 && !subNode.InspectingSubNode) {

                if (inspectedItems == -1 && icon.Share.foldout("Share options",ref _showShareOptions)) {
                    
                    string data;
                    if (this.SendReceivePegi(subNode.name, "Books", out data)) {

                        var tmp = new NodeBook();
                        tmp.Decode(data);
                        if (tmp.NameForPEGI == NameForPEGI) Shortcuts.AddOrReplace(tmp);
                    }
                }
                
                "Entry Points".enter_List(ref entryPoints, ref _inspectedEntry, ref inspectedItems, 1).nl();

            }

            if (inspectedItems == -1)
                subNode.Nested_Inspect().nl(ref changed); 
            
            inspected = null;
            return changed;
        }
        
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            var changed = false;

            var tmp = NameForPEGI;
            if (pegi.editDelayed(ref tmp).changes(ref changed)) 
                TryRename(tmp);
            
            if (icon.Book.ClickUnFocus("Inspect book").changes(ref changed))
                edited = ind;

            if (icon.Save.Click("Save book (Also offloads RAM)"))
                Shortcuts.books.Offload(this);

            if (icon.Email.Click("Send this Book to somebody via email."))
                this.EmailData("Book {0} ".F(subNode), "Take a look at my Node Book");
            
            return changed;
        }
#endif
        #endregion

        #region Encode_Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("f", firstFree)
            .Add("sn", subNode)
            .Add_IfNotNegative("in", _inspectedNode)
            .Add_IfNotNegative("inE", _inspectedEntry)
            .Add("ep", entryPoints)
            .Add_IfNotNegative("i",inspectedItems)
            .Add("gn", gameNodesData);
          
        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "f": firstFree = data.ToInt(); break;
                case "sn": data.DecodeInto(out subNode); break;
                case "in": _inspectedNode = data.ToInt(); break;
                case "inE": _inspectedEntry = data.ToInt(); break;
                case "ep": data.Decode_List(out entryPoints); break;
                case "i": inspectedItems = data.ToInt(); break;
                case "gn": data.Decode_Dictionary(out gameNodesData); break;
                default: return false;
            }
            return true;
        }
    
        public override void Decode(string data) {
            
           data.DecodeTagsFor(this);

            if (subNode == null)
                subNode = new Node();

            subNode.Init(this, null);
            
        }

        #endregion

        #region Saving_Loading
        public void SaveToFile() => this.SaveToPersistentPath(BooksFolder, NameForPEGI);

        public void DeleteFile(string bookName) => FileDeleterUtils.DeleteFile_PersistentFolder(BooksFolder, bookName);

        public void TryRename(string newName)
        {

            if (subNode.name.SameAs(newName))
                return;

            if (newName.Length < 3)
            {
                Debug.LogError("Name is too short");
                return;
            }
            if (Shortcuts.books.GetByIGotName(newName) != null)
            {
                Debug.LogError("Book with this name already exists");
                return;
            }

            DeleteFile(NameForPEGI);
            NameForPEGI = newName;
            SaveToFile();
        }

        #endregion

    }
}