using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using STD_Logic;
using System.Collections;
using UnityEngine;

namespace NodeNotes {

    public class GameNodeAttribute : Abstract_WithTaggedTypes {
        public override TaggedTypes_STD TaggedTypes => GameNodeBase.all;
    }

    [GameNode]
    [DerrivedList()]
    public abstract class GameNodeBase : Base_Node, IGotClassTag, IPEGI_ListInspect {

        #region Tagged Types MGMT
        public override GameNodeBase AsGameNode => this;
        public virtual string ClassTag => StdEncoder.nullTag;
        public static TaggedTypes_STD all = new TaggedTypes_STD(typeof(GameNodeBase));
        public TaggedTypes_STD AllTypes => all;
        #endregion

        #region Enter & Exit

        public override void OnMouseOver() {
            if (Input.GetMouseButtonDown(0) && parentNode != null)
                VisualLayer.FromNodeToGame(this);
        }

        protected virtual void AfterEnter() { }

        protected virtual void ExitInternal() { }

        protected LoopLock loopLock = new LoopLock();

        public void Enter() {

            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    var data = Shortcuts.user.gameNodeTypeData.TryGet(ClassTag);
                    if (data != null) Decode_PerUserData(data);
                    
                    data = parentNode.root.gameNodeTypeData.TryGet(ClassTag);
                    if (data != null)  
                        Decode_PerBookData(data);
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
                    ExitInternal();
                    VisualLayer.FromGameToNode(true);
                }
        }

        public void Exit() {
            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    ExitInternal();

                   
                    Shortcuts.user.gameNodeTypeData[ClassTag] = Encode_PerUserData().ToString();
                    parentNode.root.gameNodeTypeData[ClassTag] = Encode_PerBookStaticData().ToString();

                    Debug.Log("Saving Data of Game Node {0} : {1}".F(ClassTag, parentNode.root.gameNodeTypeData[ClassTag]));

                    VisualLayer.FromGameToNode(false);

                    onExitResults.Apply();
                }
        }
        #endregion

        #region Inspector
        protected virtual bool InspectGameNode() => false;

        protected override string ResultsRole => "On Enter";

        protected virtual string ExitResultRole => "On Exit";

        int editedExitResult = -1;

        LoopLock inspectLoopLock = new LoopLock();

        public sealed override bool Inspect() {

            bool changed = false;

            if (loopLock.Unlocked)
                using (loopLock.Lock()) {

                    changed |= base.Inspect();

                    changed |= ExitResultRole.enter_List(ref onExitResults, ref editedExitResult, ref inspectedStuff, 7).nl_ifNotEntered();

                    if (ClassTag.enter(ref inspectedStuff, 8).nl_ifNotEntered())
                        InspectGameNode();

                }

            return changed;
        }

        public bool PEGI_inList(IList list, int ind, ref int edited) {

            bool changed = this.inspect_Name();

            if (icon.Play.Click("Enter Game Node"))
                VisualLayer.FromNodeToGame(this);

            return changed;
        }

        #endregion

        #region Encode & Decode

        public List<Result> onExitResults = new List<Result>();

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add("exit", onExitResults);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "b": data.DecodeInto(base.Decode); break;
                case "exit": data.Decode_List(out onExitResults); break;
                default: return false;
            }
            return true;
        }

        // Per User data will be Encoded/Decoded each time the node is Entered during play
        public virtual StdEncoder Encode_PerUserData() => new StdEncoder();
        public virtual bool Decode_PerUser(string tag, string data) => true;
        public virtual void Decode_PerUserData(string data) => data.DecodeInto(Decode_PerUser);

        // Per Node Book Data: Data will be encoded each time Node Book is Saved
        public virtual StdEncoder Encode_PerBookStaticData() => new StdEncoder();
        public virtual bool Decode_PerBookStatic(string tag, string data) => true;
        public virtual void Decode_PerBookData(string data) => data.DecodeInto(Decode_PerBookStatic);
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
