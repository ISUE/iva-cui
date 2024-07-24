# How To Modify Sample Scene In Unity


![ISUE logo](https://avatars.githubusercontent.com/u/10524889?s=200&v=4)

We will show you how to easily modify our files to get this system running for your use case!


1. Clone both repositories.
    ```sh
    git clone https://github.com/maslychm/avatar_llm_api
    ```
    ```sh
    git clone https://github.com/maslychm/avatar_llm_unity
    ```

## In Visual Studio Code   
2. Open avatar_llm_api folder in Visual Studio Code.
3. Navigate to "test_all_scenes" folder and duplicate and rename.
4. In transition_prompts.py file change the prompts as needed and make sure to save
4. Navigate to "Terminal" tab at the top left and click "New Terminal"
   ![click terminal](README_images/terminal.png)
6. Windows with venv
    ```sh
    cd insert_renamed_folder
    ```
    ```sh
    python -m venv venv
    ```
    ```sh
    venv\Scripts\activate
    ```
    ```sh
    pip install edge-tts openai FastAPI[all] ollama
    ```
    ```sh
    uvicorn app:app --reload
    ```
    ![windows venv](README_images/windows_venv.png)
    ![windows venv](README_images/windows_venv_2.png)

   Conda (anywhere)
    ```sh
    # Run on WSL and have a running llamafile with its address
    ```
    ```sh
    conda create --name ml
    ```
    ```sh
    conda activate ml
    ```
    ```sh
    pip install edge-tts openai ollama
    ```
    ```sh
    # make sure subprocess command that creates shell has access to above-installed packages
    # Install FastAPI with uvicorn
    pip install FastAPI[all] 
    ```
    ```sh
    uvicorn app:app --reload
    ```
> If you see an error, you are in the wrong directory!
## In Unity 
7. Open Unity Hub.
7. Click "Add" at the top right.
   ![click add](README_images/add_file.png)
9. Locate and click the avatar_llm_unity folder.
10. Click "Open" (loading the file will take a few seconds).
11. Click the "Edit" tab at the top left of the window, navigate to and click "Project Settings".
    ![edit tab](README_images/click_project_settings.png)
13. Scroll to and click "Whisper" and disable CUDA by unchecking the "Enable CUDA" box.
    ![disable CUDA](README_images/disable_CUDA.png)
15. Click the "Projects" tab and navigate to the "Scenes" Folder. Click on the "Prompts test scene" scene and copy, paste, and rename this scene.
    ![duplicate scene](README_images/duplicate_scene.png)
> Windows: ctrl + C (copy), ctrl + V (paste)

> Mac: command + C (copy), command + V (paste)
   
19. Navigate to the play button at the top and middle of the unity window and you are good to test out the scene!

