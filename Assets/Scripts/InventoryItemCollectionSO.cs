using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "New InventoryItemCollectionSO",
    menuName = "SO/InventoryItemCollectionSO"
)]
public sealed class InventoryItemCollectionSO : MyScriptableObjectCollection<InventoryItemSO>
{
    // todo: "Clear" and "Fetch" methods should be in abstract class "MyScriptableObjectCollection"
    // but unity does not allow generic method with "ContextMenu"
#if UNITY_EDITOR
    [ContextMenu(nameof(Clear), false, 10)]
    private void Clear()
    {
        _items.Clear();

        EditorUtility.SetDirty(this);
    }

    [ContextMenu(nameof(Fetch), false, 5)]
    private void Fetch()
    {
        var thisAssetPath = AssetDatabase.GetAssetPath(this);
        var directory = Path.GetDirectoryName(thisAssetPath);

        var filter = string.Format("t:{0}", nameof(InventoryItemSO));
        var searchInFolders = new string[] { directory };

        var guids = AssetDatabase.FindAssets(filter, searchInFolders);

        _items.Clear();

        for (int i = 0; i < guids.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            var asset = AssetDatabase.LoadAssetAtPath<InventoryItemSO>(assetPath);
            _items.Add(asset);
        }

        EditorUtility.SetDirty(this);
    }
#endif
}
