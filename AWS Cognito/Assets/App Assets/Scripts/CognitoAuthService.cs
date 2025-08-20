using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.ComponentModel;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.IO;

public class CognitoAuthService : MonoBehaviour
{
    public static CognitoAuthService instance;
    public string messageToSend;

    [Header("Core Operations"), Category("AWS Core")]

    public string region;
    public string userPoolId;
    public string clientId;
    public string bucketName;

    [Header("Identification And Authentication"), Category("AWS Cognito")]
    public string identityCode;
    public string newPassword;
    public string clientEmail;

    [Header("AWS Providers")]
    private AmazonCognitoIdentityProviderClient provider;
    private AmazonS3Client s3Client;

    [Header("AWS Credentials"), Category("Basic AWS Credentials")]

    public string accessKey;
    public string secretAccessKey;
    private void Awake()
    {
        instance = this; // Dont destroy it on load
        var creds = new BasicAWSCredentials(accessKey, secretAccessKey);
        var cognito = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        var s3config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        provider = new AmazonCognitoIdentityProviderClient(creds, cognito);
        s3Client = new AmazonS3Client(creds, s3config);
    }

    public async Task SignUp(string email, string password)
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
        //  await ConfirmSignUp(email);
            var response = await provider.SignUpAsync(request);
            Debug.Log($"Sign up successful! Response Required: {response.UserConfirmed}");
        }
        catch
        {
            Debug.Log("Sign up failed!");
        }
    }

    public async Task SignIn(string email, string password)
    {
        var request = new InitiateAuthRequest
        {
            ClientId = clientId,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
        };

        Debug.Log(request.IsUnityNull()); // false it is
        request.AuthParameters = new Dictionary<string, string>();

        request.AuthParameters.Add("USERNAME", email); // passing the email value to the AWS server
        request.AuthParameters.Add("PASSWORD", password); // passing the password value to the AWS server

        try
        {
            var response = await provider.InitiateAuthAsync(request);
            Debug.Log($"Sign in successful! {response.AuthenticationResult.AccessToken}");
            await StoreS3(bucketName, messageToSend);
        }

        catch(Exception ex)
        {
            Debug.Log($"Sign in failed! {ex.Message}");
        }

    }

    private async Task ConfirmSignUp(string email, string accessCode)
    {
        var request = new ConfirmSignUpRequest
        {
            ClientId = clientId,
            ConfirmationCode = accessCode,
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

    public async Task ForgotPassword(string email)
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

    private async Task StoreS3(string bucketName, string contentToPost)
    {
        string key = $"users/{Guid.NewGuid()}.txt";

        var s3request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = $"users/{Guid.NewGuid()}.txt",
            ContentType = "text/plain",
            ContentBody = contentToPost
        };


        try
        {
            PutObjectResponse response = await s3Client.PutObjectAsync(s3request);
            Debug.Log("Stored data in S3 successfully!");
        }

        catch(Exception ex)
        {
            Debug.Log($"Store S3 - Error! | {ex.Message}");
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
            Debug.Log($"Successful confirm forget password! STATUS: STATUS: {response.HttpStatusCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Confirm forget password failed: {e.Message}");
        }
    }
}