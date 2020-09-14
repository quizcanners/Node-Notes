using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using UnityEngine.Audio;

namespace NodeNotes_Visual
{

#pragma warning disable IDE0018 // Inline variable declaration

    public class AmbientSoundsMixerMgmt : PresentationSystemsAbstract, IPEGI
    {
        public static AmbientSoundsMixerMgmt instance;

        public AudioMixerGroup ActiveAmbientGroup;
        public AudioMixerGroup FadingAmbientGroup;

        public AudioMixerSnapshot Normal;
        public AudioMixerSnapshot FadeToBehindTheWall;
        public AudioMixerSnapshot FadeToDistance;

        private bool _activeIsA;
        public AudioSource MainMusicA;
        public AudioSource MainMusicB;
        private AudioSource ActiveMusicSource => _activeIsA ? MainMusicA : MainMusicB;
        private AudioSource FadingMusicSource => _activeIsA ? MainMusicB : MainMusicA;

        private void Swap()
        {

            if (ActiveMusicSource.clip)
            {
                _cutoffTime[ActiveMusicSource.clip] = ActiveMusicSource.time;
            }

            _activeIsA = !_activeIsA;

            ActiveMusicSource.outputAudioMixerGroup = ActiveAmbientGroup;
            FadingMusicSource.outputAudioMixerGroup = FadingAmbientGroup;

            _timer = _transitionLength;
        }

        private string targetMusic;
        private string currentMusic;
        private TransitionType transitionType = TransitionType.BehindTheWall;
        enum TransitionType { BehindTheWall = 0, IntoDistance = 1 }


        private float _transitionLength = 3f;
        private float _timer = 0;

        private Dictionary<AudioClip, float> _cutoffTime = new Dictionary<AudioClip, float>();

        // TODO: Remember the time where the sound stopped

        private void Update()
        {
            if (_timer >= 0)
            {
                _timer -= Time.deltaTime;

                float portion = Mathf.Clamp01(_timer / _transitionLength);

                FadingMusicSource.volume = portion;
                ActiveMusicSource.volume = 1 - portion;

                if (_timer < 0)
                {
                    FadingMusicSource.Stop();
                    Normal.TransitionTo(0);
                }
            }
            else
            {
                if (!targetMusic.IsNullOrEmpty() && !targetMusic.Equals(currentMusic))
                {
                    Swap();

                    AudioClip song;
                    if (Shortcuts.Assets.Get(targetMusic, out song))
                    {
                        ActiveMusicSource.clip = song;
                        ActiveMusicSource.loop = true;
                        float time;
                        if (_cutoffTime.TryGetValue(song, out time))
                        {
                            ActiveMusicSource.time = time;
                        }
                        ActiveMusicSource.Play();
                    }

                    float trns = _transitionLength*0.5f;

                    switch (transitionType)
                    {
                        case TransitionType.IntoDistance: FadeToDistance.TransitionTo(trns); break;
                        case TransitionType.BehindTheWall:
                        default: FadeToBehindTheWall.TransitionTo(trns); break;
                    }

                  

                    currentMusic = targetMusic;
                }
            }
        }

        public override void ManagedOnEnable()
        {
            instance = this;
        }

        public void OnEnable() => ManagedOnEnable();

        #region Encode & Decode
        public override string ClassTag => "Ambnt";
        
        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "m":   targetMusic = data; break;
                case "tr":  transitionType = (TransitionType)data.ToInt(); break;
                case "len": _transitionLength = data.ToFloat(); break;
                default: return true;
            }

            return false;
        }

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotEmpty("m", targetMusic)
            .Add("tr", (int)transitionType)
            .Add("len", _transitionLength);

        #endregion
        
        #region Inspect

        public override bool Inspect()
        {
            var changed = false;

            "Ambient".select(60, ref targetMusic, Shortcuts.Assets.GetAudioClipObjectsKeys()).nl(ref changed);

            "Transition type".editEnum(100, ref transitionType).nl(ref changed);

            "_Length".edit(90, ref _transitionLength, 0.1f, 10f).nl(ref changed);

            return changed;
        }

        public override string NameForDisplayPEGI() => "Ambient Sound";
        #endregion
    }
}
