using UnityEngine;

public class UIPageType : MonoBehaviour
{
    public UIPageTypes pageType;
}

public enum UIPageTypes
{
    None,
    Home,
    SignIn,
    SignUp,
    ConfirmOTPSignUp,
    ForgetPassword,
    ConfirmOTPForgetPassword,
    GlobalChat,
    CreateNickName
}