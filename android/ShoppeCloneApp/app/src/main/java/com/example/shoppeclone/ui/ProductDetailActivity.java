package com.example.shoppeclone.ui;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.bumptech.glide.Glide;
import com.example.shoppeclone.R;
import com.example.shoppeclone.api.AddCartDto;
import com.example.shoppeclone.api.CartApi;
import com.example.shoppeclone.api.Product;
import com.example.shoppeclone.api.ProductsApi;
import com.example.shoppeclone.net.ApiClient;
import com.google.android.material.bottomsheet.BottomSheetDialog;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * 🛍 Màn hình chi tiết sản phẩm (Shopee style)
 */
public class ProductDetailActivity extends AppCompatActivity {

    private ImageView imgProduct, btnBack, btnCart;
    private TextView txtName, txtPrice, txtDescription, txtStock, txtCategory, txtCreatedAt;
    private Button btnAddCart;

    private ProductsApi productsApi;
    private CartApi cartApi;
    private Product currentProduct;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_product_detail);

        // Ánh xạ view
        imgProduct = findViewById(R.id.imgProduct);
        btnBack = findViewById(R.id.btnBack);
        btnCart = findViewById(R.id.btnCart);
        txtName = findViewById(R.id.txtName);
        txtPrice = findViewById(R.id.txtPrice);
        txtDescription = findViewById(R.id.txtDescription);
        txtStock = findViewById(R.id.txtStock);
        txtCategory = findViewById(R.id.txtCategory);
        txtCreatedAt = findViewById(R.id.txtCreatedAt);
        btnAddCart = findViewById(R.id.btnAddCart);

        productsApi = ApiClient.get(this).create(ProductsApi.class);
        cartApi = ApiClient.get(this).create(CartApi.class);

        btnBack.setOnClickListener(v -> finish());
        btnCart.setOnClickListener(v -> startActivity(new Intent(this, CartActivity.class)));

        int productId = getIntent().getIntExtra("product_id", -1);
        if (productId == -1) {
            Toast.makeText(this, "Không tìm thấy sản phẩm", Toast.LENGTH_SHORT).show();
            finish();
            return;
        }

        loadProductDetail(productId);

        btnAddCart.setOnClickListener(v -> {
            if (currentProduct != null) showAddToCartSheet(currentProduct);
            else Toast.makeText(this, "Đang tải sản phẩm...", Toast.LENGTH_SHORT).show();
        });
    }

    private void loadProductDetail(int id) {
        productsApi.getById(id).enqueue(new Callback<Product>() {
            @Override
            public void onResponse(Call<Product> call, Response<Product> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(ProductDetailActivity.this, "❌ Không tải được sản phẩm", Toast.LENGTH_SHORT).show();
                    return;
                }

                currentProduct = response.body();
                Product p = currentProduct;

                txtName.setText(p.name);
                txtPrice.setText(String.format("%,.0f₫", p.price));
                txtDescription.setText(p.description != null ? p.description : "Không có mô tả sản phẩm");
                txtStock.setText("Còn hàng: " + p.stock);
                txtCategory.setText("Danh mục ID: " + p.id);
                txtCreatedAt.setText("Ngày tạo: đang cập nhật...");

                Glide.with(ProductDetailActivity.this)
                        .load(p.thumbnailUrl)
                        .placeholder(R.drawable.ic_placeholder)
                        .into(imgProduct);
            }

            @Override
            public void onFailure(Call<Product> call, Throwable t) {
                Toast.makeText(ProductDetailActivity.this, "⚠️ Lỗi mạng: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showAddToCartSheet(Product product) {
        BottomSheetDialog sheet = new BottomSheetDialog(this, R.style.BottomSheetDialogTheme);
        View view = getLayoutInflater().inflate(R.layout.sheet_add_to_cart, null);
        sheet.setContentView(view);

        ImageView img = view.findViewById(R.id.sheet_imgProduct);
        TextView txtName = view.findViewById(R.id.sheet_txtName);
        TextView txtPrice = view.findViewById(R.id.sheet_txtPrice);
        TextView txtStock = view.findViewById(R.id.sheet_txtStock);
        TextView txtQty = view.findViewById(R.id.txtQuantity);
        ImageButton btnMinus = view.findViewById(R.id.btnMinus);
        ImageButton btnPlus = view.findViewById(R.id.btnPlus);
        Button btnConfirm = view.findViewById(R.id.btnConfirmAdd);

        Glide.with(this).load(product.thumbnailUrl)
                .placeholder(R.drawable.ic_placeholder)
                .into(img);

        txtName.setText(product.name);
        txtPrice.setText(String.format("%,.0f₫", product.price));
        txtStock.setText("Kho: " + product.stock);

        final int[] quantity = {1};
        txtQty.setText(String.valueOf(quantity[0]));

        btnMinus.setOnClickListener(v -> {
            if (quantity[0] > 1) {
                quantity[0]--;
                txtQty.setText(String.valueOf(quantity[0]));
            }
        });

        btnPlus.setOnClickListener(v -> {
            quantity[0]++;
            txtQty.setText(String.valueOf(quantity[0]));
        });

        btnConfirm.setOnClickListener(v -> {
            AddCartDto dto = new AddCartDto(product.id, quantity[0]);

            cartApi.add(dto).enqueue(new Callback<Void>() {
                @Override
                public void onResponse(Call<Void> call, Response<Void> response) {
                    if (response.isSuccessful()) {
                        Toast.makeText(ProductDetailActivity.this, "✅ Đã thêm vào giỏ hàng", Toast.LENGTH_SHORT).show();
                        sheet.dismiss();
                    } else {
                        Toast.makeText(ProductDetailActivity.this,
                                "❌ Thêm thất bại! Mã: " + response.code(),
                                Toast.LENGTH_SHORT).show();
                    }
                }

                @Override
                public void onFailure(Call<Void> call, Throwable t) {
                    Toast.makeText(ProductDetailActivity.this,
                            "⚠️ Lỗi mạng: " + t.getMessage(),
                            Toast.LENGTH_SHORT).show();
                }
            });
        });

        sheet.show();
    }
}
