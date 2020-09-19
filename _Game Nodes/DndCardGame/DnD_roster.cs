using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(Tag, "D&D roster")]
    public class DnDRoster : GameNodeBase {
        public const string Tag = "DnD_rost";
        public override string ClassTag => Tag;

        private static List<DnDRosterGroup> _perBookGroups = new List<DnDRosterGroup>();

        private static int _inspectedGroup = -1;

        #region Encode & Decode

        public override CfgEncoder Encode() => 
        new CfgEncoder()    
        //this.EncodeUnrecognized()
            .Add("b", base.Encode);

        public override void Decode(string tg, CfgData data) {
            switch (tg) {
                case "b": data.Decode(base.Decode); break;
                case "el": data.ToList(out _perBookGroups); break;
                case "i": _inspectedGroup = data.ToInt(); break;
            }   
        }

        public override CfgEncoder Encode_PerBookStaticData() => new CfgEncoder()//this.EncodeUnrecognized()
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

    public class DnDRosterGroup : ICfg
    { 

        public string name;
        public string NameForPEGI { get { return name; } set { name = value; } }

        private List<DndRosterElement> _elements = new List<DndRosterElement>();
        private int _inspectedElement = -1;

        #region Encode & Decode



        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "n": name = data.ToString(); break;
                case "el": data.ToList(out _elements); break;
                case "i": _inspectedElement = data.ToInt(); break;
            }
        }

        public CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("el", _elements)
            .Add_IfNotNegative("i", _inspectedElement);
        #endregion

        #region Inspector
        public virtual bool Inspect()
        {
            var changed = false;

            "Roster".edit_List(ref _elements, ref _inspectedElement).changes(ref changed);

            return changed;
        }


        #endregion

    }

    public class DndRosterElement : ICfg, IPEGI, IGotName {

        public string name;
        public string description;

        public ConditionBranch visibilityConditions = new ConditionBranch();

        public string NameForPEGI { get { return name; } set { name = value; } }

        #region Encode & Decode



        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("d", description)
            .Add("vc", visibilityConditions);

        public virtual void Decode(string tg, CfgData data) {
            switch (tg) {
                case "n": name = data.ToString(); break;
                case  "d": description = data.ToString(); break;
                case "vc": data.Decode(out visibilityConditions); break;
            }
        }
        #endregion

        #region Inspector
        private int _inspectedItems = -1;

        public bool Inspect() {
            bool changed = false;
            
            if (_inspectedItems == -1)
                "Description".editBig(ref description).nl(ref changed);
            
            "Conditions".enter_Inspect(visibilityConditions, ref _inspectedItems, 4).changes(ref changed);

            return changed;
        }
        #endregion

    }

}
