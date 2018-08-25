using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharedTools_Stuff;

namespace LinkedNotes
{

    [ExecuteInEditMode]
    public class CreateNodeButton : MonoBehaviour
    {

        public static bool showCreateButtons = false;

        public Image image;
        public List<GameObject> children = new List<GameObject>();
        public float delay = 0f;
        float timer = 0;
        public bool toLeft;

        Vector3 startPosition = Vector3.zero;

        private void Start() {
            if (!image)
                image = GetComponent<Image>();

            if (children.Count == 0)
                for (int i = 0; i < transform.childCount; i++)
                    children.Add(transform.GetChild(i).gameObject);

            startPosition = transform.position;

            if (Application.isPlaying)
                transform.position = dest;
        }

        void SetEnabled(bool to)  {
            image.enabled = to;
            foreach (var c in children)
                if (c)
                    c.SetActive(to);
            timer = delay;
        }

        Vector3 dest => showCreateButtons ? startPosition : startPosition + Vector3.right * Screen.width * 0.2f * (toLeft ? -1 : 1);

        // Update is called once per frame
        void Update() {
            if (Application.isPlaying) {

                if (timer > 0)
                    timer -= Time.deltaTime;
                else  {
                    if (image.enabled) {

                   
                        float portion;
                        transform.position = MyMath.Lerp(transform.position, dest, Screen.width, out portion);

                        if (portion == 1) {
                            if (!showCreateButtons)
                                SetEnabled(false);
                            else
                                timer = delay;
                        }
                    }
                    else
                    if (showCreateButtons)
                        SetEnabled(true);
                }
            }
        }
    }
}
