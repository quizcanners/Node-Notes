using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using PlaytimePainter;
using QuizCannersUtilities;
using RayMarching;
using UnityEngine;
using UnityEngine.Networking;

namespace NodeNotes_Visual
{
    
#pragma warning disable IDE0018 // Inline variable declaration

    [ExecuteInEditMode]
    public class NodesVisualLayer : NodesVisualLayerAbstract {

        public static NodesVisualLayer Instance => instLogicMgmt as NodesVisualLayer;

        [SerializeField] protected Camera mainCam;

        [SerializeField] protected Canvas canvas;

        public static Camera MainCam
        {
            get
            {
                if (!Instance.mainCam)
                    Instance.mainCam = Camera.main;
                return Instance.mainCam;
            }
        }

        public TextureDownloadManager textureDownloader = new TextureDownloadManager();

        #region Presentation Modes

        private List<ILinkedLerping> modesAsLinkedLeprs = new List<ILinkedLerping>();

        public List<PresentationMode> presentationControllers = new List<PresentationMode>();

        public static PresentationMode _selectedController;

        public static PresentationMode SelectedPresentationMode
        {
            get
            {
                if (!_selectedController)
                    SetPresentationMode(null);

                return _selectedController;
            }

            set { _selectedController = value; }
        }

        public static void SetPresentationMode(Node source)
        {

            var bgc = Instance.presentationControllers;

            if (source == null)
            {

                foreach (var bc in bgc)
                    if (bc) bc.FadeAway();

                return;
            }

            var tag = source?.visualStyleTag;

            if (tag.IsNullOrEmpty() && bgc.Count > 0)
                tag = bgc[0].ClassTag;

            string data = "";

            if (source != null)
                source.visualStyleConfigs.TryGetValue(tag, out data);

            foreach (var bc in bgc)
                if (bc && bc.ClassTag != tag)
                    bc.FadeAway();

            foreach (var bc in bgc)
                if (bc && bc.ClassTag == tag)
                {
                    _selectedController = bc;
                    bc.TryFadeIn();
                    data.TryDecodeInto(bc);
                    break;
                }
        }

        public override void HideAllBackgrounds()
        {
            foreach (var bc in Instance.presentationControllers)
                bc.FadeAway();
        }

        #endregion

        #region Presentation Systems

        private List<ILinkedLerping> systemAsLinkedLeprs = new List<ILinkedLerping>();

        [SerializeField] protected List<PresentationSystemsAbstract> presentationSystems;

        public Dictionary<string, PresentationSystemConfigurations> presentationSystemPerNodeConfigs = new Dictionary<string, PresentationSystemConfigurations>();

        #endregion

        #region Game Nodes

        public List<GameControllerBase> gameNodeControllers = new List<GameControllerBase>();

        #endregion

        #region Node MGMT

        public override void OnNodeDelete(int index)
        {
            foreach (var config in presentationSystemPerNodeConfigs)
                config.Value.OnNodeDelete(index);
        }

        public override void Show(Base_Node node) => SelectedPresentationMode.MakeVisible(node);
        
        public override void Hide(Base_Node node) => SelectedPresentationMode.MakeHidden(node);

        private readonly LoopLock _setNodeLoopLock = new LoopLock();

        public override Node CurrentNode => Shortcuts.CurrentNode; 
        
        public override void OnBeforeNodeSet(Node node) {

            SetPresentationMode(node);
            
            if (Application.isPlaying)
                node?.SetInspected();

            if (!SelectedPresentationMode)
            {
                if (CurrentNode != null)
                    Debug.LogError("Selected Visual Layer is null");
            }
            else 
                SelectedPresentationMode.OnBeforeNodeSet(node);

            foreach (var system in presentationSystems)
            {
                if (system)
                {
                    PresentationSystemConfigurations cfg;
                    if (presentationSystemPerNodeConfigs.TryGetValue(system.ClassTag, out cfg))
                        system.Decode(cfg.GetConfigFor(node));
                    else 
                        system.Decode("");
                }
            }

            // BOTCHING:
            var rtx = RayRenderingManager.instance;
            rtx.playLerpAnimation = false;
        }

        public override void OnLogicVersionChange() {

            if (gameNode == null) {
                var cn = Shortcuts.CurrentNode;
                SetPresentationMode(cn);
                SelectedPresentationMode.OnLogicUpdate();
            }
        }

        #endregion

        #region Inspector

        pegi.GameView.Window _playtimeInspectorWindowOnGui = new pegi.GameView.Window();

