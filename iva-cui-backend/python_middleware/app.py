"""
uvicorn app:app --reload
"""

import os
from fastapi import FastAPI, Query, BackgroundTasks
from fastapi.staticfiles import StaticFiles

import time

from TTS import _make_speech
from conversation_handler import ConversationHandler

LLM_CLIENT_NAME = "llamafile_llama3"
# LLM_CLIENT_NAME = "openai_4"
# LLM_CLIENT_NAME = "openai_4mini"

ON_THIS_DEVICE = False

# create a static/ directory if it doesn't exist
if not os.path.exists("static"):
    os.makedirs("static")

app = FastAPI()

app.mount("/static", StaticFiles(directory="static"), name="static")


def save_timings_to_file(
    role,
    user_input_word_count,
    response_word_count,
    transition_length,
    process_time,
    speech_time,
):
    """
    Writes a line to the logging file with the following header:
    llm_client_name,role,user_input_word_count,response_word_count,transition_length,process_time,speech_time
    """

    with open("timings", "a") as f:
        line = f"{LLM_CLIENT_NAME},"
        line += f"{role},"
        line += f"{user_input_word_count},"
        line += f"{response_word_count},"
        line += f"{transition_length},"
        line += f"{process_time:.3f},"
        line += f"{speech_time:.3f}\n"

        f.write(line)


@app.get("/speak/{role}/")
async def speak(
    role: str,
    background_tasks: BackgroundTasks,
    text: str = Query(default="Hello, how is it going?", alias="q"),
):
    if len(text) < 2:
        print("Text too short")
        text = "Hey"

    global handler

    _st = time.time()
    llm_response, next_user_task = handler.process_user_message(role, text)
    _llm_processing_duration = (time.time() - _st) * 1000

    _st = time.time()
    voice, rate = handler.get_role_voice(role)
    fname = await _make_speech(llm_response, voice, rate)
    _speech_generation_duration = (time.time() - _st) * 1000

    # Add background task to save timings after response is sent
    background_tasks.add_task(
        save_timings_to_file,
        role,
        user_input_word_count=len(text.split()),
        response_word_count=len(llm_response.split()),
        process_time=_llm_processing_duration,
        transition_length=len(next_user_task.split()),
        speech_time=_speech_generation_duration,
    )

    response = {
        "message": llm_response,
        "audio": fname,
        "transition": next_user_task,
    }
    logging_info = {
        "llm_client_name": LLM_CLIENT_NAME,
        "user_input_word_count": len(text.split()),
        "response_word_count": len(llm_response.split()),
        "transition_length": len(next_user_task.split()),
        "llm_generation_time": f"{_llm_processing_duration:.3f}",
        "speech_generation_time": f"{_speech_generation_duration:.3f}",
    }

    response.update(logging_info)

    return response


@app.get("/refresh/{scene_name}/")
async def refresh(scene_name: str):
    global handler
    handler = ConversationHandler(scene_name, LLM_CLIENT_NAME)
    return {"message": "Message history refreshed!"}


@app.get("/check_transition/{role}/")
async def check_transition(role: str):
    global handler

    transition = handler.check_for_state_transition(role)
    return {"role": role, "transition": transition}
