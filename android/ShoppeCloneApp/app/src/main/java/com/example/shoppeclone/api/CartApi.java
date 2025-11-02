package com.example.shoppeclone.api;

import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface CartApi {
    @GET("api/Cart")
    Call<List<CartItem>> get();

    @POST("api/Cart")
    Call<Void> add(@Body AddCartDto dto); // GIỮ NGUYÊN Void

    @DELETE("api/Cart/{productId}")
    Call<Void> remove(@Path("productId") int productId);
}
