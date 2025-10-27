package com.example.shoppeclone.ui;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.AuthApi;
import com.example.shoppeclone.api.LoginDto;
import com.example.shoppeclone.api.LoginResponse;
import com.example.shoppeclone.net.ApiClient;
import com.example.shoppeclone.net.SessionManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        // 🔹 Nếu đã có token thì bỏ qua màn login, vào thẳng ProductListActivity
        if (SessionManager.getToken(this) != null) {
            startActivity(new Intent(this, MainActivity.class));
            finish();
            return;
        }

        EditText edtEmail = findViewById(R.id.edtEmail);
        EditText edtPass = findViewById(R.id.edtPassword);
        Button btnLogin = findViewById(R.id.btnLogin);
        TextView tvGoRegister = findViewById(R.id.tvGoRegister); // 🔹 TextView “Đăng ký ngay”

        AuthApi api = ApiClient.get(this).create(AuthApi.class);

        // 🔹 Xử lý khi bấm "Đăng nhập"
        btnLogin.setOnClickListener(v -> {
            String email = edtEmail.getText().toString().trim();
            String pass = edtPass.getText().toString();

            if (email.isEmpty() || pass.isEmpty()) {
                Toast.makeText(this, "Nhập email và mật khẩu", Toast.LENGTH_SHORT).show();
                return;
            }

            api.login(new LoginDto(email, pass)).enqueue(new Callback<LoginResponse>() {
                @Override
                public void onResponse(Call<LoginResponse> call, Response<LoginResponse> rsp) {
                    if (!rsp.isSuccessful() || rsp.body() == null) {
                        Toast.makeText(LoginActivity.this, "Đăng nhập thất bại: " + rsp.code(), Toast.LENGTH_LONG).show();
                        return;
                    }

                    // 🔹 Lưu token và chuyển sang màn hình chính
                    SessionManager.saveToken(LoginActivity.this, rsp.body().token);
                    Toast.makeText(LoginActivity.this, "Đăng nhập thành công", Toast.LENGTH_SHORT).show();

                    Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                    startActivity(intent);
                    finish();
                }

                @Override
                public void onFailure(Call<LoginResponse> call, Throwable t) {
                    Toast.makeText(LoginActivity.this, "Lỗi mạng: " + t.getMessage(), Toast.LENGTH_LONG).show();
                }
            });
        });

        // 🔹 Xử lý khi bấm "Chưa có tài khoản? Đăng ký ngay"
        tvGoRegister.setOnClickListener(v -> {
            Intent intent = new Intent(LoginActivity.this, RegisterActivity.class);
            startActivity(intent);
            overridePendingTransition(android.R.anim.fade_in, android.R.anim.fade_out); // hiệu ứng mượt
        });
    }
}
