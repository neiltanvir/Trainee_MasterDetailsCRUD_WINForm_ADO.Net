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
using System.IO;
using MasterDetailsCRUDAPP.Report;

namespace MasterDetailsCRUDAPP
{
    public partial class Form1 : Form
    {
        int inTraineeID = 0;
        bool isDefaultImage = true;
        string strConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Data\\TraineeDB.mdf;Integrated Security=True", strPreviousImage = "";
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        void Clear()
        {
            txtTraineeCode.Text = txtTraineeName.Text = "";
            cmbCourse.SelectedIndex = cmbGender.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtRegular.Checked = true;
            if (dgvTraineeTSP.DataSource == null)
                dgvTraineeTSP.Rows.Clear();
            else
                dgvTraineeTSP.DataSource = (dgvTraineeTSP.DataSource as DataTable).Clone();
            inTraineeID = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\defaultImage.jpg");
            isDefaultImage = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CourseComboBoxFill();
            FillTraineeDataGridView();
            Clear();
        }

        void CourseComboBoxFill()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Course", sqlCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                DataRow topItem = dtbl.NewRow();
                topItem[0] = 0;
                topItem[1] = "-Select-";
                dtbl.Rows.InsertAt(topItem, 0);
                cmbCourse.ValueMember = dgvcmbCourse.ValueMember = "CourseID";
                cmbCourse.DisplayMember = dgvcmbCourse.DisplayMember = "Course";
                cmbCourse.DataSource = dtbl;
                dgvcmbCourse.DataSource = dtbl.Copy();
            }
        }

        private void btnImageBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = new Bitmap(ofd.FileName);
                isDefaultImage = false;
                strPreviousImage = "";
            }
        }

        private void btnImageClear_Click(object sender, EventArgs e)
        {
            pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultImage.jpg");
            isDefaultImage = true;
            strPreviousImage = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int _TraineeID = 0;
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    //Master
                    SqlCommand sqlCmd = new SqlCommand("TraineeAddOrEdit", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@TraineeID", inTraineeID);
                    sqlCmd.Parameters.AddWithValue("@TraineeCode", txtTraineeCode.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@TraineeName", txtTraineeName.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@CourseID", Convert.ToInt32(cmbCourse.SelectedValue));
                    sqlCmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    sqlCmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                    sqlCmd.Parameters.AddWithValue("@State", rbtRegular.Checked ? "Regular" : "Irregular");
                    if (isDefaultImage)
                        sqlCmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    else if (inTraineeID > 0 && strPreviousImage != "")
                        sqlCmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                    else
                        sqlCmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    _TraineeID = Convert.ToInt32(sqlCmd.ExecuteScalar());
                }
                //Details
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    foreach (DataGridViewRow dgvRow in dgvTraineeTSP.Rows)
                    {
                        if (dgvRow.IsNewRow) break;
                        else
                        {
                            SqlCommand sqlCmd = new SqlCommand("TraineeTSPAddOrEdit", sqlCon);
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("@TraineeTSPID", Convert.ToInt32(dgvRow.Cells["dgvtxtTraineeTSPID"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtTraineeTSPID"].Value));
                            sqlCmd.Parameters.AddWithValue("@TraineeID", _TraineeID);
                            sqlCmd.Parameters.AddWithValue("@TSPName", dgvRow.Cells["dgvtxtTSPName"].Value == DBNull.Value ? "" : dgvRow.Cells["dgvtxtTSPName"].Value);
                            sqlCmd.Parameters.AddWithValue("@CourseID", Convert.ToInt32(dgvRow.Cells["dgvcmbCourse"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvcmbCourse"].Value));
                            sqlCmd.Parameters.AddWithValue("@Round", Convert.ToInt32(dgvRow.Cells["dgvtxtRound"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtRound"].Value));
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                FillTraineeDataGridView();
                Clear();
                MessageBox.Show("Submitted Successfully");
            }
        }

        bool ValidateMasterDetailForm()
        {
            bool _isValid = true;
            if (txtTraineeName.Text.Trim() == "")
            {
                MessageBox.Show("Trainee Name is required");
                _isValid = false;
            }
            //Add more validations if needed.
            return _isValid;
        }

        string SaveImage(string _imagePath)
        {
            string _fileName = Path.GetFileNameWithoutExtension(_imagePath);
            string _extension = Path.GetExtension(_imagePath);
            //shorten image name
            _fileName = _fileName.Length <= 15 ? _fileName : _fileName.Substring(0, 15);
            _fileName = _fileName + DateTime.Now.ToString("yymmssfff") + _extension;
            pbxPhoto.Image.Save(Application.StartupPath + "\\Images\\" + _fileName);
            return _fileName;
        }

        void FillTraineeDataGridView()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("TraineeViewAll", sqlCon);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                dgvTrainee.DataSource = dtbl;
                dgvTrainee.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvTrainee.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvTrainee.Columns[0].Visible = false;
            }

        }

        private void dgvTrainee_DoubleClick(object sender, EventArgs e)
        {
            if (dgvTrainee.CurrentRow.Index != -1)
            {
                DataGridViewRow _dgvCurrentRow = dgvTrainee.CurrentRow;
                inTraineeID = Convert.ToInt32(_dgvCurrentRow.Cells[0].Value);
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("TraineeViewByID", sqlCon);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDa.SelectCommand.Parameters.AddWithValue("@TraineeID", inTraineeID);
                    DataSet ds = new DataSet();
                    sqlDa.Fill(ds);

                    //Master - Fill
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtTraineeCode.Text = dr["TraineeCode"].ToString();
                    txtTraineeName.Text = dr["TraineeName"].ToString();
                    cmbCourse.SelectedValue = Convert.ToInt32(dr["CourseID"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DOB"].ToString());
                    cmbGender.Text = dr["Gender"].ToString();
                    if (dr["State"].ToString() == "Regular")
                        rbtRegular.Checked = true;
                    else
                        rbtIrregular.Checked = true;
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultImage.jpg");
                        isDefaultImage = true;
                    }
                    else
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        isDefaultImage = false;
                    }
                    dgvTraineeTSP.AutoGenerateColumns = false;
                    dgvTraineeTSP.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl.SelectedIndex = 0;
                }
            }
        }

        private void dgvTraineeTSP_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvTraineeTSP.CurrentRow;
            if (dgvRow.Cells["dgvtxtEmpCompID"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are You Sure to Delete this Record ?", "Master Detail CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                    {
                        sqlCon.Open();
                        SqlCommand sqlCmd = new SqlCommand("TraineeTSPDelete", sqlCon);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@TraineeTSPID", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCompID"].Value));
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                else
                    e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CrystalReportForm cf = new CrystalReportForm();
            cf.Show();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you Sure to Delete this Record ?", "Master Detail CRUD", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand("TraineeDelete", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@TraineeID", inTraineeID);
                    sqlCmd.ExecuteNonQuery();
                    Clear();
                    FillTraineeDataGridView();
                    MessageBox.Show("Deleted Successfully");
                };
            }
        }
    }
}
