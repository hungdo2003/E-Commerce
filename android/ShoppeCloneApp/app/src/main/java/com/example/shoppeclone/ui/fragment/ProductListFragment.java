package com.example.shoppeclone.ui.fragment;

import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.PagedProducts;
import com.example.shoppeclone.api.ProductsApi;
import com.example.shoppeclone.net.ApiClient;
import com.example.shoppeclone.ui.CartActivity;
import com.example.shoppeclone.ui.ProductAdapter;
import com.example.shoppeclone.api.ProductItem;
import com.example.shoppeclone.ui.ProductDetailActivity; // 🆕 thêm import

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Fragment hiển thị danh sách sản phẩm.
 * Có thanh tìm kiếm và icon giỏ hàng ở trên cùng.
 */
public class ProductListFragment extends Fragment {

    private ProductsApi productsApi;
    private ProductAdapter adapter;

    private EditText edtSearch;
    private ImageView imgCart;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_product_list, container, false);

        // 🧱 Ánh xạ view
        RecyclerView recyclerView = view.findViewById(R.id.recycler);
        edtSearch = view.findViewById(R.id.edtSearch);
        imgCart = view.findViewById(R.id.imgCart);

        // ✅ Dùng GridLayoutManager để hiển thị 2 cột
        GridLayoutManager gridLayoutManager = new GridLayoutManager(requireContext(), 2);
        recyclerView.setLayoutManager(gridLayoutManager);

        adapter = new ProductAdapter();
        recyclerView.setAdapter(adapter);

        // ⚙️ Khởi tạo API client
        productsApi = ApiClient.get(requireContext()).create(ProductsApi.class);

        // 🛒 Bấm icon giỏ hàng → mở CartActivity
        imgCart.setOnClickListener(v ->
                startActivity(new Intent(requireContext(), CartActivity.class))
        );

        // 🎯 Bấm vào card sản phẩm → mở trang chi tiết
        adapter.setListener(product -> {
            Intent intent = new Intent(requireContext(), ProductDetailActivity.class);
            intent.putExtra("product_id", product.id);
            startActivity(intent);
        });

        // 🚀 Gọi API ban đầu
        loadProducts("");

        // 🔍 Lọc sản phẩm theo từ khóa tìm kiếm
        edtSearch.addTextChangedListener(new TextWatcher() {
            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override public void afterTextChanged(Editable s) {}
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                loadProducts(s.toString().trim());
            }
        });

        return view;
    }

    /**
     * Hàm gọi API lấy danh sách sản phẩm (có tìm kiếm).
     */
    private void loadProducts(String keyword) {
        productsApi.list(1, 20, keyword.isEmpty() ? null : keyword, null)
                .enqueue(new Callback<PagedProducts>() {
                    @Override
                    public void onResponse(Call<PagedProducts> call, Response<PagedProducts> response) {
                        if (!response.isSuccessful() || response.body() == null) {
                            Toast.makeText(requireContext(),
                                    "Lỗi tải sản phẩm: " + response.code(),
                                    Toast.LENGTH_SHORT).show();
                            return;
                        }

                        if (response.body().items == null || response.body().items.isEmpty()) {
                            adapter.submit(java.util.Collections.emptyList());
                        } else {
                            adapter.submit(response.body().items);
                        }
                    }

                    @Override
                    public void onFailure(Call<PagedProducts> call, Throwable t) {
                        Toast.makeText(requireContext(),
                                "Lỗi mạng: " + t.getMessage(),
                                Toast.LENGTH_LONG).show();
                    }
                });
    }
}
