package com.example.shoppeclone.api;

public class RegisterResponse {
    public String token;
    public UserInfo user;

    public static class UserInfo {
        public int id;
        public String fullName;
        public String email;
    }
}
