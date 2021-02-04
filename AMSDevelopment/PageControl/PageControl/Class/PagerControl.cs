using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using MySql.Data.MySqlClient;
using tq = TarsierEyes.MySQL;
using tqa = TarsierEyes.Common.Synchronization;
using MetroFramework.Controls;
using System.Threading;

namespace PageControl.Pager
{    
    public partial class PagerControl :MetroUserControl
    {

        public class PaginationModule
        {
            public string ModuleName { get; set; }

            public string Limit { get; set; }

            public bool PageLocked { get; set; }
        }


        Label _label = null;

        private enum ListItemEnum
        {
            L100 = 0,
            L200 = 1,
            L300 = 2,
            L400 = 3,
            L500 = 4,
            L1000 = 5,
            L10000 = 6,
            L50000 = 7,
            L100000 = 8,
            LALL = 9
        }

        string _connectionstring = "";
        string _currentModule = "";
        string _commandtext = "";
       
        long _page = 0;
        long _total = 0;
        string _search = "''";
        bool _disabledDisplayAll=false;
        string _cbolimit = "100";
    
        C1FlexGrid  grid = new C1FlexGrid();

        public C1FlexGrid Grid
        {
            get { return grid; }
            set { grid = value; }
        }

        public List<PaginationModule> ListofModule = new List<PaginationModule>();

        public long Page { get { return _page; } set { _page = value; } }
        public string Connection { get { return _connectionstring; } set { _connectionstring = value; } }
        public string CommandText { get { return _commandtext; } set { _commandtext = value; } }
        public string Search { get { return _search; } set { _search = value; } }
        public Label Status { get { return _label; } set { _label = value; } }
        public string Limit { get { return _cbolimit; } set { _cbolimit = value; cboLimit.Text =  _cbolimit; } }
        public bool DisabledDisplayAll { get { return _disabledDisplayAll; } set { _disabledDisplayAll = value; } }

        public string CurrentModule { get { return _currentModule; } set { _currentModule = value; } }

        public delegate void AfterDataLoad(object sender, EventArgs e);
        public event AfterDataLoad AfterDataLoaded;

        public delegate void AfterNextClick(object sender, EventArgs e);
        public event AfterNextClick AfterNextClicked;

        public delegate void AfterFirstClick(object sender, EventArgs e);
        public event AfterFirstClick AfterFirstClicked;

        public delegate void AfterPrevClick(object sender, EventArgs e);
        public event AfterPrevClick AfterPrevClicked;

        public delegate void AfterLastClick(object sender, EventArgs e);
        public event AfterLastClick AfterLastClicked;

        public delegate void AfterLimitSelect(object sender, EventArgs e);
        public event AfterLimitSelect AfterLimitSelected;

        DataTable _tb = new DataTable();

        public string ConnectionString = "";

        public PagerControl()
        {
            InitializeComponent();
            cboLimit.SelectionChangeCommitted += CboLimit_SelectionChangeCommitted;
            ConnectionString = _connectionstring;           
        }

        private void CboLimit_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cboLimit.Enabled == false) return;
            _page = 0;

            switch ((ListItemEnum)Enum.Parse(typeof(ListItemEnum),cboLimit.SelectedIndex.ToString()))
            {
                case ListItemEnum.L100:
                    cboLimit.Text = "100";
                    break;
                case ListItemEnum.L200:
                    cboLimit.Text = "200";
                    break;
                case ListItemEnum.L300:
                    cboLimit.Text = "300";
                    break;
                case ListItemEnum.L400:
                    cboLimit.Text = "400";
                    break;
                case ListItemEnum.L500:
                    cboLimit.Text = "500";
                    break;
                case ListItemEnum.L1000:
                    cboLimit.Text = "1000";
                    break;
                case ListItemEnum.L10000:
                    cboLimit.Text = "10000";
                    break;
                case ListItemEnum.L50000:
                    cboLimit.Text = "50000";
                    break;
                case ListItemEnum.L100000:
                    cboLimit.Text = "100000";
                    break;
                case ListItemEnum.LALL:
                    cboLimit.Text = "ALL";
                    break;
                default:
                    break;
            }
            try
            {
                PaginationModule _pm = this.ListofModule.Where(c => c.ModuleName == _currentModule).SingleOrDefault();
                if (_pm != null)
                {
                    if (_pm.PageLocked == true)
                    {
                        chkLocked_CheckedChanged(null, null);
                    }
                }
            }
            catch (Exception) { }
           
