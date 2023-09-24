using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bilibili_LuckyDraw
{
    public partial class Login : Form
    {
        public UserMo userMo;
        public Login()
        {
            InitializeComponent();
            userMo = new UserMo();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ////创建Cookie
            //var cookie = webView.CoreWebView2.CookieManager.CreateCookie("token", DataExchangeHelper.LoginResult.token, domain, "/");
            ////设置Cookie
            //webView.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
            ////获取Cookie
            //List<CoreWebView2Cookie> list = await webView.CoreWebView2.CookieManager.GetCookiesAsync(url);
            ////删除Cookie
            //webView.CoreWebView2.CookieManager.DeleteAllCookies();

             
            string cookie_src = "";
            List<CoreWebView2Cookie> cookieList = await webView21.CoreWebView2.CookieManager.GetCookiesAsync("https://www.bilibili.com");
            for (int i = 0; i < cookieList.Count; ++i)
            {
                CoreWebView2Cookie cookie = webView21.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie(cookieList[i].ToSystemNetCookie());
                cookie_src += cookie.Name + "=" + cookie.Value + ";";
                if (cookie.Name == "DedeUserID")
                {
                    userMo.upid = cookie.Value;
                }
            }
            userMo.cookie_src = cookie_src;
            this.DialogResult = DialogResult.OK;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            //打开b站  
            string url = "https://passport.bilibili.com/login";
            this.webView21.Source = new Uri(url);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.webView21.CoreWebView2.Navigate(textBox1.Text.Trim());
        }

        private void button3_Click(object sender, EventArgs e)
        { 
            webView21.CoreWebView2.CookieManager.DeleteAllCookies();
            string url = "https://passport.bilibili.com/login";
            this.webView21.CoreWebView2.Navigate(url);
        }
    }
}
