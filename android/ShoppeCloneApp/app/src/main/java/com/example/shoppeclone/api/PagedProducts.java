package com.example.shoppeclone.api;

import java.util.List;

public class PagedProducts {
    public int total;
    public int page;
    public int size;
    public List<ProductItem> items;
}
