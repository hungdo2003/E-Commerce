package com.example.shoppeclone.api;

import com.google.gson.annotations.SerializedName;

public class ZpCreateResponse {
    @SerializedName("returncode")
    public int returnCode;

    @SerializedName("returnmessage")
    public String returnMessage;

    @SerializedName("orderurl")
    public String orderUrl;

    @SerializedName("zptranstoken")
    public String zpTransToken;
}
