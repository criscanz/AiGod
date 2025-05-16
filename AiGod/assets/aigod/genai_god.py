#genai_god.py
# This script takes a prompt and generates a response using the Google Gemini API.
from google import genai
import sys
def gen(prompt: str, api_key: str):

    client = genai.Client(api_key="AIzaSyBGeT9RGm3lSnH6ll8I4N_w97iJSW_FZGA") # replace with the api_key var later

    response = client.models.generate_content(
        model="gemini-2.5-flash-preview-04-17",
        contents=prompt,
    )
                                                       
    return response.text
if len(sys.argv) > 1:
    # Arguments were passed
    var1 = sys.argv[1]
    var2 = sys.argv[2]
    print(gen(var1, var2))
else:
    # No arguments were passed
    print("Error:no prompt or no key")
