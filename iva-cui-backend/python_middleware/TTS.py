import edge_tts
import time
import asyncio


async def _make_speech(text_input, voice=None, rate=None):
    if not voice:
        voice = "de-DE-FlorianMultilingualNeural"
        print(f"Voice not specified. Using default voice: {voice}")

    if not rate:
        rate = "+0%"

    print(f"Generating voice {voice}")

    # Get the current Unix timestamp
    current_timestamp = int(time.time())

    # Convert the timestamp to string
    timestamp_string = str(current_timestamp)

    file_name = "static/" + timestamp_string + ".mp3"

    try:
        communicate = edge_tts.Communicate(text_input, voice, rate=rate)
        await communicate.save(file_name)
        return file_name
    except edge_tts.exceptions.NoAudioReceived:
        print(f"No audio received with voice {voice}.")
        return None
    except asyncio.TimeoutError:
        print(f"Request timed out for voice {voice}.")
        return None


# Wrapping the _make_speech coroutine to be used synchronously
def make_speech(text_input, voice, rate):
    return asyncio.run(_make_speech(text_input, voice, rate))
