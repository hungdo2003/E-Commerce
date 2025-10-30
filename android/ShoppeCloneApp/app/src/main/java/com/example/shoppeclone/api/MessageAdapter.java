package com.example.shoppeclone.api;

import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;
import com.example.shoppeclone.R;
import com.example.shoppeclone.api.Message;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;

public class MessageAdapter extends RecyclerView.Adapter<MessageAdapter.MessageViewHolder> {

    private List<Message> messages;
    private SimpleDateFormat timeFormat;

    public MessageAdapter(List<Message> messages) {
        this.messages = messages;
        this.timeFormat = new SimpleDateFormat("HH:mm", Locale.getDefault());
    }

    @NonNull
    @Override
    public MessageViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_message, parent, false);
        return new MessageViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull MessageViewHolder holder, int position) {
        Message message = messages.get(position);
        holder.bind(message);
    }

    @Override
    public int getItemCount() {
        return messages.size();
    }

    public void addMessage(Message message) {
        messages.add(message);
        notifyItemInserted(messages.size() - 1);
    }

    class MessageViewHolder extends RecyclerView.ViewHolder {
        private LinearLayout messageContainer;
        private TextView tvMessage;
        private TextView tvTime;

        public MessageViewHolder(@NonNull View itemView) {
            super(itemView);
            messageContainer = itemView.findViewById(R.id.messageContainer);
            tvMessage = itemView.findViewById(R.id.tvMessage);
            tvTime = itemView.findViewById(R.id.tvTime);
        }

        public void bind(Message message) {
            tvMessage.setText(message.getText());
            tvTime.setText(timeFormat.format(new Date(message.getTimestamp())));

            // Sử dụng FrameLayout.LayoutParams
            FrameLayout.LayoutParams params = (FrameLayout.LayoutParams) messageContainer.getLayoutParams();

            if (message.isUser()) {
                // User message - right aligned, blue background
                params.gravity = Gravity.END;
                tvMessage.setBackgroundResource(R.drawable.bg_user_message);
                tvMessage.setTextColor(ContextCompat.getColor(itemView.getContext(), android.R.color.white));
                tvTime.setGravity(Gravity.END);
            } else {
                // AI message - left aligned, gray background
                params.gravity = Gravity.START;
                tvMessage.setBackgroundResource(R.drawable.bg_ai_message);
                tvMessage.setTextColor(ContextCompat.getColor(itemView.getContext(), android.R.color.black));
                tvTime.setGravity(Gravity.START);
            }

            messageContainer.setLayoutParams(params);
        }}
}