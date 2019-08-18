using System.Collections.Generic;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using QcTriggerLogic;
using System.Collections;
using UnityEngine;

namespace NodeNotes {

    public class GameNodeAttribute : AbstractWithTaggedTypes
    {
        public override TaggedTypesCfg TaggedTypes => GameNodeBase.all;
    }

    [GameNode]
    [DerivedList()]
    public abstract class GameNodeBase : Base_Node, IGotClassTag
    {
        private List<Result> _onExitResults = new List<Result>();
        
        #region Tagged Types MGMT
        public override GameNodeBase AsGameNode => this;
        public abstract string ClassTag { get;  } 
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(GameNodeBase));
        public TaggedTypesCfg AllTypes => all;
        #endregion

        #region Enter & Exit

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && parentNode != null)
                VisualLayer.FromNodeToGame(this);
        }

        protected virtual void AfterEnter() { }

        protected virtual void OnExit() { }

        protected LoopLock loopLock = new LoopLock();

        public void Enter() {

            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    var data = Shortcuts.user.gameNodesData.TryGet(ClassTag);
                    if (data != null)
                        Decode(data);
                    
                    data = parentNode.parentBook.gameNodesData.TryGet(ClassTag);
                    if (data != null)  
                        Decode(data);
                    else
                        Debug.Log("No per book data for. {0}".F(ClassTag));

                    VisualLayer.FromNodeToGame(this);

                    results.Apply();

                    AfterEnter();
                }
        }

        public void FailExit() {
            //Debug.Log("Exiting Game Node Without Saving");

            if (loopLock.Unlocked)
                using (loopLock.Lock()) {
                    OnExit();
                    VisualLayer.FromGameToNode(true);
                }
        }

        public void Exit() {
            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    OnExit();
                    
                    Shortcuts.user.gameNodesData[ClassTag] = Encode_PerUserData().ToString();
                    parentNode.parentBook.gameNodesData[ClassTag] = Encode_PerBookStaticData().ToString();

                    Debug.Log("Saving Data of Game Node {0} : {1}".F(ClassTag, parentNode.parentBook.gameNodesData[ClassTag]));

                    VisualLayer.FromGameToNode(false);

                    _onExitResults.Apply();
                }
        }
        #endregion

        #region Inspector
      
        protected virtual bool InspectGameNode() => false;
        
        protected int inspectedGameNodeItems = -1;

        private readonly LoopLock _inspectLoopLock = new LoopLock();
        
        protected override string ResultsRole => "On Enter";

        protected virtual string ExitResultRole => "On Exit";

        public override void ResetInspector() {
            inspectedGameNodeItems = -1;
            _editedExitResult = -1;

            base.ResetInspector();
        }

        private int _editedExitResult = -1;

        protected virtual string GameNodeTypeName => ClassTag; 
        
        public sealed override bool Inspect() {
            
            var changed = false;

            if (!_inspectLoopLock.Unlocked) return changed;
            
            using (_inspectLoopLock.Lock()) {

                changed |= base.Inspect();

                if (showLogic)
                    ExitResultRole.enter_List(ref _onExitResults, ref _editedExitResult, ref inspectedItems, 7, ref changed).SetLastUsedTrigger();
                        
                pegi.nl_ifNotEntered();
                
                bool current = Shortcuts.visualLayer.IsCurrentGameNode(this);
                  
                if (GameNodeTypeName.conditional_enter(current, ref inspectedItems, 8).nl_ifNotEntered())
                    InspectGameNode();
                else if (inspectedItems == -1 && !current && "Enter game node".Click())
                    Shortcuts.visualLayer.FromNodeToGame(this);

            }

            return changed;
        }
        
        public override bool InspectInList(IList list, int ind, ref int edited) {

            IndexForPEGI.ToString().write(20);

            bool changed = this.inspect_Name();

            if (icon.Play.Click("Enter Game Node"))
                VisualLayer.FromNodeToGame(this);

            return changed;
        }

        #endregion

        #region Encode & Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("unrecGN", "test")
            .Add_IfNotEmpty("exit", _onExitResults)
            .Add_IfNotNegative("ign", inspectedGameNodeItems);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "exit": data.Decode_List(out _onExitResults); break;
                case "ign": inspectedGameNodeItems = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        // Per User data will be Encoded/Decoded each time the node is Entered during play
        public virtual CfgEncoder Encode_PerUserData() => new CfgEncoder();

        // Per Node Book Data: Data will be encoded each time Node Book is Saved
        public virtual CfgEncoder Encode_PerBookStaticData() => new CfgEncoder();
        #endregion
    }


    /*
    [TaggedType(classTag)]
    public class GameNodeTest0 : GameNodeBase {
        const string classTag = "test0";

        public override string ClassTag => classTag;

    }

    [TaggedType(classTag, "Test Node 1")]
    public class GameNodeTest1 : GameNodeBase
    {
        const string classTag = "testSame";

        public override string ClassTag => classTag;

    }

    [TaggedType(classTag, "test2")]
    public class GameNodeTest2 : GameNodeBase
    {
        const string classTag = "test2";

        public override string ClassTag => classTag;

    }*/

}
