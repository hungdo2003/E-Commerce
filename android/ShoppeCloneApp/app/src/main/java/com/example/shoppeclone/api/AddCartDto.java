package com.example.shoppeclone.api;

public class AddCartDto {
    public int productId;
    public int quantity;
    public AddCartDto(int p, int q){
        productId = p;
        quantity = q;
    }
}


