from conversation_handler import ConversationHandler

PINK = "\033[95m"
CYAN = "\033[96m"
Y = "\033[93m"
NEON_GREEN = "\033[92m"
R = "\033[91m"
RESET = "\033[0m"

# LLM_CLIENT_NAME = "llamafile_llama3"
LLM_CLIENT_NAME = "ollama"

handler = ConversationHandler("Shirts", LLM_CLIENT_NAME)
curr_role = "agent1"


def speak_to_agent(role, usr_msg):
    llm_response, next_user_task = handler.process_user_message(role, usr_msg)

    print(Y + f"{role}: {llm_response}" + RESET)


def check_for_transition(role):
    handler.check_for_state_transition(role)


print(R + f"Switch roles by typing 'agent1', 'agent2', or 'agent3'")
print(R + f"Type 'history' to see the conversation history")
print(R + f"Type 'exit' or 'quit' to exit the program")

while True:

    usr_msg = input(CYAN + f"Addressing {curr_role}: Enter command or message: ")

    if usr_msg in ["exit", "quit"]:
        break

    if usr_msg == "history":
        print(RESET + handler.get_agent_history_debug(curr_role))
        continue

    if usr_msg in ["agent1", "agent2", "agent3"]:
        curr_role = usr_msg
        print(PINK + f">> Role changed to {curr_role}")
        continue

    speak_to_agent(curr_role, usr_msg)
    check_for_transition(curr_role)

print("Exiting...")
