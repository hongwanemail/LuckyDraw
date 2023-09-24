using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bilibili_LuckyDraw
{
    public class Bilibili
    {
        public string Bilibili_UID { get; set; }
        public string Bilibili_Uname { get; set; }
        public string Bilibili_Comment { get; set; }

    }
    public class Dynamic
    {
        public string 视频标题 { get; set; }
        public string 动态内容 { get; set; }
        public int 转发数 { get; set; }
        public int 评论数 { get; set; }
        public int 点赞数 { get; set; }

    }


    public class Video
    {
        public string 视频标题 { get; set; }
        public int 播放量 { get; set; }
        public int 弹幕数 { get; set; }
        public int 评论数 { get; set; }
        public int 点赞数 { get; set; }
        public int 投币数 { get; set; }
        public int 收藏数 { get; set; }
        public int 转发数 { get; set; }
        public int aid { get; set; }
    }
    public class UserMo
    {
        public string upid { get; set; }
        public string cookie_src { get; set; }
    }


    public static class PublicHelp
    { 
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 指定Url地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="url">请求链接地址</param>
        /// <returns></returns>
        public static string New_Get(string url, string cookies="")
        {
            string result = "";
            try
            {

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                if (cookies != "")
                {
                    myRequest.Headers.Add("cookie", cookies);
                }
                myRequest.Method = "GET";
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception er)
            {
                if (er.Message.Contains("412"))
                {
                    result = "请求被拦截";
                } 
            }
            return result;
        }
    }
}