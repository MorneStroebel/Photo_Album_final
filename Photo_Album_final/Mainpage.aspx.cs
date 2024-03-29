﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Photo_Album_final
{
    public partial class mainpage : System.Web.UI.Page
    {
        string DbConnect = ConfigurationManager.ConnectionStrings["dbconection"].ConnectionString;
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader datar;
        String sql, sqlinsert, sqlupdate, sqldelete;
        SqlDataAdapter adpt;
        String userid = "";
        string connectionString = ConfigurationManager.AppSettings["Storageconnection"].ToString();
        string accountname = "project2photostorage";
        string myid = "";
        string imageid = "";
        string sendid = "";
        string reciever = "";
        String albumname;
        string photourl;
        ImageButton imgbtn2 = new ImageButton();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] != null)
            {

                con = new SqlConnection(DbConnect);


                con.Open();

                sql = "SELECT * FROM users WHERE user_email = '" + Session["Username"].ToString() + "'";

                cmd = new SqlCommand(sql, con);

                datar = cmd.ExecuteReader();
                if (datar.Read())
                {
                    userid = datar.GetValue(0).ToString();
                    welcomelabel.Text = datar.GetValue(2).ToString();
                }

                con.Close();
                datar.Close();
                cmd.Dispose();

                if (Session["foto"] != null)
                {
                    if (!IsPostBack)
                    {
                        con = new SqlConnection(DbConnect);

                        con.Open();

                        sql = "SELECT * FROM photos WHERE photo_name = '" + Session["foto"].ToString() + "' AND users_user_id = '" + userid + "'";

                        cmd = new SqlCommand(sql, con);
                        datar = cmd.ExecuteReader();

                        while (datar.Read())
                        {
                            ImageButton imgbtn = new ImageButton();
                            imgbtn.ImageUrl = datar.GetValue(3).ToString();
                            imgbtn.Width = Unit.Pixel(150);
                            imgbtn.Height = Unit.Pixel(150);
                            imgbtn.Style.Add("padding", "5px");
                            imgbtn.Style.Add("margin", "2px");
                            imgbtn.Click += new ImageClickEventHandler(imgbtn_Click);
                            viewallpanel.Controls.Add(imgbtn);
                        }

                        con.Close();
                        datar.Close();
                        cmd.Dispose();
                        Session["foto"] = null;
                    }
                }
                else
                {


                    con.Open();

                    sql = "SELECT * FROM photos where users_user_id ='" + userid + "'";

                    cmd = new SqlCommand(sql, con);

                    datar = cmd.ExecuteReader();

                    while (datar.Read())
                    {
                        ImageButton imgbtn = new ImageButton();
                        imgbtn.ImageUrl = datar.GetValue(3).ToString();
                        imgbtn.Width = Unit.Pixel(150);
                        imgbtn.Height = Unit.Pixel(150);
                        imgbtn.Style.Add("padding", "5px");
                        imgbtn.Style.Add("margin", "2px");
                        imgbtn.Click += new ImageClickEventHandler(imgbtn_Click);
                        viewallpanel.Controls.Add(imgbtn);
                    }
                    datar.Close();
                    cmd.Dispose();
                    con.Close();
                }
            }
            else
            {
                searchpanel.Visible = false;
                viewallpanel.Visible = false;
                welcomelabel.Text = "Please Login!";
                logout.Text = "Login";
            }
        }
        protected void btnsearch_Click(object sender, EventArgs e)
        {
                
                con = new SqlConnection(DbConnect);

                con.Open();

                
                sql = "SELECT * FROM photos WHERE photo_name = '" + search.Text + "' AND users_user_id = '" + userid + "'";

                cmd = new SqlCommand(sql, con);
                datar = cmd.ExecuteReader();

                if (datar.Read())
                {
                    Session["foto"] = search.Text;
                    Response.Redirect("Mainpage.aspx");
                    search.Text = "";
                }
                else
                {
                    viewallpanel.Controls.Clear();
                    Label error = new Label();
                    error.Text = "Could not find foto! click on view all to show all photos!";
                    viewallpanel.Controls.Add(error);  
                }

                datar.Close();
                cmd.Dispose();
                con.Close();
                
        }
        protected void imgbtn2_Click(object sender, ImageClickEventArgs e)
        {

                photourl = ((ImageButton)sender).ImageUrl.ToString();

                Image1.ImageUrl = photourl;

                photopanel.Visible = true;
                searchpanel.Visible = false;
                viewallpanel.Visible = false;
                Session["foto"] = null;
        }
        protected void imgbtn_Click(object sender, ImageClickEventArgs e)
        {

            photourl = ((ImageButton)sender).ImageUrl.ToString();
            
            Image1.ImageUrl = photourl;

            photopanel.Visible = true;
            searchpanel.Visible = false;
            viewallpanel.Visible = false;
        }
        protected void Backbtn_Click(object sender, EventArgs e)
        {
            viewallpanel.Visible = true;
            searchpanel.Visible = true;
            photopanel.Visible = false;
            uploadpanel.Visible = false;

            uploaderror.Visible = false;
            uploaderror.Text = "";
            fototxb.Text = "";

            cancelbtn.Visible = false;
            changenametxb.Text = "";
            changenametxb.Visible = false;
            changelbl.Visible = false;

            deletebtn.Enabled = true;
            addbtn.Enabled = true;
            Sharebtn.Enabled = true;
            changenamebtn.Enabled = true;
            Downloadbtn.Enabled = true;
            Sharebtn.Text = "Share";
            Downloadbtn.Text = "Download";
            addbtn.Text = "Add to album";

            users.Visible = false;
        }
        protected void logout_Click(object sender, EventArgs e)
        {
            if (logout.Text == "Logout")
            {
                Session.RemoveAll();
                Response.Redirect("Login.aspx");
            }
            else if (logout.Text == "Login")
            {
                welcomelabel.Text = "Please login";
                Response.Redirect("Login.aspx");
            }
        }
        protected void btnupload_Click(object sender, EventArgs e)
        {
            uploadpanel.Visible = true;
            viewallpanel.Visible = false;
            searchpanel.Visible = false;
        }
        protected void savenbtn_Click(object sender, EventArgs e)
        {
            string ext, path, filename, photoname, Userid = "", azurepath;

            uploaderror.Visible = false;

            uploaderror.Text = "";

            if (FileUpload1.HasFile)
            {
                filename = FileUpload1.FileName;
                path = Server.MapPath("~\\photos\\");
                ext = System.IO.Path.GetExtension(filename);

                if (ext == ".jpg" || ext == ".png" || ext == ".gif")
                {
                    if (fototxb.Text != "")
                    {

                        photoname = fototxb.Text;
                        azurepath = "https://project2photostorage.blob.core.windows.net/";
                        azurepath += welcomelabel.Text.ToLower();
                        azurepath += "/";
                        azurepath += FileUpload1.FileName;

                        con = new SqlConnection(DbConnect);

                        con.Open();

                        sql = "SELECT * FROM photos WHERE photo_name = '" + photoname + "' AND users_user_id = '" + userid + "'";

                        cmd = new SqlCommand(sql, con);

                        datar = cmd.ExecuteReader();

                        if (datar.Read())
                        {
                            uploaderror.Visible = true;
                            uploaderror.Text = "Filename already exists";

                            fototxb.Text = "";

                            con.Close();
                            datar.Close();
                            cmd.Dispose();
                        }
                        else
                        {
                            try
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                    path += System.IO.Path.GetFileName(FileUpload1.FileName);
                                    

                                    FileUpload1.SaveAs(path);

                                    StorageCredentials creden = new StorageCredentials(accountname, connectionString);
                                    CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                                    CloudBlobClient client = acc.CreateCloudBlobClient();
                                    CloudBlobContainer cont = client.GetContainerReference(welcomelabel.Text.ToLower());

                                    cont.CreateIfNotExists();
                                    cont.SetPermissions(new BlobContainerPermissions
                                    {
                                        PublicAccess = BlobContainerPublicAccessType.Blob
                                    });


                                    CloudBlockBlob cblob = cont.GetBlockBlobReference(FileUpload1.FileName);

                                    using (Stream file = System.IO.File.OpenRead(path))
                                    {
                                        cblob.UploadFromStream(file);
                                    }

                                    con = new SqlConnection(DbConnect);

                                    con.Open();

                                    sql = "SELECT * FROM users WHERE user_name = '" + welcomelabel.Text + "'";

                                    cmd = new SqlCommand(sql, con);

                                    datar = cmd.ExecuteReader();
                                    if (datar.Read())
                                        Userid = datar.GetValue(0).ToString();

                                    con.Close();
                                    datar.Close();
                                    cmd.Dispose();


                                    con.Open();

                                    adpt = new SqlDataAdapter();

                                    sqlinsert = "INSERT INTO photos (users_user_id, photo_name, photo_path) values( '" + Userid + "','" + photoname + "','" + azurepath + "')";

                                    cmd = new SqlCommand(sqlinsert, con);
                                    adpt.InsertCommand = new SqlCommand(sqlinsert, con);
                                    adpt.InsertCommand.ExecuteNonQuery();


                                    File.Delete(path);

                                    Response.Redirect("Mainpage.aspx");
                                    viewallpanel.Visible = true;
                                    searchpanel.Visible = true;
                                    photopanel.Visible = false;
                                    uploadpanel.Visible = false;
                                    uploaderror.Visible = false;
                                    uploaderror.Text = "";
                                    fototxb.Text = "";
                                }
                                else
                                {
                                    path += System.IO.Path.GetFileName(FileUpload1.FileName);
                                    FileUpload1.SaveAs(path);

                                    StorageCredentials creden = new StorageCredentials(accountname, connectionString);
                                    CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                                    CloudBlobClient client = acc.CreateCloudBlobClient();
                                    CloudBlobContainer cont = client.GetContainerReference(welcomelabel.Text.ToLower());

                                    cont.CreateIfNotExists();
                                    cont.SetPermissions(new BlobContainerPermissions
                                    {
                                        PublicAccess = BlobContainerPublicAccessType.Blob
                                    });


                                    CloudBlockBlob cblob = cont.GetBlockBlobReference(FileUpload1.FileName);

                                    using (Stream file = System.IO.File.OpenRead(path))
                                    {
                                        cblob.UploadFromStream(file);
                                    }

                                    con = new SqlConnection(DbConnect);

                                    con.Open();

                                    sql = "SELECT * FROM users WHERE user_name = '" + welcomelabel.Text + "'";

                                    cmd = new SqlCommand(sql, con);

                                    datar = cmd.ExecuteReader();
                                    if (datar.Read())
                                        Userid = datar.GetValue(0).ToString();

                                    con.Close();
                                    datar.Close();
                                    cmd.Dispose();


                                    con.Open();

                                    adpt = new SqlDataAdapter();

                                    sqlinsert = "INSERT INTO photos (users_user_id, photo_name, photo_path) values( '" + Userid + "','" + photoname + "','" + azurepath + "')";

                                    cmd = new SqlCommand(sqlinsert, con);
                                    adpt.InsertCommand = new SqlCommand(sqlinsert, con);
                                    adpt.InsertCommand.ExecuteNonQuery();


                                    File.Delete(path);

                                    Response.Redirect("Mainpage.aspx");
                                    viewallpanel.Visible = true;
                                    searchpanel.Visible = true;
                                    photopanel.Visible = false;
                                    uploadpanel.Visible = false;
                                    uploaderror.Visible = false;
                                    uploaderror.Text = "";
                                    fototxb.Text = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                uploaderror.Visible = true;
                                uploaderror.Text = "File Not Uploaded!!" + ex.Message.ToString();
                            }
                        }
                    }
                    else
                    {
                        uploaderror.Visible = true;
                        uploaderror.Text = "Please enter a name for the photo!";
                        fototxb.Text = "";

                    }
                }
                else
                {
                    uploaderror.Visible = true;
                    uploaderror.Text = "photos need to be in .jpg, .png or .gif format!";
                    fototxb.Text = "";
                }
            }
            else
            {
                uploaderror.Visible = true;
                uploaderror.Text = "Please select a photo!";
            }
        }
        protected void Downloadbtn_Click(object sender, EventArgs e)
        {
            string url = Image1.ImageUrl.ToString();
            String Imagename = System.IO.Path.GetFileName(url);
            String ext = System.IO.Path.GetExtension(Imagename);
            String name = "";
            Response.ClearContent();
            Response.Buffer = true;

            if (Downloadbtn.Text == "Download")
            {
                String filename = "";


                con = new SqlConnection(DbConnect);

                con.Open();

                sql = "SELECT * FROM send_photo WHERE users_user_id = '" + userid + "'";

                cmd = new SqlCommand(sql, con);

                datar = cmd.ExecuteReader();

                if (datar.Read())
                {
                    filename = datar.GetValue(2).ToString();
                }
                con.Close();
                datar.Close();
                cmd.Dispose();

                con = new SqlConnection(DbConnect);

                con.Open();

                sql = "SELECT * FROM photos WHERE photo_path = '" + url + "'";

                cmd = new SqlCommand(sql, con);

                datar = cmd.ExecuteReader();

                if (datar.Read())
                {
                    name = datar.GetValue(2).ToString();
                }
                name += ext;
                con.Close();
                datar.Close();
                cmd.Dispose();

                StorageCredentials creden = new StorageCredentials(accountname, connectionString);
                CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                CloudBlobClient client = acc.CreateCloudBlobClient();
                CloudBlobContainer cont = client.GetContainerReference(welcomelabel.Text.ToLower());

                CloudBlockBlob cblob = cont.GetBlockBlobReference(Imagename);

                 MemoryStream memstream = new MemoryStream();


                cblob.DownloadToStream(memstream);


                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = cblob.Properties.ContentType.ToString();
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", name));
                Response.AddHeader("Content-Length", cblob.Properties.Length.ToString());
                Response.BinaryWrite(memstream.ToArray());

                StringWriter stringWriter = new StringWriter();
                HtmlTextWriter htmlTextWriter = new HtmlTextWriter(stringWriter);
                Response.Write(stringWriter.ToString());

                Response.End();


            }
            else if (Downloadbtn.Text == "Save")
            {
                try
                {
                    if (changenametxb.Text != "")
                    {
                        con = new SqlConnection(DbConnect);

                        con.Open();

                        sql = "SELECT * FROM photos WHERE photo_name = '" + changenametxb.Text + "' AND users_user_id = '" + userid + "'";

                        cmd = new SqlCommand(sql, con);

                        datar = cmd.ExecuteReader();

                        if (datar.Read())
                        {
                            changelbl.Visible = true;
                            changelbl.Text = "Filename already exists";

                            changenametxb.Text = "";

                            con.Close();
                            datar.Close();
                            cmd.Dispose();
                        }
                        else
                        {
                            con.Close();

                            con.Open();

                            adpt = new SqlDataAdapter();

                            sqlupdate = "UPDATE photos SET photo_name = '" + changenametxb.Text + "' WHERE photo_path = '" + Image1.ImageUrl.ToString() + "'";

                            cmd = new SqlCommand(sqlupdate, con);

                            adpt.InsertCommand = new SqlCommand(sqlupdate, con);
                            adpt.InsertCommand.ExecuteNonQuery();
                            Downloadbtn.Text = "Download";
                            changenamebtn.Text = "Change name";
                            changenametxb.Text = "";
                            changenametxb.Visible = false;

                            deletebtn.Enabled = true;
                            addbtn.Enabled = true;
                            Sharebtn.Enabled = true;
                            changenamebtn.Enabled = true;
                            cancelbtn.Visible = false;
                            changelbl.Visible = false;
                        }
                    }
                    else
                    {
                        changelbl.Visible = true;
                        changelbl.Text = "Enter a new name!";
                    }
                }
                catch
                {
                    changelbl.Visible = true;
                    changelbl.Text = "could not find image! ";
                }
            }
        }
        protected void Sharebtn_Click(object sender, EventArgs e)
        {
            users.Items.Clear();
            users.Items.Insert(0, new ListItem("--Select User--", "0"));

            con.Open();

            sql = "SELECT * FROM users WHERE user_name = '" + welcomelabel.Text + "'";

            cmd = new SqlCommand(sql, con);

            datar = cmd.ExecuteReader();

            if (datar.Read())
            {
                myid = datar.GetValue(2).ToString();
                con.Close();
                datar.Close();
                cmd.Dispose();
            }

            con = new SqlConnection(DbConnect);

            con.Open();

            sql = "SELECT * FROM photos WHERE photo_path = '" + Image1.ImageUrl.ToString() + "'";

            cmd = new SqlCommand(sql, con);

            datar = cmd.ExecuteReader();

            if (datar.Read())
            {
                imageid = datar.GetValue(1).ToString();
                con.Close();
                datar.Close();
                cmd.Dispose();
            }

            con = new SqlConnection(DbConnect);

            con.Open();

            sql = "SELECT* FROM users WHERE user_name != '" + myid + "'";

            cmd = new SqlCommand(sql, con);

            datar = cmd.ExecuteReader();
            while (datar.Read())
            {
                users.Items.Add(new ListItem(datar.GetValue(2).ToString(), datar.GetValue(0).ToString()));
            }
            cmd.Dispose();
            con.Close();
            datar.Close();

            if (Sharebtn.Text == "Share")
            {
                users.Visible = true;
                cancelbtn.Visible = true;
                Sharebtn.Text = "Send";

                changenamebtn.Enabled = false;
                deletebtn.Enabled = false;
                Downloadbtn.Enabled = false;
            }
            else if (Sharebtn.Text == "Send")
            {

                if (reciever == "")
                {
                    changelbl.Visible = true;
                    changelbl.Text = "Please select a user to send to!";
                }
                else
                {

                    con = new SqlConnection(DbConnect);
                    con.Open();

                    sql = "SELECT * FROM users WHERE user_name = '" + reciever + "'";

                    cmd = new SqlCommand(sql, con);

                    datar = cmd.ExecuteReader();

                    if (datar.Read())
                    {
                        sendid = datar.GetValue(0).ToString();
                        con.Close();
                        datar.Close();
                        cmd.Dispose();
                    }


                    con.Close();

                    con.Open();

                    adpt = new SqlDataAdapter();

                    sqlinsert = "INSERT INTO send_photo (users_user_id, photos_photo_id, sender_name) values(  '" + sendid + "','" + imageid + "','" + myid + "')";

                    cmd = new SqlCommand(sqlinsert, con);
                    adpt.InsertCommand = new SqlCommand(sqlinsert, con);
                    adpt.InsertCommand.ExecuteNonQuery();


                    cmd.Dispose();
                    con.Close();
                    users.Visible = false;
                    Sharebtn.Text = "Share";
                    cancelbtn.Visible = false;

                    changenamebtn.Enabled = true;
                    deletebtn.Enabled = true;
                    Downloadbtn.Enabled = true;
                }
            }
        }
        protected void changenamebtn_Click(object sender, EventArgs e)
        {
            changenamebtn.Enabled = false;
            deletebtn.Enabled = false;
            addbtn.Enabled = false;
            Sharebtn.Enabled = false;

            changenametxb.Visible = true;
            Downloadbtn.Text = "Save";
            cancelbtn.Visible = true;

        }
        protected void search_TextChanged(object sender, EventArgs e)
        {

        }
        protected void btnviewall_Click(object sender, EventArgs e)
        {
            Session["foto"] = null;
            Response.Redirect("Mainpage.aspx");
        }
        protected void btnshared_Click(object sender, EventArgs e)
        {
            Session["foto"] = null;
            Response.Redirect("Shared.aspx");
        }
        protected void btnrecieved_Click(object sender, EventArgs e)
        {
            Session["foto"] = null;
            Response.Redirect("Recieved.aspx");
        }
        protected void btnalbums_Click(object sender, EventArgs e)
        {
            Session["foto"] = null;
            Response.Redirect("Albums.aspx");
        }
        protected void deletebtn_Click(object sender, EventArgs e)
        {
            string url = Image1.ImageUrl.ToString();
            String name = System.IO.Path.GetFileName(url);

            try
            {
                con.Open();
                adpt = new SqlDataAdapter();
                sqldelete = "DELETE photos WHERE photo_path = '" + url + "'";
                cmd = new SqlCommand(sqldelete, con);

                adpt.DeleteCommand = new SqlCommand(sqldelete, con);
                adpt.DeleteCommand.ExecuteNonQuery();

                StorageCredentials creden = new StorageCredentials(accountname, connectionString);
                CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                CloudBlobClient client = acc.CreateCloudBlobClient();
                CloudBlobContainer cont = client.GetContainerReference(welcomelabel.Text.ToLower());

                CloudBlockBlob cblob = cont.GetBlockBlobReference(name);
                cblob.Delete();

                con.Close();
                datar.Close();
                cmd.Dispose();
                Response.Redirect("Mainpage.aspx");
            }
            catch
            {
                changelbl.Visible = true;
                changelbl.Text = "could not delete photo!";
            }

        }
        protected void addbtn_Click(object sender, EventArgs e)
        {
            users.Items.Clear();
            users.Items.Insert(0, new ListItem("--Select album--", "0"));


            con = new SqlConnection(DbConnect);

            con.Open();

            sql = "SELECT * FROM photos WHERE photo_path = '" + Image1.ImageUrl.ToString() + "'";

            cmd = new SqlCommand(sql, con);

            datar = cmd.ExecuteReader();

            if (datar.Read())
            {
                Session["imageid"] = datar.GetValue(1).ToString();
                con.Close();
                datar.Close();
                cmd.Dispose();
            }

            con = new SqlConnection(DbConnect);

            con.Open();

            sql = "SELECT* FROM album WHERE users_user_id = '" + userid + "'";

            cmd = new SqlCommand(sql, con);

            datar = cmd.ExecuteReader();
            while (datar.Read())
            {
                users.Items.Add(new ListItem(datar.GetValue(2).ToString(), datar.GetValue(0).ToString()));
            }
            cmd.Dispose();
            con.Close();
            datar.Close();

            if (addbtn.Text == "Add to album")
            {
                addbtn.Text = "Add";
                users.Visible = true;
                cancelbtn.Visible = true;

                Sharebtn.Enabled = false;
                changenamebtn.Enabled = false;
                deletebtn.Enabled = false;
                Downloadbtn.Enabled = false;
            }
            else if (addbtn.Text == "Add")
            {

                if (albumname == "")
                {
                    changelbl.Visible = true;
                    changelbl.Text = "Please select a album!";
                }
                else
                {

                    con = new SqlConnection(DbConnect);
                    con.Open();

                    sql = "SELECT * FROM album WHERE album_name = '" + reciever + "'";

                    cmd = new SqlCommand(sql, con);

                    datar = cmd.ExecuteReader();

                    if (datar.Read())
                    {
                        sendid = datar.GetValue(1).ToString();
                        con.Close();
                        datar.Close();
                        cmd.Dispose();
                    }



                    con.Close();
                    con.Open();
                    adpt = new SqlDataAdapter();

                    sqlinsert = "INSERT INTO album_photos (photos_photo_id, album_album_id) values( '" + Session["imageid"].ToString() + "','" + sendid + "')";

                    cmd = new SqlCommand(sqlinsert, con);
                    adpt.InsertCommand = new SqlCommand(sqlinsert, con);
                    adpt.InsertCommand.ExecuteNonQuery();

                    cmd.Dispose();
                    con.Close();
                    users.Visible = false;
                    users.Visible = false;
                    Sharebtn.Text = "Share";
                    cancelbtn.Visible = false;

                    changenamebtn.Enabled = true;
                    deletebtn.Enabled = true;
                    Downloadbtn.Enabled = true;
                    Sharebtn.Enabled = true;
                }
                changelbl.Visible = true;
                changelbl.Text = reciever;
            }
        
        }
        protected void users_SelectedIndexChanged(object sender, EventArgs e)
        {
            reciever = users.SelectedItem.Text;
        }
        protected void cancelbtn_Click(object sender, EventArgs e)
        {
            Downloadbtn.Text = "Download";
            Sharebtn.Text = "Share";
            cancelbtn.Visible = false;
            changenametxb.Text = "";
            changenametxb.Visible = false;

            changenamebtn.Enabled = true;
            deletebtn.Enabled = true;
            addbtn.Enabled = true;
            Sharebtn.Enabled = true;
            Downloadbtn.Enabled = true;

            users.Visible = false;
        }
        protected void btnrecieved2_Click(object sender, EventArgs e)
        {
            Response.Redirect("Recieved2.aspx");
        }

    }

}