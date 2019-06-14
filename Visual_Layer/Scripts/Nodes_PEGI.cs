using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using UnityEngine.UI;
using TMPro;
using NodeNotes;


namespace NodeNotes_Visual
{

    [ExecuteInEditMode]
    public class Nodes_PEGI : NodesVisualLayerAbstract {

        public static Nodes_PEGI nodeMgmtInstPegi;
        
        public Shortcuts shortcuts;

        public QcUtils.TextureDownloadManager textureDownloader = new QcUtils.TextureDownloadManager();

        #region UI_Buttons 
        public TextMeshProUGUI editButton;

        public Button addButton;

        public NodeCircleController circlePrefab;

        public Button deleteButton;
        #endregion
        
        #region Click Events
        public void RightTopButton()
        {
            selectedNode = null;
            Base_Node.editingNodes = !Base_Node.editingNodes;
            if (editButton)
                editButton.text = Base_Node.editingNodes ? "Play" : "Edit";

            CreateNodeButton.showCreateButtons = false;

            if (addButton)
                addButton.gameObject.SetActive(Base_Node.editingNodes);
            if (deleteButton)
                deleteButton.gameObject.SetActive(false);

            AddLogicVersion();
        }

        public void ToggleShowAddButtons() {
            AddLogicVersion();
            CreateNodeButton.showCreateButtons = !CreateNodeButton.showCreateButtons;
        }

        public void AddNode() => MakeVisible(Shortcuts.CurrentNode.Add<Node>());

        public void AddLink() => MakeVisible(Shortcuts.CurrentNode.Add<NodeLinkComponent>());

        public void AddButton() => MakeVisible(Shortcuts.CurrentNode.Add<NodeButtonComponent>());

        public void DeleteSelected()
        {

            if (!selectedNode) return;

            var node = selectedNode.source;

            if (node.parentNode == null) return;
            
            selectedNode.Unlink();
            node.Delete();
            SetSelected(null);
                
            
        }
        #endregion

        #region BG
        public List<NodesStyleBase> backgroundControllers = new List<NodesStyleBase>();
        public static void SetBackground (NodeCircleController circle) {

            var data = circle ? circle.backgroundConfig : "";
            var tag = circle ? circle.background : "";

            var bgc = nodeMgmtInstPegi.backgroundControllers;

            if (tag.Length == 0 && bgc.Count > 0)
                tag = bgc[0].ClassTag;

            
            foreach (var bc in bgc)
                if (bc != null) {
                    if (bc.ClassTag == tag) {
                        bc.TryFadeIn();
                        data.TryDecodeInto(bc);
                    }
                    else
                        bc.FadeAway();
                }
        }
        
        #endregion

        #region Node MGMT

        private static readonly List<NodeCircleController> NodesPool = new List<NodeCircleController>();
        private static int _firstFree = 0;

        public static void Deactivate(NodeCircleController n) {
            n.gameObject.SetActive(false);
            _firstFree = Mathf.Min(_firstFree, n.IndexForPEGI);
       }

        private static void Delete (NodeCircleController ctr) {
            var ind = NodesPool.IndexOf(ctr);
            NodesPool.RemoveAt(ind);
            _firstFree = Mathf.Min(_firstFree, ind);
            ctr.gameObject.DestroyWhatever();
        }

        private static void DeleteAllNodes() {
            foreach (var e in NodesPool)
                if (e)
                {
                    if (Application.isPlaying)
                        e.isFading = true;
                    else
                        e.gameObject.DestroyWhatever();

                }

            NodesPool.Clear();
        }
        
        public override void Show(Base_Node node) => MakeVisible(node);

