using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.ComponentModel;
using Unity.VisualScripting;
using System.Collections.Generic;

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
    private AmazonDynamoDBClient dynamoDBClient;
    [Header("AWS Credentials"), Category("Basic AWS Credentials")]

    public string accessKey;
    public string secretAccessKey;

    [Header("AWS DynamoDB"), Category("AmazonDynamoDB")]

    public string tableName;

    [Header("Unit Testing Booleans")]

    public bool isSignedUp = false;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        try
        {
            var creds = new BasicAWSCredentials(accessKey, secretAccessKey);
            var cognito = new AmazonCognitoIdentityProviderConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            var s3config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            var dynamo = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            provider = new AmazonCognitoIdentityProviderClient(creds, cognito);
            s3Client = new AmazonS3Client(creds, s3config);
            dynamoDBClient = new AmazonDynamoDBClient(creds, dynamo);

            Debug.Log("[CognitoAuthService] Awake initialized successfully!");
        }
        catch(Exception e)
        {
            Debug.LogError($"Awake initialization error! {e.Message}");
        }
        
    }

    public async Task SignUp(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            Debug.LogError("[Cognito] SignUp failed: clientId is null/empty.");
            return;
        }
        if (provider == null)
        {
            Debug.LogError("[Cognito] SignUp failed: provider is null (Awake() likely failed).");
            return;
        }
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Debug.LogError("[Cognito] SignUp failed: email/password missing.");
            return;
        }

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

        clientEmail = email;
        newPassword = password;

        try
        {
            isSignedUp = true;
            var response = await provider.SignUpAsync(request);
            Debug.Log($"Sign up successful! Response Required: {response.UserConfirmed}");
        }
        catch
        {
            isSignedUp = false;
            Debug.Log("Sign up failed!");
        }
    }

    public async Task SignIn(string email, string password, string newPassword = null)
    {
        var request = new InitiateAuthRequest
        {
            ClientId = clientId,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            }
        };

        newPassword = password;
        
        Debug.Log(request.IsUnityNull()); // false it is
        request.AuthParameters = new Dictionary<string, string>();

        request.AuthParameters.Add("USERNAME", email); // passing the email value to the AWS server
        request.AuthParameters.Add("PASSWORD", password); // passing the password value to the AWS server

        try
        {
            var response = await provider.InitiateAuthAsync(request);

            if(response.AuthenticationResult != null)
            {
                Debug.Log($"Sign In Successful! RESULT: {response.AuthenticationResult} HTTPS CODE: {response.HttpStatusCode}");
                await AddUser(tableName, response.AuthenticationResult.IdToken, email, true);
                return;
            }

            Debug.LogError($"Sign-in did not fetch any tokens! Challenge faced: {response.ChallengeName}");

            if(response.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
            {
                var challengeRequest = new RespondToAuthChallengeRequest
                {
                    ClientId = clientId,
                    ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                    Session = response.Session,
                    ChallengeResponses = new Dictionary<string, string>
                    {
                        { "USERNAME", email },
                        { "NEW_PASSWORD", newPassword }
                    }
                };

                var challengeResponse = await provider.RespondToAuthChallengeAsync(challengeRequest);
                if(challengeResponse.AuthenticationResult != null)
                {
                    Debug.Log($"Sign In Successful! RESULT: {response.AuthenticationResult} HTTPS CODE: {response.HttpStatusCode}");
                    return;
                }

                else
                {
                    Debug.Log($"Unidentified Challenge! {response.ChallengeName}");
                }
            }
        }

        catch(Exception e)
        {
            Debug.LogError($"Sign-In Error: {e.Message}");
        }
    }

    public async Task ConfirmSignUp(string email, string accessCode)
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

        catch(Exception e)
        {
            Debug.Log($"Confirm sign up failed! {e.Message}");
        }
    }

    public async Task ResendOTP(string email)
    {
        try
        {
            var request = new ResendConfirmationCodeRequest
            {
                ClientId = clientId,
                Username = email
            };

            var response = await provider.ResendConfirmationCodeAsync(request);
            var delivery = response.CodeDeliveryDetails;

            Debug.Log($"Sent an OTP confirmation to {delivery.Destination} via {delivery.DeliveryMedium}");
        }

        catch (Exception e)
        {
            Debug.LogError($"Resend OTP failed: {e.Message}");
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
            UIManager.instance.OpenPage(UIPageTypes.ConfirmOTPForgetPassword);
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

    public async Task AddUser(string tableName, string userID, string userMail, bool isOnline)
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "UserID", new AttributeValue { S = userID } },
                    { "UserMail", new AttributeValue { S = userMail } },
                    { "LastOnline", new AttributeValue { BOOL = isOnline } },
                    { "LastOnlineTime", new AttributeValue{ S = DateTime.UtcNow.ToString() } },
                }
            };

            await dynamoDBClient.PutItemAsync(request);
        }

        catch(Exception e)
        {
            Debug.LogError($"Error inserting item in {tableName} table. {e.Message}");
        }
    }


    public async Task ConfirmForgetPassword(string email, string code, string password)
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
            UIManager.instance.OpenPage(UIPageTypes.SignIn);
            Debug.Log($"Successful confirm forget password! STATUS: STATUS: {response.HttpStatusCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Confirm forget password failed: {e.Message}");
        }
    }
}