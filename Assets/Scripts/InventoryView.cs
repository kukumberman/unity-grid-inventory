using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryView : MonoBehaviour
{
    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private Sprite _cellSprite;

    [SerializeField]
    private VisualTreeAsset _itemUxmlPrefab;

    [SerializeField]
    private VisualTreeAsset _windowUxmlPrefab;

    [SerializeField]
    private VisualTreeAsset _scrollItemUxmlPrefab;

    [SerializeField]
    private int _cellSize;

    [SerializeField]
    private bool _processDragInUpdate;

    private InventoryGridCollectionElement _inventoryStashElement;

    private InventoryItemElement _draggableElement;
    private Vector2 _cachedPosition;
    private Vector2 _clickRelativeOffset;

    private Vector2 _screenPosition;
    private Vector2 _targetPosition;
    private Vector2Int _draggableGridPosition;

    private bool _cachedRotatedState;
    private VisualElement _cachedParentElement;

    private VisualElement _backpackWindowsContentParentElement;
    private List<InventoryGridCollectionElement> _listOfGridCollectionElements = new();
    private Dictionary<BackpackInventoryItem, InventoryWindowElement> _windowMap = new();
    private List<VisualElement> _pickResults = new();

    public InventoryGridCollectionElement Stash => _inventoryStashElement;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (_draggableElement != null)
            {
                _draggableElement.IsRotated = !_draggableElement.IsRotated;
                HandleDrag(_draggableElement);
            }
        }

        if (_processDragInUpdate && _draggableElement != null)
        {
            UpdateDragValues();
            HandleDrag(_draggableElement);
        }
    }

    public void OnItemRemovedEventHandler(InventoryItem inventoryItem)
    {
        if (inventoryItem is BackpackInventoryItem backpackItem)
        {
            var list = new List<InventoryItem>();
            backpackItem.Inventory.GetItemsDeeplyNonAlloc(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is BackpackInventoryItem innerBackpackItem)
                {
                    if (_windowMap.TryGetValue(innerBackpackItem, out var innerWindowElement))
                    {
                        _windowMap.Remove(innerBackpackItem);

                        innerWindowElement.RemoveFromHierarchy();
                    }
                }
            }

            if (_windowMap.TryGetValue(backpackItem, out var windowElement))
            {
                _windowMap.Remove(backpackItem);

                windowElement.RemoveFromHierarchy();
            }
        }
    }

    public void CreateGrid(Vector2Int gridSize)
    {
        _inventoryStashElement = _document.rootVisualElement.Q<InventoryGridCollectionElement>(
            "inventory-stash"
        );
        _inventoryStashElement.Setup(_cellSize);
        _inventoryStashElement.CreateGrid(gridSize, CreateCell, CreateItem);
        _inventoryStashElement.FitWidth();

        _backpackWindowsContentParentElement = _document.rootVisualElement.Q<VisualElement>(
            "ve-backpack-windows-content"
        );

        var btnSave = _document.rootVisualElement.Q<Button>("btn-save");
        var btnLoad = _document.rootVisualElement.Q<Button>("btn-load");

        btnSave.clicked += () => InventoryManager.Singleton.Save();
        btnLoad.clicked += () => InventoryManager.Singleton.Load();

        CreateScrollList();
    }

    public void BindAndSync(Inventory rootInventory)
    {
        if (_windowMap.Count > 0)
        {
            var windowElements = _windowMap.Values.ToArray();
            foreach (var element in windowElements)
            {
                element.RemoveFromHierarchy();
            }

            _windowMap.Clear();
        }

        _inventoryStashElement.Bind(rootInventory);
        _inventoryStashElement.Sync();
    }

    private InventoryItemElement CreateItemAt(Vector2Int gridPosition, InventoryItem inventoryItem)
    {
        var element = _itemUxmlPrefab.Instantiate()[0] as InventoryItemElement;
        foreach (var styleSheet in _itemUxmlPrefab.stylesheets)
        {
            element.styleSheets.Add(styleSheet);
        }

        element.style.position = new StyleEnum<Position>(Position.Absolute);

        element.SetScreenPosition(gridPosition * _cellSize);

        element.Setup(_cellSize, inventoryItem.Item.GridSize);
        element.SetSprite(inventoryItem.Item.Sprite);
        element.SetTitle(inventoryItem.Item.Id);
        element.IsRotated = inventoryItem.IsRotated;

        element.DynamicId = inventoryItem.Id;

        element.RegisterCallback<ClickEvent>(ClickHandler);
        element.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        element.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        element.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        element.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);

        return element;
    }

    private InventoryItemElement CreateItem(InventoryItem inventoryItem)
    {
        return CreateItemAt(inventoryItem.GridPosition, inventoryItem);
    }

    private void DestroyDraggedElement()
    {
        _inventoryStashElement.ResetCellsColor();
        _draggableElement.RemoveFromHierarchy();
        _draggableElement = null;
        _cachedPosition = Vector2.zero;
        _clickRelativeOffset = Vector2.zero;
        _draggableGridPosition = Vector2Int.zero;
    }

    private void ResetDraggedElement()
    {
        if (_draggableElement == null)
        {
            return;
        }

        _draggableElement.IsRotated = _cachedRotatedState;
        _draggableElement.SetScreenPosition(_cachedPosition);
        _cachedParentElement.Add(_draggableElement);

        _cachedParentElement = null;
        _draggableElement = null;
        _cachedPosition = Vector2.zero;
        _clickRelativeOffset = Vector2.zero;
        _draggableGridPosition = Vector2Int.zero;
    }

    private void ResetColorOfAllAvailableCells()
    {
        PopulateGridCollectionsElements();

        foreach (var gridElement in _listOfGridCollectionElements)
        {
            gridElement.ResetCellsColor();
        }
    }

    public void MarkCellsArea(string inventoryId, int x, int y, int width, int height, bool allowed)
    {
        var gridElement = _listOfGridCollectionElements.Find(
            element => element.DynamicId == inventoryId
        );
        gridElement.ResetCellsColor();
        gridElement.MarkCellsArea(x, y, width, height, allowed);
    }

    private VisualElement CreateCell()
    {
        var element = new VisualElement();

        element.style.backgroundImage = new StyleBackground(_cellSprite);
        element.style.width = new StyleLength(new Length(_cellSize, LengthUnit.Pixel));
        element.style.height = new StyleLength(new Length(_cellSize, LengthUnit.Pixel));
        element.AddToClassList("cell");

        return element;
    }

    private void UpdateDragValues()
    {
        _screenPosition = Input.mousePosition;
        _screenPosition.y = Screen.height - _screenPosition.y;

        var style = _document.rootVisualElement.resolvedStyle;
        var screenSize = new Vector2(style.width, style.height);

        var width01 = Mathf.InverseLerp(0, Screen.width, _screenPosition.x);
        var height01 = Mathf.InverseLerp(0, Screen.height, _screenPosition.y);

        _screenPosition.x = Mathf.Lerp(0, screenSize.x, width01);
        _screenPosition.y = Mathf.Lerp(0, screenSize.y, height01);

        _targetPosition = _screenPosition + _clickRelativeOffset;
    }

    private void HandleDrag(InventoryItemElement element)
    {
        element.SetScreenPosition(_targetPosition);

        InventoryGridCollectionElement inventoryGridElement = null;
        PopulateGridCollectionsElements();

        foreach (var gridElement in _listOfGridCollectionElements)
        {
            if (gridElement.InsideRect(_screenPosition))
            {
                inventoryGridElement = gridElement;
                break;
            }
        }

        if (inventoryGridElement == null)
        {
            Debug.Log("todo: outside");
            return;
        }

        _draggableGridPosition = inventoryGridElement.ScreenPositionToGrid(_targetPosition);

        var rotated = _draggableElement.IsRotated;

        Inventory inventory;
        if (inventoryGridElement.DynamicId != null)
        {
            var dynamicItemDropTarget = InventoryManager.Singleton.GetDynamicItemById(
                inventoryGridElement.DynamicId
            );

            if (
                dynamicItemDropTarget == null
                || dynamicItemDropTarget is not BackpackInventoryItem backpackItem
            )
            {
                return;
            }

            inventory = backpackItem.Inventory;
        }
        else
        {
            inventory = InventoryManager.Singleton.RootInventory;
        }

        var inventoryItem = InventoryManager.Singleton.GetDynamicItemById(element.DynamicId);

        if (inventoryItem == null)
        {
            return;
        }

        var item = inventoryItem.Item;
        var width = !rotated ? item.Width : item.Height;
        var height = !rotated ? item.Height : item.Width;

        var allowed = inventory.IsAreaEmptyOrOccupiedByItem(
            _draggableGridPosition.x,
            _draggableGridPosition.y,
            width,
            height,
            inventoryItem
        );

        ResetColorOfAllAvailableCells();
        MarkCellsArea(
            inventoryGridElement.DynamicId,
            _draggableGridPosition.x,
            _draggableGridPosition.y,
            width,
            height,
            allowed
        );
    }

    private void ClickHandler(ClickEvent evt)
    {
        if (evt.ctrlKey)
        {
            var element = evt.currentTarget as VisualElement;
            var inventoryItemElement = element as InventoryItemElement;
            InventoryManager.Singleton.TryRemoveItem(inventoryItemElement.DynamicId);
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
        _cachedRotatedState = _draggableElement.IsRotated;
        _cachedParentElement = _draggableElement.parent;
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

            MouseDragReleased(evt);
            return;
        }

        if (evt.button == 1)
        {
            var inventoryItemElement = target as InventoryItemElement;
            var dynamicItem = InventoryManager.Singleton.GetDynamicItemById(
                inventoryItemElement.DynamicId
            );

            if (
                dynamicItem is BackpackInventoryItem backpackItem
                && dynamicItem.Item is BackpackInventoryItemSO backpackStaticItem
            )
            {
                if (_windowMap.TryGetValue(backpackItem, out var windowElement))
                {
                    windowElement.SetScreenPosition(evt.position);
                    windowElement.BringToFront();
                }
                else
                {
                    CreateBackpackWindow(backpackItem, evt.position);
                }
            }
        }
    }

    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt) { }

    private void MouseDragReleased(PointerUpEvent evt)
    {
        var target = evt.currentTarget as VisualElement;

        if (target != _draggableElement)
        {
            return;
        }

        GetElementUnderMouse(
            evt.position,
            out var gridElementUnderMouse,
            out var itemElementUnderMouse
        );

        if (itemElementUnderMouse != null)
        {
            HandleMouseReleasedDropOnItem(itemElementUnderMouse);
        }
        else if (gridElementUnderMouse != null)
        {
            HandleMouseReleasedDropOnGrid(gridElementUnderMouse);
        }
        else
        {
            ResetDraggedElement();
            ResetColorOfAllAvailableCells();
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        var target = evt.currentTarget as VisualElement;

        if (target.HasPointerCapture(evt.pointerId))
        {
            if (target is InventoryItemElement itemElement && !_processDragInUpdate)
            {
                UpdateDragValues();
                HandleDrag(itemElement);
            }
        }
    }

    private void PopulateGridCollectionsElements()
    {
        _listOfGridCollectionElements.Clear();

        _listOfGridCollectionElements.Add(_inventoryStashElement);

        _backpackWindowsContentParentElement
            .Query<InventoryWindowElement>()
            .Children<InventoryGridCollectionElement>()
            .ToList(_listOfGridCollectionElements);

        _listOfGridCollectionElements.Reverse();
    }

    private void CreateBackpackWindow(BackpackInventoryItem backpackItem, Vector2 position)
    {
        var backpackStaticItem = backpackItem.Item as BackpackInventoryItemSO;

        var windowElement = _windowUxmlPrefab.Instantiate()[0] as InventoryWindowElement;

        windowElement.Setup();
        windowElement.GridCollection.Setup(_cellSize);
        windowElement.GridCollection.CreateGrid(
            backpackStaticItem.BackpackGridSize,
            CreateCell,
            CreateItem
        );
        windowElement.GridCollection.FitWidthAndHeight();
        windowElement.GridCollection.Bind(backpackItem.Inventory);
        windowElement.GridCollection.Sync();
        windowElement.GridCollection.DynamicId = backpackItem.Id;
        windowElement.MakeAbsolute();
        windowElement.SetScreenPosition(position);
        windowElement.UpdateWidth();
        windowElement.SetTitle(backpackStaticItem.Id);

        _backpackWindowsContentParentElement.Add(windowElement);
        _windowMap.Add(backpackItem, windowElement);
        windowElement.RegisterCallback<DetachFromPanelEvent>(
            (evt) =>
            {
                _windowMap.Remove(backpackItem);
            }
        );
    }

    private bool GetElementUnderMouse(
        Vector2 position,
        out InventoryGridCollectionElement gridElementUnderMouse,
        out InventoryItemElement itemElementUnderMouse
    )
    {
        gridElementUnderMouse = null;
        itemElementUnderMouse = null;

        _pickResults.Clear();

        _document.rootVisualElement.panel.PickAll(position, _pickResults);

        foreach (var element in _pickResults)
        {
            if (element == _draggableElement)
            {
                continue;
            }

            if (element is InventoryItemElement itemElement)
            {
                itemElementUnderMouse = itemElement;
                return true;
            }

            if (element.ClassListContains("cell"))
            {
                var currentParent = element.parent;

                while (true)
                {
                    if (currentParent == null)
                    {
                        break;
                    }

                    if (currentParent is InventoryGridCollectionElement gridElement)
                    {
                        gridElementUnderMouse = gridElement;
                        return true;
                    }

                    currentParent = currentParent.parent;
                }
            }
        }

        return false;
    }

    private void HandleMouseReleasedDropOnItem(InventoryItemElement itemElementUnderMouse)
    {
        var targetDynamicItem = InventoryManager.Singleton.GetDynamicItemById(
            itemElementUnderMouse.DynamicId
        );

        if (targetDynamicItem == null)
        {
            return;
        }

        if (targetDynamicItem is not BackpackInventoryItem backpackItem)
        {
            return;
        }

        var transferedSuccessfully = InventoryManager.Singleton.TransferItemToInventory(
            _draggableElement.DynamicId,
            backpackItem.Id
        );

        Debug.Log($"transferedSuccessfully: {transferedSuccessfully}");

        if (transferedSuccessfully)
        {
            DestroyDraggedElement();
        }
        else
        {
            ResetDraggedElement();
        }

        ResetColorOfAllAvailableCells();
    }

    private void HandleMouseReleasedDropOnGrid(InventoryGridCollectionElement gridElementUnderMouse)
    {
        var result = InventoryManager.Singleton.MoveItemToInventory(
            _draggableElement.DynamicId,
            gridElementUnderMouse.DynamicId,
            _draggableGridPosition,
            _draggableElement.IsRotated
        );

        Debug.Log(result);

        if (result)
        {
            DestroyDraggedElement();
        }
        else
        {
            ResetDraggedElement();
        }

        ResetColorOfAllAvailableCells();
    }

    private void CreateScrollList()
    {
        var scrollView = _document.rootVisualElement.Q<ScrollView>("item-scroll-list");
        scrollView.contentContainer.Clear();

        scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        var collection = InventoryManager.Singleton.ItemCollection;

        for (int i = 0, length = collection.Items.Count; i < length; i++)
        {
            var element = _scrollItemUxmlPrefab.Instantiate()[0] as InventoryItemScrollElement;
            foreach (var styleSheet in _scrollItemUxmlPrefab.stylesheets)
            {
                element.styleSheets.Add(styleSheet);
            }

            var staticItem = collection.Items[i];
            element.Setup(staticItem.Sprite, staticItem.Id);
            element.StaticItemId = staticItem.Id;

            element.RegisterCallback<ClickEvent>(ClickHandlerFoo);

            scrollView.contentContainer.Add(element);
        }
    }

    private void ClickHandlerFoo(ClickEvent evt)
    {
        if (evt.currentTarget is InventoryItemScrollElement scrollElement)
        {
            var staticItem = InventoryManager.Singleton.GetStaticItemById(
                scrollElement.StaticItemId
            );

            if (staticItem == null)
            {
                return;
            }

            InventoryManager.Singleton.AddItem(staticItem);
        }
    }
}
