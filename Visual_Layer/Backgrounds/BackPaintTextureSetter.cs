using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;


namespace NodeNotes_Visual {

    [ExecuteInEditMode]
    public class BackPaintTextureSetter : MonoBehaviour
    {

        //public Renderer skeRenderer;

        public Texture backPaintTexture;

        public Camera paintCamera;

        ShaderProperty.TextureValue nebulaTex_Property = new ShaderProperty.TextureValue("_Nebula_BG");

        ShaderProperty.VectorValue nebulaPos_Property = new ShaderProperty.VectorValue("_Nebula_Pos");

        // Use this for initialization
        void OnEnable() {
            nebulaTex_Property.GlobalValue = backPaintTexture;

        }

        // Update is called once per frame
        void Update() {

            if (paintCamera)
                paintCamera.Render();

            nebulaPos_Property.GlobalValue = transform.position.ToVector4(transform.localScale.x);

        }
    }
}
