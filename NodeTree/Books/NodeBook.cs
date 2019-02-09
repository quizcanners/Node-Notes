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
        public int firstFree = 0;
        public CountlessSTD<Base_Node> allBaseNodes = new CountlessSTD<Base_Node>();

        Base_Node this[int ind] { get { return allBaseNodes[ind]; } }

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
        public Dictionary<string, string> gameNodeTypeData = new Dictionary<string, string>();
        public Node subNode = new Node();
        #endregion

        #region Inspector

        public bool String_SearchMatch(string searchString) =>  subNode.SearchMatch_Obj(searchString);
        
        public override void ResetInspector()
        {
            inspectedNode = -1;
            inspectedEntry = -1;

            subNode.ResetInspector();

            foreach (var ep in entryPoints)
                ep.ResetInspector();

            base.ResetInspector();
        }

        int inspectedNode = -1;
        int inspectedEntry = -1;

        public override string NameForPEGI { get => subNode.name; set => subNode.name = value; }

        #if PEGI
        public BookEntryPoint GetEntryPoint(string name) => entryPoints.GetByIGotName(name);

        public static NodeBook inspected;

        public override bool Inspect()  {
            bool changed = false;
            inspected = this;

            pegi.nl();

            if (subNode.inspectedStuff == -1 && !subNode.InspectingSubnode) {

                if (inspectedStuff == -1)
                {
                    
                    string data;
                    if (this.Send_Recieve_PEGI(subNode.name, "Books", out data)) {

                        NodeBook tmp = new NodeBook();
                        tmp.Decode(data);
                        if (tmp.NameForPEGI == NameForPEGI) Shortcuts.AddOrReplace(tmp);
                    }
                }
                

                "Entry Points".enter_List(ref entryPoints, ref inspectedEntry, ref inspectedStuff, 1).nl();

            }

            if (inspectedStuff == -1)
            changed |= subNode.Nested_Inspect(); //"Root Node".NestedInspect(); // (subNode, ref inspectedStuff, 2);
      
            
            inspected = null;
            return changed;
        }
        
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            var changed = false;

            string tmp = NameForPEGI;
            if (pegi.editDelayed(ref tmp)) 
                TryRename(tmp);
            
            if (icon.Edit.ClickUnfocus())
                edited = ind;

            if (icon.Save.Click())
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
            .Add_IfNotNegative("in", inspectedNode)
            .Add_IfNotNegative("inE", inspectedEntry)
            .Add("ep", entryPoints)
            .Add_IfNotNegative("i",inspectedStuff)
            .Add("gn", gameNodeTypeData);
          
        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": subNode.name = data; break;
                case "f": firstFree = data.ToInt(); break;
                case "sn": data.DecodeInto(out subNode); break;
                case "in": inspectedNode = data.ToInt(); break;
                case "inE": inspectedEntry = data.ToInt(); break;
                case "ep": data.Decode_List(out entryPoints); break;
                case "i": inspectedStuff = data.ToInt(); break;
                case "gn": data.Decode_Dictionary(out gameNodeTypeData); break;
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
        public void SaveToFile() => this.SaveToPersistantPath(BooksFolder, NameForPEGI);

        public void DeleteFile(string bname) => StuffDeleter.DeleteFile_PersistantFolder(BooksFolder, bname);

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