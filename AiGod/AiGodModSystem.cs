using System.Diagnostics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Server;

namespace AiGod
{
    using System.Diagnostics;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    public class AiGodModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from God on the server side: " + Lang.Get("aigod:hello"));

            base.StartServerSide(api);

            api.Event.PlayerChat += Event_PlayerChat; ;
            
        }

        private void Event_PlayerChat(IServerPlayer byPlayer, int channelId, ref string message, ref string data, Vintagestory.API.Datastructures.BoolRef consumed)
        {
            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                           $"God Says: {GenAiPython(message)}",
                           EnumChatType.Notification);
        }
        private String GenAiPython(string message)
        {
            string pythonScriptPath = Environment.CurrentDirectory + "/genai_god.py";
            string arguments = message; // Optional arguments for the script

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python3", // or "python3"
                Arguments = $"{pythonScriptPath} {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error: " + error);
                    return error;
                }
                else
                {
                    return output; // or handle the output as needed
                }
            }
        }
    }
}
