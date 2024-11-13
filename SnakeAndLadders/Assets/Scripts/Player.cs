using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id { get; private set; }
    public bool isLocal { get; private set; }

    public string username;

    private void OnDestroy()
    {
        list.Remove(id);
    }

    private void Move(Vector2 position)
    {
        Debug.Log($"Player new position {position}");
        transform.position = new Vector3(position.x, position.y, 0);
    }

    public static void Spawn(ushort id, string username, Vector2 position)
    {
        Player player;
        Vector3 position3 = new Vector3(position.x, position.y, 0);
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position3, Quaternion.identity).GetComponent<Player>();
            player.isLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.isLocal = false;
        }


        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
       if(list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.Move(message.GetVector2());
        }
    }
}
