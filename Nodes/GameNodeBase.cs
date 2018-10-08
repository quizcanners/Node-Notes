using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTools_Stuff;
using PlayerAndEditorGUI;

namespace NodeNotes
{

    [GameNode()]
    [DerrivedList()]
    public class GameNodeBase : Base_Node {

        public static List<string> allKeys = null;

        static Dictionary<string, Type> allGameNodes = null;

        public static void RefreshNodeTypesList()
        {
            allGameNodes = new Dictionary<string, Type>();

            allKeys = new List<string>();

            List<Type> allTypes = CsharpFuncs.GetAllChildTypesOf<GameNodeBase>();

            foreach (var t in allTypes)
            {
                var att = t.ClassAttribute<GameNodeAttribute>();

                if (att != null)
                {
                    allKeys.Add(att.tag);
                    allGameNodes.Add(att.tag, t);
                }

            }
        }

        public static Dictionary<string, Type> AllGameNodes { get {

                if (allGameNodes == null)
                    RefreshNodeTypesList();

            return allGameNodes;
            }
        } 
        
        public override GameNodeBase AsGameNode => this;

        public virtual string UniqueTag => "Is A Base Class";

        public virtual void Enter() { }

        public void Exit() =>  CurrentNode = parentNode;
        
    }


   
    [AttributeUsage(AttributeTargets.Class)]
    public class GameNodeAttribute : Attribute, IAttributeWithTaggetTypes_STD {

       public string tag;

        public GameNodeAttribute(string ntag) {
            tag = ntag;
        }

        public GameNodeAttribute() { }
        
        public List<string> AllTags() {

            if (GameNodeBase.allKeys == null)
                GameNodeBase.RefreshNodeTypesList();

            return GameNodeBase.allKeys;
        }

        public Type GetType(string tag)  {
            Type t;
            GameNodeBase.AllGameNodes.TryGetValue(tag, out t);
            return t;
        }
    }

}
