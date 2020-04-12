using System;
using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class EnumeratedAssetBase<T, G> : ScriptableObject, IPEGI where T : struct, IComparable, IFormattable, IConvertible where G: Object
{

    [SerializeField] public Object defaultAsset;

    [SerializeField] protected List<EnumeratedObject> enumeratedObjects = new List<EnumeratedObject>();

    private bool TryGet(T value, out EnumeratedObject obj)
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
        EnumeratedObject sp;

        return (TryGet(enumKey, out sp) ? sp.value : defaultAsset) as G;
    }

    #region Inspector
    public virtual bool Inspect()
    {
        pegi.toggleDefaultInspector(this);
        
        "Defaul {0}".F(typeof(G).ToPegiStringType()).edit(120, ref defaultAsset, allowSceneObjects: true).nl();
        
        EnumeratedObject.inspectedEnum = typeof(T);
        EnumeratedObject.inspectedObjectType = typeof(G);


        "Enumerated {0}".F(typeof(G).ToPegiStringType()).edit_List(ref enumeratedObjects).nl();

        return false;
    }
    #endregion
}

[Serializable]
public class EnumeratedObject : IPEGI_ListInspect, IGotDisplayName
{
    [SerializeField] private string nameForInspector = "";
    public Object value;

    #region Inspector
    public static Type inspectedEnum;
    public static Type inspectedObjectType;

    public bool InspectInList(IList list, int ind, ref int edited)
    {

        var changed = false;

        var name = Enum.ToObject(inspectedEnum, ind).ToString();

        if (!nameForInspector.Equals(name))
        {
            nameForInspector = name;
            changed = true;
        }

        nameForInspector.write(90);

        pegi.edit(ref value, inspectedObjectType);

        return changed;
    }

    public string NameForDisplayPEGI() => nameForInspector + " " + (value ? value.name : ("No " + inspectedObjectType.ToString()));

    #endregion
}

