using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTools_Stuff;
using PlayerAndEditorGUI;

namespace NodeNotes {

    public class GameNodeAttribute : Abstract_WithTaggedTypes {
        public override TaggedTypes_STD TaggedTypes => GameNodeBase.all;
    }

    [GameNode]
    [DerrivedList()]
    public abstract class GameNodeBase : Base_Node, IGotClassTag {
        public override GameNodeBase AsGameNode => this;
        
        public virtual void Enter() { }

        public void Exit() =>  CurrentNode = parentNode;

        public virtual string ClassTag => StdEncoder.nullTag;
        public static TaggedTypes_STD all = new TaggedTypes_STD(typeof(GameNodeBase));
        public TaggedTypes_STD AllTypes => all;
    }

    [TaggedType(classTag)]
    public class GameNodeTest0 : GameNodeBase {
        const string classTag = "test0";

        public override string ClassTag => classTag;

    }

   // [TaggedType(classTag)]
    public class GameNodeTest1 : GameNodeBase
    {
        const string classTag = "test1";

        public override string ClassTag => classTag;

    }

    //[TaggedType(classTag)]
    public class GameNodeTest2 : GameNodeBase
    {
        const string classTag = "test2";

        public override string ClassTag => classTag;

    }

}
