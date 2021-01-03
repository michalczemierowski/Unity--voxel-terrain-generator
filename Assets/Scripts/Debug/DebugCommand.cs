/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.DebugUtils
{
    public class DebugCommand
    {
        private const string DEFAULT_SYNTAX_ERROR_MSG = "The command syntax is incorrect";
        private const string DEFAULT_ARG_ERROR_MSG = "The argument syntax is incorrect";
        private const string DEFAULT_EXECUTED_MSG = "Command executed successfully";
        private const string DEFAULT_HELP_MSG = "There is no help message for this command";

        public string Command { get; }
        public System.Func<DebugCommand, string[], DebugCommandStatus> ExecuteAction { get; }

        public string SyntaxErrorMsg { get; }
        public string ArgumentErrorMsg { get; }
        public string ExecutedMsg { get; }
        public string HelpMsg { get; }

        public DebugCommand(string command, System.Func<DebugCommand, string[], DebugCommandStatus> executeAction,
         string syntaxErrorMsg = DEFAULT_SYNTAX_ERROR_MSG,
         string argumentErrorMsg = DEFAULT_ARG_ERROR_MSG,
         string executedMsg = DEFAULT_EXECUTED_MSG,
         string helpMsg = DEFAULT_HELP_MSG)
        {
            Command = command;
            ExecuteAction = executeAction;

            SyntaxErrorMsg = syntaxErrorMsg;
            ArgumentErrorMsg = argumentErrorMsg;
            ExecutedMsg = executedMsg;
            HelpMsg = DebugCommandHandler.COMMAND_PREFIX + helpMsg;
        }

        /// <summary>
        /// Check if commands are the same
        /// </summary>
        public bool Compare(string command)
        {
            return Command.ToLower().Equals(command.ToLower());
        }

        /// <summary>
        /// Execute command
        /// </summary>
        public DebugCommandStatus Execute(string[] args)
        {
            if(ExecuteAction == null)
                return DebugCommandStatus.ACTION_IS_NULL_ERROR;

            return ExecuteAction.Invoke(this, args);
        }
    }
}
