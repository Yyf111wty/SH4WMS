using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BarScanner
{
    public class BarcodeScannerClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _ipAddress;
        private int _port;
        public bool _isConnected;
        public string msg;
        private string _recdata; // 改为私有字段
        public event Action<string> BarcodeScanned; // 恢复扫码事件


        // 添加线程安全的属性访问器
        public string RecData
        {
            get { return _recdata; }
            private set { _recdata = value; }
        }

        public void Connect(string _ipAddress, int _port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(_ipAddress, _port);
                _stream = _client.GetStream();
                _isConnected = true;
                //Console.WriteLine("扫描仪连接成功.");
                msg = "扫描仪连接成功.";
                // 启动数据接收线程
                var receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                _isConnected = false;
                //Console.WriteLine("扫描仪连接成功.");
                msg = "连接失败:"+ ex.Message;
               // Console.WriteLine($"连接失败: {ex.Message}");
            }
        }
        public void Start()
        {
         
        }
        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            while (_isConnected)
            {
                try
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        RecData = data.Trim();

                        // 触发扫码事件
                        BarcodeScanned?.Invoke(RecData);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Receive error: {ex.Message}");
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            _stream?.Close();
            _client?.Close();
            msg = "连接断开";
            // MessageBox.Show("连接断开");
        }

        public void AutoReconnect(int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries && !_isConnected)
            {
                try
                {
                    Connect(_ipAddress, _port);
                    break;
                }
                catch
                {
                    retryCount++;
                    Thread.Sleep(2000); // 等待2秒重试
                }
            }
        }


    }
}
