package com.example.shoppeclone.ui;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.shoppeclone.R;
import com.example.shoppeclone.api.CartItem;

import java.util.ArrayList;
import java.util.List;

public class CartAdapter extends RecyclerView.Adapter<CartAdapter.VH> {

    public interface OnAction {
        void onRemove(CartItem item);
        void onQuantityChanged(CartItem item, int newQty);
    }

    private final List<CartItem> data = new ArrayList<>();
    private OnAction listener;

    public void setListener(OnAction l) { this.listener = l; }

    public void submit(List<CartItem> items) {
        data.clear();
        if (items != null) data.addAll(items);
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public VH onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View v = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.row_cart_item, parent, false);
        return new VH(v);
    }

    @Override
    public void onBindViewHolder(@NonNull VH h, int i) {
        CartItem it = data.get(i);

        Glide.with(h.img.getContext()).load(it.thumbnailUrl).into(h.img);
        h.name.setText(it.name);
        h.price.setText(String.format("%,.0f₫", it.price));
        h.qty.setText(String.valueOf(it.quantity));

        // ➖ Giảm số lượng
        h.btnMinus.setOnClickListener(v -> {
            if (it.quantity > 1) {
                it.quantity--;
                h.qty.setText(String.valueOf(it.quantity));
                if (listener != null) listener.onQuantityChanged(it, it.quantity);
            }
        });

        // ➕ Tăng số lượng
        h.btnPlus.setOnClickListener(v -> {
            it.quantity++;
            h.qty.setText(String.valueOf(it.quantity));
            if (listener != null) listener.onQuantityChanged(it, it.quantity);
        });

        // 🗑 Xóa sản phẩm
        h.btnRemove.setOnClickListener(v -> {
            if (listener != null) listener.onRemove(it);
        });
    }

    @Override
    public int getItemCount() { return data.size(); }

    static class VH extends RecyclerView.ViewHolder {
        ImageView img;
        TextView name, price, qty;
        ImageButton btnMinus, btnPlus, btnRemove; // ✅ dùng ImageButton đúng với XML

        VH(View v) {
            super(v);
            img = v.findViewById(R.id.img);
            name = v.findViewById(R.id.name);
            price = v.findViewById(R.id.price);
            qty = v.findViewById(R.id.qty);
            btnMinus = v.findViewById(R.id.btnMinus);
            btnPlus = v.findViewById(R.id.btnPlus);
            btnRemove = v.findViewById(R.id.btnRemove);
        }
    }
}
