package com.example.shoppeclone.ui;

import android.os.Bundle;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.AddCartDto;
import com.example.shoppeclone.api.PagedProducts;
import com.example.shoppeclone.api.ProductsApi;
import com.example.shoppeclone.api.CartApi;
import com.example.shoppeclone.net.ApiClient;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProductListActivity extends AppCompatActivity {

    private ProductsApi productsApi;
    private CartApi cartApi;
    private ProductAdapter adapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_product_list);

        // RecyclerView + Adapter
        RecyclerView rv = findViewById(R.id.recycler);
        rv.setLayoutManager(new LinearLayoutManager(this));
        adapter = new ProductAdapter();
        rv.setAdapter(adapter);

        // API clients (dùng ApiClient trung tâm)
        productsApi = ApiClient.get(this).create(ProductsApi.class);
        cartApi     = ApiClient.get(this).create(CartApi.class);

        // Sự kiện "Thêm vào giỏ" cho từng item
        adapter.setListener(p ->
                cartApi.add(new AddCartDto(p.id, 1)).enqueue(new Callback<Void>() {
                    @Override public void onResponse(Call<Void> call, Response<Void> response) {
                        if (response.isSuccessful()) {
                            Toast.makeText(ProductListActivity.this, "Đã thêm vào giỏ", Toast.LENGTH_SHORT).show();
                        } else if (response.code() == 401) {
                            Toast.makeText(ProductListActivity.this, "Cần đăng nhập", Toast.LENGTH_SHORT).show();
                            startActivity(new android.content.Intent(ProductListActivity.this, LoginActivity.class));
                        } else {
                            Toast.makeText(ProductListActivity.this, "Thêm thất bại: HTTP " + response.code(), Toast.LENGTH_LONG).show();
                        }
                    }
                    @Override public void onFailure(Call<Void> call, Throwable t) {
                        Toast.makeText(ProductListActivity.this, "Lỗi mạng: " + t.getMessage(), Toast.LENGTH_LONG).show();
                    }
                })
        );


        // Gọi API lấy danh sách sản phẩm
        loadProducts();
        findViewById(R.id.btnCart).setOnClickListener(v -> {
            startActivity(new android.content.Intent(this, CartActivity.class));
        });
    }

    private void loadProducts() {
        productsApi.list(1, 20, null, null).enqueue(new Callback<PagedProducts>() {
            @Override
            public void onResponse(Call<PagedProducts> call, Response<PagedProducts> rsp) {
                if (!rsp.isSuccessful() || rsp.body() == null) {
                    Toast.makeText(ProductListActivity.this, "Lỗi: " + rsp.code(), Toast.LENGTH_SHORT).show();
                    return;
                }
                if (rsp.body().items == null || rsp.body().items.isEmpty()) {
                    Toast.makeText(ProductListActivity.this, "Chưa có sản phẩm.", Toast.LENGTH_SHORT).show();
                    adapter.submit(java.util.Collections.emptyList());
                } else {
                    adapter.submit(rsp.body().items);
                }
            }

            @Override
            public void onFailure(Call<PagedProducts> call, Throwable t) {
                Toast.makeText(ProductListActivity.this, "Lỗi mạng: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }


    private GlobalChatOverlay chatOverlay;

    @Override
    protected void onResume() {
        super.onResume();
        if (chatOverlay == null) chatOverlay = new GlobalChatOverlay(this);
        chatOverlay.attach();
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (chatOverlay != null) chatOverlay.detach();
    }
}
