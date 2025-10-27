package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface AuthApi {

    // 🔹 API đăng nhập
    @POST("api/Auth/Login")
    Call<LoginResponse> login(@Body LoginDto dto);

    // 🔹 API đăng ký
    @POST("api/Auth/register")
    Call<Void> register(@Body RegisterDto dto);


}
