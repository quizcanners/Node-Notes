using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes
{

    public interface ICategorized  {
        List<PickedCategory> MyCategories { get; set; }
    } 

    public class CategoryRoot<T> : ICfg, IPEGI where T : ICategorized {

        public Countless<Category<T>> allSubs = new Countless<Category<T>>();

        public int unusedIndex;

        public List<Category<T>> subCategories = new List<Category<T>>();

        #region Inspect

 
        private int inspected = -1;
        
        public bool SelectCategory(PickedCategory pc)
        {
            var changed = false;

            var categoryFound = false;

            if (pc.path.Count > 0)
            {
                var ind = pc.path[0];

                foreach (var t in subCategories)
                    if (t.IndexForPEGI == ind) {
                        categoryFound = true;
                        t.SelectCategory(pc, 1).changes(ref changed);
                        break;
                    }
            }

            if (categoryFound) return changed;

            int tmp = -1;
            if ("Category".select_Index(ref tmp, subCategories).changes(ref changed)) {
                var c = subCategories.TryGet(tmp);
                if (c!= null)
                    pc.path.ForceSet(0,c.IndexForPEGI);
            }


            return changed;
        }
        
        public virtual bool Inspect()
        {
            current = this;

            var changed = false;

            "Categories".edit_List(ref subCategories, ref inspected).nl(ref changed);

            current = null;

            return changed;
        }
        #endregion

        #region Encode & Decode
        public static CategoryRoot<T> current = new CategoryRoot<T>();

       // public void Decode(string data) => this.DecodeTagsFrom(data);

        public virtual CfgEncoder Encode()
        {
            current = this;

            var cody = new CfgEncoder()//this.EncodeUnrecognized()
                .Add("s", subCategories)
                .Add_IfNotNegative("i", inspected)
                .Add("fi", unusedIndex);

            current = null;

            return cody;
        }

        public virtual void Decode(string data)
        {
            current = this;
            this.DecodeTagsFrom(data);
            current = null;
        }

        public virtual bool Decode(string tg, string data) {

            switch (tg) {
                case "s": data.Decode_List(out subCategories); break;
                case "i": inspected = data.ToInt(); break;
                case "fi": unusedIndex = data.ToInt(); break;
                default: return false;
            }

            return true;
        }
        #endregion
    }

    public class Category<T> : ICfg, IGotName, IGotIndex, IPEGI where T: ICategorized
    {

        public string NameForPEGI { get; set; }
        public int IndexForPEGI { get; set; }
        public List<Category<T>> subCategories = new List<Category<T>>();
        public List<T> elements = new List<T>();

        public Category() {
            var r = CategoryRoot<T>.current;

            r.allSubs[r.unusedIndex] = this;
            IndexForPEGI = r.unusedIndex;
            r.unusedIndex++;
        }

        #region Inspect

        private int _inspected = -1;
        
        public bool Select(ref T val) => pegi.select(ref val, elements);
        
        public virtual bool Inspect() {
            var changed = false;

            if (_inspected == -1) {
                var n = NameForPEGI;
                if ("Name".edit(ref n).nl(ref changed))
                    NameForPEGI = n;
            }

            "Sub".edit_List(ref subCategories, ref _inspected).nl(ref changed);

            return changed;
        }

        public bool SelectCategory(PickedCategory pc, int depth)
        {

            var changed = false;

            var categoryFound = false;

            if (pc.path.Count > depth) {

                NameForPEGI.nl();

                if (pc.path.Count == depth + 1 && icon.Exit.Click(ref changed))
                    pc.path.RemoveLast();
                else {
                    var ind = pc.path[depth];

                    foreach (var t in subCategories)
                        if (t.IndexForPEGI == ind) {
                            categoryFound = true;
                            t.SelectCategory(pc, depth + 1).changes(ref changed);
                            break;
                        }
                }
            }

            if (categoryFound) return false;

            NameForPEGI.write(PEGI_Styles.ClickableText);

            int tmp = -1;
            if ("Sub Category".select_Index(ref tmp, subCategories).nl(ref changed))
            {
                var c = subCategories.TryGet(tmp);
                if (c != null)
                    pc.path.ForceSet(depth, c.IndexForPEGI);
            }

            return changed;
        }

#endregion

#region Encode & Decode

        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("n", NameForPEGI)
            .Add("i", IndexForPEGI)
            .Add_IfNotEmpty("s", subCategories)
            .Add_IfNotNegative("in",_inspected);

        public virtual void Decode(string data)
        {
            this.DecodeTagsFrom(data);
            CategoryRoot<T>.current.allSubs[IndexForPEGI] = this;
        }

        public virtual bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "n": NameForPEGI = data; break;
                case "s": data.Decode_List(out subCategories); break;
                case "i":  IndexForPEGI = data.ToInt(); break;
                case "in": _inspected = data.ToInt(); break;
                default: return false;
            }

            return true;
        }

#endregion
    }

    public class PickedCategory: AbstractCfg {

        public List<int> path = new List<int>();
        
        public bool Inspect<T>() where T: ICategorized => CategoryRoot<T>.current.SelectCategory(this);

#region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()
            .Add_IfNotEmpty("p", path);

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "p": data.Decode_List(out path); break;
                default: return false;
            }

            return true;
        }
#endregion

     
    }

}