﻿using System;
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
        [NonSerialized] private string materialTag;

        private Base_Node source;

        private Vector3 relativePosition;
        private float relativeZoom = 1f;

        private void UpdateMaterial() => _painter.Material = Shortcuts.Instance.GetMaterial(materialTag);

        public override void Decode(string data)
        {
            base.Decode(data);

            var mc = new MeshConstructor(_painter);

            mc.Construct(_painter);

            UpdateMaterial();

        }

        public override CfgEncoder Encode() => base.Encode()
                .Add("pos", relativePosition)
                .Add("s", relativeZoom)
                .Add("m", _painter.EncodeMeshStuff)
                .Add_IfNotEmpty("mat", materialTag);
         
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "pos": relativePosition = data.ToVector3(); break;
                case "s": relativeZoom = data.ToFloat(); break;
                case "m": _painter.Decode(data); break;
                case "mat": materialTag = data; break;
                default: return false;
            }

            return true;
        }
        
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
                
            }

          


        }

        #region Inspector

        public override bool Inspect()
        {
            var changed = false;

            "Mesh object".write();

            this.ClickHighlight().nl();
            
            if ("Mat".select(40, ref materialTag, Shortcuts.Instance.GetMaterialKeys()).nl())
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
                    MeshEditorManager.Inst.StopEditingMesh();
            }
        }


        #region Transform Lerping

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
                meshesParentTf = new GameObject("Node Notes World Root").transform;
                meshesParentTf.transform.position = Vector3.zero;
                meshesParentTf.gameObject.hideFlags = HideFlags.DontSave;
                positionLerp = new LinkedLerp.TransformPosition(meshesParentTf, 1000);
                scaleLerp = new LinkedLerp.TransformLocalScale(meshesParentTf, 10);
            }
            else
                UnparentAll();

            var currentCenterNode = currentCenterMesh ? currentCenterMesh.source.AsNode : null;


            if (currentCenterMesh && (newCenterNode.IsDirectChildOf(currentCenterNode) ||
                                      currentCenterNode.IsDirectChildOf(newCenterNode))) {

             

                bool zoomingIn = newCenterNode.IsDirectChildOf(currentCenterNode);

                Debug.Log("Got zooming "+ (zoomingIn ? "In" : "Out"));

                float scaleCoefficient =
                    (zoomingIn ? (1f / newCenterMesh.relativeZoom) : currentCenterMesh.relativeZoom) / meshesParentTf.localScale.x;
                
                meshesParentTf.position = zoomingIn ?
                    newCenterMesh.transform.position :
                    (meshesParentTf.TransformPoint(-currentCenterMesh.relativePosition));

                meshesParentTf.localScale = Vector3.one * scaleCoefficient;

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


        private static NodeNotesMeshObject currentCenterMesh;
        private static Transform meshesParentTf;
            // Will also track and parent/unparant nodes that are fading away
        private static List<NodeNotesMeshObject> meshes = new List<NodeNotesMeshObject>();

        private static void UnparentAll() {
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