using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    public class NodeLinkComponent : NodeComponent
    {
        public ConditionBranch conditions = new ConditionBranch();

        public List<Result> results = new List<Result>();

        public override bool PEGI()
        {
            bool changed = base.PEGI();

            changed |= conditions.Nested_Inspect();

            return changed;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("cnd", conditions)
            .Add("rslts", results);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "cnd": data.DecodeInto(out conditions); break;
                case "rslts": data.DecodeInto(out results); break;
                default: return false;
            }
            return true;
        }

    }
}