using System;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using Unity.Entities;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueUI : GameControllerBase, IPEGI, IManageFading {

    public static DialogueUI instance;


    private ShaderProperty.VectorValue textMeshProEdgeFading = new ShaderProperty.VectorValue("_FadeRange");
    
    public DialogueUI_SpeechBox conversationHistoryPrefab;

    public DialogueUI_SpeechBox optionPrefab;
    
    public Material upperText;

    public Material lowerText;

    [NonSerialized] protected List<string> history = new List<string>();
    protected List<DialogueUI_SpeechBox> historyPool = new List<DialogueUI_SpeechBox>();

    [NonSerialized] protected List<string> options = new List<string>();
    protected List<DialogueUI_SpeechBox> optionsPool = new List<DialogueUI_SpeechBox>();


    public float separator = 0.4f;

    public float historyGap = 130;
    public float optionsGap = 60;

    private float historyOffset = 0;
    private float optionsOffset = 0;
    private enum ScrollingState { None, ScrollingOptions, ScrollingHistory }
    private ScrollingState state;

    private Vector2 prevousMousePos;
    public void Update()
    {

        const float scrollbackSpeed = 1000;

        if (state == ScrollingState.None)
        {
            if (Input.GetMouseButton(0))
            {
                state = (Input.mousePosition.y / Screen.height) > separator
                    ? ScrollingState.ScrollingHistory
                    : ScrollingState.ScrollingOptions;

                prevousMousePos = Input.mousePosition;
            }
        }
        else
        {

            if (!Input.GetMouseButton(0))
                state = ScrollingState.None;
            else
            {
                float diff = Input.mousePosition.y - prevousMousePos.y;

                if (state == ScrollingState.ScrollingHistory)
                    historyOffset += diff;
                else
                    optionsOffset += diff;

                prevousMousePos = Input.mousePosition;
            }


        }
        
        float pos = historyOffset;

        foreach (var h in historyPool) {
            var rt = h.rectTransform;
            var anch = rt.anchoredPosition;
            anch.y = pos;
            rt.anchoredPosition = anch;
            pos += historyGap;
        }



        if (state != ScrollingState.ScrollingHistory) {
            if (historyOffset > 0) {
                LerpUtils.IsLerpingBySpeed(ref historyOffset, 0, scrollbackSpeed);
            }
            else if (pos < historyGap) {
                LerpUtils.IsLerpingBySpeed(ref historyOffset, historyOffset + historyGap - pos, scrollbackSpeed);
            }
        }

        pos = optionsOffset;

        foreach (var h in optionsPool) {
            var rt = h.rectTransform;
            var anch = rt.anchoredPosition;
            anch.y = pos;
            rt.anchoredPosition = anch;
            pos -= optionsGap;
        }


        if (state != ScrollingState.ScrollingOptions)
        {
            if (optionsOffset < 0)
            {
                LerpUtils.IsLerpingBySpeed(ref optionsOffset, 0, scrollbackSpeed);
            }
            else if (pos > - optionsGap)
            {
                LerpUtils.IsLerpingBySpeed(ref optionsOffset, optionsOffset - optionsGap - pos, scrollbackSpeed);
            }
        }

    }

    public override void Initialize()
    {
        base.Initialize();
        instance = this;
    }

    void OnEnable() {
        if (lowerText && upperText) {
            textMeshProEdgeFading.SetOn(lowerText, new Vector4(0, 0, 1, separator));
            textMeshProEdgeFading.SetOn(upperText, new Vector4(0, separator, 1, 1));
        }
    }

    private int inspectedHistory = -1;
    private int inspectedOptions = -1;

    public RectTransform aboveTheLineParent;

    public RectTransform belowTheLineParent;

    private string tmpText;

    private void AddToOptions(string text)
    {
        var newH = Instantiate(optionPrefab, belowTheLineParent);
        optionsPool.Add(newH);
        newH.Text = text;
    }

    private void AddToHistory(string text)
    {
        var newH = Instantiate(conversationHistoryPrefab, aboveTheLineParent);
        historyPool.Add(newH);
        newH.Text = text;
    }

    public bool Inspect() {
        var changed = false;

        pegi.nl();

        "History".edit_List_MB(ref historyPool, ref inspectedHistory).nl(ref changed);
        "Options".edit_List_MB(ref optionsPool, ref inspectedOptions).nl(ref changed);

        if ("Add History".Click()) 
            AddToHistory(tmpText);

        if ("Add to Options".Click())
            AddToOptions(tmpText);

        pegi.nl();

        pegi.editBig(ref tmpText);

        return changed;
    }

    public void FadeAway() {
        gameObject.SetActive(false);
    }

    public bool TryFadeIn()   {
        gameObject.SetActive(true);
        return true;
    }
}
