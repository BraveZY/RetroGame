package com.jxhy.aekecameratool;

import android.content.Context;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbManager;
import android.util.Log;

import java.util.HashMap;

public class AEKECamHelper {
    static String NEW_CAMERA_VID = "1d6b";
    static String NEW_CAMERA_PID = "102";
    static int CURRENT_TYPE = -1;
    static int CAMERA_TYPE_OLD = 0;
    static int CAMERA_TYPE_NEW = 1;

    static int getDeviceType(Context context){
        if (CURRENT_TYPE == -1){
            UsbManager manager = (UsbManager) context.getSystemService(Context.USB_SERVICE);
            HashMap<String, UsbDevice> devices = manager.getDeviceList();
            for (UsbDevice device : devices.values()) {
                int vid = device.getVendorId();
                int pid = device.getProductId();
                Log.i("Unity","1 vid:" + vid + ",pid:" + pid);
                Log.i("Unity","2 vid:" + Integer.toHexString(vid) + ",pid:" + Integer.toHexString(pid));
                if (Integer.toHexString(vid).equals(NEW_CAMERA_VID) && Integer.toHexString(pid).equals( NEW_CAMERA_PID)){
                    CURRENT_TYPE = CAMERA_TYPE_NEW;
                    return CURRENT_TYPE;
                }
            }
            CURRENT_TYPE = CAMERA_TYPE_OLD;
            return CURRENT_TYPE;
        }
        return CURRENT_TYPE;
    }
}