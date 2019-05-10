using System.Collections;
using System.Collections.Generic;
using QuizCannersUtilities;
using STD_Logic;

namespace PlayerAndEditorGUI
{

    [TaggedType(classTag)]
    public class ConditionalSentence : MultilanguageSentence, IAmConditional
    {

        const string classTag = "cndSnt";

        public override string ClassTag => classTag;

        readonly ConditionBranch _condition = new ConditionBranch();

        public bool CheckConditions(Values values) => _condition.CheckConditions(values);

        #region Inspector

#if PEGI
        public override bool InspectInList(IList list, int ind, ref int edited)
        {
            var changed = this.inspect_Name();
            if (this.Click_Enter_Attention(_condition.IsTrue() ? icon.Active : icon.InActive,
                currentLanguage.ToPegiString()))
                edited = ind;
            return changed;
        }

        public override bool Inspect()
        {
            var changes = _condition.Nested_Inspect().nl();
            changes |= base.Inspect();
            return changes;
        }
#endif

        #endregion

        #region Encode & Decode

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("b", base.Encode)
            .Add_IfNotDefault("cnd", _condition);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "b":
                    data.Decode_Base(base.Decode, this);
                    break;
                case "cnd":
                    _condition.Decode(data);
                    break;
                default: return false;
            }

            return true;
        }

        #endregion

    }


    public static class MultiLanguageSentenceExtensions
    {

        public static Sentence GetNextText(this List<Sentence> list, ref int startIndex)
        {

            while (list.Count > startIndex)
            {
                var txt = list[startIndex];

                if (!txt.TryTestCondition())
                    startIndex++;
                else
                    return txt;
            }

            return null;
        }

    }
}