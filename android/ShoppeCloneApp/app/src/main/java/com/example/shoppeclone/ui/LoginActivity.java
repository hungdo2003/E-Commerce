package com.example.shoppeclone.ui;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
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
    @Override protected void onCreate(Bundle b){
        super.onCreate(b);
        setContentView(R.layout.activity_login);

        EditText edtEmail = findViewById(R.id.edtEmail);
        EditText edtPass  = findViewById(R.id.edtPassword);
        Button btnLogin   = findViewById(R.id.btnLogin);

        AuthApi api = ApiClient.get(this).create(AuthApi.class);

        btnLogin.setOnClickListener(v -> {
            String email = edtEmail.getText().toString().trim();
            String pass  = edtPass.getText().toString();
            if(email.isEmpty() || pass.isEmpty()){
                Toast.makeText(this, "Nhập email và mật khẩu", Toast.LENGTH_SHORT).show();
                return;
            }
            api.login(new LoginDto(email, pass)).enqueue(new Callback<LoginResponse>() {
                @Override public void onResponse(Call<LoginResponse> call, Response<LoginResponse> rsp) {
                    if(!rsp.isSuccessful() || rsp.body()==null){
                        Toast.makeText(LoginActivity.this, "Đăng nhập thất bại: " + rsp.code(), Toast.LENGTH_LONG).show();
                        return;
                    }
                    SessionManager.saveToken(LoginActivity.this, rsp.body().token);
                    Toast.makeText(LoginActivity.this, "Đăng nhập thành công", Toast.LENGTH_SHORT).show();
                    // quay lại trang trước (ProductList/Cart)
                    finish();
                }
                @Override public void onFailure(Call<LoginResponse> call, Throwable t) {
                    Toast.makeText(LoginActivity.this, "Lỗi mạng: " + t.getMessage(), Toast.LENGTH_LONG).show();
                }
            });
        });
    }
}
