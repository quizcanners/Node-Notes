using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual {

    [TaggedType(tag, "DnD roster")]
    public class DnD_roster : GameNodeBase {
        public const string tag = "DnD_rost";
        public override string ClassTag => tag;

        static List<DnDRosterGroup> perBookGroups = new List<DnDRosterGroup>();

        static int inspectedGroup = -1;

        #region Encode & Decode

        public override StdEncoder Encode() => 
            this.EncodeUnrecognized()
            .Add("b", base.Encode);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "el": data.Decode_List(out perBookGroups); break;
                case "i": inspectedGroup = data.ToInt(); break;
                default: return false;
            }
            return true;           
        }

        public override StdEncoder Encode_PerBookStaticData() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("el", perBookGroups)
            .Add_IfNotNegative("i", inspectedGroup);
        #endregion

        #region Inspector
        #if PEGI
        protected override bool InspectGameNode() {
            bool changed = "Roster Groups".edit_List(ref perBookGroups, ref inspectedGroup);
            return changed;
        }
        #endif
        #endregion

    }

    public class DnDRosterGroup : AbstractKeepUnrecognizedStd, IPEGI, IGotName {

        public string name;
        public string NameForPEGI { get { return name; } set { name = value; } }

        List<DnDrosterElement> elements = new List<DnDrosterElement>();
        int inspectedElement = -1;

        #region Encode & Decode
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "n": name = data; break;
                case "el": data.Decode_List(out elements); break;
                case "i": inspectedElement = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("el", elements)
            .Add_IfNotNegative("i", inspectedElement);
        #endregion

        #region Inspector
        #if PEGI
        public override bool Inspect()
        {
            bool changed = base.Inspect();
            
            changed |= "Roster".edit_List(ref elements, ref inspectedElement);

            return changed;
        }
        #endif
        #endregion

    }

    public class DnDrosterElement : AbstractKeepUnrecognizedStd, IPEGI, IGotName {

        public string name;
        public string description;

        public ConditionBranch visibilityConditions = new ConditionBranch();

        public string NameForPEGI { get { return name; } set { name = value; } }

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("d", description)
            .Add("vc", visibilityConditions);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "n": name = data; break;
                case  "d": description = data; break;
                case "vc": data.DecodeInto(out visibilityConditions); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector
        #if PEGI
        public override bool Inspect() {
            bool changed = false;
            
            if (inspectedStuff == -1)
            changed |= "Description".editBig(ref description).nl();
            
            changed |= "Conditions".enter_Inspect(visibilityConditions, ref inspectedStuff, 4);

            return changed;
        }
        #endif
        #endregion

    }

}
