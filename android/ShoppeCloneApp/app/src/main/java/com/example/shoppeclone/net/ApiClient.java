package com.example.shoppeclone.net;

import android.content.Context;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.logging.HttpLoggingInterceptor;
import okhttp3.Protocol;
import java.util.Arrays;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    // ĐỔI PORT này thành port BE của bạn (ví dụ: 54813)
    public static final String BASE_URL = "http://10.0.2.2:5080/";

    private static Retrofit retrofit;

    // Tạo Retrofit với BASE_URL mặc định và tự động đính kèm Bearer token từ SharedPreferences
    public static Retrofit get(Context ctx){
        if (retrofit == null) {
            HttpLoggingInterceptor log = new HttpLoggingInterceptor();
            log.setLevel(HttpLoggingInterceptor.Level.BODY);

            OkHttpClient client = new OkHttpClient.Builder()
                    .addInterceptor(log)
                    .addInterceptor(chain -> {
                        String token = SessionManager.getToken(ctx);
                        Request original = chain.request();
                        if (token != null && !token.isEmpty()) {
                            Request req = original.newBuilder()
                                    .header("Authorization", "Bearer " + token)
                                    .build();
                            return chain.proceed(req);
                        }
                        return chain.proceed(original);
                    })
                    // Tránh lỗi stream do HTTP/2 không tương thích ở môi trường dev
                    .protocols(Arrays.asList(Protocol.HTTP_1_1))
                    .retryOnConnectionFailure(true)
                    .build();

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(client)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }

    // Tạo Retrofit với baseUrl truyền vào, dùng khi muốn trỏ tới môi trường khác (dev/staging/prod)
    public static Retrofit get(String baseUrl, Context ctx){
        HttpLoggingInterceptor log = new HttpLoggingInterceptor();
        log.setLevel(HttpLoggingInterceptor.Level.BODY);

        OkHttpClient client = new OkHttpClient.Builder()
                .addInterceptor(log)
                .addInterceptor(chain -> {
                    String token = SessionManager.getToken(ctx);
                    Request original = chain.request();
                    if (token != null && !token.isEmpty()) {
                        Request req = original.newBuilder()
                                .header("Authorization", "Bearer " + token)
                                .build();
                        return chain.proceed(req);
                    }
                    return chain.proceed(original);
                })
                .protocols(Arrays.asList(Protocol.HTTP_1_1))
                .retryOnConnectionFailure(true)
                .build();

        return new Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
    }

}
