using NodeNotes;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using UnityEngine;

namespace NodeNotes_Visual
{

#pragma warning disable IDE0018 // Inline variable declaration
    public class PresentationSystemConfigurations : ICfg, IPEGI
    {

        public Countless<string> perNodeConfigs = new Countless<string>();

        public CountlessCfg<ConditionalConfig> perNodeConfigConditional = new CountlessCfg<ConditionalConfig>();

        public class ConditionalConfig : ICfg
        {
            public CfgData config;
            public ConditionBranch condition = new ConditionBranch();

            public void Decode(string tg, CfgData data)
            {
                switch (tg)
                {
                    case "cf": config = data; break;
                    case "cnd": condition.DecodeFull(data); break;
                }
            }

            public CfgEncoder Encode() => new CfgEncoder()
                .Add("cf", config)
                .Add("cnd", condition);
        }

        public CfgData GetConfigFor(Node node)
        {
            Node iteration = node;
            while (iteration != null)
            {
                var val = perNodeConfigs[iteration.IndexForPEGI];
                if (!val.IsNullOrEmpty())
                {
                    return new CfgData(val);
                }

                iteration = iteration.parentNode;
            }

            return new CfgData();
        }

        public virtual void OnNodeDelete(int index)
        {
            perNodeConfigs[index] = null;
            perNodeConfigConditional[index] = null;
        }

        #region Inspector

        public bool Inspect()
        {
            var changed = false;

            "Configs: {0}".F(perNodeConfigs.CountForInspector()).nl();

            perNodeConfigs.Inspect().nl();

            return changed;
        }

        public bool InspectFor(Node node, PresentationSystemsAbstract presentationSystem)
        {
            pegi.nl();

            bool saveOnSchange = presentationSystem.SaveOnEdit;

            if (node == null)
                "Node is null".writeWarning();
             else if (!presentationSystem)
                "No presentation system".writeWarning();
            else
            {
                var changed = false;

                var index = node.IndexForPEGI;

              
                "Configs: {0}".F(perNodeConfigs.CountForInspector()).nl();

                string cfg;
                if (perNodeConfigs.TryGet(index, out cfg))
                {
                    if ("Clear Config".ClickConfirm("clM").nl())
                        perNodeConfigs[index] = default;
                    else
                    {
                        if (!saveOnSchange && "Save {0} config for {1}".F(presentationSystem.ClassTag, node.GetNameForInspector()).Click().nl())
                            perNodeConfigs[index] = presentationSystem.Encode().ToString();
                        
                        presentationSystem.Inspect().nl(ref changed);
                        
                        if (changed && saveOnSchange)
                            perNodeConfigs[index] = presentationSystem.Encode().ToString();
                    }
                }
                else if ("+ Cfg override for {0} ".F(node.NameForPEGI).Click().nl())
                        perNodeConfigs[index] = "";
                
                return changed;
            }

            pegi.nl();

            return false;
        }

        #endregion

        #region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
            .Add("pn", perNodeConfigs.Encode)
            .Add("pnc", perNodeConfigConditional);

        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "pn": data.DecodeInto(out perNodeConfigs); break;
                case "pnc": perNodeConfigConditional.Decode(data); break;
            }
        }


        #endregion
    }

    // Change this to interface and create wrapper classes to plug others in
    public abstract class PresentationSystemsAbstract : MonoBehaviour, ICfg, IGotClassTag, IPEGI, IGotDisplayName
    {
        public abstract void ManagedOnEnable();

        public abstract bool Inspect();

        public abstract string NameForDisplayPEGI();

        public virtual bool SaveOnEdit => true;

        #region Encode & Decode

        public abstract string ClassTag { get; }

        public abstract CfgEncoder Encode();

        public abstract void Decode(string tg, CfgData data);
        #endregion
    }
}