using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonNetworkSettings : MonoBehaviourPunCallbacks
{
    [SerializeField] private string roomName;

    #region Singleton
    public static PhotonNetworkSettings instance;

    public PhotonNetworkSettings Instance
    {
        get
        {
            if(instance == null)
            {
                Debug.LogError("Photon Network Settings is not initialized! Please initialize it first.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "GC_Chat";
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    #region Connect To Server
    // summary: We call this function once we have signed in successfully.
    public void ConnectToServer(string userName)
    {
        Debug.Log("ConnectToServer called");
        try
        {
            PhotonNetwork.NickName = userName;
            PhotonNetwork.ConnectUsingSettings();
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Connect To Server Error! {e.Message}");
            return;
        }
    }
    #endregion

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master!");
        base.OnConnectedToMaster();

        PhotonNetwork.JoinOrCreateRoom("GC_Chat", new RoomOptions { MaxPlayers = 16 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{roomName} joined successfully!");
        GlobalChatManager.Instance.UserOnlineField();
        base.OnJoinedRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{roomName} joined error! Return Code: {returnCode}");
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnConnected()
    {
        Debug.Log("Photon Connected Successfully!");
        base.OnConnected();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Photon disconnected.");
        base.OnDisconnected(cause);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log("Master client switched!");
    }

}