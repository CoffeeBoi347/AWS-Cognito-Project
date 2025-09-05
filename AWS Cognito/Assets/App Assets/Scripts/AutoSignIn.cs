using UnityEngine;

public class AutoSignIn : MonoBehaviour
{
    #region Singleton
    public static AutoSignIn instance;
    public AutoSignInState autoSignInState = AutoSignInState.False;
    public static AutoSignIn Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindFirstObjectByType<AutoSignIn>();
                if(instance == null)
                {
                    Debug.LogError("Auto Sign In is not initialized! Please initialize it first.");
                }
            }
            return instance;
        }
    }
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        autoSignInState = AutoSignInState.False;
    }

    private void Start()
    {
        PerformSignIn();
    }
    #endregion

    public void PerformSignIn()
    {
        string userEmail = PlayerPrefs.GetString("UserEmail");
        string userPass = PlayerPrefs.GetString("UserPassword");

        if(!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userPass))
        {
            _ = CognitoAuthService.instance.SignIn(userEmail, userPass);
            autoSignInState = AutoSignInState.True;
            return;
        }

        else
        {
            Debug.Log("No user details found for auto sign in.");
            return;
        }
    }


    public void StoreUserDetails(string userEmail, string userPassword)
    {
        PlayerPrefs.SetString("UserEmail", userEmail);
        PlayerPrefs.SetString("UserPassword", userPassword);
        PlayerPrefs.Save();
    }
}

public enum AutoSignInState
{
    True,
    False
}
