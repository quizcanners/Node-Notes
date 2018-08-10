using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using System;

namespace LinkedNotes
{

    [ExecuteInEditMode] 
    public class Nodes_PEGI : LogicMGMT
    {
        public static Nodes_PEGI NodeMGMT_inst;

        [NonSerialized] public Base_Node Cut_Paste;

        public Shortcuts shortcuts;
        
        public override bool PEGI() {
            bool changed = false;

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
        
        private void OnDisable() =>  shortcuts?.Save_STDdata();
        
        public override void OnEnable()
        {
            base.OnEnable();

            NodeMGMT_inst = this;

            shortcuts.Load_STDdata();
        }

    }
}
