using NodeNotes;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(Tag, "D&D Campfire")]
    public class DndCampfire : GameNodeBase {

        public const string Tag = "DnD_Stats";
        public override string ClassTag => Tag;

        public override CfgEncoder Encode_PerUserData() => base.Encode_PerUserData();

        public override void Decode(string tg, CfgData data) {
            switch (tg) {
                case "b": data.Decode(base.Decode); break; //.Decode_Base(base.Decode); break;
            }
        }
    }
}