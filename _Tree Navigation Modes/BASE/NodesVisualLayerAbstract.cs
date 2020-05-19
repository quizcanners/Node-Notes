using System;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {
    
    public abstract class NodesVisualLayerAbstract : LogicMGMT {

        public static NodesVisualLayerAbstract InstAsNodesVisualLayer => instLogicMgmt as NodesVisualLayerAbstract;

        public Shortcuts shortcuts;

        public abstract Node CurrentNode { get; }

        public abstract void OnBeforeNodeSet(Node node);
        
        [NonSerialized] protected GameNodeBase gameNode;

        protected LoopLock loopLockEnt = new LoopLock();

        public bool IsCurrentGameNode(GameNodeBase gb) => (gameNode != null && gameNode == gb);

        public static Node preGameNode;

        public abstract void Show(Base_Node node);

        public abstract void Hide(Base_Node node);

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

        public abstract CfgEncoder EncodePerBookData();

        public abstract void Decode(string data);


    }

    public interface INodeVisualPresentation : ICfg
    {
        void OnSourceNodeUpdate(Base_Node node);
    }

}