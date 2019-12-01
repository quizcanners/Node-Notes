using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using QcTriggerLogic;

namespace NodeNotes_Visual {

    [TaggedType(Tag, "D&D roster")]
    public class DnDRoster : GameNodeBase {
        public const string Tag = "DnD_rost";
        public override string ClassTag => Tag;

        private static List<DnDRosterGroup> _perBookGroups = new List<DnDRosterGroup>();

        private static int _inspectedGroup = -1;

        #region Encode & Decode

        public override CfgEncoder Encode() => 
            this.EncodeUnrecognized()
            .Add("b", base.Encode);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "el": data.Decode_List(out _perBookGroups); break;
                case "i": _inspectedGroup = data.ToInt(); break;
                default: return false;
            }
            return true;           
        }

        public override CfgEncoder Encode_PerBookStaticData() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("el", _perBookGroups)
            .Add_IfNotNegative("i", _inspectedGroup);
        #endregion

        #region Inspector
        protected override bool InspectGameNode() {
            bool changed = "Roster Groups".edit_List(ref _perBookGroups, ref _inspectedGroup);
            return changed;
        }
        #endregion

    }

    public class DnDRosterGroup : AbstractKeepUnrecognizedCfg, IPEGI, IGotName {

        public string name;
        public string NameForPEGI { get { return name; } set { name = value; } }

        private List<DndRosterElement> _elements = new List<DndRosterElement>();
        private int _inspectedElement = -1;

        #region Encode & Decode
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "n": name = data; break;
                case "el": data.Decode_List(out _elements); break;
                case "i": _inspectedElement = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("el", _elements)
            .Add_IfNotNegative("i", _inspectedElement);
        #endregion

        #region Inspector
        public override bool Inspect()
        {
            bool changed = base.Inspect();
            
            "Roster".edit_List(ref _elements, ref _inspectedElement).changes(ref changed);

            return changed;
        }
        #endregion

    }

    public class DndRosterElement : AbstractKeepUnrecognizedCfg, IPEGI, IGotName {

        public string name;
        public string description;

        public ConditionBranch visibilityConditions = new ConditionBranch();

        public string NameForPEGI { get { return name; } set { name = value; } }

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
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
        public override bool Inspect() {
            bool changed = false;
            
            if (_inspectedItems == -1)
                "Description".editBig(ref description).nl(ref changed);
            
            "Conditions".enter_Inspect(visibilityConditions, ref _inspectedItems, 4).changes(ref changed);

            return changed;
        }
        #endregion

    }

}
