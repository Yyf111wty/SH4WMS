using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TR.ORM.Model;
using TR.Service;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;



namespace BarScannerIn
{
    public partial class fm : Form
    { // 控件声明
        private readonly TextBox _txtBarcode = new TextBox();
        private readonly Button _btnBind = new Button();
        private readonly Label _lblStatus = new Label();
        private readonly Label _smyStatus = new Label();
        private readonly Panel _headerPanel = new Panel();
        private readonly PictureBox _scannerIcon = new PictureBox();
        wms_base_locationService service = new wms_base_locationService();
        wms_bill_putinbindingheaderService servicespec = new wms_bill_putinbindingheaderService();
        wms_bill_putinbindingbodyService servicespecbody = new wms_bill_putinbindingbodyService();
        // 替换 CallContext 的典型用法
        private static readonly AsyncLocal<string> _asyncContext = new AsyncLocal<string>();

        public static void SetContext(string value) => _asyncContext.Value = value;
        public static string GetContext() => _asyncContext.Value;
        public fm()
        {
            InitializeUIAsync();
            this.Text = "产品条码绑定系统";
            this.Size = new Size(800, 600); // 窗口大小为800×600
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; // 禁止最大化
        }
        // 在窗体关闭时取消订阅

        private void InitializeUIAsync()
        {

            // 创建主容器面板，设置边距和内边距
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 6,
                Padding = new Padding(50, 10, 50, 50), // 增加上下边距，使内容垂直居中
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
             
            };

