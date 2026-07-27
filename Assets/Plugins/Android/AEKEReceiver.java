package com.jxhy.aekebroadcasttool;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

public class AEKEReceiver  extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        if (receiveListener!=null)
            receiveListener.OnReceive(intent);
    }

    public interface OnReceiveListener {
        void OnReceive(Intent intent);
    }

    OnReceiveListener receiveListener;

    public void SetOnReceiveListener(OnReceiveListener receiveListener){
        this.receiveListener = receiveListener;
    }
}

