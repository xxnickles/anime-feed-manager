namespace AnimeFeedManager.Shared.Types;

public record UserRole
{
    public const string UserValue = "User";
    public const string AdminValue = "Admin";
    private const string NoneValue = "None";
    
    private readonly string _value;

    private UserRole(string value)
    {
        _value = value;
    }
    
    public static implicit operator string(UserRole role) => role._value;

    public override string ToString()
    {
        return _value;
    }
    
    public bool IsAdmin() => _value == AdminValue;
    
    public bool NoRole() => _value == NoneValue;

    
    
    public static UserRole Admin() => new(AdminValue);
    public static UserRole User() => new(UserValue);
    public static UserRole None() => new(NoneValue);
    
    public static UserRole FromString(string value) => value switch
    {
        AdminValue => Admin(),
        UserValue => User(),
        _ => None()
    };
}