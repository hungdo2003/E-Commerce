package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface ChatApi {
    @POST("api/Chat/ask")
    Call<ChatResponse> ask(@Body ChatRequest req);
}
