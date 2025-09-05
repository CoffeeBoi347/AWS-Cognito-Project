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
    public void SendMessageImage(string username, string message, byte[] image)
    {
        usernameField.text = username;
        messageField.text = message;

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(image);
        userImage.texture = tex;
    }
}