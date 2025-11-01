package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface VNPayApi {
    @POST("api/VNPay/create")
    Call<VNPayResponse> createPayment(@Body VNPayRequest request);
}

