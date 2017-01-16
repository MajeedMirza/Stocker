using System;
using System.Drawing;
using System.Windows.Forms;

namespace Stocker
{
    public partial class Stocker : Form
    {
        string webData;
        string pList;
        public Stocker()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //pList = "\"\"";
            //Properties.Settings.Default.StockList = pList;
            //Properties.Settings.Default.Save();
            this.Size = Properties.Settings.Default.Size;
            pList = Properties.Settings.Default.StockList.ToUpper();
            RefreshGrid();
        }
        
        private void RefreshGrid()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            string webStr = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20(" + pList + ")%0A%09%09&diagnostics=true&env=http%3A%2F%2Fdatatables.org%2Falltables.env";
            webData = wc.DownloadString(webStr);
            if (webData.Contains("Invalid environment specified") || webData.Contains("<LastTradePriceOnly/>")) //|| webData.Contains("<AskRealtime/>")
            {
                MessageBox.Show("Please enter a valid ticker symbol.");
                pList = Properties.Settings.Default.StockList.ToUpper();
                txtSearch.Text = "";
                return;
            }
            dgv1.Rows.Clear();
            Properties.Settings.Default.StockList = pList;
            Properties.Settings.Default.Save();
            string edit = webData;
            int rownum = 0;
            while (edit.Contains("<quote symbol=")) {
                edit = edit.Remove(0, edit.IndexOf("<quote symbol=") + 15);
                string symbol = "";
                foreach (char c in edit)
                {
                    if(c != '"')
                    {
                        symbol += c;
                    }
                    else
                    {
                        break;
                    }
                }
                edit = edit.Remove(0, edit.IndexOf("<Change>") + 8);
                string change = "";
                foreach (char c in edit)
                {
                    if (c != '<')
                    {
                        change += c;
                    }
                    else
                    {
                        break;
                    }
                }
                edit = edit.Remove(0, edit.IndexOf("<LastTradePriceOnly>") + 20);
                string Current = "";
                foreach (char c in edit)
                {
                    if (c != '<')
                    {
                        Current += c;
                    }
                    else
                    {
                        break;
                    }
                }
                edit = edit.Remove(0, edit.IndexOf("<DaysRange>") + 11);
                string range = "";
                foreach (char c in edit)
                {
                    if (c != '<')
                    {
                        range += c;
                    }
                    else
                    {
                        break;
                    }
                }

                edit = edit.Remove(0, edit.IndexOf("<ChangeinPercent>") + 17);
                string perChange = "";
                foreach (char c in edit)
                {
                    if (c != '<')
                    {
                        perChange += c;
                    }
                    else
                    {
                        break;
                    }
                }

                string[] row1 = new string[] { symbol, Current, range, change, perChange};
                dgv1.Rows.Add(row1);
                if (change.Contains("-"))
                {
                    dgv1.Rows[rownum].Cells[3].Style.BackColor = Color.Salmon;
                }
                else
                {
                    dgv1.Rows[rownum].Cells[3].Style.BackColor = Color.LightGreen;
                }
                if (perChange.Contains("-"))
                {
                    dgv1.Rows[rownum].Cells[4].Style.BackColor = Color.Salmon;
                }
                else
                {
                    dgv1.Rows[rownum].Cells[4].Style.BackColor = Color.LightGreen;
                }
                rownum++;
                dgv1.CurrentCell = null;
                txtSearch.Text = "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgv1.Rows)
            {
               if(row.Cells[0].Value.ToString() == txtSearch.Text)
                {
                    //"YHOO" % 2C"AAPL" % 2C"GOOG" % 2C"MSFT" % 2C"NFLX" % 2C"TTWO" % 2C"TSLA" % 2C"CMG" % 2C"P" % 2C"REGN" % 2C"FB"""%2C
                    pList = pList.Replace("%2C\"" + txtSearch.Text + "\"", "");
                    //pList = pList.Replace(txtSearch.Text, "");
                    Properties.Settings.Default.StockList = pList;
                    Properties.Settings.Default.Save();
                    dgv1.Rows.Remove(row);
                    txtSearch.Text = "";
                    return;
                }
            }
            if (txtSearch.Text == "")
            {
                MessageBox.Show("Please enter a ticker symbol.");
                return;
            }
            if (!(pList.Contains("%2C\"" + txtSearch.Text + "\"")))
            {
                pList += "%2C\"" + txtSearch.Text + "\"";
            }
            RefreshGrid();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnSearch.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void dgv1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string myValue = dgv1[0, e.RowIndex].Value.ToString();
            System.Diagnostics.Process.Start("https://ca.finance.yahoo.com/q?s=" + myValue);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void Stocker_ResizeEnd(object sender, EventArgs e)
        {
            Properties.Settings.Default.Size = this.Size;
            Properties.Settings.Default.Save();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            pList = "\"\"";
            Properties.Settings.Default.StockList = pList;
            Properties.Settings.Default.Save();
            dgv1.Rows.Clear();
        }
    }
}