        protected override void ResetInspector()
        {
            shortcuts?.ResetInspector();
            base.ResetInspector();
        }

        private enum InspectedItem { Triggers = 1, CurrentNode = 2, Books = 4, Users = 5}
        
        private int _inspectedItems = -1;
        protected bool InspectionTabs()
        {
            var cn = Shortcuts.CurrentNode;

            if (cn != null) {
                icon.State.toggle("[{0}] Current: {1} - {2}".
                    F(Shortcuts.users.current.bookMarks.Count, cn.parentBook.GetNameForInspector(), cn.GetNameForInspector()), ref _inspectedItems, (int)InspectedItem.CurrentNode);
            }
            else icon.InActive.write("No Active Node");

            icon.Book.toggle("Node Books", ref _inspectedItems, (int)InspectedItem.Books);

            icon.User.toggle("Users", ref _inspectedItems, (int)InspectedItem.Users);

            icon.Condition.toggle("Trigger groups", ref _inspectedItems, (int)InspectedItem.Triggers);

            icon.Debug.toggle("Close All", ref _inspectedItems, -1);

            return false;
        }

        private int _inspectedBackground = -1;
        private int _inspectedDebugItem = -1;
        private int _inspectedSingleton = -1;
        private int _inspectedNodeStuff = -1;
        private int _inspectedPresSysCfg = -1;


