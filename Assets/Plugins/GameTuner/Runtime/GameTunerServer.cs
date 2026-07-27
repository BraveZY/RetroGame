using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// GameTuner 独立 HTTP 服务器。
/// 
/// - 独立端口（默认 8089），不依赖项目现有 HttpServer（8088）。
/// - 支持 Editor（WiFi 网卡 IP）和 Android APK（JNI 获取 IP）。
/// - 提供动态 /api/* 路由和内嵌调参页面（不依赖 StreamingAssets）。
/// - 所有请求在线程池中处理，ParameterHub 通过 lock 保证线程安全。
/// </summary>
public class GameTunerServer
{
    private const int BasePort = 8089;

    private HttpListener _listener;
    private Thread _thread;

    public string AccessUrl { get; private set; }
    public bool IsRunning => _listener != null && _listener.IsListening;

    // ─── 生命周期 ────────────────────────────────────────────────────────────

    public void Start()
    {
        string ip = GetLocalIP();
        int port = BasePort;

        while (port < BasePort + 100)
        {
            try
            {
                _listener = new HttpListener();
                // 使用通配符前缀，接受任意 Host 头，避免 Bad Request (Invalid host)
                // ip 只用于 Console 展示，不作为绑定条件
                _listener.Prefixes.Add($"http://+:{port}/");
                _listener.Start();
                AccessUrl = $"http://{ip}:{port}";
                break;
            }
            catch
            {
                _listener?.Close();
                _listener = null;
                port++;
            }
        }

        if (_listener == null)
        {
            Debug.LogWarning("[GameTuner] 无法启动 HTTP 服务，所有端口均被占用。");
            return;
        }

        _thread = new Thread(ListenLoop) { IsBackground = true, Name = "GameTunerServer" };
        _thread.Start();

        Debug.Log($"[GameTuner] 调参地址: {AccessUrl}");
    }

    public void Stop()
    {
        try { _listener?.Stop(); _listener?.Close(); }
        catch { /* 忽略关闭时异常 */ }
        finally { _listener = null; }
    }

    // ─── 请求循环 ─────────────────────────────────────────────────────────────

    private void ListenLoop()
    {
        while (_listener != null && _listener.IsListening)
        {
            try
            {
                var ctx = _listener.GetContext();
                ThreadPool.QueueUserWorkItem(_ => HandleRequest(ctx));
            }
            catch { break; }
        }
    }

