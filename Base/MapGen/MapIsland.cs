using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class MapIsland 
{
	private int _id;
	private readonly MapCoords _mapCoords;
	private readonly Vector2I _topLeftCoord;

	private Vector2I? _cachedCenterCoord;
	private Vector2I? _cachedCenterCoordInIsland;
	private List<Vector2I> _cachedBorderCoords;

	public MapIsland(MapCoords mapCoords, Vector2I topLeftCoord)
	{
		_mapCoords = mapCoords;
		_topLeftCoord = topLeftCoord;

		Debug.Assert(_mapCoords.HasCoord(topLeftCoord));
	}

	public int Id
	{
		get { return _id; }
		set { _id = value; }
	}

	public MapCoords Coords 
	{ 
		get { return _mapCoords; } 
	}

    public override string ToString()
    {
		if (_id == 0)
			return $"Coords: {_mapCoords.Count}";

        return $"Id {_id}  Coords: {_mapCoords.Count}";
	}

	public Vector2I GetCenterCoord(bool inIsland = true)
	{
		if (_cachedCenterCoord == null)
		{
			Vector2 first = _topLeftCoord;
			Vector2 last = _topLeftCoord;

			foreach (Vector2I coord in _mapCoords.Coords)
			{
				if (first.X > coord.X)
					first.X = coord.X;
				if (first.Y > coord.Y)
					first.Y = coord.Y;
				if (last.X < coord.X)
					last.X = coord.X;
				if (last.Y < coord.Y)
					last.Y = coord.Y;
			}

			_cachedCenterCoord = (Vector2I)(first + (last - first) * 0.5f);
		}

		if (!inIsland)
			return _cachedCenterCoord.Value;

		if (_cachedCenterCoordInIsland == null)
		{
			if (_mapCoords.HasCoord(_cachedCenterCoord.Value))
			{
				_cachedCenterCoordInIsland = _cachedCenterCoord;
			}
			else
			{
				float? closestDist = null;
				foreach (Vector2I coord in _mapCoords.Coords)
				{
					float dist = (_cachedCenterCoord.Value - coord).LengthSquared();
					if (closestDist == null || closestDist > dist)
					{
						closestDist = dist;
						_cachedCenterCoordInIsland = coord;
					}
				}
			}
		}

		return _cachedCenterCoordInIsland.Value;
	}

	public List<Vector2I> GetOutline(Orientation orientation, bool optimize = true)
	{
		// Important: We assume, that x/y starts at the topmost row, on its leftmost tile.

		Vector2I topRight = new Vector2I(1, 0);
		Vector2I topLeft = new Vector2I(0, 0);
		Vector2I bottomLeft = new Vector2I(0, 1);
		Vector2I bottomRight = new Vector2I(1, 1);

		List<Vector2I> corners;

		Vector2I start = _topLeftCoord;
		Vector2I coord = start;

		List<Vector2I> outline = new();

		Direction4 startDir;

		if (orientation == Orientation.CW)
		{
			// Create clockwise outline

			if (_mapCoords.HasCoordAtDir(coord, Direction4.E))
			{
				startDir = Direction4.E;
				coord.X += 1;
			}
			else if (_mapCoords.HasCoordAtDir(coord, Direction4.S))
			{
				startDir = Direction4.S;
				coord.Y += 1;
			}
			else
			{
				outline.Add(coord + topRight);
				outline.Add(coord + bottomRight);
				outline.Add(coord + bottomLeft);
				outline.Add(coord + topLeft);
				return outline;
			}

			// corner_index points to the bottom left corner if indexed with dir
			corners = new() { bottomLeft, topLeft, topRight, bottomRight };
		}
		else
		{
			// Create counter-clockwise outline

			if (_mapCoords.HasCoordAtDir(coord, Direction4.S))
			{
				startDir = Direction4.S;
				coord.Y += 1;
			}
			else if (_mapCoords.HasCoordAtDir(coord, Direction4.E))
			{
				startDir = Direction4.E;
				coord.X += 1;
			}
			else
			{
				outline.Add(coord + topLeft);
				outline.Add(coord + bottomLeft);
				outline.Add(coord + bottomRight);
				outline.Add(coord + topRight);
				return outline;
            }

			// corner_index points to the bottom right corner in direction dir
			corners = new() { bottomRight, bottomLeft, topLeft, topRight };
        }

		Direction4 dir = startDir;

		int debug = 0;


		while (++debug < 10000)
		{
			// Shit, this is tricky....
			// Convention: We only add outline coords, which are NOT shared with the next tile!

			if (_mapCoords.HasCoordAtDir(coord, dir.Turn(orientation.Reverse())))
			{
				dir = dir.Turn(orientation.Reverse());
			}
			else if (_mapCoords.HasCoordAtDir(coord, dir))
			{
				outline.Add(coord + corners[(int)dir]);
			}
			else if (_mapCoords.HasCoordAtDir(coord, dir.Turn(orientation)))
			{
				outline.Add(coord + corners[(int)dir]);
				outline.Add(coord + corners[(int)dir.Turn(orientation)]);

				dir = dir.Turn(orientation);
			}
			else
			{
				outline.Add(coord + corners[(int)dir]);
				outline.Add(coord + corners[(int)dir.Turn(orientation)]);
				outline.Add(coord + corners[(int)dir.Reverse()]);

				dir = dir.Reverse();
			}

			if (coord == start && dir == startDir)
				break;

			coord = coord.StepDir(dir);
        }

		Debug.Assert(debug < 10000);


		if (optimize && outline.Count > 2)
		{
			int size = outline.Count;

			Vector2I prev = outline.Last();
			Vector2I current = outline.First();

			int i = 0;

			while (i < size)
			{
				Vector2I next;

				if (i < size - 1)
					next = outline[i + 1];
				else
					next = outline[0];

				if (current.X == prev.X && current.X == next.X)
				{
					outline.RemoveAt(i);
					size -= 1;
				}
				else if (current.Y == prev.Y && current.Y == next.Y)
				{
					outline.RemoveAt(i);
					size -= 1;
				}
				else
				{
					i += 1;
				}

				prev = current;
				current = next;
			}
		}

		return outline;
    }

	public ReadOnlyCollection<Vector2I> GetBorder(Orientation orientation = Orientation.CW)
	{
		if (_cachedBorderCoords != null)
		{
			return _cachedBorderCoords.AsReadOnly();
		}

		_cachedBorderCoords = new();

        // Important: We assume, that x/y starts at the topmost row, on its leftmost tile.

        Vector2I start = _topLeftCoord;
        Vector2I coord = start;


		Direction4 startDir;

		if (orientation == Orientation.CW)
		{
			// Create clockwise border

			if (_mapCoords.HasCoordAtDir(coord, Direction4.E))
			{
				startDir = Direction4.E;
				coord.X += 1;
			}
			else if (_mapCoords.HasCoordAtDir(coord, Direction4.S))

			{
				startDir = Direction4.S;
				coord.Y += 1;
			}
			else
			{
                _cachedBorderCoords.Add(coord);
				return _cachedBorderCoords.AsReadOnly();
			}
		}
		else
		{
			// Create counter-clockwise outline

			if (_mapCoords.HasCoordAtDir(coord, Direction4.S))
			{
				startDir = Direction4.S;
				coord.Y += 1;
			}
			else if (_mapCoords.HasCoordAtDir(coord, Direction4.E))
			{
				startDir = Direction4.E;
				coord.X += 1;
			}
			else
			{
                _cachedBorderCoords.Add(coord);
				return _cachedBorderCoords.AsReadOnly();
			}
		}

        Direction4 dir = startDir;
		int debug = 0;
		List<Vector2I> outline = new();

		while (++debug < 10000)
		{
			// Shit, this is tricky....
			// Convention: We only add outline coords, which are NOT shared with the next tile!

			_cachedBorderCoords.Add(coord);


			if (_mapCoords.HasCoordAtDir(coord, dir.Turn(orientation.Reverse())))
			{
				dir = dir.Turn(orientation.Reverse());
			}
			else if (_mapCoords.HasCoordAtDir(coord, dir))
			{
				// Do Nothing
			}
			else if (_mapCoords.HasCoordAtDir(coord, dir.Turn(orientation)))
			{
				dir = dir.Turn(orientation);
			}
			else
			{
				dir = dir.Turn(orientation.Reverse());
			}

			if (coord == start && dir == startDir)
				break;

			coord = coord.StepDir(dir);
        }

		Debug.Assert(debug < 10000);

		return _cachedBorderCoords.AsReadOnly();
	}
}
