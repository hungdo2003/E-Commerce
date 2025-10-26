package com.example.shoppeclone.ui;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.CartApi;
import com.example.shoppeclone.api.CartItem;
import com.example.shoppeclone.net.ApiClient;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CartActivity extends AppCompatActivity {
    private CartApi cartApi;
    private CartAdapter adapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_cart);

        // 🔙 Nút quay lại
        ImageView btnBack = findViewById(R.id.btnBack);
        btnBack.setOnClickListener(v -> finish());

        // 🧱 Ánh xạ RecyclerView
        RecyclerView recyclerView = findViewById(R.id.recycler);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        adapter = new CartAdapter();
        recyclerView.setAdapter(adapter);

        // ⚙️ Khởi tạo API
        cartApi = ApiClient.get(this).create(CartApi.class);

        // 💳 Nút Thanh Toán
        Button btnCheckout = findViewById(R.id.btnCheckout);
        btnCheckout.setOnClickListener(v ->
                startActivity(new Intent(CartActivity.this, CheckoutActivity.class))
        );

        // 🗑 Lắng nghe sự kiện trong adapter (Xóa / Cập nhật SL)
        adapter.setListener(new CartAdapter.OnAction() {
            @Override
            public void onRemove(CartItem item) {
                cartApi.remove(item.productId).enqueue(new Callback<Void>() {
                    @Override
                    public void onResponse(Call<Void> call, Response<Void> response) {
                        if (response.isSuccessful()) {
                            Toast.makeText(
                                    CartActivity.this,
                                    "🗑 Đã xóa " + item.name + " khỏi giỏ hàng",
                                    Toast.LENGTH_SHORT
                            ).show();
                            loadCart();
                        } else {
                            Toast.makeText(
                                    CartActivity.this,
                                    "❌ Xóa thất bại (" + response.code() + ")",
                                    Toast.LENGTH_SHORT
                            ).show();
                        }
                    }

                    @Override
                    public void onFailure(Call<Void> call, Throwable t) {
                        Toast.makeText(
                                CartActivity.this,
                                "⚠️ Lỗi mạng: " + t.getMessage(),
                                Toast.LENGTH_SHORT
                        ).show();
                    }
                });
            }

            @Override
            public void onQuantityChanged(CartItem item, int newQty) {
                Toast.makeText(
                        CartActivity.this,
                        "🔢 Đã thay đổi " + item.name + " thành " + newQty,
                        Toast.LENGTH_SHORT
                ).show();
            }
        });

        // 🔄 Lấy dữ liệu giỏ hàng khi vào màn hình
        loadCart();
    }

    /** 🔁 Hàm tải danh sách sản phẩm trong giỏ hàng */
    private void loadCart() {
        cartApi.get().enqueue(new Callback<List<CartItem>>() {
            @Override
            public void onResponse(Call<List<CartItem>> call, Response<List<CartItem>> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(
                            CartActivity.this,
                            "❌ Lỗi tải giỏ hàng (" + response.code() + ")",
                            Toast.LENGTH_SHORT
                    ).show();
                    return;
                }
                adapter.submit(response.body());
            }

            @Override
            public void onFailure(Call<List<CartItem>> call, Throwable t) {
                Toast.makeText(
                        CartActivity.this,
                        "⚠️ Lỗi mạng: " + t.getMessage(),
                        Toast.LENGTH_SHORT
                ).show();
            }
        });
    }
}
