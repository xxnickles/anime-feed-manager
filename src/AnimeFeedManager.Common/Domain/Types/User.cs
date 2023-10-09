using Email = AnimeFeedManager.Common.Types.Email;

namespace AnimeFeedManager.Common.Domain.Types
{
    public record User(string Id, Email Email);

    public static class UserRoles 
    {
        public const string Admin = "admin";
    }
}