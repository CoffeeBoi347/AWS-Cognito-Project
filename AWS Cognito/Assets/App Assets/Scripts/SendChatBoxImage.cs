using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SendChatBoxImage : MonoBehaviourPun
{
    [SerializeField] private TMP_Text usernameField;
    [SerializeField] private RawImage userImage;
    [SerializeField] private TMP_Text messageField;

    [PunRPC]
    public void SendMessageImage(string username, string message, Texture2D image)
    {
        usernameField.text = username;
        messageField.text = message;
        userImage.texture = image;
        userImage.SetNativeSize();
    }
}