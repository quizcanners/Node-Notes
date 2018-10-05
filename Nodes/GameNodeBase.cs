using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTools_Stuff;
using PlayerAndEditorGUI;

namespace NodeNotes
{

    public class GameNodeBase : Base_Node {

        public static Dictionary<string, Type> allGameNodes = new Dictionary<string, Type>();

        public virtual string UniqueTag => "Is A Base Class";

        public virtual void Enter() { }

        public void Exit() {
            CurrentNode = parentNode;
        }

       /* public override bool PEGI() {
            var changed = base.PEGI();



        }*/

    }


    [AttributeUsage(AttributeTargets.Class)]
    public class GameNodeAttribute : Attribute {

        public GameNodeAttribute(string tag, Type type) =>
            GameNodeBase.allGameNodes.Add(tag, type);
    }

}
