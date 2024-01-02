using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New InventoryItemCollectionSO",
    menuName = "SO/InventoryItemCollectionSO"
)]
public sealed class InventoryItemCollectionSO : ScriptableObject
{
    public List<InventoryItemSO> Items;
}
