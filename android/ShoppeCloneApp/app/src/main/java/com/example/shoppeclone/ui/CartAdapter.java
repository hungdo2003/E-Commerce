package com.example.shoppeclone.ui;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
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
    }

    private final List<CartItem> data = new ArrayList<>();
    private OnAction listener;

    public void setListener(OnAction l){ this.listener = l; }

    public void submit(List<CartItem> items){
        data.clear();
        if(items != null) data.addAll(items);
        notifyDataSetChanged();
    }

    @NonNull @Override public VH onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View v = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.row_cart_item, parent, false);
        return new VH(v);
    }

    @Override public void onBindViewHolder(@NonNull VH h, int i) {
        CartItem it = data.get(i);
        h.name.setText(it.name);
        h.qty.setText("x" + it.quantity);
        h.price.setText(String.valueOf(it.price));
        Glide.with(h.img.getContext()).load(it.thumbnailUrl).into(h.img);

        h.btnRemove.setOnClickListener(v -> {
            if (listener != null) listener.onRemove(it);
        });
    }

    @Override public int getItemCount() { return data.size(); }

    static class VH extends RecyclerView.ViewHolder {
        ImageView img; TextView name; TextView qty; TextView price; Button btnRemove;
        VH(View v){
            super(v);
            img = v.findViewById(R.id.img);
            name = v.findViewById(R.id.name);
            qty = v.findViewById(R.id.qty);
            price = v.findViewById(R.id.price);
            btnRemove = v.findViewById(R.id.btnRemove);
        }
    }
}
