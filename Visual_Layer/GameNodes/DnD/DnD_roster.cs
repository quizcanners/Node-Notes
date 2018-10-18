using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNotes;
using PlayerAndEditorGUI;
using SharedTools_Stuff;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual
{
    [TaggedType(tag, "DnD roster tags")]
    public class DnD_roster : GameNodeBase {
        public const string tag = "DnD_rost";
        public override string ClassTag => tag;

        List<DnDRosterGroup> bookGroups = new List<DnDRosterGroup>();
        int inspectedGroup = -1;

        #region Encode & Decode
        public override bool Decode_PerBook(string tag, string data) {
            switch (tag) {
                case "el": data.DecodeInto_List(out bookGroups); break;
                case "i": inspectedGroup = data.ToInt(); break;
                default: return false;
            }
            return true;           
        }

        public override StdEncoder Encode_PerBookData() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("el", bookGroups)
            .Add_IfNotNegative("i", inspectedGroup);
        #endregion

        #region Inspector

        protected override bool InspectGameNode() {
            bool changed =  base.Inspect();
            changed |= "Roster Groups".fold_enter_exit_List(bookGroups, ref inspectedGroup, ref inspectedStuff, 5);
            return changed;
        }

        #endregion

    }

    public class DnDRosterGroup : AbstractKeepUnrecognized_STD, IPEGI, IGotName {

        public string name;
        public string NameForPEGI { get { return name; } set { name = value; } }

        List<DnDrosterElement> elements = new List<DnDrosterElement>();
        int inspectedElement = -1;

        #region Encode & Decode
        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "el": data.DecodeInto_List(out elements); break;
                case "i": inspectedElement = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("el", elements)
            .Add_IfNotNegative("i", inspectedElement);
        #endregion

        #region Inspector

        public override bool Inspect()
        {
            bool changed = base.Inspect();

            changed |= "Roster".edit_List(elements, ref inspectedElement);

            return changed;
        }

        #endregion
    }

    public class DnDrosterElement : AbstractKeepUnrecognized_STD, IPEGI, IGotName {

        public string name;
        public string description;

        public ConditionBranch visibilityConditions = new ConditionBranch();

        public string NameForPEGI { get { return name; } set { name = value; } }

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("d", description)
            .Add("vc", visibilityConditions);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": name = data; break;
                case  "d": description = data; break;
                case "vc": data.DecodeInto(out visibilityConditions); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector
        public override bool Inspect() {
            bool changed = false;
            changed |= "Description".editBig(ref description).nl();

            changed |= visibilityConditions.Nested_Inspect();

            return changed;
        }


        #endregion

    }

}
