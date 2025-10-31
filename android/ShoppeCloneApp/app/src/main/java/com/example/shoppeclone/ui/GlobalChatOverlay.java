package com.example.shoppeclone.ui;

import android.app.Activity;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import com.example.shoppeclone.R;

public class GlobalChatOverlay {

    private final Activity activity;
    private View chatButtonView;

    public GlobalChatOverlay(Activity activity) {
        this.activity = activity;
    }

    /** Gọi trong onResume() của mỗi Activity để hiển thị nút chat toàn cục */
    public void attach() {
        if (chatButtonView != null) return; // tránh gắn trùng

        ViewGroup root = activity.findViewById(android.R.id.content);
        chatButtonView = LayoutInflater.from(activity).inflate(R.layout.widget_chat_button, root, false);
        root.addView(chatButtonView);

        View fab = chatButtonView.findViewById(R.id.fabGlobalChat);
        fab.setOnClickListener(v -> {
            Intent i = new Intent(activity, ChatTestActivity.class);
            activity.startActivity(i);
        });
    }

    /** Gọi trong onPause() nếu muốn ẩn khi rời Activity */
    public void detach() {
        if (chatButtonView != null) {
            ViewGroup root = activity.findViewById(android.R.id.content);
            root.removeView(chatButtonView);
            chatButtonView = null;
        }
    }
}
