package com.example.shoppeclone.api;

public class CartItem {
    public int id;
    public int productId;
    public int quantity;
    public String name;          // từ BE: Product.Name
    public double price;         // từ BE: Product.Price
    public String thumbnailUrl;  // từ BE: Product.ThumbnailUrl
}
