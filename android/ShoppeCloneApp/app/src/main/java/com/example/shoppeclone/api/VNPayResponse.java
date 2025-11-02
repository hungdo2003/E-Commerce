package com.example.shoppeclone.api;

public class VNPayResponse {
    // 🔥 SỬA: Backend chỉ trả về paymentUrl, không có success/message
    public String paymentUrl;

    // Có thể thêm constructor nếu cần
    public VNPayResponse() {}

    public VNPayResponse(String paymentUrl) {
        this.paymentUrl = paymentUrl;
    }
}