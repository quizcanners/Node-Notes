using NodeNotes.RayTracing;
using NodeNotes_Visual;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeNotes.NodeNotesAssetGroup;

namespace NodeNotes
{

#pragma warning disable IDE0018 // Inline variable declaration
    [Serializable]
    public class NodeNotesAssetsMgmt : IPEGI
    {

        [SerializeField] public List<NodeNotesAssetGroup> assetGroups;

        [SerializeField] private FilteredMaterials Materials = new FilteredMaterials((NodeNotesAssetGroup grp) => grp.materials);
        [SerializeField] private FilteredSdfObjects SdfObjects = new FilteredSdfObjects((NodeNotesAssetGroup grp) => grp.sdfObjects);
        [SerializeField] private FilteredAudioClips AudioClips = new FilteredAudioClips((NodeNotesAssetGroup grp) => grp.audioClips);
        [SerializeField] private FilteredRayTracedObjects RtObjects = new FilteredRayTracedObjects((NodeNotesAssetGroup grp) => grp.rayTraceObjects);


        private void RefreshAssetGroups()
        {
            Materials.Refresh();
            AudioClips.Refresh();
            SdfObjects.Refresh();
            RtObjects.Refresh();
        }



        #region Meshes

        [SerializeField] protected Mesh _defaultMesh;
        public Mesh GetMesh(string name) => _defaultMesh;

        #endregion

        #region Materials

        [Serializable]
        private class FilteredMaterials : FilteredAssetGroup<Material> {
            public FilteredMaterials(Func<NodeNotesAssetGroup, TaggedAssetsList<Material>> getOne) : base(getOne)
            {
            }
        }

        public bool Get(string key, out Material mat)
        {
            mat = Materials.Get(key, assetGroups);
            return mat;
        }

        #endregion

        #region SDF objects
        [Serializable]
        public class FilteredSdfObjects : FilteredAssetGroup<SDFobject>
        {
            public FilteredSdfObjects(Func<NodeNotesAssetGroup, TaggedAssetsList<SDFobject>> getOne) : base(getOne)
            {
            }
        }

        public bool Get(string key, out SDFobject sdf)
        {
            sdf = SdfObjects.Get(key, assetGroups);
            return sdf;
        }

        public List<string> GetSdfObjectsKeys() => SdfObjects.GetAllKeysKeys(assetGroups);

        

        #endregion

        #region Audio Clips
        [Serializable]
        public class FilteredAudioClips : FilteredAssetGroup<AudioClip>
        {
            public FilteredAudioClips(Func<NodeNotesAssetGroup, TaggedAssetsList<AudioClip>> getOne) : base(getOne)
            {
            }
        }

        public bool Get(string key, out AudioClip sdf)
        {
            sdf = AudioClips.Get(key, assetGroups);
            return sdf;
        }

        public List<string> GetAudioClipObjectsKeys() => AudioClips.GetAllKeysKeys(assetGroups);
        #endregion

        #region Filtered RayTracing Objects
        [Serializable]
        public class FilteredRayTracedObjects : FilteredAssetGroup<RayTracingPrefabObject>
        {
            public FilteredRayTracedObjects(Func<NodeNotesAssetGroup, TaggedAssetsList<RayTracingPrefabObject>> getOne) : base(getOne)
            {
            }
        }

        public bool Get(string key, out RayTracingPrefabObject sdf)
        {
            sdf = RtObjects.Get(key, assetGroups);
            return sdf;
        }

        public List<string> GetRayTracedObjectsKeys() => RtObjects.GetAllKeysKeys(assetGroups);
        #endregion

        [SerializeField] public AudioClip onMouseDownButtonSound;
        [SerializeField] public AudioClip onMouseClickSound;
        [SerializeField] public AudioClip onMouseClickFailedSound;
        [SerializeField] public AudioClip onMouseLeaveSound;
        [SerializeField] public AudioClip onMouseHoldSound;
        [SerializeField] public AudioClip onSwipeSound;

        private int _inspectedGroup;

        public bool Inspect()
        {
            var changed = false;

            if ("Refresh".Click().nl())
                RefreshAssetGroups();

            "Asset Groups".edit_List_SO(ref assetGroups, ref _inspectedGroup).nl(ref changed);

            if (changed)
                RefreshAssetGroups();
            
            return changed;
        
        }
        
        #region Filtered Group Base

        [Serializable]
        public abstract class FilteredAssetGroup<T> where T : UnityEngine.Object
        {
            [SerializeField] protected T _defaultMaterial;
            [NonSerialized] private Dictionary<string, T> filteredOnjects = new Dictionary<string, T>();
            [NonSerialized] private List<string> allObjectKeys;

            public void Refresh()
            {
                filteredOnjects.Clear();
                allObjectKeys = null;
            }

            private readonly Func<NodeNotesAssetGroup, TaggedAssetsList<T>> _getOne;

            public List<string> GetAllKeysKeys(List<NodeNotesAssetGroup> assetGroups)
            {
                if (allObjectKeys != null)
                    return allObjectKeys;

                allObjectKeys = new List<string>();

                foreach (var assetGroup in assetGroups)
                foreach (var taggedObject in _getOne(assetGroup).taggedList) //assetGroup.materials.taggedList)
                    allObjectKeys.Add(taggedObject.tag);

                return allObjectKeys;
            }

            public T Get(string key, List<NodeNotesAssetGroup> assetGroups)
            {
                if (key.IsNullOrEmpty())
                    return _defaultMaterial;

                T mat;

                if (!filteredOnjects.TryGetValue(key, out mat))
                    foreach (var group in assetGroups)
                        if (_getOne(group).TreGet(key, out mat))
                        {
                            filteredOnjects[key] = mat;
                            break;
                        }

                return mat ? mat : _defaultMaterial;
            }

            public FilteredAssetGroup(Func<NodeNotesAssetGroup, TaggedAssetsList<T>> getOne)
            {
                _getOne = getOne;
            }

        }
        
        #endregion
    }
}
