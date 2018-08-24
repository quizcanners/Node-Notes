﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using System;
using UnityEngine.UI;
using TMPro;

namespace LinkedNotes {

    [ExecuteInEditMode]
    public class Nodes_PEGI : LogicMGMT {

        pegi.windowPositionData window = new pegi.windowPositionData();

        public static Nodes_PEGI NodeMGMT_inst;

        [NonSerialized] public Base_Node Cut_Paste;

        public Shortcuts shortcuts;

        static Node _currentNode;

        public TextMeshProUGUI editButton;

        public Button addButton;

        public NodeCircleController circlePrefab;

        public Image Play_Edit_Button_Image;
        public Sprite playImage;
        public Sprite editImage;

        static List<NodeCircleController> nodesPool = new List<NodeCircleController>();
        static int firstFree = 0;

        static void VisualizeNode(Base_Node n) {

            NodeCircleController nnp = null;

            if (n.previousVisualRepresentation != null) {
                var tmp = n.previousVisualRepresentation as NodeCircleController;
                if (tmp != null && tmp.isFading)
                    nnp = tmp;
            }
            
            if (nnp == null)
            while (firstFree < nodesPool.Count) {
                var np = nodesPool[firstFree];
                if (np.isFading)
                {
                    nnp = np;
                    break;
                }
                else firstFree++;
            }

                if (nnp == null)
                {
                    nnp = Instantiate(NodeMGMT_inst.circlePrefab);
                    nnp.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                    nodesPool.Add(nnp);
                }

            nnp.LinkTo(n);
                
        }
        
        public static Node CurrentNode {
            get { return _currentNode; }
            set {

                Node wasAParent = null;

                if (value != null && _currentNode != null) {
                    var s = value as Node;

                    if (s != null)
                    {
                        if (s.subNotes.Contains(_currentNode))
                            wasAParent = _currentNode;
                    }
                }


                foreach (var n in nodesPool)
                    if (n != null && !n.isFading) {
                        if (!n.source.Equals(value) && (!n.source.Equals(wasAParent)))
                            n.Unlink();
                        else
                            n.assumedPosition = false;
                    }
                firstFree = 0;

                _currentNode = value;

                if (_currentNode != null) {

                    if (_currentNode.visualRepresentation == null)
                        VisualizeNode(_currentNode);//.LinkTo();

                    Shortcuts.playingInBook = value.root.IndexForPEGI;
                    Shortcuts.playingInNode = value.IndexForPEGI;

                    foreach (var n in _currentNode.subNotes)
                        if (n.visualRepresentation == null)
                            VisualizeNode(n);
                }

            }
        }
        
        public override bool PEGI() {
            bool changed = false;

            if (!circlePrefab)
            {
                changed |= "Circles Prefab".edit(ref circlePrefab).nl();
                return changed;
            }

            changed |= base.PEGI();

            if (!showDebug) {

                if ("Values ".fold_enter_exit(ref inspectedLogicBranchStuff, 1))
                    Values.global.PEGI();

                pegi.nl();

                if (!shortcuts)
                    "Shortcuts".edit(ref shortcuts).nl();
                else
                if (icon.StateMachine.fold_enter_exit("Shortcuts", ref inspectedLogicBranchStuff, 2))
                    shortcuts.Nested_Inspect();
                else
                    pegi.nl();
                
                if (icon.Condition.fold_enter_exit("Dependencies", ref inspectedLogicBranchStuff, 3)) {
                    pegi.nl();
                    "Edit Button".edit(ref editButton).nl();
                    "Add Button".edit(ref addButton).nl();
                }
                
                pegi.nl();
            }

            return changed;
        }
        
        public NodeCircleController selectedNode;
        public void RightTopButton() {
                selectedNode = null;
                Base_Node.editingNodes = !Base_Node.editingNodes;
                if (editButton)
                    editButton.text = Base_Node.editingNodes ? "Play" : "Edit";
            if (addButton)
                addButton.gameObject.SetActive(Base_Node.editingNodes);
        }
        
        public void SetSelected(NodeCircleController node ) {
            if (selectedNode)
                selectedNode.assumedPosition = false;
            selectedNode = node;
            if (node)
                node.assumedPosition = false;
        }

        private void OnDisable() {
            CurrentNode = null;
            ClearPool();
            shortcuts?.Save_STDdata();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            NodeMGMT_inst = this;
            
            ClearPool();

            shortcuts.Load_STDdata();
            
            if (Application.isPlaying)
                CurrentNode = Shortcuts.TryGetCurrentNode();

            editButton.text = "Edit";
            if (addButton)
                addButton.gameObject.SetActive(false);
        }

        void ClearPool()
        {
            foreach (var n in nodesPool)
                if (n != null)
                    n.DestroyWhatever();

            nodesPool.Clear();
        }

        public override void DerrivedUpdate() {

        }

        public void AddNode() => VisualizeNode(CurrentNode.AddNode());
       
        public void OnGUI() {
            if (selectedNode)
            window.Render(selectedNode.PEGI, selectedNode.ToPEGIstring());
        }
    }
}
