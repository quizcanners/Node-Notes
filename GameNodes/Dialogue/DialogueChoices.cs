using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual {
 
    public class Interaction : AbstractKeepUnrecognized_STD, IGotName, IPEGI, IAmConditional, INeedAttention {

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

        public bool CheckConditions(Values vals) => conditions.CheckConditions(vals);

        #region Encode & Decode

        public Interaction() {
            texts.Add(new Sentance());
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("ref", referanceName)
            .Add_IfNotDefault("Conds", conditions)
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

        public static List<Interaction> inspectedList;
        
        public void RenameReferenceName (string oldName, string newName) {
            foreach (var o in options)
                o.RenameReference(oldName, newName);
        }

        int inspectedText = -1;
        int inspectedChoice = -1;
        int inspectedResult = -1;
        public static bool renameLinkedReferances = true; 

        public override void ResetInspector() {
            inspectedText = -1;
            inspectedChoice = -1;
            inspectedResult = -1;
            base.ResetInspector();
        }

#if PEGI
        public string NeedAttention() {

            var na = options.NeedAttentionMessage();
            if (na != null)
                return na;

            return texts.NeedAttentionMessage("texts", false);
        }
        
        public string NameForPEGI { get { return referanceName; }
            set { 
                if (renameLinkedReferances && DialogueNode.inspected != null)
                    DialogueNode.inspected.interactionBranch.RenameReferance(referanceName, value);
                referanceName = value;
            } }

        public override bool Inspect() {
            bool changed = false;

            if (inspectedStuff == -1) {
                this.inspect_Name("Reanme Reference", "Other choices can set this interaction as a next one").changes(ref changed);
                pegi.toggle(ref renameLinkedReferances, icon.Link, icon.UnLinked, "Will all the references to this Interaction be renamed as well.").nl(ref changed);
            }

            if (inspectedStuff == 1 && inspectedText == -1)
                Sentance.LanguageSelector_PEGI().nl();

            conditions.enter_Inspect_AsList(ref inspectedStuff, 4).nl(ref changed);
            
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

        void RenameReferenceLoop(LogicBranch<Interaction> br, string oldName, string newName) {
            foreach (var e in br.elements)
                e.RenameReferenceName(oldName, newName);

            foreach (var sb in br.subBranches)
                RenameReferenceLoop(sb, oldName, newName);
        }

#if PEGI
        public override bool Inspect()
        {
            CollectAll(ref Interaction.inspectedList);

            return base.Inspect();
        }
#endif

        public void RenameReferance (string oldName, string newName) => RenameReferenceLoop(this, oldName, newName);
        
        public InteractionBranch() {
            name = "root";
        }
    }
    
    public class DialogueChoice : AbstractKeepUnrecognized_STD, IPEGI, IGotName, INeedAttention
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

        public void RenameReference(string oldName, string newName) => nextOne = nextOne.SameAs(oldName) ? newName : nextOne;
            
        int inspectedResult = -1;
        int inspectedText = -1;
#if PEGI

        public string NeedAttention() {

            var na = text.NeedAttention();
            if (na != null) return na;

            na = texts2.NeedAttentionMessage();
            if (na != null) return na;

            return null;
        }

        public string NameForPEGI {
            get { return text.NameForPEGI; }
            set { text.NameForPEGI = value; } }

        public override bool Inspect() {

            bool changed = false;

            if (icon.Hint.enter(text.ToPEGIstring() ,ref inspectedStuff, 1))
                text.Nested_Inspect();
            else if (inspectedStuff == -1) Sentance.LanguageSelector_PEGI().nl();

            conditions.enter_Inspect_AsList(ref inspectedStuff, 2).nl_ifNotEntered(ref changed);

            "Results".enter_List(ref results, ref inspectedResult, ref inspectedStuff, 3, ref changed).SetLastUsedTrigger();
                
            pegi.nl_ifNotEntered();

            if (inspectedStuff == 4 && inspectedText == -1)
                Sentance.LanguageSelector_PEGI().nl();

            "After choice texts".enter_List(ref texts2, ref inspectedText, ref inspectedStuff, 4).nl_ifNotEntered(ref changed);

            if (!nextOne.IsNullOrEmpty() && icon.Delete.Click("Remove any followups"))
                nextOne = "";

            if (inspectedStuff == -1)
                "Go To".select_iGotName(60, ref nextOne, Interaction.inspectedList).nl();

            return changed;
        }

    
#endif
#endregion
    }


}
