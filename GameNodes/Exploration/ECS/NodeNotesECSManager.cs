using PlayerAndEditorGUI;
using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NodeNotes_Visual {

    public class EntitySTDAttribute : Abstract_WithTaggedTypes {
        public override TaggedTypes_STD TaggedTypes => EntityDataSTD_Base.all;
    }

    [EntitySTD]
    public abstract class EntityDataSTD_Base : AbstractKeepUnrecognized_STD, IGotClassTag{

        public abstract string ClassTag { get; }
        public static TaggedTypes_STD all = new TaggedTypes_STD(typeof(EntityDataSTD_Base));
        public TaggedTypes_STD AllTypes => all;

        public static Entity inspectedEntity;
        public abstract void AddComponent(Entity e);

        public abstract void SetData(Entity e);
    }

    [TaggedType(classTag, "Position")]
    public class Entity_PositionSTD : EntityDataSTD_Base, IPEGI_ListInspect {
        const string classTag = "pos";

        Vector3 pos;

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotZero("pos", pos);

        public override bool Decode(string tag, string data) {
            switch (tag) {
                case "pos": pos = data.ToVector3(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        public override string ClassTag => classTag;

        public override void AddComponent(Entity e) => e.Add<Position>();
        
        public bool PEGI_inList(IList list, int ind, ref int edited) => "pos".edit(30, ref pos);

        public override void SetData(Entity e) => e.Set(new Position() { Value = new float3(pos.x, pos.y, pos.z) });
    }

    [TaggedType(classTag, "Rotation")]
    public class Entity_RotationSTD : EntityDataSTD_Base, IPEGI_ListInspect {
        const string classTag = "rot";
        Quaternion qt;
        public override string ClassTag => classTag;

        #region Encode & Decode
        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add(classTag, qt);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case classTag: qt = data.ToQuaternion(); break;
                default: return false;
            }
            return true;
        }
        #endregion

        public override void AddComponent(Entity e) => e.Add<Rotation>();
        
        public override void SetData(Entity e) => e.Set(new Rotation() { Value = new quaternion() { value = new float4(qt.x,qt.y,qt.z,qt.w)} });

        public bool PEGI_inList(IList list, int ind, ref int edited) => "Rotation".edit(60, ref qt);
    }


    public static class NodeNotesECSManager {

        #region Encode & Decode
        public static StdEncoder Encode(this float3 v3) => new StdEncoder()
            .Add_IfNotEpsilon("x", v3.x)
            .Add_IfNotEpsilon("y", v3.y)
            .Add_IfNotEpsilon("z", v3.z);

        public static StdEncoder Encode(this quaternion q) => new StdEncoder()
          .Add_IfNotEpsilon("x", q.value.x)
          .Add_IfNotEpsilon("y", q.value.y)
          .Add_IfNotEpsilon("z", q.value.z)
          .Add_IfNotEpsilon("w", q.value.w);

        public static float3 ToFloat3(this string data) {
            float3 tmp = new float3();

            var cody = new StdDecoder(data);

            foreach (var t in cody)
            {
                var d = cody.GetData();
                switch (t)
                {
                    case "x": tmp.x = d.ToFloat(); break;
                    case "y": tmp.y = d.ToFloat(); break;
                    case "z": tmp.z = d.ToFloat(); break;
                }
            }

            return tmp;
        }

        public static quaternion Toquaternion(this string data)
        {
            quaternion tmp = new quaternion();

            var cody = new StdDecoder(data);

            foreach (var t in cody)
            {
                var d = cody.GetData();
                switch (t)
                {
                    case "x": tmp.value.x = d.ToFloat(); break;
                    case "y": tmp.value.y = d.ToFloat(); break;
                    case "z": tmp.value.z = d.ToFloat(); break;
                    case "w": tmp.value.w = d.ToFloat(); break;
                }
            }

            return tmp;
        }
        #endregion

        static bool ExitOrDrawPEGI<T>(NativeArray<T> array, ref int index, List_Data ld = null) where T : struct
        {
            bool changed = false;

            if (index >= 0)
            {
                if (array == null || index >= array.Length || icon.List.ClickUnfocus("Return to {0} array".F(pegi.GetCurrentListLabel<T>(ld))).nl())
                    index = -1;
                else
                    changed |= array[index].Try_Nested_Inspect();
            }

            return changed;
        }

        public static T edit_Array<T>(ref NativeArray<T> array, ref int inspected, ref bool changed, List_Data datas = null) where T : struct
        {
            T added = default(T);

            if (array == null)
            {
                if ("init array".ClickUnfocus().nl())
                    array = new NativeArray<T>();
            }
            else
            {

                changed |= ExitOrDrawPEGI(array, ref inspected);

                if (inspected == -1)
                {
                    for (int i = 0; i < array.Length; i++)
                        changed |= array[i].Name_ClickInspect_PEGI<T>(null, i, ref inspected, datas).nl();
                }
            }

            return added;
        }

        public static bool edit_Array<T>(this string label, ref NativeArray<T> array, ref int inspected, List_Data datas = null) where T : struct
        {
            label.write_ListLabel(ref inspected);
            bool changed = false;
            edit_Array(ref array, ref inspected, ref changed, datas).listLabel_Used();

            return changed;
        }



        public static EntityManager manager;

        public static void Init()
        {
            Debug.Log("Getting manager");
            manager = World.Active.GetOrCreateManager<EntityManager>();
        }

        public static Entity Add<T>(this Entity ent) where T : struct, IComponentData
        {
            manager.AddComponent(ent, typeof(T));
            return ent;
        }

        public static Entity Set<T>(this Entity ent, T dta) where T : struct, IComponentData
        {
            manager.SetComponentData(ent, dta);
            return ent;
        }

        public static T Get<T>(this Entity ent) where T : struct, IComponentData =>
            manager.GetComponentData<T>(ent);

        public static bool Has<T>(this Entity ent) where T : struct, IComponentData =>
            manager.HasComponent<T>(ent);

        public static NativeArray<ComponentType> GetComponentTypes (this Entity ent) =>
            manager.GetComponentTypes(ent, Allocator.Temp);

        public static Entity Instantiate(GameObject prefab) {
            Entity e = manager.Instantiate(prefab);

            if (prefab)  {
                Debug.Log("Instantiating ECS from prefab {0}".F(prefab));
                e.Set(new ExplorationLerpData { Value = e.Index });
            }
            return e;
        }

        public static Entity Instantiate(List<EntityDataSTD_Base> cmps) { 
            Entity e = manager.CreateEntity();

            foreach (var c in cmps) {
                c.AddComponent(e);
                c.SetData(e);
            }

            return e;
        }


        static int exploredEntity = -1;
        public static bool Inspect()   {

            var changed = false;

            if (manager != null) {

                NativeArray<Entity> all = manager.GetAllEntities(Allocator.Temp);

                GameObject go = null;

                if ("instantiate".edit(ref go).nl())
                    Instantiate(go);

                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];

                    if (e.ToPEGIstring().foldout(ref exploredEntity, i).nl())
                        e.Inspect().nl(ref changed);
                    
                }

            }
            else if (icon.Search.Click("Find manager"))
                Init();

            return changed;
        }

        public static bool Inspect(this Entity e) {
            bool changed = false;

            if (e.Has<ExplorationLerpData>()) {
                var cmps = manager.GetComponentData<ExplorationLerpData>(e);
                cmps.Inspect_AsInList();
            }

            if (e.Has<Position>()) {
                var pos = e.Get<Position>();
                "Position".write(60);
                "X".edit(20,ref pos.Value.x).changes(ref changed);
                "Y".edit(20, ref pos.Value.y).changes(ref changed);
                "Z".edit(20, ref pos.Value.z).changes(ref changed);
                if (changed)
                    e.Set(pos);
                pegi.nl();
            }

            if (e.Has<Rotation>())
            {
                var rot = e.Get<Rotation>();

                var q = rot.Value.value;

                Vector3 eul = (new Quaternion(q.x, q.y, q.z, q.w)).eulerAngles;

                "Rotation".edit(60, ref eul).nl(ref changed);

                if (changed) {
                    var qt = new Quaternion();
                    qt.eulerAngles = eul;

                    rot.Value.value = new Unity.Mathematics.float4(qt.x, qt.y, qt.z, qt.w);
                    e.Set(rot);
                }

                pegi.nl();
            }

            return changed;
        }
    }
}