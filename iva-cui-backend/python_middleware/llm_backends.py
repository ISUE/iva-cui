from ollama import Client
from openai import OpenAI


def load_openai_key():
    with open("../CHATGPT_API.txt") as f:
        return f.readline()


class OpenAIClient_llama3:
    def __init__(self, host_address="http://127.0.0.1:8082/v1"):
        self.host_address = host_address
        self.client = OpenAI(
            base_url=self.host_address,
            api_key="sk-no-key-required",
        )

    def chat(self, messages, temperature=0.5, max_tokens=None):
        completion = self.client.chat.completions.create(
            model="llama3",
            messages=messages,
            max_tokens=max_tokens,
            temperature=temperature,
            stop=["<|im_start|>", "<|eot_id|>"],
        )
        return completion.choices[0].message.content


class OpenAIClient_gpt4o:
    def __init__(self):
        self.client = OpenAI(
            api_key=load_openai_key(),
        )

    def chat(self, messages, temperature=0.5, max_tokens=None):
        completion = self.client.chat.completions.create(
            model="gpt-4o",
            messages=messages,
            max_tokens=max_tokens,
            temperature=temperature,
            # stop=["<|im_start|>", "<|eot_id|>"],
        )
        return completion.choices[0].message.content


class OpenAIClient_gpt4o_mini:
    def __init__(self):
        self.client = OpenAI(
            api_key=load_openai_key(),
        )

    def chat(self, messages, temperature=0.5, max_tokens=None):
        completion = self.client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            max_tokens=max_tokens,
            temperature=temperature,
            # stop=["<|im_start|>", "<|eot_id|>"],
        )
        return completion.choices[0].message.content


class OllamaClient:
    def __init__(self, host_address="http://127.0.0.1:11434"):
        self.client = Client(host=host_address)

    def chat(self, messages, temperature=0.5, max_tokens=None):
        completion = self.client.chat(
            model="llama3.1:8b-instruct-q5_K_M",
            messages=messages,
            options={
                "num_predict": max_tokens,
                "temperature": temperature,
                # "stop": []
            },
        )
        return completion["message"]["content"]
