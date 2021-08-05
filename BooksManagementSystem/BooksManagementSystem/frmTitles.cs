using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooksManagementSystem
{
    public partial class frmTitles : Form
    {
        public frmTitles()
        {
            Thread t = new Thread(new ThreadStart(StartForm));
            t.Start();
            Thread.Sleep(5000);
            InitializeComponent();
            t.Abort();
        }

        private void StartForm()
        {
            Application.Run(new welcomeForm());
        }

        OleDbConnection booksConn;
        OleDbCommand titlesComm;
        OleDbDataAdapter titlesAdapter;
        DataTable titlesTable;
        CurrencyManager titlesManager;
        public string AppState { get; set; }
        OleDbCommandBuilder builderComm;
        public int CurrentPosition { get; set; }
        OleDbCommand authorsComm;
        OleDbDataAdapter authorsAdapter;
        DataTable [] authorsTable = new DataTable[4];
        ComboBox[] authorCombo = new ComboBox[4];
        OleDbCommand ISBNAuthorsComm;
        OleDbDataAdapter ISBNAuthorsAdapter;
        DataTable ISBNAuthorsTable;
        OleDbCommand publisherComm;
        OleDbDataAdapter publisherAdaptor;
        DataTable publisherTable;

        private void frmTitles_Load(object sender, EventArgs e)
        {
            try
            {
                var connString = " Provider = Microsoft.ACE.OLEDB.12.0; Data Source = F:\\Books.accdb; Persist Security Info = False";
                booksConn = new OleDbConnection(connString);
                booksConn.Open();
                titlesComm = new OleDbCommand("SELECT * FROM Titles ORDER BY Title", booksConn);
                titlesAdapter = new OleDbDataAdapter();
                titlesAdapter.SelectCommand = titlesComm;
                titlesTable = new DataTable();
                titlesAdapter.Fill(titlesTable);
                txtTitle.DataBindings.Add("Text", titlesTable, "Title");
                txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");
                txtISBN.DataBindings.Add("Text", titlesTable, "ISBN");
                txtDescription.DataBindings.Add("Text", titlesTable, "Description");
                txtSubject.DataBindings.Add("Text", titlesTable, "Subject");
                titlesManager = (CurrencyManager)BindingContext[titlesTable];

                authorCombo[0] = cboAuthor1;
                authorCombo[1] = cboAuthor2;
                authorCombo[2] = cboAuthor3;
                authorCombo[3] = cboAuthor4;
                authorsComm = new OleDbCommand("SELECT * FROM Authors ORDER BY Author", booksConn);
                authorsAdapter = new OleDbDataAdapter();
                authorsAdapter.SelectCommand = authorsComm;

                for (int i =0; i < 4; i++)
                {
                    authorsTable[i] = new DataTable();
                    authorsAdapter.Fill(authorsTable[i]);
                    authorCombo[i].DataSource = authorsTable[i];
                    authorCombo[i].DisplayMember = "Author";
                    authorCombo[i].ValueMember = "Au_ID";
                    authorCombo[i].SelectedIndex = -1;
                }

                publisherComm = new OleDbCommand("Select * from Publishers Order By Name", booksConn);
                publisherAdaptor = new OleDbDataAdapter();
                publisherTable = new DataTable();
                publisherAdaptor.SelectCommand = publisherComm;
                publisherAdaptor.Fill(publisherTable);

                cboPublisher.DataSource = publisherTable;
                cboPublisher.DisplayMember = "Name";
                cboPublisher.ValueMember = "PubID";
                cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");

                SetAppState("View");
                GetAuthors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            booksConn.Close();
            booksConn.Dispose();
            titlesComm.Dispose();
            titlesAdapter.Dispose();
            titlesTable.Dispose();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            titlesManager.Position = 0;
            GetAuthors();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            titlesManager.Position = titlesManager.Count - 1;
            GetAuthors();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            titlesManager.Position--;
            GetAuthors();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            titlesManager.Position++;
            GetAuthors();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Equals("") || txtSearch.Text.Length < 3)
            {
                MessageBox.Show("Invalid Search", "Invalid Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow[] foundRecords;
            titlesTable.DefaultView.Sort = "Title";
            foundRecords = titlesTable.Select("Title LIKE '*" + txtSearch.Text + "*'");

            if (foundRecords.Length == 0)
            {
                MessageBox.Show("No record found", "No record found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                frmSearch searchForm = new frmSearch(foundRecords, "Titles");
                searchForm.ShowDialog();
                var index = searchForm.Index;
                titlesManager.Position = titlesTable.DefaultView.Find(foundRecords[index]["Title"]);
                GetAuthors();
            }
        }

        private void SetAppState(string appState)
        {
            switch (appState)
            {
                case "View":
                    txtTitle.ReadOnly = true;
                    txtYearPublished.ReadOnly = true;
                    txtISBN.ReadOnly = true;
                    txtDescription.ReadOnly = true;
                    txtSubject.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAddNew.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    btnFind.Enabled = true;
                    btnAuthors.Enabled = true;
                    btnPublishers.Enabled = true;
                    cboAuthor1.Enabled = false;
                    cboAuthor2.Enabled = false;
                    cboAuthor3.Enabled = false;
                    cboAuthor4.Enabled = false;
                    btnXAuthor1.Enabled = false;
                    btnXAuthor2.Enabled = false;
                    btnXAuthor3.Enabled = false;
                    btnXAuthor4.Enabled = false;
                    cboPublisher.Enabled = false;
                    break;
                default:
                    txtTitle.ReadOnly = false;
                    txtYearPublished.ReadOnly = false;
                    if (appState == "Add")
                    {
                        txtISBN.ReadOnly = false;
                    } else
                    {
                        txtISBN.ReadOnly = true;
                    }
                    
                    txtDescription.ReadOnly = false;
                    txtSubject.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAddNew.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    btnFind.Enabled = false;
                    btnAuthors.Enabled = false;
                    btnPublishers.Enabled = false;
                    cboAuthor1.Enabled = true;
                    cboAuthor2.Enabled = true;
                    cboAuthor3.Enabled = true;
                    cboAuthor4.Enabled = true;
                    btnXAuthor1.Enabled = true;
                    btnXAuthor2.Enabled = true;
                    btnXAuthor3.Enabled = true;
                    btnXAuthor4.Enabled = true;
                    cboPublisher.Enabled = true;
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            txtYearPublished.DataBindings.Clear();
            SetAppState("Edit");
            AppState = "Edit";
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            cboAuthor1.SelectedIndex = -1;
            cboAuthor2.SelectedIndex = -1;
            cboAuthor3.SelectedIndex = -1;
            cboAuthor4.SelectedIndex = -1;
            CurrentPosition = titlesManager.Position;
            SetAppState("Add");
            titlesManager.AddNew();
            AppState = "Add";
        }

        private bool ValidateIntput()
        {
            string message = "";
            bool isOK = true;

            if (txtTitle.Text.Equals(""))
            {
                message = "You must enter a title.\r\n";
                isOK = false;
            }

            int inputYear, currentYear;
            if (!txtYearPublished.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtYearPublished.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear > currentYear)
                {
                    message += "Year published cannot be greater than current year \r\n";
                    isOK = false;
                }
            }

            if (!(txtISBN.Text.Length == 13))
            {
                message += "Incomplete ISBN\r\n";
                isOK = false;
            }

            //TO DO validate publisher

            if (!isOK)
            {
                MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return isOK;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateIntput())
            {
                return;
            }

            try
            {
                var savedRecord = txtISBN.Text;
                titlesManager.EndCurrentEdit();
                builderComm = new OleDbCommandBuilder(titlesAdapter);  
                
                if (AppState == "Edit")
                {
                    var titleRow = titlesTable.Select("ISBN = '" + savedRecord + "'");

                    if (String.IsNullOrEmpty(txtYearPublished.Text))
                        titleRow[0]["Year_Published"] = DBNull.Value;
                    else
                        titleRow[0]["Year_Published"] = txtYearPublished.Text;

                    titlesAdapter.Update(titlesTable);
                    txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");
                }
                else
                {                    
                    titlesAdapter.Update(titlesTable);
                    DataRow[] foundRecords;
                    titlesTable.DefaultView.Sort = "Title";
                    foundRecords = titlesTable.Select("ISBN = '" + savedRecord + "'");
                    titlesManager.Position = titlesTable.DefaultView.Find(foundRecords[0]["Title"]);
                }

                builderComm = new OleDbCommandBuilder(ISBNAuthorsAdapter);
                if (ISBNAuthorsTable.Rows.Count != 0)
                {
                    for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++)
                    {
                        ISBNAuthorsTable.Rows[i].Delete();
                    }

                    ISBNAuthorsAdapter.Update(ISBNAuthorsTable);
                }

                for(int i = 0; i < 4; i++)
                {
                    if (authorCombo[i].SelectedIndex != -1)
                    {
                        ISBNAuthorsTable.Rows.Add();
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["ISBN"] = txtISBN.Text;
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["Au_ID"] = authorCombo[i].SelectedValue;
                    }
                }

                ISBNAuthorsAdapter.Update(ISBNAuthorsTable);

                MessageBox.Show("Record Saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete record",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (response == DialogResult.No)
                return;

            try
            {
                titlesManager.RemoveAt(titlesManager.Position);
                builderComm = new OleDbCommandBuilder(titlesAdapter);
                titlesAdapter.Update(titlesTable);
                AppState = "Delete";
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Deleting record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            titlesManager.CancelCurrentEdit();

            if (AppState == "Edit")
                txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");

            if (AppState == "Add")
                titlesManager.Position = CurrentPosition;

            SetAppState("View");
        }

        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >='0' && e.KeyChar <= '9') || e.KeyChar == 8)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void GetAuthors()
        {
            for (int i = 0; i < 4; i++)
            {
                authorCombo[i].SelectedIndex = -1;
            }

            ISBNAuthorsComm = new OleDbCommand("Select * FROM Title_Author WHERE ISBN = '" + txtISBN.Text + "'", booksConn);
            ISBNAuthorsAdapter = new OleDbDataAdapter();
            ISBNAuthorsAdapter.SelectCommand = ISBNAuthorsComm;
            ISBNAuthorsTable = new DataTable();
            ISBNAuthorsAdapter.Fill(ISBNAuthorsTable);

            if (ISBNAuthorsTable.Rows.Count == 0)
                return;

            for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++)
            {
                authorCombo[i].SelectedValue = ISBNAuthorsTable.Rows[i]["Au_ID"].ToString();
            }
        }

        private void btnXAuthor_Click(object sender, EventArgs e)
        {
            Button btnClicked = (Button) sender;
            switch (btnClicked.Name)
            {
                case "btnXAuthor1":
                    cboAuthor1.SelectedIndex = -1;
                    break;
                case "btnXAuthor2":
                    cboAuthor2.SelectedIndex = -1;
                    break;
                case "btnXAuthor3":
                    cboAuthor3.SelectedIndex = -1;
                    break;
                case "btnXAuthor4":
                    cboAuthor4.SelectedIndex = -1;
                    break;
            }
        }

        private void btnAuthors_Click(object sender, EventArgs e)
        {
            frmAuthors authorForm = new frmAuthors();
            authorForm.ShowDialog();
            authorForm.Dispose();
            booksConn.Close();

            var connString = " Provider = Microsoft.ACE.OLEDB.12.0; Data Source = F:\\Books.accdb; Persist Security Info = False";
            booksConn = new OleDbConnection(connString);
            booksConn.Open();
            authorsAdapter.SelectCommand = authorsComm;

            for (int i = 0; i < 4; i++)
            {
                authorsTable[i] = new DataTable();
                authorsAdapter.Fill(authorsTable[i]);
                authorCombo[i].DataSource = authorsTable[i];
            }

            GetAuthors();
        }

        private void btnPublishers_Click(object sender, EventArgs e)
        {
            frmPublishers pubForm = new frmPublishers();
            pubForm.ShowDialog();
            pubForm.Dispose();
            var connString = " Provider = Microsoft.ACE.OLEDB.12.0; Data Source = F:\\Books.accdb; Persist Security Info = False";
            booksConn = new OleDbConnection(connString);
            booksConn.Open();
            cboPublisher.DataBindings.Clear();
            publisherAdaptor.SelectCommand = publisherComm;
            publisherTable = new DataTable();
            publisherAdaptor.Fill(publisherTable);
            cboPublisher.DataSource = publisherTable;
            cboPublisher.DisplayMember = "Name";
            cboPublisher.ValueMember = "PubID";
            cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
