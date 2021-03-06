﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class _Default : System.Web.UI.Page
{
    DB_Helper db = new DB_Helper();
    public DataRow R;
    public int UserCount;
    void BindLookupDropdown(string type, DropDownList l)
    {
        DataSet d = db.GetDataSetCache("select id_lookup,text from lookups where type='" + type + "'");
        l.DataTextField = "text";
        l.DataValueField = "id_lookup";
        l.DataSource = d;
        l.DataBind();
        l.Items.Insert(0, "");
    }

    public string GetLink(object o)
    {
        DataRowView r = ((DataRowView)o);
        if (r["ActionType"].ToString() == "Messaged") return "/Account/Chat?id=" + r["id_user"].ToString();

        string profile =  Utils.GetProfileLink(o);
        return profile;
    }

    List<DataRow> featured;

    DataRow GetOneFeatured()
    {
        if (featured.Count == 0) return null;
        DataRow x= featured[0];
        featured.RemoveAt(0);
        return x;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        rnd = new Random(DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour); //changes every hour

        featured = GetFeaturesMembers();

        Page.Form.Attributes["data-form"] = "subscribe";
        string sex = MyUtils.IsMale ? "F" : "M";
        string sql = "";


        photospromo.Visible = MyUtils.GetUserField("id_photo") is DBNull;

        sql = "exec GET_RECENT_ACTIVITY "+MyUtils.ID_USER;
        db.IgnoreDuplicateKeys = true; //getting multiple id_user in GET_RECENT_ACTIVITY
        DataSet d1 = GetUsers(sql,MyUtils.ImageSize.TINY,false);


        db.IgnoreDuplicateKeys = false;
        repRecentUsersList.DataSource = d1;
        repRecentUsersList.DataBind();
        noactivity.Visible = d1.Tables[0].Rows.Count == 0;

        Filter f = new Filter();
        f.orderby = orderby_type.NEWEST;
        sql = f.GetSQL(1, 10);

//        sql = "exec SEARCH_USERS 1,10,0," + MyUtils.ID_USER + ",'cast (datediff(year,birthdate,getdate()-datepart(dy,birthdate)+1) as int) BETWEEN 18 and 29 and id_photo is not null and sex=''" + sex + "''',1,'GUID, UsersTable.id_user, username as usr,place as plc,title as tit,distance as dis,age,lastlogin_time','id_user desc'";
        DataSet d2 = GetUsers(sql);
        repNewUsers.DataSource = d2;
        repNewUsers.DataBind();

        f.orderby = orderby_type.RECENT;
        sql = f.GetSQL(1, 10);
//        sql = "exec SEARCH_USERS 1,10,0," + MyUtils.ID_USER + ",'cast (datediff(year,birthdate,getdate()-datepart(dy,birthdate)+1) as int) BETWEEN 18 and 29 and id_photo is not null and sex=''" + sex + "''',1,'GUID, UsersTable.id_user, username as usr,place as plc,title as tit,distance as dis,age,lastlogin_time','lastlogin_time desc'";
        DataSet d3 = GetUsers(sql);
        repRecently.DataSource = d3;
        repRecently.DataBind();

    }


    private DataSet GetUsers(string sql,MyUtils.ImageSize sz=MyUtils.ImageSize.SMALL,bool deleterows=true)
    {

        DataSet d = db.GetDataSet(sql);

        MixInFeatured(d, deleterows);


        d.Tables[0].Columns.Add("mainphoto_");
        d.Tables[0].Columns.Add("online");
        foreach (DataRow r in d.Tables[0].Rows)
        {
            r["mainphoto_"] = MyUtils.GetImageUrl(r, sz);
            r["online"] = MyUtils.IsOnline((int)r["id_user"]) ? "1" : "0";// ..GetImageUrl(r, MyUtils.ImageSize.MEDIUM);
        }
        d.Tables[0].Columns.Remove("mainphoto");
        d.Tables[0].Columns["mainphoto_"].ColumnName = "MainPhoto";

        d.Tables[0].TableName = "U";
        //DataTable t = new DataTable();
        //t.TableName = "filter";
        //t.Columns.Add("json");
        //t.Rows.Add(t.NewRow());
        //t.Rows[0]["json"] = f.ToString();
        //d.Tables.Add(t);
        return d;

    }
    Random rnd;
    private void MixInFeatured(DataSet d, bool deleterows)
    {
        HashSet<int> uniq = new HashSet<int>();

        foreach (DataRow r in d.Tables[0].Rows) uniq.Add(Convert.ToInt32(r["id_user"]));

        DataRowCollection rows = d.Tables[0].Rows;
        int originalcount = rows.Count;
        for (int i = 0; i < 3; i++)
        {
            DataRow r = GetOneFeatured();
            if (r == null) break;
            if (uniq.Contains(Convert.ToInt32(r["id_user"])))
            {
                featured.Add(r); //put it back!
                continue;
            }
            DataRow n= d.Tables[0].NewRow();
            foreach (DataColumn c in n.Table.Columns)
                if (r.Table.Columns.IndexOf(c.ColumnName) >= 0) n[c] = r[c.ColumnName];
                else if (c.ColumnName == "ActionType") n[c] = "Featured Member";
            
            rows.InsertAt(n, rnd.Next(rows.Count)); //insert at random location
        }

        int todelete = rows.Count - originalcount;
        if (deleterows && todelete > 0)
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (rows[i]["vip"].ToString() != "Y") { rows.RemoveAt(i); i++; todelete--; }
                if (todelete == 0) break;
            }
    }

    public string GetActivity(object id_user, object time)
    {
        if (time == null || time == DBNull.Value) return "";
        string s=MyUtils.TimeAgo(Convert.ToDateTime(time));
        return s;
    }

    List<DataRow> GetFeaturesMembers()
    {
        List<DataRow> r = new List<DataRow>();
        DataSet d = db.GetDataSetCache("exec SEARCH_USERS 1,10,0,15187,' (MEMBERSHIP=''VIP'' or (MEMBERSHIP is null and isnull(CREDITS,0)>0)) and isnull(Cast([dbo].[fnCalcDistanceMiles](@lat,@long,latitude,longitude) as Decimal(6,0)),0)<=100 and sex=''" + (MyUtils.IsFemale ? "M" : "F") + "''',2,'GUID, UsersTable.id_user, username as usr,place as plc,title as tit,distance as dis,age,lastlogin_time','NEWID()'", 60);
        foreach (DataRow aa in d.Tables[0].Rows) r.Add(aa);
        return r;
    }

    public string UpdateOffer(object dataItem)
    {
        DataRowView userRow = (DataRowView)dataItem;
        int? mystate = null;
        int? thierstate = null;
        string res = "";

        string a= userRow["ActionType"].ToString();

        switch (a)
        {
            case "Viewed":
                a = "Viewed your profile";
                break;
            case "Messaged":
                a = "Messaged you";
                break;
            case "Accepted":
                a = "Accepted " + (Convert.ToDouble(userRow["IOfferedThem"])).ToString("c0") + " offer";
                break;
            case "Rejected":
                a = "Rejected " + (Convert.ToDouble(userRow["IOfferedThem"])).ToString("c0") + " offer";
                break;
            case "Favorited":
                a = "Favorited you";
                break;
            case "Winked":
                a = "Winked at you";
                break;
            case "Offered":
                a = "Offered you " + (Convert.ToDouble(userRow["TheyOfferedMe"])).ToString("c0");
                break;

        }

        return a;

        if (userRow["My_id_offer_state"] != DBNull.Value) mystate = Convert.ToInt32(userRow["My_id_offer_state"]);
        if (userRow["Their_id_offer_state"] != DBNull.Value) thierstate = Convert.ToInt32(userRow["Their_id_offer_state"]);

        //no offer yet
        //this.BUT_SENDOFFER.Visible = mystate == null && thierstate == null;

        string offeredamount = "";
        if (userRow["IOfferedThem"] != DBNull.Value) offeredamount = Convert.ToDouble(userRow["IOfferedThem"]).ToString("C0");
        if (userRow["TheyOfferedMe"] != DBNull.Value) offeredamount = Convert.ToDouble(userRow["TheyOfferedMe"]).ToString("C0");

        //pending
        if (mystate == 403 && thierstate == null) res = "You sent "+ offeredamount + " offer";
        if (mystate == null && thierstate == 403)
        {
            //they are asking...
            //DIV_ACCEPTCOUNTERREJECT.Visible = true;
            res = SheHe(userRow["sex"].ToString(), "She is asking ", "He is offering ") + " " + Convert.ToDouble(userRow["TheyOfferedMe"]).ToString("C0");
        }
        //rejected
        if (mystate == null && thierstate == 405) res = "You've rejected " + SheHe(userRow["sex"].ToString(), "her", "his") + " " + Convert.ToDouble(userRow["TheyOfferedMe"]).ToString("C0") + " offer";
        if (mystate == 405 && thierstate == null) res = SheHe(userRow["sex"].ToString(), "She", "He") + " rejected your " + Convert.ToDouble(userRow["IOfferedThem"]).ToString("C0") + " offer";


        //        OfferAmount.InnerText = offeredamount;

        if (mystate == 404 || thierstate == 404)
        {
            //BUT_SENDMESSAGE.Visible = true;
            //agreedprice.Visible = true;
            //agreedprice.InnerText = offeredamount;
        }

        //bool IsUnlocked = userRow["Unlocked"].ToString() == "1";
        //if (MyUtils.IsFemale || IsUnlocked) BUT_SENDMESSAGE.InnerHtml = BUT_SENDMESSAGE.InnerHtml.Replace("[SEND MESSAGE]", "Send Message");
        //else
        //{
        //    BUT_SENDMESSAGE.InnerHtml = BUT_SENDMESSAGE.InnerHtml.Replace("[SEND MESSAGE]", "Unlock Messaging for " + Convert.ToInt32(userRow["UnlockCredits"]) + " credits");
        //}

        //if (res != "") OfferText.Style["display"] = "block";

        //        BUT_SENDMESSAGE.HRef = "/Account/Chat?id=" + userRow["id_user"].ToString();
        //BUT_SENDMESSAGE.ServerClick += BUT_SENDMESSAGE_ServerClick;

        //        if (MyUtils.IsFemale) BUT_SENDOFFER.Attributes.Add("data-counter", "1");

        return res;
    }

    string SheHe(string orig, string she, string he)
    {
        if (orig == "F") return she;
        return he;
    }

    protected void btnSendCode_Click(object sender, EventArgs e)
    {
    }
}