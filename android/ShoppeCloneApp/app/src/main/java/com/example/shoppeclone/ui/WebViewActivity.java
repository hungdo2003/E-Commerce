package com.example.shoppeclone.ui;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.shoppeclone.R;

public class WebViewActivity extends AppCompatActivity {
    private WebView webView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_webview);

        webView = findViewById(R.id.webView);
        webView.getSettings().setJavaScriptEnabled(true);
        webView.getSettings().setDomStorageEnabled(true);

        // Xử lý khi WebView load URL
        webView.setWebViewClient(new WebViewClient() {
            @Override
            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                // Nếu là URL return từ VNPay, xử lý kết quả
                if (url.contains("/api/VNPay/return") || url.contains("/api/VNPay/deeplink")) {
                    // PHÂN TÍCH URL VÀ CHUYỂN THÔNG TIN THÀNH CÔNG
                    boolean isSuccess = checkPaymentSuccess(url);

                    // Tạo intent với thông tin thành công
                    Intent resultIntent = new Intent(WebViewActivity.this, CheckoutActivity.class);
                    resultIntent.putExtra("payment_success", isSuccess);
                    resultIntent.putExtra("from_webview", true);

                    // Trích xuất orderId từ URL
                    String orderId = extractOrderIdFromUrl(url);
                    if (orderId != null) {
                        resultIntent.putExtra("order_id", orderId);
                    }

                    startActivity(resultIntent);
                    finish();
                    return true;
                }

                // Cho phép WebView load các URL khác
                return false;
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);

                // Nếu là trang deeplink, tự động xử lý sau 2 giây
                if (url.contains("/api/VNPay/deeplink")) {
                    webView.postDelayed(() -> {
                        boolean isSuccess = checkPaymentSuccess(url);

                        Intent resultIntent = new Intent(WebViewActivity.this, CheckoutActivity.class);
                        resultIntent.putExtra("payment_success", isSuccess);
                        resultIntent.putExtra("from_webview", true);

                        String orderId = extractOrderIdFromUrl(url);
                        if (orderId != null) {
                            resultIntent.putExtra("order_id", orderId);
                        }

                        startActivity(resultIntent);
                        finish();
                    }, 2000);
                }
            }
        });

        // Load payment URL
        String paymentUrl = getIntent().getStringExtra("payment_url");
        if (paymentUrl != null) {
            webView.loadUrl(paymentUrl);
        } else {
            Toast.makeText(this, "Lỗi: Không có URL thanh toán", Toast.LENGTH_SHORT).show();
            finish();
        }
    }

    private boolean checkPaymentSuccess(String url) {
        // Kiểm tra nếu URL chứa thông tin thành công
        return url.contains("vnp_ResponseCode=00") ||
                url.contains("success=true") ||
                url.contains("/api/VNPay/deeplink?success=true");
    }

    private String extractOrderIdFromUrl(String url) {
        try {
            Uri uri = Uri.parse(url);

            // Thử lấy từ nhiều parameter khác nhau
            String orderId = uri.getQueryParameter("orderId");
            if (orderId == null) {
                orderId = uri.getQueryParameter("vnp_TxnRef");
            }
            if (orderId == null) {
                // Trích xuất từ orderInfo
                String orderInfo = uri.getQueryParameter("vnp_OrderInfo");
                if (orderInfo != null && orderInfo.contains("#")) {
                    String[] parts = orderInfo.split("#");
                    if (parts.length > 1) {
                        orderId = parts[1].replaceAll("[^0-9]", "");
                    }
                }
            }

            return orderId;
        } catch (Exception e) {
            return "unknown";
        }
    }

    @Override
    public void onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack();
        } else {
            // Nếu user back, coi như thanh toán thất bại
            Intent resultIntent = new Intent(this, CheckoutActivity.class);
            resultIntent.putExtra("payment_success", false);
            resultIntent.putExtra("from_webview", true);
            startActivity(resultIntent);
            finish();
        }
    }
}