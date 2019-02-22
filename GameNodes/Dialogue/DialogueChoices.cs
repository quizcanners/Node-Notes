using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual {
 
    public class Interaction : AbstractKeepUnrecognizedStd, IPEGI, IGotName, IGotDisplayName, IAmConditional, INeedAttention, IPEGI_ListInspect {

        public string referenceName = "";
        public ConditionBranch conditions = new ConditionBranch();
        public List<Sentance> texts = new List<Sentance>();
        public List<DialogueChoice> options = new List<DialogueChoice>();
        public List<Result> finalResults = new List<Result>();

        public void Execute() {
            for (int j = 0; j < options.Count; j++)
                if (options[j].conditions.IsTrue) { options[j].results.Apply(); break; }

            finalResults.Apply();
        }

        public bool CheckConditions(Values values) => conditions.CheckConditions(values);

        #region Encode & Decode

        public Interaction() {
            texts.Add(new Sentance());
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("ref", referenceName)
            .Add_IfNotDefault("Conds", conditions)
            .Add_IfNotEmpty("txt", texts)
            .Add_IfNotEmpty("opt", options)
            .Add_IfNotEmpty("fin", finalResults)
            .Add_IfNotNegative("is", inspectedStuff)
            .Add_IfNotNegative("it", _inspectedText)
            .Add_IfNotNegative("bc", _inspectedChoice)
            .Add_IfNotNegative("ir", _inspectedResult);
        
        public override bool Decode(string tg, string data) {
            switch (tg)  {
                case "ref": referenceName = data; break;
                case "Conds": data.DecodeInto(out conditions); break;
                case "txt": data.Decode_List(out texts); break;
                case "opt": data.Decode_List(out options); break;
                case "fin": data.Decode_List(out finalResults); break;
                case "is": inspectedStuff = data.ToInt(); break;
                case "it": _inspectedText = data.ToInt(); break;
                case "bc": _inspectedChoice = data.ToInt(); break;
                case "ir": _inspectedResult = data.ToInt(); break;
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

        private int _inspectedText = -1;
        private int _inspectedChoice = -1;
        private int _inspectedResult = -1;
        public static bool renameLinkedReferences = true; 

        public override void ResetInspector() {
            _inspectedText = -1;
            _inspectedChoice = -1;
            _inspectedResult = -1;
            base.ResetInspector();
        }

        public string NameForPEGI
        {
            get { return referenceName; }
            set
            {
                if (renameLinkedReferences && DialogueNode.inspected != null)
                    DialogueNode.inspected.interactionBranch.RenameReferance(referenceName, value);
                referenceName = value;
            }
        }

#if PEGI
        public string NameForDisplayPEGI => texts[0].NameForPEGI;

        public string NeedAttention() {

            var na = options.NeedAttentionMessage();
            return na ?? texts.NeedAttentionMessage("texts", false);
        }
        
        public override bool Inspect() {
            var changed = false;

            if (inspectedStuff == -1)
            {

                var n = NameForPEGI;

                if (renameLinkedReferences)
                {
                    if ("Ref".editDelayed(50, ref n))
                        NameForPEGI = n;
                } else
                if ("Ref".edit(50, ref n))
                    NameForPEGI = n;

                //this.inspect_Name("Rename Reference", "Other choices can set this interaction as a next one").changes(ref changed);

                pegi.toggle(ref renameLinkedReferences, icon.Link, icon.UnLinked, "Will all the references to this Interaction be renamed as well.").changes(ref changed);

                ("You can use reference to link end of one interaction with the start of another. But the first text of it will be skipped. First sentence is the option user picks to start an interaction. Like 'Lets talk about ...' " +
                 "which is not needed if the subject is currently being discussed from interaction that came to an end.")
                    .fullWindowDocumentationClick().nl();

            }

            if (inspectedStuff == 1 && _inspectedText == -1)
                Sentance.LanguageSelector_PEGI().nl();

            conditions.enter_Inspect_AsList(ref inspectedStuff, 4).nl(ref changed);
            
            "Texts".enter_List(ref texts, ref _inspectedText, ref inspectedStuff, 1).nl_ifNotEntered(ref changed);

            "Choices".enter_List(ref options, ref _inspectedChoice, ref inspectedStuff, 2).nl_ifNotEntered(ref changed);

            "Final Results".enter_List(ref finalResults, ref _inspectedResult, ref inspectedStuff, 3, ref changed).SetLastUsedTrigger();

            if (inspectedStuff == -1)
                "Results that will be set after any choice is selected".fullWindowDocumentationClick();

            pegi.nl_ifNotEntered();

            return false;

        }

        public bool PEGI_inList(IList list, int ind, ref int edited)
        {
            var changed = false;
            texts[0].inspect_Name().changes(ref changed);

            if (icon.Enter.Click())
                edited = ind;

            return changed;

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
    
    public class DialogueChoice : AbstractKeepUnrecognizedStd, IPEGI, IGotName, INeedAttention
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

        public override bool Decode(string tg, string data)
        {

            switch (tg)
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

            if (icon.Hint.enter(text.ToPegiString() ,ref inspectedStuff, 1))
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
