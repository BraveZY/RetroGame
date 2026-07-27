package com.jxhy.utils;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Intent;

import java.lang.reflect.Method;
import java.lang.reflect.Proxy;

public class DeepLinkUtil {
    public static void hookOnNewIntent(Activity activity, OnNewIntentListener listener) {
        try {
            Method original = Activity.class.getDeclaredMethod("onNewIntent", Intent.class);
            original.setAccessible(true);
            Proxy.newProxyInstance(
                    activity.getClassLoader(),
                    new Class<?>[] { Activity.class },
                    (proxy, method, args) -> {
                        if (method.getName().equals("onNewIntent")) {
                            Intent intent = (Intent) args[0];
                            listener.onNewIntent();
                            return original.invoke(activity, args);
                        }
                        return method.invoke(activity, args);
                    }
            );
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static void startActivity(Activity activity) {
        ComponentName componentName = new ComponentName("com.jxhy.childdance", "com.unity3d.player.UnityPlayerActivity");
        Intent intent = new Intent();
        intent.setComponent(componentName);
        activity.startActivity(intent);
    }

    public interface  OnNewIntentListener {
        void onNewIntent();
    }
}
