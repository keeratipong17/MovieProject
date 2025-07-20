using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace MovieProject
{
    public partial class FrmMovie : Form
    {
        //สร้างตัวแปรเก็บรูปที่แปลงเป็น Binary/Byte Array เอาไว้บันทึกลง DB
        byte[] movieImage;
        public FrmMovie()
        {
            InitializeComponent();
        }

        //เมธอดแปลง Binary เป็น รูป
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        //สร้างเมธอดแปลงรูปเป็น Binary/Byte Array
        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void getAllMenuToListView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open(); //เปิดการเชื่อมต่อไปยังฐานข้อมูล

                    //สร้างคำสั่ง SQL ในที่นี้คือ ดึงข้อมูลทั้งหมดจากตาราง menu_tb
                    string strSQL = "SELECT movieId, movieName, movieDetail, movieDate, movieType   FROM movie_tb";

                    //จัดการให้ SQL ทำงาน
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL ซึ่งเป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน dataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //ตั้งค่าทั่วไปของ ListView
                        lvShowAllMovie.Items.Clear();
                        lvShowAllMovie.Columns.Clear();
                        lvShowAllMovie.FullRowSelect = true;
                        lvShowAllMovie.View = View.Details;

                        lvShowSearchMovie.Items.Clear();
                        lvShowSearchMovie.Columns.Clear();
                        lvShowSearchMovie.FullRowSelect = true;
                        lvShowSearchMovie.View = View.Details;

                        //กำหนดรายละเอียดของ Column ใน ListView
                        lvShowSearchMovie.Columns.Add("รหัสภาพยนต์", 80, HorizontalAlignment.Left);
                        lvShowSearchMovie.Columns.Add("ชื่อภาพยนต์", 80, HorizontalAlignment.Left);

                        lvShowAllMovie.Columns.Add("รูปภาพยนต์", 80, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ชื่อภาพยนต์", 80, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("รายละเอียด", 150, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("วันที่เข้าฉาย", 80, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนต์", 80, HorizontalAlignment.Right);

                        //Loop วนเข้าไปใน DataTable
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); //สร้าง item เพื่อเก็บแต่ละข้อมูลในแต่ละรายการ

                            //เอารูปใส่ใน item
                            Image movieImage = null;
                            if (dataRow["movieImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["movieImage"];
                                //แปลงข้อมูลรูปจากฐานข้อมูลซึ่งเป็น Binary ให้เป็นรูป
                                movieImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (movieImage != null)
                            {
                                imageKey = $"movie_{dataRow["movieId"]}";
                                lvShowAllMovie.SmallImageList.Images.Add(imageKey, movieImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }



                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }
        }
        

        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            getAllMenuToListView();
            cbbMovieType.SelectedIndex = 0;
            btUpdateMovie.Enabled = false;
            btDeleteMovie.Enabled = false;

           

        }

    }
}
