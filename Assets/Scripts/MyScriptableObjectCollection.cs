using System.Collections.Generic;
using UnityEngine;

public abstract class MyScriptableObjectCollection<T> : ScriptableObject
    where T : ScriptableObject
{
    [SerializeField]
    protected List<T> _items;

    public IReadOnlyList<T> Items => _items;
}
