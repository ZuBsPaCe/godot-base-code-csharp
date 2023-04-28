using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class MapCoords
{
	private readonly List<Vector2I> _coordList;
	private readonly HashSet<Vector2I> _coords;

	public MapCoords(IEnumerable<Vector2I> coords)
	{
		Debug.Assert(coords.Any(), "Reason: GetRandomCoord could fail...");

		_coordList = coords.ToList();
		_coords = new HashSet<Vector2I>(coords);
	}

	public ReadOnlyCollection<Vector2I> Coords
	{
		get { return _coordList.AsReadOnly(); }
	}

	public int Count
	{
		get { return _coords.Count; }
	}

	public bool HasCoord(Vector2I coord)
	{
		return _coords.Contains(coord);
	}

	public bool HasCoord(int x, int y)
	{
		return _coords.Contains(new Vector2I(x, y));
	}

	public bool HasCoordAtDir(int x, int y, Direction4 dir)
	{
		return HasCoordAtDir(new Vector2I(x, y), dir);
	}

	public bool HasCoordAtDir(Vector2I coord, Direction4 dir)
	{
		return _coords.Contains(coord.StepDir(dir));
	}

	public bool HasCoordAtDir(int x, int y, Direction8 dir)
	{
		return HasCoordAtDir(new Vector2I(x, y), dir);
	}

	public bool HasCoordAtDir(Vector2I coord, Direction8 dir)
	{
		return _coords.Contains(coord.StepDir(dir));
	}

	public bool TouchesCoord4(Vector2I coord)
	{
		if (_coords.Contains(coord))
		{
			return true;
		}

		foreach (Direction4 dir in Enum.GetValues<Direction4>())
		{
			if (HasCoordAtDir(coord, dir))
				return true;
		}

		return false;
	}

	public bool TouchesCoord8(Vector2I coord)
	{
		if (_coords.Contains(coord))
		{
			return true;
		}

		foreach (Direction8 dir in Enum.GetValues<Direction8>())
		{
			if (HasCoordAtDir(coord, dir))
				return true;
		}

		return false;
	}

	public Vector2I GetRandomCoord()
	{
		return _coordList.GetRandomItem();
	}
}
