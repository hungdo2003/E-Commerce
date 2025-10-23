package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface PaymentsApi {
    @POST("api/Payments/zp/create")
    Call<ZpCreateResponse> create(@Body CreatePaymentDto dto);
}
