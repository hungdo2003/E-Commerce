package com.example.shoppeclone.api;

public class CreatePaymentDto {
    public int orderId;
    public String description;
    public CreatePaymentDto(int orderId, String description){
        this.orderId = orderId; this.description = description;
    }
}
