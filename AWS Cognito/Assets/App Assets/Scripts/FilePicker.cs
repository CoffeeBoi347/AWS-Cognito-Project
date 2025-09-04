using UnityEngine;
using SFB;
using System.IO;
using UnityEngine.UI;

public class FilePicker : MonoBehaviour
{
    public static FilePicker _instance;
    public bool hasLoadImage;
    public RawImage rawImage;

    [SerializeField] private GameObject notificationPanel;

    #region Singleton
    public static FilePicker Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<FilePicker>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Load File
    public void LoadFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "jpg", false);
        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            GlobalChatManager.Instance.messageType = MessageType.Image;
            hasLoadImage = true;

            Debug.Log($"Selected File: {paths[0]}");

            string filePath = paths[0];
            byte[] fileData = File.ReadAllBytes(filePath);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool texLoadImage = tex.LoadImage(fileData);

            if (!texLoadImage)
            {
                Debug.LogError("Failed to load image data into texture.");
                return;
            }

            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            rawImage.texture = tex;
            //rawImage.SetNativeSize();

            OpenNotificationPanel();    
        }
    }
    #endregion

    #region Notification Panels Manager
    public void OpenNotificationPanel()
    {
        var seq = LeanTween.sequence();

        seq.append(() => notificationPanel.SetActive(true));
        seq.append(LeanTween.scale(notificationPanel, Vector3.one, 0.1f).setEaseInBounce());
        seq.append(0.8f);
        seq.append(LeanTween.scale(notificationPanel, Vector3.zero, 0.1f).setEaseOutBounce());
        seq.append(() => notificationPanel.SetActive(false));
    }

    public void CloseNotificationsPanel()
    {
        notificationPanel.SetActive(false);
    }
    #endregion
}
