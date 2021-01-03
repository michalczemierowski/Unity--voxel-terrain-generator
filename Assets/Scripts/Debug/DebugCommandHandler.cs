using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxelTG.Extensions;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.DebugUtils
{
    public class DebugCommandHandler : ToggleableUI
    {
        public const char COMMAND_PREFIX = '/';

        [SerializeField] private TMP_InputField inputField;

        /// <summary>
        /// True if input field is enabled
        /// </summary>
        public override bool IsUIAcive => inputField.gameObject.activeSelf;

        /// <summary>
        /// Array containing all available commands
        /// </summary>
        private DebugCommand[] allCommands;

        private void Start()
        {
            inputField.onSubmit.AddListener(HandleCommands);
            // close ui on deselect
            inputField.onDeselect.AddListener((msg) => this.OneFrameDelay(CloseUI));

            // initialize commands
            InitCommands();
        }

        private string ParseCommand(string msg, out string[] args)
        {
            if (msg == null || msg.Length == 0 || msg[0] != COMMAND_PREFIX)
            {
                args = null;
                return string.Empty;
            }

            string[] result = msg.Split(' ');
            // remove COMMAND_PREFIX
            string cmd = result[0].Remove(0, 1);

            // remove command name from args
            args = result.Skip(1).ToArray();
            return cmd;
        }

        private void HandleCommands(string msg)
        {
            CloseUI();

            string cmd = ParseCommand(msg, out string[] args);
            for (int i = 0; i < allCommands.Length; i++)
            {
                if (allCommands[i].Compare(cmd))
                {
                    if (args != null)
                    {
                        string lowerArg = args[0].ToLower();
                        if (lowerArg.Equals("-h") || lowerArg.Equals("-help"))
                        {
                            CommandHelp(allCommands[i]);
                            break;
                        }
                    }

                    switch (allCommands[i].Execute(args))
                    {
                        case DebugCommandStatus.OK:
                            CommandExecuted(allCommands[i]);
                            break;
                        case DebugCommandStatus.ARGUMENT_ERROR:
                            CommandArgumentError(allCommands[i], args);
                            break;
                        case DebugCommandStatus.SYNTAX_ERROR:
                            CommandSyntaxError(allCommands[i], args);
                            break;
                    }
                    break;
                }
            }
        }

        private void InitCommands()
        {
            allCommands = new DebugCommand[]
            {
                new DebugCommand("get", CommandHandle_GET, helpMsg: "get [item/block type] [amount = 1]"),
                new DebugCommand("clear-inventory", CommandHandle_CLEAR_INVENTORY, helpMsg: "clear-inventory"),
            };
        }

        /// <summary>
        /// Called when there is a problem with command arguments (e.g. command requires 2 args but only 1 is provided)
        /// </summary>
        private void CommandArgumentError(DebugCommand command, string[] args)
        {
            string msg = $"[{command.Command.ToUpper()}] {command.ArgumentErrorMsg}";
            DebugManager.AddDebugMessageStatic(msg);

#if UNITY_ENGINE
            Debug.Log(msg, this);
#endif
        }

        /// <summary>
        /// Called when there is a problem with arguments syntax (e.g. /get qwerty will cause this because there is no such item type)
        /// </summary>
        private void CommandSyntaxError(DebugCommand command, string[] args)
        {
            string msg = $"[{command.Command.ToUpper()}] {command.SyntaxErrorMsg}";
            DebugManager.AddDebugMessageStatic(msg);

#if UNITY_ENGINE
            Debug.Log(msg, this);
#endif
        }

        /// <summary>
        /// Called when command was executed successfully
        /// </summary>
        private void CommandExecuted(DebugCommand command)
        {
            string msg = $"[{command.Command.ToUpper()}] {command.ExecutedMsg}";
            DebugManager.AddDebugMessageStatic(msg);

#if UNITY_ENGINE
            Debug.Log(msg, this);
#endif
        }

        /// <summary>
        /// Called when player wants to see help for command (arg[0] == '-h' v '-help')
        /// </summary>
        private void CommandHelp(DebugCommand command)
        {
            string msg = command.HelpMsg;
            DebugManager.AddDebugMessageStatic(msg);

#if UNITY_ENGINE
            Debug.Log(msg, this);
#endif
        }
        /// <summary>
        /// Command handler for 'GET' command
        /// </summary>
        private DebugCommandStatus CommandHandle_GET(DebugCommand command, string[] args)
        {
            if (args.Length < 1)
                return DebugCommandStatus.ARGUMENT_ERROR;

            int amount = 1;
            // try get amount
            if (args.Length >= 2)
                int.TryParse(args[1], out amount);

            // try parse args and add items to inventory
            if (Enum.TryParse(args[0], true, out BlockType blockType))
                PlayerController.InventorySystem.AddItem(blockType, amount);
            else if (Enum.TryParse(args[0], true, out ItemType itemType))
                PlayerController.InventorySystem.AddItem(itemType, amount);
            else
                return DebugCommandStatus.SYNTAX_ERROR;

            return DebugCommandStatus.OK;
        }

        private DebugCommandStatus CommandHandle_CLEAR_INVENTORY(DebugCommand command, string[] args)
        {
            InventorySlot[] allInventorySlots = PlayerController.InventorySystem.InventorySlots.ToArray();
            foreach (var slot in allInventorySlots)
            {
                PlayerController.InventorySystem.RemoveItem(slot, -1);
            }

            return DebugCommandStatus.OK;
        }

        public override void OpenUI()
        {
            inputField.gameObject.SetActive(true);
            inputField.Select();
            inputField.ActivateInputField();

            UIManager.IsUsingUIInput = true;

            base.OpenUI();
        }

        public override void CloseUI()
        {
            inputField.SetTextWithoutNotify(string.Empty);
            EventSystem.current.SetSelectedGameObject(null);
            inputField.gameObject.SetActive(false);

            UIManager.IsUsingUIInput = false;

            base.CloseUI();
        }
    }
}
