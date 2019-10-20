using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace NodeNotes_Visual.ECS {

    public struct TruePosition_Component : IComponentData {
        public float3 value;
    }

    [TaggedType(classTag, "True Position")]
    public class AccelerationCfg : ComponentCfgGeneric<TruePosition_Component> {
        const string classTag = "truePos";
        public override string ClassTag => classTag;
    }

    #region Player Controls
    public struct PlayerControls_Component : IComponentData { }

    [TaggedType(classTag, "Player Controls")]
    public class PlayerControlsCfg : ComponentCfgGeneric<PlayerControls_Component> {
        
        #region Tagged Class

        const string classTag = "plCnt";
        public override string ClassTag => classTag;

        #endregion

    }
    #endregion

    #region Phisics Array

    [Serializable]
    public struct PhisicsArrayDynamic_Component : IComponentData, IGotDisplayName, IPEGI_ListInspect, ICfg
    {
        public uint phisixIndex;

        public float testValue;

        #region Inspector

        public string NameForDisplayPEGI() => "Phisics Array index: "+phisixIndex;

        public bool InspectInList(IList list, int ind, ref int edited)
        {
            var changed = "PhisX Index".edit(80, ref phisixIndex);

            "Test value".edit(60, ref testValue).changes(ref changed);

            if (icon.Refresh.Click("Reset test value").changes(ref changed))
                testValue = 0;

            return changed;
        }

        #endregion

        #region Encode & Decode

        public CfgEncoder Encode() => new CfgEncoder()
            .Add("i", phisixIndex)
            .Add("tv", testValue);

        public bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "i":
                    phisixIndex = data.ToUInt();
                    break;
                case "tv":
                    testValue = data.ToFloat();
                    break;
                default: return false;
            }

            return true;
        }

        public void Decode(string data) => data.DecodeTagsFor(this);

        #endregion

    }

    [TaggedType(classTag, "Phisics Array Index")]
    public class PhisicsArrayDynamicCfg : ComponentCfgGeneric<PhisicsArrayDynamic_Component> {

        #region Tagged Class

        const string classTag = "phArr";
        public override string ClassTag => classTag;

        #endregion

        #region Set Data

        public override void SetData(Entity e) => e.Set(new PhisicsArrayDynamic_Component() {phisixIndex = 0});

        #endregion
        
    }

    #endregion


}
