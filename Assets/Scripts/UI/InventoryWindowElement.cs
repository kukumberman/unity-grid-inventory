using UnityEngine;
using UnityEngine.UIElements;

public sealed class InventoryWindowElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InventoryWindowElement, UxmlTraits> { }

    private VisualElement _titlebarElement;
    private Label _labelTitle;

    private Vector3 _dragStartPosition;
    private Vector3 _targetPositionOnDragStart;

    public InventoryGridCollectionElement GridCollection { get; private set; }

    public void Setup()
    {
        _titlebarElement = this.Q<VisualElement>("titlebar");
        _labelTitle = _titlebarElement.Q<Label>("label-title");
        var btnClose = _titlebarElement.Q<Button>("btn-close");
        btnClose.clicked += () =>
        {
            RemoveFromHierarchy();
        };

        var btnSort = _titlebarElement.Q<Button>("btn-sort");
        btnSort.clicked += () =>
        {
            InventoryManager.Singleton.Sort(GridCollection.DynamicId);
        };

        GridCollection = this.Q<InventoryGridCollectionElement>("inventory");

        _titlebarElement.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        _titlebarElement.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        _titlebarElement.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
    }

    public void MakeAbsolute()
    {
        style.position = new StyleEnum<Position>(Position.Absolute);
    }

    public void SetScreenPosition(Vector2 position)
    {
        style.left = new StyleLength(new Length(position.x, LengthUnit.Pixel));
        style.top = new StyleLength(new Length(position.y, LengthUnit.Pixel));
    }

    public void SetTitle(string title)
    {
        _labelTitle.text = title;
    }

    public void UpdateWidth()
    {
        style.width = new StyleLength(new Length(GridCollection.TotalPixelWidth, LengthUnit.Pixel));
    }

    private void PointerDownHandler(PointerDownEvent evt)
    {
        if (evt.button == 0)
        {
            var target = evt.currentTarget as VisualElement;
            target.CapturePointer(evt.pointerId);

            _dragStartPosition = evt.position;
            _targetPositionOnDragStart.x = target.parent.resolvedStyle.left;
            _targetPositionOnDragStart.y = target.parent.resolvedStyle.top;
            BringToFront();
        }
    }

    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (evt.button == 0)
        {
            var target = evt.currentTarget as VisualElement;
            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);

                _dragStartPosition = Vector3.zero;
                _targetPositionOnDragStart = Vector3.zero;
            }
        }
    }

    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        var target = evt.currentTarget as VisualElement;
        if (target.HasPointerCapture(evt.pointerId))
        {
            Debug.Assert(target.parent == this);

            var deltaPosition = evt.position - _dragStartPosition;
            var position = _targetPositionOnDragStart + deltaPosition;
            target.parent.style.left = new StyleLength(new Length(position.x, LengthUnit.Pixel));
            target.parent.style.top = new StyleLength(new Length(position.y, LengthUnit.Pixel));
        }
    }
}
