using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace kusrovikdb
{
    public partial class Form2 : Form
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["kusrovikdb.Properties.Settings.KAFEDRAConnectionString"].ConnectionString;
        SqlConnection cn = new SqlConnection(connectionString);
        DataSet teachers = new DataSet();
        DataSet subjects = new DataSet();
        DataSet groups = new DataSet();
        DataSet works = new DataSet();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            try
            {
                cn.Open();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            
            SqlCommand cm;
            //teachers
            cm = new SqlCommand("SELECT ID, Name FROM Teachers", cn);
            SqlDataAdapter teachersAdapter = new SqlDataAdapter(cm);
            teachersAdapter.Fill(teachers);
            comboBox1.DataSource = teachers.Tables[0];
            comboBox1.DisplayMember = "Name";
            //subjects
            cm = new SqlCommand("SELECT ID, Name FROM Subjects", cn);
            SqlDataAdapter subjectsAdapter = new SqlDataAdapter(cm);
            subjectsAdapter.Fill(subjects);
            comboBox2.DataSource = subjects.Tables[0];
            comboBox2.DisplayMember = "Name";
            //groups
            cm = new SqlCommand("SELECT ID, Name FROM Groups", cn);
            SqlDataAdapter groupsAdapter = new SqlDataAdapter(cm);
            groupsAdapter.Fill(groups);
            comboBox3.DataSource = groups.Tables[0];
            comboBox3.DisplayMember = "Name";
            //works
            cm = new SqlCommand("SELECT ID, Name FROM Works", cn);
            SqlDataAdapter worksAdapter = new SqlDataAdapter(cm);
            worksAdapter.Fill(works);
            comboBox4.DataSource = works.Tables[0];
            comboBox4.DisplayMember = "Name";

           // MessageBox.Show(teachers.Tables[0].Rows[0]["Name"].ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int TeacherID = int.Parse(teachers.Tables[0].Rows[comboBox1.SelectedIndex]["ID"].ToString());
                int SubjectID = int.Parse(subjects.Tables[0].Rows[comboBox2.SelectedIndex]["ID"].ToString());
                int GroupID = int.Parse(groups.Tables[0].Rows[comboBox3.SelectedIndex]["ID"].ToString());
                int WorkID = int.Parse(works.Tables[0].Rows[comboBox4.SelectedIndex]["ID"].ToString());
                int Semestr = int.Parse(textBox1.Text);
                string Work = works.Tables[0].Rows[comboBox4.SelectedIndex]["Name"].ToString().Trim();

                SqlCommand cm;
                cm = new SqlCommand("SELECT ID, Lect, Prac, Lab FROM SemesterPlan WHERE SubjectID=" + SubjectID + " AND Semester=" + Semestr, cn);
                SqlDataAdapter semesterAdapter = new SqlDataAdapter(cm);
                DataSet semestr = new DataSet();
                semesterAdapter.Fill(semestr);
                int ID = int.Parse(semestr.Tables[0].Rows[0]["ID"].ToString().Trim());
                string PlanHours = "0";

                switch (Work)
                {
                    case "Курсовая":
                        PlanHours = "dbo.HNorms(1,"+GroupID+")";
                        break;
                    case "Экзамен":
                        PlanHours = "dbo.HNorms(2," + GroupID + ")";
                        break;
                    case "Диплом":
                        PlanHours = "dbo.HNorms(3," + GroupID + ")";
                        break;
                    case "Зачет":
                        PlanHours = "dbo.HNorms(4," + GroupID + ")";
                        break;
                    case "Лекции":
                        int Lect = int.Parse(semestr.Tables[0].Rows[0]["Lect"].ToString().Trim());
                        PlanHours = Lect.ToString();
                        break;
                    case "Практики":
                        int Prac = int.Parse(semestr.Tables[0].Rows[0]["Prac"].ToString().Trim());
                        PlanHours = Prac.ToString();
                        break;
                    case "Лабы":
                        string buf = semestr.Tables[0].Rows[0]["Lab"].ToString().Trim();
                        int Lab = 0;
                        if (!string.IsNullOrWhiteSpace(buf))
                             Lab = int.Parse(buf);
                        PlanHours = Lab.ToString();
                        break;
                }

                cm = new SqlCommand(@"
                INSERT INTO Fixation (SubjectID, GroupID, WorkID, PlanHours, TeacherID, SemesterPlanID)
                VALUES("+SubjectID+", "+GroupID+", "+ WorkID+", "+PlanHours+", "+TeacherID+", "+ID+")", cn);
                cm.ExecuteNonQuery();

                //MessageBox.Show("Добавление прошло успешно", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Form f = Application.OpenForms["Form3"];
                if (f != null)
                {
                    (f as Form3).fillDataGrid();
                }
                Form f2 = Application.OpenForms["Form4"];
                if (f2 != null)
                {
                    (f2 as Form4).fillDataGrid();
                }


            }
            catch(Exception ex)
            {
               // MessageBox.Show("Ошибка в заполнении данных, проверьте правильность заполнения всех полей");
                if (ex.GetType().Name == "IndexOutOfRangeException")
                {
                    MessageBox.Show("Ошибка. Скорее всего не существует записи в таблице плана семестра с таким предметом и семестром");
                } else
                    MessageBox.Show(ex.Message);
            }

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            cn.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
