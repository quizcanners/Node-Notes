using NodeNotes;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(Tag, "D&D Campfire")]
    public class DndCampfire : GameNodeBase {

        public const string Tag = "DnD_Stats";
        public override string ClassTag => Tag;

        public override CfgEncoder Encode_PerUserData() => base.Encode_PerUserData();

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.DecodeInto(base.Decode); break; //.Decode_Base(base.Decode); break;
                default: return false;
            }

            return true;
        }
    }
}