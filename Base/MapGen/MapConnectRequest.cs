using System;
using System.Collections.Generic;

public class MapConnectRequest<TYPE> where TYPE : Enum
{
    // TileType => Avoidance (=Weight). Those tiles can be used for paths.
    // Higher Weight = More avoidance
	private Dictionary<TYPE, float> _digTileTypes = new();

	public TYPE PathTileType { get; }
	public MapConnectStrategy Strategy { get; }
	public MapConnectPoint ConnectPoint { get; }
	public bool Manhattan { get; }

	public MapConnectAvoidance Avoidance { get; set; } = MapConnectAvoidance.None;
	public float AvoidanceFactor { get; set; } = 2f;

	// Only valid for MapConnectStrategy.RANDOM
	// random_connection_to_others/self is a minimum
	// For self paths, you need to set avoidance and connect_point accordingly.
	public int RandomConnectionToOthers { get; set; } = 1;
	public float RandomConnectionToOthersActivation { get; set; } = 1f;
	public int RandomConnectionToSelf { get; set; } = 1;
	public float RandomConnectionToSelfActivation { get; set; } = 0f;
	public int RandomConnectionIterations { get; set; } = 20;

    public MapConnectRequest(
		TYPE pathTileType, 
		MapConnectStrategy strategy = MapConnectStrategy.Minimal, 
		MapConnectPoint connectPoint = MapConnectPoint.StartFromCenter, 
		bool manhattan = false)
    {
        PathTileType = pathTileType;
        Strategy = strategy;
        ConnectPoint = connectPoint;
        Manhattan = manhattan;

		AddDigTile(pathTileType);
    }

	public void AddDigTile(TYPE digTileType, float avoidance = 1f)
	{
		_digTileTypes[digTileType] = avoidance;
	}

	public bool CanDig(TYPE tileType, out float avoidance)
	{
		return _digTileTypes.TryGetValue(tileType, out avoidance);
	}
}
