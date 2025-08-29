using UnityEngine;
using TMPro;
using Photon.Pun;
public class SendChatBox : MonoBehaviourPun
{
    [SerializeField] private TMP_Text userNameField;
    [SerializeField] private TMP_Text messageField;

    [PunRPC]
    public void SendMessage(string username, string message)
    {
        userNameField.text = username;
        messageField.text = message;
    }
}
