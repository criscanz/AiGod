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
    using System.IO;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;
    public class AiGodModSystem : ModSystem
    {
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
            //Make a new server thread with IServerApi.AddServerThread() to keep this script from making the server have atomic shit while it waits to run the python script
            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                           $"God Says: {GenAiPython(message)}",
                           EnumChatType.Notification);
        }
        private String GenAiPython(string message)
        {
            string pythonScriptPath = Path.Combine(Environment.CurrentDirectory, "genai_god.py");//INPORTANT - FIX THIS SHITTY WAY, RN YOU HAVE TO PUT THE PYTHON SCRIPT IN THE ROOT FOLDER OF THE SERVER, FIX THIS METHOD LATER, MAYBE MAKE IT A CONFIG OPTION OR SOMETHING
            string arguments = "\""+message+"\""; // the message contents - RLY INPORTANT THAT IT HAS QUOTES, DONT REMOVE THEM

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python", // or "python3" python works on windows, if it doesnt, try python3 or reinstall python
                Arguments = $"\"{pythonScriptPath}\" {arguments} {1}",
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
                    return output + error;
                }
                else
                {
                    return output; // hopefully doesnt break :)
                }
            }
        }
    }
}
