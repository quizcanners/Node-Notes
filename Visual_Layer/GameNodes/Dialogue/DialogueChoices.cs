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
 
    public class Interaction : AbstractKeepUnrecognized_STD, IGotName {

        public string name = "";
        public ConditionBranch conditions = new ConditionBranch();
        public List<Sentance> texts;
        public List<DialogueChoice> options;
        public List<Result> finalResults;

        public void Execute() {
            for (int j = 0; j < options.Count; j++)
                if (options[j].conditions.IsTrue) { options[j].results.Apply(); break; }

            finalResults.Apply();
        }
        
        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("ref", name)
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
                case "ref": name = data; break;
                case "Conds": data.DecodeInto(out conditions); break;
                case "txt": data.DecodeInto_List(out texts); break;
                case "opt": data.DecodeInto_List(out options); break;
                case "fin": data.DecodeInto_List(out finalResults); break;
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


        int inspectedStuff = -1;
        int inspectedText = -1;
        int inspectedChoice = -1;
        int inspectedResult = -1;

#if PEGI

      

        public string NameForPEGI { get { return name; } set { name = value; } }

        public override bool Inspect() {
            bool changed = false;

            if (inspectedStuff == -1)
                "Reference".editDelayed(ref name, 50).nl();

            changed |= "Texts".fold_enter_exit_List(texts, ref inspectedText, ref inspectedStuff, 01).nl();

            changed |= "Choices".fold_enter_exit_List(options, ref inspectedChoice, ref inspectedStuff, 1).nl();
           
            changed |= "Final Results".fold_enter_exit_List(finalResults, ref inspectedResult, ref inspectedStuff, 2).nl();

            "Conditions".fold_enter_exit(ref inspectedStuff, 3).nl();
                changed |= conditions.Inspect();

            return false;

        }
#endif
        #endregion

    }

    public class InteractionBranch : LogicBranch<Interaction>  {  }
    
    public class DialogueChoice : AbstractKeepUnrecognized_STD
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
                case "t": text = new Sentance(data); break;
                case "t2": data.DecodeInto_List(out texts2); break;
                case "res": data.DecodeInto_List(out results); break;
                case "ins": inspectedStuff = data.ToInt(); break;
                default: return false;
            }
            return true;

        }
        #endregion

        #region Inspector
        int inspectedStuff = -1;
        int inspectedResult = -1;
#if PEGI

        public static List<Interaction> inspectedInteractions;

        public override bool Inspect()
        {
            bool changed = false;

            if ("Text:".fold_enter_exit(ref inspectedStuff, 0).nl_ifFalse())
                changed |= text.Inspect();

            if ("Conditions:".fold_enter_exit(ref inspectedStuff,1).nl_ifFalse())
                conditions.Nested_Inspect();

            changed |= "Results:".fold_enter_exit_List(results, ref inspectedResult, ref inspectedStuff, 2).nl_ifFalse();
                
            if ("After choice text:".fold_enter_exit(ref inspectedStuff, 3).nl_ifFalse())
                texts2.PEGI();

            if ((inspectedStuff ==-1) && "Go To:".select_iGotName(ref nextOne, inspectedInteractions).nl())

            pegi.newLine();

            return changed;
        }
#endif
        #endregion
    }


}
