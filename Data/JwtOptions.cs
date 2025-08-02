// путь: /Data/JwtOptions.cs  (или /Configuration/JwtOptions.cs)
namespace EventHub.Data   // или EventHub.Configuration, как вам удобнее
{
    public class JwtOptions
    {
        // Здесь имена свойств должны совпадать с ключами в секции "Jwt" вашего JSON
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpireMinutes { get; set; }  // если вы его добавили в appsettings.Development.json
    }
}
