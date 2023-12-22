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
    private VisualTreeAsset _windowUxmlPrefab;

    [SerializeField]
    private int _cellSize;

    private InventoryGridCollectionElement _inventoryStashElement;

    private InventoryItemElement _draggableElement;
    private Vector2 _cachedPosition;
    private Vector2 _clickRelativeOffset;

    private Vector2 _screenPosition;
    private Vector2 _targetPosition;
    private Vector2Int _draggableGridPosition;

    public InventoryGridCollectionElement Stash => _inventoryStashElement;

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
        _inventoryStashElement = _document.rootVisualElement.Q<InventoryGridCollectionElement>(
            "inventory-stash"
        );
        _inventoryStashElement.Setup(_cellSize);
        _inventoryStashElement.CreateGrid(gridSize, CreateCell);
        _inventoryStashElement.FitWidth();
    }

    public InventoryItemElement CreateItemAt(Vector2Int gridPosition, InventoryItem inventoryItem)
    {
        var element = _itemUxmlPrefab.Instantiate()[0] as InventoryItemElement;

        element.style.position = new StyleEnum<Position>(Position.Absolute);

        element.SetScreenPosition(gridPosition * _cellSize);

        element.Setup(_cellSize, inventoryItem.Item.GridSize);
        element.SetSprite(inventoryItem.Item.Sprite);
        element.SetTitle(inventoryItem.Item.Id);
        element.SetRotated(inventoryItem.IsRotated);

        element.name = inventoryItem.Id;

        element.RegisterCallback<ClickEvent>(ClickHandler);
        element.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        element.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        element.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        element.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);

        return element;
    }

    public void RemoveItemElementByDynamicId(string id)
    {
        _inventoryStashElement.RemoveItemElementByDynamicId(id);
    }

    public void PlaceDraggedElementAt(int x, int y)
    {
        _inventoryStashElement.ResetCellsColor();

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
        _inventoryStashElement.MarkCellsArea(x, y, width, height, allowed);
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
        _inventoryStashElement.ResetCellsColor();

        _draggableElement.SetScreenPosition(pixelPosition);

        _inventoryStashElement.AddItemElement(_draggableElement);

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
    }

    private void HandleDrag(InventoryItemElement element)
    {
        element.SetScreenPosition(_targetPosition);

        _draggableGridPosition = _inventoryStashElement.ScreenPositionToGrid(_targetPosition);

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

        if (evt.button != 0)
        {
            return;
        }

        var target = evt.currentTarget as VisualElement;
        target.CapturePointer(evt.pointerId);

        _draggableElement = target as InventoryItemElement;
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

        if (evt.button == 1)
        {
            var dynamicItem = InventoryManager.Singleton.GetDynamicItemById(target.name);

            if (
                dynamicItem is BackpackInventoryItem backpackItem
                && dynamicItem.Item is BackpackInventoryItemSO backpackStaticItem
            )
            {
                CreateBackpackWindow(backpackItem);
            }
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

    private void CreateBackpackWindow(BackpackInventoryItem backpackItem)
    {
        var backpackStaticItem = backpackItem.Item as BackpackInventoryItemSO;

        var windowElement = _windowUxmlPrefab.Instantiate()[0] as InventoryWindowElement;

        windowElement.Setup();
        windowElement.GridCollection.Setup(_cellSize);
        windowElement.GridCollection.CreateGrid(backpackStaticItem.BackpackGridSize, CreateCell);
        windowElement.GridCollection.FitWidthAndHeight();
        windowElement.MakeAbsolute();
        windowElement.SetScreenPosition(100, 100);
        windowElement.UpdateWidth();
        windowElement.SetTitle(backpackStaticItem.Id);

        for (int i = 0; i < backpackItem.Inventory.Items.Count; i++)
        {
            var itemElement = CreateItemAt(
                backpackItem.GridPosition,
                backpackItem.Inventory.Items[i]
            );

            windowElement.GridCollection.AddItemElement(itemElement);
        }

        _document.rootVisualElement.Add(windowElement);
    }
}
