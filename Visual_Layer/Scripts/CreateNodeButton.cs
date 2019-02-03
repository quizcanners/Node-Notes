using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuizCannersUtilities;

namespace NodeNotes
{

    [ExecuteInEditMode]
    public class CreateNodeButton : MonoBehaviour
    {

        public static bool showCreateButtons = false;

        public Graphic[] graphicElements;
        public List<GameObject> children = new List<GameObject>();
        public float delay = 0f;
        float timer = 0;
        public bool toLeft;
        public RectTransform rectTranform;

        Vector2 startPosition = Vector3.zero;

        private void OnEnable()
        {
            if (graphicElements.IsNullOrEmpty())
                graphicElements = GetComponentsInChildren<Graphic>();
            
        }

        private void Start() {

            if (children.Count == 0)
                for (int i = 0; i < transform.childCount; i++)
                    children.Add(transform.GetChild(i).gameObject);

            if (!rectTranform)
                rectTranform = GetComponent<RectTransform>();

            startPosition = rectTranform.anchoredPosition;

            if (Application.isPlaying)
                rectTranform.anchoredPosition = Dest;
        }

        void SetEnabled(bool to)  {

            foreach (var e in graphicElements)
                if (e) e.enabled = to;

            foreach (var c in children)
                if (c)
                    c.SetActive(to);
            timer = delay;
        }

        Vector2 Dest => showCreateButtons ? startPosition : startPosition + Vector2.right * Screen.width * 0.2f * (toLeft ? -1 : 1);

        // Update is called once per frame
        void Update() {
            if (Application.isPlaying) {

                if (timer > 0)
                    timer -= Time.deltaTime;
                else  {
                    if (graphicElements[0].enabled) {
                        
                        float portion;
                        rectTranform.anchoredPosition = MyMath.Lerp(rectTranform.anchoredPosition, Dest, Screen.width, out portion);

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
