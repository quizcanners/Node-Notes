using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes_Visual {

    public class NodeNotes_Scene_Camera : MonoBehaviour, IPEGI, ICfg
    {

        public static NodeNotes_Scene_Camera inst;

        [SerializeField] private GodMode _camera;

        public void Decode(string data)
        {
            new CfgDecoder(data).DecodeTagsFor(this);
        }

        public bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "gm": _camera.Decode(data); break;

                default: return false;
            }

            return true;
        }

        public CfgEncoder Encode()
        {
            var cody = new CfgEncoder()
                .Add("gm", _camera);



            return cody;

        }

        #region Inspector


        public bool Inspect() {

            var changed = false;

            pegi.toggleDefaultInspector(this).nl();

            var inst = Shortcuts.Instance;

            "Camera".edit(ref _camera).changes(ref changed);

            if (!_camera && icon.Search.Click())
                _camera = GetComponent<GodMode>();

            pegi.nl();

            if (!inst)
                "No Shortcuts".writeWarning();
            else if (changed)
                inst.SetToDirty();

            return changed;
        }

        #endregion

        private void OnEnable()
        {
            inst = this;
        }

   
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotes_Scene_Camera))]
    public class NodeNotes_CameraDrawer : PEGI_Inspector_Mono<NodeNotes_Scene_Camera> { }
#endif
}