using System;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NodeNotes_Visual.ECS {

#pragma warning disable IDE0019 // Simplify 'default' expression


    #region Generic Entity Config
 /*   public class ComponentSTDAttributeAttribute : AbstractWithTaggedTypes {
        public override TaggedTypesCfg TaggedTypes => ComponentCfgAbstract.all;
    }

    [ComponentSTDAttribute]*/
    public abstract class ComponentCfgAbstract : AbstractKeepUnrecognizedCfg, IGotClassTag
    {
        #region Tagged Types
        public abstract string ClassTag { get; }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(ComponentCfgAbstract));
        //public TaggedTypesCfg AllTypes => all;
        #endregion

        #region Component MGMT

        public virtual void AddComponentSetData(Entity e)
        {
            e.Add(ComponentType);
            SetData(e);
        }

        public abstract ComponentType ComponentType { get; }

        public abstract void SetData(Entity e);

        public abstract bool HasComponent(Entity e);
        #endregion

        #region Inspector
        public static Entity inspectedEntity;
        
        #endregion

    }

    public abstract class ComponentCfgGeneric<T> : ComponentCfgAbstract, IPEGI where T : struct, IComponentData {

        public static EntityManager Manager => NodeNotesECSManager.Manager;

        static ComponentType type;

        public override ComponentType ComponentType
        {
            get {

                if (type == null)
                {

                    try
                    {
                        type = ComponentType.ReadWrite<T>();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Couldn't get read write type for {0}: {1}".F(typeof(T), ex.ToString()));
                    }
                }

                return type;
            }
        }

        public virtual T GetData(Entity e) => e.Get<T>();

        public virtual void SetData(Entity e, T cmp) => e.Set(cmp);

        public override void SetData(Entity e) => SetData(e, new T());

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);

        #region Inspector

        protected virtual bool InspectActiveData(ref T dta) {
            var ipl = dta as IPEGI;
            if (ipl != null)
                return ipl.Inspect();
            else
                return false;
        }

        public override bool Inspect() {

            if (inspectedEntity != null && HasComponent(inspectedEntity)) {

                var cmp = GetData(inspectedEntity);

                if (InspectActiveData(ref cmp))
                   SetData(inspectedEntity, cmp);
                 
            }

            return false;
        }
        #endregion


    }

    public abstract class ComponentSharedCfgGeneric<T> : ComponentCfgAbstract, IPEGI where T : struct, ISharedComponentData
    {

        public static EntityManager Manager => NodeNotesECSManager.Manager;
        
        static ComponentType type;

        public override ComponentType ComponentType
        {
            get
            {

                try
                {
                    type = ComponentType.ReadWrite<T>();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Couldn't get read write type for {0}: {1}".F(typeof(T), ex.ToString()));
                }

                return type;
            }
        }

        public virtual T GetData(Entity e) => e.GetShared<T>();

        public override void SetData(Entity e) { }
        
        public virtual void SetData(Entity e, T cmp) => e.SetShared(cmp);

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);

        #region Inspector

        protected virtual bool InspectActiveData(ref T dta)
        {
            var ipl = dta as IPEGI;
            if (ipl != null)
                return ipl.Inspect();
            else
                return false;
        }

        public override bool Inspect() {

            if (inspectedEntity != null && HasComponent(inspectedEntity)) {

                var cmp = GetData(inspectedEntity);

                if (InspectActiveData(ref cmp))
                    SetData(inspectedEntity, cmp);
            }
            return false;
        }
        #endregion
    }

    #endregion

}
