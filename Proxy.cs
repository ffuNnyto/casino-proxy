using System.Net;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Game.Providers.Hacksaw;
using Game.Providers.Pragmatic;

namespace AppProxy;
public class Proxy
{

    public static void Start()
    {

        var proxyServer = new ProxyServer();
        var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 6798, true);
        
        proxyServer.EnableHttp2 = true;
        proxyServer.CertificateManager.CreateRootCertificate(false);
        proxyServer.CertificateManager.TrustRootCertificate();
        proxyServer.AddEndPoint(explicitEndPoint);

        explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
        proxyServer.BeforeRequest += OnRequest;
        proxyServer.BeforeResponse += OnResponse;

        proxyServer.Start();
        proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);
    }
    public static async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
    {
        string hostname = e.HttpClient.Request.RequestUri.Host;

        //e.DecryptSsl = true;


        if (hostname.Contains("hacksaw") || hostname.Contains("pragmatic"))
            e.DecryptSsl = true;



        await Task.Delay(10);
    }
    public static async Task OnRequest(object sender, SessionEventArgs e)
    {
        string method = e.HttpClient.Request.Method.ToUpper();

        if ((method == "POST" || method == "PUT" || method == "PATCH"))
        {
            byte[] bodyBytes = await e.GetRequestBody();
            e.SetRequestBody(bodyBytes);
            string bodyString = await e.GetRequestBodyAsString();
            e.SetRequestBodyString(bodyString);
        }
    }
    public static async Task OnResponse(object sender, SessionEventArgs e)
    {
        if (e.HttpClient.Request.Method == "GET" || e.HttpClient.Request.Method == "POST")
        {
            if (e.HttpClient.Response.StatusCode == 200 || e.HttpClient.Response.StatusCode == 404)
            {
                if (e.HttpClient.Response.ContentType != null && e.HttpClient.Response.ContentType.Trim().ToLower().Contains("text/html"))
                {
                    byte[] bodyBytes = await e.GetResponseBody();
                    e.SetResponseBody(bodyBytes);
                    string body = await e.GetResponseBodyAsString();
                    e.SetResponseBodyString(body);
                }
            }
        }

        if (e.HttpClient.Request.Url.Contains("hacksawgaming.com"))
        {
            string body = await e.GetResponseBodyAsString();
            if (e.HttpClient.Request.Url.Contains("/authenticate")) Hacksaw.Auth(body);
            if (e.HttpClient.Request.Url.Contains("/gameLaunch")) Hacksaw.AddSession(e.HttpClient.Request.BodyString);
            if (e.HttpClient.Request.Url.Contains("api/play/bet")) await Hacksaw.GetResponse(e.HttpClient.Request.BodyString, body);
        }

        if (e.HttpClient.Request.Url.Contains("pragmaticplay"))
        {

            string body = await e.GetResponseBodyAsString();
            if (e.HttpClient.Request.Url.Contains("/gameService"))
                await Pragmatic.GetResponse(e.HttpClient.Request.BodyString, body, e.HttpClient.Request.Headers.GetHeaders("referer")![0].Value);


        }
    }

}

