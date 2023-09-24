using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace bilibili_LuckyDraw
{
    public partial class bilibili : Form
    {
        UserMo userMo;
        public bilibili()
        {
            InitializeComponent();
            userMo = new UserMo();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            if (login.ShowDialog() == DialogResult.OK)
            {
                userMo = login.userMo;
                txt_UP.Text = userMo.upid;
                //更新粉丝数
                刷新粉丝数();
            }

        }

        private void 刷新粉丝数()
        {
            try
            {
                if (txt_UP.Text.Trim() != "")
                {
                    //查询 b站api 
                    using (var client = new WebClient())
                    {
                        string url = "https://api.bilibili.com/x/relation/stat?vmid=" + txt_UP.Text.Trim() + "&jsonp=jsonp";
                        var responseString = PublicHelp.New_Get(url);

                        if (responseString == "请求被拦截")
                        {
                            textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                            lab_fans.Text = "0";
                            return; 
                        } 
                        if (responseString == "")
                        {
                            lab_fans.Text = "0";
                            return;
                        }
                        JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);

                        if (jo["code"].ToString() == "0")
                            lab_fans.Text = jo["data"]["follower"].ToString();
                        else
                            lab_fans.Text = "0";
                    }
                }
            }
            catch (Exception er)
            {
                textBox_Process.Text = "获取数据错误！";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //走时间用的 
            timer1.Interval = 1000;
            timer1.Start();

            //复制程序本身 读取md5 如何删除
            try
            {
                File.Copy(Assembly.GetEntryAssembly().Location, Application.StartupPath + "1.exe", true);
                this.Text += "  MD5:" + PublicHelp.GetMD5HashFromFile(Application.StartupPath + "1.exe");
                File.Delete(Application.StartupPath + "1.exe");

            }
            catch (Exception er)
            {
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "https://space.bilibili.com/602398876");
            }
            catch (Exception er)
            {
                MessageBox.Show("调用浏览器失败！", "提示", MessageBoxButtons.OK);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = "当前时间：" + DateTime.Now.ToString("yyyy年MM月dd日 HH点mm分ss秒");
            //获取UP主粉丝数 
            刷新粉丝数();

            if (The_Timer && DateTime.Now >= dateTimePicker1.Value)
            {
                //定时抽奖
                BilibiliDynamic();
                The_Timer = false;
                button4.Text = "定时抽奖";
            }
        }

        private void 刷新连接数据()
        {
            try
            {
                textBox_data.Text = "获取数据中......\r\n";
                string Dynamic_id = GetDynamicid();
                if (Dynamic_id == "")
                {
                    lab_Type.Text = "未知";
                }
                else
                {
                    textBox_data.Text += "获取数据成功，ID为：" + Dynamic_id + "\r\n\r\n";


                    if (lab_Type.Text == "视频动态")
                    {
                        Dynamic dynamic = 获取视频动态信息(Dynamic_id);
                        if (dynamic != null)
                        {
                            textBox_data.Text += "动态内容：" + dynamic.动态内容 + "\r\n\r\n";
                            textBox_data.Text += "视频标题：" + dynamic.视频标题 + "\r\n\r\n";

                            textBox_data.Text += "转发数：" + dynamic.转发数 + "\r\n";
                            textBox_data.Text += "评论数：" + dynamic.评论数 + "\r\n";
                            textBox_data.Text += "点赞数：" + dynamic.点赞数 + "\r\n\r\n";
                        }
                    }

                    if (lab_Type.Text == "图文动态")
                    {
                        Dynamic dynamic = 获取图文动态信息(Dynamic_id);
                        if (dynamic != null)
                        {
                            textBox_data.Text += "动态内容：" + dynamic.动态内容 + "\r\n\r\n";

                            textBox_data.Text += "转发数：" + dynamic.转发数 + "\r\n";
                            textBox_data.Text += "评论数：" + dynamic.评论数 + "\r\n";
                            textBox_data.Text += "点赞数：" + dynamic.点赞数 + "\r\n\r\n";
                        }
                    }
                    else if (lab_Type.Text == "视频")
                    {

                        Video video = 获取视频信息(Dynamic_id);
                        if (video != null)
                        {
                            textBox_data.Text += "视频标题：" + video.视频标题 + "\r\n\r\n";
                            textBox_data.Text += "播放量：" + video.播放量 + "\r\n";
                            textBox_data.Text += "弹幕数：" + video.弹幕数 + "\r\n";
                            textBox_data.Text += "评论数：" + video.评论数 + "\r\n";
                            textBox_data.Text += "点赞数：" + video.点赞数 + "\r\n";
                            textBox_data.Text += "投币数：" + video.投币数 + "\r\n";
                            textBox_data.Text += "转发数：" + video.转发数 + "\r\n\r\n";
                        }
                    }
                }
            }
            catch (Exception er)
            {
                textBox_Process.Text = er.Message;
                textBox_Process.Text = "获取连接数据错误";
            }
            textBox_data.Text += "数据读取完成!\r\n";
        }

        private Dynamic 获取图文动态信息(string dynamic_id)
        {
            Dynamic dynamic = new Dynamic();
            try
            {
                string DynamicAPI = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id=" + dynamic_id;
                //如果需要登录 就是这个 
                if (userMo.cookie_src == null || userMo.cookie_src == "")
                {
                    textBox_Process.Text = "需要登录!";
                    return null;
                }
                string responseString = PublicHelp.New_Get(DynamicAPI, userMo.cookie_src);
                if (responseString == "请求被拦截")
                {
                    textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                    return null;
                }
                JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);
                if (jo != null && jo["code"].ToString() == "0")
                {
                    if (jo["data"]["card"] == null)
                    {
                        textBox_Process.Text = "请求成功，但是数据不对，请重试!";
                        return null;
                    }

                    string comment = jo["data"]["card"]["desc"]["comment"].ToString();
                    string repost = jo["data"]["card"]["desc"]["repost"].ToString();
                    string like = jo["data"]["card"]["desc"]["like"].ToString();

                    dynamic.转发数 = int.Parse(repost);
                    dynamic.点赞数 = int.Parse(like);
                    dynamic.评论数 = int.Parse(comment);

                    string card = jo["data"]["card"]["card"].ToString();
                    JObject card_jo = (JObject)JsonConvert.DeserializeObject(card);


                    if (card_jo["item"]["description"] != null)
                    {
                        string m_description = card_jo["item"]["description"].ToString();
                        dynamic.动态内容 = m_description;
                    }
                    else if (card_jo["item"]["content"] != null)
                    {
                        string content = card_jo["item"]["content"].ToString();
                        dynamic.动态内容 = content;
                    }
                    else
                    {
                        dynamic.动态内容 = "接口又改了...";
                    }
                }
            }
            catch (Exception er)
            {
                textBox_Process.Text = er.Message;
                return null;
            }
            return dynamic;
        }

        private Video 获取视频信息(string dynamic_id)
        {
            Video video = new Video();
            try
            {
                string videoapi = "https://api.bilibili.com/x/web-interface/view?bvid=" + dynamic_id;

                string responseString = PublicHelp.New_Get(videoapi);
                if (responseString == "请求被拦截")
                {
                    textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                    return null;
                }
                if (responseString == "")
                    return null;
                JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);
                if (jo["code"].ToString() == "0")
                {
                    string title = jo["data"]["title"].ToString();
                    string aid = jo["data"]["aid"].ToString();
                    string view = jo["data"]["stat"]["view"].ToString();
                    string like = jo["data"]["stat"]["like"].ToString();
                    string reply = jo["data"]["stat"]["reply"].ToString();
                    string danmaku = jo["data"]["stat"]["danmaku"].ToString();
                    string favorite = jo["data"]["stat"]["favorite"].ToString();
                    string coin = jo["data"]["stat"]["coin"].ToString();
                    string share = jo["data"]["stat"]["share"].ToString();

                    video.aid = int.Parse(aid);
                    video.视频标题 = title;
                    video.播放量 = int.Parse(view);
                    video.弹幕数 = int.Parse(danmaku);
                    video.评论数 = int.Parse(reply);
                    video.点赞数 = int.Parse(like);
                    video.投币数 = int.Parse(coin);
                    video.收藏数 = int.Parse(favorite);
                    video.转发数 = int.Parse(share);
                }

            }
            catch (Exception er)
            {
                textBox_Process.Text = er.Message;
                return null;
            }

            return video;
        }

        private Dynamic 获取视频动态信息(string dynamic_id)
        {
            Dynamic dynamic = new Dynamic();
            try
            {
                string DynamicAPI = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id=" + dynamic_id;
                //如果需要登录 就是这个 
                if (userMo.cookie_src == null || userMo.cookie_src == "")
                {
                    textBox_Process.Text = "需要登录!";
                    return null;
                }
                string responseString = PublicHelp.New_Get(DynamicAPI, userMo.cookie_src);
                if (responseString == "请求被拦截")
                {
                    textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                    return null;
                }
                JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);
                if (jo != null && jo["code"].ToString() == "0")
                {
                    if (jo["data"]["card"] == null)
                    {
                        textBox_Process.Text = "请求成功，但是数据不对，请重试!";
                        return null;
                    }
                    string card = jo["data"]["card"]["card"].ToString();

                    string repost = jo["data"]["card"]["desc"]["repost"].ToString();
                    string like = jo["data"]["card"]["desc"]["like"].ToString();

                    dynamic.转发数 = int.Parse(repost);
                    dynamic.点赞数 = int.Parse(like);


                    JObject card_jo = (JObject)JsonConvert.DeserializeObject(card);

                    string m_dynamic = card_jo["dynamic"].ToString();
                    string m_title = card_jo["title"].ToString();
                    dynamic.动态内容 = m_dynamic;
                    dynamic.视频标题 = m_title;

                    string comment = card_jo["stat"]["reply"].ToString();
                    dynamic.评论数 = int.Parse(comment);


                }
            }
            catch (Exception er)
            {
                textBox_Process.Text = er.Message;
                return null;
            }
            return dynamic;
        }

        /// <summary>
        /// 获取  Dynamic_id
        /// </summary>
        /// <returns></returns>
        public string GetDynamicid()
        {
            string Dynamic_id = "";
            try
            {
                if (txt_dynamic.Text.Trim() == "")
                {
                    return Dynamic_id;
                }
                string[] s = txt_dynamic.Text.Trim().Split('/');
                string bilibili_domain = s[2];

                if (bilibili_domain == "www.bilibili.com" && s[3] == "video")
                {
                    lab_Type.Text = "视频";
                    Dynamic_id = s[4].Split('?')[0];
                }
                else if (bilibili_domain == "www.bilibili.com" && s[3] == "opus")
                {
                    lab_Type.Text = "图文动态";
                    Dynamic_id = s[4].Split('?')[0];

                }
                else if (bilibili_domain == "t.bilibili.com")
                {
                    lab_Type.Text = "视频动态";
                    Dynamic_id = s[3].Split('?')[0];
                }

            }
            catch (Exception er)
            {
                textBox_Process.Text = "输入的链接无法解析!";
                lab_Type.Text = "未知";
            }
            return Dynamic_id;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            刷新连接数据();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (txt_UP.Text.Trim() == "")
            {
                MessageBox.Show("请输入UP主ID", "提示", MessageBoxButtons.OK);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "https://space.bilibili.com/" + txt_UP.Text.Trim());
            }
            catch (Exception er)
            {
                MessageBox.Show("调用浏览器失败！", "提示", MessageBoxButtons.OK);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                BilibiliDynamic();
            }
            catch (Exception er)
            {
                MessageBox.Show("呃~~~报错了...\r\n大概率是B站又又又改接口了。去B站联系一下作者吧！\r\n点软件那个黑色的logo就行 (^=^)", "提示", MessageBoxButtons.OK);
            }
        }

        private void BilibiliDynamic()
        {
            textBox_Process.Text = "";
            List<Bilibili> list = new List<Bilibili>();
            string Dynamic_id = GetDynamicid();

            if (lab_Type.Text.Contains("动态"))
            {
                Dynamic dynamic = null;
                if (lab_Type.Text == "视频动态")
                {
                    dynamic = 获取视频动态信息(Dynamic_id);
                    if (dynamic == null)
                    {
                        textBox_Process.Text = "获取视频动态信息失败!";
                        return;
                    }
                }
                else if (lab_Type.Text == "图文动态")
                {
                    dynamic = 获取图文动态信息(Dynamic_id);
                    if (dynamic == null)
                    {
                        textBox_Process.Text = "获取图文动态信息失败!";
                        return;
                    }
                }
               

                string offset = "";
                while (true)
                {
                    //支持最新的B站动态转发用户获取
                    //B站最新转发用户信息是滚屏显示的
                    //没一次显示数据 都会带上 下一次请求数据需要的   参数 offset
                    //offset 是一直在变化的 之前的接口只能获取500个 


                    string DynamicAPI = "https://api.bilibili.com/x/polymer/web-dynamic/v1/detail/forward?id=" + Dynamic_id + "&offset=" + offset;
                    string responseString = PublicHelp.New_Get(DynamicAPI, userMo.cookie_src);
                    if (responseString == "请求被拦截")
                    {
                        textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                        break;
                    }
                    if (responseString == "")
                    {
                        textBox_Process.Text = "获取动态转发用户数据信息失败!\r\n\r\n";
                        break;
                    }
                    JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);
                    if (jo["code"].ToString() != "0")
                    {
                        textBox_Process.Text = "获取动态转发用户数据信息失败!\r\n\r\n";
                        break;
                    }
                    offset = jo["data"]["offset"].ToString();
                    if (offset == "")
                    {
                        break;
                    }
                    JArray jar = JArray.Parse(jo["data"]["items"].ToString());
                    foreach (var item in jar)
                    {
                        Bilibili bilibili = new Bilibili();
                        bilibili.Bilibili_UID = item["user"]["mid"].ToString();
                        bilibili.Bilibili_Uname = item["user"]["name"].ToString();
                        bilibili.Bilibili_Comment = item["desc"]["text"].ToString();

                        //去重
                        Bilibili bili = list.Find(x => x.Bilibili_UID == bilibili.Bilibili_UID);
                        if (bili == null)
                        {
                            list.Add(bilibili);
                        }
                    }
                    //添加翻页时间  防止IP被锁
                    Thread.Sleep(100);
                }
                textBox_Process.Text += "获取数据成功！\r\n";
                textBox_Process.Text += "\r\n\r\n";
                textBox_Process.Text += "总转发用户：" + dynamic.转发数 + "\r\n";
                textBox_Process.Text += "去重后用户：" + list.Count + "\r\n";

                if (dynamic.转发数 == 0)
                {
                    textBox_Process.Text += "没人转发！\r\n";
                }
                else
                {
                    textBox_Process.Text += "中奖用户信息:\r\n\r\n";

                    if (num_people.Value > list.Count)
                    {
                        num_people.Value = list.Count;
                    }

                    for (int i = 0; i < num_people.Value; i++)
                    {
                        //生成随机数
                        Random rd = new Random();
                        int Bilibili_Doge = rd.Next(0, list.Count);
                        textBox_Process.Text += "第" + (i + 1) + "位中奖用户：\r\n";
                        textBox_Process.Text += "用户ID：" + list[Bilibili_Doge].Bilibili_UID + "\r\n";
                        textBox_Process.Text += "用户名：" + list[Bilibili_Doge].Bilibili_Uname + "\r\n";
                        textBox_Process.Text += "转发详情：" + list[Bilibili_Doge].Bilibili_Comment + "\r\n\r\n";
                        list.RemoveAt(Bilibili_Doge);
                    }
                    textBox_Process.Text += "时间:"+DateTime.Now.ToString("f");
                }
            }

            else if (lab_Type.Text == "视频")
            {
                Video video = 获取视频信息(Dynamic_id);
                if (video == null)
                {
                    textBox_Process.Text = "获取视频信息失败!";
                    return;
                }
                list = new List<Bilibili>();
                int Tmp_count = 1;
                while (true)
                {
                    string DynamicAPI = "https://api.bilibili.com/x/v2/reply/main?csrf=da259c186696fad9c98e88ee5e3c96ec&mode=3&next=" + Tmp_count + "&oid=" + video.aid + "&plat=1&type=1";
                    string responseString = PublicHelp.New_Get(DynamicAPI,userMo.cookie_src);
                    if (responseString == "请求被拦截")
                    {
                        textBox_Process.Text = responseString + ",IP被封了，等一会再试吧!\r\n\r\n";
                        break;
                    }
                    if (responseString == "")
                    {
                        textBox_Process.Text = "获取视频评论区用户数据信息失败!\r\n\r\n";
                        break;
                    }

                    JObject jo = (JObject)JsonConvert.DeserializeObject(responseString);
                    if (jo["code"].ToString() != "0")
                    {
                        textBox_Process.Text = "获取动态转发用户数据信息失败!\r\n\r\n";
                        break;
                    }

                    string Top_replies = jo["data"]["replies"].ToString();
                    JArray jar = JArray.Parse(Top_replies);
                    if (jar == null || jar.Count == 0)
                        break;

                    foreach (var item in jar)
                    {
                        //主评论信息
                        Bilibili bilibili = new Bilibili();
                        bilibili.Bilibili_UID = item["member"]["mid"].ToString();
                        bilibili.Bilibili_Uname = item["member"]["uname"].ToString();
                        bilibili.Bilibili_Comment = item["content"]["message"].ToString();
                        //去重
                        Bilibili bili = list.Find(x => x.Bilibili_UID == bilibili.Bilibili_UID);
                        if (bili == null)
                        {
                            list.Add(bilibili);
                        }

                        //回复的信息列表 
                        string replies = item["replies"].ToString();
                        if (replies == "")
                            continue;
                        JArray jar_item = JArray.Parse(replies);
                        foreach (var jar_item_in in jar_item)
                        {
                            bilibili = new Bilibili();
                            bilibili.Bilibili_UID = jar_item_in["member"]["mid"].ToString();
                            bilibili.Bilibili_Uname = jar_item_in["member"]["uname"].ToString();
                            bilibili.Bilibili_Comment = jar_item_in["content"]["message"].ToString();
                            //去重
                            bili = list.Find(x => x.Bilibili_UID == bilibili.Bilibili_UID);
                            if (bili == null)
                            {
                                list.Add(bilibili);
                            }
                        }
                    }
                    Tmp_count++;
                    //添加翻页时间  防止IP被锁
                    Thread.Sleep(100);
                } 
                if (list != null && list.Count > 0)
                {
                    //去uP主自己
                    Bilibili bili_UP = list.Find(x => x.Bilibili_UID == txt_UP.Text.Trim());
                    if (bili_UP != null)
                    {
                        list.Remove(bili_UP);
                    }
                    textBox_Process.Text += "获取数据成功！\r\n";
                    textBox_Process.Text += "总评论用户：" + video.评论数 + "\r\n";
                    textBox_Process.Text += "去重后用户：" + list.Count + "\r\n";

                    if (video.评论数 == 0)
                    {
                        textBox_Process.Text += "没人评论！\r\n";
                    }
                    else
                    {
                        textBox_Process.Text += "中奖用户信息:\r\n\r\n";

                        if (num_people.Value > list.Count)
                        {
                            num_people.Value = list.Count;
                        }

                        for (int i = 0; i < num_people.Value; i++)
                        {
                            //生成随机数
                            Random rd = new Random();
                            int Bilibili_Doge = rd.Next(0, list.Count);
                            textBox_Process.Text += "第" + (i + 1) + "位中奖用户：\r\n";
                            textBox_Process.Text += "用户ID：" + list[Bilibili_Doge].Bilibili_UID + "\r\n";
                            textBox_Process.Text += "用户名：" + list[Bilibili_Doge].Bilibili_Uname + "\r\n";
                            textBox_Process.Text += "评论详情：" + list[Bilibili_Doge].Bilibili_Comment + "\r\n\r\n";
                            list.RemoveAt(Bilibili_Doge);
                        }
                        textBox_Process.Text += "时间:" + DateTime.Now.ToString("f");
                        textBox_Process.Text += "\r\n\r\n -*- B站抽奖工具由创客宏万提供 程序开源 -*- ";
                    }
                }
                else
                {
                    textBox_Process.Text += "B站接口没有反馈 请重试\r\n";
                }
            }
            else
            {
                textBox_Process.Text += "信息不对 不能抽奖\r\n";
            }
        }
        bool The_Timer = false;
        private void button4_Click(object sender, EventArgs e)
        {
            The_Timer = !The_Timer;
            if (The_Timer)
            {
                button4.Text = "取消定时";
            }
            else
            {
                button4.Text = "定时抽奖";
            }   
        }
    }
}