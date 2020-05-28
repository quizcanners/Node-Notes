using System;
using System.Collections.Generic;
using System.IO;
using NodeNotes_Visual;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using static NodeNotes.NodeNotesAssetGroups;

namespace NodeNotes {


#pragma warning disable IDE0018 // Inline variable declaration

    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Story Nodes/Shortcuts", order = 0)]
    public class Shortcuts : CfgReferencesHolder {

        public const string ProjectName = "NodeNotes";

        public static bool editingNodes = false;

        public static UsersData users = new UsersData();

        public static NodeStoryBooks books = new NodeStoryBooks();
        
        #region Assets

        public static Shortcuts Instance => NodesVisualLayerAbstract.InstAsNodesVisualLayer.shortcuts;

        [SerializeField] public List<NodeNotesAssetGroups> assetGroups;
        
        // MESHES
        [SerializeField] protected Mesh _defaultMesh;
        public Mesh GetMesh(string name) => _defaultMesh;
        
        // MATERIALS
        [Serializable]
        private class FilteredMaterials : FilteredAssetGroup<Material>
        {
            public FilteredMaterials(Func<NodeNotesAssetGroups, TaggedAssetsList<Material>> getOne) : base(getOne) { }
        }

        [SerializeField] private FilteredMaterials Materials = new FilteredMaterials((NodeNotesAssetGroups grp) => grp.materials);
        public bool Get(string key, out Material mat)
        {
            mat = Materials.Get(key, assetGroups);
            return mat;
        }
        
        // SDF Objects
        [Serializable]
        public class FilteredSdfObjects : FilteredAssetGroup<SDFobject>
        {
            public FilteredSdfObjects(Func<NodeNotesAssetGroups, TaggedAssetsList<SDFobject>> getOne) : base(getOne) { }
        }

        public bool Get(string key, out SDFobject sdf)
        {
            sdf = SdfObjects.Get(key, assetGroups);
            return sdf;
        }

        public List<string> GetSdfObjectsKeys() => SdfObjects.GetAllKeysKeys(assetGroups);

        private FilteredSdfObjects SdfObjects = new FilteredSdfObjects((NodeNotesAssetGroups grp) => grp.sdfObjects);

        // Audio Clips
        [Serializable]
        public class FilteredAudioClips : FilteredAssetGroup<AudioClip> {
            public FilteredAudioClips(Func<NodeNotesAssetGroups, TaggedAssetsList<AudioClip>> getOne) : base(getOne) { }
        }

        private FilteredAudioClips AudioClips = new FilteredAudioClips((NodeNotesAssetGroups grp) => grp.audioClips);

        public bool Get(string key, out AudioClip sdf) {
            sdf = AudioClips.Get(key, assetGroups);
            return sdf;
        }

        public List<string> GetAudioClipObjectsKeys() => AudioClips.GetAllKeysKeys(assetGroups);


        private void RefreshAssetGroups()
        {
            AudioClips.Refresh();
            SdfObjects.Refresh();
            Materials.Refresh();
        }


        [Serializable]
        public abstract class FilteredAssetGroup<T> where T: UnityEngine.Object
        {
            [SerializeField] protected T _defaultMaterial;
            [NonSerialized] private Dictionary<string, T> filteredOnjects = new Dictionary<string, T>();
            [NonSerialized] private List<string> allObjectKeys;

            public void Refresh()
            {
                filteredOnjects.Clear();
                allObjectKeys = null;
            }

            private readonly Func<NodeNotesAssetGroups, TaggedAssetsList<T>> _getOne;

            public List<string> GetAllKeysKeys(List<NodeNotesAssetGroups> assetGroups)
            {
                if (allObjectKeys != null)
                    return allObjectKeys;

                allObjectKeys = new List<string>();

                foreach (var assetGroup in assetGroups)
                foreach (var taggedObject in _getOne(assetGroup).taggedList)//assetGroup.materials.taggedList)
                    allObjectKeys.Add(taggedObject.tag);

                return allObjectKeys;
            }

            public T Get(string key, List<NodeNotesAssetGroups> assetGroups)
            {
                if (key.IsNullOrEmpty())
                    return _defaultMaterial;

                T mat;

                if (!filteredOnjects.TryGetValue(key, out mat))
                    foreach (var group in assetGroups)
                        if (_getOne(group).TreGet(key, out mat))
                        {
                            filteredOnjects[key] = mat;
                            break;
                        }

                return mat ? mat : _defaultMaterial;
            }

            public FilteredAssetGroup(Func<NodeNotesAssetGroups, TaggedAssetsList<T>> getOne)
            {
                _getOne = getOne;
            }

        }
        

        [SerializeField] public AudioClip onMouseDownButtonSound;
        [SerializeField] public AudioClip onMouseClickSound;
        [SerializeField] public AudioClip onMouseClickFailedSound;
        [SerializeField] public AudioClip onMouseLeaveSound;
        [SerializeField] public AudioClip onMouseHoldSound;
        [SerializeField] public AudioClip onSwipeSound;

        #endregion

        #region Progress

        static LoopLock loopLock = new LoopLock();

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

        static bool expectingLoopCall;

        public static NodesVisualLayerAbstract visualLayer;

        [NonSerialized] public static Base_Node Cut_Paste;

        #endregion
        
        #region Inspector
        
        public static bool showPlaytimeUI;
        
        public override void ResetInspector() {
            base.ResetInspector();
            users.ResetInspector();
            books.ResetInspector();         
        }

        private int _inspectedGroup;
        public bool InspectAssets()
        {
            var changed = false;

            if ("Refresh".Click().nl())
                RefreshAssetGroups();

            "Asset Groups".edit_List_SO(ref assetGroups, ref _inspectedGroup).nl(ref changed);

            if (changed)
            {
               RefreshAssetGroups();
                this.SetToDirty();
            }

            return changed;
        }

        private enum InspectedItems { InspectUser = 4, editUser = 6  }

        public override bool Inspect() {

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

                        var authorFolders = QcFile.Explorer.GetFolderNamesFromPersistentFolder(NodeBook_Base.BooksRootFolder);

                        foreach (var authorFolder in authorFolders) {

                            var fls = QcFile.Explorer.GetFileNamesFromPersistentFolder(Path.Combine(NodeBook_Base.BooksRootFolder, authorFolder));

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

            QcFile.Save.ToPersistentPath(_generalItemsFolder, _generalItemsFile, Encode().ToString());
        }

        public override CfgEncoder Encode()
        {
            return this.EncodeUnrecognized()
                //.Add("trigs", TriggerGroup.all)
                .Add("bkSrv", books)
                .Add_IfTrue("ptUI", showPlaytimeUI)
                .Add("us", users.all)
                .Add_String("curUser", users.current.Name);
        }

        public override bool Decode(string tg, string data)
        {
            switch (tg)  {
                //case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "bkSrv": books.Decode(data); break;
                case "ptUI": showPlaytimeUI = data.ToBool(); break;
                case "us": data.Decode_List(out users.all); break;
                case "curUser": users.LoadUser(data); break;
                default: return false;
            }
            return true;
        }

        #endregion
    }
}
