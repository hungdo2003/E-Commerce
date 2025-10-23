package com.example.shoppeclone.ui;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.shoppeclone.R;

import java.util.ArrayList;
import java.util.List;

public class ChatAdapter extends RecyclerView.Adapter<ChatAdapter.VH> {
    static class Item { boolean bot; String text; Item(boolean b, String t){bot=b;text=t;} }
    private final List<Item> data = new ArrayList<>();
    public void addUser(String t){ data.add(new Item(false, t)); notifyDataSetChanged(); }
    public void addBot(String t){ data.add(new Item(true, t)); notifyDataSetChanged(); }
    @NonNull @Override public VH onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        int layout = viewType==1? R.layout.row_chat_bot : R.layout.row_chat_user;
        View v = LayoutInflater.from(parent.getContext()).inflate(layout, parent, false);
        return new VH(v);
    }
    @Override public void onBindViewHolder(@NonNull VH h, int i) { h.text.setText(data.get(i).text); }
    @Override public int getItemCount(){ return data.size(); }
    @Override public int getItemViewType(int position){ return data.get(position).bot?1:0; }
    static class VH extends RecyclerView.ViewHolder {
        TextView text;
        VH(View v){ super(v); text=v.findViewById(R.id.text); }
    }
}
