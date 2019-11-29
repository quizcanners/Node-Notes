using System;
using System.Collections;
using System.Collections.Generic;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{

    using PlaytimePainter;

    public class NodeNotesMeshObject : ComponentCfg
    {

        [NonSerialized] private PlaytimePainter _painter;
        [NonSerialized] private MeshFilter _meshFilter;
        [NonSerialized] private MeshRenderer _meshRenderer;

        public override void Decode(string data)
        {
            base.Decode(data);

            var editedMesh = new EditableMesh(_painter);

            var mc = new MeshConstructor(editedMesh, _painter.MeshProfile, _painter.SharedMesh);
        }

        public override CfgEncoder Encode() => base.Encode()
            .Add("pos", transform.position)
            .Add("m", _painter.EncodeMeshStuff);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "pos": transform.position = data.ToVector3(); break;
                    
                case "m": _painter.Decode(data); break;
                default: return false;
            }

            return true;
        }

       
       
        void Reset()
        {
            _painter = gameObject.AddComponent<PlaytimePainter>();
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}