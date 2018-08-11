using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SharedTools_Stuff;

namespace LinkedNotes {

    [ExecuteInEditMode]
    public class NodeCircleController : MonoBehaviour {

        public TextMeshPro text;

        public Node source;

        float size;

        public bool assumedPosition;


        private void OnEnable()
        {
            if (!text)
                text = GetComponentInChildren<TextMeshPro>();
        }

        public void Init(Node node)
        {
            source = node;
            assumedPosition = false;
        }


        // Update is called once per frame
        void Update() {

            if (source != null)
            {


                var lp = transform.localPosition;

                if (!assumedPosition)
                {
                    float portion;

                    transform.localPosition = MyMath.Lerp(lp, source.localPosition, 1, out portion);

                    if (portion == 1)
                        assumedPosition = true;
                }
                else
                {


                    source.localPosition = lp;
                }
            }

        }
    }
}