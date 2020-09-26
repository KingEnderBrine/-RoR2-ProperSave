using R2API.Utils;
using RoR2;

namespace ProperSave
{
    internal class Commands
    {
        private const string LoadLobbyCommand = "ps_load_lobby";
        internal static void RegisterCommands()
        {
            CommandHelper.AddToConsoleWhenReady();
        }

        internal static void UnregisterCommands()
        {
            RoR2.Console.instance.concommandCatalog.Remove(LoadLobbyCommand);
        }

        [ConCommand(commandName = LoadLobbyCommand, flags = ConVarFlags.None, helpText = "Load saved game suitable for current lobby")]
        private static void CCRequestLoadLobby(ConCommandArgs args)
        {
            if (Run.instance != null)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Can't load while run is active");
                return;
            }
            if (Loading.IsLoading)
            {
                ProperSavePlugin.InstanceLogger.LogInfo("Already loading");
                return;
            }
            ProperSavePlugin.Instance.StartCoroutine(Loading.LoadLobby());
        }
    }
}
