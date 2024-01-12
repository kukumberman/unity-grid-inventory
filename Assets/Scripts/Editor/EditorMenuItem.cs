using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kukumberman.GridInventory.Editor
{
    public static class EditorMenuItem
    {
        private const string kMenuItemName = "Assets/Create InventoryItemSO";

        [MenuItem(kMenuItemName)]
        private static void CreateStaticItem()
        {
            if (Selection.objects.Length == 0)
            {
                return;
            }

            if (Selection.objects[0] is not Sprite sprite)
            {
                return;
            }

            var staticItem = ScriptableObject.CreateInstance<InventoryItemSO>();
            staticItem.SetSprite(sprite);

            var assetName = string.Format("{0}.asset", sprite.name);

            var spriteRelativePath = AssetDatabase.GetAssetPath(sprite);
            var relativeDirectory = Path.GetDirectoryName(spriteRelativePath);
            var assetRelativePath = Path.Combine(relativeDirectory, assetName);

            AssetDatabase.CreateAsset(staticItem, assetRelativePath);

            Selection.activeObject = staticItem;
        }

        [MenuItem(kMenuItemName, true)]
        private static bool CreateStaticItemValidate()
        {
            return Selection.objects.Length == 1 && Selection.objects[0] is Sprite;
        }
    }
}