    private void HandleRequest(HttpListenerContext ctx)
    {
        try
        {
            // CORS 头（允许浏览器跨域调试）
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, OPTIONS");
            ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (ctx.Request.HttpMethod == "OPTIONS")
            {
                ctx.Response.StatusCode = 204;
                ctx.Response.Close();
                return;
            }

            string path = ctx.Request.Url.LocalPath;

            if (path.StartsWith("/api/"))
                HandleApiRequest(ctx, path);
            else
                ServeIndexHtml(ctx);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GameTuner] 请求处理异常: {ex.Message}");
        }
    }

    // ─── API 路由 ──────────────────────────────────────────────────────────────

    private void HandleApiRequest(HttpListenerContext ctx, string path)
    {
        // GET /api/params → 返回所有参数
        if (path == "/api/params" && ctx.Request.HttpMethod == "GET")
        {
            WriteJson(ctx, 200, BuildParamsJson(ParameterHub.GetAll()));
            return;
        }

        // PUT /api/params/{id} → 更新单个参数值
        if (path.StartsWith("/api/params/") && ctx.Request.HttpMethod == "PUT")
        {
            string id = path.Substring("/api/params/".Length);
            float value = ParseValueFromBody(ReadBody(ctx));
            bool ok = ParameterHub.SetValue(id, value);
            WriteJson(ctx, ok ? 200 : 404, ok ? "{\"ok\":true}" : "{\"ok\":false,\"error\":\"id not found\"}");
            return;
        }

        // POST /api/params/all/reset → 重置全部参数
        if (path == "/api/params/all/reset" && ctx.Request.HttpMethod == "POST")
        {
            ParameterHub.ResetAll();
            WriteJson(ctx, 200, "{\"ok\":true}");
            return;
        }

        WriteJson(ctx, 404, "{\"error\":\"not found\"}");
    }

    // ─── 静态页面 ─────────────────────────────────────────────────────────────

    private void ServeIndexHtml(HttpListenerContext ctx)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(IndexHtml);
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    // ─── 工具方法 ─────────────────────────────────────────────────────────────

    private static string BuildParamsJson(List<ParameterEntry> entries)
    {
        var sb = new StringBuilder("{\"params\":[");
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var e = entries[i];
            sb.Append('{');
            sb.Append($"\"id\":\"{Esc(e.id)}\",");
            sb.Append($"\"category\":\"{Esc(e.category)}\",");
            sb.Append($"\"name\":\"{Esc(e.name)}\",");
            sb.Append($"\"description\":\"{Esc(e.description ?? "")}\",");
            sb.Append($"\"minValue\":{e.minValue.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"maxValue\":{e.maxValue.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"defaultValue\":{e.defaultValue.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"currentValue\":{e.currentValue.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"step\":{e.step.ToString(CultureInfo.InvariantCulture)}");
            sb.Append('}');
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static string ReadBody(HttpListenerContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static float ParseValueFromBody(string body)
    {
        // 解析 {"value": 1.5} 中的数值，不引入外部 JSON 库
        int idx = body.IndexOf("\"value\"", StringComparison.Ordinal);
        if (idx < 0) return 0f;
        int colon = body.IndexOf(':', idx);
        if (colon < 0) return 0f;
        string raw = body.Substring(colon + 1).Trim().TrimEnd('}', ' ');
        return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
    }

    private static void WriteJson(HttpListenerContext ctx, int status, string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    private static string Esc(string s) =>
        s?.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") ?? "";

    // ─── IP 获取（与 HttpServer.cs 保持相同逻辑）──────────────────────────────

    private static string GetLocalIP()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                var contextClass = new AndroidJavaClass("android.content.Context");
                string WIFI_SERVICE = contextClass.GetStatic<string>("WIFI_SERVICE");
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var wifiMgr = context.Call<AndroidJavaObject>("getSystemService", WIFI_SERVICE);
                var wifiInfo = wifiMgr.Call<AndroidJavaObject>("getConnectionInfo");
                int ip = wifiInfo.Call<int>("getIpAddress");
                return $"{ip & 0xFF}.{(ip >> 8) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 24) & 0xFF}";
            }
            catch { return "127.0.0.1"; }
        }

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            // 跳过回环、未激活、虚拟隧道等无效网卡
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

            foreach (var addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                string ip = addr.Address.ToString();
                if (ip.StartsWith("127.")) continue; // 二次过滤回环地址
                return ip;
            }
        }
        return "127.0.0.1";

    }

    // ─── 内嵌调参页面（无需 StreamingAssets）────────────────────────────────────

    private const string IndexHtml = @"<!DOCTYPE html>
