using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using STD_Logic;
using System.Collections;
using UnityEngine;

namespace NodeNotes {

    public class GameNodeAttribute : AbstractWithTaggedTypes
    {
        public override TaggedTypesStd TaggedTypes => GameNodeBase.all;
    }

    [GameNode]
    [DerivedList()]
    public abstract class GameNodeBase : Base_Node, IGotClassTag
    {
        private List<Result> _onExitResults = new List<Result>();
        
        #region Tagged Types MGMT
        public override GameNodeBase AsGameNode => this;
        public abstract string ClassTag { get;  } 
        public static TaggedTypesStd all = new TaggedTypesStd(typeof(GameNodeBase));
        public TaggedTypesStd AllTypes => all;
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
                    
                    data = parentNode.root.gameNodesData.TryGet(ClassTag);
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
            Debug.Log("Exiting Game Node Without Saving");

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
                    parentNode.root.gameNodesData[ClassTag] = Encode_PerBookStaticData().ToString();

                    Debug.Log("Saving Data of Game Node {0} : {1}".F(ClassTag, parentNode.root.gameNodesData[ClassTag]));

                    VisualLayer.FromGameToNode(false);

                    _onExitResults.Apply();
                }
        }
        #endregion

        #region Inspector
      
        protected virtual bool InspectGameNode() => false;

        protected override string ResultsRole => "On Enter";

        protected virtual string ExitResultRole => "On Exit";

        public override void ResetInspector() {
            inspectedGameNodeStuff = -1;
            _editedExitResult = -1;

            base.ResetInspector();
        }

        protected int inspectedGameNodeStuff = -1;
        private int _editedExitResult = -1;

        private readonly LoopLock _inspectLoopLock = new LoopLock();

#if PEGI

        protected virtual string GameNodeTypeName => ClassTag; 
        
        public sealed override bool Inspect() {

            if (!Shortcuts.visualLayer.IsCurrentGameNode(this))
                Shortcuts.visualLayer.FromNodeToGame(this);

            var changed = false;

            if (!_inspectLoopLock.Unlocked) return changed;
            
            using (_inspectLoopLock.Lock()) {

                changed |= base.Inspect();

                if (showLogic)
                    ExitResultRole.enter_List(ref _onExitResults, ref _editedExitResult, ref inspectedStuff, 7, ref changed).SetLastUsedTrigger();
                        
                pegi.nl_ifNotEntered();

                if (GameNodeTypeName.enter(ref inspectedStuff, 8).nl_ifNotEntered())
                    InspectGameNode();

            }

            return changed;
        }
        
        public override bool PEGI_inList(IList list, int ind, ref int edited) {

            IndexForPEGI.ToString().write(20);

            bool changed = this.inspect_Name();

            if (icon.Play.Click("Enter Game Node"))
                VisualLayer.FromNodeToGame(this);

            return changed;
        }

#endif

        #endregion

        #region Encode & Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("unrecGN", "test")
            .Add_IfNotEmpty("exit", _onExitResults)
            .Add_IfNotNegative("ign", inspectedGameNodeStuff);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "exit": data.Decode_List(out _onExitResults); break;
                case "ign": inspectedGameNodeStuff = data.ToInt(); break;
                default: return false;
            }
            return true;
        }

        // Per User data will be Encoded/Decoded each time the node is Entered during play
        public virtual StdEncoder Encode_PerUserData() => new StdEncoder();

        // Per Node Book Data: Data will be encoded each time Node Book is Saved
        public virtual StdEncoder Encode_PerBookStaticData() => new StdEncoder();
        #endregion
    }

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

    }

}
