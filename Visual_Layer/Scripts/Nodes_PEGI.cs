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

namespace NodeNotes_Visual
{

    [ExecuteInEditMode]
    public class Nodes_PEGI : NodesVisualLayerAbstract {

        public static Nodes_PEGI NodeMGMT_inst;
        
        public Shortcuts shortcuts;

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

        public void AddNode() => VisualizeNode(Shortcuts.CurrentNode.Add<Node>());

        public void AddLink() => VisualizeNode(Shortcuts.CurrentNode.Add<NodeLinkComponent>());

        public void AddButton() => VisualizeNode(Shortcuts.CurrentNode.Add<NodeButtonComponent>());

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
            var tag = circle ? circle.background : "null";

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

        static void VisualizeNode(Base_Node n) {

            NodeCircleController nnp = null;

            if (n.previousVisualRepresentation != null)
            {
                var tmp = n.previousVisualRepresentation as NodeCircleController;
                if (tmp != null && tmp.isFading)
                    nnp = tmp;
            }

            if (nnp == null)
            {
                while (firstFree < nodesPool.Count)
                {
                    var np = nodesPool[firstFree];
                    if (np.isFading)
                    {
                        nnp = np;
                        break;
                    }
                    else firstFree++;
                }
            }

            if (nnp == null)
            {
                if (NodeMGMT_inst.circlePrefab == null)
                    return;

                nnp = Instantiate(NodeMGMT_inst.circlePrefab);
                nodesPool.Add(nnp);
            }

            nnp.LinkTo(n);

        }

        LoopLock loopLock = new LoopLock();

        public override Node CurrentNode
        {

            get
            {
                return Shortcuts.CurrentNode;
            }

            set
            {


                if (loopLock.Unlocked)
                {
                    using (loopLock.Lock())
                    {

                        if (Application.isPlaying)
                        {

                            SetSelected(null);

                            Node wasAParent = null;

                            var curNode = Shortcuts.CurrentNode;

                            if (value != null && curNode != null)
                            {
                                var s = value as Node;
                                if (s != null)
                                {
                                    if (s.coreNodes.Contains(curNode))
                                        wasAParent = curNode;
                                }
                            }

                            foreach (var n in nodesPool)
                                if (n != null && !n.isFading)
                                {
                                    if (!n.source.Equals(value) && (!n.source.Equals(wasAParent)))
                                        n.Unlink();
                                    else
                                        n.assumedPosition = false;
                                }

                            firstFree = 0;

                            Shortcuts.CurrentNode = value;

                            NodeCircleController circle = value != null ? value.visualRepresentation as NodeCircleController : null;

                            UpdateVisibility();

                            SetBackground(circle);
                        }
                        else
                            Shortcuts.CurrentNode = value;
                    }
                }

            }
        }

        public static void UpdateVisibility(Base_Node node)  {

            if (node != null) {

                if (node.visualRepresentation == null)  {

                    if (Base_Node.editingNodes || (node.Conditions_isVisibile()))// && node.parentNode != null))
                        VisualizeNode(node);
                } else {

                    if (!Base_Node.editingNodes && !node.Conditions_isVisibile())
                        (node.visualRepresentation as NodeCircleController).Unlink();
                }
            }
        }

        public static void UpdateVisibility()
        {
            var cn = Shortcuts.CurrentNode;

            if (Application.isPlaying) {

                if (cn != null)  {
                    UpdateVisibility(cn);
                    foreach (var sub in cn)
                        UpdateVisibility(sub);
                }
            }
        }

        public NodeCircleController selectedNode;

        public void SetSelected(NodeCircleController node)
        {
            if (selectedNode)
                selectedNode.assumedPosition = false;

            selectedNode = node;

            if (node)
                node.assumedPosition = false;

            if (deleteButton)
                deleteButton.gameObject.SetActive(selectedNode);
        }

        void ClearPool()
        {
            foreach (var n in nodesPool)
                if (n != null)
                    n.DestroyWhatever();

            nodesPool.Clear();
        }


        #endregion

        #region Inspector
#if PEGI
        pegi.windowPositionData window = new pegi.windowPositionData();
        
        public override bool Inspect() {

            bool changed = base.Inspect();

     
                var cn = Shortcuts.CurrentNode;

                if (icon.StateMachine.conditional_enter(cn != null, ref inspectedStuff , 2))
                    changed |= cn.Nested_Inspect();

                if (inspectedStuff == -1)  {
                    if (cn != null)
                        "{0} -> [{1}] Current: {2} - {3}".F(Shortcuts.user.startingPoint, Shortcuts.user.bookMarks.Count, cn.root.ToPEGIstring(), cn.ToPEGIstring()).nl();
                    pegi.nl();
                }

                if ("Values ".enter(ref inspectedStuff, 3))
                    Values.global.Inspect();

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
                    changed |= "Backgrounds".edit(() => backgroundControllers, this).nl();
                    changed |= "Circles Prefab".edit(ref circlePrefab).nl();
                }

                pegi.nl();

                if (inspectedStuff == -1 && "Encode / Decode Test".Click().nl())
                {
                    OnDisable();
                    OnEnable();
                }
            



            return changed;
        }

        public void OnGUI() {

            if (selectedNode)
                window.Render(selectedNode);

            if (Shortcuts.CurrentNode == null && shortcuts)
                window.Render(shortcuts);
        }

        #endif
        #endregion
        
        private void OnDisable() {
            shortcuts?.SaveAll();
            Shortcuts.CurrentNode = null;
            ClearPool();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            NodeMGMT_inst = this;

            ClearPool();

            shortcuts.LoadAll();

            editButton.text = "Edit";
            if (addButton)
                addButton.gameObject.SetActive(false);

            Shortcuts.visualLayer = this;

        }
        
        int logicVersion = -1;
        public override void DerrivedUpdate()
        {
            if (logicVersion != currentLogicVersion)
            {
                UpdateVisibility();
                logicVersion = currentLogicVersion;
            }
        }
       
    }
}
