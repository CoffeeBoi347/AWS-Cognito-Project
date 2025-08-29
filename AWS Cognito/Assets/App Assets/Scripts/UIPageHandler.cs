using Amazon.CognitoIdentityProvider;
using Photon.Pun;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPageHandler : MonoBehaviour
{
    #region Singleton
    public static UIPageHandler instance;
    public CognitoAuthService cognitoAuthService;

    [Header("Input Streams - Sign In"), Category("Sign In")]

    public TMP_InputField signInInpField;
    public TMP_InputField passwordInpField;

    [Header("Input Streams - Sign Up"), Category("Sign Up")]

    public TMP_InputField signUpInpField;
    public TMP_InputField signUpPasswordField;

    [Header("Input Streams - Forget Password"), Category("Forget Password")]

    public TMP_InputField forgetPasswordInpField;

    [Header("Input Streams - Confirm OTP"), Category("Confirm OTP - Sign Up")]

    public TMP_InputField userEmailText;
    public TMP_InputField confirmOTPFieldText;

    [Header("Input Streams - Confirm Forget Password OTP"), Category("Confirm Forget Password - OTP")]

    public TMP_InputField userEmailField;
    public TMP_InputField userCodeField;
    public TMP_InputField userPasswordField;

    [Header("Input Streams - Create NickName"), Category("Create Nickname")]

    public TMP_InputField createNicknameField;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    #region Basic Page Operations
    public void OpenSignInPage()
    {
        try
        {
            UIManager.instance.OpenPage(UIPageTypes.SignIn);
        }
        catch(System.Exception e)
        {
            Debug.Log($"{e.Message}");
        }
    }

    public void OpenSignUpPage()
    {
        try
        {
            UIManager.instance.OpenPage(UIPageTypes.SignUp);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
    }

    public void OpenForgetPasswordPage()
    {
        try
        {
            UIManager.instance.OpenPage(UIPageTypes.ForgetPassword);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
    }

    public void OpenConfirmOTP()
    {
        UIManager.instance.OpenPage(UIPageTypes.ConfirmOTPSignUp);
    }
    #endregion

    public async void ConfirmSignIn()
    {
        try
        {
            await CognitoAuthService.instance.SignIn(signInInpField.text, passwordInpField.text);
        }

        catch(System.Exception e)
        {
            Debug.LogError($"Couldn't sign in! {e.Message}");
        }
    }

    public async void ConfirmSignUp()
    {
        if (string.IsNullOrEmpty(signUpInpField.text) || string.IsNullOrEmpty(signUpPasswordField.text))
        {
            Debug.LogError("Sign-Up Input Parameters Are NULL.");
            return;
        }

        Debug.Log("Performing sign up!");
        await CognitoAuthService.instance.SignUp(signUpInpField.text, signUpPasswordField.text);

        Debug.Log("[UIPageHandler] SignUp confirmed!");
    }


    public async void ConfirmForgetPassword()
    {
        try
        {
            CognitoAuthService.instance.ForgotPassword(forgetPasswordInpField.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Forget password error! {e.Message}");
        }
    }

    public async void ConfirmOTPForgetPassword()
    {
        try
        {
            CognitoAuthService.instance.ConfirmForgetPassword(userEmailField.text, userCodeField.text, userPasswordField.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Confirm forget password error! {e.Message}");
        }
    }

    public async void ConfirmOTPSignUp()
    {
        try
        {
            CognitoAuthService.instance.ConfirmSignUp(userEmailText.text, confirmOTPFieldText.text);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"OTP Sign Up Error! {e.Message}");
        }
    }

    public void CreateNickName()
    {
        PhotonNetworkSettings.instance.ConnectToServer(createNicknameField.text);
        PhotonNetwork.NickName = createNicknameField.text;
        UIManager.instance.OpenPage(UIPageTypes.GlobalChat);
    }
}