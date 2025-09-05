using Photon.Pun;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalChatManager : Singleton<GlobalChatManager>
{
    [Header("User Fields")]
    public TMP_InputField userMessageInpField;
    public TMP_Text userOnlineField;

    private string messageSend;
    private string userName;
    public string textBoxObjName;
    public string textBoxObjImgName;

    [Header("Message Type")]
    
    public MessageType messageType = MessageType.Text;

    [Header("Message Holders")]
    public Transform userMessagesHolder;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendMessageGC();
        }

        FilePicker.Instance.ResetMessageImage();
    }

    public async void SendMessageGC()
    {
        if (!PhotonNetwork.InRoom) return;

        messageSend = userMessageInpField.text;
        userName = PhotonNetwork.NickName;

        switch (messageType)
        {
            case MessageType.Image:
                Texture2D tex = FilePicker.Instance.rawImage.texture as Texture2D;
                if (tex == null)
                {
                    Debug.LogError("RawImage.texture is not a Texture2D!");
                }
                byte[] bytes = tex.EncodeToPNG();

                photonView.RPC("ReceiveMessageImage", RpcTarget.All, userName, messageSend, bytes);
                break;
            case MessageType.Text:
                photonView.RPC("ReceiveMessage", RpcTarget.All, userName, messageSend);
                break;

        }
        await CognitoAuthService.instance.StoreMessage(
            CognitoAuthService.instance.messageHolderTableName,
            CognitoAuthService.instance.identityToken,
            userMessageInpField.text,
            System.DateTime.UtcNow.ToString()
        );

        userMessageInpField.text = string.Empty;
    }

    [PunRPC]
    private void ReceiveMessageImage(string senderName, string messageToSend, byte[] bytes)
    {
        GameObject textBoxObj = PhotonNetwork.Instantiate(textBoxObjImgName, Vector3.zero, Quaternion.identity);
        textBoxObj.transform.SetParent(userMessagesHolder, false);
        textBoxObj.GetPhotonView().RPC("SendMessageImage", RpcTarget.All, senderName, messageToSend, bytes);
    }

    [PunRPC]
    private void ReceiveMessage(string senderName, string messageToSend)
    {
        GameObject textBoxObj = PhotonNetwork.Instantiate(textBoxObjName, Vector3.zero, Quaternion.identity);
        textBoxObj.transform.SetParent(userMessagesHolder, false);

        textBoxObj.GetComponent<SendChatBox>().SendMessage(senderName, messageToSend);
        textBoxObj.GetPhotonView().RPC("SendMessage", RpcTarget.All, senderName, messageToSend);
    }

    public void UserOnlineField()
    {
        userOnlineField.text = $"Connected Users: {PhotonNetwork.CountOfPlayersInRooms + 1}";
    }
}

public enum MessageType
{
    Text,
    Image
}