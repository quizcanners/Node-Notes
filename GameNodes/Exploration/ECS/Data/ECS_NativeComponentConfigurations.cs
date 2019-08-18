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

    #region Unity Native Components


    [TaggedType(classTag, "Local To World")]
    public class LocalToWorldCfg : ComponentCfgGeneric<LocalToWorld>, IPEGI_ListInspect
    {
        #region Tagged Class
        const string classTag = "locToW";
        public override string ClassTag => classTag;
        #endregion

        #region Inspector

        public bool InspectInList(IList list, int ind, ref int edited)
        {
            "Local To World shows mesh".write();

            return false;
        }

        #endregion

    }

    [TaggedType(classTag, "Render Mesh")]
    public class RenderMeshCfg : ComponentSharedCfgGeneric<RenderMesh>, IPEGI_ListInspect
    {
        #region Tagged Class
        const string classTag = "rendMesh";
        public override string ClassTag => classTag;
        #endregion

        public string mesh;
        public string mat;

        public override void SetData(Entity e) => e.Set_Mesh(Shortcuts.Instance.GetMesh(mesh), Shortcuts.Instance.GetMaterial(mat));

        #region Inspector

        public bool InspectInList(IList list, int ind, ref int edited)
        {
            "Render Mesh".write();

            return false;
        } // "mesh".edit(40, ref mesh) || "mat".edit(40, ref mat);

        #endregion

        #region Encode & Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("me", mesh)
            .Add_String("ma", mat);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "me": mesh = data; break;
                case "ma": mat = data; break;
                default: return false;
            }
            return true;
        }
        #endregion

    }

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

        protected override bool InspectActiveData(ref Translation dta)
        {

            float3 val = dta.Value;

            Vector3 v3 = new Vector3(val.x, val.y, val.z);

            if ("Current Position".edit(ref v3))
            {
                dta.Value = new float3(v3.x, v3.y, v3.z);
                return true;
            }

            return false;
        }

        public bool InspectInList(IList list, int ind, ref int edited) {

            var changed = "Position".edit(ref startPosition);

            if (icon.Enter.Click())
                edited = ind;

            return changed;

        }
        
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
    public class RotationCfg : ComponentCfgGeneric<Rotation>, IPEGI_ListInspect
    {

        #region Tagged class
        const string classTag = "rot";
        public override string ClassTag => classTag;
        #endregion

        Quaternion qt;

        public override void SetData(Entity e) => e.Set_Rotation(qt);

        #region Inspector

        public bool InspectInList(IList list, int ind, ref int edited) {

            var changes = "Rotation".edit(ref qt);

            if (icon.Enter.Click())
                edited = ind;

            return changes;
        }

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

        public static Entity Set_Mesh(this Entity e, Mesh Mesh, Material mat) => e.SetShared(new RenderMesh() { mesh = Mesh, material = mat });

        public static Entity Set_Position(this Entity e, Vector3 pos) => e.Set(new Translation() { Value = new float3(pos.x, pos.y, pos.z) });

        public static Entity Set_Rotation(this Entity e, Quaternion qt) => e.Set(new Rotation() { Value = new quaternion() { value = new float4(qt.x, qt.y, qt.z, qt.w) } });
    }

    #endregion

}