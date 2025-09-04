using Photon.Pun;
using TMPro;
using UnityEngine;

public class GlobalChatManager : Singleton<GlobalChatManager>
{
    [Header("User Fields")]
    public TMP_InputField userMessageInpField;
    public TMP_Text userOnlineField;

    private string messageSend;
    private string userName;
    public string textBoxObjName;

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
    }

    public async void SendMessageGC()
    {
        if (!PhotonNetwork.InRoom) return;

        messageSend = userMessageInpField.text;
        userName = PhotonNetwork.NickName;

        switch (messageType)
        {
            case MessageType.Image:
                //photonView.RPC("ReceiveMessageImage", RpcTarget.All, userName, FilePicker.Instance.rawImage);
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