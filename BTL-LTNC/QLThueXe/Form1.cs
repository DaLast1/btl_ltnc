using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace QLThueXe
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        // Khai báo các biến toàn cục
        string BienSo = "";
        string MaHD = "";
        int GiaThue = 0;
        string thanhTien = "";
        string TrangThai = "";

        string connectStr = "Data Source = DESKTOP-0F2KRRS; Initial Catalog = QLTX; Integrated Security = True;";
        SqlConnection conn = null;

        // Thay đổi trạng thái Control
        public void changePropContr(bool val)
        {
            tab1_TenK.Enabled = val;
            tab1_RB1.Enabled = val;
            tab1_RB2.Enabled = val;
            tab1_SDT.Enabled = val;
            tab1_DiaChi.Enabled = val;
            tab1_LHThue.Enabled = val;
            tab1_TGBD.Enabled = val;
            tab1_TGT.Enabled = val;
            tab1_TGKT.Enabled = val;
            tab1_TT.Enabled = val;
        }

        // Reset trạng thái các Button
        public void resetBtnState(bool val)
        {
            tab1_btnDat.Enabled = val;
            tab1_btnHuy.Enabled = val;
            tab1_btnBatDau.Enabled = val;
            tab1_btnKet.Enabled = val;
        }

        // Hàm tạo Mã ngẫu nhiên
        public Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Update tình trạng của các xe theo thời gian thực
        public void updateTLPX()
        {
            try
            {
                string sql_getHD = "select * from HoaDon, Xe where HoaDon.TinhTrang = N'Chưa hoàn thành' and HoaDon.BienSo = Xe.BienSo;";
                SqlDataAdapter getHD = new SqlDataAdapter(sql_getHD, conn);
                DataTable dtHD = new DataTable();
                getHD.Fill(dtHD);

                foreach (DataRow row in dtHD.Rows)
                {
                    DateTime now = DateTime.Now;
                    DateTime TGBD = Convert.ToDateTime(row["TGBD"]);
                    DateTime TGKT = Convert.ToDateTime(row["TGKT"]);

                    TimeSpan spanNow = DateTime.Now.Subtract(new DateTime(2020, 1, 1, 0, 0, 0));
                    double tNow = spanNow.TotalSeconds;
                    TimeSpan spanTGKT = TGKT.Subtract(new DateTime(2020, 1, 1, 0, 0, 0));
                    double tTGKT = spanTGKT.TotalSeconds;

                    if (now >= TGBD)
                    {
                        if (now >= TGKT)
                        {
                            if (row["LoaiHinhThue"].ToString() == "Theo giờ")
                            {
                                int thanhTien = Convert.ToInt32(row["ThanhTien"]);
                                thanhTien += (int)((((tNow - tTGKT) / 60 / 15) + 1) * (GiaThue / 10 / 15));
                                row["ThanhTien"] = thanhTien;
                            }
                            else if (row["LoaiHinhThue"].ToString() == "Theo ngày")
                            {
                                int thanhTien = Convert.ToInt32(row["ThanhTien"]);
                                thanhTien += (int)((((tNow - tTGKT) / 60 / 60) + 1) * (GiaThue * 20 / 10 / 60));
                                row["ThanhTien"] = thanhTien;
                            }
                            row["TrangThai"] = "Quá giờ";
                        }
                        else
                        {
                            if (row["TrangThai"].ToString() != "Đang thuê")
                            {
                                row["TrangThai"] = "Tới giờ";
                            }
                        }
                    }
                    else
                    {
                        row["TrangThai"] = "Đã đặt";
                    }

                    string sql_update_X = "update Xe set TrangThai = N'"
                        + row["TrangThai"] + "' where BienSo = '"
                        + row["BienSo"] + "';";
                    string sql_update_HD = "update HoaDon set ThanhTien = "
                        + row["ThanhTien"] + " where BienSo = '"
                        + row["BienSo"] + "';";

                    SqlCommand update_X = new SqlCommand(sql_update_X, conn);
                    update_X.ExecuteNonQuery();
                    SqlCommand update_HD = new SqlCommand(sql_update_HD, conn);
                    update_HD.ExecuteNonQuery();
                }

                string sql_getbtn = "select * from Xe";
                SqlDataAdapter getbtn = new SqlDataAdapter(sql_getbtn, conn);
                DataTable dtX = new DataTable();
                getbtn.Fill(dtX);
                tab2_dataGridView.DataSource = dtX;

                List<Button> lsBtnXD = new List<Button>();
                List<Button> lsBtnXM = new List<Button>();
                List<Button> lsBtnOT = new List<Button>();

                foreach (DataRow row in dtX.Rows)
                {
                    Color bColor = new Color();
                    switch (row["TrangThai"])
                    {
                        case "Chưa thuê":
                            bColor = Color.Transparent;
                            break;
                        case "Đã đặt":
                            bColor = Color.Orange;
                            break;
                        case "Tới giờ":
                            bColor = Color.Yellow;
                            break;
                        case "Đang thuê":
                            bColor = Color.Lime;
                            break;
                        case "Quá giờ":
                            bColor = Color.Red;
                            break;
                    }

                    if (row["LoaiXe"].ToString() == "Xe đạp")
                    {
                        lsBtnXD.Add(new Button() { Text = row["BienSo"].ToString(), BackColor = bColor, Dock = DockStyle.Fill });
                    }
                    else if (row["LoaiXe"].ToString() == "Xe máy")
                    {
                        lsBtnXM.Add(new Button() { Text = row["BienSo"].ToString(), BackColor = bColor, Dock = DockStyle.Fill });
                    }
                    else if (row["LoaiXe"].ToString() == "Ô tô")
                    {
                        lsBtnOT.Add(new Button() { Text = row["BienSo"].ToString(), BackColor = bColor, Dock = DockStyle.Fill });
                    }
                }

                TLP_XeDap.Controls.Clear();
                TLP_XeMay.Controls.Clear();
                TLP_OTo.Controls.Clear();

                for (int i = 0; i < lsBtnXD.Count; i++)
                {
                    lsBtnXD[i].Click += btn_Click;
                    lsBtnXD[i].Leave += btn_Leave;
                    TLP_XeDap.Controls.Add(lsBtnXD[i]);
                }

                for (int i = 0; i < lsBtnXM.Count; i++)
                {
                    lsBtnXM[i].Click += btn_Click;
                    lsBtnXM[i].Leave += btn_Leave;
                    TLP_XeMay.Controls.Add(lsBtnXM[i]);
                }

                for (int i = 0; i < lsBtnOT.Count; i++)
                {
                    lsBtnOT[i].Click += btn_Click;
                    lsBtnOT[i].Leave += btn_Leave;
                    TLP_OTo.Controls.Add(lsBtnOT[i]);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Hàm lấy mã HoaDon
        public string getMaHD()
        {
            string sql_getMaHD = "select * from HoaDon where BienSo = '" + BienSo + "' and TinhTrang = N'Chưa hoàn thành';";
            SqlDataAdapter getMaHD = new SqlDataAdapter(sql_getMaHD, conn);
            DataTable dtMaHD = new DataTable();
            getMaHD.Fill(dtMaHD);

            if (dtMaHD.Rows.Count != 0)
            {
                return dtMaHD.Rows[0][0].ToString();
            }
            return MaHD;
        }

        // Check tình trạng các xe theo thời gian thực
        public void timerTick(object sender, EventArgs e)
        {
            try
            {
                updateTLPX();
                tab1_TGBD.Value = DateTime.Now;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        Queue<Button> btnQ = new Queue<Button>();

        // Click Button Xe
        public void btn_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                btnQ.Enqueue(btn);

                btnQ.Last().FlatStyle = FlatStyle.Flat;
                btnQ.Last().FlatAppearance.BorderSize = 1;
                btnQ.Last().FlatAppearance.BorderColor = Color.OrangeRed;

                if (btnQ.Count > 1)
                {
                    btnQ.First().FlatStyle = FlatStyle.Standard;
                    btnQ.Dequeue();
                }

                BienSo = btn.Text;

                string sql_select = "select * from Xe where BienSo = '" + btn.Text + "';";
                SqlDataAdapter daXe = new SqlDataAdapter(sql_select, conn);
                DataTable dtXe = new DataTable();
                daXe.Fill(dtXe);

                GiaThue = Convert.ToInt32(dtXe.Rows[0][4]);

                string sql_select_K = "select Khach.* from Khach, HoaDon where Khach.MaK = HoaDon.MaK and BienSo = '" + btn.Text + "' and TinhTrang = N'Chưa hoàn thành';";
                SqlDataAdapter daK = new SqlDataAdapter(sql_select_K, conn);
                DataTable dtK = new DataTable();
                daK.Fill(dtK);

                string sql_select_HD = "select * from HoaDon where BienSo = '" + btn.Text + "' and TinhTrang = N'Chưa hoàn thành';";
                SqlDataAdapter daHD = new SqlDataAdapter(sql_select_HD, conn);
                DataTable dtHD = new DataTable();
                daHD.Fill(dtHD);

                tab1_BSX.Text = dtXe.Rows[0][0].ToString();
                tab1_LoaiXe.Text = dtXe.Rows[0][1].ToString();
                tab1_Hang.Text = dtXe.Rows[0][2].ToString();
                tab1_KieuXe.Text = dtXe.Rows[0][3].ToString();
                tab1_GiaThue.Text = dtXe.Rows[0][4].ToString();

                if (dtK.Rows.Count != 0)
                {
                    tab1_TenK.Text = dtK.Rows[0][1].ToString() != "" ? dtK.Rows[0][1].ToString() : "";
                    tab1_RB1.Checked = dtK.Rows[0][2].ToString() == "Nam" ? true : false;
                    tab1_RB2.Checked = dtK.Rows[0][2].ToString() == "Nữ" ? true : false;
                    tab1_SDT.Text = dtK.Rows[0][3].ToString() != "" ? dtK.Rows[0][3].ToString() : "";
                    tab1_DiaChi.Text = dtK.Rows[0][4].ToString() != "" ? dtK.Rows[0][4].ToString() : "";
                }

                if (dtHD.Rows.Count != 0)
                {
                    tab1_LHThue.Text = dtHD.Rows[0][3].ToString() != "" ? dtHD.Rows[0][3].ToString() : "";
                    tab1_TGBD.Value = dtHD.Rows[0][5].ToString() != "" ? Convert.ToDateTime(dtHD.Rows[0][5]) : tab1_TGBD.Value;
                    tab1_TGKT.Value = dtHD.Rows[0][6].ToString() != "" ? Convert.ToDateTime(dtHD.Rows[0][6]) : tab1_TGKT.Value;
                    tab1_TGT.Value = dtHD.Rows[0][4].ToString() != "" ? Convert.ToDecimal(dtHD.Rows[0][4]) : tab1_TGT.Value;
                    tab1_TT.Text = dtHD.Rows[0][7].ToString() != "" ? dtHD.Rows[0][7].ToString() : "";
                    thanhTien = dtHD.Rows[0][7].ToString();
                }

                if (dtXe.Rows[0][5].ToString() == "Chưa thuê")
                {
                    changePropContr(true);
                    resetBtnState(false);

                    tab1_btnDat.Enabled = true;
                }
                else if (dtXe.Rows[0][5].ToString() == "Đã đặt")
                {
                    changePropContr(false);
                    resetBtnState(false);

                    tab1_btnHuy.Enabled = true;

                    MaHD = getMaHD();
                }
                else if (dtXe.Rows[0][5].ToString() == "Tới giờ")
                {
                    changePropContr(false);
                    resetBtnState(false);

                    tab1_btnBatDau.Enabled = true;

                    MaHD = getMaHD();
                }
                else if (dtXe.Rows[0][5].ToString() == "Đang thuê")
                {
                    changePropContr(false);
                    resetBtnState(false);

                    tab1_btnKet.Enabled = true;

                    MaHD = getMaHD();
                }
                else if (dtXe.Rows[0][5].ToString() == "Quá giờ")
                {
                    changePropContr(false);
                    resetBtnState(false);

                    tab1_btnKet.Enabled = true;

                    MaHD = getMaHD();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Leave Button Xe
        public void btn_Leave(object sender, EventArgs e)
        {
            try
            {
                changePropContr(true);

                tab1_TenK.Text = "";
                tab1_RB1.Checked = false;
                tab1_RB2.Checked = false;
                tab1_SDT.Text = "";
                tab1_DiaChi.Text = "";

                tab1_LHThue.Text = "";
                tab1_TGBD.Value = DateTime.Now;
                tab1_TGKT.Value = DateTime.Now;
                tab1_TGT.Value = tab1_TGT.Value;
                tab1_TT.Text = "";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Xử lí khi load Form
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                conn = new SqlConnection(connectStr);
                conn.Open();

                timer.Enabled = true;
                timer.Start();
                timer.Tick += new EventHandler(timerTick);

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Đặt chuyến
        private void tab1_btnDat_Click(object sender, EventArgs e)
        {
            try
            {
                string MaHD = RandomString(5);
                string MaK = RandomString(5);

                string TenK = tab1_TenK.Text;
                string GT = tab1_RB1.Checked ? "Nam" : tab1_RB2.Checked ? "Nữ" : "";
                string SDT = tab1_SDT.Text;
                string DiaChi = tab1_DiaChi.Text;

                if (TenK == "" || GT == "" || SDT == "" || DiaChi == "") throw new Exception("Something is missing...");

                string LHThue = tab1_LHThue.Text;
                string TGBD = tab1_TGBD.Value.ToString();
                string TGT = tab1_TGT.Value.ToString();
                string TGKT = tab1_TGKT.Value.ToString();
                string ThanhTien = tab1_TT.Text;

                if (LHThue == "" || ThanhTien == "") throw new Exception("Something is missing...");

                string sql_insert_k = "insert into Khach values('"
                    + MaK + "', N'"
                    + TenK + "', N'"
                    + GT + "', '"
                    + SDT + "', N'"
                    + DiaChi + "');";

                SqlCommand insert_k = new SqlCommand(sql_insert_k, conn);
                insert_k.ExecuteNonQuery();

                string sql_insert_hd = "insert into HoaDon values('"
                    + MaHD + "', '"
                    + MaK + "', '"
                    + BienSo + "', N'"
                    + LHThue + "', "
                    + TGT + ", '"
                    + TGBD + "', '"
                    + TGKT + "', "
                    + ThanhTien + ", N'Chưa hoàn thành');";

                SqlCommand insert_hd = new SqlCommand(sql_insert_hd, conn);
                insert_hd.ExecuteNonQuery();

                string sql_update_x = "update Xe set TrangThai = N'Đã đặt' where BienSo = '" + BienSo + "'";
                SqlCommand update_x = new SqlCommand(sql_update_x, conn);
                update_x.ExecuteNonQuery();

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật đơn vị và giá trị tối thiêu cho NumericUpdown Thời gian
        // dựa trên giá trị của ComboBox Loại hình
        private void tab1_LHThue_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab1_LHThue.Text == "Theo giờ")
                {
                    tab1_TGT.DecimalPlaces = 1;
                    tab1_TGT.Increment = 0.5M;
                    tab1_TGT.Minimum = 0.5M;
                }
                else if (tab1_LHThue.Text == "Theo ngày")
                {
                    tab1_TGT.DecimalPlaces = 0;
                    tab1_TGT.Increment = 1;
                    tab1_TGT.Minimum = 1;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật giá trị của DatetimePicker Kết thúc và Textbox Thành tiền
        // dựa trên giá trị của ComboBox Loại hình, NumericUpdown Thời gian và DatetimePicker Bắt đầu
        private void tab1_TGT_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab1_LHThue.Text == "Theo giờ")
                {
                    tab1_TGKT.Value = DateTime.Now;
                    tab1_TGKT.Value = tab1_TGBD.Value.AddHours((double)tab1_TGT.Value);
                    tab1_TT.Text = (Convert.ToInt32(tab1_TGT.Value * GiaThue)).ToString();
                }
                else if (tab1_LHThue.Text == "Theo ngày")
                {
                    tab1_TGKT.Value = DateTime.Now;
                    tab1_TGKT.Value = tab1_TGBD.Value.AddDays((double)tab1_TGT.Value);
                    tab1_TT.Text = (Convert.ToInt32(tab1_TGT.Value * GiaThue * 20)).ToString();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Hủy chuyến
        private void tab1_btnHuy_Click(object sender, EventArgs e)
        {
            try
            {
                string sql_delete = "delete from HoaDon where MaHD = '" + MaHD + "';";
                SqlCommand deleteHD = new SqlCommand(sql_delete, conn);
                deleteHD.ExecuteNonQuery();

                string sql_update_x = "update Xe set TrangThai = N'Chưa thuê' where BienSo = '" + BienSo + "'";
                SqlCommand update_x = new SqlCommand(sql_update_x, conn);
                update_x.ExecuteNonQuery();

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Bắt đầu chuyến
        private void tab1_btnBatDau_Click(object sender, EventArgs e)
        {
            try
            {
                string sql_update_x = "update Xe set TrangThai = N'Đang thuê' where BienSo = '" + BienSo + "'";
                SqlCommand update_x = new SqlCommand(sql_update_x, conn);
                update_x.ExecuteNonQuery();

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Kết thúc chuyến
        private void tab1_btnKet_Click(object sender, EventArgs e)
        {
            try
            {
                string sql_update_x = "update Xe set TrangThai = N'Chưa thuê' where BienSo = '" + BienSo + "'";
                SqlCommand update_x = new SqlCommand(sql_update_x, conn);
                update_x.ExecuteNonQuery();

                string sql_update_hd = "update HoaDon set ThanhTien = " + thanhTien + ", TinhTrang = N'Đã hoàn thành' where MaHD = '" + MaHD + "'";
                SqlCommand update_hd = new SqlCommand(sql_update_hd, conn);
                update_hd.ExecuteNonQuery();

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật Item tương ứng cho các ComboBox Hãng, Kiểu xe dựa trên giá trị của ComboBox Loại xe
        private void tab2_cbLoaiXe_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab2_cbLoaiXe.Text == "Xe đạp")
                {
                    tab2_cbHang.Text = "";
                    tab2_cbKieuXe.Text = "";

                    tab2_cbHang.Items.Clear();
                    tab2_cbKieuXe.Items.Clear();

                    tab2_cbHang.Items.Add("Raleigh");
                    tab2_cbHang.Items.Add("Focus");
                    tab2_cbHang.Items.Add("Felt");
                    tab2_cbHang.Items.Add("Specialized");
                    tab2_cbHang.Items.Add("Trek");
                    tab2_cbHang.Items.Add("Pinarello");
                    tab2_cbHang.Items.Add("Eddy Merckx");
                    tab2_cbHang.Items.Add("BMC");
                    tab2_cbHang.Items.Add("Giant");
                    tab2_cbHang.Items.Add("Salsa");

                    tab2_cbKieuXe.Items.Add("Xe đạp thường");
                    tab2_cbKieuXe.Items.Add("Xe đạp đôi");
                    tab2_cbKieuXe.Items.Add("Xe địa hình");
                }
                else if (tab2_cbLoaiXe.Text == "Xe máy")
                {
                    tab2_cbHang.Text = "";
                    tab2_cbKieuXe.Text = "";

                    tab2_cbHang.Items.Clear();
                    tab2_cbKieuXe.Items.Clear();

                    tab2_cbHang.Items.Add("Yamaha");
                    tab2_cbHang.Items.Add("Ducati");
                    tab2_cbHang.Items.Add("Honda");
                    tab2_cbHang.Items.Add("Royal Enfield");
                    tab2_cbHang.Items.Add("Kawasaki");
                    tab2_cbHang.Items.Add("BMW");
                    tab2_cbHang.Items.Add("Harley Davidson");
                    tab2_cbHang.Items.Add("Suzuki");
                    tab2_cbHang.Items.Add("SYM");
                    tab2_cbHang.Items.Add("Husqvarna");

                    tab2_cbKieuXe.Items.Add("Xe số");
                    tab2_cbKieuXe.Items.Add("Xe tay ga");
                    tab2_cbKieuXe.Items.Add("Xe tay côn");
                }
                else if (tab2_cbLoaiXe.Text == "Ô tô")
                {
                    tab2_cbHang.Text = "";
                    tab2_cbKieuXe.Text = "";

                    tab2_cbHang.Items.Clear();
                    tab2_cbKieuXe.Items.Clear();

                    tab2_cbHang.Items.Add("Tesla");
                    tab2_cbHang.Items.Add("BMW");
                    tab2_cbHang.Items.Add("Ferrari");
                    tab2_cbHang.Items.Add("Ford");
                    tab2_cbHang.Items.Add("Porsche");
                    tab2_cbHang.Items.Add("Honda");
                    tab2_cbHang.Items.Add("Lamborghini");
                    tab2_cbHang.Items.Add("Toyota");
                    tab2_cbHang.Items.Add("Bentley");
                    tab2_cbHang.Items.Add("Maserati");

                    tab2_cbKieuXe.Items.Add("Xe thể thao");
                    tab2_cbKieuXe.Items.Add("Xe 4 chỗ");
                    tab2_cbKieuXe.Items.Add("Xe 7 chỗ");
                    tab2_cbKieuXe.Items.Add("Xe 9 chỗ");
                    tab2_cbKieuXe.Items.Add("Xe 16 chỗ");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Thêm thông tin xe
        private void tab2_btThem_Click(object sender, EventArgs e)
        {
            try
            {
                if (tab2_tbBS.Text == "" || tab2_cbLoaiXe.Text == "" || tab2_cbHang.Text == "" || tab2_cbKieuXe.Text == "" || tab2_tbGia.Text == "") throw new Exception("Something is missing...");

                string sql_add = "insert into Xe values('"
                    + tab2_tbBS.Text + "', N'"
                    + tab2_cbLoaiXe.Text + "', N'"
                    + tab2_cbHang.Text + "', N'"
                    + tab2_cbKieuXe.Text + "', "
                    + tab2_tbGia.Text + ", N'Chưa thuê');";
                SqlCommand cmd = new SqlCommand(sql_add, conn);
                cmd.ExecuteNonQuery();

                tab2_cbLoaiXe.Text = "";
                tab2_cbHang.Text = "";
                tab2_cbKieuXe.Text = "";
                tab2_tbBS.Text = "";
                tab2_tbGia.Text = "";

                string sql_getXe = "select * from Xe";
                SqlDataAdapter getXe = new SqlDataAdapter(sql_getXe, conn);
                DataTable dtX = new DataTable();
                getXe.Fill(dtX);
                tab2_dataGridView.DataSource = dtX;

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật thông tin xe
        private void tab2_btSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (TrangThai != "Chưa thuê") throw new Exception("Cannot change information now...");

                string sql_update = "update Xe set LoaiXe = N'"
                + tab2_cbLoaiXe.Text + "', Hang = N'"
                + tab2_cbHang.Text + "', KieuXe = N'"
                + tab2_cbKieuXe.Text + "', GiaThue = "
                + tab2_tbGia.Text + " where BienSo = '" + tab2_tbBS.Text + "';";
                SqlCommand cmd = new SqlCommand(sql_update, conn);
                cmd.ExecuteNonQuery();

                string sql_getXe = "select * from Xe";
                SqlDataAdapter getXe = new SqlDataAdapter(sql_getXe, conn);
                DataTable dtX = new DataTable();
                getXe.Fill(dtX);
                tab2_dataGridView.DataSource = dtX;

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Xóa thông tin xe
        private void tab2_btXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (TrangThai != "Chưa thuê") throw new Exception("Cannot delete vehicle now...");

                string sql_delete = "delete from Xe where BienSo = '" + tab2_tbBS.Text + "';";
                SqlCommand cmd = new SqlCommand(sql_delete, conn);
                cmd.ExecuteNonQuery();

                tab2_cbLoaiXe.Text = "";
                tab2_cbHang.Text = "";
                tab2_cbKieuXe.Text = "";
                tab2_tbBS.Text = "";
                tab2_tbGia.Text = "";

                string sql_getXe = "select * from Xe";
                SqlDataAdapter getXe = new SqlDataAdapter(sql_getXe, conn);
                DataTable dtX = new DataTable();
                getXe.Fill(dtX);
                tab2_dataGridView.DataSource = dtX;

                updateTLPX();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Xác định thông tin xe từ Row đang được chọn trong DataGridView
        private void tab2_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = tab2_dataGridView.Rows[e.RowIndex];

                    tab2_tbBS.Text = row.Cells[0].Value.ToString();
                    tab2_cbLoaiXe.Text = row.Cells[1].Value.ToString();
                    tab2_cbHang.Text = row.Cells[2].Value.ToString();
                    tab2_cbKieuXe.Text = row.Cells[3].Value.ToString();
                    tab2_tbGia.Text = row.Cells[4].Value.ToString();
                    TrangThai = row.Cells[5].Value.ToString();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật Item tương ứng cho các ComboBox Hãng, Kiểu xe dựa trên giá trị của ComboBox Loại xe
        private void tab2_LoaiXe_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab2_LoaiXe.Text == "")
                {
                    tab2_Hang.Text = "";
                    tab2_KieuXe.Text = "";

                    tab2_Hang.Items.Clear();
                    tab2_KieuXe.Items.Clear();
                }
                else if (tab2_LoaiXe.Text == "Xe đạp")
                {
                    tab2_Hang.Text = "";
                    tab2_KieuXe.Text = "";

                    tab2_Hang.Items.Clear();
                    tab2_KieuXe.Items.Clear();

                    tab2_Hang.Items.Add("Raleigh");
                    tab2_Hang.Items.Add("Focus");
                    tab2_Hang.Items.Add("Felt");
                    tab2_Hang.Items.Add("Specialized");
                    tab2_Hang.Items.Add("Trek");
                    tab2_Hang.Items.Add("Pinarello");
                    tab2_Hang.Items.Add("Eddy Merckx");
                    tab2_Hang.Items.Add("BMC");
                    tab2_Hang.Items.Add("Giant");
                    tab2_Hang.Items.Add("Salsa");

                    tab2_KieuXe.Items.Add("Xe đạp thường");
                    tab2_KieuXe.Items.Add("Xe đạp đôi");
                    tab2_KieuXe.Items.Add("Xe địa hình");
                }
                else if (tab2_LoaiXe.Text == "Xe máy")
                {
                    tab2_Hang.Text = "";
                    tab2_KieuXe.Text = "";

                    tab2_Hang.Items.Clear();
                    tab2_KieuXe.Items.Clear();

                    tab2_Hang.Items.Add("Yamaha");
                    tab2_Hang.Items.Add("Ducati");
                    tab2_Hang.Items.Add("Honda");
                    tab2_Hang.Items.Add("Royal Enfield");
                    tab2_Hang.Items.Add("Kawasaki");
                    tab2_Hang.Items.Add("BMW");
                    tab2_Hang.Items.Add("Harley Davidson");
                    tab2_Hang.Items.Add("Suzuki");
                    tab2_Hang.Items.Add("SYM");
                    tab2_Hang.Items.Add("Husqvarna");

                    tab2_KieuXe.Items.Add("Xe số");
                    tab2_KieuXe.Items.Add("Xe tay ga");
                    tab2_KieuXe.Items.Add("Xe tay côn");
                }
                else if (tab2_LoaiXe.Text == "Ô tô")
                {
                    tab2_Hang.Text = "";
                    tab2_KieuXe.Text = "";

                    tab2_Hang.Items.Clear();
                    tab2_KieuXe.Items.Clear();

                    tab2_Hang.Items.Add("Tesla");
                    tab2_Hang.Items.Add("BMW");
                    tab2_Hang.Items.Add("Ferrari");
                    tab2_Hang.Items.Add("Ford");
                    tab2_Hang.Items.Add("Porsche");
                    tab2_Hang.Items.Add("Honda");
                    tab2_Hang.Items.Add("Lamborghini");
                    tab2_Hang.Items.Add("Toyota");
                    tab2_Hang.Items.Add("Bentley");
                    tab2_Hang.Items.Add("Maserati");

                    tab2_KieuXe.Items.Add("Xe thể thao");
                    tab2_KieuXe.Items.Add("Xe 4 chỗ");
                    tab2_KieuXe.Items.Add("Xe 7 chỗ");
                    tab2_KieuXe.Items.Add("Xe 9 chỗ");
                    tab2_KieuXe.Items.Add("Xe 16 chỗ");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Tìm kiếm xe
        private void tab2_btTimKiem_Click(object sender, EventArgs e)
        {
            try
            {
                string BS, LX, H, KX, TT;

                BS = tab2_BienSo.Text == "" ? "%" : tab2_BienSo.Text;
                LX = tab2_LoaiXe.Text == "" ? "%" : tab2_LoaiXe.Text;
                H = tab2_Hang.Text == "" ? "%" : tab2_Hang.Text;
                KX = tab2_KieuXe.Text == "" ? "%" : tab2_KieuXe.Text;
                TT = tab2_TT.Text == "" ? "%" : tab2_TT.Text;

                string sql_search = "select * from Xe where BienSo like '" +
                                BS + "' and LoaiXe like N'" + LX + "' and KieuXe like N'" +
                                KX + "' and Hang like '" + H + "' and TrangThai like N'" + TT + "'";
                SqlDataAdapter daSX = new SqlDataAdapter(sql_search, conn);
                DataTable dtSX = new DataTable();
                daSX.Fill(dtSX);
                tab2_dataGridView.DataSource = dtSX;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Cập nhật Item tương ứng cho các ComboBox chọn mốc thời gian dựa trên giá trị của ComboBox Thống kê theo
        private void tab3_LH_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tab3_LH.Text == "Tháng")
                {
                    tab3_N.Visible = true;

                    tab3_T1.Text = "";
                    tab3_T2.Text = "";

                    tab3_T1.Items.Clear();
                    tab3_T2.Items.Clear();

                    for (int i = 1; i <= 12; i++)
                    {
                        tab3_T1.Items.Add(i);
                        tab3_T2.Items.Add(i);
                    }
                }
                else if (tab3_LH.Text == "Năm")
                {
                    tab3_N.Visible = false;

                    tab3_T1.Text = "";
                    tab3_T2.Text = "";

                    tab3_T1.Items.Clear();
                    tab3_T2.Items.Clear();

                    for (int i = 2020; i <= 2025; i++)
                    {
                        tab3_T1.Items.Add(i);
                        tab3_T2.Items.Add(i);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Thống kê
        private void tab3_btnTK_Click(object sender, EventArgs e)
        {
            try
            {
                if (tab3_LH.Text == "") throw new Exception("Please choose type of statistics...");
                if (tab3_T1.Text == "" || tab3_T2.Text == "" || (tab3_N.Visible == true && tab3_N.Text == "")) throw new Exception("Please choose time period...");

                string sql_getData = "";
                string xVal = "", yVal = "";
                switch (tab3_DTTK.Text)
                {
                    case "Doanh thu":
                        yVal = "DoanhThu";
                        xVal = tab3_LH.Text == "Tháng" ? "Thang" : "Nam";
                        sql_getData = tab3_LH.Text == "Tháng" ?
                            "select SUM(ThanhTien) as DoanhThu, MONTH(TGKT) as Thang from HoaDon where MONTH(TGKT) >= '"
                            + tab3_T1.Text + "' and MONTH(TGKT) <= '"
                            + tab3_T2.Text + "' and YEAR(TGKT) = '"
                            + tab3_N.Text + "' and TinhTrang = N'Đã hoàn thành' group by MONTH(TGKT) order by Thang desc;" :
                            "select SUM(ThanhTien) as DoanhThu, YEAR(TGKT) as Nam from HoaDon where YEAR(TGKT) >= '"
                            + tab3_T1.Text + "' and YEAR(TGKT) <= '"
                            + tab3_T2.Text + "' and TinhTrang = N'Đã hoàn thành' group by YEAR(TGKT) order by Nam desc;";
                        break;
                    case "Lượt thuê":
                        yVal = "LuotThue";
                        xVal = tab3_LH.Text == "Tháng" ? "Thang" : "Nam";
                        sql_getData = tab3_LH.Text == "Tháng" ?
                            "select COUNT(MaHD) as LuotThue, MONTH(TGKT) as Thang from HoaDon where MONTH(TGKT) >= '"
                            + tab3_T1.Text + "' and MONTH(TGKT) <= '"
                            + tab3_T2.Text + "' and YEAR(TGKT) = '"
                            + tab3_N.Text + "' and TinhTrang = N'Đã hoàn thành' group by MONTH(TGKT) order by Thang desc;" :
                            "select COUNT(MaHD) as LuotThue, YEAR(TGKT) as Nam from HoaDon where YEAR(TGKT) >= '"
                            + tab3_T1.Text + "' and YEAR(TGKT) <= '"
                            + tab3_T2.Text + "' and TinhTrang = N'Đã hoàn thành' group by YEAR(TGKT) order by Nam desc;";
                        break;
                    case "Loại xe":
                        yVal = "DoanhThuLoai";
                        xVal = "LoaiXe";
                        sql_getData = tab3_LH.Text == "Tháng" ?
                            "select SUM(ThanhTien) as DoanhThuLoai, Xe.LoaiXe as LoaiXe from HoaDon, Xe where MONTH(TGKT) >= '"
                            + tab3_T1.Text + "' and MONTH(TGKT) <= '"
                            + tab3_T2.Text + "' and YEAR(TGKT) = '"
                            + tab3_N.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.LoaiXe order by DoanhThuLoai desc;" :
                            "select SUM(ThanhTien) as DoanhThuLoai, Xe.LoaiXe as LoaiXe from HoaDon, Xe where YEAR(TGKT) >= '"
                            + tab3_T1.Text + "' and YEAR(TGKT) <= '"
                            + tab3_T2.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.LoaiXe order by DoanhThuLoai desc;";
                        break;
                    case "Hãng":
                        yVal = "DoanhThuHang";
                        xVal = "Hang";
                        sql_getData = tab3_LH.Text == "Tháng" ?
                            "select SUM(ThanhTien) as DoanhThuHang, Xe.Hang as Hang from HoaDon, Xe where MONTH(TGKT) >= '"
                            + tab3_T1.Text + "' and MONTH(TGKT) <= '"
                            + tab3_T2.Text + "' and YEAR(TGKT) = '"
                            + tab3_N.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.Hang order by DoanhThuHang desc;" :
                            "select SUM(ThanhTien) as DoanhThuHang, Xe.Hang as Hang from HoaDon, Xe where YEAR(TGKT) >= '"
                            + tab3_T1.Text + "' and YEAR(TGKT) <= '"
                            + tab3_T2.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.Hang order by DoanhThuHang desc;";
                        break;
                    case "Kiểu xe":
                        yVal = "DoanhThuKieu";
                        xVal = "KieuXe";
                        sql_getData = tab3_LH.Text == "Tháng" ?
                            "select SUM(ThanhTien) as DoanhThuKieu, Xe.KieuXe as KieuXe from HoaDon, Xe where MONTH(TGKT) >= '"
                            + tab3_T1.Text + "' and MONTH(TGKT) <= '"
                            + tab3_T2.Text + "' and YEAR(TGKT) = '"
                            + tab3_N.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.KieuXe order by DoanhThuKieu desc;" :
                            "select SUM(ThanhTien) as DoanhThuKieu, Xe.KieuXe as KieuXe from HoaDon, Xe where YEAR(TGKT) >= '"
                            + tab3_T1.Text + "' and YEAR(TGKT) <= '"
                            + tab3_T2.Text + "' and TinhTrang = N'Đã hoàn thành' and HoaDon.BienSo = Xe.BienSo group by Xe.KieuXe order by DoanhThuKieu desc;";
                        break;
                }

                SqlDataAdapter daTK = new SqlDataAdapter(sql_getData, conn);
                DataTable dtTK = new DataTable();
                daTK.Fill(dtTK);

                tab3_DoThi.DataSource = dtTK;
                tab3_DoThi.Titles.Clear();
                tab3_DoThi.Series["Thống kê"]["PixelPointWidth"] = "50";
                tab3_DoThi.Series["Thống kê"].XValueMember = xVal;
                tab3_DoThi.Series["Thống kê"].YValueMembers = yVal;
                tab3_DoThi.Titles.Add("Thống kê " + tab3_DTTK.Text);

                tab3_Bang.DataSource = dtTK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Oops, something went wrong...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
