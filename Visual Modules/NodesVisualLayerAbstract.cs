﻿using System;
using QcTriggerLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;

namespace NodeNotes
{
    public abstract class NodesVisualLayerAbstract : LogicMGMT
    {

        public Shortcuts shortcuts;

        public abstract Node CurrentNode { get; set; }

        [NonSerialized] protected GameNodeBase gameNode = null;

        protected LoopLock loopLockEnt = new LoopLock();

        public bool IsCurrentGameNode(GameNodeBase gb) => (gameNode != null && gameNode == gb);

        public static Node preGameNode;

        public abstract void Show(Base_Node node);

        public abstract void Hide(Base_Node node);

        public virtual void FromNodeToGame(GameNodeBase gn)
        {

            if (gameNode != null)
            {
                Debug.LogError("Exit previous Game Node");
                FromGameToNode();
            }

            if (loopLockEnt.Unlocked)
                using (loopLockEnt.Lock())
                {

                    if (CurrentNode != null)
                    {
                        if (CurrentNode as Node != null)
                            preGameNode = CurrentNode;
                        else
                        {
                            Debug.LogError("Current Node {0}: {1} is not a Node".F(CurrentNode.GetNameForInspector(),
                                CurrentNode.GetType().ToPegiStringType()));
                            preGameNode = gn.parentNode;
                        }
                    }
                    else preGameNode = gn.parentNode;

                    CurrentNode = null;

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
                        Shortcuts.user.ReturnToMark();
                    else
                    {
                        if (preGameNode != null)
                            CurrentNode = preGameNode;
                        else Debug.LogError("Pre Game Node was null");
                    }
                }
        }

        protected virtual void OnDisable()
        {
            if (shortcuts)
                shortcuts.SaveAll();

            Shortcuts.CurrentNode = null;
        }

      
    }
}