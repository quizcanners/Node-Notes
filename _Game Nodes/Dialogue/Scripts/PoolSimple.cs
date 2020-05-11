using PlayerAndEditorGUI;
using QuizCannersUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeNotes_Visual
{

    [Serializable]
    public class PoolSimple<T> : IPEGI where T : Component
    {

        private ListMetaData activeList;
        public List<T> active = new List<T>();
        public List<T> disabled = new List<T>();
        public T prefab;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in active)
                yield return i;
        }

        public void DeleteAll()
        {

            foreach (var el in active)
                el.gameObject.DestroyWhatever();

            foreach (var el in disabled)
                el.gameObject.DestroyWhatever();

            active.Clear();
            disabled.Clear();

        }

        public void Disable(T obj)
        {
            active.Remove(obj);
            obj.gameObject.SetActive(false);
            disabled.Add(obj);
        }

        public void Disable(bool disableFirst = true)
        {
            if (active.Count > 0)
            {
                if (disableFirst)
                    Disable(active[0]);
                else
                    Disable(active[active.Count - 1]);
            }
        }

        public T GetOne(Transform parent, bool insertFirst = false)
        {

            T toReturn;

            if (disabled.Count > 0)
            {
                toReturn = disabled[0];
                disabled.RemoveAt(0);
            }
            else
                toReturn = Object.Instantiate(prefab, parent);

            if (insertFirst)
                active.Insert(0, toReturn);
            else
                active.Add(toReturn);

            toReturn.gameObject.SetActive(true);

            return toReturn;
        }

        public bool Inspect()
        {
            var changed = false;

            "Prefab".edit(ref prefab).nl(ref changed);

            "Inactive: {0};".F(disabled.Count).writeHint();

            activeList.edit_List_UObj(ref active).nl(ref changed);

            return changed;
        }

        public PoolSimple(string name)
        {
            activeList = new ListMetaData(name, true, true, showAddButton: false);

        }

        public int Count => active.Count;
    }

}