            if (AfterLimitSelected != null) AfterLimitSelected(this, new EventArgs());
        }

        private void PagerControl_Load(object sender, EventArgs e)
        {
            try
            {
                cboLimit.SelectedIndex = 0;
            }
            catch (Exception) { }
            
        }
        public void Initialize()
        {

            GetData();            
        }
        public void GetData()
        {
            if (this.DisabledDisplayAll == true)
            {
                cboLimit.Items.Remove("ALL");
            }
            else if (cboLimit.Items.Contains("ALL") == false)
            {
                cboLimit.Items.Add("ALL");
            }

            this.Invoke((MethodInvoker)delegate
            {
                LoadData();
            });

        }

        public void LoadData()
        {
            if(this.DisabledDisplayAll == true)
            {
                cboLimit.Items.Remove("ALL");
            }
            else if (cboLimit.Items.Contains("ALL") == false)
            {
                cboLimit.Items.Add("ALL");
            }

            if (this.ListofModule != null)
            {
                try
                {
                    PaginationModule _pm = this.ListofModule.Where(c => c.ModuleName == _currentModule).SingleOrDefault();
                    if (_pm != null)
                    {
                        if (_pm.PageLocked == true)
                        {
                            cboLimit.Enabled = false;
                            chkLocked.Checked = true;
                            cboLimit.Text = _pm.Limit;
                            chkLocked.Enabled = false;
                            chkLocked.Checked = true;
                            chkLocked.Enabled = true;
                            cboLimit.Enabled = true;
                        }
                        else if (_pm.PageLocked == false)
                        {
                            chkLocked.Enabled = false;
                            chkLocked.Checked = false;
                            chkLocked.Enabled = true;
                        }
                    }
                }
                catch (Exception)
                {
                    chkLocked.Enabled = false;
                    chkLocked.Checked = false;
                    chkLocked.Enabled = true;
                }
            }
            else
            {
                chkLocked.Enabled = false;
                chkLocked.Checked = false;
                chkLocked.Enabled = true;
            }

            string _display = "Page {0} of {1}";
            string _status = "Showing {0} to {1} of {2} entries";
            string _cursearch = "";

            if (_search == "" || _search == "''")
            {
                _cursearch = "''";
            }
            else
            {
                _cursearch = "'%" + _search + "%'";
            }

            long _limit = 0;
            if (cboLimit.Text != "ALL") _limit = Convert.ToInt64(cboLimit.Text);
            string _query = string.Format(_commandtext, (_cursearch == "''" ? _page : 0), _limit, _cursearch);                       

            tqa.WaitToFinish(loadToDataTableAsync(_query));
            tqa.WaitToFinish(loadToGridAsync(_tb));

            if (cboLimit.Text != "ALL")
            {
                lblRecord.Text = "record per page";
                flpPage.Visible = true;

                if (_tb.Columns.Contains("Total") && _tb.Rows.Count > 0)
                {
                    _total = _tb.Rows[0].Field<long>("Total");
                }


                long _recordfrom = (_page * _limit) + 1;
                long _recordto = (_recordfrom - 1) + _limit;

                if (_limit == 0) _limit = 100;
                decimal _temp = Convert.ToDecimal(_total) / Convert.ToDecimal(_limit);
                lblDisplay.Text = string.Format(_display, _page + 1, Math.Ceiling(_temp));

                if (_page > Math.Ceiling(_temp))
                {
                    _page = _total; GetData();
                    return;
                }

                if (Math.Ceiling(_temp) == 1 || _page == Convert.ToInt32(Math.Ceiling(_temp)) - 1)
                {
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    _recordto = _total;
                }
                else
                {
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                }

                if (_page == 0)
                {
                    btnFirst.Enabled = false;
                    btnPrev.Enabled = false;
                }
                else
                {
                    btnFirst.Enabled = true;
                    btnPrev.Enabled = true;
                }


                if (_label != null)
                {
                    _label.Text = _total == 0 ? "No record to display" : String.Format(_status, _recordfrom, _recordto, _total);
                    _label.Width = 170;
                }

                this.Visible = (_total > 100);
            }
            else
            {
                if (_tb.Columns.Contains("Total") && _tb.Rows.Count > 0)
                {
                    _total = _tb.Rows[0].Field<long>("Total");
                }
                this.Visible = (_total > 100);

                lblRecord.Text = "record";
                flpPage.Visible = false;
               
            }

          //foreach (Column _col in grid.Cols)
          //  {
          //      if (_col.DataType !=null)
          //      {
          //          if (_col.DataType.Name == typeof(System.DateTime).Name)
          //          {
          //              _col.Format = "yyyy-MM-dd";
          //              _col.TextAlign = TextAlignEnum.CenterCenter;
          //          }
          //      }
          //  }


            if (grid.Cols.Contains("..."))
            {
                grid.Cols["..."].AllowResizing = false;
                grid.Cols["..."].AllowSorting = false;
                grid.Cols["..."].Width = 45;
                grid.Cols["..."].TextAlignFixed = TextAlignEnum.CenterCenter;
            }

            if (grid.Cols.Contains("Total"))
            {
                grid.Cols["Total"].Visible = false;
            }

            grid.Cols[0].Visible = false;
            grid.AllowFreezing = AllowFreezingEnum.None;
            grid.Cols.Frozen = 2;
            grid.AllowEditing = false;
            grid.AllowDelete = false;
            //grid.AutoSizeCols(2, grid.Cols.Count - 1, 0);

            if (AfterDataLoaded != null) AfterDataLoaded(this, new EventArgs());
        }

        private IAsyncResult loadToDataTableAsync(String qry)
        {
            Action<String> delLoad = new Action<String>(loadToDataTable);
            IAsyncResult ar = delLoad.BeginInvoke(qry, null, delLoad);
            return ar;
        }

        private void loadToDataTable(string qry)
        {
            _tb = new DataTable();
            try
            {
                MySqlConnection _con = new MySqlConnection(ConnectionString);
                MySqlCommand _com = new MySqlCommand(qry, _con);
                MySqlDataAdapter _adp = new MySqlDataAdapter(_com);
                _com.CommandTimeout = 500;
                _adp.Fill(_tb);

                _con.Dispose(); _con = null;
                _com.Dispose(); _com = null;
                _adp.Dispose(); _adp = null;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Load to FlexGrid
        /// </summary>
        /// <param name="dSource"></param>
        /// <returns></returns>
        private IAsyncResult loadToGridAsync(DataTable dSource)
        {
            Action<DataTable> delLoad = new Action<DataTable>(loadToGrid);
            IAsyncResult ar = delLoad.BeginInvoke(dSource, null, delLoad);
            return ar;
        }

        private void loadToGrid(DataTable dSource)
        {
            if (grid != null)
            {
                try
                {
                    grid.DataSource = dSource;
                }
                catch (Exception) { }
            }
        }


        private void btnNext_Click(object sender, EventArgs e)
        {
            long _limit = Convert.ToInt64(cboLimit.Text);
            decimal _temp = Convert.ToDecimal(_total) / Convert.ToDecimal(_limit);
            _page += 1;
            if (Convert.ToInt32(Math.Ceiling(_temp)) - 1 < _page)
            {
                _page = Convert.ToInt32(Math.Ceiling(_temp)) - 1;
            }

            if (AfterNextClicked != null) AfterNextClicked(this, new EventArgs());
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            long _limit = Convert.ToInt64(cboLimit.Text);
            decimal _temp = Convert.ToDecimal(_total) / Convert.ToDecimal(_limit);
            _page = Convert.ToInt32(Math.Ceiling(_temp)) - 1;

            if (AfterLastClicked != null) AfterLastClicked(this, new EventArgs());
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            _page = 0;

            if (AfterFirstClicked != null) AfterFirstClicked(this, new EventArgs());
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _page -= 1;
            if (_page < 0) _page = 0;

            if (AfterPrevClicked != null) AfterPrevClicked(this, new EventArgs());
        }

        private void chkLocked_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLocked.Enabled == false) return;
            if (chkLocked.Checked == true)
            {
                try
                {
                    PaginationModule _pm = this.ListofModule.Where(c => c.ModuleName == _currentModule).SingleOrDefault();
                    if (_pm != null)
                    {
                        ListofModule.Remove(_pm);
                    }
                }
                catch (Exception) { }
                try
                {                   
                    PaginationModule _new = new PaginationModule();
                    _new.ModuleName = _currentModule;
                    _new.PageLocked = true;
                    _new.Limit = cboLimit.Text;
                    ListofModule.Add(_new);                   
                }
                catch (Exception) { }              
            }
            else
            {                
                try
                {
                    PaginationModule _pm = this.ListofModule.Where(c => c.ModuleName == _currentModule).SingleOrDefault();
                    if (_pm != null)
                    {
                        ListofModule.Remove(_pm);
                    }    
                }
                catch (Exception) { }
                cboLimit.Enabled = false;
                cboLimit.Text = "100";
                cboLimit.Enabled = true;
            }
        }
    }
}
