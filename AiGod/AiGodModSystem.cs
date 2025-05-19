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
    using System.Threading;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;

    public class AiGodModSystem : ModSystem
    {
        private int mood = 0;

        private ICoreServerAPI sapi;
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from God on the server side: " + Lang.Get("aigod:hello"));

            base.StartServerSide(api);
            
            this.sapi = api;

            api.Event.PlayerChat += Event_PlayerChat;
            
        }

        private void Event_PlayerChat(IServerPlayer byPlayer, int channelId, ref string message, ref string data, Vintagestory.API.Datastructures.BoolRef consumed)
        {
            string text = message;
            if (text.Contains("god")) {
                Thread genThread = new Thread(() => msg(byPlayer, text));
                genThread.Start();
            }
        }
        private void msg(IServerPlayer byPlayer, string message)
        {   
            string aiMessage = GenAiPython(message);
            if (aiMessage.Contains(":::"))
            {
                commandInterpreter(byPlayer, aiMessage);
                aiMessage = aiMessage.Substring(0,aiMessage.IndexOf(":::"));
            }
            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                           $"God: {aiMessage}",
                           EnumChatType.Notification);
            
        }

        private void commandInterpreter(IServerPlayer byPlayer, string aiMessage)
        {
            if (aiMessage.Contains("KHBSK"))
            {
                ItemStack stack = new ItemStack();
                stack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;
                stack.StackSize = 1;
                sapi.World.SpawnItemEntity(stack, byPlayer.Entity.Pos.XYZ.Add(0, 1, 0));
                // duplicates whatever is held in hand (can be very risky, so be careful)
            }
            else if (aiMessage.Contains("KHAAA")){mood--;}
            else if (aiMessage.Contains("KHHHH")){mood++;}
        }//Commands to add: smite, set temporal stability to 0, 

        private String GenAiPython(string message)
        {
            /*string pythonScriptPath = Path.Combine(Environment.CurrentDirectory, "genai_god.py");*///INPORTANT - FIX THIS SHITTY WAY, RN YOU HAVE TO PUT THE PYTHON SCRIPT IN THE ROOT FOLDER OF THE SERVER, FIX THIS METHOD LATER, MAYBE MAKE IT A CONFIG OPTION OR SOMETHING
            string pythonScriptPath = "assets\\aigod\\genai_god.py";
            string arguments = "\""+message+"\""; // the message contents - RLY INPORTANT THAT IT HAS QUOTES, DONT REMOVE THEM
            string api_key = "\"AIzaSyBGeT9RGm3lSnH6ll8I4N_w97iJSW_FZGA\"";
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python", // or "python3" python works on windows, if it doesnt, try python3 or reinstall python
                Arguments = $"\"{pythonScriptPath}\" {arguments} {api_key} {mood}",
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
