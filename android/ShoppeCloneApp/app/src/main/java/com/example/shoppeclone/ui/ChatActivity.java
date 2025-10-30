//package com.example.shoppeclone.ui;
//
//import android.os.Bundle;
//import android.widget.EditText;
//import androidx.appcompat.app.AppCompatActivity;
//import androidx.recyclerview.widget.LinearLayoutManager;
//import androidx.recyclerview.widget.RecyclerView;
//
//import com.example.shoppeclone.R;
//import com.example.shoppeclone.api.*;
//import com.example.shoppeclone.net.ApiClient;
//
//import retrofit2.Call;
//import retrofit2.Callback;
//import retrofit2.Response;
//
//public class ChatActivity extends AppCompatActivity {
//    ChatApi api; RecyclerView rv; ChatAdapter adapter; EditText input;
//    @Override protected void onCreate(Bundle b){
//        super.onCreate(b);
//        setContentView(R.layout.activity_chat);
//        // Sử dụng ApiClient trung tâm với BASE_URL mặc định
//        api = ApiClient.get(this).create(ChatApi.class);
//        rv = findViewById(R.id.recycler); rv.setLayoutManager(new LinearLayoutManager(this));
//        adapter = new ChatAdapter(); rv.setAdapter(adapter);
//        input = findViewById(R.id.input);
//        findViewById(R.id.btnSend).setOnClickListener(v -> send());
//    }
//    void send(){
//        String msg = input.getText().toString(); if(msg.isEmpty()) return;
//        adapter.addUser(msg); input.setText("");
//        ChatRequest req = new ChatRequest(); req.message = msg; req.productContext = null;
//        api.ask(req).enqueue(new Callback<ChatResponse>(){
//            @Override public void onResponse(Call<ChatResponse> call, Response<ChatResponse> rsp){
//                if(!rsp.isSuccessful() || rsp.body()==null || rsp.body().choices==null || rsp.body().choices.isEmpty()) return;
//                String text = rsp.body().choices.get(0).message.content;
//                adapter.addBot(text); rv.scrollToPosition(adapter.getItemCount()-1);
//            }
//            @Override public void onFailure(Call<ChatResponse> call, Throwable t){}
//        });
//    }
//}
