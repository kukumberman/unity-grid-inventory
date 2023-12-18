using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public sealed class InventoryView : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<string> _onInventoryElementClicked;

    [SerializeField]
    private UnityEvent<string, Vector2Int> _onInventoryItemDragged;

    [SerializeField]
    private UnityEvent<string, Vector2Int> _onInventoryItemDropped;

    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private Sprite _cellSprite;

    [SerializeField]
    private VisualTreeAsset _itemUxmlPrefab;

    [SerializeField]
    private int _cellSize;

    [SerializeField]
    private int _visibleGridHeight;

    private ScrollView _scrollView;
    private VisualElement _cellsContentParentElement;
    private VisualElement _itemsContentParentElement;

    private Grid2D _grid;
    private List<VisualElement> _cells;

    private VisualElement _draggableElement;
    private Vector2 _cachedPosition;
    private Vector2 _clickRelativeOffset;

    private Vector2 _screenPosition;
    private Vector2 _targetPosition;
    private Vector2 _snapPosition;
    private Vector2 _localPosition;
    private Vector2Int _draggableGridPosition;

    private void Update()
    {
        UpdateDragValues();

        if (_draggableElement != null)
        {
            HandleDrag(_draggableElement);
        }
    }

    public void CreateGrid(Vector2Int gridSize)
    {
        _scrollView = _document.rootVisualElement.Q<ScrollView>();
        var scrollContent = _scrollView.Q<VisualElement>("unity-content-container");
        _cellsContentParentElement = scrollContent[0];
        _itemsContentParentElement = scrollContent[1];

        for (int i = _cellsContentParentElement.childCount - 1; i >= 0; i--)
        {
            _cellsContentParentElement.RemoveAt(i);
        }

        var pixelGridWidth = gridSize.x * _cellSize;
        var pixelGridHeight = _visibleGridHeight * _cellSize;

        _cellsContentParentElement.style.width = new StyleLength(
            new Length(pixelGridWidth, LengthUnit.Pixel)
        );
        _cellsContentParentElement.style.height = new StyleLength(
            new Length(pixelGridHeight, LengthUnit.Pixel)
        );

        var totalPixelWidth = pixelGridWidth + 24;

        _scrollView.style.width = new StyleLength(new Length(totalPixelWidth, LengthUnit.Pixel));
        _scrollView.style.height = _cellsContentParentElement.style.height;
        _scrollView.parent.style.width = _scrollView.style.width;

        var cellCount = gridSize.x * gridSize.y;

        _cells = new List<VisualElement>(cellCount);
        _grid = new Grid2D(gridSize.x, gridSize.y);

        for (int i = 0; i < cellCount; i++)
        {
            var element = CreateCell();
            _cellsContentParentElement.Add(element);
            _cells.Add(element);
        }
    }

    public void CreateItemAt(
        Vector2Int gridPosition,
        InventoryItem inventoryItem,
        bool rotated = false
    )
    {
        var element = _itemUxmlPrefab.Instantiate()[0];

        element.style.position = new StyleEnum<Position>(Position.Absolute);

        element.style.left = new StyleLength(
            new Length(gridPosition.x * _cellSize, LengthUnit.Pixel)
        );
        element.style.top = new StyleLength(
            new Length(gridPosition.y * _cellSize, LengthUnit.Pixel)
        );

        var itemGridSize = inventoryItem.Item.GridSize;
        var pixelSize = itemGridSize * _cellSize;
        var pixelWidth = !rotated ? pixelSize.x : pixelSize.y;
        var pixelHeight = !rotated ? pixelSize.y : pixelSize.x;
        element.style.width = new StyleLength(new Length(pixelWidth, LengthUnit.Pixel));
        element.style.height = new StyleLength(new Length(pixelHeight, LengthUnit.Pixel));

        var angle = !rotated ? 0 : 90;

        var imageElement = element.Q<VisualElement>("ve-image");

        imageElement.style.backgroundImage = new StyleBackground(inventoryItem.Item.Sprite);

        imageElement.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));

        imageElement.style.width = new StyleLength(new Length(pixelSize.x, LengthUnit.Pixel));
        imageElement.style.height = new StyleLength(new Length(pixelSize.y, LengthUnit.Pixel));

        if (rotated)
        {
            imageElement.style.left = new StyleLength(new Length(pixelSize.y, LengthUnit.Pixel));
        }

        var nameLabel = element.Q<Label>("label-name");
        nameLabel.text = inventoryItem.Item.Id;

        element.name = inventoryItem.Id;

        element.RegisterCallback<ClickEvent>(ClickHandler);
        element.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        element.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        element.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        element.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);

        _itemsContentParentElement.Add(element);
    }

    public void RemoveItemElementByUniqueId(string id)
    {
        var element = _itemsContentParentElement.Q<VisualElement>(id);

        if (element != null)
        {
            element.RemoveFromHierarchy();
        }
    }

    public void PlaceDraggedElementAt(int x, int y)
    {
        ResetCellsColor();

        var position = Vector2.zero;
        position.x = x * _cellSize;
        position.y = y * _cellSize;

        MoveDraggableElementTo(position);
    }

    public void ResetDraggedElement()
    {
        if (_draggableElement == null)
        {
            return;
        }

        MoveDraggableElementTo(_cachedPosition);
    }

    public void MarkCellsArea(int x, int y, int width, int height, bool allowed)
    {
        ResetCellsColor();

        var color = allowed ? Color.green : Color.red;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var xx = x + i;
                var yy = y + j;

                if (!_grid.IsInside(xx, yy))
                {
                    continue;
                }

                var idx = _grid.GridToIndex(xx, yy);

                _cells[idx].style.unityBackgroundImageTintColor = color;
            }
        }
    }

    private void ResetCellsColor()
    {
        for (int i = 0, length = _cells.Count; i < length; i++)
        {
            _cells[i].style.unityBackgroundImageTintColor = Color.white;
        }
    }

    private VisualElement CreateCell()
    {
        var element = new VisualElement();

        element.style.backgroundImage = new StyleBackground(_cellSprite);
        element.style.width = new StyleLength(new Length(_cellSize, LengthUnit.Pixel));
        element.style.height = new StyleLength(new Length(_cellSize, LengthUnit.Pixel));

        return element;
    }

    private void MoveDraggableElementTo(Vector2 pixelPosition)
    {
        ResetCellsColor();

        _draggableElement.style.left = new StyleLength(
            new Length(pixelPosition.x, LengthUnit.Pixel)
        );
        _draggableElement.style.top = new StyleLength(
            new Length(pixelPosition.y, LengthUnit.Pixel)
        );

        _itemsContentParentElement.Add(_draggableElement);

        _draggableElement = null;
        _cachedPosition = Vector2.zero;
        _clickRelativeOffset = Vector2.zero;
        _draggableGridPosition = Vector2Int.zero;
    }

    private void UpdateDragValues()
    {
        _screenPosition = Input.mousePosition;
        _screenPosition.y = Screen.height - _screenPosition.y;

        _targetPosition = _screenPosition + _clickRelativeOffset;
        _snapPosition = _targetPosition;

        var contentParentWorldPosition = (Vector2)
            _cellsContentParentElement.worldTransform.GetPosition();
        _snapPosition -= contentParentWorldPosition;

        _snapPosition.x = Mathf.Round(_snapPosition.x / _cellSize) * _cellSize;
        _snapPosition.y = Mathf.Round(_snapPosition.y / _cellSize) * _cellSize;

        _snapPosition += contentParentWorldPosition;

        _localPosition = _cellsContentParentElement.WorldToLocal(_snapPosition);
    }

    private void HandleDrag(VisualElement element)
    {
        element.style.left = new StyleLength(new Length(_targetPosition.x, LengthUnit.Pixel));
        element.style.top = new StyleLength(new Length(_targetPosition.y, LengthUnit.Pixel));

        _draggableGridPosition.x = Mathf.FloorToInt(_localPosition.x / _cellSize);
        _draggableGridPosition.y = Mathf.FloorToInt(_localPosition.y / _cellSize);

        _onInventoryItemDragged.Invoke(element.name, _draggableGridPosition);
    }

    private void ClickHandler(ClickEvent evt)
    {
        if (evt.ctrlKey)
        {
            var element = evt.currentTarget as VisualElement;
            var id = element.name;
            _onInventoryElementClicked.Invoke(id);
        }
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (evt.ctrlKey)
        {
            return;
        }

        var target = evt.currentTarget as VisualElement;
        target.CapturePointer(evt.pointerId);

        _draggableElement = target;
        var elementWorldPosition = _draggableElement.worldTransform.GetPosition();
        _cachedPosition.x = _draggableElement.resolvedStyle.left;
        _cachedPosition.y = _draggableElement.resolvedStyle.top;
        _clickRelativeOffset = (Vector2)elementWorldPosition - (Vector2)evt.position;
        _document.rootVisualElement.Add(_draggableElement);
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        var target = evt.currentTarget as VisualElement;

        if (target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        var target = evt.currentTarget as VisualElement;

        if (target != _draggableElement)
        {
            return;
        }

        _onInventoryItemDropped.Invoke(_draggableElement.name, _draggableGridPosition);
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        var target = evt.currentTarget as VisualElement;

        if (target.HasPointerCapture(evt.pointerId))
        {
            //UpdateDragValues();
            //HandleDrag(target);
        }
    }
}
