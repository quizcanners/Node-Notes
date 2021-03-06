﻿using NodeNotes_Visual;
using QuizCannersUtilities;
using System;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
using NodeNotes.RayTracing;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes
{
    [CreateAssetMenu(fileName = "Unity Asset Group", menuName = "Node Nodes/Unity Asset Group", order = 0)]
    public class NodeNotesAssetGroup : ScriptableObject, IPEGI
    {

        [Serializable] public class TaggedAudioClips : TaggedAssetsList<AudioClip> { }
        public TaggedAudioClips audioClips;

        [Serializable] public class TaggedMaterials : TaggedAssetsList<Material> { }
        public TaggedMaterials materials;

        [Serializable] public class TaggedSdfObject : TaggedAssetsList<SDFobject> { }
        public TaggedSdfObject sdfObjects;

        [Serializable] public class TaggedRayTracingPrefabs : TaggedAssetsList<RayTracingPrefabObject> { }
        public TaggedRayTracingPrefabs rayTraceObjects;

        protected static bool TryGetAsset_Internal<T, G>(string tag, out T asset, List<G> list, Dictionary<string, T> lookup) where G: TaggedAssetGeneric where T : Object
        {
            if (lookup.TryGetValue(tag, out asset))
                return true;

            foreach (var tm in list)
            {
                if (tag.Equals(tm.tag) && tm.asset)
                {
                    asset = tm.asset as T;
                    if (!asset)
                    {
                        Debug.LogError("Couldn't convert {0} to {1}".F(tag, typeof(T)));
                    }
                    return true;
                }
            }

            asset = null;

            return false;
        }

        private int _inspectedAssetList = -1;
        public bool Inspect()
        {
            pegi.toggleDefaultInspector(this).nl();

            audioClips.enter_Inspect(ref _inspectedAssetList, 0).nl();

            materials.enter_Inspect(ref _inspectedAssetList, 1).nl();

            sdfObjects.enter_Inspect(ref _inspectedAssetList, 2).nl();

            rayTraceObjects.enter_Inspect(ref _inspectedAssetList, 3).nl();

            return false;
        }

        [Serializable]
        public abstract class TaggedAssetsList<T>: IPEGI where T : Object
        {
            [SerializeField] public List<TaggedAssetGeneric> taggedList;
            private Dictionary<string, T> _listCached = new Dictionary<string, T>();
            public bool TreGet(string tag, out T mat) => TryGetAsset_Internal(tag, out mat, taggedList, _listCached);


            private int _inspectedElement = -1;
            public bool Inspect()
            {
                pegi.nl();

                TaggedAssetGeneric.inspectedType = typeof(T);

                "{0} Assets".F(typeof(T).ToString().SimplifyTypeName()).edit_List(ref taggedList, ref _inspectedElement).nl();
                
                return false;
            }
        }

        [Serializable]
        public class TaggedAssetGeneric: IPEGI_ListInspect
        {
            public string tag = "tag";
            public Object asset;

            public static Type inspectedType;

            public bool InspectInList(IList list, int ind, ref int edited)
            {
                pegi.edit(ref tag);
                
                pegi.edit(ref asset, inspectedType);

                return false;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesAssetGroup))]
    public class NodeNotesAssetGroupsDrawer : PEGI_Inspector_SO<NodeNotesAssetGroup> { }
#endif
}