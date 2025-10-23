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
import com.example.shoppeclone.api.CreatePaymentDto;
import com.example.shoppeclone.api.OrdersApi;
import com.example.shoppeclone.api.PaymentsApi;
import com.example.shoppeclone.api.ZpCreateResponse;
import com.example.shoppeclone.net.ApiClient;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {
    private OrdersApi ordersApi;
    private PaymentsApi paymentsApi;
    private TextView txtTotal;
    private Button btnPay;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        txtTotal = findViewById(R.id.txtTotal);
        btnPay = findViewById(R.id.btnPay);

        ordersApi = ApiClient.get(this).create(OrdersApi.class);
        paymentsApi = ApiClient.get(this).create(PaymentsApi.class);

        btnPay.setOnClickListener(v -> createOrderAndPay());
    }

    private void createOrderAndPay() {
        ordersApi.createFromCart().enqueue(new Callback<CreateOrderResponse>() {
            @Override
            public void onResponse(Call<CreateOrderResponse> call, Response<CreateOrderResponse> response) {
                if (!response.isSuccessful() || response.body() == null) {
                    Toast.makeText(CheckoutActivity.this,
                            "Tạo đơn hàng thất bại: HTTP " + response.code(),
                            Toast.LENGTH_LONG).show();
                    return;
                }

                CreateOrderResponse order = response.body();
                txtTotal.setText(String.format("Tổng tiền: %.0f", order.totalAmount));

                requestZaloPay(order);
            }

            @Override
            public void onFailure(Call<CreateOrderResponse> call, Throwable t) {
                Toast.makeText(CheckoutActivity.this,
                        "Lỗi mạng khi tạo đơn: " + t.getMessage(),
                        Toast.LENGTH_LONG).show();
            }
        });
    }

    private void requestZaloPay(CreateOrderResponse order) {
        String description = "Thanh toán đơn #" + order.orderId;
        paymentsApi.create(new CreatePaymentDto(order.orderId, description))
                .enqueue(new Callback<ZpCreateResponse>() {
                    @Override
                    public void onResponse(Call<ZpCreateResponse> call, Response<ZpCreateResponse> response) {
                        if (!response.isSuccessful() || response.body() == null) {
                            Toast.makeText(CheckoutActivity.this,
                                    "Gọi ZaloPay thất bại: HTTP " + response.code(),
                                    Toast.LENGTH_LONG).show();
                            return;
                        }

                        ZpCreateResponse pay = response.body();
                        if (pay.returnCode != 1) {
                            String message = TextUtils.isEmpty(pay.returnMessage)
                                    ? "Không nhận được mã thành công từ ZaloPay"
                                    : pay.returnMessage;
                            Toast.makeText(CheckoutActivity.this, message, Toast.LENGTH_LONG).show();
                            return;
                        }

                        if (!TextUtils.isEmpty(pay.orderUrl)) {
                            openUri(pay.orderUrl);
                        } else if (!TextUtils.isEmpty(pay.zpTransToken)) {
                            openUri("zalopay://app?zp_trans_token=" + pay.zpTransToken);
                        } else {
                            Toast.makeText(CheckoutActivity.this,
                                    "ZaloPay không trả về orderUrl hoặc zpTransToken.",
                                    Toast.LENGTH_LONG).show();
                        }
                    }

                    @Override
                    public void onFailure(Call<ZpCreateResponse> call, Throwable t) {
                        Toast.makeText(CheckoutActivity.this,
                                "Lỗi mạng khi gọi ZaloPay: " + t.getMessage(),
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
                    "Không tìm thấy ứng dụng ZaloPay hoặc trình duyệt phù hợp.",
                    Toast.LENGTH_LONG).show();
        }
    }
}
