using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeNotes
{
    [CreateAssetMenu(fileName = "Unity Asset Group", menuName = "Story Nodes/Unity Asset Group", order = 0)]
    public class NodeNotesAssetGroups : ScriptableObject
    {

        public List<TaggedMaterial> materials;

        public bool TreGetMaterial(string tag, out Material mat) => TryGetAsset(tag, out mat, materials);

        private bool TryGetAsset<T, G>(string tag, out T asset, List<G> list) where G: TaggedAssetGeneric<T> where T : Object
        {
            foreach (var tm in list)
            {
                if (tag.Equals(tm.tag) && tm.asset)
                {
                    asset = tm.asset;
                    return true;
                }
            }

            asset = null;

            return false;
        }


        [Serializable]
        public class TaggedMaterial : TaggedAssetGeneric<Material>
        {

        }
        
        [Serializable]
        public abstract class TaggedAssetGeneric<T> 
        {
            public string tag;
            public T asset;
        }
    }
}