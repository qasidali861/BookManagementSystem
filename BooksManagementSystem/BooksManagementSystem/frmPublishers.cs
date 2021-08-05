﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooksManagementSystem
{
    public partial class frmPublishers : Form
    {
        public frmPublishers()
        {
            InitializeComponent();
        }

        OleDbConnection pubConn;
        OleDbCommand pubCommand;
        OleDbDataAdapter pubAdapter;
        DataTable pubTable;
        CurrencyManager pubManager;
        bool connOK = true;
        public string AppState { get; set; }
        public int CurrentPosition { get; set; }

        private void frmPublishers_Load(object sender, EventArgs e)
        {
            try
            {
                var connString = " Provider = Microsoft.ACE.OLEDB.12.0; Data Source = F:\\Books.accdb; Persist Security Info = False";
                pubConn = new OleDbConnection(connString);
                pubConn.Open();
                pubCommand = new OleDbCommand("SELECT * from Publishers ORDER BY Name", pubConn);
                pubTable = new DataTable();
                pubAdapter = new OleDbDataAdapter();
                pubAdapter.SelectCommand = pubCommand;
                pubAdapter.Fill(pubTable);

                txtPubId.DataBindings.Add("Text", pubTable, "PubID");
                txtName.DataBindings.Add("Text", pubTable, "Name");
                txtCompanyName.DataBindings.Add("Text", pubTable, "Company_Name");
                txtAddress.DataBindings.Add("Text", pubTable, "Address");
                txtCity.DataBindings.Add("Text", pubTable, "City");
                txtState.DataBindings.Add("Text", pubTable, "State");
                txtTelephone.DataBindings.Add("Text", pubTable, "Mobile_Number");

                pubManager = (CurrencyManager)BindingContext[pubTable];
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connOK = false;
            }            
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            pubManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            pubManager.Position++;
        }

        private void SetAppState(string appState)
        {
            switch (appState)
            {
                case "View":
                    txtName.ReadOnly = true;
                    txtCompanyName.ReadOnly = true;
                    txtAddress.ReadOnly = true;
                    txtCity.ReadOnly = true;
                    txtState.ReadOnly = true;
                    txtTelephone.ReadOnly = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnEdit.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    btnSearch.Enabled = true;
                    txtSearch.Enabled = true;
                    break;
                default:
                    txtName.ReadOnly = false;
                    txtCompanyName.ReadOnly = false;
                    txtAddress.ReadOnly = false;
                    txtCity.ReadOnly = false;
                    txtState.ReadOnly = false;
                    txtTelephone.ReadOnly = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnEdit.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    btnSearch.Enabled = false;
                    txtSearch.Enabled = false;
                    txtName.Focus();
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                SetAppState("Edit");
                AppState = "Edit";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Editing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentPosition = pubManager.Position;
                pubManager.AddNew();
                SetAppState("Add");
                AppState = "Add";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Adding Record Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            pubManager.CancelCurrentEdit();
            pubManager.Position = CurrentPosition;
            SetAppState("View");
        }

        private bool ValidInput()
        {
            bool inputOK = true;

            if (txtName.Text.Trim().Equals(""))
            {
                MessageBox.Show("Publisher Name is required", "Invalid Input", MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);

                txtName.Focus();
                inputOK = false;
            }

            return inputOK;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidInput())
            {
                return;
            }

            try
            {
                var savedRecord = txtName.Text;
                pubManager.EndCurrentEdit();
                var builderComm = new OleDbCommandBuilder(pubAdapter);                
                pubTable.DefaultView.Sort = "Name";
                pubManager.Position = pubTable.DefaultView.Find(savedRecord);
                pubAdapter.Update(pubTable);
                MessageBox.Show("Record saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (response == DialogResult.No)
            {
                return;
            }

            try
            {
                pubManager.RemoveAt(pubManager.Position);
                var builderComm = new OleDbCommandBuilder(pubAdapter);
                pubAdapter.Update(pubTable);
                AppState = "Delete";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Deleting Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox) sender;
            if (e.KeyChar == 13)
            {
                switch(textBox.Name)
                {
                    case "txtName":
                        txtCompanyName.Focus();
                        break;
                    case "txtCompanyName":
                        txtAddress.Focus();
                        break;
                    case "txtAddress":
                        txtCity.Focus();
                        break;
                    case "txtCity":
                        txtState.Focus();
                        break;
                    case "txtState":
                        txtTelephone.Focus();
                        break;
                    case "txtZip":
                        txtTelephone.Focus();
                        break;
                    case "txtTelephone":
                        btnSave.Focus();
                        break;
                }
            }
        }

        private void frmPublishers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connOK)
            {
                pubConn.Close();
                pubConn.Dispose();
                pubCommand.Dispose();
                pubAdapter.Dispose();
                pubTable.Dispose();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            pubManager.Position = 0;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            pubManager.Position = pubManager.Count - 1;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Equals("") || txtSearch.Text.Length < 3)
            {
                MessageBox.Show("Invalid Search", "Invalid Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow[] foundRecords;
            pubTable.DefaultView.Sort = "Name";
            foundRecords = pubTable.Select("Name LIKE '*" + txtSearch.Text + "*'");

            if (foundRecords.Length == 0)
            {
                MessageBox.Show("No record found", "No record found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                frmSearch searchForm = new frmSearch(foundRecords, "Publishers");
                searchForm.ShowDialog();
                var index = searchForm.Index;
                pubManager.Position = pubTable.DefaultView.Find(foundRecords[index]["Name"]);
            }
        }
    }
}
