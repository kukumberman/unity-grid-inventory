using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryItemScrollElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InventoryItemScrollElement, UxmlTraits> { }

    private VisualElement _imageElement;
    private Label _nameLabel;

    public string StaticItemId
    {
        get => name;
        set => name = value;
    }

    public void Setup(Sprite sprite, string displayName)
    {
        _imageElement = this.Q<VisualElement>("ve-image");
        _nameLabel = this.Q<Label>("label-name");

        _imageElement.style.backgroundImage = new StyleBackground(sprite);
        _nameLabel.text = displayName;
    }
}
