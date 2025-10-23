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
import com.example.shoppeclone.api.ProductItem;

import java.util.ArrayList;
import java.util.List;

public class ProductAdapter extends RecyclerView.Adapter<ProductAdapter.VH> {
    public interface OnItemAction { void onAddToCart(ProductItem p); }
    private List<ProductItem> data = new ArrayList<>();
    private OnItemAction listener;
    public void setListener(OnItemAction l){ this.listener = l; }
    public void submit(List<ProductItem> items){ data = items==null? new ArrayList<>() : items; notifyDataSetChanged(); }
    @NonNull @Override public VH onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View v = LayoutInflater.from(parent.getContext()).inflate(R.layout.row_product, parent, false);
        return new VH(v);
    }
    @Override public void onBindViewHolder(@NonNull VH h, int i) {
        ProductItem p = data.get(i);
        h.name.setText(p.name);
        h.price.setText(String.valueOf(p.price));
        Glide.with(h.img.getContext()).load(p.thumbnailUrl).into(h.img);
        h.btnAdd.setOnClickListener(v -> { if(listener!=null) listener.onAddToCart(p); });
    }
    @Override public int getItemCount() { return data.size(); }
    static class VH extends RecyclerView.ViewHolder {
        ImageView img; TextView name; TextView price; Button btnAdd;
        VH(View v){ super(v); img=v.findViewById(R.id.img); name=v.findViewById(R.id.name); price=v.findViewById(R.id.price); btnAdd=v.findViewById(R.id.btnAdd); }
    }
}
