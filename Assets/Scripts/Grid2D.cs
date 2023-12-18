using UnityEngine;

public sealed class Grid2D
{
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;

    public Grid2D(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public bool IsInside(int x, int y)
    {
        var xx = x >= 0 && x < _width;
        var yy = y >= 0 && y < _height;
        return xx && yy;
    }

    public int GridToIndex(int x, int y)
    {
        return y * _width + x;
    }

    public void IndexToGrid(int index, out int x, out int y)
    {
        x = index % _width;
        y = Mathf.FloorToInt(index / _width);
    }
}
