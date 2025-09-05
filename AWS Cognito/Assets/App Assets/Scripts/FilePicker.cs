using UnityEngine;
using SFB;
using System.IO;
using UnityEngine.UI;
using Photon.Pun;
public class FilePicker : MonoBehaviourPunCallbacks
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
            FileInfo fileInfo = new FileInfo(paths[0]);
            if (fileInfo.Length > 30 * 1024) // 100 KB
            {
                Debug.LogError("Image too large. Max 100KB allowed.");
                return;
            }

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
        seq.append(LeanTween.scale(notificationPanel, Vector3.one, 0.5f).setEaseInBounce());
        seq.append(1.5f);
        seq.append(LeanTween.scale(notificationPanel, Vector3.zero, 0.5f).setEaseOutBounce());
        seq.append(() => notificationPanel.SetActive(false));
    }

    public void CloseNotificationsPanel()
    {
        notificationPanel.SetActive(false);
    }

    public void ResetMessageImage()
    {
        if(GlobalChatManager.Instance.messageType == MessageType.Image && Input.GetKeyDown(KeyCode.Return))
        {
            GlobalChatManager.Instance.messageType = MessageType.Text;
            hasLoadImage = false;
            rawImage.texture = null;
        }
    }
    #endregion
}
