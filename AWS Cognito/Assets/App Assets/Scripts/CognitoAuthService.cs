using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class CognitoAuthService : MonoBehaviour
{
    public string region;
    public string userPoolId;
    public string clientId;

    public string identityCode;
    public string newPassword;
    public string clientEmail;
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
        await SignIn(clientEmail, newPassword);
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

        catch(Exception ex)
        {
            Debug.Log($"Sign in failed! {ex.Message}");
        }

    }

    private async Task ConfirmSignUp(string code, string email)
    {
        var request = new ConfirmSignUpRequest
        {
            ClientId = clientId,
            ConfirmationCode = code,
            Username = email
        };

        try
        {
            var response = await provider.ConfirmSignUpAsync(request);
            Debug.Log($"Sign up successful! STATUS: {response.HttpStatusCode}");
        }

        catch
        {
            Debug.Log("Confirm sign up failed!");
        }
    }

    private async Task ForgotPassword(string email)
    {
        ForgotPasswordRequest request = new ForgotPasswordRequest
        {
            ClientId = clientId,
            Username = email
        };

        try
        {
            var response = await provider.ForgotPasswordAsync(request);
            Debug.Log($"Successful forgot password! STATUS: {response.HttpStatusCode}");
        }
        catch (Exception ex) 
        {
            Debug.Log($"Forgot password failed! {ex.Message}");
        }
    }

    private async Task ConfirmForgetPassword(string email, string code, string password)
    {
        ConfirmForgotPasswordRequest request = new ConfirmForgotPasswordRequest
        {
            ClientId = clientId,
            Username = email,
            ConfirmationCode = code,
            Password = password
        };

        try
        {
            var response = await provider.ConfirmForgotPasswordAsync(request);
            Debug.Log($"Successful confirm forget passwor! STATUS: STATUS: {response.HttpStatusCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Confirm forget password failed: {e.Message}");
        }
    }
}