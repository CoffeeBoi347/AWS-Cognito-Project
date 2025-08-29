using UnityEngine;
using Photon.Pun;
public class Singleton<T> : MonoBehaviourPun where T : MonoBehaviourPun
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if(instance == null)
                {
                    Debug.LogError("Instance not found! Please assign it properly from the inspector.");
                }
            }
            return instance;
        }
    }

    public virtual void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}