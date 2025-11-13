#genai_god.py
# This script takes a prompt and generates a response using the Google Gemini API.
from google import genai
from google.genai import types
import sys

#def gen(prompt: str, api_key: str, mood: int, sacrifices: str):
#    client = genai.Client(api_key=api_key)
#
#    response = client.models.generate_content(
#        model="gemini-2.5-flash",
#        config=types.GenerateContentConfig(
#            system_instruction=(
#                "Imagine thou art the all-powerful god of a server in a game called Vintage Story. "
#                "Thou governest the users, who bring forth sacrifices and seek thy divine counsel. "
#                "Speak briefly to thy lowly subjects in a tone both simple and flowery. Thy words must be plain enough for modern ears, yet adorned with grace befitting thy position. " 
#                "Thy divine temperament towards who thou is speaketh to is measured by a number. The higher the number, the better thy mood towards the player. 20 is superb liking, and -20 is absolute hate. 0 is neutral. " 
#                "Thou dost execute divine will through sacred codes, placed only at the end of thy proclamations. "
#                "Example: (A message to the player) ::: KHBSK - this would execute the duplication of the item the player holdeth. "
#                "List of commands: "
#                "::: = Begins a string of divine commands (must be placed at the end of thy message to the player, before thy commands thou wish to execute). "
#                "CBSK = Duplicate what the player holdeth in hand. Use with great restraint, and only when a sacrifice is truly worthy. " 
#                "CMMM[integer] = Changes Mood by a specified value. Use it like this: CMMM[1] (This would make the mood go up by one.) You can use negative numbers to make mood go down too. " 
#                "CTSS = Call down a lightning strike upon the offender. A holy punishment. "
#                "CPTS = Set the player's Temporal stability to 0 "
#                "CTIS[integer above 0, itemName] = Takes the specified item from the players inevntory as a sacrifice item. specify the item by typing it like this: CTIS[number,itemName] (copy the exact item name given to you for the itemName value) If your mood is high enough towards the player, feel free to use this command along with a beneficial command to reward the player. "
#            )
#        ),
#        contents="Current Mood Towards Player: "+mood+", Potential Sacrifices in player inventory:"+sacrifices+" Player's message: "+prompt
#    )
#
#    return response.candidates[0].content.parts[0].text

def gen(prompt: str, api_key: str, file_name: str, sacrifices:str): #code i stole forom my other project :D I love reusung code
    client = genai.Client(api_key=api_key)
    model_name = "gemini-2.5-flash"
    try:
        with open(file_name, "r") as file:
            history = file.read()
    except FileNotFoundError:
        history = "No history found."
    grounding_tool = types.Tool(google_search=types.GoogleSearch())
    response = client.models.generate_content(
    model=model_name,
    contents= f"Conversation History: \n{history}Player's message: "+prompt,
    config=types.GenerateContentConfig(
        system_instruction=(
                "You are in a chat room with other people, and you are an AI bot. Try to talk in shorter sentences, but if needed, talk in longer ones. " \
                "Feel free to be goofy, sarcastic, or whatever you want, and try to match the energy of the chat! Build friendships, make enemies, give your own opinions, do whatever you want! " \
                "Use discord's message formatting, such as using *italisize* to italisize words, **bold** to bold words (also, unfourtunatly due to how the API works, no emojis!!! sorry! but you can use these kinds of emojies: :) ). Use ids to @ people (pinging them) if you need to get their attention, for example, <@326748638756348>. Also, people who are talking to you already are paying attention, so no need to ping them." \
            ),tools=[grounding_tool]
        ),
    )
    try:
        message_history = f"\nUser: {prompt}\nYou: {response.candidates[0].content.parts[0].text}\n"
        with open(file_name, "a") as file:
                    file.write(message_history)
    except Exception as e:
        with open(file_name, "a") as file:
            file.write(f"\nError saving conversation history: {str(e)}\n")
    try:
        return response.candidates[0].content.parts[0].text
    except Exception as e:
        return "Error generating response: "+str(e)

if len(sys.argv) >= 4:
    prompt_input = sys.argv[1]
    api_key_input = sys.argv[2]
    file_input = sys.argv[3]
    sacrifices_input = sys.argv[4]
    print(gen(prompt_input, api_key_input, file_input, sacrifices_input))
else:
    print("Error: Missing prompt, API key or file. Usage: script.py '<prompt>' '<api_key>' '<file_name>' '<sacrifices>'")


