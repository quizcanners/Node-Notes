using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes {

    /*public class GameNodeAttribute : AbstractWithTaggedTypes
    {
        public override TaggedTypesCfg TaggedTypes => GameNodeBase.all;
    }

    [GameNode]*/
    [DerivedList]
    public abstract class GameNodeBase : Base_Node, IGotClassTag
    {
        private List<Result> _onExitResults = new List<Result>();
        
        #region Tagged Types MGMT
        public override GameNodeBase AsGameNode => this;
        public abstract string ClassTag { get;  } 
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(GameNodeBase));
        //public TaggedTypesCfg AllTypes => all;
        #endregion

        #region Enter & Exit

        public override bool OnMouseOver(bool click) {
            if (click && parentNode != null)
            {
                VisualLayer.FromNodeToGame(this);
                return true;
            }

            return false;
        }

        protected virtual void OnEnter() { }

        protected virtual void OnExit() { }

        protected LoopLock loopLock = new LoopLock();

        public void Enter() {

            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    var data = Shortcuts.users.current.gameNodesData_PerUser.TryGet(ClassTag);
                    if (data.IsEmpty == false)
                        this.DecodeFull(data);
                    
                    data = parentNode.parentBook.gameNodesConfigs.TryGet(ClassTag);
                    if (data.IsEmpty == false)  
                        this.DecodeFull(data);
                    else
                        Debug.Log("No per book data for. {0}".F(ClassTag));

                    VisualLayer.FromNodeToGame(this);

                    results.Apply();

                    OnEnter();
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

                    Shortcuts.users.current.gameNodesData_PerUser[ClassTag] = Encode_PerUserData().CfgData;
                    parentNode.parentBook.gameNodesConfigs[ClassTag] = Encode_PerBookStaticData().CfgData;

                    VisualLayer.FromGameToNode();

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

                if (_inspectedItems == (int)InspectItems.Logic)
                    ExitResultRole.enter_List(ref _onExitResults, ref _editedExitResult, ref _inspectedItems, 7, ref changed).SetLastUsedTrigger();
                        
                pegi.nl_ifNotEntered();
                
                bool current = Shortcuts.visualLayer.IsCurrentGameNode(this);
                  
                if (GameNodeTypeName.conditional_enter(current, ref _inspectedItems, 8).nl_ifNotEntered())
                    InspectGameNode();
                else if (_inspectedItems == -1 && !current && "Enter game node".Click())
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

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("b", base.Encode)
            .Add_String("unrecGN", "test")
            .Add_IfNotEmpty("exit", _onExitResults)
            .Add_IfNotNegative("ign", inspectedGameNodeItems);

        public override void Decode(string tg, CfgData data) {
            switch (tg) {
                case "b": data.Decode(base.Decode); break; 
                case "exit": data.ToList(out _onExitResults); break;
                case "ign": inspectedGameNodeItems = data.ToInt(0); break;
            }
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
