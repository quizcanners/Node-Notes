using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NodeNotes_Visual.ECS {

#pragma warning disable IDE0019 // Simplify 'default' expression


    #region Generic Entity Config
    public class ComponentSTDAttributeAttribute : AbstractWithTaggedTypes {
        public override TaggedTypesCfg TaggedTypes => ComponentCfgAbstract.all;
    }

    [ComponentSTDAttribute]
    public abstract class ComponentCfgAbstract : AbstractKeepUnrecognizedCfg, IGotClassTag, IPEGI_ListInspect
    {
        #region Tagged Types
        public abstract string ClassTag { get; }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(ComponentCfgAbstract));
        public TaggedTypesCfg AllTypes => all;
        #endregion

        #region Component MGMT
        public virtual void AddComponent(Entity e) => e.Add(ComponentType);

        public abstract IComponentData GetComponent(Entity e);

        public abstract ComponentType ComponentType { get; }

        public abstract void SetData(Entity e);

        public abstract void SetData(Entity e, IComponentData cmp);

        public abstract bool HasComponent(Entity e);
        #endregion

        #region Inspector
        public static Entity inspectedEntity;
        public virtual bool InspectInList(IList list, int ind, ref int edited)
        {

            if (inspectedEntity != null && HasComponent(inspectedEntity))
            {

                var cmp = GetComponent(inspectedEntity);
                var ipl = cmp as IPEGI_ListInspect;
                if (ipl != null)
                {

                    var changed = false;
                    if (ipl.InspectInList(list, ind, ref edited).changes(ref changed))
                        SetData(inspectedEntity, cmp);

                    return changed;
                }
                else "No PEGI list inspect for {0}".F(ClassTag);
            }
            else "No ENtity or Component on it".write();

            return false;
        }
        #endregion
    }

    public abstract class ComponentCfgGeneric<T> : ComponentCfgAbstract where T : struct, IComponentData {

        public static EntityManager Manager => NodeNotesECSManager.manager;

        static ComponentType type = ComponentType.ReadWrite<T>();
        
        public override ComponentType ComponentType => type;
        
        public override IComponentData GetComponent(Entity e) => e.Get<T>();

        public override void SetData(Entity e, IComponentData cmp) => e.Set((T)cmp);

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);
    }
#endregion

}
