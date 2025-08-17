using UnityEngine;

public class UIPageType : MonoBehaviour
{
    public UIPageTypes pageType;
}

public enum UIPageTypes
{
    Home,
    SignIn,
    SignUp,
    ConfirmOTPSignUp,
    ForgetPassword,
    ConfirmOTPForgetPassword
}