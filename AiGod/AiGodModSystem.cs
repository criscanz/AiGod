using System;
using Vintagestory.API.Config;
namespace AiGod
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.Server;

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
            if (text.Contains("god"))
            {
                Thread genThread = new Thread(() => msg(byPlayer, text));
                genThread.Start();
            }
        }
        private void msg(IServerPlayer byPlayer, string message)
        {
            byte[] playerMoodByte = byPlayer.WorldData.GetModdata("mood");
            int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
            string aiMessage = GenAiPython(message, playerMood, SacrificeCheck(byPlayer));//need to add sacrifice check ,SacrificeCheck(byPlayer) after playerMood
            if (aiMessage.Contains(":::"))
            {
                commandInterpreter(byPlayer, aiMessage.Substring(aiMessage.IndexOf(":::")));
                aiMessage = aiMessage.Substring(0, aiMessage.IndexOf(":::"));
            }
            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                           $"God: {aiMessage}",
                           EnumChatType.Notification);

        }

        private void commandInterpreter(IServerPlayer byPlayer, string aiMessage)
        {
            Console.WriteLine("Command Interpreter called with message: " + aiMessage);//Im switching this entire system to be a switch statement. It will hopefully work instead.
            
            if (aiMessage.Contains("CBSK"))
            {
                ItemStack stack = new ItemStack();
                stack.SetFrom(byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack);
                stack.StackSize = 1;
                sapi.World.SpawnItemEntity(stack, byPlayer.Entity.Pos.XYZ.Add(0, 1, 0));
                // duplicates whatever is held in hand (can be very risky, so be careful)  
            }
            if (aiMessage.Contains("CAAA"))
            {
                byte[] playerMoodByte = byPlayer.GetModdata("mood"); // set mood down by one
                int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                playerMood--;
                byte[] moodBytes = BitConverter.GetBytes(playerMood);
                byPlayer.SetModdata("mood", moodBytes);
            }
            if (aiMessage.Contains("CHHH"))
            {
                byte[] playerMoodByte = byPlayer.GetModdata("mood"); // set mood up by one
                int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                playerMood++;
                byte[] moodBytes = BitConverter.GetBytes(playerMood);
                byPlayer.SetModdata("mood", moodBytes);
            }
            if (aiMessage.Contains("CTSS"))
            {
                //replace this with a thunderstorm ability
            }
            if (aiMessage.Contains("CPTS"))
            {//sets temporal stability to 0
                double value = 0.0;
                ((TreeAttribute)byPlayer.Entity.WatchedAttributes).SetDouble("temporalStability", value);
            }
            if (aiMessage.Contains("CTIS[")&& aiMessage.Contains("]CTIS"))//I see the problem, else if just stops past this point.
            {//checks for a sacrifice in the player's inventory, if it exists, remove it, else, make god angry
                IInventory hotbar = byPlayer.InventoryManager.GetHotbarInventory();
                bool tookItem = false;

                String GrabCommand = aiMessage.Substring(aiMessage.IndexOf("CTIS["), aiMessage.IndexOf("]CTIS")+1);
                foreach (var item in hotbar)
                {
                    if (!item.Empty && GrabCommand.Contains(item.GetStackName()))
                    {

                        string amountToTake = Regex.Match(GrabCommand, @"\d+").Value;
                        int amountToTakeInt = Int32.Parse(amountToTake);
                        int stackSize = item.Itemstack.StackSize;
                        if (amountToTakeInt > stackSize)
                        {
                            Console.WriteLine("Negative Items Found!");
                            byte[] playerMoodByte2 = byPlayer.GetModdata("mood");
                            int playerMood2 = playerMoodByte2 != null ? BitConverter.ToInt32(playerMoodByte2, 0) : 0;
                            playerMood2 -= 12;
                            byte[] moodBytes2 = BitConverter.GetBytes(playerMood2);
                            byPlayer.SetModdata("mood", moodBytes2);
                            ((TreeAttribute)byPlayer.Entity.WatchedAttributes).SetDouble("temporalStability", 0.0);
                            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                                  $"Don't do that again.",
                                  EnumChatType.CommandError);

                            tookItem = true;
                            item.Itemstack = null;
                            item.MarkDirty();
                        }
                        else if (stackSize > amountToTakeInt)
                        {
                            Console.WriteLine("Taking " + item.GetStackName() + " and taking " + amountToTakeInt + " amount");
                            item.Itemstack.StackSize = stackSize - amountToTakeInt;
                            item.MarkDirty();
                            tookItem = true;
                        }
                        else
                        {
                            Console.WriteLine("Removing item from hotbar: " + item.GetStackName());
                            item.Itemstack = null;
                            item.MarkDirty();
                            tookItem = true;
                        }
                        byte[] playerMoodByte = byPlayer.GetModdata("mood");
                        int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                        playerMood += 2;
                        byte[] moodBytes = BitConverter.GetBytes(playerMood);
                        byPlayer.SetModdata("mood", moodBytes);
                    }
                }
                if (tookItem == false)
                {
                    Console.WriteLine("No item found!");
                    byte[] playerMoodByte = byPlayer.GetModdata("mood");
                    int playerMood = playerMoodByte != null ? BitConverter.ToInt32(playerMoodByte, 0) : 0;
                    playerMood -= 12;
                    byte[] moodBytes = BitConverter.GetBytes(playerMood);
                    byPlayer.SetModdata("mood", moodBytes);
                    ((TreeAttribute)byPlayer.Entity.WatchedAttributes).SetDouble("temporalStability", 0.0);
                    byPlayer.SendMessage(GlobalConstants.GeneralChatGroup,
                          $"Don't do that again.",
                          EnumChatType.CommandError);
                }
            }
        }//Commands to add: smite,

        private static String SacrificeCheck(IServerPlayer byPlayer)
        {
            String potentialSacrifices = "";
            IInventory hotbar = byPlayer.InventoryManager.GetHotbarInventory();
            foreach (var item in hotbar)
            {
                if (!item.Empty)
                {
                    potentialSacrifices = potentialSacrifices + item.Itemstack.StackSize + "*" + item.GetStackName() + ", ";
                }
            }
            if (potentialSacrifices.Length == 1)
            {
                potentialSacrifices = "No sacrifices found in hotbar";
            }

            Console.WriteLine(potentialSacrifices);

            return potentialSacrifices;
        }
        private static String GenAiPython(string message, int mood, String sacrifices)
        {
            /*string pythonScriptPath = Path.Combine(Environment.CurrentDirectory, "genai_god.py");*///INPORTANT - FIX THIS SHITTY WAY, RN YOU HAVE TO PUT THE PYTHON SCRIPT IN THE ROOT FOLDER OF THE SERVER, FIX THIS METHOD LATER, MAYBE MAKE IT A CONFIG OPTION OR SOMETHING
            string pythonScriptPath = "assets\\aigod\\genai_god.py";
            string arguments = "\"" + message + "\""; // the message contents - RLY INPORTANT THAT IT HAS QUOTES, DONT REMOVE THEM
            string api_key = "\"AIzaSyBGeT9RGm3lSnH6ll8I4N_w97iJSW_FZGA\"";
            string sacrifices2 = "\"" + sacrifices + "\"";
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "python", // or "python3" python works on windows, if it doesnt, try python3 or reinstall python
                Arguments = $"\"{pythonScriptPath}\" {arguments} {api_key} {mood} {sacrifices2}",
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
                    return output; // hopefully doesnt break :)
                }
            }
        }
    }
}
