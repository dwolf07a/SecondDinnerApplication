using UnityEngine;
using Opencoding.CommandHandlerSystem;
using Unity.VisualScripting;


public class ConsoleCommands : MonoBehaviour
{


    // This will register the command handlers the first time the class is used.
    void Awake()
    {
        CommandHandlers.RegisterCommandHandlers(this);
    }

    void OnDisable()
    {
        CommandHandlers.UnregisterCommandHandlers(this);
    }

    [CommandHandler(Description = "Clear the level list and start the player from the beginning.")]
    private void ResetLevel()
    {

    }
}