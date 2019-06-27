using System;
using UnityEngine;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System.Collections.Generic;
using NodeNotes;
using TMPro;
using PlaytimePainter;
using QcTriggerLogic;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class WhiteBackground : BackgroundBase
    {

        public static WhiteBackground inst;

        public const string classTag = "white";

        public override string ClassTag => classTag;

        public bool isFading;

        public Color color = Color.white;

        #region Click Events

        public void RightTopButton()
        {
            selectedNode = null;

            Base_Node.editingNodes = !Base_Node.editingNodes;

            if (editButton)
                editButton.text = Base_Node.editingNodes ? "Play" : "Edit";

            if (addButton)
                addButton.gameObject.SetActive(Base_Node.editingNodes);

            if (deleteButton)
                deleteButton.gameObject.SetActive(false);

            SetShowAddButtons(false);
        }

        public void ToggleShowAddButtons() => SetShowAddButtons(!CreateNodeButton.showCreateButtons);

        private void SetShowAddButtons(bool val)
        {
            CreateNodeButton.showCreateButtons = val;

            addButtonCourner.targetValue = CreateNodeButton.showCreateButtons ? 1 : 0;

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

        public TextMeshProUGUI editButton;

        public RoundedGraphic addButton;

        private LinkedLerp.FloatValue addButtonCourner = new LinkedLerp.FloatValue("+- courner", 0, 8);

        public NodeCircleController circlePrefab;

        public RoundedGraphic deleteButton;
        #endregion

        #region Inspector
        #if !NO_PEGI
        public string NameForPEGIdisplay => "White Background";

        public override bool Inspect() {
            bool changed = false;
            
            pegi.nl();

            if (Application.isPlaying && selectedNode)
            {
                if (selectedNode.source.Nested_Inspect()) {
                    selectedNode.UpdateName();
                    OnLogicVersionChange();
                }
            }

            if (selectedNode == null || (selectedNode.source == Shortcuts.CurrentNode && selectedNode.source.inspectedItems == 21))
            {
                "Background Color".edit(ref color).nl();

                if (icon.Create.enter("Dependencies", ref inspectedItems, 5))
                {
                    pegi.nl();
                    "Edit Button".edit(90, ref editButton).nl(ref changed);
                    "Add Button".edit(90, ref addButton).nl(ref changed);
                    "Delete Button".edit(90, ref deleteButton).nl(ref changed);

                    "Circles Prefab".edit(90, ref circlePrefab).nl(ref changed);

                    "Nodes Pool: {0}; First Free: {1}".F(NodesPool.Count, _firstFree).nl();
                }
            }

            return changed;
        }
        #endif
        #endregion

        #region Encode & Decode

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "col": color = data.ToColor(); break;
                default: return true;

            }
            return false;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("col", color);

        #endregion

        public override void FadeAway() => isFading = true;

        public override bool TryFadeIn() => isFading = false;

        [NonSerialized] private Camera _mainCam;

        private Camera MainCam
        {
            get
            {
                if (!_mainCam)
                    _mainCam = Camera.main;
                return _mainCam;
            }
        }

        private readonly LoopLock _loopLock = new LoopLock();
        
        public override void SetNode(Node node) {
        
                if (!_loopLock.Unlocked) return;

                using (_loopLock.Lock())
                {

                    if (Application.isPlaying && Shortcuts.CurrentNode != node)
                    {

                        if (node == null)
                            return;

                        SetSelected(null);

                        var previousN = Shortcuts.CurrentNode;

                        var previous = previousN?.visualRepresentation as NodeCircleController;

                        Shortcuts.CurrentNode = node;

                        UpdateVisibility(node, previous);

                        foreach (var n in NodesPool)
                            UpdateVisibility(n.source, previous);

                        UpdateCurrentNodeGroupVisibilityAround(node, previous);

                        _firstFree = 0;

                    }
                    else
                        Shortcuts.CurrentNode = node;
                }
            
        }

        public override void OnLogicUpdate()
        {
            var node = Shortcuts.CurrentNode;
            
            UpdateVisibility(node);

            foreach (var n in NodesPool)
                UpdateVisibility(n.source);

            UpdateCurrentNodeGroupVisibilityAround(node);
        }

        private readonly LerpData _ld = new LerpData();
        void Update() {

            _ld.Reset();

            addButtonCourner.Portion(_ld);
            NodesPool.Portion(_ld);
            slidingButtons.Portion(_ld);

            addButtonCourner.Lerp(_ld);
            NodesPool.Lerp(_ld);
            slidingButtons.Lerp(_ld);

            addButton.SetCorner(1, addButtonCourner.CurrentValue);
            
            if (!MainCam) return;

            var col = _mainCam.backgroundColor;

            _mainCam.backgroundColor = col.LerpBySpeed(Color.white, 3);



        }

        #region Node MGMT
        [NonSerialized] private readonly List<NodeCircleController> NodesPool = new List<NodeCircleController>();
        [NonSerialized] private int _firstFree = 0;

        public void Deactivate(NodeCircleController n)
        {
            n.gameObject.SetActive(false);
            _firstFree = Mathf.Min(_firstFree, n.IndexForPEGI);
        }

        private void Delete(NodeCircleController ctr)
        {
            var ind = NodesPool.IndexOf(ctr);
            NodesPool.RemoveAt(ind);
            _firstFree = Mathf.Min(_firstFree, ind);
            ctr.gameObject.DestroyWhatever();
        }

        private void DeleteAllNodes()
        {
            Debug.Log("Destroying nodes");

            foreach (var e in NodesPool)
                if (e)
                {
                    if (Application.isPlaying)
                        e.isFading = true;
                    else
                        e.gameObject.DestroyWhatever();

                }

            if (!Application.isPlaying)
                NodesPool.Clear();
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
                if (tmp && tmp.isFading && node == tmp.myLastLinkedNode) {
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
                    else _firstFree++;
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
            if (!ncc) return;

            ncc.Unlink();

            if (!Application.isPlaying)
                Delete(ncc);
            else
                ncc.SetFadeAwayRelation(previous);

        }
        
        private void UpdateVisibility(Base_Node node, NodeCircleController previous = null)
        {

            if (node == null) return;
            
            bool editingOrGotParent = (Base_Node.editingNodes ||
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

        public override void OnLogicVersionChange()
        {

        }


        #endregion

        public override void ManagedOnEnable()
        {
            inst = this;

            if (editButton)
                editButton.text = "Edit";

            if (addButton)
                addButton.gameObject.SetActive(false);
        }

        public override void ManagedOnDisable()  {
            
            DeleteAllNodes();

        }

    }
}