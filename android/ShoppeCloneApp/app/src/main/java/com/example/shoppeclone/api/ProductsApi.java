package com.example.shoppeclone.api;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface ProductsApi {
    @GET("api/Products")
    Call<PagedProducts> list(@Query("page") int page,
                             @Query("size") int size,
                             @Query("q") String q,
                             @Query("categoryId") Integer categoryId);

    @GET("api/Products/{id}")
    Call<Product> getById(@Path("id") int id);

}
