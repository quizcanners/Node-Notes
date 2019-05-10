using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NodeNotes_Visual.ECS {
    
    #region Phisics Array
    
    [Serializable]
    public struct PhisicsArrayDynamic_Component : IComponentData, IGotDisplayName, IPEGI_ListInspect, ICfg {
        public uint phisixIndex;

        public float testValue;

        #region Inspector
        #if PEGI
        public string NameForDisplayPEGI => "Phisics Array index";

        public bool InspectInList(IList list, int ind, ref int edited) {
            var changed = "PhisX Index".edit(80, ref phisixIndex);

            "Test value".edit(60, ref testValue).changes(ref changed);

            if (icon.Refresh.Click("Reset test value").changes(ref changed))
                testValue = 0;

            return changed;
        }
#endif

#endregion

        #region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
            .Add("i", phisixIndex)
            .Add("tv", testValue);

        public bool Decode(string tg, string data) {
            switch (tg) {
                case "i": phisixIndex = data.ToUInt(); break;
                case "tv": testValue = data.ToFloat(); break;
                default: return false;
            }
            return true;
        }

        public void Decode(string data) => data.DecodeTagsFor(this);
        #endregion

    }

    [TaggedType(classTag, "Phisics Array Index")]
    public class PhisicsArrayDynamicCfg : ComponentCfgGeneric<PhisicsArrayDynamic_Component>
    {

#region Tagged Class
        const string classTag = "phArr";
        public override string ClassTag => classTag;
#endregion

#region Set Data

        public override void SetData(Entity e) 
            => e.Set(new PhisicsArrayDynamic_Component() { phisixIndex = 0 });

#endregion
    }
#endregion

    
#region Unity Native Components
    [TaggedType(classTag, "Position")]
    public class PositionCfg : ComponentCfgGeneric<Translation>, IPEGI_ListInspect
    {
#region Tagged Class
        const string classTag = "pos";
        public override string ClassTag => classTag;
#endregion

        Vector3 startPosition;
        
        public override void SetData(Entity e) => e.Set_Position(startPosition);

        #region Inspector
#if PEGI
        public override bool InspectInList(IList list, int ind, ref int edited) => "pos".edit(30, ref startPosition);
#endif
#endregion

#region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_IfNotZero("pos", startPosition);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "pos": startPosition = data.ToVector3(); break;
                default: return false;
            }
            return true;
        }
#endregion

    }

    [TaggedType(classTag, "Rotation")]
    public class RotationCfg : ComponentCfgGeneric<Rotation>, IPEGI_ListInspect {

#region Tagged class
        const string classTag = "rot";
        public override string ClassTag => classTag;
#endregion

        Quaternion qt;

        public override void SetData(Entity e) => e.Set_Rotation(qt);

        #region Inspector
#if PEGI
        public override bool InspectInList(IList list, int ind, ref int edited) => "Rotation".edit(60, ref qt);
#endif
#endregion

#region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add(classTag, qt);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case classTag: qt = data.ToQuaternion(); break;
                default: return false;
            }
            return true;
        }
#endregion

    }
#endregion


#region Extensions 

    public static class ECS_STD_Extensions {

        public static Entity Set_Position (this Entity e, Vector3 pos) => e.Set(new Translation() { Value = new float3(pos.x, pos.y, pos.z) });

        public static Entity Set_Rotation (this Entity e, Quaternion qt) => e.Set(new Rotation() { Value = new quaternion() { value = new float4(qt.x, qt.y, qt.z, qt.w) } });
    }

#endregion
}