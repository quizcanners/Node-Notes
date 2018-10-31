using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using System;
using UnityEngine.UI;
using TMPro;
using NodeNotes;
using UnityEngine.Networking;

namespace NodeNotes_Visual
{

    [ExecuteInEditMode]
    public class Nodes_PEGI : NodesVisualLayerAbstract {

        public static Nodes_PEGI NodeMGMT_inst;
        
        public Shortcuts shortcuts;

        public TextureDownloadManager textureDownloader = new TextureDownloadManager();

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

        public void DeleteSelected() {

            if (selectedNode != null) {

                var node = selectedNode.source;

                if (node.parentNode != null) {
                    selectedNode.Unlink();
                    node.Delete();
                    SetSelected(null);
                }
            }
        }
        #endregion

        #region BG
        public List<NodesStyleBase> backgroundControllers = new List<NodesStyleBase>();
        public void SetBackground (NodeCircleController circle) {
          //  var bg = circle != null ? backgroundControllers.TryGetByTag(circle.background) : null;
            var data = circle ? circle.backgroundConfig : "";
            var tag = circle ? circle.background : "";

            if (tag.Length == 0 && backgroundControllers.Count > 0)
                tag = backgroundControllers[0].ClassTag;

            for (int i=0; i<backgroundControllers.Count; i++) {
                var bc = backgroundControllers[i];// as IManageFading;
                if (bc != null) {
                    if (bc.ClassTag == tag) {
                        bc.TryFadeIn();
                        data.TryDecodeInto(bc);
                    }
                    else
                        bc.FadeAway();
                }
            }



        }
        #endregion

        #region Node MGMT

        static List<NodeCircleController> nodesPool = new List<NodeCircleController>();
        static int firstFree = 0;

        public void Deactivate(NodeCircleController n) {
            n.gameObject.SetActive(false);
            firstFree = Mathf.Min(firstFree, n.IndexForPEGI);
       }

        void Delete (NodeCircleController ctr) {
            var ind = nodesPool.IndexOf(ctr);
            nodesPool.RemoveAt(ind);
            firstFree = Mathf.Min(firstFree, ind);
            ctr.gameObject.DestroyWhatever();
        }

        void DeleteAllNodes() {
            foreach (var e in nodesPool)
                if (e!= null) {
                if (Application.isPlaying)
                     e.isFading = true;
                else
                    e.gameObject.DestroyWhatever();

            }

            nodesPool.Clear();
        }
        
        public override void Show(Base_Node node) => MakeVisible(node);

        static void MakeVisible(Base_Node node, NodeCircleController centerNode = null) {

            NodeCircleController nnp = null;

            if (centerNode == null) {
                centerNode = node.parentNode?.visualRepresentation as NodeCircleController;


            }

            bool reusing = false;

            if (node.previousVisualRepresentation != null) {
                var tmp = node.previousVisualRepresentation as NodeCircleController;
                if (tmp != null && tmp.isFading && node == tmp.myLastLinkedNode) {
                    nnp = tmp;
                    reusing = true;
                    Debug.Log("Reusing previous for {0}".F(node.ToPEGIstring()));
                }
            }

            if (nnp == null) {
                while (firstFree < nodesPool.Count) {
                    var np = nodesPool[firstFree];

                    if (!np.gameObject.activeSelf) {
                        nnp = np;
                        break;
                    }
                    else firstFree++;
                }
            }

            if (nnp == null) {
                if (NodeMGMT_inst.circlePrefab == null)
                    return;

                nnp = Instantiate(NodeMGMT_inst.circlePrefab);
                nnp.IndexForPEGI = nodesPool.Count;
                nodesPool.Add(nnp);

                Debug.Log("Creating new for {0}".F(node.ToPEGIstring()));
            }

            nnp.LinkTo(node);

            if (!reusing)
                nnp.SetStartPositionOn(centerNode);
        }

        public override void Hide(Base_Node node) => MakeHidden(node);

        static void MakeHidden(Base_Node node, NodeCircleController centerNode = null)
        {
            var ncc = node.visualRepresentation as NodeCircleController;
            if (ncc) {
                ncc.Unlink();
                ncc.SetFadeAwayRelation(centerNode);

                if (!Application.isPlaying)
                    NodeMGMT_inst.Delete(ncc);
            }
        }

        LoopLock loopLock = new LoopLock();

