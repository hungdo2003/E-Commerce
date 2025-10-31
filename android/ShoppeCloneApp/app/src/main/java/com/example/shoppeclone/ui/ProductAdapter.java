package com.example.shoppeclone.ui;

import android.graphics.Paint;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.shoppeclone.R;
import com.example.shoppeclone.api.ProductItem;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class ProductAdapter extends RecyclerView.Adapter<ProductAdapter.VH> {

    // 🔹 Giao diện callback cho fragment
    public interface OnItemAction {
        void onItemClick(ProductItem p); // click cả card để mở detail
    }

    private List<ProductItem> data = new ArrayList<>();
    private OnItemAction listener;
    private Random random = new Random();

    public void setListener(OnItemAction l) {
        this.listener = l;
    }

    public void submit(List<ProductItem> items) {
        data = (items == null) ? new ArrayList<>() : items;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public VH onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View v = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.row_product, parent, false);
        return new VH(v);
    }

    @Override
    public void onBindViewHolder(@NonNull VH h, int i) {
        ProductItem p = data.get(i);
        
        // Tên sản phẩm
        h.name.setText(p.name);
        
        // Giá hiện tại
        h.price.setText(String.format("₫%,.0f", p.price));
        
        // Hình ảnh
        Glide.with(h.img.getContext())
                .load(p.thumbnailUrl)
                .placeholder(R.drawable.ic_placeholder)
                .into(h.img);

        // Rating giả (4.0 - 5.0)
        double rating = 4.0 + random.nextDouble();
        h.rating.setText(String.format("%.1f", rating));

        // Số lượng đã bán giả
        int sold = random.nextInt(1000) + 50;
        if (sold > 999) {
            h.sold.setText("Đã bán 999+");
        } else {
            h.sold.setText("Đã bán " + sold);
        }

        // Hiển thị giá gốc nếu có giảm giá (30% ngẫu nhiên)
        if (random.nextInt(10) < 3) {
            h.originalPrice.setVisibility(View.VISIBLE);
            h.badgeSale.setVisibility(View.VISIBLE);
            double originalPrice = p.price * (1.3 + random.nextDouble() * 0.5);
            h.originalPrice.setText(String.format("₫%,.0f", originalPrice));
            h.originalPrice.setPaintFlags(h.originalPrice.getPaintFlags() | Paint.STRIKE_THRU_TEXT_FLAG);
        } else {
            h.originalPrice.setVisibility(View.GONE);
            h.badgeSale.setVisibility(View.GONE);
        }

        // Click toàn card để mở chi tiết
        h.itemView.setOnClickListener(v -> {
            if (listener != null) listener.onItemClick(p);
        });
    }

    @Override
    public int getItemCount() {
        return data.size();
    }

    static class VH extends RecyclerView.ViewHolder {
        ImageView img;
        TextView name, price, originalPrice, rating, sold, badgeSale, stock;

        VH(View v) {
            super(v);
            img = v.findViewById(R.id.img);
            name = v.findViewById(R.id.name);
            price = v.findViewById(R.id.price);
            originalPrice = v.findViewById(R.id.originalPrice);
            rating = v.findViewById(R.id.rating);
            sold = v.findViewById(R.id.sold);
            badgeSale = v.findViewById(R.id.badgeSale);
            stock = v.findViewById(R.id.stock);
        }
    }
}
