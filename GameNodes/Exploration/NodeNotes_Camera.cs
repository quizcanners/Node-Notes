using System;
using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using NodeNotes_Visual.ECS;
using UnityEngine;
using PlayerAndEditorGUI;
using PlaytimePainter;
using QuizCannersUtilities;
using Unity.Entities;
using Unity.Transforms;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes_Visual {

    public class NodeNotes_Camera : MonoBehaviour, IPEGI
    {

        public static NodeNotes_Camera inst;

        [SerializeField] private Camera _camera;

        #region Inspector
        private int _inspectedStuff = -1;

        public bool Inspect() {

            var changed = false;

            var inst = Shortcuts.Instance;

            "Camera".edit(ref _camera).changes(ref changed);

            if (!_camera && icon.Search.Click())
                _camera = GetComponent<Camera>();

            pegi.nl();

            if (!inst)
                "No Shortcuts".writeWarning();
            else {




                "FPS camera".enter_Inspect(inst.FpsCamera, ref _inspectedStuff, 0).nl(ref changed);
                "Default Camera".enter_Inspect(inst.defaultCamera, ref _inspectedStuff, 1).nl(ref changed);

                
            }

            if (changed)
                inst.SetToDirty();

            return changed;
        }

        #endregion


        public bool FPS {

            set {

                var scts = Shortcuts.Instance;

                (value ? scts.FpsCamera : scts.defaultCamera).To(_camera);

                _fps = value;
            }
        }

        public float speed = 10;
        private bool _fps = false;
        public float sensitivity = 5;
        private float _rotationY;
        private Vector3 centeredPosition = Vector3.zero;

        public Vector3 relativePosition = Vector3.zero;

        public void Update() {

            if (_fps)
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

            }

        }

        private void OnEnable()
        {
            inst = this;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotes_Camera))]
    public class NodeNotes_CameraDrawer : PEGI_Inspector_Mono<NodeNotes_Camera> { }
#endif
}