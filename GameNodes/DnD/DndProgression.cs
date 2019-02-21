using NodeNotes;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(Tag, "D&D Campfire")]
    public class DndCampfire : GameNodeBase {

        public const string Tag = "DnD_Stats";
        public override string ClassTag => Tag;


        private static DndCharacterStats MainCharacterStats = new DndCharacterStats();


        public override StdEncoder Encode_PerUserData() => this.EncodeUnrecognized()
            .Add("hero", MainCharacterStats);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "hero": data.DecodeInto(out MainCharacterStats); break;
                default: return false;
            }

            return true;
        }
    }

    public class DndCharacterStats : AbstractKeepUnrecognizedStd {




    }

    public class DndCharacterRace {

    }
}