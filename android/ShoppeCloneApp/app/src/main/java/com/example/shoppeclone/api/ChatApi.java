package com.example.shoppeclone.api;

import com.example.shoppeclone.api.ChatAIResponse;
import com.example.shoppeclone.api.ChatRequestGemini;
import com.example.shoppeclone.api.ChatRequestRag;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.Headers;
import retrofit2.http.POST;

public interface ChatApi {

    // Gemini mode: POST /api/ai/chat
    @Headers({
        "Accept: application/json",
        "Content-Type: application/json"
    })
    @POST("api/ai/chat")
    Call<ChatAIResponse> chatGemini(@Body ChatRequestGemini body);

    // Trained + RAG mode: POST /api/ai/chat-rag
    @Headers({
        "Accept: application/json",
        "Content-Type: application/json"
    })
    @POST("api/ai/chat-rag")
    Call<ChatAIResponse> chatRag(@Body ChatRequestRag body);
}
