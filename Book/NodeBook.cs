using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;
using STD_Logic;

namespace NodeNotes
{
    
    public class NodeBook : NodeBook_Base, IPEGI_ListInspect, IPEGI {
        
        public int firstFree = 0;
        public CountlessSTD<Base_Node> allBaseNodes = new CountlessSTD<Base_Node>();
        public List<BookEntryPoint> entryPoints = new List<BookEntryPoint>();
        public Node subNode = new Node();

      

        #region Inspector
        int inspectedNode = -1;
        int inspectedEntry = -1;

        public override string NameForPEGI { get => subNode.name; set => subNode.name = value; }

#if PEGI
          public BookEntryPoint GetEntryPoint(string name) => entryPoints.GetByIGotName(name);

        public static NodeBook inspected;

        int inspectedStuff = -1;

        public override bool PEGI()  {
            bool changed = false;
            inspected = this;

            //if (subNode.inspectedSubnode == -1)
            "Entry Points".fold_enter_exit_List(entryPoints, ref inspectedEntry, ref inspectedStuff, 0).nl();

            if ("Subnode".fold_enter_exit(ref inspectedStuff, 1))
            changed |= subNode.Nested_Inspect();

            pegi.nl();
            
            inspected = null;
            return changed;
        }
        
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            var changed = false;

            string tmp = subNode.name;
            if (pegi.editDelayed(ref tmp)) 
                TryRename(tmp);
            
            if (icon.Edit.Click())
                edited = ind;

            if (icon.Save.Click())
                Shortcuts.books.Offload(this);

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
            .Add("ep", entryPoints);
          
        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": subNode.name = data; break;
                case "f": firstFree = data.ToInt(); break;
                case "sn": data.DecodeInto(out subNode); break;
                case "in": inspectedNode = data.ToInt(); break;
                case "inE": inspectedEntry = data.ToInt(); break;
                case "ep": data.DecodeInto(out entryPoints); break;
                default: return false;
            }
            return true;
        }
    
        public override ISTD Decode(string data) {
            
           var ret = data.DecodeTagsFor(this);

            if (subNode == null)
                subNode = new Node();

            subNode.Init(this, null);
            
            return ret;
        }

        #endregion

        #region Saving_Loading
        public void SaveToFile() => this.SaveToPersistantPath(BooksFolder, NameForPEGI);

        public void DeleteFile(string bname) => StuffDeleter.DeleteFile_PersistantFolder(BooksFolder, bname);

        public void TryRename(String newName)
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

            DeleteFile(subNode.name);
            subNode.name = newName;
            SaveToFile();
        }
        #endregion

    }
}