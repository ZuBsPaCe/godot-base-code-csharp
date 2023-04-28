public enum MapConnectStrategy
{
    // Always tries to connect the closest islands. Can be star-like. Connects all islands. Usually has dead ends.
    Minimal,

    // Starts from the outermost island and tries to connect to the closest island. Can be snake-like. Connects all islands. Usually has dead ends.
    Linear,

    // Always tries to create a circular path, passing all islands. Connects all islands. Rarely has dead ends.
    Circular,

    // Tries to connect each island to the closest x islands. Can happen, that not all islands are connected!
    // Useful, if you need lots of connected rooms or passages after eroding walls.
    Closest1,
    Closest2,
    Closest3,
    Closest4,

    // Randomly connects rooms. Adjust random_connection_* settings and avoidance in MapConnectRequest. Usually does not connect all islands!
    // Useful AFTER having connected all rooms, with THE SAME islands used before to create alternative paths.
    Random
}

