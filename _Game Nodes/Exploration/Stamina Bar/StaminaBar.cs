using PlayerAndEditorGUI;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes_Visual {
    
    [ExecuteAlways]
    public class StaminaBar : MonoBehaviour, IPEGI {

        [SerializeField] protected Image _image;
        [SerializeField] protected TextMeshProUGUI _staminaCount;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip onFillCrossTheCenter;
        [SerializeField] protected AudioClip onHitCrossTheCenter;
        [SerializeField] protected AudioClip onHitBelow;
        [SerializeField] protected AudioClip onHitAbove;
        [SerializeField] protected Graphic centralLine;
        
        ShaderProperty.FloatValue _staminaLineInShader = new ShaderProperty.FloatValue("_NodeNotesStaminaPortion");
        private float _staminaLine = 1f;

        private bool Above => _staminaLine >= 0.5f;
        
        ShaderProperty.FloatValue _previousStaminaLineInShader = new ShaderProperty.FloatValue("_NodeNotesStaminaPortion_Prev");
        private float _previousStaminaLine = 1f;

        private float showPreviousTimer;

        private ShaderProperty.FloatValue _staminaCurve = new ShaderProperty.FloatValue("_NodeNotes_StaminaCurve");
        [SerializeField] private float _staminaCurveValue = 3;

        public float StaminaCurve
        {
            get { return _staminaCurveValue; }
            set
            {
                var prev = StaminaPortion;

                _staminaCurveValue = value;

                StaminaPortion = prev;

                _staminaCurve.GlobalValue = _staminaCurveValue;
            }
        }

        private void Play(AudioClip clip, float pitch = 1)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }

        // Start is called before the first frame update
        void Reset()
        {
            _image = GetComponent<Image>();
        }
        
        public float StaminaPoints
        {
            get { return (StaminaPortion * 100f); }
            set
            { StaminaPortion = value * 0.01f;}
        }

        private float StaminaPortion
        {
            get
            {
                var above = _staminaLine >= 0.5f;

                float off = (above ?  _staminaLine - 0.5f : 0.5f - _staminaLine) * 2; // 1

                off = 1 - off; // 2

                float thickness = Mathf.Pow(off, 1 + StaminaCurve); // 3

                thickness *= 0.5f; // 4

                return above ? 1f - thickness : (thickness); // 5
            }

            set
            {
                var above = value > 0.5f;

                value = above ? 1f - value : value; //5

                value *= 2; // 4

                value = Mathf.Pow(value, 1f/(1f+ StaminaCurve)); //3

                value = 1 - value; // 2

                value *= 0.5f;

                value = (above ? value + 0.5f : 0.5f - value);

                _staminaLine = value;

            }
        }
        
        // Update is called once per frame
        void Update()
        {

            if (Application.isPlaying)
            {

                bool above = Above;

                _staminaLine = Mathf.Clamp01(_staminaLine + Time.deltaTime * 0.005f);

                if (Above && !above)
                    audioSource.PlayOneShot(onFillCrossTheCenter);

                if (Input.GetKeyDown(KeyCode.Alpha1))
                    Use(1);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    Use(2);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    Use(4);
                
            }

            //StaminaCurve = 1 + Mathf.Pow(Mathf.Clamp01(_staminaLine * 2),2) * 5;

            _staminaLineInShader.GlobalValue = _staminaLine;
            
            if (showPreviousTimer > 0)
            {
                if (_staminaLine >= _previousStaminaLine)
                    showPreviousTimer = 0;

                showPreviousTimer -= Time.deltaTime;
            }
            else
            {
                LerpUtils.IsLerpingBySpeed(ref _previousStaminaLine, _staminaLine, 0.1f);
            }

            _previousStaminaLineInShader.GlobalValue = _previousStaminaLine;

            if (_staminaCount)
                _staminaCount.text = ((int)StaminaPoints).ToString();

            _staminaCount.color = Above ? Color.LerpUnclamped(Color.yellow, Color.green, (_staminaLine-0.5f) * 2) : Color.LerpUnclamped(Color.magenta, Color.blue, _staminaLine*2);

            centralLine.TrySetAlpha(Above ? 1 : 0);
        }

        void OnEnable()
        {
            _staminaCurve.GlobalValue = _staminaCurveValue;
        }

        public void Use(int cost)
        {

            var pnts = StaminaPoints;

            if (pnts >= cost)
            {
                var above = Above;

                _previousStaminaLine = _staminaLine;

                showPreviousTimer = 1f;

                StaminaPoints = pnts - cost;
                
                if (!above)
                    Play(onHitBelow, 0.5f + _staminaLine);
                else if (above && !Above)
                    Play(onHitCrossTheCenter, Mathf.Pow(_staminaLine*2.1f, 6));
                else
                    Play(onHitAbove, 1f + (1f-_staminaLine));

            }

        }

        private bool InspectSkill(string name, int cost)
        {
            
            var points = (int)StaminaPoints;

            if (points > cost && "{0} [{1} st]".F(name, cost).Click().nl())
            {
                Use(cost);
                return true;
            }
            
            return false;
        }

        public bool Inspect()
        {
            pegi.EditorView.Lock_UnlockClick(gameObject);
            pegi.toggleDefaultInspector(this).nl();

            var curve = StaminaCurve;

            if ("Stamina Curve".edit(ref curve, 0, 10f).nl())
                StaminaCurve = curve;

              

            "Stamina".edit01(40, ref _staminaLine).nl();

            int points = (int)StaminaPoints;

            if ("Points".editDelayed(ref points).nl())
                StaminaPoints = points;

            InspectSkill("Shoot", 1).nl();

            InspectSkill("Kick", 4).nl();

            InspectSkill("Spell", 10).nl();

            return false;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(StaminaBar))]
    public class StaminaBarDrawer : PEGI_Inspector_Mono<StaminaBar> { }
    #endif

}