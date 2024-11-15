using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id;
    public string username;

    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;
    private void OnDestroy()
    {
        list.Remove(id);
    }
    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        player.id = id;
        player.username = username;

        player.SendSpawned(); // this only sends to already connected clients

        list.Add(id, player);

        // Send the new player to all other players
        foreach (Player otherPlayer in list.Values)
        {
            if(otherPlayer.id != id)
                otherPlayer.SendSpawned(id);
        }
    }

    #region Messages

    private void SendSpawned()
    { 
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }
    
    private void SendSpawned(ushort clientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), clientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(transform.position);
        return message;
    }


    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort clientId, Message message)
    {
        Spawn(clientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort clientId, Message message)
    {
        if (list.TryGetValue(clientId, out Player player))
        {
            bool[] inputs = message.GetBools(5);
            Vector2 position = message.GetVector2();
            player.Movement.SetInput(inputs, position);
            player.transform.position = position;
        }
    }

    #endregion

}
