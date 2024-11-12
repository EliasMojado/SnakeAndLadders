using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id;
    public string username;

    private void OnDestroy()
    {
        list.Remove(id);
    }
    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        player.id = id;
        player.username = username;
        list.Add(id, player);
    }


    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort clientId, Message message)
    {
        Spawn(clientId, message.GetString());
    }

}