<html lang=""zh"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
<title>GameTuner</title>
<style>
* { margin:0; padding:0; box-sizing:border-box; -webkit-tap-highlight-color:transparent; }
:root {
  --bg: #1a1a2e; --card: #16213e; --accent: #00d4ff;
  --glow: rgba(0,212,255,0.3); --ok: #4ade80;
  --text: #fff; --dim: #888; --track: #0f3460;
}
body { font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif; background:var(--bg); color:var(--text); min-height:100vh; padding-bottom:80px; }
.header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; background:var(--card); position:sticky; top:0; z-index:100; }
.header h1 { font-size:18px; color:var(--accent); }
.status { display:flex; align-items:center; gap:8px; font-size:12px; color:var(--dim); }
.dot { width:8px; height:8px; border-radius:50%; background:#444; }
.dot.on { background:var(--ok); box-shadow:0 0 8px var(--ok); }
.tabs { display:flex; gap:8px; padding:12px 20px; overflow-x:auto; }
.tab { padding:8px 16px; background:var(--card); border-radius:20px; font-size:13px; white-space:nowrap; cursor:pointer; border:none; color:var(--text); }
.tab.active { background:var(--accent); color:var(--bg); }
.grid { padding:16px 20px; display:grid; gap:14px; }
.card { background:var(--card); padding:18px 16px; border-radius:14px; }
.card-top { display:flex; justify-content:space-between; align-items:center; margin-bottom:12px; }
.card-top label { font-size:14px; font-weight:500; }
.val { color:var(--accent); font-family:monospace; font-weight:bold; background:rgba(0,212,255,0.1); padding:2px 8px; border-radius:4px; cursor:pointer; min-width:52px; text-align:center; }
input[type=range] { width:100%; height:6px; -webkit-appearance:none; background:var(--track); border-radius:3px; outline:none; }
input[type=range]::-webkit-slider-thumb { -webkit-appearance:none; width:22px; height:22px; background:var(--accent); border-radius:50%; cursor:pointer; box-shadow:0 0 10px var(--glow); }
.range { display:flex; justify-content:space-between; margin-top:6px; font-size:11px; color:var(--dim); }
.desc { margin-top:6px; font-size:11px; color:var(--dim); line-height:1.4; }
footer { display:flex; gap:12px; padding:16px 20px; position:fixed; bottom:0; left:0; right:0; background:var(--bg); border-top:1px solid #16213e; }
.btn { flex:1; padding:12px; border:none; border-radius:8px; font-size:14px; font-weight:600; cursor:pointer; }
.btn-reset { background:#e74c3c; color:#fff; }
.btn-export { background:var(--accent); color:var(--bg); }
</style>
</head>
<body>
<header class=""header"">
  <h1>GameTuner</h1>
  <div class=""status""><span class=""dot"" id=""dot""></span><span id=""st"">连接中...</span></div>
</header>
<nav class=""tabs"" id=""tabs""></nav>
<main class=""grid"" id=""grid""></main>
<footer>
  <button class=""btn btn-reset"" onclick=""resetAll()"">重置</button>
  <button class=""btn btn-export"" onclick=""exportConfig()"">导出 JSON</button>
</footer>
<script>
let allParams = [], cat = 'all', timers = {};
async function load() {
  try {
    const r = await fetch('/api/params');
    allParams = (await r.json()).params || [];
    renderTabs(); renderGrid(); setStatus(true);
  } catch { setStatus(false); }
}
function renderTabs() {
  const cats = [...new Set(allParams.map(p => p.category))];
  document.getElementById('tabs').innerHTML =
    tab('全部', 'all') + cats.map(c => tab(c, c)).join('');
}
function tab(label, key) {
  return `<button class=""tab${cat===key?' active':''}"" onclick=""setCat('${key}')"">${label}</button>`;
}
function renderGrid() {
  const list = cat === 'all' ? allParams : allParams.filter(p => p.category === cat);
  document.getElementById('grid').innerHTML = list.map(p => `
    <div class=""card"">
      <div class=""card-top"">
        <label>${p.name}</label>
        <span class=""val"" id=""v_${p.id}"" onclick=""manualInput('${p.id}')"">${p.currentValue.toFixed(2)}</span>
      </div>
      <input type=""range"" min=""${p.minValue}"" max=""${p.maxValue}"" step=""${p.step}""
        value=""${p.currentValue}"" oninput=""onInput(this,'${p.id}')"">
      <div class=""range""><span>${p.minValue}</span><span>${p.maxValue}</span></div>
      ${p.description ? `<div class=""desc"">${p.description}</div>` : ''}
    </div>`).join('');
}
function onInput(el, id) {
  const v = parseFloat(el.value);
  document.getElementById('v_' + id).textContent = v.toFixed(2);
  clearTimeout(timers[id]);
  timers[id] = setTimeout(() => {
    fetch('/api/params/' + id, {
      method: 'PUT', headers: {'Content-Type':'application/json'},
      body: JSON.stringify({value: v})
    });
  }, 50);
}
function manualInput(id) {
  const p = allParams.find(x => x.id === id);
  const v = prompt(`${p.name} (${p.minValue} ~ ${p.maxValue})`, p.currentValue);
  if (v !== null && !isNaN(parseFloat(v))) {
    fetch('/api/params/' + id, {
      method: 'PUT', headers: {'Content-Type':'application/json'},
      body: JSON.stringify({value: parseFloat(v)})
    }).then(load);
  }
}
async function resetAll() {
  if (!confirm('确定重置所有参数？')) return;
  await fetch('/api/params/all/reset', {method:'POST'});
  load();
}
function exportConfig() {
  const cfg = {};
  allParams.forEach(p => cfg[p.id] = p.currentValue);
  const a = document.createElement('a');
  a.href = URL.createObjectURL(new Blob([JSON.stringify(cfg, null, 2)], {type:'application/json'}));
  a.download = `gametuner_${Date.now()}.json`;
  a.click();
}
function setCat(c) { cat = c; renderTabs(); renderGrid(); }
function setStatus(ok) {
  document.getElementById('dot').className = 'dot' + (ok ? ' on' : '');
  document.getElementById('st').textContent = ok ? '已连接' : '连接失败';
}
load();
setInterval(load, 5000);
</script>
</body>
</html>";
}
