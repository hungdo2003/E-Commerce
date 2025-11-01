package com.example.shoppeclone.ui;

import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.text.TextUtils;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.CreateOrderResponse;
import com.example.shoppeclone.api.OrdersApi;
import com.example.shoppeclone.api.VNPayApi;
import com.example.shoppeclone.api.VNPayRequest;
import com.example.shoppeclone.api.VNPayResponse;
import com.example.shoppeclone.net.ApiClient;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {
    private OrdersApi ordersApi;
    private VNPayApi vnPayApi;
    private TextView txtTotal;
    private Button btnPay;
    private CreateOrderResponse currentOrder;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        txtTotal = findViewById(R.id.txtTotal);
        btnPay = findViewById(R.id.btnPay);

        ordersApi = ApiClient.get(this).create(OrdersApi.class);
        vnPayApi = ApiClient.get(this).create(VNPayApi.class);

        btnPay.setOnClickListener(v -> createOrderAndPay());
    }

    private void createOrderAndPay() {
        ordersApi.createFromCart().enqueue(new Callback<CreateOrderResponse>() {
            @Override
            public void onResponse(Call<CreateOrderResponse> call, Response<CreateOrderResponse> response) {
                if (!response.isSuccessful()) {
                    String errorMsg = "Tạo đơn hàng thất bại";
                    
                    // Xử lý các lỗi cụ thể
                    if (response.code() == 400) {
                        errorMsg = "🛒 Giỏ hàng trống! Vui lòng thêm sản phẩm trước khi thanh toán.";
                    } else if (response.code() == 401) {
                        errorMsg = "⚠️ Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                    } else {
                        errorMsg = "❌ Lỗi: HTTP " + response.code();
                    }
                    
                    Toast.makeText(CheckoutActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                    return;
                }
                
                if (response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "⚠️ Không nhận được thông tin đơn hàng",
                            Toast.LENGTH_LONG).show();
                    return;
                }

                currentOrder = response.body();
                txtTotal.setText(String.format("Tổng tiền: %.0f₫", currentOrder.totalAmount));

                // Thanh toán trực tiếp bằng VNPay
                requestVNPay(currentOrder);
            }

            @Override
            public void onFailure(Call<CreateOrderResponse> call, Throwable t) {
                Toast.makeText(CheckoutActivity.this,
                        "⚠️ Lỗi mạng khi tạo đơn: " + t.getMessage(),
                        Toast.LENGTH_LONG).show();
            }
        });
    }

    private void requestVNPay(CreateOrderResponse order) {
        String description = "Thanh toán đơn hàng #" + order.orderId;
        VNPayRequest request = new VNPayRequest(order.orderId, description);
        
        vnPayApi.createPayment(request).enqueue(new Callback<VNPayResponse>() {
            @Override
            public void onResponse(Call<VNPayResponse> call, Response<VNPayResponse> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "Gọi VNPay thất bại: HTTP " + response.code(),
                            Toast.LENGTH_LONG).show();
                    return;
                }

                VNPayResponse vnPayResponse = response.body();
                if (vnPayResponse.success && !TextUtils.isEmpty(vnPayResponse.paymentUrl)) {
                    openUri(vnPayResponse.paymentUrl);
                } else {
                    Toast.makeText(CheckoutActivity.this,
                            "Lỗi: " + vnPayResponse.message,
                            Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<VNPayResponse> call, Throwable t) {
                Toast.makeText(CheckoutActivity.this,
                        "Lỗi mạng khi gọi VNPay: " + t.getMessage(),
                        Toast.LENGTH_LONG).show();
            }
        });
    }


    private void openUri(String uri) {
        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(uri));
        try {
            startActivity(intent);
        } catch (ActivityNotFoundException ex) {
            Toast.makeText(this,
                    "Không tìm thấy ứng dụng thanh toán hoặc trình duyệt phù hợp.",
                    Toast.LENGTH_LONG).show();
        }
    }
}
