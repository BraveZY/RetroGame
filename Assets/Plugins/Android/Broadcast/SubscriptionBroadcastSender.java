package com.kinhank.unity;

import android.content.Context;
import android.content.Intent;
import android.text.TextUtils;
import android.util.Log;

public final class SubscriptionBroadcastSender {
    private static final String TAG = "UnitySubscribeSender";

    private SubscriptionBroadcastSender() {
    }

    public static boolean sendBroadcast(
            Context context,
            String action,
            String targetPackage,
            String extraKey,
            int extraValue) {
        if (context == null) {
            Log.e(TAG, "Context is null.");
            return false;
        }

        if (TextUtils.isEmpty(action)) {
            Log.e(TAG, "Action is empty.");
            return false;
        }

        if (TextUtils.isEmpty(targetPackage)) {
            Log.e(TAG, "Target package is empty.");
            return false;
        }

        if (TextUtils.isEmpty(extraKey)) {
            Log.e(TAG, "Extra key is empty.");
            return false;
        }

        try {
            Intent intent = new Intent(action);
            intent.setPackage(targetPackage);
            intent.putExtra(extraKey, extraValue);
            context.sendBroadcast(intent);
            Log.d(TAG, "Sent broadcast. action=" + action + ", targetPackage=" + targetPackage + ", extraKey=" + extraKey + ", extraValue=" + extraValue);
            return true;
        } catch (Exception exception) {
            Log.e(TAG, "Failed to send broadcast.", exception);
            return false;
        }
    }
}
