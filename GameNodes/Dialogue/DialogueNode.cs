using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using STD_Logic;
namespace NodeNotes_Visual {
    
    [TaggedType(Tag, "Dialogue Node")]
    public class DialogueNode : GameNodeBase  {
        private const string Tag = "GN_talk";

        public override string ClassTag => Tag;

        public InteractionBranch interactionBranch = new InteractionBranch();

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("inBr", interactionBranch);
        
        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break; 
                case "inBr": data.DecodeInto(out interactionBranch);  break;
                default: return false;
            }
            return true;
        }
        #endregion

        #region Inspector
        
        #if PEGI

        protected override string GameNodeTypeName => "Dialogue";

        public static DialogueNode inspected;

        protected override bool InspectGameNode() {
            inspected = this;

            var changed = Inspect();

            "{0} Dialogue".F(name).write();

            if (icon.Play.enter(ref inspectedGameNodeStuff, 13).nl_ifNotEntered())
            {

                if (icon.Refresh.Click("Restart dialogue", 20))
                    BackToInitials();
                else
                {
                    DistantUpdate();
                    pegi.nl();
                    for (var i = 0; i < OptText.Count; i++)
                        if (OptText[i].Click(13).nl())
                        {
                            SelectOption(i);
                            DistantUpdate();
                        }

                }
            }
            else
                interactionBranch.Nested_Inspect().nl(ref changed);

            inspected = null;

            return changed;
        }

        #endif
        #endregion

        #region Options MGMT

        private static string SingleText {
            get { return OptText.Count > 0 ? OptText[0] : null; }
            set { OptText.Clear(); OptText.Add(value); } }

        private static readonly List<string> OptText = new List<string>();
        private static readonly List<Interaction> PossibleInteractions = new List<Interaction>();
        private static readonly List<DialogueChoice> PossibleOptions = new List<DialogueChoice>();

        private static bool CheckOptions(Interaction ia)
        {
            ClearTexts();
            var cnt = 0;
            foreach (var dio in ia.options)
                if (dio.conditions.IsTrue) {
                    OptText.Add(dio.text.ToString());
                    PossibleOptions.Add(dio);
                    cnt++;
                }


            _questVersion = LogicMGMT.currentLogicVersion;

            return cnt > 0;
        }

        private void CollectInteractions() => CollectInteractions(interactionBranch);

        private void CollectInteractions(LogicBranch<Interaction> gr) {
            if (!gr.IsTrue()) return;
            
            foreach (var si in gr.elements)  {
                if (!si.IsTrue()) continue;
                OptText.Add(si.texts[0].ToPegiString());
                PossibleInteractions.Add(si);
            }

            foreach (var sgr in gr.subBranches)
                CollectInteractions(sgr);
        }

        private void BackToInitials() {
            LogicMGMT.AddLogicVersion();
            ClearTexts();
            
            CollectInteractions();

            if (PossibleInteractions.Count != 0) {

                _questVersion = LogicMGMT.currentLogicVersion;
              
                _interactionStage = 0;
                _textNo = 0;

                if (continuationReference.IsNullOrEmpty()) return;
                
                foreach (var ie in PossibleInteractions)
                    if (ie.referenceName.SameAs(continuationReference)) {
                        _interaction = ie;
                        _interactionStage++;
                        SelectOption(0);
                        break;
                    }
            }
            else
                Exit();
        }

        protected override void AfterEnter() => BackToInitials();

        private static int _textNo;
        private static int _interactionStage;

        static Interaction _interaction;
        static DialogueChoice _option;

        static int _questVersion;

        private void DistantUpdate()  {
            if (_questVersion == LogicMGMT.currentLogicVersion) return;
            
            switch (_interactionStage) {

                case 0: BackToInitials(); break;
                case 1: GotBigText(); break;
                case 3: CheckOptions(_interaction); break;
                case 5:
                    var sntx = _option.texts2.GetNextText(ref _textNo);

                    if (sntx != null)
                        SingleText = sntx.ToString();
                        
                    break;
            }

            _questVersion = LogicMGMT.currentLogicVersion;
        }

        private static void ClearTexts() {
            OptText.Clear();
            PossibleInteractions.Clear();
            PossibleOptions.Clear();
        }

        private static bool GotBigText()
        {

            var txt = _interaction.texts.GetNextText(ref _textNo);

            if (txt == null) return false;
            
            SingleText = txt.ToString();
            return true;

        }
        
        static string continuationReference;

        private void SelectOption(int no)
        {
            LogicMGMT.AddLogicVersion();
            switch (_interactionStage)
            {
                case 0:
                    _interactionStage++; _interaction = PossibleInteractions[no]; goto case 1;
                case 1:
                    continuationReference = null;
                    _textNo++;
                    if (GotBigText()) break;
                    _interactionStage++;
                    goto case 2;
                case 2:
                    _interactionStage++;
                    if (!CheckOptions(_interaction)) goto case 4; break;
                case 3:
                    _option = PossibleOptions[no];
                    _option.results.Apply();
                    continuationReference = _option.nextOne;
                    _interaction.finalResults.Apply();
                    _textNo = -1;
                    goto case 5;

                case 4:
                    _interaction.finalResults.Apply(); BackToInitials(); break;
                case 5:

                    _textNo++;

                    var sentence = _option.texts2.GetNextText(ref _textNo);
                    if (sentence != null)
                        SingleText = sentence.ToString();
                    else
                        goto case 6;

                    break;

                case 6:

                    BackToInitials();
                    break;
            }
        }

      
        #endregion
    }
}
