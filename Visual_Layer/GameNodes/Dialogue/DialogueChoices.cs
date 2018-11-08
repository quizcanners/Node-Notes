using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual {
 
    public class Interaction : AbstractKeepUnrecognized_STD, IGotName, IPEGI {

        public string referanceName = "";
        public ConditionBranch conditions = new ConditionBranch();
        public List<Sentance> texts = new List<Sentance>();
        public List<DialogueChoice> options = new List<DialogueChoice>();
        public List<Result> finalResults = new List<Result>();

        public void Execute() {
            for (int j = 0; j < options.Count; j++)
                if (options[j].conditions.IsTrue) { options[j].results.Apply(); break; }

            finalResults.Apply();
        }
        
        #region Encode & Decode

        public Interaction() {
            texts.Add(new Sentance());
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("ref", referanceName)
            .Add("Conds", conditions)
            .Add_IfNotEmpty("txt", texts)
            .Add_IfNotEmpty("opt", options)
            .Add_IfNotEmpty("fin", finalResults)
            .Add_IfNotNegative("is", inspectedStuff)
            .Add_IfNotNegative("it", inspectedText)
            .Add_IfNotNegative("bc", inspectedChoice)
            .Add_IfNotNegative("ir", inspectedResult);



        public override bool Decode(string tag, string data) {
            switch (tag)  {
                case "ref": referanceName = data; break;
                case "Conds": data.DecodeInto(out conditions); break;
                case "txt": data.Decode_List(out texts); break;
                case "opt": data.Decode_List(out options); break;
                case "fin": data.Decode_List(out finalResults); break;
                case "is": inspectedStuff = data.ToInt(); break;
                case "it": inspectedText = data.ToInt(); break;
                case "bc": inspectedChoice = data.ToInt(); break;
                case "ir": inspectedResult = data.ToInt(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector

        int inspectedText = -1;
        int inspectedChoice = -1;
        int inspectedResult = -1;

        public override void ResetInspector() {
            inspectedText = -1;
            inspectedChoice = -1;
            inspectedResult = -1;
            base.ResetInspector();
        }

#if PEGI

        public string NameForPEGI { get { return referanceName; } set { referanceName = value; } }

        public override bool Inspect() {
            bool changed = false;

            if (inspectedStuff == 1 && inspectedText == -1)
                Sentance.LanguageSelector_PEGI().nl();
            
            "Conditions".enter_Inspect(conditions, ref inspectedStuff, 4).nl(ref changed);
            
            "Texts".enter_List(ref texts, ref inspectedText, ref inspectedStuff, 1).nl_ifNotEntered(ref changed);

            "Choices".enter_List(ref options, ref inspectedChoice, ref inspectedStuff, 2).nl_ifNotEntered(ref changed);

            "Final Results".enter_List(ref finalResults, ref inspectedResult, ref inspectedStuff, 3, ref changed).SetLastUsedTrigger();

            pegi.nl_ifNotEntered();

            return false;

        }
#endif
        #endregion

    }

    public class InteractionBranch : LogicBranch<Interaction>  {
        public override string NameForElements => "Interactions";

        public override void ResetInspector() {


            base.ResetInspector();
        }

    }
    
    public class DialogueChoice : AbstractKeepUnrecognized_STD, IPEGI, IGotName
    {
        public ConditionBranch conditions = new ConditionBranch();
        public Sentance text = new Sentance();
        public List<Sentance> texts2 = new List<Sentance>();
        public List<Result> results = new List<Result>();
        public string nextOne = "";

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
         .Add_IfNotEmpty("goto", nextOne)
         .Add("cnd", conditions)
         .Add("t", text)
         .Add_IfNotEmpty("t2", texts2)
         .Add_IfNotEmpty("res", results)
         .Add_IfNotNegative("ins", inspectedStuff);

        public override bool Decode(string tag, string data)
        {

            switch (tag)
            {
                case "goto": nextOne = data; break;
                case "cnd": data.DecodeInto(out conditions); break;
                case "t": text.Decode(data); break;
                case "t2": data.Decode_List(out texts2); break;
                case "res": data.Decode_List(out results); break;
                case "ins": inspectedStuff = data.ToInt(); break;
                default: return false;
            }
            return true;

        }
        #endregion

        #region Inspector
        int inspectedResult = -1;
        int inspectedText = -1;
        #if PEGI

        public static List<Interaction> inspectedInteractions;

        public string NameForPEGI {
            get { return text.NameForPEGI; }
            set { text.NameForPEGI = value; } }

        public override bool Inspect() {

            bool changed = false;

            if (icon.Hint.enter(text.ToPEGIstring() ,ref inspectedStuff, 1))
                text.Nested_Inspect();
            else if (inspectedStuff == -1) Sentance.LanguageSelector_PEGI().nl();

            if ("Conditions:".enter(ref inspectedStuff,2).nl_ifNotEntered())
                conditions.Nested_Inspect();

            "Results:".enter_List(ref results, ref inspectedResult, ref inspectedStuff, 3, ref changed).SetLastUsedTrigger();
                
            pegi.nl_ifNotEntered();

            if (inspectedStuff == 4 && inspectedText == -1)
                Sentance.LanguageSelector_PEGI().nl();

            "After choice texts:".enter_List(ref texts2, ref inspectedText, ref inspectedStuff, 4).nl_ifNotEntered(ref changed);

            if (inspectedStuff == -1)
                "Go To:".select_iGotName(ref nextOne, inspectedInteractions).nl();

            return changed;
        }
        #endif
        #endregion
    }


}
