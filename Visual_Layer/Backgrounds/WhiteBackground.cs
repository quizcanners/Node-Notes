using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class WhiteBackground : NodesStyleBase {

        const string classTag = "white";

        public override string ClassTag => classTag;

        public bool isFading;

        public Color color = Color.white;

        #region Inspector
#if PEGI
        public string NameForPEGIdisplay => "White Background";

        public override bool Inspect() {
            bool changed = false;

            "Background Color".edit(ref color).nl();

            return changed;
        }
#endif
        #endregion

        #region Encode & Decode

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "col": color = data.ToColor(); break;
                default: return true;

            }
            return false;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("col", color);

        #endregion

        public override void FadeAway() => isFading = true;

        public override bool TryFadeIn() => isFading = false;

        [NonSerialized] private Camera _mainCam;

        private Camera MainCam
        {
            get
            {
                if (!_mainCam)
                    _mainCam = Camera.main;
                return _mainCam;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isFading) return;

            if (!MainCam) return;

            var col = _mainCam.backgroundColor;

            _mainCam.backgroundColor = col.Lerp_bySpeed(Color.white, 3);

        }
    }
}