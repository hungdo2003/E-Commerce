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
    @Override
    protected void onResume() {
        super.onResume();

        // Kiểm tra nếu có kết quả từ WebViewActivity
        handleWebViewResult();
    }

    private void handleWebViewResult() {
        Intent intent = getIntent();
        if (intent != null && intent.hasExtra("from_webview")) {
            boolean success = intent.getBooleanExtra("payment_success", false);
            String orderId = intent.getStringExtra("order_id");

            if (success) {
                Toast.makeText(this, "✅ Thanh toán thành công! Đơn hàng #" + orderId, Toast.LENGTH_LONG).show();

                // Chuyển về MainActivity
                Intent resultIntent = new Intent(this, MainActivity.class);
                resultIntent.putExtra("payment_success", true);
                if (orderId != null) {
                    resultIntent.putExtra("order_id", orderId);
                }
                resultIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                startActivity(resultIntent);
                finish();
            } else {
                Toast.makeText(this, "❌ Thanh toán thất bại hoặc bị hủy", Toast.LENGTH_LONG).show();
                btnPay.setEnabled(true);
                btnPay.setText("Thanh toán ngay");
            }

            // Reset intent để tránh xử lý nhiều lần
            setIntent(null);
        }
    }

    private void createOrderAndPay() {
        // Vô hiệu hóa nút để tránh click nhiều lần
        btnPay.setEnabled(false);
        btnPay.setText("Đang xử lý...");

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
                    btnPay.setEnabled(true);
                    btnPay.setText("Thanh toán ngay");
                    return;
                }

                if (response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "⚠️ Không nhận được thông tin đơn hàng",
                            Toast.LENGTH_LONG).show();
                    btnPay.setEnabled(true);
                    btnPay.setText("Thanh toán ngay");
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
                btnPay.setEnabled(true);
                btnPay.setText("Thanh toán ngay");
            }
        });
    }

    private void requestVNPay(CreateOrderResponse order) {
        vnPayApi.createPayment(order.orderId).enqueue(new Callback<VNPayResponse>() {
            @Override
            public void onResponse(Call<VNPayResponse> call, Response<VNPayResponse> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "Gọi VNPay thất bại: HTTP " + response.code(),
                            Toast.LENGTH_LONG).show();
                    btnPay.setEnabled(true);
                    btnPay.setText("Thanh toán ngay");
                    return;
                }

                VNPayResponse vnPayResponse = response.body();

                if (vnPayResponse.success && !TextUtils.isEmpty(vnPayResponse.paymentUrl)) {
                    // Mở trình duyệt với payment URL
                    openUri(vnPayResponse.paymentUrl);

                    // Hiển thị thông báo chờ
                    Toast.makeText(CheckoutActivity.this,
                            "Đang chuyển hướng đến VNPay...",
                            Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(CheckoutActivity.this,
                            "Lỗi: " + vnPayResponse.message,
                            Toast.LENGTH_LONG).show();
                    btnPay.setEnabled(true);
                    btnPay.setText("Thanh toán ngay");
                }
            }

            @Override
            public void onFailure(Call<VNPayResponse> call, Throwable t) {
                Toast.makeText(CheckoutActivity.this,
                        "Lỗi mạng khi gọi VNPay: " + t.getMessage(),
                        Toast.LENGTH_LONG).show();
                btnPay.setEnabled(true);
                btnPay.setText("Thanh toán ngay");
            }
        });
    }

    private void openUri(String uri) {
        // Dùng WebView thay vì Browser
        Intent webViewIntent = new Intent(this, WebViewActivity.class);
        webViewIntent.putExtra("payment_url", uri);
        startActivity(webViewIntent);

        // Vô hiệu hóa nút cho đến khi có kết quả
        btnPay.setEnabled(false);
        btnPay.setText("Đang xử lý...");
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
            // Xử lý https deeplink
            else if ("https".equals(uri.getScheme()) &&
                    "moira-subjugular-anna.ngrok-free.dev".equals(uri.getHost()) &&
                    uri.getPath() != null && uri.getPath().startsWith("/api/VNPay/deeplink")) {

                // Lấy parameters từ https deeplink
                boolean success = "true".equals(uri.getQueryParameter("success"));
                String orderId = uri.getQueryParameter("orderId");
                String amount = uri.getQueryParameter("amount");
                String transactionId = uri.getQueryParameter("transactionId");

                if (success) {
                    Toast.makeText(this, "✅ Thanh toán thành công! Đơn hàng #" + orderId, Toast.LENGTH_LONG).show();

                    Intent resultIntent = new Intent(this, MainActivity.class);
                    resultIntent.putExtra("payment_success", true);
                    resultIntent.putExtra("order_id", orderId);
                    resultIntent.putExtra("amount", amount);
                    resultIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                    startActivity(resultIntent);
                    finish();
                } else {
                    Toast.makeText(this, "❌ Thanh toán thất bại!", Toast.LENGTH_LONG).show();
                    btnPay.setEnabled(true);
                    btnPay.setText("Thanh toán ngay");
                }
            }
        }
    }

    /**
     * Xử lý kết quả thanh toán từ query parameters
     */
    private void processPaymentResult(Uri uri) {
        String responseCode = uri.getQueryParameter("vnp_ResponseCode");
        String orderInfo = uri.getQueryParameter("vnp_OrderInfo");
        String transactionNo = uri.getQueryParameter("vnp_TransactionNo");
        String amount = uri.getQueryParameter("vnp_Amount");

        // Extract orderId từ orderInfo hoặc transaction reference
        String orderId = extractOrderIdFromOrderInfo(orderInfo);
        if (orderId == null) {
            orderId = uri.getQueryParameter("vnp_TxnRef");
        }

        Log.d("VNPay", "Payment result - ResponseCode: " + responseCode + ", OrderId: " + orderId);

        if ("00".equals(responseCode)) {
            // Thanh toán thành công
            String successMsg = "✅ Thanh toán thành công!";
            if (orderId != null) {
                successMsg += " Đơn hàng #" + orderId;
            }
            Toast.makeText(this, successMsg, Toast.LENGTH_LONG).show();

            // Chuyển về màn hình chính với thông báo thành công
            Intent resultIntent = new Intent(this, MainActivity.class);
            resultIntent.putExtra("payment_success", true);
            if (orderId != null) {
                resultIntent.putExtra("order_id", orderId);
            }
            resultIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
            startActivity(resultIntent);
            finish();
        } else {
            // Thanh toán thất bại
            String errorMsg = getVNPayErrorDescription(responseCode);
            Toast.makeText(this, "❌ " + errorMsg, Toast.LENGTH_LONG).show();

            // Quay lại màn hình thanh toán
            btnPay.setEnabled(true);
            btnPay.setText("Thanh toán ngay");
        }
    }

    /**
     * Trích xuất orderId từ orderInfo (ví dụ: "Thanh toán đơn hàng #15")
     */
    private String extractOrderIdFromOrderInfo(String orderInfo) {
        if (orderInfo != null && orderInfo.contains("#")) {
            String[] parts = orderInfo.split("#");
            if (parts.length > 1) {
                return parts[1].replaceAll("[^0-9]", "");
            }
        }
        return null;
    }

    /**
     * Lấy mô tả lỗi từ response code của VNPay
     */
    private String getVNPayErrorDescription(String responseCode) {
        if (responseCode == null) return "Thanh toán thất bại";

        switch (responseCode) {
            case "07": return "Giao dịch bị nghi ngờ (gian lận)";
            case "09": return "Thẻ/Tài khoản chưa đăng ký dịch vụ Internet Banking";
            case "10": return "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần";
            case "11": return "Đã hết hạn chờ thanh toán. Xin quý khách thực hiện lại giao dịch";
            case "12": return "Thẻ/Tài khoản của khách hàng bị khóa";
            case "13": return "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)";
            case "24": return "Khách hàng hủy giao dịch";
            case "51": return "Tài khoản không đủ số dư để thực hiện giao dịch";
            case "65": return "Tài khoản đã vượt quá hạn mức giao dịch trong ngày";
            case "75": return "Ngân hàng thanh toán đang bảo trì";
            case "79": return "KH nhập sai mật khẩu thanh toán quá số lần quy định";
            case "99": return "Các lỗi khác";
            default: return "Thanh toán thất bại (Mã lỗi: " + responseCode + ")";
        }
    }
}