using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Diploma1.IdentityService.Settings;

public class JwtTokenSettingsProvider(IConfiguration configuration)
{
    private TokenValidationParameters? _tokenValidationParameters;
    public TokenValidationParameters GetCognitoTokenValidationParams()
    {
        if (_tokenValidationParameters is not null)
        {
            return _tokenValidationParameters;
        }
        var cognitoIssuer = $"https://cognito-idp.{configuration["AWS:Region"]}.amazonaws.com/{configuration["AWS:UserPoolId"]}"; 
        var jwtKeySetUrl = $"{cognitoIssuer}/.well-known/jwks.json";
        
        var tokenValidationParams = new TokenValidationParameters
        {
            IssuerSigningKeys = GetCognitoSigningKeys(jwtKeySetUrl),
            ValidAudience = configuration["AWS:AppClientId"],
            ValidIssuer = cognitoIssuer,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
        };
        _tokenValidationParameters = tokenValidationParams;
        return tokenValidationParams;
    }

    private IEnumerable<SecurityKey> GetCognitoSigningKeys(string jwtKeySetUrl)
    {
        var json = new HttpClient().GetStringAsync(jwtKeySetUrl).Result;
        var keys = JsonSerializer.Deserialize<JsonWebKeySet>(json)?.Keys;
        return (IEnumerable<SecurityKey>)keys ?? throw new NullReferenceException();
    }
}