using System.Collections;
using System.Collections.Generic;
using System.IO;
using NodeNotes_Visual; // TODO: Move configs to visual layer
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes
{
    
    #pragma warning disable IDE0018 // Inline variable declaration

    public class NodeBook : NodeBook_Base, ICfgCustom, IPEGI_ListInspect, IPEGI, IPEGI_Searchable
    {

        #region Values

        public int firstFree = 1;
        public CountlessCfg<Base_Node> allBaseNodes = new CountlessCfg<Base_Node>();

        private Base_Node this[int ind] => allBaseNodes[ind];

        public override NodeBook AsLoadedBook => this;
        
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
        public Dictionary<string, CfgData> gameNodesConfigs = new Dictionary<string, CfgData>();
        public Dictionary<string, CfgData> presentationModesConfigs = new Dictionary<string, CfgData>();

        private CfgData _visualLayerConfig;

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
       
        public bool String_SearchMatch(string searchString) => pegi.Try_SearchMatch_Obj(subNode, searchString);
        
        public BookEntryPoint GetEntryPoint(string name) => entryPoints.GetByIGotName(name);

        public static NodeBook inspected;

        private bool _showShareOptions;

        public bool Inspect()  {

            inspected = this;

            pegi.nl();

            var changed = false;

            if ((subNode._inspectedItems == -1 && !subNode.InspectingSubNode) || Shortcuts.CurrentNode == null)
            {
                if (_inspectedItems == -1 && icon.Share.foldout("Share options", ref _showShareOptions))
                {

                    CfgData data;
                    if (this.SendReceivePegi(subNode.name, "Books", out data))
                    {

                        var tmp = new NodeBook();
                        tmp.Decode(data);
                        if (tmp.NameForPEGI == NameForPEGI) Shortcuts.books.AddOrReplace(tmp);
                    }
                }

                "Entry Points".enter_List(ref entryPoints, ref _inspectedEntry, ref _inspectedItems, 1).nl(ref changed);
            }
            else _inspectedItems = -1;

            if (_inspectedItems == -1) {

                "Author: {0} {1}".F(authorName, this.EditedByCurrentUser() ? "(ME)" : "").write();

                if (!subNode.InspectingSubNode) {

                    "Change".select(ref replacingAuthor, Shortcuts.users.all);

                    if (replacingAuthor != -1 && replacingAuthor < Shortcuts.users.all.Count &&
                        !Shortcuts.users.all[replacingAuthor].Equals(authorName)
                        && icon.Replace.ClickConfirm("repAu",
                            "Changing an author may break links to this book from other books."))
                        authorName = Shortcuts.users.all[replacingAuthor];
                }

                pegi.nl();

                if (!subNode.InspectingSubNode)
                    "ROOT NODE:".nl(PEGI_Styles.ListLabel);

                subNode.Nested_Inspect().nl(ref changed);

            }

            inspected = null;
            return changed;
        }

        private int replacingAuthor = -1;

        public bool InspectInList(IList list, int ind, ref int edited) {
            var changed = false;

            var tmp = NameForPEGI;

            if (this.EditedByCurrentUser())
            {
                if (pegi.editDelayed(ref tmp).changes(ref changed))
                    TryRename(tmp);
            }
            else NameForDisplayPEGI().write();

            if (icon.Enter.ClickUnFocus("Inspect book").changes(ref changed))
                edited = ind;

            if (icon.Save.Click("Save book (Also offloads RAM)"))
                this.Offload();

            if (this.EditedByCurrentUser() && icon.Undo.ClickConfirm("ldBk"+ NameForDisplayPEGI(), "This will reload your Book from resources"))
                this.Reload();
            
            if (icon.Email.Click("Send this Book to somebody via email."))
                this.EmailData("Book {0} ".F(subNode), "Take a look at my Node Book");
            
            return changed;
        }

        #endregion

        #region Encode_Decode

        public bool loadedPresentation;

        public void LoadPerBookConfigs() {
            
            if (Application.isPlaying && NodesVisualLayer.Instance) {
                
                foreach (var bg in NodesVisualLayer.Instance.presentationControllers) {
                    CfgData data;
                    presentationModesConfigs.TryGetValue(bg.ClassTag, out data);
                    bg.DecodeFull(data);
                }

                NodesVisualLayerAbstract.InstAsNodesVisualLayer.Decode(_visualLayerConfig);

                loadedPresentation = true;
            }
        }

        public void UpdatePerBookConfigs() {
            if (Application.isPlaying && NodesVisualLayer.Instance) {
                if (loadedPresentation)
                    foreach (var bg in NodesVisualLayer.Instance.presentationControllers) {
                        var data = bg.EncodePerBookData().CfgData;
                        presentationModesConfigs[bg.ClassTag] = data;
                    }

                _visualLayerConfig = NodesVisualLayerAbstract.InstAsNodesVisualLayer.EncodePerBookData().CfgData;
            }
        }

        public override CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("n", NameForPEGI) // for Offloaded book
            .Add("f", firstFree)
            .Add("sn", subNode)
            .Add_IfNotNegative("in", _inspectedNode)
            .Add_IfNotNegative("inE", _inspectedEntry)
            .Add_IfNotEmpty("ep", entryPoints)
            .Add_IfNotNegative("i",_inspectedItems)
            .Add_IfNotEmpty("gn", gameNodesConfigs)
            .Add_IfNotEmpty("bg", presentationModesConfigs)
            .Add("visLr", _visualLayerConfig);
          
        public override void Decode(string tg, CfgData data) {
            switch (tg) {
                case "b": data.Decode(base.Decode); break;//data.Decode_Base(base.Decode, this); break;
                case "f": firstFree = data.ToInt(); break;
                case "sn": data.Decode(out subNode); break;
                case "in": _inspectedNode = data.ToInt(0); break;
                case "inE": _inspectedEntry = data.ToInt(0); break;
                case "ep": data.ToList(out entryPoints); break;
                case "i": _inspectedItems = data.ToInt(0); break;
                case "gn": data.Decode_Dictionary(out gameNodesConfigs); break;
                case "bg": data.Decode_Dictionary(out presentationModesConfigs); break;
                case "visLr": _visualLayerConfig = data; break;
         
            }
        }
    
        public void Decode(CfgData data) {

            this.DecodeTagsFrom(data);

            if (subNode == null)
                subNode = new Node();

            subNode.Init(this, null);
            
        }

        #endregion

        #region Saving_Loading

        private bool AuthoringAStory => this.EditedByCurrentUser() && Application.isEditor;

        public void SaveToFile() {
            if (AuthoringAStory) {
                this.SaveToResources(Shortcuts.ProjectName, this.BookFolder(), BookName);
                QcUnity.RefreshAssetDatabase();
            }
            else
            {
                this.SaveToPersistentPath(this.BookFolder(), NameForPEGI);
            }
        }

        public bool TryLoad(IBookReference reff) {

            if (reff.EditedByCurrentUser() && Application.isEditor) 
                return this.TryLoadFromResources(reff.BookFolder(), reff.BookName);
            
            return this.LoadFromPersistentPath(reff.BookFolder(), reff.BookName);
        }

        private void DeleteFile(string bookName) {

            if (AuthoringAStory) {
                QcFile.Delete.FromResources(Shortcuts.ProjectName, Path.Combine(this.BookFolder(), bookName), asBytes: true);
                QcUnity.RefreshAssetDatabase();
            }
            else 
                QcFile.Delete.FromPersistentFolder(this.BookFolder(), bookName, asBytes: true);
        }

        public void TryRename(string newName) {

            if (subNode.name.SameAs(newName))
                return;

            if (newName.Length < 3) {
                Debug.LogError("Name is too short");
                return;
            }

            if (Shortcuts.books.all.GetByIGotName(newName) != null) {
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