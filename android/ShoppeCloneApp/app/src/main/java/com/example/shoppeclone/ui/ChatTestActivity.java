package com.example.shoppeclone.ui;

import android.os.Bundle;
import android.text.TextUtils;
import android.view.inputmethod.EditorInfo;
import android.widget.*;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.shoppeclone.R;
import com.example.shoppeclone.api.MessageAdapter;
import com.example.shoppeclone.api.ChatApi;
import com.example.shoppeclone.api.ChatAIResponse;
import com.example.shoppeclone.api.ChatMessage;
import com.example.shoppeclone.api.ChatRequestGemini;
import com.example.shoppeclone.api.ChatRequestRag;
import com.example.shoppeclone.api.Message;
import com.example.shoppeclone.net.ApiClient;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ChatTestActivity extends AppCompatActivity {

    private Switch swMode;
    private EditText etInput;
    private Button btnSend;
    private ProgressBar pb;
    private RecyclerView rvMessages;
    private ImageButton btnBack; // THÊM VÀO

    private ChatApi api;
    private MessageAdapter messageAdapter;
    private List<Message> messageList;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        try {
            setContentView(R.layout.activity_chat_test_simple);

            swMode  = findViewById(R.id.swMode);
            etInput = findViewById(R.id.etInput);
            btnSend = findViewById(R.id.btnSend);
            pb      = findViewById(R.id.pb);
            rvMessages = findViewById(R.id.rvMessages);
            btnBack = findViewById(R.id.btnBack); // THÊM VÀO

            // Initialize message list and adapter
            messageList = new ArrayList<>();
            messageAdapter = new MessageAdapter(messageList);

            // Setup RecyclerView
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            layoutManager.setStackFromEnd(true);
            rvMessages.setLayoutManager(layoutManager);
            rvMessages.setAdapter(messageAdapter);

            // Create ChatApi from ApiClient
            api = ApiClient.get(this).create(ChatApi.class);

            btnSend.setOnClickListener(v -> send());

            // THÊM VÀO: Xử lý sự kiện click cho nút back
            btnBack.setOnClickListener(v -> finish());

            etInput.setOnEditorActionListener((tv, actionId, ev) -> {
                if (actionId == EditorInfo.IME_ACTION_SEND) {
                    send();
                    return true;
                }
                return false;
            });
        } catch (Exception e) {
            e.printStackTrace();
            Toast.makeText(this, "Lỗi: " + e.getMessage(), Toast.LENGTH_LONG).show();
        }
    }

    private void setLoading(boolean loading) {
        pb.setVisibility(loading ? android.view.View.VISIBLE : android.view.View.GONE);
        btnSend.setEnabled(!loading);
        etInput.setEnabled(!loading);
    }

    private void send() {
        String q = etInput.getText().toString().trim();
        if (TextUtils.isEmpty(q)) {
            etInput.setError("Nhập câu hỏi trước đã");
            return;
        }

        // Add user message to chat
        Message userMessage = new Message(q, true, System.currentTimeMillis());
        messageAdapter.addMessage(userMessage);
        rvMessages.smoothScrollToPosition(messageAdapter.getItemCount() - 1);

        // Clear input
        etInput.setText("");

        setLoading(true);

        // Create history: only 1 user message
        List<ChatMessage> messages = new ArrayList<>();
        messages.add(new ChatMessage("user", q));

        if (swMode.isChecked()) {
            // RAG (Trained): POST /api/ai/chat-rag
            ChatRequestRag body = new ChatRequestRag(
                    null,
                    messages,
                    3,
                    null,
                    null,
                    null
            );

            api.chatRag(body).enqueue(new Callback<ChatAIResponse>() {
                @Override
                public void onResponse(Call<ChatAIResponse> call, Response<ChatAIResponse> resp) {
                    setLoading(false);
                    if (!resp.isSuccessful() || resp.body() == null) {
                        addAiMessage("Lỗi: HTTP " + resp.code());
                        return;
                    }
                    ChatAIResponse d = resp.body();
                    if (d.error) {
                        addAiMessage("Server error: " + (d.detail != null ? d.detail : d.status));
                        return;
                    }
                    addAiMessage(d.reply != null ? d.reply : "(rỗng)");
                }

                @Override
                public void onFailure(Call<ChatAIResponse> call, Throwable t) {
                    setLoading(false);
                    addAiMessage("Không kết nối được: " + t.getMessage());
                }
            });

        } else {
            // Gemini: POST /api/ai/chat
            ChatRequestGemini body = new ChatRequestGemini(
                    null,      // system (optional)
                    messages
            );

            api.chatGemini(body).enqueue(new Callback<ChatAIResponse>() {
                @Override
                public void onResponse(Call<ChatAIResponse> call, Response<ChatAIResponse> resp) {
                    setLoading(false);
                    if (!resp.isSuccessful() || resp.body() == null) {
                        addAiMessage("Lỗi: HTTP " + resp.code());
                        return;
                    }
                    ChatAIResponse d = resp.body();
                    if (d.error) {
                        addAiMessage("Server error: " + (d.detail != null ? d.detail : d.status));
                        return;
                    }
                    addAiMessage(d.reply != null ? d.reply : "(rỗng)");
                }

                @Override
                public void onFailure(Call<ChatAIResponse> call, Throwable t) {
                    setLoading(false);
                    addAiMessage("Không kết nối được: " + t.getMessage());
                }
            });
        }
    }

    private void addAiMessage(String text) {
        Message aiMessage = new Message(text, false, System.currentTimeMillis());
        messageAdapter.addMessage(aiMessage);
        rvMessages.smoothScrollToPosition(messageAdapter.getItemCount() - 1);
    }
}