        private static void MakeVisible(Base_Node node, NodeCircleController centerNode = null) {

            NodeCircleController nnp = null;

            if (!centerNode) 
                centerNode = node.parentNode?.visualRepresentation as NodeCircleController;

            var reusing = false;

            if (node.previousVisualRepresentation != null) {
                var tmp = node.previousVisualRepresentation as NodeCircleController;
                if (tmp && tmp.isFading && node == tmp.myLastLinkedNode) {
                    nnp = tmp;
                    if (tmp.gameObject.activeSelf) {
                        reusing = true;
                        Debug.Log("Reusing previous for {0}".F(node.GetNameForInspector()));
                    }
                }
            }

            if (!nnp) {
                while (_firstFree < NodesPool.Count) {
                    var np = NodesPool[_firstFree];

                    if (!np.gameObject.activeSelf) {
                        nnp = np;
                        break;
                    }
                    else _firstFree++;
                }
            }

            if (!nnp) {
                if (!nodeMgmtInstPegi.circlePrefab)
                    return;

                nnp = Instantiate(nodeMgmtInstPegi.circlePrefab);
                nnp.IndexForPEGI = NodesPool.Count;
                NodesPool.Add(nnp);

                Debug.Log("Creating new for {0}".F(node.GetNameForInspector()));
            }

            nnp.LinkTo(node);

            if (!reusing)
                nnp.SetStartPositionOn(centerNode);
            
        }

        public override void Hide(Base_Node node) => MakeHidden(node);

        private static void MakeHidden(Base_Node node, NodeCircleController previous = null)
        {
            var ncc = node.visualRepresentation as NodeCircleController;
            if (!ncc) return;
            
            ncc.Unlink();
        
            if (!Application.isPlaying)
                Delete(ncc);
            else
                ncc.SetFadeAwayRelation(previous);

        }

        private readonly LoopLock _loopLock = new LoopLock();

        public override Node CurrentNode
        {

            get
            {
                return Shortcuts.CurrentNode;
            }

            set
            {

                if (!_loopLock.Unlocked) return;
                
                using (_loopLock.Lock()) {

                    if (Application.isPlaying && Shortcuts.CurrentNode != value) {

                        if (value == null)
                        {
                           // Debug.LogError("{0} is not a Node GetBrushType: {1}".F(value.GetNameForInspector(),
                             //   value == null ? "Null" : value.GetType().ToPegiStringType()));

                            return;
                        }

                        SetSelected(null);
                        
                        var previous = Shortcuts.CurrentNode?.visualRepresentation as NodeCircleController;

                        Shortcuts.CurrentNode = value;

                        UpdateVisibility(value, previous);

                        foreach (var n in NodesPool)
                            UpdateVisibility(n.source, previous);
                 
                        UpdateCurrentNodeGroupVisibilityAround(previous);

                        _firstFree = 0;

                    }
                    else
                        Shortcuts.CurrentNode = value;
                }
                

            }
        }

        private static void UpdateVisibility(Base_Node node, NodeCircleController previous = null)
        {

            if (node == null) return;

            var shouldBeVisible = (Base_Node.editingNodes || 
                ((node.parentNode != null && node.Conditions_isVisible()) || !Application.isPlaying))
                && (Shortcuts.CurrentNode != null 
                && (node == Shortcuts.CurrentNode || Shortcuts.CurrentNode.Contains(node)));

            if (node.visualRepresentation == null)  {
                if (shouldBeVisible)
                    MakeVisible(node, previous);
            } else 
                if (!shouldBeVisible)
                    MakeHidden(node, previous);

            if (node.visualRepresentation != null)
                (node.visualRepresentation as NodeCircleController).SetDirty();

            
        }

        private static void UpdateCurrentNodeGroupVisibilityAround(NodeCircleController centerNode = null) {
            var cn = Shortcuts.CurrentNode;
            
            SetBackground(cn?.visualRepresentation as NodeCircleController);

            if (!Application.isPlaying) return;

            if (cn == null) return;
                
            UpdateVisibility(cn, centerNode);
                foreach (var sub in cn)
                    UpdateVisibility(sub, centerNode);
                
            
        }

        public override void UpdateVisibility() => UpdateCurrentNodeGroupVisibilityAround();

        public NodeCircleController selectedNode;

