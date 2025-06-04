using UnityEngine;

public interface IStaticInventoryItem
{
    string Id { get; }

    int Width { get; }

    int Height { get; }

    Sprite Sprite { get; }
}
