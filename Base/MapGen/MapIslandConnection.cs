using Godot;
using System.Collections.Generic;

public class MapIslandConnection
{
	public MapIsland SourceIsland { get; }
	public MapIsland TargetIsland { get; }

	public Vector2I SourceCoord { get; }
	public Vector2I TargetCoord { get; }

	public float DistanceSq { get; }
	public bool IsValid { get; }

    public MapIslandConnection(
		MapIsland sourceIsland, 
		MapIsland targetIsland,
		MapConnectPoint connectPoint)
    {
        SourceIsland = sourceIsland;
        TargetIsland = targetIsland;

		IsValid = true;

        switch (connectPoint)
        {
            case MapConnectPoint.StartFromCenter:
				{
					SourceCoord = sourceIsland.GetCenterCoord();
					TargetCoord = targetIsland.GetCenterCoord();
				}
                break;

            case MapConnectPoint.StartFromClosest:
				{
					var border1 = sourceIsland.GetBorder();
					var border2 = targetIsland.GetBorder();

					int? closestDistSq = null;
					List<Vector2I> closestSourceCoords = new();
					List<Vector2I> closestTargetCoords = new();

					foreach (Vector2I coord1 in border1)
						foreach (Vector2I coord2 in border2)
						{
							int distSq = (coord1 - coord2).LengthSquared();
							if (closestDistSq != null && closestDistSq < distSq)
								continue;

							if (closestDistSq == null)
							{
								closestDistSq = distSq;
								closestSourceCoords.Add(coord1);
								closestTargetCoords.Add(coord2);
							}
							else if (closestDistSq == distSq)
							{
								closestSourceCoords.Add(coord1);
								closestTargetCoords.Add(coord2);
							}
							else
							{
								closestDistSq = distSq;
								closestSourceCoords.Clear();
								closestTargetCoords.Clear();
								closestSourceCoords.Add(coord1);
								closestTargetCoords.Add(coord2);
							}
						}

					Debug.Assert(closestSourceCoords.Count > 0);
					if (closestSourceCoords.Count == 1)
					{
						SourceCoord = closestSourceCoords[0];
						TargetCoord = closestTargetCoords[0];
					}
					else
					{
						int i = GD.RandPosInt() % closestSourceCoords.Count;
						SourceCoord = closestSourceCoords[i];
						TargetCoord= closestTargetCoords[i];
					}
				}
                break;

            case MapConnectPoint.StartFromWall:
				{
					var border1 = sourceIsland.GetBorder();
					var border2 = targetIsland.GetBorder();

					int? closestDistSq = null;
					List<Vector2I> closestSourceCoords = new();
					List<Vector2I> closestTargetCoords = new();

					for (int i = 0; i < border1.Count; ++i)
					{
						Vector2I coord1 = border1[i];
						Vector2I prev1 = border1[Mathf.PosMod(i - 1, border1.Count)];
						Vector2I next1 = border1[Mathf.PosMod(i + 1, border1.Count)];

						if (!((coord1.X == prev1.X && coord1.X == next1.X) || (coord1.Y == prev1.Y && coord1.Y == next1.Y)))
							continue;

						for (int j = 0; j < border2.Count; ++j)
						{
                            Vector2I coord2 = border2[j];
                            Vector2I prev2 = border2[Mathf.PosMod(j - 1, border2.Count)];
                            Vector2I next2 = border2[Mathf.PosMod(j + 1, border2.Count)];

                            if (!((coord2.X == prev2.X && coord2.X == next2.X) || (coord2.Y == prev2.Y && coord2.Y == next2.Y)))
                                continue;

							int distSq = (coord1 - coord2).LengthSquared();
							if (closestDistSq != null && closestDistSq < distSq)
								continue;

                            if (closestDistSq == null)
                            {
                                closestDistSq = distSq;
                                closestSourceCoords.Add(coord1);
                                closestTargetCoords.Add(coord2);
                            }
                            else if (closestDistSq == distSq)
                            {
                                closestSourceCoords.Add(coord1);
                                closestTargetCoords.Add(coord2);
                            }
                            else
                            {
                                closestDistSq = distSq;
                                closestSourceCoords.Clear();
                                closestTargetCoords.Clear();
                                closestSourceCoords.Add(coord1);
                                closestTargetCoords.Add(coord2);
                            }
                        }
					}

					if (closestDistSq == null)
					{
						IsValid = false;
						return;
                    }

                    Debug.Assert(closestSourceCoords.Count > 0);
                    if (closestSourceCoords.Count == 1)
                    {
                        SourceCoord = closestSourceCoords[0];
                        TargetCoord = closestTargetCoords[0];
                    }
                    else
                    {
                        int i = GD.RandPosInt() % closestSourceCoords.Count;
                        SourceCoord = closestSourceCoords[i];
                        TargetCoord = closestTargetCoords[i];
                    }
                }
                break;

            case MapConnectPoint.StartFromRandom:
				{
					SourceCoord = sourceIsland.Coords.GetRandomCoord();
					TargetCoord = targetIsland.Coords.GetRandomCoord();
				}
                break;

			default:
                Debug.Fail($"Unknown MapConnectPoint [{connectPoint}]");
				break;
        }

		DistanceSq = (SourceCoord - TargetCoord).LengthSquared();
    }

	public bool Has(MapIsland island)
	{
		return SourceIsland == island || TargetIsland == island;
	}

	public MapIsland GetOther(MapIsland island)
	{
		Debug.Assert(Has(island));
		return 
			SourceIsland == island
			? TargetIsland
			: SourceIsland;
	}

	public static MapIslandConnection GetConnection(List<MapIslandConnection> connections, MapIsland sourceIsland, MapIsland targetIsland)
	{
		Debug.Assert(sourceIsland != targetIsland);
		foreach (var connection in connections)
			if (connection.Has(sourceIsland) && connection.Has(targetIsland))
				return connection;

		Debug.Fail();
		return null;
	}

	public static List<MapIslandConnection> GetConnections(List<MapIslandConnection> connections, MapIsland island)
	{
		List<MapIslandConnection> foundConnections = new();
		foreach (var connection in connections)
			if (connection.Has(island)) 
				foundConnections.Add(connection);
		return foundConnections;
	}

	public static bool HasConnection(List<MapIslandConnection> connections, MapIsland island)
	{
		foreach (var connection in connections)
			if (connection.Has(island))
				return true;

		return false;
	}

	public static void EraseConnections(List<MapIslandConnection> connections, MapIsland island)
	{
		for (int i = connections.Count - 1; i >= 0; --i)
			if (connections[i].Has(island))
				connections.RemoveAt(i);
	}

	public static float GetDistanceSq(List<MapIslandConnection> connections, MapIsland sourceIsland, MapIsland targetIsland)
	{
		return GetConnection(connections, sourceIsland, targetIsland).DistanceSq;
	}
}
