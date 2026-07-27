using System;
using System.Collections;
using System.Collections.Generic;
using GameCoreRuntime;
using UnityEngine;
using static CamCenter;
using CameraType = UnityEngine.CameraType;

public class AddCamera : ICamBase
{

    public IEnumerator IELaunch(bool front, int width, int height)
    {
        yield return null;
        //GameCore.Camera.Init(GameCoreRuntime.CameraType.UNITY_WEBCAMERA, width, height, 30);
        // if (GameCore.IsInit)
        GameCore.Camera?.Play();
        State = ECamState.Playing;
    }

    public Color32[] Pixels32 => throw new NotImplementedException();

    public int Width => GameCore.Camera.Width;

    public int Height => GameCore.Camera.Height;

    public int Angle => throw new NotImplementedException();

    public bool Front => true;

    public Texture Preview => GameCore.Camera.CameraTexture;

    public ECamPluginType PluginType => throw new NotImplementedException();

    public ECamState State { get; private set; }

    public int Count => throw new NotImplementedException();

    public Color[] GetPixels(int x, int y, int width, int height)
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        // if (GameCore.IsInit)
        GameCore.Camera?.Stop();
        State = ECamState.Paused;
    }

    public void Resume()
    {
        // if (GameCore.IsInit)
        GameCore.Camera?.Play();
        State = ECamState.Playing;
    }

    public void Stop()
    {
        // if (GameCore.IsInit)
        GameCore.Camera?.Stop();
        State = ECamState.Stoped;
    }
}