        public override bool Inspect() {

            bool changed = false;

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

            InspectionTabs().nl();


            if (_inspectedItems == (int) InspectedItem.Triggers)
                base.Inspect().nl(ref changed);
            

            if (_inspectedItems == (int) InspectedItem.CurrentNode)
            {

                var source = CurrentNode;
                
                if (pegi.conditional_enter(source != null, ref _inspectedNodeStuff, 0))
                {
                    if (source.visualRepresentation == null)
                    {
                        "Node is not currently visualized".write();

                        if ("Force Show".Click("Hidden. Click to show visual representation."))
                        {
                            Shortcuts.visualLayer.Show(source);
                        }
                    }
                    else if ("Force Hide".Click("Visible. Click To Hide Visual Representation."))
                        Shortcuts.visualLayer.Hide(source);

                    pegi.nl();

                    pegi.Try_Nested_Inspect(source.visualRepresentation).nl(ref changed);
                } else 

                if (CurrentNode != null)
                {
                    CurrentNode.NameForPEGI.write(PEGI_Styles.ListLabel);

                    if (_inspectedNodeStuff != -1)
                        pegi.nl();
                }

                pegi.nl();
                
                if (pegi.conditional_enter(SelectedPresentationMode, ref _inspectedNodeStuff, 1))
                    SelectedPresentationMode.Nested_Inspect().changes(ref changed);

                if (CurrentNode != null)
                {
                    if (_inspectedNodeStuff == -1)
                    {
                        "Tree Mode".write(100);
                        if (PresentationMode.all.selectTypeTag(ref CurrentNode.visualStyleTag).changes(ref changed))
                            SetPresentationMode(CurrentNode);
                    }
                }

                pegi.nl();

                int index = 2;

                foreach (var system in presentationSystems)
                {
                    if (system.GetNameForInspector_Uobj().enter(ref _inspectedNodeStuff, index).nl())
                    {
                        PresentationSystemConfigurations cfg;
                        if (!presentationSystemPerNodeConfigs.TryGetValue(system.ClassTag, out cfg))
                        {
                            if ("Add {0} configs".F(system.GetNameForInspector()).Click().nl(ref changed))
                            {
                                cfg = new PresentationSystemConfigurations();
                                presentationSystemPerNodeConfigs.Add(system.ClassTag, cfg);
                            }
                        }
                        else
                            cfg.InspectFor(CurrentNode.AsNode, system).nl(ref changed);
                        
                    }

                    index++;
                }                
            }

            pegi.nl();

            if (!shortcuts)
                "Shortcuts".edit(ref shortcuts).nl(ref changed);
            else if (_inspectedItems == (int)InspectedItem.Books)
                shortcuts.Nested_Inspect().nl(ref changed);

            if (_inspectedItems == (int)InspectedItem.Users)
                Shortcuts.users.Nested_Inspect(ref changed);

            if (_inspectedItems == -1)
            {
              

                "Presentation Modes [For Node Tree]"
                    .enter_List_UObj(ref presentationControllers, ref _inspectedBackground, ref _inspectedDebugItem, 0)
                    .nl(ref changed);

                "Game Controllers".enter_List_UObj(ref gameNodeControllers, ref _inspectedDebugItem, 1).nl(ref changed);

                "Presentation Systems".enter_List_UObj(ref presentationSystems, ref _inspectedSingleton, ref _inspectedDebugItem, 2).nl(ref changed);

                "Textures".enter_Inspect(textureDownloader, ref _inspectedDebugItem, 3).nl_ifNotEntered(ref changed);

                if ("Assets".enter(ref _inspectedDebugItem, 4).nl())
                    Shortcuts.Instance.InspectAssets().nl();

                if ("Test Web Requests".enter(ref _inspectedDebugItem, 5).nl())
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

                if ("Presentation System Cfgs".enter(ref _inspectedDebugItem, 6).nl())
                {
                    "Cfgs".edit_Dictionary_Values(presentationSystemPerNodeConfigs, ref _inspectedPresSysCfg).nl();
                }
                
                if (_inspectedDebugItem == -1)
                {
                    pegi.toggleDefaultInspector(this).nl();

                    "Lerp by {0}, portion: {1}".F(_ld.dominantParameter, _ld.MinPortion).nl();    

                    "Playtime UI".toggleIcon(ref Shortcuts.showPlaytimeUI).nl();

                    if ("Encode / Decode Test".Click(ref changed))
                    {
                        OnDisable();
                        OnEnable();
                    }

                    pegi.nl();
                    
                }
            }

            if (changed && SelectedPresentationMode)
                SelectedPresentationMode.OnLogicUpdate();

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

        #region Encode & Decode

        public override CfgEncoder EncodePerBookData()
        {
            /*EncodePresentationSystem(NodeNotesGradientController.instance);
            EncodePresentationSystem(AmbientSoundsMixerMgmt.instance);
            EncodePresentationSystem(RayRenderingManager.instance);*/

            var cody = new CfgEncoder()
                .Add("gSys2", presentationSystemPerNodeConfigs);
                //.Add_IfNotEmpty("gSys", presentationSystemsConfigs);

            return cody;

        }

        public bool Decode(string tg, string data)
        {
            switch (tg)
            {

                case "gSys2": data.Decode_Dictionary(out presentationSystemPerNodeConfigs); break;
                // DEPRECATED (TMP)
                case "gSys":
                    var dicTmp = new Dictionary<string, string>();
                    data.Decode_Dictionary(out dicTmp);
                    foreach (var pair in dicTmp)
                    {
                        var cfg = new PresentationSystemConfigurations();
                        pair.Value.DecodeInto(out cfg.perNodeConfigs);
                        presentationSystemPerNodeConfigs[pair.Key] = cfg;
                    }
                    break;
                default: return false;
            }

            return true;
        }

        public override void Decode(string data)
        {
            presentationSystemPerNodeConfigs.Clear();
            //presentationSystemsConfigs.Clear();
            new CfgDecoder(data).DecodeTagsFor(Decode);
           /* DECODEPresentationSystem(NodeNotesGradientController.instance);
            DECODEPresentationSystem(AmbientSoundsMixerMgmt.instance);
            DECODEPresentationSystem(RayRenderingManager.instance);*/

        }

        #endregion

        #region Linked Lerp
        private LerpData _ld = new LerpData();

        protected override void DerivedUpdate() {

            base.DerivedUpdate();

            if (Input.GetKey(KeyCode.Escape)) {
                OnDisable();
                Application.Quit();
                Debug.Log("Quit click");
            }

            _ld.Reset();

            modesAsLinkedLeprs.Portion(_ld);
            systemAsLinkedLeprs.Portion(_ld);

            modesAsLinkedLeprs.Lerp(_ld);
            systemAsLinkedLeprs.Lerp(_ld);
        }
        #endregion

        protected override void OnDisable() {

            base.OnDisable();

            foreach (var bg in presentationControllers)
                bg.ManagedOnDeInitialize();

            textureDownloader.Dispose();

        }

        public override void OnEnable() {

            foreach (var gc in gameNodeControllers)
                if (gc) gc.Initialize();

            systemAsLinkedLeprs.Clear();
            foreach (var script in presentationSystems)
            {
                script.ManagedOnEnable();

                var asLerp = script as ILinkedLerping;
                if (asLerp!= null)
                    systemAsLinkedLeprs.Add(asLerp);
            }
            
            Shortcuts.visualLayer = this;

            base.OnEnable();

            modesAsLinkedLeprs.Clear();
            foreach (var bg in presentationControllers)
            {
                bg.ManagedOnInitialize();

                var asLerp = bg as ILinkedLerping;
                if (asLerp != null)
                    modesAsLinkedLeprs.Add(asLerp);
            }

        }


    }




}
