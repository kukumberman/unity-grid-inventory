using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryGridCollectionElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InventoryGridCollectionElement, UxmlTraits> { }

    // todo
    private int _visibleGridHeight = 7;

    private int _cellSize;

    private ScrollView _scrollView;
    private VisualElement _cellsContentParentElement;
    private VisualElement _itemsContentParentElement;

    private Grid2D _grid;
    private List<VisualElement> _cells;

    public void Setup(int cellSize)
    {
        _cellSize = cellSize;
    }

    public void CreateGrid(Vector2Int gridSize, Func<VisualElement> cellFactory)
    {
        _scrollView = this.Q<ScrollView>();
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

        style.width = _scrollView.style.width;

        var cellCount = gridSize.x * gridSize.y;

        _cells = new List<VisualElement>(cellCount);
        _grid = new Grid2D(gridSize.x, gridSize.y);

        for (int i = 0; i < cellCount; i++)
        {
            var element = cellFactory();
            _cellsContentParentElement.Add(element);
            _cells.Add(element);
        }
    }

    public void AddItemElement(InventoryItemElement element)
    {
        _itemsContentParentElement.Add(element);
    }

    public void RemoveItemElementByDynamicId(string id)
    {
        var element = _itemsContentParentElement.Q<VisualElement>(id);

        if (element != null)
        {
            element.RemoveFromHierarchy();
        }
    }

    public Vector2Int ScreenPositionToGrid(Vector2 position)
    {
        var snapPosition = position;

        var contentParentWorldPosition = (Vector2)
            _cellsContentParentElement.worldTransform.GetPosition();
        snapPosition -= contentParentWorldPosition;

        snapPosition.x = Mathf.Round(snapPosition.x / _cellSize) * _cellSize;
        snapPosition.y = Mathf.Round(snapPosition.y / _cellSize) * _cellSize;

        snapPosition += contentParentWorldPosition;

        var localPosition = _cellsContentParentElement.WorldToLocal(snapPosition);

        var gridPosition = Vector2Int.zero;
        gridPosition.x = Mathf.FloorToInt(localPosition.x / _cellSize);
        gridPosition.y = Mathf.FloorToInt(localPosition.y / _cellSize);

        return gridPosition;
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

    public void ResetCellsColor()
    {
        for (int i = 0, length = _cells.Count; i < length; i++)
        {
            _cells[i].style.unityBackgroundImageTintColor = Color.white;
        }
    }
}
