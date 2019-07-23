using System;
using System.Collections.Generic;
using System.Xml.Schema;
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

    public RectTransform separatorLine;

    private float Separator => separatorPosition.CurrentValue;

    [NonSerialized] protected List<string> history = new List<string>();
    protected List<DialogueUI_SpeechBox> historyPool = new List<DialogueUI_SpeechBox>();

    [NonSerialized] protected List<string> options = new List<string>();
    protected List<DialogueUI_SpeechBox> optionsPool = new List<DialogueUI_SpeechBox>();

    [Serializable]
    public class ScrollData {
        public float gap = 100;
        [NonSerialized] public float offset;
        [NonSerialized] public float innertia;

        public void AddOffset(float off) {
            offset += off;
            innertia = (innertia * 4 + off/Time.deltaTime) * 0.2f;
        }

        public void FadeInnertia() => innertia = Mathf.Lerp(innertia, 0, Time.deltaTime * 10);
        

        public void ApplyInnertia() {

            offset += innertia * Time.deltaTime;

            LerpUtils.IsLerpingBySpeed(ref innertia, 0, 5000);
        }

    }

    public ScrollData historyScroll = new ScrollData();
    public ScrollData optionsScroll = new ScrollData();

  //  public float separator = 0.4f;

    private enum ScrollingState { None, ScrollingOptions, ScrollingHistory }
    private ScrollingState state;

    private Vector2 prevousMousePos;

    LerpData ld = new LerpData();

    LinkedLerp.FloatValue separatorPosition = new LinkedLerp.FloatValue("Separator", 0.5f, 2);

    public void Update() {

        ld.Reset();


        separatorPosition.targetValue = Mathf.Min(0.6f, optionsPool.Count * 0.2f);

     


        historyPool.Portion(ld);
        optionsPool.Portion(ld);
        separatorPosition.Portion(ld);

        historyPool.Lerp(ld);
        optionsPool.Lerp(ld);

        if (separatorPosition.targetValue != separatorPosition.CurrentValue) {
            separatorPosition.Lerp(ld);
            UpdateCourners();
        }

        const float scrollbackSpeed = 1000;

        if (state == ScrollingState.None) {
            if (Input.GetMouseButton(0)) {
                state = (Input.mousePosition.y / Screen.height) > separatorPosition.CurrentValue
                    ? ScrollingState.ScrollingHistory
                    : ScrollingState.ScrollingOptions;

                prevousMousePos = Input.mousePosition;
            }
        } else {

            if (!Input.GetMouseButton(0))
                state = ScrollingState.None;
            else {
                float diff = Input.mousePosition.y - prevousMousePos.y;

                (state == ScrollingState.ScrollingHistory ? historyScroll : optionsScroll).AddOffset(diff);
                
                prevousMousePos = Input.mousePosition;
            }
        }

        if (scrollHistoryUpRequested && state != ScrollingState.None)
            scrollHistoryUpRequested = false;
        
        float pos = historyScroll.offset;

        foreach (var h in historyPool) {
            var rt = h.rectTransform;
            var anch = rt.anchoredPosition;
            anch.y = pos;
            rt.anchoredPosition = anch;
            pos += historyScroll.gap;
        }
        
        if (state != ScrollingState.ScrollingHistory) {

            historyScroll.ApplyInnertia();

            bool outOfList = true;

            if (historyScroll.offset > 0 || scrollHistoryUpRequested) {
                LerpUtils.IsLerpingBySpeed(ref historyScroll.offset, 0, scrollbackSpeed);
            }
            else if (pos < historyScroll.gap) {
                LerpUtils.IsLerpingBySpeed(ref historyScroll.offset, historyScroll.offset + historyScroll.gap - pos, scrollbackSpeed);
            }
            else
            
                outOfList = false;
            
            if (outOfList)
                historyScroll.FadeInnertia();
           

        }

        pos = optionsScroll.offset;

        foreach (var h in optionsPool) {
            var rt = h.rectTransform;
            var anch = rt.anchoredPosition;
            anch.y = pos;
            rt.anchoredPosition = anch;
            pos -= optionsScroll.gap;
        }
        
        if (state != ScrollingState.ScrollingOptions) {
            
            optionsScroll.ApplyInnertia();

            bool outOfTheList = true;

            if (optionsScroll.offset < 0) {
                LerpUtils.IsLerpingBySpeed(ref optionsScroll.offset, 0, scrollbackSpeed);
            }
            else if (pos > -optionsScroll.gap) {
                LerpUtils.IsLerpingBySpeed(ref optionsScroll.offset, optionsScroll.offset - optionsScroll.gap - pos, scrollbackSpeed);
            }
            else
                outOfTheList = false;
            
            if (outOfTheList)
                optionsScroll.FadeInnertia();
        }

    }

    public override void Initialize()
    {
        base.Initialize();
        instance = this;
    }

    void UpdateCourners() {

        if (historyPool.Count > 0) {
            if (historyPool.Count > 1)
                foreach (var box in historyPool) {
                    box.isLast = false;
                    box.isFirst = false;
                    box.graphic.FadeFromY = Separator;
                }

            historyPool[0].isLast = true;
            historyPool.Last().isFirst = true;
        }

        if (optionsPool.Count > 0) {

            if (optionsPool.Count > 1)
                for (int i = 0; i<optionsPool.Count; i++) {
                    var box = optionsPool[i];
                    box.isLast = false;
                    box.isFirst = false;
                    box.index = i;
                    box.graphic.FadeToY = Separator;
                }

            optionsPool[0].isFirst = true;
            optionsPool.Last().isLast = true;
        }


        var min = separatorLine.anchorMin;
        var max = separatorLine.anchorMax;

        min.y = Separator;
        max.y = Separator;
        separatorLine.anchorMin = min;
        separatorLine.anchorMax = max;

        separatorLine.anchoredPosition = Vector2.zero;
        

        if (lowerText && upperText) {
            textMeshProEdgeFading.SetOn(lowerText, new Vector4(0, 0, 1, Separator));
            textMeshProEdgeFading.SetOn(upperText, new Vector4(0, Separator, 1, 1));
        }

    }

    void OnEnable() {
      

        UpdateCourners();

    }

    public void Click(int index) {
        Debug.Log("Clicked "+ index );
    }

    private int inspectedHistory = -1;
    private int inspectedOptions = -1;

    public RectTransform aboveTheLineParent;

    public RectTransform belowTheLineParent;

    private string tmpText;

    private bool scrollHistoryUpRequested = false;

    private void AddToOptions(string text)
    {
        var newH = Instantiate(optionPrefab, belowTheLineParent);
        optionsPool.Add(newH);
        newH.Text = text;

        UpdateCourners();
    }

    private void AddToHistory(string text)
    {
        var newH = Instantiate(conversationHistoryPrefab, aboveTheLineParent);
        historyPool.Insert(0, newH);
        newH.Text = text;

        historyScroll.offset -= historyScroll.gap;

        scrollHistoryUpRequested = true;

        UpdateCourners();
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

        var sp = Separator;
        if ("Separator ".edit(ref sp, 0, 1).nl(ref changed))
        {
            separatorPosition.targetValue = sp;
            separatorPosition.CurrentValue = sp;
        }

        if (changed)
            UpdateCourners();

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
