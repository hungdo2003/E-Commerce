package com.example.shoppeclone.ui;

import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;

import com.example.shoppeclone.R;
import com.example.shoppeclone.ui.fragment.HomeFragment;
import com.example.shoppeclone.ui.fragment.ProfileFragment;
import com.example.shoppeclone.ui.fragment.ProductListFragment;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.navigation.NavigationBarView;

public class MainActivity extends AppCompatActivity {

    private GlobalChatOverlay chatOverlay;
    private boolean shouldShowChat = true; // Biến để kiểm soát hiển thị chat AI

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        BottomNavigationView bottomNav = findViewById(R.id.bottom_navigation);

        // 🏠 Mặc định hiển thị Trang chủ khi mở app
        loadFragment(new HomeFragment(), true);

        // 🔹 Dùng listener dạng truyền thống thay vì lambda để tránh lỗi Java 8
        bottomNav.setOnItemSelectedListener(new NavigationBarView.OnItemSelectedListener() {
            @Override
            public boolean onNavigationItemSelected(@NonNull android.view.MenuItem item) {
                Fragment selectedFragment = null;
                boolean showChat = true;
                int itemId = item.getItemId();

                if (itemId == R.id.nav_home) {
                    selectedFragment = new HomeFragment();
                    showChat = true; // Hiển thị chat AI ở trang chủ
                } else if (itemId == R.id.nav_products) {
                    selectedFragment = new ProductListFragment();
                    showChat = true; // Hiển thị chat AI ở trang sản phẩm
                } else if (itemId == R.id.nav_profile) {
                    selectedFragment = new ProfileFragment();
                    showChat = false; // Ẩn chat AI ở trang tài khoản
                }


                if (selectedFragment != null) {
                    loadFragment(selectedFragment, showChat);
                }

                return true;
            }
        });
    }

    private void loadFragment(Fragment fragment, boolean showChat) {
        getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.frame_container, fragment)
                .commit();
        
        // Cập nhật trạng thái hiển thị chat AI
        shouldShowChat = showChat;
        updateChatVisibility();
    }

    private void updateChatVisibility() {
        if (chatOverlay == null) {
            chatOverlay = new GlobalChatOverlay(this);
        }

        if (shouldShowChat) {
            chatOverlay.attach();
        } else {
            chatOverlay.detach();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        updateChatVisibility();
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (chatOverlay != null) chatOverlay.detach();
    }
}