            // 列样式：标签列固定宽度，输入框列占满剩余空间
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // 加宽标签列
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            //mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            // 行样式（新增空白行）
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // 提示行
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // 条码行
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // 规格行
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // 空白行（间距）
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));   // 按钮行
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // 提示行


            this.Controls.Add(mainPanel);

            // 条码标签
            var lblBarcode = new Label
            {
                Dock = DockStyle.Fill,
                Text = "条码:",
                Font = new Font("微软雅黑", 10, FontStyle.Bold), // 增大字体
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            // 条码输入框（不可编辑）
            var txtBarcode = new TextBox
            {
                Dock = DockStyle.Fill,
                //ReadOnly = true,
                Font = new Font("Consolas", 12), // 增大字体
                BackColor = Color.WhiteSmoke,
                Margin = new Padding( 0), // 左右留5px边距，视觉更友好
                //Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom // 四向锚定
            };
            // 新增：连续扫描复选框（第4列）
            var chkContinuous = new CheckBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 12),
                Text ="连续扫描",
                TextAlign = ContentAlignment.MiddleLeft,
                //Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, // 四向锚定
                AutoSize = true
            };
            //// 新增：连续扫描标签（第3列）
            //var lblContinuous = new Label
            //{
            //    Dock = DockStyle.Fill,
            //    Text = "连续扫描",
            //    Font = new Font("微软雅黑", 10, FontStyle.Bold),
            //    AutoSize = true,
            //    TextAlign = ContentAlignment.MiddleRight
            //};
            // 新增：0-6下拉框（第4列）
            var cboSpecValue = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10),
                //Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom ,// 四向锚定
                DropDownStyle = ComboBoxStyle.DropDown
                //DropDownStyle = ComboBoxStyle.DropDownList // 仅可选择，不可输入
            };
            // 添加0-6选项
            for (int i = 0; i <= 6; i++)
            {
                cboSpecValue.Items.Add(i.ToString());
            }
            cboSpecValue.SelectedIndex = 0; // 默认选中0
            // 规格标签
            var lblSpec = new Label
            {
                Dock = DockStyle.Fill,
                Text = "规格:",
                Font = new Font("微软雅黑", 10, FontStyle.Bold), // 增大字体
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            // 规格输入框
            var txtSpec = new TextBox
            {
                Dock = DockStyle.Fill,
                TabIndex = 0,
                Font = new Font("微软雅黑", 12), // 增大字体
                Margin= new Padding(5, 0, 5, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };

            // 绑定按钮
            var btnBind = new Button
            {
                Text = "绑定条码",
                Font = new Font("微软雅黑", 14, FontStyle.Bold), // 增大字体                
                Size = new Size(200, 70), // 增大按钮尺寸
                //AutoSize = true,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatAppearance = { BorderSize = 0 } // 无边框
            };
            //_smyStatus
            // 扫描仪状态提示标签
            var smyblStatus = new Label
            {
                Font = new Font("微软雅黑", 12), // 增大字体
                ForeColor = Color.DimGray,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            // 状态提示标签
            var lblStatus = new Label
            {
                Font = new Font("微软雅黑", 12), // 增大字体
                ForeColor = Color.DimGray,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            // 提示标签（跨列）
            smyblStatus.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.SetColumnSpan(smyblStatus, 3);
            mainPanel.Controls.Add(smyblStatus, 0, 0);
            // 添加控件到布局
            lblBarcode.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.Controls.Add(lblBarcode, 0, 1);
            //txtBarcode.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.Controls.Add(txtBarcode, 1, 1);
            chkContinuous.Anchor = AnchorStyles.Top; // 完全居中
            mainPanel.Controls.Add(chkContinuous, 2, 1);    // 第3列：连续扫描复选框

            lblSpec.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.Controls.Add(lblSpec, 0, 2);
            //txtSpec.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.Controls.Add(txtSpec, 1, 2);
            //cboSpecValue.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.Controls.Add(cboSpecValue, 2, 2);
            // 按钮容器（跨列居中）
            var btnContainer = new Panel
            {

                Dock = DockStyle.Fill
            };
            btnContainer.Controls.Add(btnBind);
            btnBind.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.SetColumnSpan(btnContainer, 3);
            mainPanel.Controls.Add(btnContainer, 0, 4);

            // 提示标签（跨列）
            lblStatus.Anchor = AnchorStyles.None; // 完全居中
            mainPanel.SetColumnSpan(lblStatus, 3);
            mainPanel.Controls.Add(lblStatus, 0, 5);
            //扫码器逻辑


            // 模拟在其他线程中处理数据
            //BarcodeScannerClient barcodeScannerClient = new BarcodeScannerClient();
            //barcodeScannerClient.Connect("192.168.13.129", 502);
            // 扫码器逻辑
            BarcodeScannerClient barcodeScannerClient = new BarcodeScannerClient();

            // 订阅扫码事件
            barcodeScannerClient.BarcodeScanned += (scannedData) =>
            {
                // 使用Invoke确保在UI线程更新控件
                if (smyblStatus.InvokeRequired)
                {
                    smyblStatus.Invoke(new Action<string>((data) =>
                    {
                        smyblStatus.Text = $"扫描到: {data}";
                        smyblStatus.ForeColor = Color.Blue;

                        string txt = txtBarcode.Text.Trim();
                        // 同时更新条码输入框
                        if (chkContinuous.Checked == true)
                        {
                            if (txt != "" )
                            {

                                if (txt.Split('/').Contains(data))
                                {
                                    txtBarcode.Text = txt;
                                    

                                }
                                else
                                {
                                    if (txt.Length<9)
                                    {
                                        txtBarcode.Text = txt+"/"  + data + "/";
                                       
                                    }
                                    else
                                    {
                                        txtBarcode.Text = txt+ data + "/";
                                    }
                                   
                                }
                            }
                            else
                            {
                                txtBarcode.Text = data;
                            }
                          
                        }
                        else
                        {
                            txtBarcode.Text = data;
                        }
                       
                    }), scannedData);
                }
                else
                {
                    smyblStatus.Text = $"扫描到: {scannedData}";
                    smyblStatus.ForeColor = Color.Blue;
                    txtBarcode.Text = scannedData;
                }
            };

            // 连接扫码器
            barcodeScannerClient.Connect("192.168.1.149", 5000);

            // 更新连接状态
            if (barcodeScannerClient._isConnected)
            {
                smyblStatus.Text = barcodeScannerClient.msg;
                smyblStatus.ForeColor = Color.Green;
            }
            else
            {
                smyblStatus.Text = barcodeScannerClient.msg;
                smyblStatus.ForeColor = Color.Red;
            }
            // WinForms 示例
            //mainPanel.Invoke(new Action(() => { smyblStatus.Text += barcodeScannerClient.BarcodeScanned; }));
            // 按钮点击事件（保持原有逻辑）
            btnBind.Click += (sender, e) =>
            {
                if (string.IsNullOrEmpty(txtBarcode.Text) || string.IsNullOrEmpty(txtSpec.Text))
                {
                    lblStatus.Text = "绑定失败：条码或规格均不能为空!";
                    lblStatus.ForeColor = Color.Red;
                    return;
                }
                else
                {
                    //符合规则则提示绑定成功，否则绑定失败
                    //
                   // string pattern = @"^(0\.(1[5-9][0-9]|[2-9][0-9]{2})|[1-5]\.[0-9]{3}|6\.([0-4][0-9]{2}|500))([A-Z])?[0-6]$";
                    string pattern = @"^(0\.(1[5-9][0-9]|[2-9][0-9]{2})|[1-5]\.[0-9]{3}|6\.([0-4][0-9]{2}|500))([A-Z])?$";
                    bool isValid = Regex.IsMatch(txtSpec.Text.Trim(), pattern);
                    if (isValid)
                    {
                        try
                        {
                            string resultstr = txtSpec.Text.Trim();
                            string mc = cboSpecValue.Text.Trim();
                            string barlist = txtBarcode.Text.Trim();
                            string[] list=null;
                            if (barlist.Substring(barlist.Length - 1, 1) != "/")
                            {
                                list = new string[] { barlist };
                            }
                            else
                            { 

                              list=  barlist.Substring(0,barlist.Length-1).Split('/') ;
                              
                            }
                            foreach (string s in list)
                            {
                                // string[] list = txtBarcode.Text.Trim().Split(',');
                                //判定是否重复绑定
                                var barcode = service.Query(x => x.CONTAINER == s);
                                var spec = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2);
                                if (barcode.Count > 0)
                                {
                                    lblStatus.Text = s + "绑定失败：库中已存在该条码!";
                                    lblStatus.ForeColor = Color.Red;
                                    return;
                                }
                                else
                                {
                                    
                                    if (spec != null && spec.Count > 0)//已绑定则覆盖
                                    {
                                        wms_bill_putinbindingheader oldheadmodel = servicespec.Query(x => x.CONTAINER == s && x.STATE < 2).FirstOrDefault();
                                        wms_bill_putinbindingbody oldbodymodel = servicespecbody.Query(x => x.HEADERID == oldheadmodel.ID).FirstOrDefault();
                                        // wms_bill_putinbindingheader headmodel = new wms_bill_putinbindingheader();
                                        wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();
                                        //headmodel.ID = oldheadmodel.ID;
                                        //headmodel.SOURCEORDER = oldheadmodel.SOURCEORDER;
                                        //headmodel.CONTAINER = oldheadmodel.CONTAINER;
                                        //headmodel.CONTAINERTYPE = oldheadmodel.CONTAINERTYPE;
                                        //headmodel.QUESTFLAG = oldheadmodel.QUESTFLAG;
                                        //headmodel.STATE = oldheadmodel.STATE;
                                        //headmodel.TARGET = oldheadmodel.TARGET;
                                        //headmodel.ENABLED = oldheadmodel.ENABLED;
                                        //headmodel.REMARK = oldheadmodel.REMARK;
                                        //headmodel.CREATOR = oldheadmodel.CREATOR;
                                        //headmodel.CREATETIME = DateTime.Now;
                                        bodymodel.ID = oldbodymodel.ID;
                                        bodymodel.HEADERID = oldbodymodel.HEADERID;
                                        bodymodel.ITEMCODE = resultstr + "*M" + mc;
                                        bodymodel.CREATOR = "手持扫描";
                                        bodymodel.CREATETIME = DateTime.Now;
                                        var results=  servicespecbody.Update(bodymodel);
                                        if (results )
                                        {


                                            lblStatus.Text = resultstr + "*M" + mc + "绑定成功!";
                                            // lblStatus.Text = $"已绑定：{txtBarcode.Text} - {txtSpec.Text}";
                                            lblStatus.ForeColor = Color.Green;
                                            txtBarcode.Text = "";
                                            txtSpec.Text = "";
                                        }
                                        else
                                        {
                                            lblStatus.Text = "绑定失败：导入数据错误!";
                                            lblStatus.ForeColor = Color.Red;
                                            return;

                                        }
                                    }
                                    else
                                    {

                                        //插入数据，提示绑定成功  
                                        string id = Guid.NewGuid().ToString("D");
                                        wms_bill_putinbindingheader headmodel = new wms_bill_putinbindingheader();
                                        wms_bill_putinbindingbody bodymodel = new wms_bill_putinbindingbody();
                                        //         INSERT INTO wms_bill_putinbindingbody(ID, HEADERID, ITEMCODE, LOT, WEIGHT, TASK_NO_SN, CREATOR, CREATETIME)
                                        //VALUES
                                        //(@ID, @HEADERID, @ITEMCODE, @LOT, @WEIGHT, @TASK_NO_SN, 'SAP', sysdate())

                                        headmodel.ID = id;
                                        headmodel.SOURCEORDER = s;
                                        headmodel.CONTAINER = s;
                                        headmodel.CONTAINERTYPE = "0";
                                        headmodel.QUESTFLAG = 0;
                                        headmodel.STATE = 0;
                                        headmodel.TARGET = "101";
                                        headmodel.ENABLED = 1;
                                        headmodel.REMARK = "入库";
                                        headmodel.CREATOR = "手持扫描";
                                        headmodel.CREATETIME = DateTime.Now;
                                        var results = (servicespec.InsertReturnIdentity(headmodel) == 1);

                                        string bodyid = Guid.NewGuid().ToString("D");
                                        bodymodel.ID = bodyid;
                                        bodymodel.HEADERID = id;
                                        bodymodel.ITEMCODE = resultstr + "*M" + mc;
                                        bodymodel.CREATOR = "手持扫描";
                                        bodymodel.CREATETIME = DateTime.Now;
                                        var results1 = (servicespecbody.InsertReturnIdentity(bodymodel) == 1);
                                        if (results & results1)
                                        {


                                            lblStatus.Text = resultstr + "*M" + mc + "绑定成功!";
                                            // lblStatus.Text = $"已绑定：{txtBarcode.Text} - {txtSpec.Text}";
                                            lblStatus.ForeColor = Color.Green;
                                            txtBarcode.Text = "";
                                            txtSpec.Text = "";
                                        }
                                        else
                                        {
                                            lblStatus.Text = "绑定失败：导入数据错误!";
                                            lblStatus.ForeColor = Color.Red;
                                            return;

                                        }
                                    }


                                }


                            }
                        }
                        catch (Exception ee)
                        {
                            lblStatus.Text = "绑定失败：" + ee.Message;
                            lblStatus.ForeColor = Color.Red;
                            return;
                        }
                    }
                    else
                    {
                        lblStatus.Text = "绑定失败：规格格式错误!";
                        lblStatus.ForeColor = Color.Red;
                        return;
                    }

                }


            };

          
        }
    }
}