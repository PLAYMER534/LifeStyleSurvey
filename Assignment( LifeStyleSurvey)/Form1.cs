using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Assignment__LifeStyleSurvey_
{
    public partial class Form1 : Form
    {
        private SQLiteConnection? conn;

        // Panels
        private Panel? panelSurvey, panelResults;

        // Personal info controls
        private TextBox? txtName, txtEmail, txtContact;
        private DateTimePicker? dtpDOB;
        private CheckBox? cbPizza, cbPasta, cbPapWors, cbOther;
        private TextBox lblStats;

        // Rating Table
        private TableLayoutPanel? ratingTable;
        private string[] statements = {
            "I like to watch movies",
            "I like to listen to radio",
            "I like to eat out",
            "I like to watch TV"
        };
        private CheckBox[,]? ratingChecks;

        private Button? btnSubmit, btnShowChart;
        private DataGridView? dgvResults;
        private void ClearSurveyInputs()
        {
            foreach (Control ctrl in panelSurvey.Controls)
            {
                if (ctrl is TextBox tb)
                    tb.Clear();
                else if (ctrl is CheckBox cb)
                    cb.Checked = false;
                else if (ctrl is DateTimePicker dtp)
                    dtp.Value = DateTime.Now;
            }

            // Also clear ratings (checkboxes in ratingTable)
            for (int row = 0; row < ratingChecks!.GetLength(0); row++)
                for (int col = 0; col < ratingChecks.GetLength(1); col++)
                    ratingChecks[row, col].Checked = false;
        }

        public Form1()
        {


            InitializeComponent();
            InitializeDatabase();
            InitializeForm();
        }

        private void InitializeDatabase()
        {
            conn = new SQLiteConnection("Data Source=survey.db;Version=3;");
            conn.Open();
            var cmdAll = new SQLiteCommand("SELECT DOB FROM Survey WHERE DOB IS NOT NULL AND DOB != ''", conn);
            var readerAll = cmdAll.ExecuteReader();

            List<double> ages = new List<double>();

            while (readerAll.Read())
            {
                var dobStr = readerAll["DOB"].ToString();

                if (DateTime.TryParse(dobStr, out DateTime dob))
                {
                    double age = (DateTime.Now - dob).TotalDays / 365.25;
                    ages.Add(age);
                }
            }
            readerAll.Close();

            if (ages.Count > 0)
            {
                double avgAge = ages.Average();
                double maxAge = ages.Max();
                double minAge = ages.Min();

                // Now get the other aggregated stats in SQL separately or join here
            }
            else
            {
                // No valid DOBs
            }

            string sql = @"CREATE TABLE IF NOT EXISTS Survey (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT, Email TEXT, DOB TEXT, Contact TEXT,
                LikesPizza INTEGER, LikesPasta INTEGER, LikesPapWors INTEGER, LikesOther INTEGER,
                RateMovies INTEGER, RateRadio INTEGER, RateEatOut INTEGER, RateTV INTEGER
            )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();
        }

        private void InitializeForm()
        {
            Text = "Lifestyle Survey";
            Size = new Size(1000, 700);
            BackColor = Color.White;

            var menu = new MenuStrip();
            var menuSurvey = new ToolStripMenuItem("Fill Out Survey");
            var menuResults = new ToolStripMenuItem("View Results");
            menuSurvey.Click += (s, e) => ShowSurveyPanel();
            menuResults.Click += (s, e) =>
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter password to view results:", "Password Required", "");
                if (input == "12345") ShowResultsPanel(GetSize());
                else MessageBox.Show("Incorrect password.");
            };
            menu.Items.Add(menuSurvey);
            menu.Items.Add(menuResults);
            Controls.Add(menu);

            panelSurvey = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            panelResults = new Panel { Dock = DockStyle.Fill, Visible = false };
            Controls.Add(panelSurvey);
            Controls.Add(panelResults);

            InitializeSurveyPanel();
            InitializeResultsPanel();
        }

        private void InitializeSurveyPanel()
        {
            var y = 40;
            panelSurvey!.Controls.Add(CreateLabel("Full Names", 20, y));
            txtName = CreateTextBox(150, y); txtName.BackColor = Color.LightBlue; panelSurvey.Controls.Add(txtName); y += 35;
            panelSurvey.Controls.Add(CreateLabel("Email", 20, y));
            txtEmail = CreateTextBox(150, y); txtEmail.BackColor = Color.LightBlue; panelSurvey.Controls.Add(txtEmail); y += 35;
            panelSurvey.Controls.Add(CreateLabel("Date of Birth", 20, y));
            dtpDOB = new DateTimePicker { Location = new Point(150, y), Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" }; panelSurvey.Controls.Add(dtpDOB); y += 35;
            panelSurvey.Controls.Add(CreateLabel("Contact Number", 20, y));
            txtContact = CreateTextBox(150, y); txtContact.BackColor = Color.LightBlue; panelSurvey.Controls.Add(txtContact); y += 40;

            panelSurvey.Controls.Add(CreateLabel("What is your favorite food?", 20, y));

            cbPizza = new CheckBox { Text = "Pizza", AutoSize = true, Location = new Point(250, y), ForeColor = Color.Blue };
            cbPasta = new CheckBox { Text = "Pasta", AutoSize = true, Location = new Point(cbPizza.Right + 20, y), ForeColor = Color.Blue };
            cbPapWors = new CheckBox { Text = "Pap and Wors", AutoSize = true, Location = new Point(cbPasta.Right + 20, y), ForeColor = Color.Blue };
            cbOther = new CheckBox { Text = "Other", AutoSize = true, Location = new Point(cbPapWors.Right + 20, y), ForeColor = Color.Blue };

            panelSurvey.Controls.AddRange(new Control[] { cbPizza, cbPasta, cbPapWors, cbOther });

            y += 50;

            panelSurvey.Controls.Add(CreateLabel("Rate your agreement (1 to 5): 1=Strongly Agree, 2=Agree, 3=Neutral, 4=Disagree, 5=Strongly Disagree  (Note: ONLY ONE SELECTION PER ROW)", 20, y));
            y += 25;

            ratingTable = new TableLayoutPanel { Location = new Point(20, y), Size = new Size(850, 160), ColumnCount = 6, RowCount = 5, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
            ratingTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            for (int i = 0; i < 5; i++) ratingTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            ratingTable.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 0);
            string[] headers = { "1", "2", "3", "4", "5" };
            for (int i = 0; i < 5; i++)
                ratingTable.Controls.Add(new Label { Text = headers[i], AutoSize = true }, i + 1, 0);

            ratingChecks = new CheckBox[statements.Length, 5];
            for (int row = 0; row < statements.Length; row++)
            {
                ratingTable.Controls.Add(new Label { Text = statements[row], AutoSize = true }, 0, row + 1);
                for (int col = 0; col < 5; col++)
                {
                    ratingChecks[row, col] = new CheckBox { ForeColor = Color.Blue };
                    ratingTable.Controls.Add(ratingChecks[row, col], col + 1, row + 1);
                }
            }
            panelSurvey.Controls.Add(ratingTable);
            y += 180;

            btnSubmit = new Button { Text = "SUBMIT", Location = new Point(350, y), Width = 100, Height = 30, BackColor = Color.LightBlue };
            btnSubmit.Click += BtnSubmit_Click!;
            panelSurvey.Controls.Add(btnSubmit);
        }

        private void InitializeResultsPanel()
        {
            dgvResults = new DataGridView { Location = new Point(20, 50), Width = 920, Height = 400, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            panelResults!.Controls.Add(dgvResults);

            btnShowChart = new Button { Text = "View Chart", Location = new Point(420, 470), Width = 150, Height = 30 };
            btnShowChart.Click += BtnShowChart_Click;
            panelResults.Controls.Add(btnShowChart);
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName!.Text) ||
                string.IsNullOrWhiteSpace(txtEmail!.Text) ||
                string.IsNullOrWhiteSpace(txtContact!.Text))
            {
                MessageBox.Show("Please fill in all personal information fields.");
                return;
            }

            int age = DateTime.Now.Year - dtpDOB!.Value.Year;
            if (dtpDOB.Value > DateTime.Now.AddYears(-age)) age--;

            if (age < 5 || age > 120)
            {
                MessageBox.Show("Age must be between 5 and 120.");
                return;
            }

            if (!ValidateRatings())
            {
                MessageBox.Show("Rate each question.");
                return;
            }

            int[] ratings = new int[4];
            for (int i = 0; i < 4; i++)
                ratings[i] = Enumerable.Range(0, 5).Where(j => ratingChecks![i, j].Checked).Sum(j => j + 1);

            var cmd = new SQLiteCommand("INSERT INTO Survey (FullName, Email, DOB, Contact, LikesPizza, LikesPasta, LikesPapWors, LikesOther, RateMovies, RateRadio, RateEatOut, RateTV) VALUES (@name, @mail, @dob, @contact, @pizza, @pasta, @papwors, @other, @m, @r, @e, @tv)", conn);
            cmd.Parameters.AddWithValue("@name", txtName!.Text);
            cmd.Parameters.AddWithValue("@mail", txtEmail!.Text);
            cmd.Parameters.AddWithValue("@dob", dtpDOB!.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@contact", txtContact!.Text);
            cmd.Parameters.AddWithValue("@pizza", cbPizza!.Checked ? 1 : 0);
            cmd.Parameters.AddWithValue("@pasta", cbPasta!.Checked ? 1 : 0);
            cmd.Parameters.AddWithValue("@papwors", cbPapWors!.Checked ? 1 : 0);
            cmd.Parameters.AddWithValue("@other", cbOther!.Checked ? 1 : 0);
            cmd.Parameters.AddWithValue("@m", ratings[0]);
            cmd.Parameters.AddWithValue("@r", ratings[1]);
            cmd.Parameters.AddWithValue("@e", ratings[2]);
            cmd.Parameters.AddWithValue("@tv", ratings[3]);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Survey submitted successfully!");
            ClearSurveyInputs();
        }


        private void BtnShowChart_Click(object? sender, EventArgs e)
        {
            var chartForm = new Form { Width = 800, Height = 500 };
            var chart = new Chart { Dock = DockStyle.Fill };
            chartForm.Controls.Add(chart);

            var chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);
            var series = new Series { ChartType = SeriesChartType.Column };

            var cmd = new SQLiteCommand("SELECT AVG(RateMovies), AVG(RateRadio), AVG(RateEatOut), AVG(RateTV) FROM Survey", conn);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                series.Points.AddXY("Movies", reader.GetDouble(0));
                series.Points.AddXY("Radio", reader.GetDouble(1));
                series.Points.AddXY("Eat Out", reader.GetDouble(2));
                series.Points.AddXY("TV", reader.GetDouble(3));
            }
            chart.Series.Add(series);
            chartForm.Show();
        }

        private void ShowSurveyPanel()
        {
            panelSurvey!.Visible = true;
            panelResults!.Visible = false;
        }

        private Size GetSize()
        {
            return Size;
        }

        private void ShowResultsPanel(Size size)
        {
            panelSurvey!.Visible = false;
            panelResults!.Visible = true;

            var adapter = new SQLiteDataAdapter("SELECT * FROM Survey", conn);
            var dt = new DataTable();
            adapter.Fill(dt);
            dgvResults!.DataSource = dt;

            var cmd = new SQLiteCommand(@"
        SELECT 
            COUNT(*) AS Total,
            AVG((julianday('now') - julianday(DOB))/365.25) AS AvgAge,
            MAX((julianday('now') - julianday(DOB))/365.25) AS Oldest,
            MIN((julianday('now') - julianday(DOB))/365.25) AS Youngest,
            ROUND(AVG(LikesPizza)*100,1) AS PizzaPercent,
            ROUND(AVG(LikesPasta)*100,1) AS PastaPercent,
            ROUND(AVG(LikesPapWors)*100,1) AS PapWorsPercent,
            ROUND(AVG(RateMovies),1) AS AvgMovies,
            ROUND(AVG(RateRadio),1) AS AvgRadio,
            ROUND(AVG(RateEatOut),1) AS AvgEatOut,
            ROUND(AVG(RateTV),1) AS AvgTV
        FROM Survey
        WHERE DOB IS NOT NULL AND DOB != ''
        ", conn);

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int total = Convert.ToInt32(reader["Total"]);
                if (total == 0)
                {
                    lblStats.Text = "No survey data available.";
                    return;
                }

                string SafeGet(object val, string fallback = "N/A") =>
                    val is DBNull ? fallback : Convert.ToString(val);

                string stats = $@"
        Total number of surveys: {total}
        Average Age: {SafeGet(reader["AvgAge"])}
        Oldest person who participated in survey: {SafeGet(reader["Oldest"])}
        Youngest person who participated in survey: {SafeGet(reader["Youngest"])}

        --- Food Preferences ---
        Percentage of people who like pizza: {SafeGet(reader["PizzaPercent"])}%
        Percentage of people who like pasta: {SafeGet(reader["PastaPercent"])}%
        Percentage of people who like pap and wors: {SafeGet(reader["PapWorsPercent"])}%

        --- Activity Ratings ---
        People who like to watch movies - average rating: {SafeGet(reader["AvgMovies"])}
        People who like to listen to radio - average rating: {SafeGet(reader["AvgRadio"])}
        People who like to eat out - average rating: {SafeGet(reader["AvgEatOut"])}
        People who like to watch TV - average rating: {SafeGet(reader["AvgTV"])}
        ";

                if (lblStats == null)
                {
                    lblStats = new TextBox
                    {
                        Location = new Point(20, 510),
                        Size = new Size(920, 300),
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        Font = new Font("Segoe UI", 10),
                        BorderStyle = BorderStyle.None
                    };
                    panelResults!.Controls.Add(lblStats);
                }

                lblStats.Text = stats;
            }
        }

        private bool ValidateRatings()
        {
            for (int row = 0; row < statements.Length; row++)
            {
                int checkedCount = 0;
                for (int col = 0; col < 5; col++)
                {
                    if (ratingChecks![row, col].Checked)
                        checkedCount++;
                }
                if (checkedCount != 1)
                    return false; // invalid if not exactly one checked
            }
            return true;
        }


        private Label CreateLabel(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

        private TextBox CreateTextBox(int x, int y) =>
            new TextBox
            {
                Location = new Point(x, y),
                Width = 200
            };

    }
}