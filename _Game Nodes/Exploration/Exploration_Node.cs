using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using NodeNotes_Visual.ECS;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using Unity.Entities;
using UnityEngine;

namespace NodeNotes_Visual {

#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0018 // Inline variable declaration


    [TaggedType(tag, "Exploration Node")]
    public class Exploration_Node : GameNodeBase {

        public const string tag = "GN_wrld";

        public override string ClassTag => tag;

        Vector3 entryPointPosition = Vector3.zero;

        static Vector3 playerPosition = Vector3.zero;

        public static List<MonoBehaviour> monoBehaviourPrefabs = new List<MonoBehaviour>();

        static List<Exploration_Element> entityConfigurations = new List<Exploration_Element>();
        static ListMetaData entityConfigurationsMeta = new ListMetaData("ECS & Mono instances");

        protected override void OnEnter() {

            if (World.DefaultGameObjectInjectionWorld == null)
            { 
                World.DefaultGameObjectInjectionWorld = new World("Node Notes");
                Debug.LogError("Recreating world");
            }

            foreach (var i in entityConfigurations)
                i.OnEnter();

        }

        protected override void OnExit() {
            
            foreach (var i in entityConfigurations)
                i.OnExit();
        }

        #region Encode & Decode

        public override bool Decode(string tg, string data)
        {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "pos": playerPosition = data.ToVector3(); break;
                case "expl": data.Decode_References(out monoBehaviourPrefabs); break;
                case "els": data.Decode_List_Derived(out entityConfigurations, ref entityConfigurationsMeta); break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode);
        
        public override CfgEncoder Encode_PerUserData() 
            => new CfgEncoder().Add("pos", playerPosition);

        public override CfgEncoder Encode_PerBookStaticData() 
            => new CfgEncoder()
            .Add_References("expl", monoBehaviourPrefabs)
            .Add("els", entityConfigurations, entityConfigurationsMeta);
        #endregion

        #region Inspector
        
        int inspectedPrefab = -1;

        protected override bool InspectGameNode() {
            var changed = false;

            pegi.nl();

            if (NodeNotesECSManager.Manager == null)
                NodeNotesECSManager.Inspect().nl(ref changed);
            else {

                "Mono Prefabs"
                    .enter_List_UObj(ref monoBehaviourPrefabs, ref inspectedPrefab, ref inspectedGameNodeItems, 0)
                    .nl(ref changed);

                entityConfigurationsMeta.enter_List(ref entityConfigurations, ref inspectedGameNodeItems, 1)
                    .nl(ref changed);

                if ("Entity Manager".enter(ref inspectedGameNodeItems, 2).nl())
                    NodeNotesECSManager.Inspect();
            }

            return changed;
        }
        
        #endregion

    }

    [DerivedList(typeof(Exploration_MonoInstance), typeof(Exploration_ECSinstance))]
    public abstract class Exploration_Element : AbstractKeepUnrecognizedCfg  {

        public bool instantiateDirectly;

        public virtual void OnEnter()
        {
            if (instantiateDirectly)
                Instantiate();

        }

        protected virtual void Instantiate(){ }

        public virtual void OnExit() { }

        #region Encode & Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfTrue("inst", instantiateDirectly);

        public override bool Decode(string tg, string data)
        {
            switch (tg) {
                case "inst": instantiateDirectly = data.ToBool(); break;
                default: return false;
            }
            return true;
        }

        #endregion

        #region Inspector

        public override bool Inspect()
        {
            var changed = "Instantiate directly".toggleIcon(ref instantiateDirectly).nl();


            return changed;
        }

        #endregion

    }

    public class Exploration_ECSinstance : Exploration_Element, IPEGI_ListInspect, IGotName, IPEGI {
        string name = "Unnamed";
        string instanceConfig;
        bool instanciated;

        List<ComponentCfgAbstract> entityComponents = new List<ComponentCfgAbstract>();
        ListMetaData componentsMeta = new ListMetaData("Components");
        EntityArchetype archetype;

