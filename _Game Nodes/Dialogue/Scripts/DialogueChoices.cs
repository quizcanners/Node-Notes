using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QcTriggerLogic;
using QuizCannersUtilities;

namespace NodeNotes_Visual {
 
    public class Interaction : AbstractKeepUnrecognizedCfg, IPEGI, IGotDisplayName, IAmConditional, INeedAttention, IPEGI_ListInspect {

        private string referenceName = "";
        public ConditionBranch conditions = new ConditionBranch();
        public ListOfSentences texts = new ListOfSentences();
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public List<Result> finalResults = new List<Result>();

        public void ResetSentences() {
            texts.Reset();
            foreach (var o in choices)
                o.ResetSentences();
        }

        public void Execute() {
            for (int j = 0; j < choices.Count; j++)
                if (choices[j].conditions.IsTrue) { choices[j].results.Apply(); break; }

            finalResults.Apply();
        }

        public bool CheckConditions(Values values) => conditions.CheckConditions(values);

        #region Encode & Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotEmpty("ref", referenceName)
            .Add_IfNotDefault("Conds", conditions)
            .Add("txts", texts)
            .Add_IfNotEmpty("opt", choices)
            .Add_IfNotEmpty("fin", finalResults)
            .Add_IfNotNegative("is", _inspectedItems)
            .Add_IfNotNegative("bc", _inspectedChoice)
            .Add_IfNotNegative("ir", _inspectedResult);
        
