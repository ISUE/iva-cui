import time
from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
import uvicorn

from faster_whisper import WhisperModel

model_size = "distil-medium.en"
model = WhisperModel(model_size, device="cuda", compute_type="float16")

app = FastAPI()


@app.post("/transcribe_audio/")
async def upload_file(file: UploadFile = File(...)):
    try:
        # Calculate file size
        # file.file.seek(0, 2)
        # file_size = file.file.tell()
        # file.file.seek(0)

        # print("Uploaded file size: ", file_size, "bytes")

        st = time.time()

        segments, info = model.transcribe(
            file.file, beam_size=5, language="en", condition_on_previous_text=False
        )
        transcription = " ".join([segment.text for segment in segments])

        print("Transcription:", transcription)

        en = time.time()

        print("Time taken: ", en - st)

        return JSONResponse(content={"transcription": transcription})
    except Exception as e:
        print("Error: ", e)
        return JSONResponse(content={"error": "Error in transcription"})


if __name__ == "__main__":

    uvicorn.run(app, host="0.0.0.0", port=8083)