        static EntityManager Manager => NodeNotesECSManager.Manager;

        Entity instance;

        public override void OnExit() => DestroyInstance();

        void DestroyInstance() {
            if (instanciated) {
                instanciated = false;
                Manager?.DestroyEntity(instance);
            }
        }

        protected override void Instantiate() {

            if (Manager != null) {

                if (!archetype.Valid)
                    archetype = entityComponents.ToArchetype();

                instance = entityComponents.Instantiate();
                instanciated = true;
            } else 
                Debug.LogError("Manager is null");
        }

#region Encode & Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("n", name)
            .Add_IfNotEmpty("cfg", instanceConfig)
            .Add("ent", entityComponents, componentsMeta, ComponentCfgAbstract.all);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
                case "n": name = data; break;
                case "cfg": instanceConfig = data; break;
                case "ent": data.Decode_List(out entityComponents, ref componentsMeta, ComponentCfgAbstract.all); break;
        
                default: return false;
            }
            return true;
        }

#endregion

#region Inspector
        public string NameForPEGI { get { return name; } set { name = value; } }
        
        public bool InspectInList(IList list, int ind, ref int edited) {

            if (instanciated && icon.Clear.Click("Delete instance"))
                DestroyInstance();
            
            var changed = this.inspect_Name();
            
            if ((instanciated ? icon.Active : icon.InActive).Click("Inspect"))
                edited = ind;
            
           if (!instanciated && icon.Play.Click())
                Instantiate();


            return changed;
        }

        public override bool Inspect() {

            var changed = base.Inspect();

            if (instanciated) {
                var cmps = instance.GetComponentTypes();
                "Got {0} components".F(cmps.Length);
            }

            if (!instanciated && icon.Play.Click())
                Instantiate();

            if (instanciated && icon.Delete.Click("Delete instance"))
                DestroyInstance();

            pegi.nl();

            ComponentCfgAbstract.inspectedEntity = instance;

            if (componentsMeta.enter_List(ref entityComponents, ref _inspectedItems, 1).nl(ref changed)) {

                archetype = entityComponents.ToArchetype();

                if (instanciated) {
                    DestroyInstance();
                    Instantiate();
                }
            }
 
            return changed;
        }

#endregion
    }

    public class Exploration_MonoInstance : Exploration_Element, IPEGI_ListInspect, IGotName, IManageFading
    {
        string name = "Unnamed";
        public int prefabIndex = -1;
        string instanceConfig;
        MonoBehaviour instance;

#region Encode & Decode

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("b", base.Encode)
            .Add_String("n", name)
            .Add("ind", prefabIndex)
            .Add_IfNotEmpty("cfg", instanceConfig);

        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "b": data.Decode_Base(base.Decode, this); break;
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
        
        public bool InspectInList(IList list, int ind, ref int edited) {

            var changed = this.inspect_Name();

            if ((instance ? icon.Active : icon.InActive).Click("Inspect"))
                edited = ind;

            instance.ClickHighlight();

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

            var changed = base.Inspect();

            "Prefab".select_Index(ref prefabIndex, Exploration_Node.monoBehaviourPrefabs);

            if (instance)
                pegi.Try_Nested_Inspect(instance).nl(ref changed);

            return changed;
        }

#endregion
        
        public void FadeAway() {

            if (instance) {

                var std = instance as ICfg;
                if (std != null)
                    instanceConfig = std.Encode().ToString(); 

                var ifd = instance as IManageFading;
                if (ifd != null)
                    ifd.FadeAway();
                else {
                    instance.gameObject.DestroyWhatever();
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
                var std = instance as ICfg;
                if (std != null)
                    std.Decode(instanceConfig);
            }

            return fadedIn;
        }

        public override void OnExit() => FadeAway();

        protected override void Instantiate() => TryFadeIn();
    }

}