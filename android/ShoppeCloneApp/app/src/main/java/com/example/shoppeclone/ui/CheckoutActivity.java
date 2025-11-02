package com.example.shoppeclone.ui;

import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.text.TextUtils;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.CreateOrderResponse;
import com.example.shoppeclone.api.OrdersApi;
import com.example.shoppeclone.api.VNPayApi;
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

        // Xử lý deep link khi activity được mở từ VNPay callback
        handleVNPayReturn(getIntent());
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        handleVNPayReturn(intent);
    }

    private void createOrderAndPay() {
        ordersApi.createFromCart().enqueue(new Callback<CreateOrderResponse>() {
            @Override
            public void onResponse(Call<CreateOrderResponse> call, Response<CreateOrderResponse> response) {
                if (!response.isSuccessful()) {
                    String errorMsg = "Tạo đơn hàng thất bại";

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
        // 🔥 SỬA: Gọi đúng endpoint với orderId trong path
        vnPayApi.createPayment(order.orderId).enqueue(new Callback<VNPayResponse>() {
            @Override
            public void onResponse(Call<VNPayResponse> call, Response<VNPayResponse> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "Gọi VNPay thất bại: HTTP " + response.code(),
                            Toast.LENGTH_LONG).show();
                    return;
                }

                VNPayResponse vnPayResponse = response.body();

                // 🔥 SỬA: Backend trả về paymentUrl trực tiếp, không có success field
                if (!TextUtils.isEmpty(vnPayResponse.paymentUrl)) {
                    // THÊM PARAMETER RETURN URL VÀO URL VNPAY
                    String returnUrl = "shoppeclone://vnpay-return";
                    String paymentUrlWithCallback = vnPayResponse.paymentUrl + "&custom_return_url=" + Uri.encode(returnUrl);

                    openUri(paymentUrlWithCallback);
                } else {
                    Toast.makeText(CheckoutActivity.this,
                            "Lỗi: Không nhận được payment URL",
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

    /**
     * Xử lý kết quả trả về từ VNPay thông qua deep link
     */
    private void handleVNPayReturn(Intent intent) {
        if (intent != null && intent.getData() != null) {
            Uri uri = intent.getData();
            Log.d("VNPay", "Received deep link: " + uri.toString());

            // Xử lý custom scheme
            if ("shoppeclone".equals(uri.getScheme()) && "vnpay-return".equals(uri.getHost())) {
                processPaymentResult(uri);
            }
            // Xử lý ngrok redirect
            else if ("https".equals(uri.getScheme()) &&
                    "moira-subjugular-anna.ngrok-free.dev".equals(uri.getHost()) &&
                    uri.getPath() != null && uri.getPath().startsWith("/api/VNPay/return")) {
                processPaymentResult(uri);
            }
        }
    }

    /**
     * Xử lý kết quả thanh toán từ query parameters
     */
    private void processPaymentResult(Uri uri) {
        String responseCode = uri.getQueryParameter("vnp_ResponseCode");
        String transactionStatus = uri.getQueryParameter("vnp_TransactionStatus");
        String orderId = uri.getQueryParameter("vnp_TxnRef");

        if ("00".equals(responseCode) && "00".equals(transactionStatus)) {
            // Thanh toán thành công
            Toast.makeText(this, "✅ Thanh toán thành công! Đơn hàng #" + orderId, Toast.LENGTH_LONG).show();

            // Chuyển về màn hình chính hoặc hiển thị kết quả
            Intent resultIntent = new Intent(this, MainActivity.class);
            resultIntent.putExtra("payment_success", true);
            resultIntent.putExtra("order_id", orderId);
            resultIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
            startActivity(resultIntent);
            finish();
        } else {
            // Thanh toán thất bại
            String errorMsg = "Thanh toán thất bại";
            if (responseCode != null) {
                errorMsg += " (Mã lỗi: " + responseCode + ")";
            }
            Toast.makeText(this, "❌ " + errorMsg, Toast.LENGTH_LONG).show();

            // Quay lại màn hình thanh toán
            btnPay.setEnabled(true);
        }
    }
}