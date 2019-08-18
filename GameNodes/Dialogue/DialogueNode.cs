using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using QcTriggerLogic;

namespace NodeNotes_Visual {
    
    [TaggedType(Tag, "Dialogue Node")]
    public class DialogueNode : GameNodeBase
    {

        public static DialogueNode enteredInstance;

        private static DialogueUI View => DialogueUI.instance;

        private const string Tag = "GN_talk";

        public override string ClassTag => Tag;

        public InteractionBranch interactionBranch = new InteractionBranch();

        public static DialogueNode inspected;

        public override bool Conditions_isVisible() {

            var vis = base.Conditions_isVisible();

            if (vis) {
                CollectInteractions(interactionBranch);
                vis &= PossibleInteractions.Count > 0;
            }

            return vis;
        }

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
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

        protected override string GameNodeTypeName => "Dialogue";
        
        protected override bool InspectGameNode() {
            inspected = this;

            var changed = Inspect();

            pegi.nl();
            
            if ("Play In Inspector".enter(ref inspectedGameNodeItems, 13).nl()) {

                "Playing {0} Dialogue".F(name).write();

                if (icon.Refresh.Click("Restart dialogue", 20))
                    BackToInteractionSelection();
                else {
                    DistantUpdate();
                    pegi.nl();
                    for (var i = 0; i < OptText.Count; i++)
                        if (OptText[i].Click(13).nl()) {
                            SelectOption(i);
                            DistantUpdate();
                        }

                }
            }

            if ("Interactions tree".enter(ref inspectedGameNodeItems, 14).nl_ifNotEntered())
                interactionBranch.Nested_Inspect().nl(ref changed);


            "Game View UI"
                .conditional_enter_inspect(DialogueUI.instance, DialogueUI.instance, ref inspectedGameNodeItems, 15)
                .nl(ref changed);

            if (inspectedGameNodeItems == -1)
                "Interaction stage: {0}".F(_interactionStage).nl();
            
            inspected = null;

            return changed;
        }

        #endregion

        #region Options MGMT

        private static string SingleText { set {
                OptText.Clear(); 
                View.SingleText = value;
            } }

        private static readonly List<string> OptText = new List<string>();
        private static readonly List<Interaction> PossibleInteractions = new List<Interaction>();
        private static readonly List<DialogueChoice> PossibleOptions = new List<DialogueChoice>();

        private static bool CheckOptions(Interaction ia) {

            ClearTexts();
            var cnt = 0;
            foreach (var dio in ia.options)
                if (dio.conditions.IsTrue) {
                    OptText.Add(dio.text.NameForPEGI);
                    PossibleOptions.Add(dio);
                    cnt++;
                }
            
            _questVersion = LogicMGMT.CurrentLogicVersion;
            
            View.Options = OptText;

            return cnt > 0;
        }

        private void CollectInteractions(LogicBranch<Interaction> gr) {

            if (!gr.IsTrue()) return;
            
            foreach (var si in gr.elements)  {

                si.ResetSentences();

                if (!si.IsTrue())
                    continue;
                
                OptText.Add(si.texts.NameForPEGI);
                PossibleInteractions.Add(si);
            }

            foreach (var sgr in gr.subBranches)
                CollectInteractions(sgr);
        }

        private void BackToInteractionSelection() {

            LogicMGMT.AddLogicVersion();
            ClearTexts();

            CollectInteractions(interactionBranch);

            if (PossibleInteractions.Count != 0) {

                _questVersion = LogicMGMT.CurrentLogicVersion;
              
                _interactionStage = 0;

                if (!continuationReference.IsNullOrEmpty()) {
                    foreach (var ie in PossibleInteractions)
                        if (ie.ReferenceName.SameAs(continuationReference)) {
                            _interaction = ie;
                            _interactionStage++;
                            SelectOption(0);
                            break;
                        }
                }

                if (View && _interactionStage == 0) {

                    var lst = new List<string>();

                    foreach (var interaction in PossibleInteractions)
                        lst.Add(interaction.texts.NameForPEGI);

                    View.Options = lst;
                }

            }
            else
                Exit();
        }
        
        protected override void OnEnter()
        {
            enteredInstance = this;
            DialogueUI.instance.TryFadeIn();
            BackToInteractionSelection();
            
        }

        protected override void OnExit() {
            DialogueUI.instance.FadeAway();

        }

        private static int _interactionStage;

        static Interaction _interaction;
        static DialogueChoice _option;

        static int _questVersion;

        private void DistantUpdate()  {

            if (_questVersion == LogicMGMT.CurrentLogicVersion) return;
            
            switch (_interactionStage) {

                case 0: BackToInteractionSelection(); break;
                case 1: SingleText = _interaction.texts.NameForPEGI;  break;
                case 3: CheckOptions(_interaction); break;
                case 5: SingleText = _option.text2.NameForPEGI; break;
            }

            _questVersion = LogicMGMT.CurrentLogicVersion;
        }

        private static void ClearTexts() {
            OptText.Clear();
            PossibleInteractions.Clear();
            PossibleOptions.Clear();
        }
        
        static string continuationReference;

        public static void SelectOption(int no)
        {
            LogicMGMT.AddLogicVersion();
            switch (_interactionStage) {
                case 0:
                    _interactionStage++; _interaction = PossibleInteractions.TryGet(no);
                    goto case 1;
                case 1:
                    continuationReference = null;

                    if (_interaction == null)
                        SingleText = "No Possible Interactions.";
                    else {
                        if (_interaction.texts.GotNextText) {
                            SingleText = _interaction.texts.GetNext();
                            break;
                        }

                        _interactionStage++;


                        goto case 2;
                    }

                    break;
                case 2:
                    _interactionStage++;
                    if (!CheckOptions(_interaction)) goto case 4; break;
                case 3:
                    _option = PossibleOptions[no];
                    _option.results.Apply();
                    _interaction.finalResults.Apply();
                    continuationReference = _option.nextOne;
                    goto case 5;

                case 4:
                    _interaction.finalResults.Apply(); enteredInstance.BackToInteractionSelection(); break;
                case 5:
                    if (_option.text2.GotNextText) {
                        SingleText = _option.text2.GetNext();
                        _interactionStage = 5;
                    }
                    else
                        goto case 6;

                    break;

                case 6:
                    enteredInstance.BackToInteractionSelection();
                    break;
            }
        }
        
        #endregion
    }
}
