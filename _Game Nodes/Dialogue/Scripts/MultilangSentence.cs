using System;
using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using Random = UnityEngine.Random;

namespace NodeNotes {


    #pragma warning disable IDE0018 // Inline variable declaration

    public enum Languages { note = 0, en = 1, uk = 2, tr = 3, ru = 4 }
    
    public abstract class Sentence: AbstractKeepUnrecognizedCfg, IGotClassTag, IGotName, IPEGI {

        #region Tagged Types MGMT
        public abstract string ClassTag { get; }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(Sentence));
        #endregion

        public override string ToString() => NameForPEGI;

        public abstract bool GotNextText { get; } // => options.Count > index;

        public abstract string NameForPEGI { get; set; }

        public abstract string GetNext();// => NameForPEGI; // Will update all the options inside;

        public virtual void Reset() { }

    }

    [TaggedType(classTag, "String")]
    public class StringSentence : Sentence, IPEGI {

        const string classTag = "s";

        protected string text = "";

        protected bool sentOne;

        public override void Reset() => sentOne = false;

        public override bool GotNextText => !sentOne && !text.IsNullOrEmpty();

        public override string ClassTag => classTag;

        public override string NameForPEGI
        {
            get { return text; }
            set { text = value; }
        }

        public override string GetNext()
        {
            sentOne = true;
            return NameForPEGI;
        }

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("t", text);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "t": text = data; break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector
        
        public override bool Inspect()
        {
            var changed = pegi.edit(ref text).nl();

            return changed;
        }
        
        #endregion

        public StringSentence()
        {

        }

        public StringSentence(string newText)
        {
            text = newText;
        }

    }


    [TaggedType(classTag, "Multi Language")]
    public class MultilanguageSentence : Sentence,  IPEGI, IPEGI_ListInspect, INeedAttention {
        
        const string classTag = "ml";

        public override string ClassTag => classTag;

        public override string NameForPEGI { get { return this[currentLanguage]; } set { this[currentLanguage] = value; } }

        protected bool sentOne;

        public override void Reset() => sentOne = false;

        public override bool GotNextText => !sentOne;

        public override string GetNext()
        {
            sentOne = true;
            return NameForPEGI;
        }

        #region Languages MGMT
        public static Languages currentLanguage = Languages.en;

        private static List<string> _languageCodes;

        public static List<string> LanguageCodes
        {
            get
            {
                if (_languageCodes != null) return _languageCodes;

                _languageCodes = new List<string>();
                var names = Enum.GetNames(typeof(Languages));
                var values = (int[])Enum.GetValues(typeof(Languages));
                for (var i = 0; i < values.Length; i++)
                    _languageCodes.ForceSet(values[i], names[i]);

                return _languageCodes;
            }
        }
        
        public string this[Languages lang]
        {
            get
            {
                string text;
                var ind = (int)lang;

                if (texts.TryGetValue(ind, out text))
                    return text;
                if (lang == Languages.en)
                {
                    text = "English Text";
                    texts[ind] = text;
                }
                else
                    text = this[Languages.en];

                return text;
            }
            set { texts[(int)lang] = value; }
        }

        public bool Contains(Languages lang) => texts.ContainsKey((int)lang);

        public bool Contains() => Contains(currentLanguage);
        
        #endregion
        
        // Change this to also use Sentence base
        public Dictionary<int, string> texts = new Dictionary<int, string>();

        bool needsReview;

        public static bool singleView = true;

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("txts", texts)
            .Add_IfTrue("na", needsReview);

        public override bool Decode(string tg, string data){
            switch (tg) {
                case "t": NameForPEGI = data; break;
                case "txts": data.Decode_Dictionary(out texts); break;
                case "na": needsReview = data.ToBool(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector

        public static bool LanguageSelector_PEGI() => pegi.editEnum(ref currentLanguage, 30);
        
        public string NeedAttention() {
            if (needsReview)
                return "Marked for review";
            return null;
        }
        
        public virtual bool InspectInList(IList list, int ind, ref int edited) {
            var changed = this.inspect_Name();

            if (this.Click_Enter_Attention(hint: currentLanguage.GetNameForInspector()))
                edited = ind;
            return changed;
        }

        public override bool Inspect() {
            string tmp = NameForPEGI;


            "Show only one language".toggleIcon(ref singleView, true);
            if (singleView)  {
                LanguageSelector_PEGI();
                if (pegi.editBig(ref tmp)) {
                    NameForPEGI = tmp;
                    return true;
                }
            } else
            {

                pegi.nl();

                "Translations".edit_Dictionary_Values(texts, LanguageCodes);

                LanguageSelector_PEGI();
                if (!Contains() && icon.Add.Click("Add {0}".F(currentLanguage.GetNameForInspector())))
                    NameForPEGI = this[currentLanguage];

                pegi.nl();
            }

            "Mark for review".toggleIcon(ref needsReview, "NEEDS REVIEW");
          

            return false;
        }

        #endregion
    }

    [TaggedType(classTag, "Random Sentence")]
    public class RandomSentence : ListOfSentences, IPEGI {

        const string classTag = "rnd";

        public override string ClassTag => classTag;

        public bool pickedOne;

        public override bool GotNextText => !pickedOne || Current.GotNextText;

        public override void Reset() {
            base.Reset();
            pickedOne = false;
        }

        public override string GetNext() {
           
            if (!pickedOne)
                index = Random.Range(0, options.Count);

            pickedOne = true;

            return Current.GetNext();
        }

        #region Inspector

        public override bool Inspect()
        {
            var changed = pegi.edit_List(ref options).nl();

            return changed;
        }

        public override bool InspectInList(IList list, int ind, ref int edited)
        {
            "RND:".write(25);
            return base.InspectInList(list, ind, ref edited);
        }

        #endregion

    }
    
    [TaggedType(classTag, "List")]
    public class ListOfSentences : Sentence, IPEGI, IPEGI_ListInspect, IGotCount
    {

        const string classTag = "lst";

        public override string ClassTag => classTag;

        protected List<Sentence> options = new List<Sentence>();

        protected Sentence Current => options.TryGet(index);

        protected int index;

        public override void Reset() {
            foreach (var o in options)
                o.Reset();

            index = 0;
        }

        public override bool GotNextText => options.Count-1 > index || (index<options.Count && Current.GotNextText);

        public override string NameForPEGI
        {
            get { return Current.NameForPEGI; }
            set { Current.NameForPEGI = value; }
        }

        public override string GetNext() {

            var ret = Current.GetNext();

            if (!Current.GotNextText)
                index = (index+1) % options.Count;

            return  Current.GetNext();
        }

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("txs", options, all)
            .Add("ins", inspectedSentence);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "txs": data.Decode_List(out options, all); break;
                case "t": options.Add(new StringSentence(data)); break;
                case "ins": inspectedSentence = data.ToInt(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector

        private int inspectedSentence = -1;

        public override bool Inspect()
        {
            pegi.nl();

            var changed = "Sentences".edit_List(ref options, ref inspectedSentence).nl();

            return changed;
        }

        public virtual bool InspectInList(IList list, int ind, ref int edited) {

            var changed = false;

            if (options.Count>0)
                options[0].inspect_Name().changes(ref changed);
            else if ("Add Sentence".Click())
                options.Add(new StringSentence(" "));

            if (icon.Enter.Click())
                edited = ind;

            return changed;
        }

        public int CountForInspector() => options.Count;

        #endregion

        public ListOfSentences() {
            options.Add(new StringSentence());
        }

    }

    [TaggedType(classTag)]
    public class ConditionalSentence : MultilanguageSentence, IAmConditional
    {

        const string classTag = "cndSnt";

        public override string ClassTag => classTag;

        readonly ConditionBranch _condition = new ConditionBranch();

        public bool CheckConditions(Values values) => _condition.CheckConditions(values);

        #region Inspector
        
        public override bool InspectInList(IList list, int ind, ref int edited)
        {
            var changed = this.inspect_Name();
            if (this.Click_Enter_Attention(_condition.IsTrue() ? icon.Active : icon.InActive,
                currentLanguage.GetNameForInspector()))
                edited = ind;
            return changed;
        }

        public override bool Inspect()
        {
            var changes = _condition.Nested_Inspect().nl();
            changes |= base.Inspect();
            return changes;
        }

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

}


