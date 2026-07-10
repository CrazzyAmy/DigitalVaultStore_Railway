namespace DigitalProject.Security
{
    public interface ITokenBlacklistService
    {
        void Blacklist(string token, DateTime expiry);
        bool IsBlacklisted(string token);
    }
}
