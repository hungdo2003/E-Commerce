package com.example.shoppeclone.api;

public class Product {
    public int id;
    public String name;
    public String description;
    public String thumbnailUrl;
    public double price;
    public int stock;
    public int categoryId;   // 🔹 Thêm để map với "categoryId"
    public String createdAt; // 🔹 Thêm để map với "createdAt"
}
