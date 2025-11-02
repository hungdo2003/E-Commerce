package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.POST;
import retrofit2.http.Path;

public interface VNPayApi {
    @POST("api/VNPay/{orderId}/pay-with-vnpay") // ✅ SỬA THÀNH POST
    Call<VNPayResponse> createPayment(@Path("orderId") int orderId);
}