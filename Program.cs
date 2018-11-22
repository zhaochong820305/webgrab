using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Data;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace webgrab
{
    class Program
    {
        //ArrayList keyword;
        public static int chongfu = 0;
        public static int youxiao = 0;
        static void Main(string[] args)
        {
            string urlfx = string.Empty;
            string title = string.Empty;
            
            ArrayList keyword = keywords();
            DataTable dt = DBZheng.getDataTable("SELECT *  FROM [dbo].[caijiset] where cishu>0 and state=1 order by id desc ");
            if (dt != null && dt.Rows.Count > 0)
            {
                
                //string str = "";
                //DataRow dr = dt.Rows[0];                
                //string chinaname = dr["chinaname"].ToString();
                foreach (DataRow dr in dt.Rows)
                {
                    Caiji cj = new Caiji();
                    cj.id = Convert.ToInt16(dr["id"]);
                    cj.chinaname = dr["chinaname"].ToString();

                    cj.classname = dr["classname"].ToString();
                    cj.url = dr["url"].ToString();
                    cj.domain = dr["domain"].ToString();
                    cj.startstr = dr["startstr"].ToString();

                    cj.endstr = dr["endstr"].ToString();
                    cj.configstart = dr["configstart"].ToString();
                    cj.configend = dr["configend"].ToString();
                    cj.textstart = dr["textstart"].ToString();
                    cj.textend = dr["textend"].ToString();

                    cj.fwhaostart = dr["fwhaostart"].ToString();
                    cj.fwhaoend = dr["fwhaoend"].ToString();

                    cj.cwdatestart = dr["cwdatestart"].ToString();
                    cj.cwdateend = dr["cwdateend"].ToString();

                    cj.fwjigoustart = dr["fwjigoustart"].ToString();
                    cj.fwjigouend = dr["fwjigouend"].ToString();

                    cj.fbriqistart = dr["fbriqistart"].ToString();
                    cj.fbriqiend = dr["fbriqiend"].ToString();

                    cj.urlstart = dr["urlstart"].ToString();
                    cj.urlend = dr["urlend"].ToString();

                    cj.titlestart = dr["titlestart"].ToString();
                    cj.titleend = dr["titleend"].ToString();
                    try { cj.cityid = Convert.ToInt32(dr["cityid"]); }
                    catch { cj.cityid = 0; }
                    
                    Console.WriteLine("采集网站："+cj.chinaname);
                    Console.WriteLine("采集分类：" + cj.classname);
                    Console.WriteLine("采集网址：" + cj.url);
                    string Html = GetHtml(cj.url);
                    //Console.WriteLine(html);
                    //提取首页内容字符
                    if ((Html.IndexOf(cj.startstr.Trim())<0) || (Html.IndexOf(cj.endstr.Trim())<0))
                    {
                        string sql1 = @"update [dbo].[caijiset] set error=0 where id=" + cj.id + "";
                        int count1 = DBZheng.getRowsCount(sql1);
                        if(count1>0)
                        {
                            return;
                        }                        
                    }
                    string Introduce = Html.Substring(Html.IndexOf(cj.startstr.Trim()));
                    Introduce = Introduce.Remove(Introduce.IndexOf(cj.endstr.Trim())).Trim();

                    ArrayList al = GetMatchesStr(Introduce, "<a[^>]*?>.*?</a>");
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    int icount = 0;
                    int cf = 0;
                    foreach (object var in al)
                    {
                        string a = var.ToString().Replace("\"", "").Replace("'", "");
                        a = Regex.Replace(a, cj.urlstart, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);//提取url 地址 开始
                        //string[] urlname = a.Split(' target=_blank>');
                        urlfx = cj.domain.Trim() + a.Substring(0, a.IndexOf(cj.urlend )); //提取url 地址 结束
                        title = a.Substring(a.IndexOf(cj.titlestart) + cj.titlestart.Length);
                        title = title.Remove(title.IndexOf(cj.titleend.Trim())).Trim();
                        if (a.StartsWith("/"))
                            a = "" + cj.domain.Trim() + a;
                        if (!a.StartsWith("http://"))
                        {
                            a = "http://" + a;
                        }
                        else
                        {
                            a = "<a href=" + a;
                        }
                        
                        
                        sb.Append(a + "/r/n");
                        i++;
                        urlfx = urlfx.Replace("amp;", "");
                        icount += GetHtmlfeixi(urlfx.Trim(), title, cj, keyword);
                        

                    }
                    //Console.WriteLine(sb.ToString());//把提取到网址输出到一个textBox,每个链接占一行 

                    Console.WriteLine("共提取" + al.Count.ToString() + "个链接,过滤关键词，采集有效数据:"+ youxiao.ToString() + "条，过滤重复:" + chongfu.ToString()+"条，插入数据库成功" + icount + "条,");
                    chongfu = 0;
                    youxiao = 0;
                    if(icount>0)
                    { 
                        string sql = @"update [dbo].[caijiset] set cishu=0 where cishu=1 and id=" + cj.id + "; ";
                        
                        sql += " update [dbo].[caijiset] set lasttime='" + DateTime.Now + "',lastnum='" + icount + "' where id=" + cj.id + "; ";
                        sql += " INSERT INTO [dbo].[caijinum] ([caijiid],[caijitime],[caijinum]) VALUES('" + cj.id + "','" + DateTime.Now + "','" + icount + "');";
                        
                        int count = DBZheng.getRowsCount(sql);
                        //if (count > 0)
                        {
                            Console.WriteLine("采集配制id成功:" + cj.id);
                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine("没有查询到配制信息，请先配制政策采集信息！ " );
            }
            Console.WriteLine("程序将在10秒后，自动退出！ ");
            System.Threading.Thread.Sleep(10000);
        }
        public static int GetHtmlfeixi(string url, string title,Caiji cj, ArrayList keyword)
        {
            int count = 0;
            string sql = string.Empty;
            string Html = GetHtml(url.Trim());
            try
            {
                Html = Html.ToString().Replace("\"", "").Replace("'", "");

                string xinxi = Html.Substring(Html.IndexOf(cj.configstart.Trim()));
                xinxi = xinxi.Remove(xinxi.IndexOf(cj.configend.Trim())).Trim();//信息 //Response.Write( xinxi);

                string Introduce = Html.Substring(Html.IndexOf(cj.textstart.Trim()) + cj.textstart.Trim().Length); //全文内容
                Introduce = Introduce.Remove(Introduce.IndexOf(cj.textend.Trim())).Trim();//内容
                //关键词过滤
                bool bkeyword = false;
                string keys = "";
                //////取消关健过滤
                //////foreach (object k1 in keyword)
                //////{
                //////    if (Introduce.Contains(k1.ToString()) || title.Contains(k1.ToString()))
                //////    {
                //////        bkeyword = true;
                //////        keys = k1.ToString();
                //////        break;

                //////    }
                //////}
                //////if (bkeyword)
                //////
                {
                    youxiao++;
                    //string sql = @"INSERT INTO [dbo].[zhengce]
                    //               ([mingcheng]           ,[wenhao]           ,[faburiqi]           ,[fawendanwen]
                    //               ,[cengji]           ,[buwensheng]           ,[gongcheng]           ,[lingyu]
                    //               ,[yiju]           ,[mubiao]           ,[youxiaoqi]           ,[hangye]
                    //               ,[chanpin]            ,[zhengceqw]           ,[zcywdizhi]         ,[state]           ,[createdate]
                    //                          ,[userid])
                    //         VALUES
                    //               ('" + Common.strFilter(mingcheng.Text) + "','" + Common.strFilter(wenhao.Text) + "','" + Common.strFilter(faburiqi.Text) + "','" + Common.strFilter(fawendanwen.Text) + "','" +
                    //                     Common.strFilter(cengji.SelectedValue) + "','" + Common.strFilter(buwensheng.Text) + "','" + Common.strFilter(gongcheng.Text) + "','" + Common.strFilter(lingyu.Text) + "','" +
                    //                     Common.strFilter(yiju.Text) + "','" + Common.strFilter(mubiao.Text) + "','" + Common.strFilter(youxiaoqi.Text) + "','" + Common.strFilter(hangye.SelectedValue) + "','" +
                    //                     Common.strFilter(chanpin.Text) + "','" + Common.strFilter(content.Text) + "','" + Common.strFilter(zcywdizhi.Text) + "',1,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    //                     + "','" + Session["userid"] + "')";

                    string wenhao = chuli(xinxi, cj.fwhaostart, cj.fwhaoend, cj.fwhaostart.Length);
                    string faburiqi = chuli(xinxi, cj.cwdatestart, cj.cwdateend, cj.cwdatestart.Length);
                    faburiqi = shijiancl(faburiqi, faburiqi.Length);
                    string fawendanwen = chuli(xinxi, cj.fwjigoustart, cj.fwjigouend, cj.fwjigoustart.Length);
                    string youxiaoqi = chuli(xinxi, cj.fbriqistart, cj.fbriqiend, cj.fbriqistart.Length);
                    youxiaoqi = shijiancl(youxiaoqi, youxiaoqi.Length);
                    string sqlc = "select * from zhengce where mingcheng='"+ Common.strFilter(title) + "' and wenhao='" + Common.strFilter(wenhao) + "' ";
                    int icount = DBZheng.getSelectRowsCount(sqlc);
                    if (icount == 0)//过滤重复数据
                    {
                         sql = @"INSERT INTO [dbo].[zhengce]
                                       ([mingcheng]          ,[wenhao]           ,[faburiqi]           ,[fawendanwen],[youxiaoqi]
                                        ,[zhengceqw]           ,[zcywdizhi]         ,[state]           ,[createdate]
                                                  ,[userid],[caiji],[keys],[buwensheng],[url])
                                 VALUES
                                       ('" + Common.strFilter(title) + "','" + Common.strFilter(wenhao) + "','" + Common.strFilter(faburiqi) + "','" + Common.strFilter(fawendanwen) + "','" + Common.strFilter(youxiaoqi)
                                           + "','" + Common.strFilter(Introduce) + "','" + Common.strFilter(url) + "',1,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                 + "','888',1,'" + Common.strFilter(keys) + "','" + Common.strFilter(cj.cityid.ToString()) + "','" + Common.strFilter(cj.url.ToString()) + "')";
                        count = DBZheng.getRowsCount(sql);
                    }
                    else
                    {
                        icount++;
                        chongfu ++;
                    }
                    
                }

            }
            catch { return 0; }
            if (count > 0)
            {
                return 1;
            }   
            else
            {
                //数据插入错误处理
                string sql1 = "INSERT INTO[dbo].[Log]([tablename],[op],[sql],[update])VALUES('zhengce','INSERT','"+sql+"','"+DateTime.Now+"')";
                DBZheng.getRowsCount(sql1);
                return 0;
            }
                
        }

        public static string chuli(string text, string startstr, string endstr, int ilen)
        {
            //string str = string.Empty;
            string str = text.Substring(text.IndexOf(startstr) + ilen);
            str = str.Remove(str.IndexOf(endstr)).Trim();//内容
            str = str.Replace("<span>", "");//北京人民政府过滤
            //Regex reg1 = new Regex(@"<td.*>");
            //str = reg1.Match(str).Groups[1].Value;
            str = str.Replace("<td nowrap=>", "");//河北人民政府过滤
            str = str.Replace("<td height=25 nowrap=>", "");//河北人民政府过滤            
            return str;
        }

        public static string shijiancl(string shijian,  int ilen)
        {
            
            string  str = shijian;//
            
            str = str.Replace("年", "-");//河北人民政府过滤
            str = str.Replace("月", "-");//河北人民政府过滤    
            str = str.Replace("日", "");//河北人民政府过滤         
            return str;
        }

        /// <summary> 
        /// 得到Html页面 
        /// </summary> 
        public static string GetHtml(string url)
        {
            StreamReader sr = null;
            string str = "";
            try
            {
                
                //读取远程路径 
                WebRequest request = WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet));
                str = sr.ReadToEnd();
                sr.Close();
            }catch
            { }
            return str;
        }
        private static ArrayList keywords()
        {
            ArrayList al2 = new ArrayList();
            DataTable dt;
            string sql = @"SELECT   [ID]      ,[SettingID]      ,[Name]  FROM [dbo].[Setting] where [SettingID]='53' and (state=1 or state is null)";

            dt = DBZheng.getDataTable(sql);
            try
            {
                if (dt.Rows.Count > 0)
                {
                    string str = "";
                    //DataRow dr = dt.Rows[0];
                    
                    //int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        //keyword[i] = dr["Name"].ToString();
                        //i++;
                        al2.Add(dr["Name"].ToString());
                    }
                    
                }
            }
            catch
            {
                //myGrid. = 0;
            }
            finally
            {

            }
            return al2;
        }

        public static ArrayList GetMatchesStr(string htmlCode, string strRegex)
        {
            ArrayList al = new ArrayList();

            Regex r = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection m = r.Matches(htmlCode);

            for (int i = 0; i < m.Count; i++)
            {
                bool rep = false;
                string strNew = m[i].ToString();

                // 过滤重复的URL 
                foreach (string str in al)
                {
                    if (strNew == str)
                    {
                        rep = true;
                        break;
                    }
                }

                if (!rep) al.Add(strNew);
            }

            //al.Sort();

            return al;
        }
    }
}
