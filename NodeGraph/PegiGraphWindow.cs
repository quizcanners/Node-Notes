using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace PlayerAndEditorGUI.NodeGraph
{
    public class PegiGraphWindow : EditorWindow
    {
        [NonSerialized] private PegiGraphView graphView;
        [SerializeField] private Object target;
        
        public void Show<T>(T rootNode) where T: IPEGI_Node
        {
            target = rootNode as Object;
            Initialize();
        }

        public void OnEnable() => Initialize();
        
        private void Initialize()
        {
            if (target)
            {

                if (graphView == null)
                {
                    graphView = new PegiGraphView()
                    {
                        name = target.GetNameForInspector_Uobj(),
                    };

                    graphView.StretchToParentSize();

                    rootVisualElement.Add(graphView);
                }

                graphView.SetTarget(target as IPEGI_Node);

                Show();
            }
            
        }
    }
}