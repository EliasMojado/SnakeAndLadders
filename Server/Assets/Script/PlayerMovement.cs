using Riptide;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;

    private bool[] inputs;

    private void Start()
    {
        Initialize();
        inputs = new bool[5];
    }

    public void SetInput(bool[] inputs, Vector2 position)
    {
        this.inputs = inputs;
        SendMovement(position);
    }

    private void SendMovement(Vector2 position)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.id);
        message.AddVector2(position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void Initialize()
    {
        // Initialize the player movement
    }
}
