using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using System.Threading.Tasks;
using UnityEngine;

public class CognitoAuthService : MonoBehaviour
{
    public string region;
    public string userPoolId;
    public string clientId;

    private AmazonCognitoIdentityProviderClient provider;

    private void Awake()
    {
        var cognito = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region),
        };

        provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), cognito);
    }

    private async void Start()
    {
        await SignUp("testuser@example.com", "Password123!");
    }

    private async Task SignUp(string email, string password)
    {
        var request = new SignUpRequest
        {
            ClientId = clientId,
            Username = email,
            Password = password
        };

        request.UserAttributes.Add(new AttributeType
        {
            Name = "email",
            Value = email
        });

        try
        {
            var response = await provider.SignUpAsync(request);
            Debug.Log($"Sign up successful! Response Required: {response.UserConfirmed}");
        }
        catch
        {
            Debug.Log("Sign up failed!");
        }
    }

    private async Task SignIn(string email, string password)
    {
        var request = new InitiateAuthRequest
        {
            ClientId = clientId,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
        };

        request.AuthParameters.Add("USERNAME", email); // passing the email value to the AWS server
        request.AuthParameters.Add("PASSWORD", password); // passing the password value to the AWS server

        try
        {
            var response = await provider.InitiateAuthAsync(request);
            Debug.Log($"Sign in successful! {response.AuthenticationResult.AccessToken}");
        }

        catch
        {
            Debug.Log("Sign in failed!");
        }

    }
}