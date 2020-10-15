using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TBDExchangeEngine
{
    public partial class ExchangeEngineForm : Form
    {
        string accno;
        string screenNum = "2000";
        List<string> codelist = new List<string>();

        public ExchangeEngineForm()
        {
            InitializeRabbitMQ();
            InitializeComponent();

            axKHOpenAPI1.CommConnect();
            axKHOpenAPI1.OnEventConnect += onEventConnect;
            axKHOpenAPI1.OnReceiveTrData += onReceiveTrData;
            axKHOpenAPI1.OnReceiveRealData += onReceiveRealData;
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "order",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                MessageBox.Show($" [x] Received {message}");
            };
            channel.BasicConsume(queue: "order",
                                 autoAck: true,
                                 consumer: consumer);
        }

        public void GetPortfolioInfo()
        {
            axKHOpenAPI1.SetInputValue("계좌번호", accno);
            axKHOpenAPI1.SetInputValue("비밀번호", "pruna1");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.SetInputValue("조회구분", "1");

            axKHOpenAPI1.CommRqData("계좌평가잔고내역요청", "opw00018", 0, screenNum);
        }

        public void GetCodeList()
        {
            // 0: KOSPI, 10: KOSDAQ
            string[] marketTypes = new string[] { "0", "10" };

            foreach (string type in marketTypes)
            {
                string stockCodeList = axKHOpenAPI1.GetCodeListByMarket(type);
                string[] stockCode = stockCodeList.Split(';');
                codelist.AddRange(stockCode);
            }

            MessageBox.Show($"total count of codelist: {codelist.Count()}");
            StreamWriter sw = new StreamWriter("C:\\Users\\hori9\\Desktop\\datatest.txt", append: true);
            sw.WriteLine($"total count of codelist: {codelist.Count()}");
            sw.Flush();
            sw.Close();
        }

        public void RegisterRealTimeData()
        {
            // 장시작시간 등록
            axKHOpenAPI1.SetRealReg("2001", "", "215", "0");

            int cnt = 0;
            int stockScreenNumber = 3000;
            int firstStock = 0;

            try
            {
                foreach (string code in codelist)
                {
                    // 주식체결 등록
                    axKHOpenAPI1.SetRealReg(stockScreenNumber.ToString(),
                                            code,
                                            "20;41", // 체결시간
                                            firstStock.ToString());

                    cnt += 1;
                    firstStock = 1;

                    if (cnt % 50 == 0)
                    {
                        stockScreenNumber += 1;
                        firstStock = 0;
                    }
                }
            }
            catch
            {
                RegisterRealTimeData();
            }

            MessageBox.Show("registered all stocks");
        }

        public void onEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {
                string accountList = axKHOpenAPI1.GetLoginInfo("ACCLIST");
                string[] account = accountList.Split(';');
                for (int i = 0; i < account.Length; i++)
                {
                    if (i == 0)
                    {
                        // 항상 첫번째 계좌 가져오기 (추후 변경 필요)
                        accno = account[i];
                    }
                }
                string userID = axKHOpenAPI1.GetLoginInfo("USER_ID");
                string userName = axKHOpenAPI1.GetLoginInfo("USER_NAME");
                string connectedServer = axKHOpenAPI1.GetLoginInfo("GetServerGubun");

                userIdLabel.Text = userID;
                userNameLabel.Text = userName;
                serverTypeLabel.Text = connectedServer.ToString();

                GetPortfolioInfo();
                GetCodeList();
                RegisterRealTimeData();
            }
        }

        public void onReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName == "계좌평가잔고내역요청")
            {
                long totalPurchase = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총매입금액"));
                long totalEstimate = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가금액"));
                long totalProfitLoss = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가손익금액"));
                double totalProfitRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총수익률(%)"));

                MessageBox.Show($"{totalPurchase.ToString()}, {totalEstimate.ToString()}, {totalProfitLoss.ToString()}");

                StreamWriter sw = new StreamWriter("C:\\Users\\hori9\\Desktop\\datatest.txt", append: true);
                sw.WriteLine($"{totalPurchase.ToString()}, {totalEstimate.ToString()}, {totalProfitLoss.ToString()}");
                sw.Flush();
                sw.Close();
            }
        }

        public void onReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            if (e.sRealType == "장시작시간")
            {
                GetMarketState(e);
            }

            if (e.sRealType == "주식호가잔량")
            {
                GetHoga(e);
            }

            if (e.sRealType == "주식체결")
            {
                GetTick(e);
            }
        }

        public void GetMarketState(AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            /*
             * 장운영구분: 215
             * 시간: 20
             * 장시작예상잔여시간: 214
             */

            int value = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealType, 215));

            if (value == 0)
            {
                // 장 시작 전
            }
            else if (value == 3)
            {
                // 장 시작
            }
            else if (value == 2)
            {
                // 장 종료, 동시호가
            }
            else if (value == 4)
            {
                // 3시 30분 장 종료
            }
        }

        public void GetHoga(AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string hogaDataString = "hoga;";

            string hogaTime = axKHOpenAPI1.GetCommRealData(e.sRealType, 21);
            hogaDataString += hogaTime + ";";

            for (int i = 0; i < 10; i++)
            {
                string sellHoga = axKHOpenAPI1.GetCommRealData(e.sRealType, 50 - i);
                string sellAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 70 - i);
                hogaDataString += $"{sellHoga};{sellAmt};";
            }

            for (int i = 0; i < 10; i++)
            {
                string buyHoga = axKHOpenAPI1.GetCommRealData(e.sRealType, 51 + i);
                string buyAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 71 + i);
                hogaDataString += $"{buyHoga};{buyAmt};";
            }

            string buyTotal = axKHOpenAPI1.GetCommRealData(e.sRealType, 128);
            string sellTotal = axKHOpenAPI1.GetCommRealData(e.sRealType, 138);
            hogaDataString += $"{buyTotal};{sellTotal};";

            StreamWriter sw = new StreamWriter("C:\\Users\\hori9\\Desktop\\hogatest.txt", append: true);
            sw.WriteLine(hogaDataString);
            sw.Flush();
            sw.Close();
        }

        public void GetTick(AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            /*
             * 체결시간: 20
             * 현재가: 10
             * (최우선)매도호가: 27
             * (최우선) 매수호가: 28
             * 거래량: 15
             * 누적거래량: 13
             * 거래회전율: 31
             * 거래비용: 32
             */
            string tickDataString = "tick;";

            string tradedTime = axKHOpenAPI1.GetCommRealData(e.sRealType, 20);
            string tradedPrice = axKHOpenAPI1.GetCommRealData(e.sRealType, 10);
            string sellHoga = axKHOpenAPI1.GetCommRealData(e.sRealType, 27);
            string buyHoga = axKHOpenAPI1.GetCommRealData(e.sRealType, 28);
            string tradedVolume = axKHOpenAPI1.GetCommRealData(e.sRealType, 15);
            string totalVolume = axKHOpenAPI1.GetCommRealData(e.sRealType, 13);
            string tradingRate = axKHOpenAPI1.GetCommRealData(e.sRealType, 31);
            string cost = axKHOpenAPI1.GetCommRealData(e.sRealType, 31);
            tickDataString += $"{tradedTime};{tradedPrice};{sellHoga};{buyHoga};{tradedVolume};{totalVolume};{tradingRate};{cost};";

            StreamWriter sw = new StreamWriter("C:\\Users\\hori9\\Desktop\\ticktest.txt", append: true);
            sw.WriteLine(tickDataString);
            sw.Flush();
            sw.Close();
        }
    }
}
