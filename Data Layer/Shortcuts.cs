using System;
using System.Collections.Generic;
using System.IO;
using NodeNotes_Visual;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using static NodeNotes.NodeNotesAssetGroup;

namespace NodeNotes {


#pragma warning disable IDE0018 // Inline variable declaration

    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Node Nodes/Shortcuts", order = 0)]
    public class Shortcuts : ScriptableObject, IPEGI, ICfg
    {
        
        public static UsersData users = new UsersData();

        public static NodeStoryBooks books = new NodeStoryBooks();
        
        public static NodesVisualLayerAbstract visualLayer;

        public static Shortcuts Instance => NodesVisualLayerAbstract.InstAsNodesVisualLayer.shortcuts;
        
        [SerializeField] NodeNotesAssetsMgmt _assets = new NodeNotesAssetsMgmt();
        public static NodeNotesAssetsMgmt Assets => NodesVisualLayerAbstract.InstAsNodesVisualLayer.shortcuts._assets;

        #region Progress

        public const string ProjectName = "Node-Notes";

        public static bool editingNodes = false;
        
        private static LoopLock loopLock = new LoopLock();
        
        static bool expectingLoopCall;

  
        public static Node CurrentNode
        {
            get { return users.current.CurrentNode; }

            set   {

                if (Application.isPlaying && visualLayer && loopLock.Unlocked)
                {
                    using (loopLock.Lock()) {
                        expectingLoopCall = true;

                        try
                        {
                            visualLayer.OnBeforeNodeSet(value);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex);
                        }

                        if (expectingLoopCall) {
                            expectingLoopCall = false;
                            users.current.FinalizeNodeChange(value);
                        }
                    }
                }
                else
                {
                    expectingLoopCall = false;
                    users.current.FinalizeNodeChange(value);
                }
            }
        }

        public static bool TryExitCurrentNode()
        {
            var node = CurrentNode;
            if (node != null && node.parentNode != null)
            {
                CurrentNode = node.parentNode;
                return true;
            }

            return false;
        }



        #endregion

        #region Inspector

        public static Base_Node Cut_Paste;

        public static bool showPlaytimeUI;
        
        public virtual void ResetInspector() {
            users.ResetInspector();
            books.ResetInspector();         
        }
        
        private enum InspectedItems { InspectUser = 4, editUser = 6  }

        private int inspectedItems = -1;

        public bool Inspect() {

            var changed = false;
            
            if (!users.current.isADeveloper)
                "Changes will not be saved as user is not a developer".writeWarning();

            if (books._inspectedBook == -1) {
                
                if (inspectedItems == -1)
                {
                    pegi.nl();

                    if (Application.isEditor) {

                        if (icon.Folder.Click("Open Save files folder"))
                            QcFile.Explorer.OpenPersistentFolder(NodeBook_Base.BooksRootFolder);

                        users.current.NameForPEGI.write();

                        pegi.nl();

                        var authorFolders = QcFile.Explorer
                            .GetFolderNamesFromPersistentFolder(NodeBook_Base.BooksRootFolder);

                        foreach (var authorFolder in authorFolders) {

                            var fls = QcFile.Explorer
                                .GetFileNamesFromPersistentFolder(Path.Combine(NodeBook_Base.BooksRootFolder, authorFolder));

                            foreach (var bookFile in fls) {

                                var loaded = books.TryGetBook(bookFile, authorFolder);

                                if (loaded == null) {
                                    "{0} by {1}".F(bookFile, authorFolder).write();

                                    if (icon.Insert.Click("Add current book to list"))
                                        books.all.Add(new NodeBook_OffLoaded(bookFile, authorFolder));

                                    pegi.nl();
                                }
                            }
                        }
                    }

                /*
                if (icon.Refresh.Click("Will populate list with mentions with books in Data folder without loading them")) {
                    
                    var lst = QcFile.ExplorerUtils.GetFileNamesFromPersistentFolder(NodeBook_Base.BooksRootFolder);

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

            if (inspectedItems == -1) 
                books.Nested_Inspect(ref changed);
            
            return changed;
        }

        #endregion

        #region Encode_Decode

        static readonly string _generalItemsFolder = "General";

        static readonly string _generalItemsFile = "config";

        public bool LoadAll()
        {
            var ok = this.LoadFromPersistentPath(_generalItemsFolder, _generalItemsFile);

            return ok;
        }

        public void SaveAll()
        {
            users.SaveCurrentToPersistentPath();

            if (CurrentNode != null)
                CurrentNode.parentBook.UpdatePerBookConfigs();

            for (int i = 0; i < books.all.Count; i++)
            {
                var book = books.all[i].AsLoadedBook;
                if (book != null && book.EditedByCurrentUser())
                    book.Offload();
            }

            List<TriggerGroup> trigs = TriggerGroup.all.GetAllObjsNoOrder();

            foreach (var gr in trigs)
            {
                gr.SaveToFile();
            }

            QcFile.Save.ToPersistentPath(_generalItemsFolder, _generalItemsFile, Encode().ToString(), asBytes: true);
        }

        public CfgEncoder Encode()
        {
            return new CfgEncoder() 
                .Add("bkSrv", books)
                .Add_IfTrue("ptUI", showPlaytimeUI)
                .Add("us", users.all)
                .Add_String("curUser", users.current.Name);
        }

        public void Decode(string tg, CfgData data)
        {
            switch (tg)  {
                case "bkSrv": books.DecodeFull(data); break;
                case "ptUI": showPlaytimeUI = data.ToBool(); break;
                case "us": data.ToList(out users.all); break;
                case "curUser": users.LoadUser(data.ToString()); break;
                
            }
        }
        


        #endregion
    }
}
