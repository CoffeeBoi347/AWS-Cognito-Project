using System.ComponentModel;
using TMPro;
using UnityEngine;

public class UIPageHandler : MonoBehaviour
{
    #region Singleton
    private static UIPageHandler instance;

    [Header("Input Streams - Sign In"), Category("Sign In")]

    public TMP_InputField signInInpField;
    public TMP_InputField passwordInpField;

    [Header("Input Streams - Sign Up"), Category("Sign Up")]

    public TMP_InputField signUpInpField;
    public TMP_InputField signUpPasswordField;

    [Header("Input Streams - Forget Password"), Category("Forget Password")]

    public TMP_InputField forgetPasswordInpField;

    private void Awake()
    {
        instance = this;
        // Dont destroy on load
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
        UIManager.instance.OpenPage(UIPageTypes.SignUp);
    }

    public void OpenForgetPasswordPage()
    {
        UIManager.instance.OpenPage(UIPageTypes.ForgetPassword);
    }
    #endregion

    public async void ConfirmSignIn()
    {
        if (signInInpField == null || passwordInpField == null)
        {
            Debug.LogError("Input fields are not assigned in Inspector!");
            return;
        }

        if (CognitoAuthService.instance == null)
        {
            Debug.LogError("CognitoAuthService.instance is NULL!");
            return;
        }

        try
        {
            await CognitoAuthService.instance.SignIn(
                signInInpField.text, passwordInpField.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Couldn't sign in! {e.Message}");
        }
    }


    public async void ConfirmSignUp()
    {
        try
        {
            await CognitoAuthService.instance.SignUp(signUpInpField.text, signUpPasswordField.text);
        }

        catch(System.Exception e)
        {
            Debug.LogError($"Couldn't sign up! {e.Message}");
        }
    }

    public async void ConfirmForgetPassword()
    {
        try
        {
            await CognitoAuthService.instance.ForgotPassword(forgetPasswordInpField.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Forget password error! {e.Message}");
        }
    }
}