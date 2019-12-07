using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using UnityEngine.UI;
using TMPro;
using NodeNotes;
using PlaytimePainter;
using Unity.Entities;
using Object = UnityEngine.Object;


namespace NodeNotes_Visual
{



#pragma warning disable IDE0018 // Inline variable declaration

    [ExecuteInEditMode]
    public class NodesVisualLayer : NodesVisualLayerAbstract {

        public static NodesVisualLayer Instance => inst as NodesVisualLayer;

        [SerializeField] protected Camera mainCam;

        [SerializeField] protected List<NodeNodesNeedEnableAbstract> forManagedOnEnable;

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
        
        public override void OnNodeSet(Node node) {

            SetBackground(node);

            if (Application.isPlaying)
                node?.SetInspected();

            SelectedVisualLayer.SetNode(node);
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

        public List<BackgroundBase> backgroundControllers = new List<BackgroundBase>();

        public static BackgroundBase _selectedController;

        public static BackgroundBase SelectedVisualLayer {
            get {
                if (!_selectedController)
                    SetBackground(null);

                return _selectedController;
            }

            set { _selectedController = value; }
        }

        public static void SetBackground(Node source)
        {

            var bgc = Instance.backgroundControllers;
            
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
            foreach (var bc in Instance.backgroundControllers)
                bc.FadeAway();
        }

        #endregion

        #region Inspector

        pegi.WindowPositionData_PEGI_GUI window = new pegi.WindowPositionData_PEGI_GUI();

        protected override void ResetInspector()
        {
            shortcuts?.ResetInspector();
            base.ResetInspector();
        }

        protected override void InspectionTabs()
        {
            var cn = Shortcuts.CurrentNode;

            if (cn != null) {
                icon.Active.toggle("{0} -> [{1}] Current: {2} - {3}"
                    .F(Shortcuts.user.startingPoint, Shortcuts.user.bookMarks.Count, cn.parentBook.GetNameForInspector(), cn.GetNameForInspector())
                    , ref inspectedItems, 2);
            }
            else icon.InActive.write("No Active Node");

            icon.Book.toggle("Node Books", ref inspectedItems, 4);

            base.InspectionTabs();
        }

        private int _inspectedDependency = -1;
        private int _inspectedBackground = -1;  

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
            
            if (inspectedItems == 2)
                SelectedVisualLayer.Nested_Inspect().changes(ref changed);

            var cn = Shortcuts.CurrentNode;
            
            pegi.nl();

            if (!shortcuts)
                "Shortcuts".edit(ref shortcuts).nl(ref changed);
            else
               if (inspectedItems == 4)
                shortcuts.Nested_Inspect().nl(ref changed);
            else
                pegi.nl();

            if ("Dependencies".enter(ref inspectedItems, 5).nl()) {

                "Backgrounds".enter_List_UObj(ref backgroundControllers, ref _inspectedBackground, ref _inspectedDependency, 0).nl(ref changed);

                "Game Controllers".enter_List_UObj(ref gameNodeControllers, ref _inspectedDependency, 1).nl(ref changed);

            }

            "Textures".enter_Inspect(textureDownloader, ref inspectedItems, 6).nl_ifNotEntered(ref changed);

            if ("Assets".enter(ref inspectedItems, 7).nl())
                Shortcuts.Instance.InspectAssets().nl();


            if (inspectedItems == -1)
            {
                "Playtime UI".toggleIcon(ref Shortcuts.showPlaytimeUI).nl();

                if ( "Encode / Decode Test".Click(ref changed)) {
                    OnDisable();
                    OnEnable();
                }
            }

            if (changed && SelectedVisualLayer)
                SelectedVisualLayer.OnLogicUpdate();

            return changed;
        }

        public void OnGUI() {

            if (!Shortcuts.showPlaytimeUI || (Application.isPlaying && !Shortcuts.editingNodes))
                return;
            
            window.Render(this);
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

            foreach (var bg in backgroundControllers)
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
            
            foreach (var bg in backgroundControllers)
                bg.ManagedOnInitialize();

        }

        public override bool InspectBackgroundTag(Node node) {

            var changed = false;

            if (BackgroundBase.all.selectTypeTag(ref node.visualStyleTag).nl(ref changed))
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
            activeList = new ListMetaData(name, true, true, allowCreating: false);

        }

        public int Count => active.Count;
    }

    public abstract class NodeNodesNeedEnableAbstract : MonoBehaviour
    {
        public abstract void ManagedOnEnable();
    }

}
