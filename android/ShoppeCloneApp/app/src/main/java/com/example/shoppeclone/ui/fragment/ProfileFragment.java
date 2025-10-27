package com.example.shoppeclone.ui.fragment;

import android.app.AlertDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import com.example.shoppeclone.R;
import com.example.shoppeclone.net.ApiClient;
import com.example.shoppeclone.net.SessionManager;
import com.example.shoppeclone.ui.LoginActivity;

public class ProfileFragment extends Fragment {

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_profile, container, false);

        Button btnLogout = view.findViewById(R.id.btnLogout);
        btnLogout.setOnClickListener(v -> showLogoutConfirmDialog());

        return view;
    }

    /** ⚙️ Hiển thị hộp thoại xác nhận đăng xuất */
    private void showLogoutConfirmDialog() {
        new AlertDialog.Builder(requireContext())
                .setTitle("Đăng xuất")
                .setMessage("Bạn có chắc muốn đăng xuất khỏi tài khoản này?")
                .setPositiveButton("Có", (dialog, which) -> performLogout())
                .setNegativeButton("Hủy", null)
                .show();
    }

    /** 🧹 Xử lý đăng xuất thật sự */
    private void performLogout() {
        // ✅ Xóa token
        SessionManager.clear(requireContext());

        // ✅ Reset Retrofit client để ngắt token đang lưu trong header
        try {
            java.lang.reflect.Field f = com.example.shoppeclone.net.ApiClient.class.getDeclaredField("retrofit");
            f.setAccessible(true);
            f.set(null, null); // reset instance về null
        } catch (Exception ignored) {}

        Toast.makeText(requireContext(), "👋 Đăng xuất thành công!", Toast.LENGTH_SHORT).show();

        // 👉 Chuyển về LoginActivity và xoá toàn bộ stack
        Intent i = new Intent(requireContext(), LoginActivity.class);
        i.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(i);
    }
}
