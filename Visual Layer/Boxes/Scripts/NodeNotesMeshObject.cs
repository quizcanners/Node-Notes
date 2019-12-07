using System;
using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using QuizCannersUtilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes_Visual
{
    using PlayerAndEditorGUI;
    using PlaytimePainter;

    public class NodeNotesMeshObject : ComponentCfg, IManageFading, ILinkedLerping
    {

        [NonSerialized] private PlaytimePainter _painter;
        [NonSerialized] private MeshFilter _meshFilter;
        [NonSerialized] private MeshRenderer _meshRenderer;
        [NonSerialized] private string _materialTag;

        private Base_Node source;

        private Vector3 relativePosition;
        private float relativeZoom = 1f;

        private void UpdateMaterial() => _painter.Material = Shortcuts.Instance.GetMaterial(_materialTag);

        #region Encode & Decode
        public override void Decode(string data)
        {
            base.Decode(data);

            if (_painter.SavedEditableMesh.IsNullOrEmpty())
                _painter.SharedMesh = Instantiate(Shortcuts.Instance.GetMesh(""));
            else 
                _painter.SharedMesh = new Mesh();

            var mc = new MeshConstructor(_painter);

            var m = mc.Construct();

            if (m)
                _painter.SharedMesh = m;

            _painter.UpdateMeshCollider(_painter.SharedMesh);
            
            UpdateMaterial();

        }

        public override CfgEncoder Encode() => base.Encode()
                .Add("pos", relativePosition)
                .Add("s", relativeZoom)
                .Add("m", _painter.EncodeMeshStuff)
                .Add_IfNotEmpty("mat", _materialTag);
         
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "pos": relativePosition = data.ToVector3(); break;
                case "s": relativeZoom = data.ToFloat(); break;
                case "m": _painter.Decode(data); break;
                case "mat": _materialTag = data; break;
                default: return false;
            }

            return true;
        }

        #endregion

        public void Reset(Base_Node node)
        {
            source = node;
            gameObject.hideFlags = HideFlags.DontSave;
            
            if (!_meshFilter)
                _meshFilter = gameObject.AddComponent<MeshFilter>();

            if (!_meshRenderer)
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (!_painter)
            {
                _painter = gameObject.AddComponent<PlaytimePainter>();
                _painter.Reset();
                meshes.Add(this);
                _painter.SharedMesh = Shortcuts.Instance.GetMesh("");
                _painter.UpdateMeshCollider(_painter.SharedMesh);
            }

        }

        #region Inspector

        public override bool Inspect()
        {
            var changed = false;

            "Mesh object".write();

            if ("Edit".Click())
            {

                MeshEditorManager.Inst.EditMesh(_painter, true);
                QcUnity.FocusOn(_painter);
            }


            this.ClickHighlight().nl();
            
            if ("Mat".select(40, ref _materialTag, Shortcuts.Instance.GetMaterialKeys()).nl())
                UpdateMaterial();

            "Relative Pos".edit(ref relativePosition).nl();

            "Relative Zoom".edit(ref relativeZoom).nl();

            return changed;
        }
        
        #endregion
        
        void Update()
        {
            if (_painter && _painter.IsEditingThisMesh)
            {
                if (!Shortcuts.editingNodes)
                {
                    MeshEditorManager.Inst.StopEditingMesh();
                }
            }
        }


        #region Transform Lerping

        public static void ManagedOnDisable()
        {
            if (meshesParentTf)
                meshesParentTf.gameObject.DestroyWhatever();
        }

        public static void OnNodeChange()
        {

            var newCenterNode = Shortcuts.CurrentNode;

            if (newCenterNode == null)
                return;

            NodeNotesMeshObject newCenterMesh = null;

            foreach (var m in meshes)
                if (m.source == newCenterNode)
                {
                    newCenterMesh = m;
                    break;
                }

            if (!newCenterMesh)
                return;

            if (!meshesParentTf)
            {
                 var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                 go.name = "WORLD ROOT";
                meshesParentTf = go.transform;
                meshesParentTf.transform.position = Vector3.zero;
                _meshParentMeshRenderer = go.GetComponent<MeshRenderer>();
                go.GetComponent<Collider>().enabled = false;
                positionLerp = new LinkedLerp.TransformPosition(meshesParentTf, 1000);
                scaleLerp = new LinkedLerp.TransformLocalScale(meshesParentTf, 10);
                OnEditingNodesToggle();
            }
            else
                UnparentAll();

            var currentCenterNode = currentCenterMesh ? currentCenterMesh.source.AsNode : null;


            if (currentCenterMesh && (newCenterNode.IsDirectChildOf(currentCenterNode) ||
                                      currentCenterNode.IsDirectChildOf(newCenterNode))) {

             

                bool zoomingIn = newCenterNode.IsDirectChildOf(currentCenterNode);

                Debug.Log("Got zooming "+ (zoomingIn ? "In" : "Out"));

                //Zoomin out is wrong

                float previousZoom = meshesParentTf.localScale.x;

                float newZoom =
                    (zoomingIn ? (1f / newCenterMesh.relativeZoom) : 
                        currentCenterMesh.relativeZoom // zooming out
                        ) / previousZoom;

                Vector3 currentCenter = meshesParentTf.position;

                meshesParentTf.position = zoomingIn ?
                    newCenterMesh.transform.position :
                    (currentCenter - currentCenterMesh.relativePosition * newZoom); // New mesh doesn't exist yet

                meshesParentTf.localScale = Vector3.one * newZoom;

            }
            else
            {
                meshesParentTf.position = Vector3.zero;
                meshesParentTf.localScale = Vector3.one;
            }



           // currentCenterNode = newCenterNode;
            ParentAll();

            foreach (var m in meshes)
                if (m == newCenterMesh) {
                    currentCenterMesh = m;
                    m.transform.localPosition = Vector3.zero;
                    m.transform.localScale = Vector3.one * m.relativeZoom;
                }
                else
                {
                    m.transform.localPosition = m.relativePosition;
                    m.transform.localScale = Vector3.one;
                }

        }

        public static void OnEditingNodesToggle()
        {
            if (_meshParentMeshRenderer)
                _meshParentMeshRenderer.enabled = Shortcuts.editingNodes;


        }

        private static NodeNotesMeshObject currentCenterMesh;
        private static Transform meshesParentTf;
        private static MeshRenderer _meshParentMeshRenderer;
            // Will also track and parent/unparant nodes that are fading away
        private static List<NodeNotesMeshObject> meshes = new List<NodeNotesMeshObject>();

        private static void ClearDestroyed()
        {
            for(int i=meshes.Count-1; i>=0; i--)
                if (!meshes[i])
                    meshes.RemoveAt(i);
        }

        private static void UnparentAll()
        {

            ClearDestroyed();

            foreach (var m in meshes)
                m.transform.parent = null;
        }

        private static void ParentAll() {
            foreach (var m in meshes)
                m.transform.parent = meshesParentTf;
        }

        private static LinkedLerp.TransformPosition positionLerp;
        private static LinkedLerp.TransformLocalScale scaleLerp;

        public void Portion(LerpData ld)
        {
            if (this == currentCenterMesh)
            {
                positionLerp.Portion(ld, Vector3.zero);
                scaleLerp.Portion(ld, Vector3.one);
            }
        }

        public void Lerp(LerpData ld, bool canSkipLerp)
        {
            if (this == currentCenterMesh)
            {
                positionLerp.Lerp(ld, false);
                scaleLerp.Lerp(ld, false);
            }
        }

        #endregion

        public void FadeAway()
        {
            meshes.Remove(this);
            gameObject.DestroyWhatever();
        }

        public bool TryFadeIn()
        {

           return true;
        }





    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesMeshObject))]
    public class NodeNotesMeshObjectDrawer : PEGI_Inspector_Mono<NodeNotesMeshObject> { }
#endif

}