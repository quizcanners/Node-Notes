using System;
using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using PlaytimePainter;
using QcTriggerLogic;
using QuizCannersUtilities;
using RayMarching;
using UnityEngine;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class BoxButtons : PresentationMode {

        public static BoxButtons inst;

        public const string classTag = "white";

        public override string ClassTag => classTag;
        
       // public Countless<string> perNodeRtxConfigs = new Countless<string>();
        
        public bool isFading;

        private bool _isRendering = true;

        protected bool IsRendering
        {
            set
            {
                _isRendering = value;

                foreach (var node in NodesPool)
                    node.IsRendering = value;
            }
            get { return _isRendering; }
        }
        
        #region Click Events

        public void RightTopButton()
        {
            selectedNode = null;

            Shortcuts.editingNodes = !Shortcuts.editingNodes;

            IsRendering = true;

            if (editButton)
                editButton.Text = Shortcuts.editingNodes ? "Play" : "Edit";

            if (addButton)
                addButton.gameObject.SetActive(Shortcuts.editingNodes);

            if (deleteButton)
                deleteButton.gameObject.SetActive(false);

            SetShowAddButtons(false);

            LevelArea.OnEditingNodesToggle();
        }

        public void ToggleShowAddButtons() => SetShowAddButtons(!CreateNodeButton.showCreateButtons);

        private void SetShowAddButtons(bool val)
        {
            CreateNodeButton.showCreateButtons = val;

            addButtonCourner.TargetValue = CreateNodeButton.showCreateButtons ? 1 : 0;

            LogicMGMT.AddLogicVersion();
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

        #region UI_Buttons 

        public List<CreateNodeButton> slidingButtons = new List<CreateNodeButton>();

        public RoundedButtonWithText editButton;

        public RoundedGraphic addButton;

        public NodeCircleController circlePrefab;

        public RoundedGraphic deleteButton;
        #endregion
        
        #region Node MGMT

        public override void OnBeforeNodeSet(Node node)
        {

            if (!_loopLock.Unlocked) return;

            using (_loopLock.Lock())
            {

                IsRendering = true;

                var previousN = Shortcuts.CurrentNode;

                if (Application.isPlaying && (previousN == null || previousN != node))
                {

                    if (node == null)
                        return;

                    SetSelected(null);

                    var previous = previousN?.visualRepresentation as NodeCircleController;

                    Shortcuts.CurrentNode = node;

                    UpdateVisibility(node, previous);

                    foreach (var n in NodesPool)
                        UpdateVisibility(n.source, previous);

                    UpdateCurrentNodeGroupVisibilityAround(node, previous);

                    _firstFree = 0;

                    NodeNotesGradientController.instance.LoadConfigFor(node);
                    AmbientSoundsMixerMgmt.instance.LoadConfigFor(node);
                    RayRenderingManager.instance.LoadConfigFor(node);

                    var rtx = RayRenderingManager.instance;

                    rtx.playLerpAnimation = false;

                    /*if (rtx)
                    {
                        Node iteration = node;
                        while (iteration != null)
                        {
                            var val = perNodeRtxConfigs[iteration.IndexForPEGI];
                            if (!val.IsNullOrEmpty())
                            {
                                rtx.Decode(val);
                              
                                break;
                            }
                            iteration = iteration.parentNode;
                        }
                    }*/

                }
                else
                    Shortcuts.CurrentNode = node;

                LevelArea.OnNodeChange();
            }

        }

        [NonSerialized] private readonly List<NodeCircleController> NodesPool = new List<NodeCircleController>();
        [NonSerialized] private int _firstFree;

        public void HideAll()
        {
            foreach (var node in NodesPool)
                if (!node.isFading)
                    MakeHidden(node.source);
        }

        public void Deactivate(NodeCircleController n)
        {
            if (isFading)
                Delete(n);
            else
            {
                n.gameObject.SetActive(false);
                _firstFree = Mathf.Min(_firstFree, n.IndexForPEGI);
            }
        }

        private void Delete(NodeCircleController ctr)
        {
            var ind = NodesPool.IndexOf(ctr);
            NodesPool.RemoveAt(ind);
            _firstFree = Mathf.Min(_firstFree, ind);
            ctr.gameObject.DestroyWhatever();
        }

        private void MakeVisible(Base_Node node, NodeCircleController centerNode = null)
        {

            NodeCircleController nnp = null;

            if (!centerNode)
                centerNode = node.parentNode?.visualRepresentation as NodeCircleController;

            var reusing = false;

            if (node.previousVisualRepresentation != null)
            {
                var tmp = node.previousVisualRepresentation as NodeCircleController;
                if (tmp && tmp.isFading && node == tmp.myLastLinkedNode)
                {
                    nnp = tmp;
                    if (tmp.gameObject.activeSelf)
                        reusing = true;

                }
            }

            if (!nnp)
            {
                while (_firstFree < NodesPool.Count)
                {
                    var np = NodesPool[_firstFree];

                    if (!np.gameObject.activeSelf)
                    {
                        nnp = np;
                        break;
                    }

                    _firstFree++;
                }
            }

            if (!nnp)
            {

                if (!circlePrefab)
                    return;

                nnp = Instantiate(circlePrefab);
                nnp.IndexForPEGI = NodesPool.Count;
                NodesPool.Add(nnp);

            }

            nnp.LinkTo(node);

            if (!reusing)
                nnp.SetStartPositionOn(centerNode);

        }

        private void MakeHidden(Base_Node node, NodeCircleController previous = null)
        {
            var ncc = node.visualRepresentation as NodeCircleController;

            if (!ncc)
                return;

            ncc.Unlink();

            if (!Application.isPlaying)
                Delete(ncc);
            else
                ncc.SetFadeAwayRelation(previous);

        }

        private void UpdateVisibility(Base_Node node, NodeCircleController previous = null)
        {

            if (node == null) return;

            bool editingOrGotParent = (Shortcuts.editingNodes ||
                                       ((node.parentNode != null && node.Conditions_isVisible()) ||
                                        !Application.isPlaying));

            bool currentNodeContains = (Shortcuts.CurrentNode != null && (node == Shortcuts.CurrentNode || Shortcuts.CurrentNode.Contains(node)));

            var shouldBeVisible = editingOrGotParent && currentNodeContains;

            if (node.visualRepresentation == null)
            {
                if (shouldBeVisible)
                    MakeVisible(node, previous);
            }
            else
            if (!shouldBeVisible)
                MakeHidden(node, previous);

            if (node.visualRepresentation != null)
                (node.visualRepresentation as NodeCircleController).SetDirty();
        }

        public void UpdateCurrentNodeGroupVisibilityAround(Node newCenter = null, NodeCircleController previousCenter = null)
        {

            if (!Application.isPlaying)
                return;

            if (newCenter == null)
                return;

            if (previousCenter == null)
                previousCenter = newCenter.visualRepresentation as NodeCircleController;

            UpdateVisibility(newCenter, previousCenter);
            foreach (var sub in newCenter)
                UpdateVisibility(sub, previousCenter);

        }

        [NonSerialized] public NodeCircleController selectedNode;

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

        public override void MakeVisible(Base_Node node)
        {

        }

        public override void MakeHidden(Base_Node node)
        {

        }

        public override void OnLogicUpdate()
        {
            var node = Shortcuts.CurrentNode;

            UpdateVisibility(node);

            foreach (var n in NodesPool)
                UpdateVisibility(n.source);

            UpdateCurrentNodeGroupVisibilityAround(node);
        }

        #endregion

        public override void ManagedOnInitialize()
        {
            inst = this;

            if (editButton)
                editButton.Text = "Edit";

            if (Application.isPlaying)
                editButton.graphic.OnClick.AddListener(RightTopButton);

            if (addButton)
                addButton.gameObject.SetActive(false);


        }

        public override void ManagedOnDeInitialize()
        {

            foreach (var e in NodesPool)
                if (e)
                {
                    e.Unlink();
                    e.gameObject.DestroyWhatever();
                }

            NodesPool.Clear();

            LevelArea.ManagedOnDisable();

        }

        public override void FadeAway() {

            HideAll();

            editButton.gameObject.SetActive(false);

            isFading = true;
        }

        public override bool TryFadeIn() {

            isFading = false;
            gameObject.SetActive(true);
            editButton.gameObject.SetActive(true);
            return true;
        }
        
        private readonly LoopLock _loopLock = new LoopLock();
        
        private float bgTransparency = 1;

        #region Lerping

        private readonly LerpData _ld = new LerpData();
        private LinkedLerp.FloatValue addButtonCourner = new LinkedLerp.FloatValue(0, 8);

        void Update() {

            var rtx = RayRenderingManager.instance;
            _ld.Reset();

            addButtonCourner.Portion(_ld);
            NodesPool.Portion(_ld);
            slidingButtons.Portion(_ld);
            rtx.Portion(_ld);


            addButtonCourner.Lerp(_ld);
            NodesPool.Lerp(_ld);
            slidingButtons.Lerp(_ld);
            rtx.Lerp(_ld, false);

            addButton.SetCorner(1, addButtonCourner.CurrentValue);

            bgTransparency = LerpUtils.LerpBySpeed(bgTransparency, 0.05f + _ld.MinPortion * 0.95f, 1f);

            NodeNotesGradientController.instance.bgTransparency.GlobalValue = bgTransparency;

            if (!isFading) {
                var col = MainCamera.backgroundColor;

            } else if (_ld.Portion() == 1)
                gameObject.SetActive(false);
        }

        #endregion
        
        #region Inspector
        public string NameForPEGIdisplay => "White Background";

        public override bool Inspect()
        {
            bool changed = false;

            pegi.nl();
            
            if (!IsRendering && "Reenable Nodes Rending".Click())
                IsRendering = true;

            Base_Node inspectedNode;

            if (Application.isPlaying && selectedNode)
            {

                inspectedNode = selectedNode.source;

                if (selectedNode.LevelArea && IsRendering && "Disable Renderers For Nodes".Click().nl())
                    IsRendering = false;

            }
            else
            {
                inspectedNode = Shortcuts.CurrentNode;
            }

            if (inspectedNode != null && inspectedNode.Nested_Inspect().nl(ref changed) && selectedNode)
                selectedNode.UpdateNameNow();

            if (inspectedNode == null || (inspectedNode._inspectedItems == -1 && (inspectedNode.AsNode == null || !inspectedNode.AsNode.InspectingSubNode)))
            {
                if ("Dependencies".enter(ref inspectedItems, 5).nl())
                {
                    if (!Application.isPlaying)
                    {
                        "Edit Button".edit(90, ref editButton).nl(ref changed);
                        "Add Button".edit(90, ref addButton).nl(ref changed);
                        "Delete Button".edit(90, ref deleteButton).nl(ref changed);
                        "Circles Prefab".edit(90, ref circlePrefab).nl(ref changed);
                    }

                    "Nodes Pool: {0}; First Free: {1}".F(NodesPool.Count, _firstFree).nl();

                    "Lerp Data dom:{0}".F(_ld.dominantParameter).nl();

                }
            }

            return changed;
        }

        #endregion

        #region Encode & Decode

        public override CfgEncoder EncodePerBookData() => new CfgEncoder();
               //.Add("bg", NodeNotesGradientController.instance.perNodeGradientConfigs.Encode())
               //.Add("rtx", perNodeRtxConfigs.Encode());
        
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                // DEPRECATED
                case "rtx": data.DecodeInto(out RayRenderingManager.instance.perNodeConfigs); break;
                case "bg": data.DecodeInto(out NodeNotesGradientController.instance.perNodeConfigs); break;
                default: return true;
            }
            return false;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized();

        #endregion

    }

    public class BackgroundGradient: AbstractKeepUnrecognizedCfg, IPEGI {

        public Color backgroundColorUp = Color.black;
        public Color backgroundColorCenter = Color.black;
        public Color backgroundColorDown = Color.black;

        #region Encode & Decode
        
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "bgUp": backgroundColorUp = data.ToColor(); break;
                case "bgc": backgroundColorCenter = data.ToColor(); break;
                case "bgDwn": backgroundColorDown = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("bgUp", backgroundColorUp)
            .Add("bgc", backgroundColorCenter)
            .Add("bgDwn", backgroundColorDown);
        #endregion

        #region Inspector
        public override bool Inspect() {

            var changed = false;

            "Background Up".edit(ref backgroundColorUp).nl(ref changed);
            "Background Center".edit(ref backgroundColorCenter).nl(ref changed);
            "Background Down".edit(ref backgroundColorDown).nl(ref changed);

            return changed;
        }
        #endregion
    }

}