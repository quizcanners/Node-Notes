using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;


namespace NodeNotes_Visual {

    [ExecuteInEditMode]
    public class BackPaintTextureSetter : MonoBehaviour
    {

        //public Renderer rendy;

        public Texture backPaintTexture;

        public Camera paintCamera;

        // Use this for initialization
        void OnEnable() {
            Shader.SetGlobalTexture("_Nebula_BG", backPaintTexture);

        }

        // Update is called once per frame
        void Update() {

            if (paintCamera)
                paintCamera.Render();

            Shader.SetGlobalVector("_Nebula_Pos", transform.position.ToVector4(transform.localScale.x));

        }
    }
}
