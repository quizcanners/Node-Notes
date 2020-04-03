using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes_Visual {

    public class NodeNotes_UI_Camera : MonoBehaviour, IPEGI
    {

        public static NodeNotes_UI_Camera inst;

        [SerializeField] private Camera _camera;

        #region Inspector
        private int _inspectedStuff = -1;

        public bool Inspect() {

            var changed = false;

            pegi.toggleDefaultInspector(this).nl();

            var inst = Shortcuts.Instance;

            "Camera".edit(ref _camera).changes(ref changed);

            if (!_camera && icon.Search.Click())
                _camera = GetComponent<Camera>();

            pegi.nl();

            if (!inst)
                "No Shortcuts".writeWarning();
            else if (changed)
                inst.SetToDirty();

            return changed;
        }

        #endregion

       /* public float speed = 10;
        public float sensitivity = 5;
        private float _rotationY;
        private Vector3 centeredPosition = Vector3.zero;

        public Vector3 relativePosition = Vector3.zero;*/

        public void Update() {

          /*  if (_fps)
            {

                transform.position = centeredPosition;

                var add = Vector3.zero;

                var tf = transform;

                if (Input.GetKey(KeyCode.W)) add += tf.forward;
                if (Input.GetKey(KeyCode.A)) add -= tf.right;
                if (Input.GetKey(KeyCode.S)) add -= tf.forward;
                if (Input.GetKey(KeyCode.D)) add += tf.right;

                relativePosition += add * speed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? 3f : 1f);
                
                var eul = tf.localEulerAngles;

                var rotationX = eul.y;
                _rotationY = eul.x;

                rotationX += Input.GetAxis("Mouse X") * sensitivity;
                _rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

                _rotationY = _rotationY < 120 ? Mathf.Min(_rotationY, 85) : Mathf.Max(_rotationY, 270);

                tf.localEulerAngles = new Vector3(_rotationY, rotationX, 0);

            }*/

        }

        private void OnEnable()
        {
            inst = this;
        }

   
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotes_UI_Camera))]
    public class NodeNotes_CameraDrawer : PEGI_Inspector_Mono<NodeNotes_UI_Camera> { }
#endif
}