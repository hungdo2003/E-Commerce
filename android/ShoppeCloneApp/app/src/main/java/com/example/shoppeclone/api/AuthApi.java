package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface AuthApi {
    @POST("api/Auth/login")
    Call<LoginResponse> login(@Body LoginDto dto);
}
