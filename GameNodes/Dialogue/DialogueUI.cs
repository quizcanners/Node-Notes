using System;
using System.Collections.Generic;
using NodeNotes;
using NodeNotes_Visual;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DialogueUI : GameControllerBase, IPEGI, IManageFading {

    public static DialogueUI instance;
    
    private ShaderProperty.VectorValue textMeshProEdgeFading = new ShaderProperty.VectorValue("_FadeRange");

    [SerializeField] protected Material upperText;

    [SerializeField] protected Material lowerText;

    [SerializeField] protected RectTransform separatorLine;

    [SerializeField] protected Graphic singlePhraseBg;

    [SerializeField] protected TextMeshProUGUI singlePhraseText;

    [SerializeField] protected AudioSource audioSource;


    private float Separator => separatorPosition.CurrentValue;

    public string SingleText {
        set {
            
            if (!SingleText.IsNullOrEmpty())
                AddToHistory(SingleText);
            
            singlePhraseText.text = value;
            
            ClearOptions();

        }
        get { return singlePhraseText.text; }
    }
    
    private void AddToOptions(string text, int index)
    {

        var opt = optionsPool.GetOne(belowTheLineParent);
        opt.Text = text;
        opt.index = index;
        opt.TryFadeIn();

        UpdateCourners();

        poolsDirty = true;
    }

    public void AddToHistory(string text) {

        var newH = historyPool.GetOne(aboveTheLineParent, true);
        newH.Text = text;
        newH.TryFadeIn();

        historyScroll.offset -= historyScroll.gap;

        scrollHistoryUpRequested = true;

        UpdateCourners();

        poolsDirty = true;
    }

    public void ClearOptions()
    {
        foreach (var o in optionsPool)
            o.FadeAway();
    }

    private List<string> options;

    public List<string> Options
    {
        set
        {
            options = value;

            SingleText = "";

            for (int i = 0; i < value.Count; i++) //(var txt in value) 
                AddToOptions(value[i], i);

        }
    }


    [Serializable]
    public class SpeedPool : PoolSimple<DialogueUI_SpeechBox>{
        public SpeedPool(string name) : base(name){}

        public SpeedPool() : base("pool") { }
    }

    public SpeedPool historyPool = new SpeedPool("History");
    public SpeedPool optionsPool = new SpeedPool("Options");
    
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

    private enum ScrollingState { None, ScrollingOptions, ScrollingHistory }
    private ScrollingState state;

    private Vector2 prevousMousePos;

    private bool poolsDirty;

    LerpData ld = new LerpData();

    LinkedLerp.FloatValue separatorPosition = new LinkedLerp.FloatValue("Separator", 0.5f, 2);
    LinkedLerp.FloatValue singlePhraseBoxHeight = new LinkedLerp.FloatValue("Single Box size", 0, 2);

    private bool scrollSoundPlayed = false;
    private bool scrollSoundPlayedUp;


    public void Update() {

        ld.Reset();

        int activeCount = 0;

        foreach (var op in optionsPool)
            activeCount += op.isFadingOut ? 0 : 1;
        
        singlePhraseBoxHeight.Portion(ld, activeCount > 0 ? 0 : 1);
        historyPool.active.Portion(ld);
        optionsPool.active.Portion(ld);
        separatorPosition.Portion(ld, Mathf.Min(0.6f, 0.3f + activeCount * 0.2f));

        historyPool.active.Lerp(ld);
        optionsPool.active.Lerp(ld);
        singlePhraseBoxHeight.Lerp(ld);

        var tf = singlePhraseBg.rectTransform;
        var size = tf.sizeDelta;
        float curBoxFadeIn = singlePhraseBoxHeight.CurrentValue;
        size.y = curBoxFadeIn * 400;
        singlePhraseBg.TrySetAlpha_DisableIfZero(curBoxFadeIn * 20);
        singlePhraseText.TrySetAlpha_DisableIfZero((curBoxFadeIn - 0.9f)*10);
        tf.sizeDelta = size;

        if (separatorPosition.TargetValue != separatorPosition.CurrentValue || poolsDirty) {
            poolsDirty = false;
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

                scrollSoundPlayed = false;
            }
        } else {

            if (!Input.GetMouseButton(0))
                state = ScrollingState.None;
            else {
                float diff = Input.mousePosition.y - prevousMousePos.y;

                bool up = diff > 0;

                if ((!scrollSoundPlayed || (up != scrollSoundPlayedUp)) && Mathf.Abs(diff)>10)
                {
                    scrollSoundPlayedUp = up;
                    scrollSoundPlayed = true;
                    audioSource.PlayOneShot(Shortcuts.Instance.onSwipeSound);
                }

                (state == ScrollingState.ScrollingHistory ? historyScroll : optionsScroll).AddOffset(diff);
                
                prevousMousePos = Input.mousePosition;
            }
        }

        if (scrollHistoryUpRequested && state != ScrollingState.None)
            scrollHistoryUpRequested = false;
        
        float pos = historyScroll.offset;

        foreach (var h in historyPool.active) {
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

        foreach (var h in optionsPool.active) {
            if (!h.isFadingOut) {
                var rt = h.rectTransform;
                var anch = rt.anchoredPosition;
                anch.y = pos;
                rt.anchoredPosition = anch;
                pos -= optionsScroll.gap;
            }
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

            foreach (var box in historyPool.active) {
                box.isLast = false;
                box.isFirst = false;
                box.graphic.FadeFromY = Separator;
            }

            historyPool.active[0].isLast = true;
            historyPool.active.Last().isFirst = true;
        }

        if (optionsPool.Count > 0) {
            
            for (int i = 0; i< optionsPool.Count; i++) {
                var box = optionsPool.active[i];
                box.isLast = false;
                box.isFirst = false;
                box.graphic.FadeToY = Separator;
            }

            optionsPool.active[0].isFirst = true;
            optionsPool.active.Last().isLast = true;
        }


        var min = separatorLine.anchorMin;
        var max = separatorLine.anchorMax;

        min.y = Separator;
        max.y = Separator;
        separatorLine.anchorMin = min;
        separatorLine.anchorMax = max;

        separatorLine.anchoredPosition = Vector2.zero;
        
        if (lowerText && upperText) {
            const float padding = 0.2f;
            const float minP = -padding;
            const float maxP = 1f + padding;

            textMeshProEdgeFading.SetOn(lowerText, new Vector4(minP, 0, maxP, Separator));
            textMeshProEdgeFading.SetOn(upperText, new Vector4(minP, Separator, maxP, 1));
        }

    }

    void OnEnable() {
        UpdateCourners();
    }

    void OnDisable() {
        historyPool.DeleteAll();
        optionsPool.DeleteAll();
    }

    public void Click(int index)
    {
        AddToHistory(options[index]);

        DialogueNode.SelectOption(index);

        audioSource.PlayOneShot(Shortcuts.Instance.onMouseClickSound);

    }

    public void ClickNext()
    {
        DialogueNode.SelectOption(0);

        audioSource.PlayOneShot(Shortcuts.Instance.onMouseDownButtonSound);
    }
    
    public void Exit()
    {

        audioSource.PlayOneShot(Shortcuts.Instance.onMouseClickSound);
        DialogueNode.enteredInstance?.Exit();
    }

    public RectTransform aboveTheLineParent;

    public RectTransform belowTheLineParent;

    private string tmpText;

    private bool scrollHistoryUpRequested = false;

    public bool Inspect() {

        var changed = false;

        pegi.nl();

        historyPool.Nested_Inspect().nl(ref changed);

        optionsPool.Nested_Inspect().nl(ref changed);
        
        pegi.editBig(ref tmpText);

        if ("Add History".Click())
            AddToHistory(tmpText);

        if ("Add Option".Click())
            AddToOptions(tmpText, optionsPool.Count);

        pegi.nl();

        var sp = Separator;
        if ("Separator ".edit(ref sp, 0, 1).nl(ref changed)) {
            separatorPosition.TargetValue = sp;
            separatorPosition.CurrentValue = sp;
        }

        if (changed)
            UpdateCourners();

        return changed;
    }

    public void FadeAway() {
        optionsPool.DeleteAll();
        historyPool.DeleteAll();
        gameObject.SetActive(false);
    }

    public bool TryFadeIn()   {
        SingleText = "";
        optionsPool.DeleteAll();
        historyPool.DeleteAll();
      
        gameObject.SetActive(true);
        return true;
    }
}
