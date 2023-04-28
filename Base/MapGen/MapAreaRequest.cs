public class MapAreaRequest
{
	public int MinWidth { get; }
	public int MaxWidth { get; }

	public int MinHeight { get; }
    public int MaxHeight { get; }

    public bool CenteredX { get; }
    public bool CenteredY { get; }

	public int Iterations { get; } = 20;

    public MapAreaRequest(
        int minWidth,
        int maxWidth,
        int minHeight,
        int maxHeight,
        bool centeredX = false,
        bool centeredY = false)
    {
		Debug.Assert(minWidth > 0);
		Debug.Assert(minWidth <= MaxWidth);

		Debug.Assert(minHeight > 0);
		Debug.Assert(minHeight <= maxHeight);

        MinWidth = minWidth;
        MaxWidth = maxWidth;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
        CenteredX = centeredX;
        CenteredY = centeredY;
    }
}
