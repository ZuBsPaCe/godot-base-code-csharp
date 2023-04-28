using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public class Mapper<TYPE, ITEM> where TYPE : Enum where ITEM : class, new()
{
    private readonly Map<TYPE, ITEM> _map;
    private readonly AStarGrid2D _astar = new();


    public Mapper(Map<TYPE, ITEM> map, int seed = 0)
    {
        _map = map;

        if (seed == 0)
        {
            GD.Randomize();
            seed = GD.RandPosInt();
        }

        GD.Print($"Mapper seed: {seed}");
        GD.Seed((ulong) seed);
    }


    public bool TryFillArea(Rect2I rect, MapFilter filter, TYPE tileType)
    {
        if (!filter.IsValidRect(rect))
            return false;


        // request_area() can invoke this with null.
        if (tileType == null)
            return true;


        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                _map.SetType(x, y, tileType);

        return true;
    }

    public bool TryFillAreaBorder(Rect2I rect, MapFilter filter, TYPE tileType, int border = 1)
    {
        if (!filter.IsValidRect(rect))
            return false;

        for (int y = rect.Position.Y; y < Mathf.Min(rect.Position.Y + border, rect.End.Y); ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                if (filter.IsValidCoord(x, y))
                    _map.SetType(x, y, tileType);

        for (int y = Mathf.Max(rect.End.Y - border, rect.Position.Y); y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                if (filter.IsValidCoord(x, y))
                    _map.SetType(x, y, tileType);

        for (int x = rect.Position.X; x < Mathf.Min(rect.Position.X + border, rect.End.X); ++x)
            for (int y = rect.Position.Y; y < rect.End.Y; ++y)
                if (filter.IsValidCoord(x, y))
                    _map.SetType(x, y, tileType);

        for (int x = Mathf.Max(rect.End.X - border, rect.Position.X); x < rect.End.X; ++x)
            for (int y = rect.Position.Y; y < rect.End.Y; ++y)
                if (filter.IsValidCoord(x, y))
                    _map.SetType(x, y, tileType);

        return true;
    }

    public bool TryRequestArea(Rect2I rect, MapFilter filter, MapAreaRequest request, TYPE tileType, out Rect2I area)
    {
        int maxWidth = Mathf.Min(request.MaxWidth, rect.Size.X);

        if (maxWidth < request.MinWidth)
        {
            Debug.Fail();
            area = default;
            return false;
        }

        int maxHeight = Mathf.Min(request.MaxHeight, rect.Size.Y);

        if (maxHeight < request.MinHeight)
        {
            Debug.Fail();
            area = default;
            return false;
        }

        for (int iteration = 0; iteration < request.Iterations; ++iteration)
        {
            int width =
                request.MinWidth < maxWidth
                ? GD.RandRange(request.MinWidth, maxWidth)
                : request.MinWidth;

            int height =
                request.MinHeight < maxHeight
                ? GD.RandRange(request.MinHeight, maxHeight)
                : request.MinHeight;

            int x;

            if (!request.CenteredX)
            {
                int xMin = rect.Position.X;
                int xMax = rect.End.X - width;

                if (xMin < xMax)
                    x = GD.RandRange(xMin, xMax);
                else
                    x = xMin;
            }
            else
            {
                x = (rect.Position.X + rect.Size.X / 2) - width / 2;
            }

            int y;

            if (!request.CenteredY)
            {
                int yMin = rect.Position.Y;
                int yMax = rect.End.Y - height;

                if (yMin < yMax)
                    y = GD.RandRange(yMin, yMax);
                else
                    y = yMin;
            }
            else
            {
                y = (rect.Position.Y + rect.Size.Y / 2) - height / 2;
            }

            area = new(x, y, width, height);

            if (TryFillArea(area, filter, tileType))
                return true;
        }

        area = default;
        return false;
    }

    public void RandomizeArea(Rect2I rect, MapFilter filter, TYPE tileType, float chance)
    {
        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
            {
                if (!filter.IsValidCoord(x, y)) 
                    continue;

                if (GD.Randf() < chance)
                    _map.SetType(x, y, tileType);
            }
    }

    public void EvolveArea(
        Rect2I rect, 
        MapFilter filter, 
        TYPE livingTileType, 
        int bornIfMoreNeighbors, 
        TYPE deadTileType, 
        int dieIfLessNeighbors, 
        bool outsideIsAlive, 
        int steps)
    {
        List<Vector2I> dieCoords = new();
        List<Vector2I> bornCoords = new();

        for (int step = 0; step < steps; ++step)
        {
            dieCoords.Clear();
            bornCoords.Clear();

            for (int y = rect.Position.Y; y <= rect.End.Y; ++y)
                for (int x = rect.Position.X; x <= rect.End.X; ++x)
                {
                    if (!filter.IsValidCoord(x, y))
                        continue;

                    int livingNeighbours = 0;
                    Vector2I coord = new(x, y);

                    foreach (Direction8 dir in Enum.GetValues<Direction8>())
                    {
                        Vector2I otherCoord = coord.StepDir(dir);

                        if (!rect.HasPoint(otherCoord))
                        {
                            if (outsideIsAlive)
                                livingNeighbours += 1;
                        }
                        else if (_map.IsType(otherCoord, livingTileType))
                        {
                            livingNeighbours += 1;
                        }
                    }

                    if (livingNeighbours < dieIfLessNeighbors)
                        dieCoords.Add(coord);
                    else if (livingNeighbours > bornIfMoreNeighbors)
                        bornCoords.Add(coord);
                }

            foreach (Vector2I coord in dieCoords)
                _map.SetType(coord, deadTileType);

            foreach (Vector2I coord in bornCoords)
                _map.SetType(coord, livingTileType);
        }
    }

    public bool TryGetRandomCoord(Rect2I rect, MapFilter filter, TYPE tileType, out Vector2I coord)
    {
        List<Vector2I> possibleCoords = GetCoords(rect, filter, tileType);
        return possibleCoords.TryGetRandomItem(out coord);
    }

    public bool TryGetLastRandomCoordInDir(Rect2I rect, MapFilter filter, TYPE tileType, Direction8 dir, out Vector2I coord)
    {
        List<Vector2I> possibleCoords = GetCoords(rect, filter, tileType);

        if (possibleCoords.Count == 1)
        {
            coord = possibleCoords[0];
            return true;
        }

        if (possibleCoords.Count == 0)
        {
            coord = default;
            return false;
        }

        Vector2I targetCoord;

        switch (dir)
        {
            case Direction8.N:
				targetCoord = new Vector2I(rect.Position.X + rect.Size.X / 2, rect.Position.Y - 1);
                break;
            case Direction8.E:
				targetCoord = new Vector2I(rect.End.X, rect.Position.Y + rect.Size.Y / 2);
                break;
            case Direction8.S:
				targetCoord = new Vector2I(rect.Position.X + rect.Size.X / 2, rect.End.Y);
                break;
            case Direction8.W:
				targetCoord = new Vector2I(rect.Position.X - 1, rect.Position.Y + rect.Size.Y / 2);
                break;
            case Direction8.NE:
				targetCoord = new Vector2I(rect.End.X, rect.Position.Y - 1);
                break;
            case Direction8.SE:
				targetCoord = new Vector2I(rect.End.X, rect.End.Y);
                break;
            case Direction8.SW:
				targetCoord = new Vector2I(rect.Position.X - 1, rect.End.Y);
                break;
            case Direction8.NW:
				targetCoord = new Vector2I(rect.Position.X - 1, rect.Position.Y - 1);
                break;

            default:
                throw new Exception($"Unknown Direction8 [{dir}]");
        }

        List<Vector2I> bestCoords = new();
        int? bestDist = null;

        foreach (Vector2I checkCoord in possibleCoords)
        {
            int dist = (targetCoord - checkCoord).LengthSquared();
            if (bestDist == null || dist == bestDist)
            {
                bestDist = dist;
                bestCoords.Add(checkCoord);
            }
            else if (dist < bestDist)
            {
                bestDist = dist;
                bestCoords.Clear();

                bestCoords.Add(checkCoord);
            }
        }

        coord = bestCoords.GetRandomItem();
        return true;
    }

    public List<Vector2I> GetCoords(Rect2I rect, MapFilter filter, TYPE tileType)
    {
        return GetCoords(rect, filter, new List<TYPE>() { tileType });
    }

    public List<Vector2I> GetCoords(Rect2I rect, MapFilter filter, List<TYPE> tileTypes)
    {
        List<Vector2I> coords = new();

        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X;)
            {
                if (!filter.IsValidCoord(x, y))
                    continue;

                if (tileTypes.Contains(_map.GetType(x, y)))
                    coords.Add(new Vector2I(x, y));
            }

        return coords;
    }

    public List<MapIsland> GetIslands(Rect2I rect, TYPE tileType)
    {
        List<MapIsland> islands = new();

        HashSet<int> seenIndexes = new();

        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
            {
                int index = y * _map.Width + x;

                if (!seenIndexes.Add(index))
                    continue;

                if (_map.IsType(x, y, tileType))
                {
                    MapIsland mapIsland = FloodFillArea(rect, x, y, tileType, seenIndexes);
                    islands.Add(mapIsland);
                    mapIsland.Id = islands.Count;
                }
            }

        return islands;
    }

    private MapIsland FloodFillArea(Rect2I rect, int xStart, int yStart, TYPE tileType, HashSet<int> seenIndexes)
    {
        Queue<Vector2I> heads = new();
        heads.Enqueue(new Vector2I(xStart, yStart));

        int startIndex = yStart * _map.Width + xStart;
        seenIndexes.Add(startIndex);

        List<Vector2I> islandCoords = new() { new Vector2I(xStart, yStart) };

        while (heads.TryDequeue(out Vector2I head))
        {
            int x = head.X;
            int y = head.Y;

            for (int i = 0; i < 4; ++i)
            {
                int xNext;
                int yNext;

                switch (i)
                {
                    case 0:
                        xNext = x;
                        yNext = y + 1;
                        break;

                    case 1:
                        xNext = x + 1;
                        yNext = y;
                        break;

                    case 2:
                        xNext = x;
                        yNext = y - 1;
                        break;

                    default:
                        Debug.Assert(i == 3);

                        xNext = x - 1;
                        yNext = y;
                        break;
                }

                Vector2I nextCoord = new Vector2I(xNext, yNext);

                if (!rect.HasPoint(nextCoord))
                    continue;

                int index = yNext * _map.Width + xNext;
                if (seenIndexes.Contains(index))
                    continue;


                if (_map.IsInside(nextCoord))
                    if (_map.IsType(nextCoord, tileType))
                    {
                        heads.Enqueue(nextCoord);
                        seenIndexes.Add(index);
                        islandCoords.Add(nextCoord);
                    }
            }
        }

        return new MapIsland(new MapCoords(islandCoords), islandCoords[0]);
    }

    private List<MapIslandConnection> GetAllIslandConnections(List<MapIsland> islands, MapConnectRequest<TYPE> request)
    {
        List<MapIslandConnection> possibleConnections = new();

        for (int sourceIndex = 0; sourceIndex < islands.Count - 1; ++sourceIndex)
        {
            MapIsland sourceIsland = islands[sourceIndex];

            for (int targetIndex = sourceIndex + 1; targetIndex < islands.Count; ++targetIndex)
            {
                MapIsland targetIsland = islands[targetIndex];

                MapIslandConnection connection = new(sourceIsland, targetIsland, request.ConnectPoint);
                if (connection.IsValid)
                    possibleConnections.Add(connection);
            }
        }

        return possibleConnections;
    }

    public bool ConnectAllRects(Rect2I rect, MapFilter filter, List<Rect2I> islands, MapConnectRequest<TYPE> request)
    {
        List<MapIsland> realIslands = new();

        foreach (Rect2I island in islands)
        {
            List<Vector2I> coords = new();

            for (int y = island.Position.Y; y < island.End.Y; ++y)
                for (int x = island.Position.X; x < island.End.X; ++x)
                    coords.Add(new Vector2I(x, y));

            realIslands.Add(new MapIsland(new MapCoords(coords), coords[0]));
        }

        return ConnectAllIslands(rect, filter, realIslands, request);
    }

    private bool ConnectAllIslands(Rect2I rect, MapFilter filter, List<MapIsland> islands, MapConnectRequest<TYPE> request)
    {
        switch (request.Strategy)
        {
            case MapConnectStrategy.Minimal:
                {
                    var possibleConnections = GetAllIslandConnections(islands, request);
                    List<MapIsland> connectedIslands = new();
                    List<MapIsland> remainingIslands = islands.ToList();

                    while (remainingIslands.Count > 0)
                    {
                        List<MapIslandConnection> remainingConnections = new();

                        if (connectedIslands.Count == 0) 
                        {
                            // First iteration: Simply connect 2 closest islands => connected_islands
                            remainingConnections.AddRange(possibleConnections);
                        }
                        else
                        {
				            // Subsequent iterations: Find closest connection to an island already in connected_islands
                            foreach (MapIsland connectedIsland in connectedIslands)
                                foreach (MapIsland remainingIsland in remainingIslands)
                                    remainingConnections.Add(MapIslandConnection.GetConnection(possibleConnections, connectedIsland, remainingIsland));
                        }

                        remainingConnections.Sort((a, b) => a.DistanceSq.CompareTo(b.DistanceSq));

                        bool connected = false;

                        foreach (MapIslandConnection connection in remainingConnections)
                        {
                            MapIsland sourceIsland = connection.SourceIsland;
                            MapIsland targetIsland = connection.TargetIsland;

                            if (TryConnectIslands(rect, filter, connection, request))
                            {
                                if (!connectedIslands.Contains(sourceIsland))
                                    connectedIslands.Add(sourceIsland);

                                if (!connectedIslands.Contains(targetIsland))
                                    connectedIslands.Add(targetIsland);

                                remainingIslands.Remove(sourceIsland);
                                remainingIslands.Remove(targetIsland);
                                connected = true;
                                break;
                            }
                        }

                        if (!connected)
                            return false;
                    }

                    return true;
                }

            case MapConnectStrategy.Linear:
                {
                    var possibleConnections = GetAllIslandConnections(islands, request);
                    List<MapIsland> remainingIslands = islands.ToList();

                    MapIsland currentIsland = null;

                    while (remainingIslands.Count > 1)
                    {
                        if (currentIsland == null)
                        {
                            // First iteration: Find outermost island
                            Vector2I center = rect.GetCenter();
                            float farthestDist = 0;

                            foreach (MapIsland island in islands)
                            {
                                float dist = ((Vector2)island.GetCenterCoord(false)).DistanceSquaredTo(center);

                                if (currentIsland == null || farthestDist < dist)
                                {
                                    farthestDist = dist;
                                    currentIsland = island;
                                }
                            }
                        }
                        else
                        {
                            // Subsequent iterations: Find closest island
                            var nextConnections = MapIslandConnection.GetConnections(possibleConnections, currentIsland);

                            nextConnections.Sort((a, b) => a.DistanceSq.CompareTo(b.DistanceSq));

                            MapIsland nextIsland = null;

                            foreach (var connection in nextConnections)
                            {
                                if (TryConnectIslands(rect, filter, connection, request))
                                {
                                    nextIsland = connection.GetOther(currentIsland);
                                    break;
                                }
                            }

                            if (nextIsland == null)
                                return false;

                            MapIslandConnection.EraseConnections(possibleConnections, currentIsland);

                            currentIsland = nextIsland;
                        }

                        remainingIslands.Remove(currentIsland);
                    }

                    return true;
                }

            case MapConnectStrategy.Circular:
                {
                    Vector2I center = rect.GetCenter();
                    List<MapIsland> checkIslands = islands.ToList();
                    List<MapIsland> remainingIslands = new();

                    Vector2 currentDir = Vector2.Up;

                    while (checkIslands.Count > 1)
                    {
                        float closestAngle = 0;
                        MapIsland nextIsland = null;
                        Vector2 nextDir = Vector2.Zero;

                        foreach (MapIsland checkIsland in checkIslands)
                        {
                            Vector2 dir = (Vector2)(checkIsland.GetCenterCoord(false) - center);

                            var angle = currentDir.AngleTo(dir);

                            var isCloser = false;

                            if (nextIsland == null)
                            {
                                isCloser = true;
                            }
                            else if (angle >= 0.0)
                            {
                                if (closestAngle < 0.0 || angle < closestAngle)
                                    isCloser = true;
                            }
                            else
                            {
                                if (closestAngle < 0.0 && angle < closestAngle)
                                    isCloser = true;
                            }

                            if (isCloser)
                            {
                                closestAngle = angle;
                                nextIsland = checkIsland;
                                nextDir = dir;
                            }
                        }


                        checkIslands.Remove(nextIsland);
                        remainingIslands.Add(nextIsland);

                        currentDir = nextDir;
                    }

                    remainingIslands.Add(checkIslands[0]);

                    for (int i = 0; i < remainingIslands.Count; ++i)
                    {
                        MapIsland currentIsland = remainingIslands[i];

                        MapIsland nextIsland;

                        if (i + 1 < remainingIslands.Count)
                            nextIsland = remainingIslands[i + 1];
                        else
                            nextIsland = remainingIslands[0];

                        if (!TryConnectIslands(rect, filter, currentIsland, nextIsland, request))
                            return false;
                    }

                    return true;
                }

            case MapConnectStrategy.Closest1:
            case MapConnectStrategy.Closest2:
            case MapConnectStrategy.Closest3:
            case MapConnectStrategy.Closest4:
                {
                    if (islands.Count <= 1)
                        return false;

                    int targetConnectionCount;

                    switch (request.Strategy)
                    {
                        case MapConnectStrategy.Closest1:
                            targetConnectionCount = 1;
                            break;

                        case MapConnectStrategy.Closest2:
                            targetConnectionCount = 2;
                            break;

                        case MapConnectStrategy.Closest3:
                            targetConnectionCount = 3;
                            break;

                        case MapConnectStrategy.Closest4:
                            targetConnectionCount = 4;
                            break;

                        default:
                            Debug.Fail();
                            return false;
                    }


                    Dictionary<MapIsland, List<MapIslandConnection>> madeConnections = new();
                    foreach (MapIsland island in islands)
                        madeConnections[island] = new();

                    var allConnections = GetAllIslandConnections(islands, request);
                    bool connected = false;

                    foreach (MapIsland currentIsland in islands)
                    {
                        List<MapIslandConnection> currentConnections = madeConnections[currentIsland];
                        int currentConnectionCount = currentConnections.Count;

                        if (currentConnectionCount >= targetConnectionCount)
                            continue;

                        var possibleConnections = MapIslandConnection.GetConnections(allConnections, currentIsland);

                        possibleConnections.Sort((a, b) => a.DistanceSq.CompareTo(b.DistanceSq));

                        foreach (MapIslandConnection connection in possibleConnections)
                        {
                            var otherIsland = connection.GetOther(currentIsland);

                            if (MapIslandConnection.HasConnection(currentConnections, otherIsland))
                                continue;

                            if (TryConnectIslands(rect, filter, connection, request))
                            {
                                connected = true;

                                madeConnections[currentIsland].Add(connection);
                                madeConnections[otherIsland].Add(connection);

                                currentConnectionCount += 1;

                                if (currentConnectionCount >= targetConnectionCount)
                                    break;
                            }
                        }
                    }

                    return connected;
                }

            case MapConnectStrategy.Random:
                {
                    Dictionary<MapIsland, int> connectionCount = new();
                    foreach (MapIsland island in islands)
                        connectionCount[island] = 0;

                    bool connected = false;

                    foreach (MapIsland currentIsland in islands)
                    {
                        List<MapIsland> otherIslands = new();

                        foreach (MapIsland checkIsland in islands)
                            if (checkIsland != currentIsland)
                                otherIslands.Add(checkIsland);

                        int currentConnectionCount = connectionCount[currentIsland];

                        var canAddToOthers =
                            currentConnectionCount < request.RandomConnectionToOthers &&
                            otherIslands.Count > 0 &&
                            request.RandomConnectionToOthers > 0 &&
                            request.RandomConnectionToOthersActivation > 0.0;

                        var addToOthers = false;

                        if (canAddToOthers)
                        {
                            if (request.RandomConnectionToOthersActivation >= 1.0)
                                addToOthers = true;
                            else
                                addToOthers = GD.Randf() < request.RandomConnectionToOthersActivation;
                        }

                        if (addToOthers)
                        {
                            for (int i = currentConnectionCount; i < request.RandomConnectionToOthers; ++i)
                            {
                                var tryCounter = 1;

                                while (tryCounter <= request.RandomConnectionIterations)
                                {
                                    MapIsland nextIsland;

                                    if (otherIslands.Count > 1)
                                        nextIsland = otherIslands[GD.RandPosInt() % otherIslands.Count];
                                    else
                                        nextIsland = otherIslands[0];

                                    if (TryConnectIslands(rect, filter, currentIsland, nextIsland, request))
                                    {
                                        connected = true;
                                        connectionCount[currentIsland] += 1;
                                        connectionCount[nextIsland] += 1;
                                        break;
                                    }

                                    tryCounter += 1;
                                }
                            }
                        }


                        var addToSelf = false;

                        if (request.RandomConnectionToSelf > 0 && request.RandomConnectionToSelfActivation > 0.0)
                        {
                            if (request.RandomConnectionToSelfActivation >= 1.0)
                                addToSelf = true;
                            else
                                addToSelf = GD.Randf() < request.RandomConnectionToSelfActivation;
                        }

                        if (addToSelf)
                        {
                            for (int i = 0; i <request.RandomConnectionToSelf; ++i)
                            {
                                var tryCounter = 1;

                                while (tryCounter <= request.RandomConnectionIterations)
                                {
                                    if (TryConnectIslands(rect, filter, currentIsland, currentIsland, request))
                                    {
                                        connected = true;
                                        break;
                                    }

                                    tryCounter += 1;
                                }
                            }
                        } 
                    }

                    return connected;
                }

            default:
                Debug.Fail($"Unknown MapConnectStrategy [{request.Strategy}]");
                return false;
        }
    }


    public bool TryConnectIslands(Rect2I rect, MapFilter filter, MapIslandConnection connection, MapConnectRequest<TYPE> request)
    {
        InitAstar(rect, filter, request, connection.SourceIsland, connection.TargetIsland);

        Debug.Assert(connection.IsValid);

        var path = _astar.GetIdPath(connection.SourceCoord, connection.TargetCoord);

        if (path.Count > 0)
        {
            foreach (Vector2I coord in path)
                _map.SetType(coord, request.PathTileType);

            return true;
        }

        return false;
    }


    public bool TryConnectIslands(Rect2I rect, MapFilter filter, MapIsland sourceIsland, MapIsland targetIsland, MapConnectRequest<TYPE> request)
    {
        var connection = new MapIslandConnection(sourceIsland, targetIsland, request.ConnectPoint);

        if (connection.IsValid)
            return TryConnectIslands(rect, filter, connection, request);

        return false;
    }


    public void InitAstar(Rect2I rect, MapFilter filter, MapConnectRequest<TYPE> request, MapIsland island1, MapIsland island2)
    {
        _astar.Clear();

        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
        _astar.Size = new Vector2I(_map.Width, _map.Height);
        _astar.CellSize = new Vector2(96, 96);

        _astar.Update();

        if (request.Manhattan)
        {
            _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
            _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
        }
        else
        {
            _astar.DefaultComputeHeuristic = 0;
            _astar.DefaultEstimateHeuristic = 0;
        }

        var handleAvoidance = request.Avoidance != MapConnectAvoidance.None;

        List<Vector2I> avoidCoords = new();
        Vector2I coord = Vector2I.Zero;

        for (int y = 0; y < _map.Height; ++y)
            for (int x = 0; x < _map.Width; ++x)
            {
                coord.X = x;
                coord.Y = y;

                if (!rect.HasPoint(coord))
                {
                    _astar.SetPointSolid(coord, true);
                }
                else if (!filter.IsValidCoord(coord))
                {
                    _astar.SetPointSolid(coord, true);
                }
                else
                {
                    TYPE currentType = _map.GetType(coord);

                    if (request.CanDig(currentType, out float currentAvoidance))
                    {
                        _astar.SetPointSolid(coord, false);
                        _astar.SetPointWeightScale(coord, currentAvoidance);

                        if (handleAvoidance && currentType.Equals(request.PathTileType))
                            if (!island1.Coords.HasCoord(coord) && !island2.Coords.HasCoord(coord))
                                avoidCoords.Add(coord);
                    }
                    else
                    {
                        _astar.SetPointSolid(coord, true);
                    }
                }
            }


        if (handleAvoidance)
        {
            switch (request.Avoidance)
            {
                case MapConnectAvoidance.AvoidPathTileType:
                    {
                        foreach (var avoidCoord in avoidCoords)
                            _astar.SetPointWeightScale(avoidCoord, request.AvoidanceFactor);
                    }
                    break;

                case MapConnectAvoidance.BlockPathTileType:
                    {
                        foreach (var avoidCoord in avoidCoords)
                            _astar.SetPointSolid(avoidCoord, true);
                    }
                    break;


                case MapConnectAvoidance.AvoidPathTileTypeAndBorder:
                    {
                        foreach (var avoidCoord in avoidCoords)
                        {
                            _astar.SetPointWeightScale(avoidCoord, request.AvoidanceFactor);

                            foreach (Direction8 dir in Enum.GetValues<Direction8>())
                            {
                                var otherCoord = avoidCoord.StepDir(dir);

                                if (!island1.Coords.TouchesCoord4(otherCoord) && !island2.Coords.TouchesCoord4(otherCoord))
                                    _astar.SetPointWeightScale(otherCoord, request.AvoidanceFactor);
                            }
                        }
                    }
                    break;

                case MapConnectAvoidance.BlockPathTileTypeAndBorder:
                    {
                        foreach (var avoidCoord in avoidCoords)
                        {
                            _astar.SetPointSolid(avoidCoord, true);

                            foreach (Direction8 dir in Enum.GetValues<Direction8>())
                            {
                                var otherCoord = avoidCoord.StepDir(dir);

                                if (!island1.Coords.TouchesCoord4(otherCoord) && !island2.Coords.TouchesCoord4(otherCoord))
                                    _astar.SetPointSolid(otherCoord, true);
                            }
                        }
                    }
                    break;

                default:
                    Debug.Fail();
                    break;
            }
        }
    }
}
