package com.example.shoppeclone.ui;

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

public class ProductAdapter extends RecyclerView.Adapter<ProductAdapter.VH> {

    // 🔹 Giao diện callback cho fragment
    public interface OnItemAction {
        void onItemClick(ProductItem p); // click cả card để mở detail
    }

    private List<ProductItem> data = new ArrayList<>();
    private OnItemAction listener;

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
        h.name.setText(p.name);
        h.price.setText(String.format("%,.0f₫", p.price));
        Glide.with(h.img.getContext())
                .load(p.thumbnailUrl)
                .placeholder(R.drawable.ic_placeholder)
                .into(h.img);

        // ✅ Click toàn card để mở chi tiết
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
        TextView name, price;

        VH(View v) {
            super(v);
            img = v.findViewById(R.id.img);
            name = v.findViewById(R.id.name);
            price = v.findViewById(R.id.price);
        }
    }
}
