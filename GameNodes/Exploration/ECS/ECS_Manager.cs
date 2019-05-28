using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NodeNotes_Visual.ECS {
    
    public static class NodeNotesECSManager {

        public static EntityManager manager;

        public static void Init() {
            Debug.Log("Getting manager");
            manager = World.Active.EntityManager; //<EntityManager>();
        }

        #region Entity MGMT
        public static Entity Instantiate(GameObject prefab)
        {
            Entity e = manager.Instantiate(prefab);

            if (prefab)
            {
                Debug.Log("Instantiating ECS from prefab {0}".F(prefab));
                e.Set(new PhisicsArrayDynamic_Component { testValue = e.Index });
            }
            return e;
        }

        public static Entity Instantiate(this EntityArchetype arch)
        {

            Entity e = manager.CreateEntity(arch);
            return e;

        }
        
        public static Entity Add(this Entity ent, ComponentType type) {
            manager.AddComponent(ent, type);
            return ent;
        }

        public static Entity Add<T>(this Entity ent) where T : struct, IComponentData
        {
            manager.AddComponent(ent, typeof(T));
            return ent;
        }

        public static Entity Set<T>(this Entity ent, T dta) where T : struct, IComponentData {
            manager.SetComponentData(ent, dta);
            return ent;
        }

        public static T Get<T>(this Entity ent) where T : struct, IComponentData =>
            manager.GetComponentData<T>(ent);

        public static bool Has<T>(this Entity ent) where T : struct, IComponentData =>
            manager.HasComponent<T>(ent);
        
        public static void Destroy(this Entity ent) => manager.DestroyEntity(ent);
        
        public static NativeArray<ComponentType> GetComponentTypes (this Entity ent) =>
            manager.GetComponentTypes(ent, Allocator.Temp);
        #endregion

        #region Entity Config 
        public static List<ComponentType> GetComponentTypes (this List<ComponentCfgAbstract> cmps) {
            var lst = new List<ComponentType>();

            foreach (var c in cmps)
                lst.Add(c.ComponentType);

            return lst;

        }

        public static EntityArchetype ToArchetype(this List<ComponentCfgAbstract> cmps) => manager.CreateArchetype(cmps.GetComponentTypes().ToArray());

        public static Entity Instantiate(this List<ComponentCfgAbstract> cmps)
        {

            Entity e = manager.CreateEntity();

            foreach (var c in cmps)
            {
                c.AddComponent(e);
                c.SetData(e);
            }

            return e;
        }
        #endregion

        #region Encode & Decode
        public static CfgEncoder Encode(this float3 v3) => new CfgEncoder()
            .Add_IfNotEpsilon("x", v3.x)
            .Add_IfNotEpsilon("y", v3.y)
            .Add_IfNotEpsilon("z", v3.z);

        public static CfgEncoder Encode(this quaternion q) => new CfgEncoder()
          .Add_IfNotEpsilon("x", q.value.x)
          .Add_IfNotEpsilon("y", q.value.y)
          .Add_IfNotEpsilon("z", q.value.z)
          .Add_IfNotEpsilon("w", q.value.w);

        public static float3 ToFloat3(this string data)
        {
            float3 tmp = new float3();

            var cody = new CfgDecoder(data);

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

            var cody = new CfgDecoder(data);

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

        #region Inspector
#if PEGI

        
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

                    if (e.ToPegiString().foldout(ref exploredEntity, i).nl())
                        e.Inspect().nl(ref changed);
                    
                }

            }
            else if (icon.Search.Click("Find manager"))
                Init();

            return changed;
        }

        public static bool Inspect(this Entity e) {
            bool changed = false;

            if (e.Has<PhisicsArrayDynamic_Component>()) {
                var cmps = manager.GetComponentData<PhisicsArrayDynamic_Component>(e);
                cmps.Inspect_AsInList();
            }

            if (e.Has<Translation>()) {
                var pos = e.Get<Translation>();
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

        static bool ExitOrDrawPEGI<T>(NativeArray<T> array, ref int index, ListMetaData ld = null) where T : struct
        {
            bool changed = false;

            if (index >= 0)
            {
                if (array == null || index >= array.Length || icon.List.ClickUnFocus("Return to {0} array".F(pegi.GetCurrentListLabel<T>(ld))).nl())
                    index = -1;
                else
                    changed |= array[index].Try_Nested_Inspect();
            }

            return changed;
        }

        public static T edit_Array<T>(ref NativeArray<T> array, ref int inspected, ref bool changed, ListMetaData metaDatas = null) where T : struct
        {
            T added = default(T);

            if (array == null)
            {
                if ("init array".ClickUnFocus().nl())
                    array = new NativeArray<T>();
            }
            else
            {

                changed |= ExitOrDrawPEGI(array, ref inspected);

                if (inspected == -1)
                {
                    for (int i = 0; i < array.Length; i++)
                        changed |= array[i].InspectValueInList<T>(null, i, ref inspected, metaDatas).nl();
                }
            }

            return added;
        }

        public static bool edit_Array<T>(this string label, ref NativeArray<T> array, ref int inspected) where T : struct
        {
            label.write_Search_ListLabel(ref inspected, null);
            bool changed = false;
            edit_Array(ref array, ref inspected, ref changed);

            return changed;
        }
#endif

        #endregion
    }
}