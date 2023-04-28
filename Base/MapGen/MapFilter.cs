using Godot;
using System;
using System.Collections.Generic;

public class MapFilter
{
	private MapFilterMode _mode;
	private int _margin;
	private List<Rect2I> _rects = new();
	private HashSet<Vector2I> _coords = new();


	public MapFilter(MapFilterMode mode, int margin = 0)
	{
		_mode = mode;
		_margin = margin;
    }

	public void SetMode(MapFilterMode mode)
	{
		_mode = mode;
    }

	public void AddRect(Rect2I rect)
	{
		_rects.Add(rect.Grow(_margin));
    }

	public void AddCoord(Vector2I coord)
	{
		_coords.Add(coord);

		if (_margin > 0)
		{
			foreach (Direction8 dir in Enum.GetValues<Direction8>())
				_coords.Add(coord.StepDir(dir));
        }
    }

	public void AddCoord(IList<Vector2I> coords)
	{
		_coords.AddRange(coords);
    }

	public bool IsValidCoord(Vector2I coord)
	{
        switch (_mode)
        {
            case MapFilterMode.Disallow:
                foreach (Rect2I rect in _rects)
                    if (rect.HasPoint(coord))
                        return false;
                
                if (_coords.Contains(coord)) 
                    return false;

                return true;

            case MapFilterMode.Allow:
                foreach (Rect2I rect in _rects)
                    if (rect.HasPoint(coord))
                        return true;

                if (_coords.Contains(coord))
                    return true;

                return false;

            case MapFilterMode.Ignore:
                return true;

            default:
                Debug.Fail($"Unknown MapFilterMode [{_mode}]");
                return false;
        }
    }

	public bool IsValidCoord(int x, int y)
    {
        return IsValidCoord(new Vector2I(x, y));
    }

    public bool IsValidRect(Rect2I rect)
    {
        switch (_mode)
        {
            case MapFilterMode.Disallow:
                foreach (Rect2 r in _rects)
                    if (r.Intersects(rect))
                        return false;

                foreach (Vector2I coord in _coords)
                    if (rect.HasPoint(coord))
                        return false;

                return true;

            case MapFilterMode.Allow:
                foreach (Rect2 r in _rects)
                    if (r.Intersects(rect))
                        return true;

                foreach (Vector2I coord in _coords)
                    if (rect.HasPoint(coord))
                        return true;

                return false;

            case MapFilterMode.Ignore:
                return true;

            default:
                Debug.Fail($"Unknown MapFilterMode [{_mode}]");
                return false;
        }
    }

    public void Clear()
    {
        _rects.Clear();
        _coords.Clear();
    }
}
