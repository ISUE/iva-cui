# IVA-CUI

This repository contains the Python and Unity code for a [paper](https://doi.org/10.1145/3719160.3736636) titled
"**Mitigating Response Delays in Free-Form Conversations with LLM-powered Intelligent Virtual Agents**" to appear in the Proceedings of the 7th ACM Conference on Conversational User Interfaces [(CUI '25)](https://cui.acm.org/2025/). If you use this code or Unity environments in your research, please cite our paper (see [Citation](#citation) section below).

## Table of Contents

- [Unity Setup](#unity-setup)
  - [User study scenes](#user-study-scenes)
  - [How to run the scenes](#how-to-run-the-scenes)
  - [Desktop mode](#desktop-mode)
  - [VR mode](#vr-mode)
  - [Controls](#controls)
- [Python Setup](#python-setup)
  - [Setup Steps](#setup-steps)
    - [Running LLM locally on Windows](#running-llm-locally-on-windows)
    - [Running Python middleware on Windows](#running-python-backend-middleware-on-windows)
    - [Running the ASR model on WSL](#running-the-asr-model-on-wsl)
- [Citation](#citation)  

## Unity Setup

### User study scenes

All scenes are located in [iva-cui-unity/Assets/Scenes/](iva-cui-unity/Assets/Scenes/). List of licenses for third-party code and assets used in this project can be found in the [ASSET_LICENSES.md](iva-cui-unity/ASSET_LICENSES.md) file.

- `City_Scene.unity` -> Scenario 1
- `Hotel_Scene.unity` -> Scenario 2
- `Museum_Scene.unity` -> Scenario 3

### How to run the scenes

- Unity version: 2022.3.21
- Run [Python backend](#python-setup) before running the Unity scenes.
- VR and Desktop (non-VR) modes are supported. Follow instructions in [Desktop Mode](#desktop-mode) and [VR Mode](#vr-mode).
- To speak with agents, **toggle mic on before** and **toggle mic off after** you speak (see [Controls](#controls)). Adjust microphone on the `SceneControls` gameobject in scene hierarchy (see screenshot below, [Desktop Mode](#desktop-mode) and [VR Mode](#vr-mode)).
- Agents will respond after a short delay. If no agent can hear you or an agent is currently *thinking* or *speaking*, you will hear a **broken mic** sound.  
![mic setup](setup.png)

### Desktop mode

1. Enable `WASD Player` gameobject in hierarchy
2. Disable `XR Interaction Setup` gameobject in hierarchy
3. On the `SceneControls` gameobject, set a working microphone

### VR mode

1. Enable `XR Interaction Setup` gameobject in hierarchy
2. Disable `WASD Player` gameobject in hierarchy
3. On the `SceneControls` gameobject, set microphone to `Oculus Virtual Audio Device` (or other device equivalent)

### Controls

| **Action**            | **VR Mode**     | **Desktop Mode** |
| --------------------- | ------------------- | -------------------- |
| Toggle microphone     | A                   | M                    |
| Move                  | Left Stick          | WASD                 |
| Look around           | Right Stick         | Mouse                |
| Sprint                | –                   | Left Shift           |
| Interact with objects | Side Trigger (Grab) | –                    |

## Python Setup

Backend (we also call it 'middleware') is responsible for handling requests from Unity, processing audio files, and interacting with the LLM server. It is located in the [iva-cui-backend](iva-cui-backend/) directory.

### Setup Steps

The outcome from following these instructions should be:

- A local LLM server running on port `8082` (or `11434` for Ollama)
- A local ASR server running on port `8083`
- A local Python middleware server running on port `8080`

#### Running LLM locally on Windows

By default, backend runs using [Ollama](https://ollama.com/download/windows). We recommend using it, however, OpenAI API-style LLM server endpoints and locally-deployed options ([llamafile](https://github.com/Mozilla-Ocho/llamafile/releases) and [LMStudio](https://lmstudio.ai/)) are also supported. LLM API endpoints are specified in [iva-cui-backend/python_middleware/llm_backends.py](iva-cui-backend/python_middleware/llm_backends.py). If you want to switch to OpenAI-style endpoints, you can do so by changing the `LLM_BACKEND` variable in [iva-cui-backend/python_middleware/app.py](iva-cui-backend/python_middleware/app.py).

##### Ollama (local, recommended)

1. Download and install [Ollama](https://ollama.com/download/windows).
2. Run `ollama run llama3.1:8b-instruct-q5_K_M`.
3. Set the `LLM_BACKEND` variable in [iva-cui-backend/python_middleware/app.py](iva-cui-backend/python_middleware/app.py) to `ollama`.

##### LMStudio (local, OpenAI-style endpoints)

1. Download, install and run [LMStudio](https://lmstudio.ai/).
2. Download this model `lmstudio-community/Meta-Llama-3.1-8B-Instruct-GGUF/Meta-Llama-3.1-8B-Instruct-Q5_K_M.gguf`.
3. Set the UI mode to "Developer" or "Power User" (bottom left corner).
4. Go to "Developer" tab -> Settings -> Server Port and set it to `8082`.
5. Start the server by toggling the switch in the top left corner.
6. Set the `LLM_BACKEND` variable in [iva-cui-backend/python_middleware/app.py](iva-cui-backend/python_middleware/app.py) to `llamafile_llama3`.

##### llamafile (local, OpenAI-style endpoints)

1. Download [llamafile-0.9.0](https://github.com/Mozilla-Ocho/llamafile/releases/tag/0.9.0)
2. Rename `llamafile-0.9.0` to `llamafile-0.9.0.exe`
3. Download `Meta-Llama-3.1-8B-Instruct-Q5_K_M.gguf` from [huggingface](https://huggingface.co/bullerwins/Meta-Llama-3.1-8B-Instruct-GGUF/tree/828492ca0d7e7efd4b316e75af8d9cd582fdec34)
4. Run `llamafile-0.9.0.exe --server -ngl 9999 -m Meta-Llama-3.1-8B-Instruct-Q5_K_M.gguf --host 0.0.0.0 --port 8082`
5. Set the `LLM_BACKEND` variable in [iva-cui-backend/python_middleware/app.py](iva-cui-backend/python_middleware/app.py) to `llamafile_llama3`

##### Official OpenAI API (remote)

1. Create a file mentioned in the `load_openai_key()` function in [iva-cui-backend/python_middleware/llm_backends.py](iva-cui-backend/python_middleware/llm_backends.py) and put your OpenAI API key there. The file should contain only the key, no other text. Alternatively, modify that function to load the key from an environment variable. You can also make the function directly return the key in the code (not recommended).
2. Set the `LLM_BACKEND` variable in [iva-cui-backend/python_middleware/app.py](iva-cui-backend/python_middleware/app.py) to `openai_4` or `openai_4mini`. You can also use other models by directly setting the `model="gpt-4o"` in an appropriate class in the [iva-cui-backend/python_middleware/llm_backends.py](iva-cui-backend/python_middleware/llm_backends.py) file.

#### Running Python backend (middleware) on Windows

```bash
# create and activate virtual environment
python -m venv venv
venv\Scripts\activate

# install the required packages
pip install openai ollama edge-tts FastAPI[all]

# navigate to the directory and run the server
cd iva-cui-backend\python_middleware
uvicorn app:app --reload
```

#### Running the ASR model on WSL

```bash
# create a virtual environment
sudo apt update
sudo apt install python3-venv
python3 -m venv venv

# activate the virtual environment
source venv/bin/activate

# install the required packages
pip install nvidia-cublas-cu12 nvidia-cudnn-cu12==9.*

export LD_LIBRARY_PATH=`python -c 'import os; import nvidia.cublas.lib; import nvidia.cudnn.lib; print(os.path.dirname(nvidia.cublas.lib.__file__) + ":" + os.path.dirname(nvidia.cudnn.lib.__file__))'`

pip install faster_whisper FastAPI[all]

# navigate to directory and run the ASR server
cd iva-cui-backend\transcription_server
python whisper_server.py
```

### Test LLM Locally

```bash
cd iva-cui-backend\python_middleware
python test_conv.py
```
## Authors

[Mykola Maslych](https://github.com/maslychm), [Mohammadreza Katebi](https://github.com/MRkatebi99), [Christopher Lee](https://github.com/hpipyT), [Yahya Hmaiti](https://github.com/YHmaiti), [Amirpouya Ghasemaghaei](https://github.com/PouyaAghaei), [Christian Pumarada](https://github.com/Aurelius1824), [Janneese Palmer](https://github.com/janneese), [Esteban Segarra Martinez](https://overcodedstack.github.io/), [Marco Emporio](https://marcokero.github.io/), [Warren Snipes](https://github.com/LockedThread), [Ryan P. McMahan](https://orcid.org/0000-0001-9357-9696), [Joseph J. LaViola Jr.](https://orcid.org/0000-0003-1186-4130)

## Citation

If you use this code in your research, please cite our paper:

```bibtex
@inproceedings{Maslych2025Mitigating,
    author    = {Mykola Maslych and Mohammadreza Katebi and Christopher Lee and Yahya Hmaiti and Amirpouya Ghasemaghaei and Christian Pumarada and Janneese Palmer and Esteban Segarra Martinez and Marco Emporio and Warren Snipes and Ryan P. McMahan and Joseph J. LaViola Jr.},
    title     = {Mitigating Response Delays in Free-Form Conversations with LLM-powered Intelligent Virtual Agents},
    booktitle = {Proceedings of the 7th ACM Conference on Conversational User Interfaces (CUI ’25)},
    year      = {2025},
    month     = jul,
    pages     = {1--15},
    publisher = {ACM},
    address   = {New York, NY, USA},
    location  = {Waterloo, ON, Canada},
    doi       = {10.1145/3719160.3736636},
    url       = {https://doi.org/10.1145/3719160.3736636}
}
```
