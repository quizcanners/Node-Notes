using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;

namespace LinkedNotes
{
    public class Nodes_PEGI : LogicMGMT
    {
        public Shortcuts shortcuts;
        
        public override bool PEGI()
        {
            bool changed = base.PEGI();

            if (!showDebug)
            {


                if ("Values ".fold_enter_exit(ref inspectedLogicBranchStuff, 1))
                    Values.global.PEGI();

                pegi.nl();

                if (icon.StateMachine.fold_enter_exit("Shortcuts", ref inspectedLogicBranchStuff, 2))
                    shortcuts.Nested_Inspect();

                pegi.nl();

             

            }

            return changed;
        }




    }
}
