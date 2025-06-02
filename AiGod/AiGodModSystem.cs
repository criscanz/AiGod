using System.Diagnostics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Server;
using Vintagestory.GameContent;
namespace AiGod
{
    using System.Diagnostics;
    using System.IO;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;
    using Vintagestory.Common;

    public class AiGodModSystem : ModSystem
    {

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
            byte[] playerMoodByte = byPlayer.WorldData.GetModdata("mood");
            int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
            string aiMessage = GenAiPython(message, playerMood);
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
                stack.SetFrom(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack);
                stack.StackSize = 1;
                sapi.World.SpawnItemEntity(stack, byPlayer.Entity.Pos.XYZ.Add(0, 1, 0));
                // duplicates whatever is held in hand (can be very risky, so be careful)  
            }
            else if (aiMessage.Contains("KHAAA")){byte[] playerMoodByte = byPlayer.GetModdata("mood"); // set mood down by one
                int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                playerMood--;
                byte[] moodBytes = BitConverter.GetBytes(playerMood);
                byPlayer.SetModdata("mood", moodBytes);
            }
            else if (aiMessage.Contains("KHHHH")){ byte[] playerMoodByte = byPlayer.GetModdata("mood"); // set mood up by one
                int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                playerMood++;
                byte[] moodBytes = BitConverter.GetBytes(playerMood);
                byPlayer.SetModdata("mood", moodBytes);
            }
            else if (aiMessage.Contains("KHTSS")) {
                //replace this with a thunderstorm ability
            }
            else if (aiMessage.Contains("KHPTS")) {//sets temporal stability to 0, preferably in the future, set player's temporal
                double value = 0.0;
                ((TreeAttribute)byPlayer.Entity.WatchedAttributes).SetDouble("temporalStability", value);
            }
            else if (aiMessage.Contains("KHTSD")) {//sets time to morning (skips ahead in time, never goes backward)(wait, how do you spell backward)
                while (sapi.World.Calendar.FullHourOfDay < 5)
                {
                    sapi.World.Calendar.CalendarSpeedMul = 1000.0f;
                }
                sapi.World.Calendar.CalendarSpeedMul = 0.5f;
            }
            else if (aiMessage.Contains("KHTSN")) {//sets time to night, works the same as setting the time to day.
                while (sapi.World.Calendar.FullHourOfDay < 19)
                {
                    sapi.World.Calendar.CalendarSpeedMul = 1000.0f;
                }
                sapi.World.Calendar.CalendarSpeedMul = 0.5f;
            }

            
        }//Commands to add: smite, set temporal stability to 0, 

        private String GenAiPython(string message,int mood)
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
