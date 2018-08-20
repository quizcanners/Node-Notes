using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using System;

namespace LinkedNotes {

    [ExecuteInEditMode]
    public class Nodes_PEGI : LogicMGMT
    {
        public static Nodes_PEGI NodeMGMT_inst;

        [NonSerialized] public Base_Node Cut_Paste;

        public Shortcuts shortcuts;

        static Node _currentNode;

        public NodeCircleController circlePrefab;

        static List<NodeCircleController> nodesPool = new List<NodeCircleController>();
        static int firstFree = 0;

        static NodeCircleController getNodeCircle {
            get { while (firstFree < nodesPool.Count) {
                    var np = nodesPool[firstFree];
                    if (np.isFading)
                        return np;
                    else firstFree++;
                }

                var nnp = Instantiate(NodeMGMT_inst.circlePrefab);
                nnp.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                nodesPool.Add(nnp);

                //Debug.Log("iNSTANTIATING A CIRCLE");

                return nnp;
            }
        }

        public static Node CurrentNode {
            get { return _currentNode; }
            set {

                foreach (var n in nodesPool)
                    if (n!= null && !n.isFading && !n.source.Equals(value))
                        n.Unlink();

                firstFree = 0;

                _currentNode = value;
                if (_currentNode != null) {

                    if (_currentNode.visualRepresentation == null)
                        getNodeCircle.LinkTo(_currentNode);

                    Shortcuts.playingInBook = value.root.IndexForPEGI;
                    Shortcuts.playingInNode = value.IndexForPEGI;
                    
                    foreach (var n in _currentNode.subNotes)
                        getNodeCircle.LinkTo(n);
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

                pegi.nl();
            }

            return changed;
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

        }

        void ClearPool()
        {
            foreach (var n in nodesPool)
                if (n != null)
                    n.DestroyWhatever();

            nodesPool.Clear();
        }

    }
}
