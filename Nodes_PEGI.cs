using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;

namespace LinkedNotes
{
    public class Nodes_PEGI : ComponentSTD, IKeepMySTD
    {
        public Shortcuts shortcuts;

        [SerializeField] string std_Data = "";

        public override bool PEGI()
        {
            bool changed = base.PEGI();

            TriggerGroup.all.PEGI();

            if (shortcuts == null)
                changed |= "Shortcuts".edit(ref shortcuts).nl();
            else
            {
                changed |= shortcuts.values.Nested_Inspect();

            }

            return changed;
        }

        public string Config_STD
        {
            get { return std_Data; }
            set { std_Data = value; }
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "scuts": data.DecodeInto(shortcuts); break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("scuts", shortcuts);
    }
}
