package com.example.shoppeclone.api;

import java.util.List;

public class ChatRequestGemini {
    public String system;
    public List<ChatMessage> messages;

    public ChatRequestGemini(String system, List<ChatMessage> messages) {
        this.system = system;
        this.messages = messages;
    }
}
