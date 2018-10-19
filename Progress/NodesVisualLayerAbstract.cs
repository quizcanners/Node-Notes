using STD_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;

namespace NodeNotes
{
    public abstract class NodesVisualLayerAbstract : LogicMGMT {
        public abstract void SetCurrentNode(Node node);

        public GameNodeBase gameNode = null;

        protected LoopLock loopLockEnt = new LoopLock();

        public virtual void EnterGameNode(GameNodeBase gn)
        {
            if (loopLockEnt.Unlocked)
                using (loopLockEnt.Lock()) {
                    ExitGameNode();
                    if (gn != null)
                        gn.Enter();
                }
            

            gameNode = gn;
        }

        protected LoopLock loopLockExit = new LoopLock();

        public virtual void ExitGameNode()  {

            if (loopLockExit.Unlocked)
                using (loopLockExit.Lock())  {
                    if (gameNode != null)
                        gameNode.Exit();
                 
                }

            gameNode = null;
        }

    }
}