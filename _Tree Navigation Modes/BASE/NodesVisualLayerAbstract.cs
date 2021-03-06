﻿using System;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {
    
    public abstract class NodesVisualLayerAbstract : LogicMGMT {

        public static NodesVisualLayerAbstract InstAsNodesVisualLayer => instLogicMgmt as NodesVisualLayerAbstract;

        public Shortcuts shortcuts;

        public Node CurrentNode => Shortcuts.CurrentNode;
        public abstract void OnBeforeNodeSet(Node node);
        
        [NonSerialized] protected GameNodeBase gameNode;

        protected LoopLock loopLockEnt = new LoopLock();

        public bool IsCurrentGameNode(GameNodeBase gb) => (gameNode != null && gameNode == gb);

        public static Node preGameNode;

        public virtual void OnNodeDelete(int index)
        {

        }

        public abstract void HideAllBackgrounds();

        public virtual void FromNodeToGame(GameNodeBase gn)
        {

            if (gameNode != null) {
                Debug.LogError("Exit previous Game Node");
                FromGameToNode();
            }

            if (loopLockEnt.Unlocked)
                using (loopLockEnt.Lock())
                {

                    //Debug.Log("From node to game");

                    if (CurrentNode != null)
                    {
                        if (CurrentNode != null)
                            preGameNode = CurrentNode;
                        else
                        {
                            Debug.LogError("Current Node {0}: {1} is not a Node".F(CurrentNode.GetNameForInspector(),
                                CurrentNode.GetType().ToPegiStringType()));
                            preGameNode = gn.parentNode;
                        }
                    }
                    else preGameNode = gn.parentNode;

                    Shortcuts.CurrentNode = null;

                    HideAllBackgrounds();

                    gn.Enter();

                    gameNode = gn;
                }
        }

        protected LoopLock loopLockExit = new LoopLock();

        public virtual void FromGameToNode(bool failed = false)
        {

            if (loopLockExit.Unlocked)
                using (loopLockExit.Lock())
                {
                    if (gameNode != null)
                    {
                        if (failed)
                            gameNode.FailExit();
                        else
                            gameNode.Exit();
                    }

                    gameNode = null;

                    if (failed)
                        Shortcuts.users.current.ReturnToBookMark();
                    else
                    {
                        if (preGameNode != null)
                            Shortcuts.CurrentNode = preGameNode;
                        else Debug.LogError("Pre Game Node was null");
                    }
                }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            shortcuts.LoadAll();
        }

        protected virtual void OnDisable()
        {
            if (shortcuts)
                shortcuts.SaveAll();
        }

        #region Encode & Decode
        public abstract CfgEncoder EncodePerBookData();

        public abstract void Decode(CfgData data);
        #endregion

        private int _inspectedStuffs = -1;

        public override bool Inspect()
        {
            var changed =  base.Inspect();

            "Shortcuts".edit_enter_Inspect(ref shortcuts, ref _inspectedStuffs, 0).nl();

            return changed;
        }

    }

    public interface INodeVisualPresentation : ICfg
    {
        void OnSourceNodeUpdate(Base_Node node);
    }

}