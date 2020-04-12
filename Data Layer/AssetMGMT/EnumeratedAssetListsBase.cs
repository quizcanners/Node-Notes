using System;
using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class EnumeratedAssetListsBase<T, G> : ScriptableObject, IPEGI where T : struct, IComparable, IFormattable, IConvertible where G: Object
{

    [SerializeField] public Object defaultAsset;

    [SerializeField] protected List<EnumeratedObjectList> enumeratedObjects = new List<EnumeratedObjectList>();

    private bool TryGet(T value, out EnumeratedObjectList obj)
    {
        int index = Convert.ToInt32(value);

        if (enumeratedObjects.Count > index)
        {
            obj = enumeratedObjects[index];
            return true;
        }

        obj = null;

        return false;
    }

    public virtual G Get(T enumKey)
    {
        EnumeratedObjectList sp;

        return (TryGet(enumKey, out sp) ? sp.list.GetRandom() : defaultAsset) as G;
    }

    #region Inspector

    private int _inspectedList = -1;

    public virtual bool Inspect()
    {
        var changed = false;

        pegi.toggleDefaultInspector(this);
        
        "Defaul {0}".F(typeof(G).ToPegiStringType()).edit(120, ref defaultAsset, allowSceneObjects: true).nl(ref changed);

        EnumeratedObjectList.inspectedEnum = typeof(T);
        EnumeratedObjectList.inspectedObjectType = typeof(G);

        "Enumerated {0}".F(typeof(G).ToPegiStringType()).edit_List(ref enumeratedObjects, ref _inspectedList).nl(ref changed);

        return changed;
    }
    #endregion



}

[Serializable]
public class EnumeratedObjectList : IPEGI_ListInspect, IGotDisplayName, IPEGI, IGotCount
{
    [SerializeField] private string nameForInspector = "";
    public List<Object> list;

    #region Inspector
    public static Type inspectedEnum;
    public static Type inspectedObjectType;

    public bool InspectInList(IList inspList, int ind, ref int edited)
    {

        var changed = false;

        var name = Enum.ToObject(inspectedEnum, ind).ToString();

        if (!nameForInspector.Equals(name))
        {
            nameForInspector = name;
            changed = true;
        }

        "{0} [{1}]".F(nameForInspector, CountForInspector()).write();

        if (list == null)
        {
            list = new List<Object>();
            changed = true;
        }

        if (list.Count < 2)
        {
            var el = list.TryGet(0);

            if (pegi.edit(ref el, inspectedObjectType, 90))
                list.ForceSet(0, el);
        }

        if (icon.Enter.Click())
            edited = ind;

        return changed;
    }

    public int CountForInspector() => list.IsNullOrEmpty() ? 0 : list.Count;

    public string NameForDisplayPEGI() => nameForInspector + " " + (list.IsNullOrEmpty() ? "Empty" : pegi.GetNameForInspector(list[0]));

    public bool Inspect()
    {
        var changed =  "All {0}".F(nameForInspector).edit_List_UObj(ref list);

        return changed;
    }



    #endregion
}

