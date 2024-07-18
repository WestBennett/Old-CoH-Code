using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Net;
using System.IO;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using HtmlAgilityPack;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Created by The Philotic Knight, a.k.a. Westley H. Bennett
/// Parses the forums.homecomingservers.com Invision Community boards and creates a Dev Tracker webpage, showing all Dev posts backwards by DateTime
/// </summary>
namespace DevTracker
{
    public partial class DevTracker : System.Web.UI.Page
    {
        private DataTable MessageData = new DataTable();
        private DataTable DevData = new DataTable();
        private DataTable SubRows = new DataTable();
        List<Tuple<string, string, string, HtmlDocument>> htmlTuples = new List<Tuple<string, string, string, HtmlDocument>>();
        protected void Page_Load(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Get the Dev ID data
            GetDevData();
            DevData.TableName = "DevIDs";

            //Get the message data
            GetMessageData(DevData);
            MessageData.DefaultView.Sort = "DateTime DESC";
            MessageData.DefaultView.ApplyDefaultSort = true;
            MessageData = MessageData.DefaultView.ToTable();

            //Stick it on the screen
            LiteralControl literal = (LiteralControl)FindControl("content");
            if (literal == null)
            {
                literal = new LiteralControl();
                Page.Controls.Add(literal);
                literal.ID = "content";
            }
            string CoHBGColor = "3A5F71";
            string CoHBGColorDark = "2B4754";
            literal.Text = @"<html><head><meta charset=""ISO-8859-1"">" +
                "<title>PK's Dev Tracker</title></head>" +
                "<body bgcolor=#" + CoHBGColor + " text=#FFFFFF link=00FF7F vlink=#32CD32>" +
                @"<center><img src=""http://www.cityofplayers.com/DevTracker/MainLogo.png"" style=""max-width:100%;max-height: 100%;""><br/>" +
                @"<h1>PK's Dev Tracker</h1><br/><a href=""http://www.cityofplayers.com"">Return to City of Players</a></center>" +
                @"<table border=1 style=""max-width:auto;border-color:#2B4754;"">";

            foreach (DataRow row in MessageData.Rows)
            {
                string UserImageHTML = row["UserHTML"].ToString().Replace("<img src=", @"<img style=""max-width:75px;"" src=");
                string tdStyle = @"<td style=""vertical-align:top;max-width:auto;border-color:#" + CoHBGColorDark + @";background-color:#" + CoHBGColorDark + @";"">";
                string InsertionString = Environment.NewLine + String.Format(
                    @"<tr style=""max-width:auto;border-color:#" + CoHBGColorDark + @";background-color:#" + CoHBGColorDark + @";"">" +
                    tdStyle + "<center>{0}<br/><b><font color =#" + ((row["Group"].ToString() == "Titan Network") ? "6de67b":
                    (row["Group"].ToString() == "Developer") ? "00ffff" : "6495ED") + ">" + row["Name"] + @"</font></b></center></td>" +
                    tdStyle + @" <a href=""" + row["MessageLink"].ToString() + @""">" + row["DateTime"] + "</a><br/>" +
                    @"<b>{1}</b></td><td style=""vertical-align:top;max-width:auto;border-color:#" + CoHBGColorDark + @";"">" +
                    row["PreviewHTML"].ToString().Replace("''", "'") + "</td></tr>", UserImageHTML, row["Subject"]).Replace("\n", @"<br/>");
                //Remove unneccessary line breaks
                do
                {
                    InsertionString = InsertionString.Replace("<br/><br/>", "<br/>");
                } while (InsertionString.Contains("<br/><br/>"));
                literal.Text += InsertionString;
            }

            literal.Text += "</table></body></html>";
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// Gets the Dev current Membership Data from the website.
        /// </summary>
        /// <returns></returns>
        private void GetDevData()
        {
            try
            {
                HtmlWeb web = new HtmlWeb();

                Dictionary<string, string> DevURLs = new Dictionary<string, string>() {
                    {"Developer", "https://forums.homecomingservers.com/search/?&type=core_members&group[8]=1" },
                    {"Titan Network","https://forums.homecomingservers.com/search/?&type=core_members&group[10]=1" },
                    {"Staff", "https://forums.homecomingservers.com/staff/" }
                };

                DevData.Columns.AddRange(new DataColumn[] { new DataColumn("ID", typeof(int)), new DataColumn("Name"), new DataColumn("Group"), new DataColumn("URL") });
                DevData.PrimaryKey = new DataColumn[] { DevData.Columns["URL"] };

                foreach (KeyValuePair<string, string> DevURL in DevURLs)
                {
                    HtmlDocument htmlDoc = web.Load(DevURL.Value);

                    HtmlNodeCollection userNames = GetHTMLNodeByTagAttributePartialValue(htmlDoc.DocumentNode, "a", "class", "ipsUserPhoto");

                    string profileURL = "https://forums.homecomingservers.com/profile/";
                    foreach (HtmlNode name in userNames)
                    {
                        string[] elements = name.OuterHtml.Split(new string[] { profileURL },
                        StringSplitOptions.None)[1].Split('/')[0].Split('-');
                        StringBuilder sbName = new StringBuilder();
                        StringBuilder sbBaseURL = new StringBuilder();
                        int x = 0;
                        foreach (string element in elements)
                        {
                            if (x == 0)
                            {
                                x = 1;
                                sbBaseURL.Append(profileURL + element);
                                continue;
                            }
                            else if (x == 1)
                            {
                                x = 2;
                                sbName.Append(element.ToUpper());
                            }
                            else
                            {
                                sbName.Append(" " + element.ToUpper());
                                x++;
                            }
                            sbBaseURL.Append("-" + element);
                        }

                        int PageNum = 1;
                        string prefix = "/content/page/";
                        string suffix = "/?all_activity=1";
                        string url = sbBaseURL.ToString() + prefix + PageNum + suffix;
                        //Add the main page
                        try
                        {
                            DevData.Rows.Add(elements[0], sbName.ToString(), DevURL.Key, url);
                        }
                        catch
                        {
                            //Prevent duplicates
                        }
                        
                    }
                }

                List<Task> TaskList = new List<Task>();
                SubRows = DevData.Clone();
                foreach (DataRow dr in DevData.Rows)
                {
                    Task CurrentTask = Task.Run(() => GetSubrows(dr));
                    TaskList.Add(CurrentTask);
                }

                Task.WaitAll(TaskList.ToArray());

                DevData.Merge(SubRows);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the SubRows of each Dev Row, if any exist
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private async Task<bool> GetSubrows(DataRow dr)
        {
            try
            {
                //Look for other pages to add
                int maxPages = 0;
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument htmlDoc = await hw.LoadFromWebAsync(dr["URL"].ToString());

                HtmlNodeCollection nodes = GetHTMLNodeByTagAttributePartialValue(htmlDoc.DocumentNode, "a", "id", "elPagination");
                if (nodes.Count > 0)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        maxPages = int.Parse(node.InnerText.Split(new string[] { "&nbsp;" }, StringSplitOptions.None)[0].
                            Split(new string[] { " of " }, StringSplitOptions.None)[1].Trim());
                        break;
                    }
                }

                string prefix = "/content/page/";
                string suffix = "/?all_activity=1";

                if (maxPages > 1)
                {
                    for (int i = 2; i <= maxPages; i++)
                    {
                        string url = dr["URL"].ToString().Replace(prefix + "1" + suffix, "") + prefix + i + suffix;
                        SubRows.Rows.Add(dr["ID"].ToString(), dr["Name"].ToString(), dr["Group"], url);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the actual Message Data from the DevData rows
        /// </summary>
        /// <param name="DevData"></param>
        /// <returns></returns>
        private bool GetMessageData(DataTable DevData)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("DevID"),
                new DataColumn("Name"),
                new DataColumn("Group"),
                new DataColumn("MessageID", typeof(int)),
                new DataColumn("DateTime", typeof(DateTime)),
                new DataColumn("Subject"),
                new DataColumn("UserHTML"),
                new DataColumn("PreviewHTML"),
                new DataColumn("MessageLink")});

            // Collect all possible URLs into a list
            List<Task> TaskList = new List<Task>();

            foreach (DataRow dr in DevData.Rows)
            {
                Task CurrentTask = Task.Run(() => CreateTuple(dr["URL"].ToString(), dr["ID"].ToString(), dr["Name"].ToString(), dr["Group"].ToString()));
                TaskList.Add(CurrentTask);
            }
            Task.WaitAll(TaskList.ToArray());

            foreach (Tuple<string, string, string, HtmlDocument> tuple in htmlTuples)
            {
                HtmlNodeCollection topics = GetHTMLNodeByTagAttributePartialValue(tuple.Item4.DocumentNode, "li", "data-role", "activityItem");
                foreach (HtmlNode topic in topics)
                {
                    //Get the DateTime from element <time datetime='2019-09-12T09:44:23Z' title='09/12/19 04:44  AM'
                    HtmlNode dtValue = GetHTMLNodeByTagAttribute(topic, "time", "title")[0];
                    //Get the target URL from <a href='https://forums.homecomingservers.com/topic/10004-focused-feedback-pineapple-vs-pizza/?do=findComment&amp;comment=92873'
                    HtmlNodeCollection links = GetHTMLNodeByTagAttributePartialValue(topic, "a", "href", "?do=findComment&amp;comment=");
                    if (links.Count == 0) continue;
                    HtmlNode link = links[0];
                    DataRow dr = dt.NewRow();
                    dr["DevID"] = tuple.Item1;
                    dr["Name"] = tuple.Item2;
                    dr["Group"] = tuple.Item3;
                    dr["MessageID"] = link.OuterHtml.Split(new string[] { "comment=" }, StringSplitOptions.None)[1].Split(
                        new string[] { "'" }, StringSplitOptions.None)[0];
                    dr["DateTime"] = DateTime.Parse(dtValue.GetAttributeValue("title", ""));
                    dr["Subject"] = link.InnerText;
                    HtmlNode UserInfo = GetHTMLNodeByTagAttributePartialValue(topic, "a", "class", "ipsUserPhoto")[0];
                    dr["UserHTML"] = UserInfo.OuterHtml;
                    HtmlNodeCollection nodes = GetHTMLNodeByTagAttributePartialValue(topic, "div", "class", "ipsType_richText");
                    if (nodes.Count > 0)
                    {
                        HtmlNode PreviewHTML = nodes[0];
                        dr["PreviewHTML"] = PreviewHTML.OuterHtml;
                    }
                    dr["MessageLink"] = link.GetAttributeValue("href", "").Replace("&amp;", "&");

                    //See if this message already exists, before adding it
                    DataRow[] drs = dt.Select("MessageID = " + dr["MessageID"]);
                    if (drs.Length == 0) dt.Rows.Add(dr);
                }
            }

            MessageData = dt;
            return true;
        }

        /// <summary>
        /// Creates the Tuple of the UserID, UserName, and HTMLDoc for use later on
        /// </summary>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<bool> CreateTuple(string url, string id, string name, string group)
        {
            try
            {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument htmlDoc = await htmlWeb.LoadFromWebAsync(url);
                htmlTuples.Add(new Tuple<string, string, string, HtmlDocument>(id, name, group, htmlDoc));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to obtain information from SCoRE website! Website may be down." +
                    Environment.NewLine + "Actual error: " + ex.Message, ex);
            }
            return true;
        }

        /// <summary>
        /// Returns an HTMLNode based on the passed in ParentNode, tag, attribute, and value
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetHTMLNodeByTagAttributeValue(HtmlNode ParentNode, string tag, string attribute, string value)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
                    if (node.Attributes[attribute].Value.ToString() != value) continue;
                    returnValue.Add(node);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load HTML element due to malformed HTML: " +
                    Environment.NewLine + ParentNode.InnerHtml.ToString(), ex);
            }
        }

        /// <summary>
        /// Returns an HTMLNode based on the passed in ParentNode, tag, attribute, and a partial value
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetHTMLNodeByTagAttributePartialValue(HtmlNode ParentNode, string tag, string attribute, string value)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
                    if (!node.Attributes[attribute].Value.ToString().Contains(value)) continue;
                    returnValue.Add(node);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load HTML element due to malformed HTML: " +
                    Environment.NewLine + ParentNode.InnerHtml.ToString(), ex);
            }
        }

        /// <summary>
        /// Returns an HTMLNode based on the passed in ParentNode, tag, and attribute
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetHTMLNodeByTagAttribute(HtmlNode ParentNode, string tag, string attribute)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    if (!node.Attributes.Contains(attribute)) continue;
                    returnValue.Add(node);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load HTML element due to malformed HTML: " +
                    Environment.NewLine + ParentNode.InnerHtml.ToString(), ex);
            }
        }

        /// <summary>
        /// Returns an HTMLNode based on the passed in ParentNode and tag
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetHTMLNodeByTag(HtmlNode ParentNode, string tag)
        {
            try
            {
                HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
                HtmlNodeCollection nodes = GetAllNodes(ParentNode);

                foreach (HtmlNode node in nodes)
                {
                    if (node.Name != tag) continue;
                    returnValue.Add(node);
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load HTML element due to malformed HTML: " +
                    Environment.NewLine + ParentNode.InnerHtml.ToString(), ex);
            }
        }
        
        /// <summary>
        /// Recursive method that drills down and returns ALL nodes from a parent node
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <returns></returns>
        private HtmlNodeCollection GetAllNodes(HtmlNode ParentNode)
        {
            HtmlNodeCollection returnValue = new HtmlNodeCollection(ParentNode);
            foreach (HtmlNode node in ParentNode.ChildNodes)
            {
                returnValue.Add(node);
                GetChildNodes(node);
            }

            void GetChildNodes(HtmlNode node)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    returnValue.Add(n);
                    if (n.HasChildNodes)
                    {
                        GetChildNodes(n);
                    }
                }
            }
            return returnValue;
        }
    }
}