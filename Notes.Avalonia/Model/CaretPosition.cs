namespace Notes.Avalonia.Model;

public struct CursorPosition
{
    public int ItemIndex { get; set; }
    public int Offset { get; set; }

    public CursorPosition(int itemIndex, int offset)
    {
        ItemIndex = itemIndex;
        Offset = offset;
    }

    public static CursorPosition Start => new(0, 0);
}

public struct SelectionRange
{
    public CursorPosition Start { get; set; }
    public CursorPosition End { get; set; }

    public bool IsEmpty => Start.ItemIndex == End.ItemIndex && Start.Offset == End.Offset;

    public SelectionRange(CursorPosition start, CursorPosition end)
    {
        Start = start;
        End = end;
    }

    public (CursorPosition first, CursorPosition last) Ordered()
    {
        if (Start.ItemIndex < End.ItemIndex ||
            (Start.ItemIndex == End.ItemIndex && Start.Offset <= End.Offset))
            return (Start, End);
        return (End, Start);
    }
}
