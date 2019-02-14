using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;

namespace NodeNotes_Visual {

    public class NodeStylesAttribute : Abstract_WithTaggedTypes { public override TaggedTypes_STD TaggedTypes => NodesStyleBase.all; }

    [NodeStyles]
    public abstract class NodesStyleBase : ComponentSTD, IManageFading, IPEGI, IGotClassTag {

        #region Tagged Types MGMT
        public virtual string ClassTag => StdEncoder.NullTag;
        public static TaggedTypes_STD all = new TaggedTypes_STD(typeof(NodesStyleBase));
        public TaggedTypes_STD AllTypes => all;
        #endregion

        public Color fallbackColor = Color.black;

        public abstract void FadeAway();

        public abstract bool TryFadeIn();
    }
}