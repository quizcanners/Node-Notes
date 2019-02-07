using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NodeNotes_Visual.ECS {

    #region Generic Entity Config
    public class ComponentSTDAttributeAttribute : Abstract_WithTaggedTypes {
        public override TaggedTypes_STD TaggedTypes => Component_STD_Abstract.all;
    }

    [ComponentSTDAttribute]
    public abstract class Component_STD_Abstract : AbstractKeepUnrecognized_STD, IGotClassTag, IPEGI_ListInspect
    {
        #region Tagged Types
        public abstract string ClassTag { get; }
        public static TaggedTypes_STD all = new TaggedTypes_STD(typeof(Component_STD_Abstract));
        public TaggedTypes_STD AllTypes => all;
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
#if PEGI
        public static Entity inspectedEntity;
        public virtual bool PEGI_inList(IList list, int ind, ref int edited)
        {

            if (inspectedEntity != null && HasComponent(inspectedEntity))
            {

                var cmp = GetComponent(inspectedEntity);
                var ipl = cmp as IPEGI_ListInspect;
                if (ipl != null)
                {

                    var changed = false;
                    if (ipl.PEGI_inList(list, ind, ref edited).changes(ref changed))
                        SetData(inspectedEntity, cmp);

                    return changed;
                }
                else "No PEGI list inspect for {0}".F(ClassTag);
            }
            else "No ENtity or Component on it".write();

            return false;
        }
#endif
#endregion
    }

    public abstract class Component_STD_Generic<T> : Component_STD_Abstract where T : struct, IComponentData {

        public static EntityManager Manager => NodeNotesECSManager.manager;

        static ComponentType type = ComponentType.Create<T>();
        
        public override ComponentType ComponentType => type;
        
        public override IComponentData GetComponent(Entity e) => e.Get<T>();

        public override void SetData(Entity e, IComponentData cmp) => e.Set((T)cmp);

        public override bool HasComponent(Entity e) => Manager.HasComponent<T>(e);
    }
#endregion

}
