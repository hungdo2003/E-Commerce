package com.example.shoppeclone.api;

public class RegisterDto {
    public String fullName;
    public String email;
    public String password;
    public String phone;

    public RegisterDto(String fullName, String email, String password, String phone) {
        this.fullName = fullName;
        this.email = email;
        this.password = password;
        this.phone = phone;
    }
}
