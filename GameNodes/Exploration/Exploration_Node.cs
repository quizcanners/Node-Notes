using NodeNotes;
using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace NodeNotes_Visual.Exploration {

    [TaggedType(tag, "Exploration Node")]
    public class Exploration_Node : GameNodeBase {

        public const string tag = "GN_wrld";

        public override string ClassTag => tag;

        Vector3 entryPointPosition = Vector3.zero;

        static Vector3 playerPosition = Vector3.zero;

        public static List<MonoBehaviour> monoBehaviourPrefabs = new List<MonoBehaviour>();

        static List<Exploration_Element> instances = new List<Exploration_Element>();
        static List_Data instancesMeta = new List_Data("Instances");


        protected override void OnExit() {
            foreach (var i in instances)
                i.OnExit();
        }

        #region Encode & Decode

        public override bool Decode(string tag, string data)
        {
            switch (tag) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "pos": playerPosition = data.ToVector3(); break;
                case "expl": data.Decode_References(out monoBehaviourPrefabs); break;
                case "els": data.Decode_List(out instances, ref instancesMeta); break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode);
        
        public override StdEncoder Encode_PerUserData() 
            => new StdEncoder().Add("pos", playerPosition);

        public override StdEncoder Encode_PerBookStaticData() 
            => new StdEncoder()
            .Add_References("expl", monoBehaviourPrefabs)
            .Add("els", instances, instancesMeta);
        #endregion

        #region Inspector
        int inspectedPrefab = -1;
        protected override bool InspectGameNode() {
            var changed = false;

            "Prefabs".enter_List_UObj(ref monoBehaviourPrefabs,ref inspectedPrefab , ref inspectedGameNodeStuff, 0).nl(ref changed);

            instancesMeta.enter_List(ref instances, ref inspectedGameNodeStuff, 1).nl(ref changed);

            if ("Entity Manager".enter(ref inspectedGameNodeStuff, 2).nl())
                NodeNotesECSManager.Inspect();

            return changed;
        }

        #endregion

    }

    [DerrivedList(typeof(Exploration_MonoInstance), typeof(Exploration_ECSinstance))]
    public class Exploration_Element : AbstractKeepUnrecognized_STD  {

        public virtual void OnExit() { }

    }

    public class Exploration_ECSinstance : Exploration_Element, IPEGI_ListInspect, IGotName, IPEGI {
        string name = "Unnamed";
        string instanceConfig;
        int instanceIndex = -1;

        List<EntityDataSTD_Base> entityComponents = new List<EntityDataSTD_Base>();
        List_Data componentsMeta = new List_Data("Components");

        static EntityManager Manager => NodeNotesECSManager.manager;

        Entity instance;

        public override void OnExit() => DestroyInstance();

        void DestroyInstance() {
            if (instanceIndex != -1) {
                instanceIndex = -1;
                Manager.DestroyEntity(instance);
            }
        }

        void Instantiate() {
            instance = NodeNotesECSManager.Instantiate(entityComponents);
            instanceIndex = instance.Index;
        }

        #region Encode & Decode
        public override void Decode(string data) {
            if (Manager == null)
                NodeNotesECSManager.Init();
            base.Decode(data);
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add_IfNotEmpty("cfg", instanceConfig)
            .Add("ent", entityComponents, componentsMeta, EntityDataSTD_Base.all);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": name = data; break;
                case "cfg": instanceConfig = data; break;
                case "ent": data.Decode_List(out entityComponents, ref componentsMeta, EntityDataSTD_Base.all); break;
                default: return false;
            }
            return true;
        }

        #endregion

        #region Inspector
        public string NameForPEGI { get { return name; } set { name = value; } }

        #if PEGI
        public bool PEGI_inList(IList list, int ind, ref int edited) {

            var changed = this.inspect_Name();

            var active = instanceIndex != -1;

            if ((active ? icon.Active : icon.InActive).Click("Inspect"))
                edited = ind;
            
           if (!active && icon.Play.Click())
                Instantiate();

            if (active && icon.Delete.Click("Delete instance"))
                DestroyInstance();

            return changed;
        }

        public override bool Inspect() {

            var changed = false;

            var active = instanceIndex != -1;

            if (active) {
                var cmps = instance.GetComponentTypes();
                "Got {0} components".F(cmps.Length);
            }

            if (!active && icon.Play.Click())
                Instantiate();

            if (active && icon.Delete.Click("Delete instance"))
                DestroyInstance();

            pegi.nl();

            if (componentsMeta.enter_List(ref entityComponents, ref inspectedStuff, 1).nl(ref changed) && active) 
                foreach (var e in entityComponents)
                    e.SetData(instance);
 
            return changed;
        }
        #endif
        #endregion
    }

    public class Exploration_MonoInstance : Exploration_Element, IPEGI_ListInspect, IGotName, IManageFading
    {
        string name = "Unnamed";
        public int prefabIndex = -1;
        string instanceConfig;
        MonoBehaviour instance;

        #region Encode & Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("n", name)
            .Add("ind", prefabIndex)
            .Add_IfNotEmpty("cfg", instanceConfig);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "n": name = data; break;
                case "ind": prefabIndex = data.ToInt(); break;
                case "cfg": instanceConfig = data; break;
                default: return false;
            }
            return true;
        }

        #endregion
        
        #region Inspector
        public string NameForPEGI { get { return name; } set { name = value; } }

        #if PEGI
        public bool PEGI_inList(IList list, int ind, ref int edited) {

            var changed = this.inspect_Name();

            if ((instance ? icon.Active : icon.InActive).Click("Inspect"))
                edited = ind;

            instance.clickHighlight();

            if (!instance) {
                var el = Exploration_Node.monoBehaviourPrefabs.TryGet(prefabIndex);
                if (el && icon.Play.Click())
                    TryFadeIn();
            }
            else if (icon.Close.Click())
                FadeAway();

            return changed;
        }

        public override bool Inspect() {

            var changed = false;

            "Prefab".select(ref prefabIndex, Exploration_Node.monoBehaviourPrefabs);

            if (instance)
                instance.Try_Nested_Inspect().nl(ref changed);

            return changed;
        }


#endif
        #endregion
        
        public void FadeAway() {

            if (instance) {

                var std = instance as ISTD;
                if (std != null)
                    instanceConfig = std.Encode().ToString(); 

                var ifd = instance as IManageFading;
                if (ifd != null)
                    ifd.FadeAway();
                else {
                    instance.DestroyWhatever();
                    instance = null;
                }
            }
        }

        public bool TryFadeIn() {

            bool fadedIn = false;

            if (instance) {
                var ifd = instance as IManageFading;
                if (ifd != null) {
                    if (ifd.TryFadeIn())
                        fadedIn = true;
                    else
                        instance = null;
                }
            }

            if (!instance) {
                var el = Exploration_Node.monoBehaviourPrefabs.TryGet(prefabIndex);
                if (el)
                    instance = Object.Instantiate(el);
            }

            if (instance) {
                instance.gameObject.SetActive(true);
                fadedIn = true;
            }

            if (fadedIn) {
                var std = instance as ISTD;
                if (std != null)
                    std.Decode(instanceConfig);
            }

            return fadedIn;
        }

        public override void OnExit() => FadeAway();
    }

}