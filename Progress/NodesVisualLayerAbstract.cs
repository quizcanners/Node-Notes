using STD_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;

namespace NodeNotes
{
    public abstract class NodesVisualLayerAbstract : LogicMGMT {
        public abstract Node CurrentNode { get; set; }

        protected GameNodeBase gameNode = null;

        public abstract void Show(Base_Node node);

        public abstract void Hide(Base_Node node);

        protected LoopLock loopLockEnt = new LoopLock();

        public bool IsCurrentGameNode(GameNodeBase gb) => (gameNode != null && gameNode == gb);

        public static Node preGameNode;

        public virtual void FromNodeToGame(GameNodeBase gn) {

            if (CurrentNode != null)
                preGameNode = CurrentNode;
            else preGameNode = gn.parentNode;

            if (loopLockEnt.Unlocked)
                using (loopLockEnt.Lock()) {
                    FromGameToNode();
                    gn.Enter();
                }

            CurrentNode = null;
            gameNode = gn;
        }

        protected LoopLock loopLockExit = new LoopLock();

        public virtual void FromGameToNode(bool failed = false)  {

            if (loopLockExit.Unlocked)
                using (loopLockExit.Lock())  {
                    if (gameNode != null) {
                        if (failed)
                            gameNode.FailExit();
                        else
                            gameNode.Exit();
                    }
                }

            gameNode = null;

            if (preGameNode != null)
                CurrentNode = preGameNode;
            else Debug.LogError("Pre Game Node was null");
        }

    }
}