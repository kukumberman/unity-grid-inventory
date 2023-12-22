using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryItemElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InventoryItemElement, UxmlTraits> { }

    private VisualElement _imageElement;
    private Label _nameLabel;

    private Vector2 _pixelSize;
    private bool _isRotated;

    public string DynamicId
    {
        get => name;
        set => name = value;
    }

    public bool IsRotated
    {
        get => _isRotated;
        set
        {
            _isRotated = value;
            SetRotated(_isRotated);
        }
    }

    public void Setup(int cellSize, Vector2Int itemGridSize)
    {
        _pixelSize = cellSize * itemGridSize;

        _imageElement = this.Q<VisualElement>("ve-image");
        _nameLabel = this.Q<Label>("label-name");
    }

    public void SetSprite(Sprite sprite)
    {
        _imageElement.style.backgroundImage = new StyleBackground(sprite);
    }

    public void SetTitle(string title)
    {
        _nameLabel.text = title;
    }

    public void SetScreenPosition(Vector2 position)
    {
        style.left = new StyleLength(new Length(position.x, LengthUnit.Pixel));
        style.top = new StyleLength(new Length(position.y, LengthUnit.Pixel));
    }

    public void SetRotated(bool rotated)
    {
        var pixelWidth = !rotated ? _pixelSize.x : _pixelSize.y;
        var pixelHeight = !rotated ? _pixelSize.y : _pixelSize.x;
        style.width = new StyleLength(new Length(pixelWidth, LengthUnit.Pixel));
        style.height = new StyleLength(new Length(pixelHeight, LengthUnit.Pixel));

        var angle = !rotated ? 0 : 90;

        _imageElement.style.rotate = new StyleRotate(
            new Rotate(new Angle(angle, AngleUnit.Degree))
        );

        _imageElement.style.width = new StyleLength(new Length(_pixelSize.x, LengthUnit.Pixel));
        _imageElement.style.height = new StyleLength(new Length(_pixelSize.y, LengthUnit.Pixel));

        var left = rotated ? _pixelSize.y : 0;
        _imageElement.style.left = new StyleLength(new Length(left, LengthUnit.Pixel));
    }
}
