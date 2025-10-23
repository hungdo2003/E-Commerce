package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.POST;

public interface OrdersApi {
    @POST("api/Orders/create-from-cart")
    Call<CreateOrderResponse> createFromCart();
}
