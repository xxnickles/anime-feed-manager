namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public readonly struct Box
{
    private const string EmptyBox = "EMPTY";

    private readonly string _boxValue;

    public Box(string boxValue)
    {
        _boxValue = boxValue;
    }

    public bool HasNoTarget() => _boxValue == EmptyBox;

    public override string ToString()
    {
        return _boxValue;
    }

    public static implicit operator string(Box box) => box._boxValue;

    public static Box Empty() => new(EmptyBox);
}
