using System.Net;
using System.Security.Cryptography;
using System.Text;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Diploma1.IdentityService.Entities;
using Diploma1.IdentityService.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Diploma1.IdentityService.Services;

public class AuthService(IOptions<AwsSettings> awsSettings, IAmazonCognitoIdentityProvider cognito, IdentityServiceDbContext dbContext) : IAuthService
{
    public async Task<AuthenticationResultType> Login(LoginRequestModel loginRequestModel)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = awsSettings.Value.AppClientId,
            AuthParameters = new Dictionary<string, string>
            {
                {"USERNAME", loginRequestModel.Email},
                {"PASSWORD", loginRequestModel.Password},
                {"SECRET_HASH", CalculateHash(loginRequestModel.Email, awsSettings.Value.AppClientId, awsSettings.Value.AppSecretKey)}
            }
        };
        var response = await cognito.InitiateAuthAsync(authRequest);
        return response.AuthenticationResult;
    }

    public async Task<bool> Register(RegistrerRequestModel registerRequestModel)
    {
        var singUpRequest = new SignUpRequest
        {
            Username = registerRequestModel.Email,
            Password = registerRequestModel.Password,
            ClientId = awsSettings.Value.AppClientId,
            SecretHash = CalculateHash(registerRequestModel.Email, awsSettings.Value.AppClientId, awsSettings.Value.AppSecretKey),
            UserAttributes = new List<AttributeType>
            {
                new AttributeType {Name = "given_name", Value = registerRequestModel.FirstName},
                new AttributeType {Name = "family_name", Value = registerRequestModel.LastName}
            }
        };
        var response = await cognito.SignUpAsync(singUpRequest);
        if (response.HttpStatusCode != HttpStatusCode.OK)
            return false;
        var userEntity = MapUser(response.UserSub, registerRequestModel);
        await dbContext.AddAsync(userEntity);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmConfirmationCode(ConfirmConfirmationCodeRequestModel requestModel)
    {
        var confirmSignUpRequest = new ConfirmSignUpRequest
        {
            ClientId = awsSettings.Value.AppClientId,
            Username = requestModel.UserId,
            ConfirmationCode = requestModel.Code,
            SecretHash = CalculateHash(requestModel.UserId, awsSettings.Value.AppClientId, awsSettings.Value.AppSecretKey)
        };

        var response = await cognito.ConfirmSignUpAsync(confirmSignUpRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    private static UserEntity MapUser(string cognitoUserName, RegistrerRequestModel requestModel)
    {
        return new UserEntity
        {
            FirstName = requestModel.FirstName,
            LastName = requestModel.LastName,
            CognitoId = cognitoUserName,
            Email = requestModel.Email,
        };
    }

    private static string CalculateHash(string email, string clientId, string clientSecret)
    {
        var message = email + clientId;
        var encodedMessage = Encoding.UTF8.GetBytes(message);
        var encodedSecret = Encoding.UTF8.GetBytes(clientSecret);
        using var hmac = new HMACSHA256(encodedSecret);
        var result = hmac.ComputeHash(encodedMessage);
        return Convert.ToBase64String(result);
    }
}