        public override bool Decode(string tg, string data) {
            switch (tg)  {
                case "ref": referenceName = data; break;
                case "Conds": data.DecodeInto(out conditions); break;
                case "txts": texts.Decode(data); break;
                case "opt": data.Decode_List(out choices); break;
                case "fin": data.Decode_List(out finalResults); break;
                case "is": _inspectedItems = data.ToInt(); break;
                case "bc": _inspectedChoice = data.ToInt(); break;
                case "ir": _inspectedResult = data.ToInt(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector

        public static List<Interaction> inspectedList = new List<Interaction>();
        
        public void RenameReferenceName (string oldName, string newName) {
            foreach (var o in choices)
                o.RenameReference(oldName, newName);
        }
        
        private int _inspectedChoice = -1;
        private int _inspectedResult = -1;
        public static bool renameLinkedReferences = true; 

        public override void ResetInspector() {
            _inspectedChoice = -1;
            _inspectedResult = -1;
            base.ResetInspector();
        }


        public string ReferenceName {
            get { return referenceName; }
            set
            {
                if (renameLinkedReferences && DialogueNode.inspected != null)
                    DialogueNode.inspected.interactionBranch.RenameReferance(referenceName, value);
                referenceName = value;
            }
        }

        /*
        public string NameForPEGI
        {
            get { return referenceName; }
            set {
                if (renameLinkedReferences && DialogueNode.inspected != null)
                    DialogueNode.inspected.interactionBranch.RenameReferance(referenceName, value);
                referenceName = value;
            }
        }*/
        
        public string NameForDisplayPEGI() => texts.NameForPEGI;

        public string NeedAttention() {

            var na = pegi.NeedsAttention(choices);

            return na;
        }
        
        public override bool Inspect() {
            var changed = false;

            if (_inspectedItems == -1)
            {

                var n = referenceName;

                if (renameLinkedReferences)
                {
                    if ("Ref".editDelayed(50, ref n))
                        ReferenceName = n;
                } else
                if ("Ref".edit(50, ref n))
                    ReferenceName = n;

                //this.inspect_Name("Rename Reference", "Other choices can set this interaction as a next one").changes(ref changed);

                pegi.toggle(ref renameLinkedReferences, icon.Link, icon.UnLinked, "Will all the references to this Interaction be renamed as well.").changes(ref changed);

                if (pegi.PopUpService.DocumentationClick("About option referance"))
                    pegi.PopUpService.FullWindwDocumentationOpen(() =>
                        "You can use reference to link end of one interaction with the start of another. But the first text of it will be skipped. First sentence is the option user picks to start an interaction. Like 'Lets talk about ...' " +
                         "which is not needed if the subject is currently being discussed from interaction that came to an end."
                        );

            }

            pegi.nl();

            if (_inspectedItems == 1)
                MultilanguageSentence.LanguageSelector_PEGI().nl();

            conditions.enter_Inspect_AsList(ref _inspectedItems, 4).nl(ref changed);
            
            "Texts".enter_Inspect(texts, ref _inspectedItems, 1).nl_ifNotEntered(ref changed);

            "Choices".enter_List(ref choices, ref _inspectedChoice, ref _inspectedItems, 2).nl_ifNotEntered(ref changed);

            "Final Results".enter_List(ref finalResults, ref _inspectedResult, ref _inspectedItems, 3, ref changed).SetLastUsedTrigger();

            if (_inspectedItems == -1)
               pegi.PopUpService.fullWindowDocumentationClickOpen("Results that will be set the moment any choice is picked, before the text that goes after it", "About Final Results");

            pegi.nl_ifNotEntered();

            return false;

        }

        public bool InspectInList(IList list, int ind, ref int edited)
        {
            var changed = false;

            texts.inspect_Name().changes(ref changed);

            if (icon.Enter.Click())
                edited = ind;

            return changed;

        }
        
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
        
        public override bool Inspect()
        {
            Interaction.inspectedList.Clear();

            CollectAll(ref Interaction.inspectedList);

            return base.Inspect();
        }

        public void RenameReferance (string oldName, string newName) => RenameReferenceLoop(this, oldName, newName);
        
        public InteractionBranch() {
            name = "root";
        }
    }
    
    public class DialogueChoice : AbstractKeepUnrecognizedCfg, IPEGI, IGotName, INeedAttention
    {
        public ConditionBranch conditions = new ConditionBranch();
        public MultilanguageSentence text = new MultilanguageSentence();
        public ListOfSentences text2 = new ListOfSentences();
        public List<Result> results = new List<Result>();
        public string nextOne = "";

        public void ResetSentences() {
            text.Reset();
            text2.Reset();
        }

#region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
         .Add_IfNotEmpty("goto", nextOne)
         .Add("cnd", conditions)
         .Add("t", text)
         .Add("ts2b", text2)
         .Add_IfNotEmpty("res", results)
         .Add_IfNotNegative("ins", _inspectedItems);

        public override bool Decode(string tg, string data)
        {

            switch (tg)
            {
                case "goto": nextOne = data; break;
                case "cnd": data.DecodeInto(out conditions); break;
                case "t": text.Decode(data); break;
                case "ts2b": text2.Decode(data); break;
                case "res": data.Decode_List(out results); break;
                case "ins": _inspectedItems = data.ToInt(); break;
                default: return false;
            }
            return true;

        }
        #endregion

        #region Inspector

        public void RenameReference(string oldName, string newName) => nextOne = nextOne.SameAs(oldName) ? newName : nextOne;
        
        int inspectedResult = -1;

        public string NeedAttention() {

            var na = text.NeedAttention();
            if (na != null) return na;
            
            return null;
        }

        public string NameForPEGI {
            get { return text.NameForPEGI; }
            set { text.NameForPEGI = value; } }

        public override bool Inspect() {

            bool changed = false;

            conditions.enter_Inspect_AsList(ref _inspectedItems, 1).nl_ifNotEntered(ref changed);

            if (icon.Hint.enter(text.GetNameForInspector() ,ref _inspectedItems, 2))
                text.Nested_Inspect();
            else if (_inspectedItems == -1) MultilanguageSentence.LanguageSelector_PEGI().nl();

            "Results".enter_List(ref results, ref inspectedResult, ref _inspectedItems, 3, ref changed).SetLastUsedTrigger();
                
            pegi.nl_ifNotEntered();

            if (_inspectedItems == 4)
                MultilanguageSentence.LanguageSelector_PEGI().nl();

            "After choice texts".enter_Inspect(text2, ref _inspectedItems, 4).nl_ifNotEntered(ref changed);

            if (_inspectedItems == -1)
            {
                if (!nextOne.IsNullOrEmpty() && icon.Delete.Click("Remove any followups"))
                    nextOne = "";
                
                "Go To".select_iGotDisplayName(60, ref nextOne, Interaction.inspectedList).nl();
            }

            return changed;
        }
        
        #endregion
    }


}
