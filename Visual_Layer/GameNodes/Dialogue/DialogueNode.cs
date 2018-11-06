using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNotes;
using PlayerAndEditorGUI;
using SharedTools_Stuff;
using STD_Logic;
using UnityEngine;

namespace NodeNotes_Visual {
    
    [TaggedType(tag, "Dialogue Node")]
    public class DialogueNode : GameNodeBase {

        public const string tag = "GN_talk";

        public override string ClassTag => tag;

        public InteractionBranch interactionBranch = new InteractionBranch();

        public int myQuestVersion = -1;

        public void UpdateLogic() => myQuestVersion = LogicMGMT.currentLogicVersion;
        
        public void Update() {
            if (myQuestVersion != LogicMGMT.currentLogicVersion)
                UpdateLogic();
        }

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("inBr", interactionBranch);
        
        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "b": data.DecodeInto(base.Decode); break;
                case "inBr": data.DecodeInto(out interactionBranch);  break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector
        #if PEGI

        protected override bool InspectGameNode() {

            bool changed = base.Inspect();

            if ("Interactions Tree".enter(ref inspectedGameNodeStuff, 10).nl())
                interactionBranch.Inspect();

            if ("Play Dialogue ".enter(ref inspectedGameNodeStuff, 13).nl_ifNotEntered()){

                if (icon.Close.Click("Close dialogue", 20))
                    Exit();
                else  {
                    pegi.nl();
                    for (int i = 0; i < _optText.Count; i++)
                    {
                        if (_optText[i].Click().nl())
                        {
                            SelectOption(i);
                            DistantUpdate();
                        }
                    }
                }
            }

            return changed;
        }

        #endif
        #endregion

        #region Options MGMT
        public static string SingleText { get { return _optText.Count > 0 ? _optText[0] : null; } set { _optText.Clear(); _optText.Add(value); } }

        public static List<string> _optText = new List<string>();
        static List<Interaction> possibleInteractions = new List<Interaction>();
        static List<DialogueChoice> possibleOptions = new List<DialogueChoice>();

        public static bool ScrollOptsDirty;

        static bool CheckOptions(Interaction ia)
        {
            ClearTexts();
            int cnt = 0;
            //for (int i = 0; i < tmp.Count; i++)
            foreach (DialogueChoice dio in ia.options)
                if (dio.conditions.IsTrue) {
                    _optText.Add(dio.text.ToString());
                    possibleOptions.Add(dio);
                    cnt++;
                }

            ScrollOptsDirty = true;

            QuestVersion = LogicMGMT.currentLogicVersion;

            if (cnt > 0)
                return true;
            else
                return false;
        }
        
        void CollectInteractions() => CollectInteractions(interactionBranch);

        void CollectInteractions(LogicBranch<Interaction> gr) {
            foreach (Interaction si in gr.elements) {
                    if (si.conditions.IsTrue) {
                        _optText.Add(si.texts[0].ToPEGIstring());
                        possibleInteractions.Add(si);
                       
                    }
            }

            foreach (var sgr in gr.subBranches)
                CollectInteractions(sgr);
        }

        public void BackToInitials() {
            LogicMGMT.AddLogicVersion();
            ClearTexts();
            
            CollectInteractions();

            if (possibleInteractions.Count != 0)
            {

                QuestVersion = LogicMGMT.currentLogicVersion;
                ScrollOptsDirty = true;

                InteractionStage = 0;
                textNo = 0;

                if (continuationReference != null)
                {
                    foreach (var ie in possibleInteractions)
                        if (ie.referanceName == continuationReference)
                        {
                            interaction = ie;
                            InteractionStage++;
                            SelectOption(0);
                            break;
                        }
                }
            }
            else
                Exit();
        }

        protected override void AfterEnter() => BackToInitials();
        
        public static int textNo;
        public static int InteractionStage;

        static Interaction interaction;
        static DialogueChoice option;

        static int QuestVersion;
        public void DistantUpdate()  {
 
                if (QuestVersion != LogicMGMT.currentLogicVersion)  {

                    switch (InteractionStage)
                    {
                        case 0: BackToInitials(); break;
                        case 1: GotBigText(); break;
                        case 3: CheckOptions(interaction); break;
                        case 5:
                            List<Sentance> tmp = option.texts2;
                            if (tmp.Count > textNo)
                                SingleText = tmp[textNo].ToString();
                            break;
                    }
                    QuestVersion = LogicMGMT.currentLogicVersion;
                }
            
        }

        static void ClearTexts()
        {
            _optText.Clear();
            ScrollOptsDirty = true;
            possibleInteractions.Clear();
        }

        static bool GotBigText()
        {
            if (textNo < interaction.texts.Count)
            {
                SingleText = interaction.texts[textNo].ToString();
                return true;
            }
            return false;
        }
        
        static string continuationReference;
        public void SelectOption(int no)
        {
            LogicMGMT.AddLogicVersion();
            switch (InteractionStage)
            {
                case 0:
                    InteractionStage++; interaction = possibleInteractions[no]; goto case 1;
                case 1:
                    continuationReference = null;
                    textNo++;
                    if (GotBigText()) break;
                    InteractionStage++;
                    goto case 2;
                case 2:
                    InteractionStage++;
                    if (!CheckOptions(interaction)) goto case 4; break;
                case 3:
                    option = possibleOptions[no];
                    option.results.Apply();
                    continuationReference = option.nextOne;
                    interaction.finalResults.Apply();
                    textNo = -1;
                    goto case 5;

                case 4:
                    interaction.finalResults.Apply(); BackToInitials(); break;

                case 5:

                    List<Sentance> txts = option.texts2;
                    if ((txts.Count > textNo + 1))
                    {
                        textNo++;
                        SingleText = txts[textNo].ToString();
                        InteractionStage = 5;
                        break;
                    }
                    goto case 6;
                case 6:

                    BackToInitials();
                    break;
            }
        }
        #endregion
    }
}
