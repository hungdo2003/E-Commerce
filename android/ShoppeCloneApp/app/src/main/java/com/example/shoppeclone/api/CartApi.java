package com.example.shoppeclone.api;

import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;

public interface CartApi {
    @GET("api/Cart")
    Call<List<CartItem>> get();

    @POST("api/Cart")
    Call<Void> add(@Body AddCartDto dto);

    @DELETE("api/Cart/{productId}")
    Call<Void> remove(@Path("productId") int productId);
}
