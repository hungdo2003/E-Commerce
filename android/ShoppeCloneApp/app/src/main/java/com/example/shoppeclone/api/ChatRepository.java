package com.example.shoppeclone.api;

import android.content.Context;

import com.example.shoppeclone.api.ChatApi;
import com.example.shoppeclone.api.ChatAIResponse;
import com.example.shoppeclone.api.ChatMessage;
import com.example.shoppeclone.api.ChatRequestGemini;
import com.example.shoppeclone.api.ChatRequestRag;
import com.example.shoppeclone.net.ApiClient;

import java.util.List;

import retrofit2.Call;

/**
 * ChatRepository — gộp 2 loại AI:
 *  GEMINI  -> /api/ai/chat
 *  RAG     -> /api/ai/chat-rag
 */
public class ChatRepository {

    /** Tuỳ chọn thêm cho RAG (không bắt buộc) */
    public static class RagOptions {
        public Integer k = 3;
        public String tag = null;
        public String source = null;
        public String tenantId = "string";

        public RagOptions() {}
        public RagOptions(Integer k, String tag, String source, String tenantId) {
            if (k != null) this.k = k;
            this.tag = tag;
            this.source = source;
            if (tenantId != null) this.tenantId = tenantId;
        }
    }

    private final ChatApi api;

    public ChatRepository(Context ctx) {
        this.api = ApiClient.get(ctx).create(ChatApi.class);
    }

    /**
     * Hàm thống nhất: tự chọn endpoint theo ChatMode.
     * @param mode     ChatMode.GEMINI hoặc ChatMode.RAG
     * @param messages Danh sách message (user, assistant)
     * @param system   Prompt hệ thống (có thể null)
     * @param ragOpt   Tuỳ chọn RAG (có thể null)
     */
    public Call<ChatAIResponse> ask(ChatMode mode, List<ChatMessage> messages,
                                    String system, RagOptions ragOpt) {
        switch (mode) {
            case GEMINI:
                ChatRequestGemini gBody = new ChatRequestGemini(system, messages);
                return api.chatGemini(gBody);

            case RAG:
            default:
                if (ragOpt == null) ragOpt = new RagOptions();
                ChatRequestRag rBody = new ChatRequestRag(
                        system,
                        messages,
                        ragOpt.k,
                        ragOpt.tag,
                        ragOpt.source,
                        ragOpt.tenantId
                );
                return api.chatRag(rBody);
        }
    }
}
