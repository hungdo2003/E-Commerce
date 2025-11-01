package com.example.shoppeclone.api;

public class VNPayRequest {
    public int orderId;
    public String description;

    public VNPayRequest(int orderId, String description) {
        this.orderId = orderId;
        this.description = description;
    }
}



