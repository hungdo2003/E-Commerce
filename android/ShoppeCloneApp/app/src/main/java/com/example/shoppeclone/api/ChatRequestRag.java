package com.example.shoppeclone.api;

import java.util.List;

public class ChatRequestRag {
    public String system;             
    public List<ChatMessage> messages; 

    // RAG options
    public Integer k;        
    public String tag;       
    public String source;    
    public String tenantId;  

    public ChatRequestRag(String system, List<ChatMessage> messages,
                          Integer k, String tag, String source, String tenantId) {
        this.system = system;
        this.messages = messages;
        this.k = k;
        this.tag = tag;
        this.source = source;
        this.tenantId = tenantId;
    }
}
