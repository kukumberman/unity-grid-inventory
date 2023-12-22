using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryGridCollectionElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InventoryGridCollectionElement, UxmlTraits> { }

    private int _cellSize;

    private ScrollView _scrollView;
    private VisualElement _cellsContentParentElement;
    private VisualElement _itemsContentParentElement;

    private Grid2D _grid;
    private List<VisualElement> _cells;

    public int TotalPixelWidth { get; private set; }
    public Vector2 PixelGridSize { get; private set; }

    public void Setup(int cellSize)
    {
        _cellSize = cellSize;
    }

    public void CreateGrid(Vector2Int gridSize, Func<VisualElement> cellFactory)
    {
        _scrollView = this.Q<ScrollView>();
        _scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        var scrollContent = _scrollView.Q<VisualElement>("unity-content-container");
        _cellsContentParentElement = scrollContent[0];
        _itemsContentParentElement = scrollContent[1];

        for (int i = _cellsContentParentElement.childCount - 1; i >= 0; i--)
        {
            _cellsContentParentElement.RemoveAt(i);
        }

        var pixelGridWidth = gridSize.x * _cellSize;
        var pixelGridHeight = gridSize.y * _cellSize;

        _cellsContentParentElement.style.width = new StyleLength(
            new Length(pixelGridWidth, LengthUnit.Pixel)
        );
        _cellsContentParentElement.style.height = new StyleLength(
            new Length(pixelGridHeight, LengthUnit.Pixel)
        );

        var totalPixelWidth = pixelGridWidth + 24;

        _scrollView.style.width = new StyleLength(new Length(totalPixelWidth, LengthUnit.Pixel));

        var cellCount = gridSize.x * gridSize.y;

        _cells = new List<VisualElement>(cellCount);
        _grid = new Grid2D(gridSize.x, gridSize.y);

        for (int i = 0; i < cellCount; i++)
        {
            var element = cellFactory();
            _cellsContentParentElement.Add(element);
            _cells.Add(element);
        }

        TotalPixelWidth = totalPixelWidth;
        PixelGridSize = new Vector2(pixelGridWidth, pixelGridHeight);
    }

    public void FitWidthAndHeight()
    {
        FitWidth();
        FitHeight();
    }

    public void FitWidth()
    {
        _scrollView.style.width = new StyleLength(new Length(TotalPixelWidth, LengthUnit.Pixel));
        style.width = _scrollView.style.width;
    }

    public void FitHeight()
    {
        _scrollView.style.height = new StyleLength(new Length(PixelGridSize.y, LengthUnit.Pixel));
        style.height = _scrollView.style.height;
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