        public void SetSelected(NodeCircleController node)
        {
            if (selectedNode)
                selectedNode.SetDirty();

            selectedNode = node;

            if (node)
                node.SetDirty();

            if (deleteButton)
                deleteButton.gameObject.SetActive(selectedNode);
        }


        #endregion

        #region Inspector
        #if !NO_PEGI
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
                    .F(Shortcuts.user.startingPoint, Shortcuts.user.bookMarks.Count, cn.root.GetNameForInspector(), cn.GetNameForInspector())
                    , ref inspectedItems, 2);
            }
            else icon.InActive.write("No Active Node");

            icon.Book.toggle("Node Books", ref inspectedItems, 4);

            base.InspectionTabs();
        }

        public override bool Inspect() {

            if (gameNode != null) {

                if (icon.Save.Click("Exit Game Node & Save")) {
                    FromGameToNode();
                    return true;
                }

                "GN: {0}".F(gameNode.GetNameForInspector()).write();

                if (icon.Close.Click("Exit Game Node in Fail").nl())
                    FromGameToNode(true);
                else return gameNode.Nested_Inspect();
            }

            if (Application.isPlaying && selectedNode) {
                if (selectedNode.Nested_Inspect()) {
                    UpdateVisibility();
                    return true;
                }
                return false;
            }

            bool changed = base.Inspect();

            var cn = Shortcuts.CurrentNode;

            if (cn!= null && inspectedItems ==2)
                cn.Nested_Inspect(ref changed);

            pegi.nl();

                if (!shortcuts)
                    "Shortcuts".edit(ref shortcuts).nl(ref changed);
                else
                   if (inspectedItems == 4)
                    shortcuts.Nested_Inspect().changes(ref changed);
                else
                    pegi.nl();

            if (icon.Create.enter("Dependencies", ref inspectedItems, 5)) {
                pegi.nl();
                "Edit Button".edit(90, ref editButton).nl(ref changed);
                "Add Button".edit(90, ref addButton).nl(ref changed);
                "Delete Button".edit(90, ref deleteButton).nl(ref changed);
                "Backgrounds".edit_Property(() => backgroundControllers, this).nl(ref changed);
                "Circles Prefab".edit(90, ref circlePrefab).nl(ref changed);

                "Nodes Pool: {0}; First Free: {1}".F(NodesPool.Count, _firstFree).nl();
            }

            pegi.nl();

            icon.Alpha.enter_Inspect("Textures", textureDownloader, ref inspectedItems, 6).nl_ifNotEntered(ref changed);

            if (inspectedItems == -1 && "Encode / Decode Test".Click(ref changed)) {
                OnDisable();
                OnEnable();
            }

            return changed;
        }

        public void OnGUI() {
            if (Application.isPlaying && !Base_Node.editingNodes)
                return;
              //  window.Render(selectedNode);
           // else 
                window.Render(this);
        }

        #endif
        #endregion

        readonly LerpData _lerpData = new LerpData();

        private int _logicVersion = -1;

        protected override void DerivedUpdate() {
            if (Input.GetKey(KeyCode.Escape)) {
                OnDisable();
                Application.Quit();
                Debug.Log("Quit click");
            }

            if (_logicVersion != currentLogicVersion)
            {
                UpdateVisibility();
                _logicVersion = currentLogicVersion;
            }

            _lerpData.Reset();

            NodesPool.Portion(_lerpData);

            NodesPool.Lerp(_lerpData);
        }

        private void OnDisable() {
            if (shortcuts)
                shortcuts.SaveAll();
            Shortcuts.CurrentNode = null;
            DeleteAllNodes();
            textureDownloader.Dispose();


        }

        public override void OnEnable()
        {
            Shortcuts.visualLayer = this;

            base.OnEnable();

            nodeMgmtInstPegi = this;

            DeleteAllNodes();

            shortcuts.LoadAll();

            editButton.text = "Edit";
            if (addButton)
                addButton.gameObject.SetActive(false);


        }
    }
}
