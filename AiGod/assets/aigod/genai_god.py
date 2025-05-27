#genai_god.py
# This script takes a prompt and generates a response using the Google Gemini API.
from google import genai
from google.genai import types
import sys

def gen(prompt: str, api_key: str, mood: int):
    client = genai.Client(api_key=api_key)

    response = client.models.generate_content(
        model="gemini-2.5-flash-preview-04-17",
        config=types.GenerateContentConfig(
            system_instruction=(
                "You are a god of a server in a game called 'Vintage Story'. "
                "You govern these people, and they will make sacrifices and ask you for advice. "
                "Also talk briefly and in a simplified Shakespearian tone. " 
                "You are also given a number, which means your mood. The higher the number, the better the mood. 20 is really good, and -20 is really bad. 0 is neutral. " 
                "You can execute commands with codes that you put at the end of your messages. "
                "For example, blah blah blah ::: KHBSK, which would execute the give command. "
                "List of commands: "
                "::: = start commanding (important, only use at the end of a message.) "
                "KHBSK = Duplicates whatever the player is holding in their hand. Use this sparingly, and only for the best of sacrifcies. " 
                "KHHHH = Make overall god mood go up by 1. Use this if the player is nice or gives a good sacrifice. " 
                "KHAAA = Make overall god mood go down by 1. Use this if a player is being rude or wrongs you. "
                "KHTSS = Spawn a thunderstorm on the player. Use this as a punishment."
            )
        ),
        contents="Current Mood: "+mood+", Player's Request: "+prompt
    )

    return response.candidates[0].content.parts[0].text

if len(sys.argv) >= 3:
    prompt_input = sys.argv[1]
    api_key_input = sys.argv[2]
    mood_input = sys.argv[3]
    print(gen(prompt_input, api_key_input, mood_input))
else:
    print("Error: Missing prompt, API key or mood. Usage: script.py '<prompt>' '<api_key>' '<mood>'")