        public override Node CurrentNode
        {

            get
            {
                return Shortcuts.CurrentNode;
            }

            set {


                if (loopLock.Unlocked)
                    using (loopLock.Lock()) {

                        if (Application.isPlaying) {

                            SetSelected(null);

                            Node wasAParent = null;

                            var curNode = Shortcuts.CurrentNode;

                            if (value != null && curNode != null) {
                                var s = value as Node;
                                if (s != null) {
                                    if (s.coreNodes.Contains(curNode))
                                        wasAParent = curNode;
                                }
                            }

                            foreach (var n in nodesPool)
                                if (n != null && !n.isFading) {
                                    if (!n.source.Equals(value) && (!n.source.Equals(wasAParent)))
                                        n.Unlink();
                                    else
                                        n.SetDirty();
                                }

                            firstFree = 0;

                            Shortcuts.CurrentNode = value;

                            UpdateVisibility(curNode);

                            NodeCircleController circle = value != null ? value.visualRepresentation as NodeCircleController : null;

                            SetBackground(circle);
                        }
                        else
                            Shortcuts.CurrentNode = value;
                    }
                

            }
        }

        public static void UpdateVisibility(Base_Node node, NodeCircleController centerNode = null)  {

            if (node != null) {

                bool shouldBeVisible = Base_Node.editingNodes || ((node.parentNode != null && node.Conditions_isVisibile()) || !Application.isPlaying);

                if (node.visualRepresentation == null)  {
                    if (shouldBeVisible)
                        MakeVisible(node, centerNode);
                } else 
                    if (!shouldBeVisible)
                        MakeHidden(node, centerNode); 
            }
        }

        public static void UpdateVisibilityAround(NodeCircleController centerNode = null) {
            var cn = Shortcuts.CurrentNode;

            if (Application.isPlaying) {

                if (cn != null) {
                    UpdateVisibility(cn, centerNode);
                    foreach (var sub in cn)
                        UpdateVisibility(sub, centerNode);
                }
            }
        }

        public override void UpdateVisibility() => UpdateVisibilityAround();

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
#if PEGI
        pegi.windowPositionData window = new pegi.windowPositionData();
        
        public override bool Inspect() {

            if (gameNode != null) {

                if (icon.Close.Click("Exit Game Node in Fail"))
                    FromGameToNode(true);
                else  if (icon.Save.Click("Exit Game Node & Save"))
                    FromGameToNode();
                else return gameNode.Nested_Inspect();
            }

            if (Application.isPlaying && selectedNode)
            {
                if (selectedNode.Nested_Inspect()) {
                    UpdateVisibility();
                    return true;
                }
                return false;
            }
            if (inspectedStuff == -1 && "Encode / Decode Test".Click()) {
                OnDisable();
                OnEnable();
            }

            bool changed = base.Inspect();
            
            var cn = Shortcuts.CurrentNode;

                if (icon.StateMachine.conditional_enter(cn != null, ref inspectedStuff , 2))
                    changed |= cn.Nested_Inspect();

                if (inspectedStuff == -1)  {
                    if (cn != null)
                        "{0} -> [{1}] Current: {2} - {3}".F(Shortcuts.user.startingPoint, Shortcuts.user.bookMarks.Count, cn.root.ToPEGIstring(), cn.ToPEGIstring()).nl();
                    pegi.nl();
                }

                pegi.nl();

                if (!shortcuts)
                    "Shortcuts".edit(ref shortcuts).nl();
                else
                   if (icon.StateMachine.enter("Node Books", ref inspectedStuff, 4))
                    shortcuts.Nested_Inspect();
                else
                    pegi.nl();

            if (icon.Condition.enter("Dependencies", ref inspectedStuff, 5))
            {
                pegi.nl();
                changed |= "Edit Button".edit(ref editButton).nl();
                changed |= "Add Button".edit(ref addButton).nl();
                changed |= "Delete Button".edit(ref deleteButton).nl();
                changed |= "Backgrounds".edit_Property(() => backgroundControllers, this).nl();
                changed |= "Circles Prefab".edit(ref circlePrefab).nl();

                "Nodes Pool: {0}; First Free: {1}".F(nodesPool.Count, firstFree).nl();

            }

            pegi.nl();

                changed |= icon.Alpha.enter_Inspect("Textures", textureDownloader, ref inspectedStuff, 6).nl_ifNotEntered();
    

          



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

        int logicVersion = -1;
        public override void DerrivedUpdate()
        {
            if (Input.GetKey(KeyCode.Escape)) {
                OnDisable();
                Application.Quit();
                Debug.Log("Quit click");
            }

            if (logicVersion != currentLogicVersion)
            {
                UpdateVisibility();
                logicVersion = currentLogicVersion;
            }
        }

        void OnDisable() {
            shortcuts?.SaveAll();
            Shortcuts.CurrentNode = null;
            DeleteAllNodes();
            textureDownloader.Dispose();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            NodeMGMT_inst = this;

            DeleteAllNodes();

            shortcuts.LoadAll();

            editButton.text = "Edit";
            if (addButton)
                addButton.gameObject.SetActive(false);

            Shortcuts.visualLayer = this;

        }
        
     


    }
}
