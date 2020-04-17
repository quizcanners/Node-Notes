using System;
using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using PlaytimePainter;
using QuizCannersUtilities;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace NodeNotes_Visual
{
    
#pragma warning disable IDE0018 // Inline variable declaration

    [ExecuteInEditMode]
    public class NodesVisualLayer : NodesVisualLayerAbstract {

        public static NodesVisualLayer Instance => inst as NodesVisualLayer;

        [SerializeField] protected Camera mainCam;

        [SerializeField] protected List<NodeNodesNeedEnableAbstract> forManagedOnEnable;

        [SerializeField] protected Canvas canvas;

        [SerializeField] protected PainterCamera painterCamera;


        public static Camera MainCam {
            get
            {
                if (!Instance.mainCam)
                    Instance.mainCam = Camera.main;
                return Instance.mainCam;
            }
        }

        public TextureDownloadManager textureDownloader = new TextureDownloadManager();

        #region Game Nodes

        public List<GameControllerBase> gameNodeControllers = new List<GameControllerBase>();

        #endregion

        #region Node MGMT

        public override void Show(Base_Node node) => SelectedVisualLayer.MakeVisible(node);
        
        public override void Hide(Base_Node node) => SelectedVisualLayer.MakeHidden(node);

        private readonly LoopLock _setNodeLoopLock = new LoopLock();

        public override Node CurrentNode => Shortcuts.CurrentNode; 
        
        public override void OnBeforeNodeSet(Node node) {

            SetBackground(node);

            if (Application.isPlaying)
                node?.SetInspected();

            if (!SelectedVisualLayer)
            {
                if (CurrentNode != null)
                    Debug.LogError("Selected Visual Layer is null");
            }
            else 
                SelectedVisualLayer.OnBeforeNodeSet(node);
        }

        public override void OnLogicVersionChange() {

            if (gameNode == null) {
                var cn = Shortcuts.CurrentNode;
                SetBackground(cn);
                SelectedVisualLayer.OnLogicUpdate();
            }
        }

        #endregion

        #region BG MGMT

        public List<PresentationMode> presentationControllers = new List<PresentationMode>();

        public static PresentationMode _selectedController;

        public static PresentationMode SelectedVisualLayer {
            get {
                if (!_selectedController)
                    SetBackground(null);

                return _selectedController;
            }

            set { _selectedController = value; }
        }

        public static void SetBackground(Node source)
        {

            var bgc = Instance.presentationControllers;
            
            if (source == null) {

                foreach (var bc in bgc)
                    if (bc) bc.FadeAway();

                return;
            }

            var tag = source?.visualStyleTag;

       
            if (tag.IsNullOrEmpty() && bgc.Count > 0)
                tag = bgc[0].ClassTag;

            string data = "";

            if (source!= null)
                source.visualStyleConfigs.TryGetValue(tag, out data);

            foreach (var bc in bgc)
                if (bc && bc.ClassTag != tag)
                        bc.FadeAway();
                
            foreach (var bc in bgc)
                if (bc && bc.ClassTag == tag) {
                    _selectedController = bc;
                    bc.TryFadeIn();
                    data.TryDecodeInto(bc);
                    break;
                }
        }

        public override void HideAllBackgrounds() {
            foreach (var bc in Instance.presentationControllers)
                bc.FadeAway();
        }

        #endregion

        #region Inspector

        pegi.GameView.Window _playtimeInspectorWindowOnGui = new pegi.GameView.Window();

        protected override void ResetInspector()
        {
            shortcuts?.ResetInspector();
            base.ResetInspector();
        }

        private enum InspectedItem { CurrentNode = 2, Books = 4, Users = 5}

        protected override void InspectionTabs()
        {
            var cn = Shortcuts.CurrentNode;

            if (cn != null) {
                icon.State.toggle("[{0}] Current: {1} - {2}".
                    F(Shortcuts.users.current.bookMarks.Count, cn.parentBook.GetNameForInspector(), cn.GetNameForInspector()), ref inspectedItems, (int)InspectedItem.CurrentNode);
            }
            else icon.InActive.write("No Active Node");

            icon.Book.toggle("Node Books", ref inspectedItems, (int)InspectedItem.Books);

            icon.User.toggle("Users", ref inspectedItems, (int)InspectedItem.Users);

            base.InspectionTabs();
        }
        
        private int _inspectedBackground = -1;
        private int _inspectedDebugItem = -1;

        public override bool Inspect() {

            if (gameNode != null) {

                if (icon.Save.Click("Exit Game Node & Save")) {
                    FromGameToNode();
                    return true;
                }

                "GN: {0}".F(gameNode.GetNameForInspector()).write();

                if (icon.Close.Click("Exit Game Node in Fail").nl())
                    FromGameToNode(true);
                else
                    return gameNode.Nested_Inspect();
            }

            bool changed = base.Inspect();
            
            if (inspectedItems == (int)InspectedItem.CurrentNode)
                SelectedVisualLayer.Nested_Inspect().changes(ref changed);
            
            pegi.nl();

            if (!shortcuts)
                "Shortcuts".edit(ref shortcuts).nl(ref changed);
            else if (inspectedItems == (int)InspectedItem.Books)
                shortcuts.Nested_Inspect().nl(ref changed);

            if (inspectedItems == (int)InspectedItem.Users)
                Shortcuts.users.Nested_Inspect(ref changed);

            if (inspectedItems == -1)
            {
                "Backgrounds"
                    .enter_List_UObj(ref presentationControllers, ref _inspectedBackground, ref _inspectedDebugItem, 0)
                    .nl(ref changed);

                "Game Controllers".enter_List_UObj(ref gameNodeControllers, ref _inspectedDebugItem, 1).nl(ref changed);

                "Textures".enter_Inspect(textureDownloader, ref _inspectedDebugItem, 2).nl_ifNotEntered(ref changed);

                if ("Assets".enter(ref _inspectedDebugItem, 3).nl())
                    Shortcuts.Instance.InspectAssets().nl();

                if ("Test Web Requests".enter(ref _inspectedDebugItem, 4).nl())
                {
                    if (testRequest == null)
                    {
                        "Test URL".edit(90, ref _testUrl).nl();
                        if ("Get".Click())
                        {
                            testRequest = UnityWebRequest.Get(_testUrl);
                            testRequest.SendWebRequest();
                        }
                    }
                    else
                    {

                        if ("Dispose Request".Click().nl())
                        {
                            testRequest.Abort();
                            testRequest.Dispose();
                            testRequest = null;
                        }
                        else
                        {

                            if (testRequest.isNetworkError)
                            {
                                "Error".nl(PEGI_Styles.ListLabel);
                                pegi.writeBig(testRequest.error);
                            }
                            else
                            {
                                if (!testRequest.isDone)
                                    "Progress: {0}".F(testRequest.downloadProgress).nl();
                                else
                                {
                                    "Done".write();

                                    if ("Read Content".Click().nl())
                                        _testDownloadedCode = testRequest.downloadHandler.text;

                                }
                            }
                        }
                    }

                    if (!_testDownloadedCode.IsNullOrEmpty())
                    {
                        if ("Clear".Click().nl())
                            _testDownloadedCode = null;
                        else
                            pegi.editBig(ref _testDownloadedCode);
                    }
                }


                if (_inspectedDebugItem == -1)
                {
                    "Playtime UI".toggleIcon(ref Shortcuts.showPlaytimeUI).nl();

                    if ("Encode / Decode Test".Click(ref changed))
                    {
                        OnDisable();
                        OnEnable();
                    }

                    pegi.nl();
                    
                }
            }

            if (changed && SelectedVisualLayer)
                SelectedVisualLayer.OnLogicUpdate();

            return changed;
        }

        private string _testDownloadedCode;

        private UnityWebRequest testRequest;

        private string _testUrl;

        public void OnGUI() {

            if (!Shortcuts.showPlaytimeUI || !Application.isPlaying || !Shortcuts.editingNodes)
                return;
            
            _playtimeInspectorWindowOnGui.Render(this);
        }
        
        #endregion

        protected override void DerivedUpdate() {

            base.DerivedUpdate();

            if (Input.GetKey(KeyCode.Escape)) {
                OnDisable();
                Application.Quit();
                Debug.Log("Quit click");
            }
        }

        protected override void OnDisable() {

            base.OnDisable();

            foreach (var bg in presentationControllers)
                bg.ManagedOnDeInitialize();

            textureDownloader.Dispose();

        }

        public override void OnEnable() {

            if (Application.isPlaying && painterCamera)
                painterCamera.OnEnable();

            foreach (var gc in gameNodeControllers)
                if (gc) gc.Initialize();

            foreach (var script in forManagedOnEnable)
                script.ManagedOnEnable();


            Shortcuts.visualLayer = this;

            base.OnEnable();
            
            shortcuts.Initialize();
            
            foreach (var bg in presentationControllers)
                bg.ManagedOnInitialize();

        }

        public override bool InspectBackgroundTag(Node node) {

            var changed = false;

            "Style (Inside)".write(110);

            if (PresentationMode.all.selectTypeTag(ref node.visualStyleTag).nl(ref changed))
                SetBackground(node);

            return changed;
        }
    }


    [Serializable]
    public class PoolSimple<T>: IPEGI where T : Component {

        private ListMetaData activeList;
        public List<T> active = new List<T>();
        public List<T> disabled = new List<T>();
        public T prefab;

        public IEnumerator<T> GetEnumerator() {
            foreach (var i in active)
                yield return i;
        }

        public void DeleteAll() {

            foreach (var el in active)
                el.gameObject.DestroyWhatever();

            foreach (var el in disabled)
                el.gameObject.DestroyWhatever();
                
            active.Clear();
            disabled.Clear();

        }

        public void Disable(T obj) {
            active.Remove(obj);
            obj.gameObject.SetActive(false);
            disabled.Add(obj);
        }

        public void Disable(bool disableFirst = true)
        {
            if (active.Count > 0)
            {
                if (disableFirst)
                    Disable(active[0]); 
                else
                    Disable(active[active.Count - 1]); 
            }
        }

        public T GetOne(Transform parent, bool insertFirst = false) {

            T toReturn;

            if (disabled.Count > 0) {
                 toReturn = disabled[0];
                 disabled.RemoveAt(0);
            }
            else
                toReturn = Object.Instantiate(prefab, parent);
            
            if (insertFirst)
                active.Insert(0, toReturn);
            else 
                active.Add(toReturn);

            toReturn.gameObject.SetActive(true);

            return toReturn;
        }

        public bool Inspect() {
            var changed = false;

            "Prefab".edit(ref prefab).nl(ref changed);
            
            "Inactive: {0};".F(disabled.Count).writeHint();
            
            activeList.edit_List_UObj(ref active).nl(ref changed);
                        
            return changed;
        }

        public PoolSimple (string name) {
            activeList = new ListMetaData(name, true, true, showAddButton: false);

        }

        public int Count => active.Count;
    }

    public abstract class NodeNodesNeedEnableAbstract : MonoBehaviour
    {
        public abstract void ManagedOnEnable();
    }

}
