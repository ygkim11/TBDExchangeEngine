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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Confluent.Kafka;

namespace TBDExchangeEngine
{
    public partial class ExchangeEngineForm : Form
    {
        IProducer<Null, string> Kafka;
        IModel Rabbit;

        bool registered = false;
        string accno;
        string serverType;
        string screenNum = "2000";
        List<string> codelist = new List<string>();

        public ExchangeEngineForm()
        {
            InitializeRabbitMQ();
            InitializeKafka();
            InitializeComponent();

            axKHOpenAPI1.CommConnect();
            axKHOpenAPI1.OnEventConnect += onEventConnect;
            axKHOpenAPI1.OnReceiveTrData += onReceiveTrData;
            axKHOpenAPI1.OnReceiveRealData += onReceiveRealData;
        }

        private void InitializeRabbitMQ()
        {
            /*
             * Docker로 RabbitMQ 인스턴스 실행하고 시작하기:
             * 
             * docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672
             * --restart=unless-stopped -e RABBITMQ_DEFAULT_USER . -e RABBITMQ_DEFAULT_PASS .
             * rabbitmq:managment
             * 
             * RabbitMQ를 통해서 execution 오더를 받아오기
             */
            var factory = new ConnectionFactory() {
                HostName = "localhost",
                UserName = "simpli",
                Password = "simsimpli123data123"
            };
            var connection = factory.CreateConnection();

            // RabbitMQ Producer 생성
            Rabbit = connection.CreateModel();
            Rabbit.QueueDeclare(queue: "kiwoom-data",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        private void InitializeKafka()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                ClientId = Dns.GetHostName(),
            };

            // Kafka Producer 생성
            Kafka = new ProducerBuilder<Null, string>(config).Build();
        }

        private void KafkaErrorHandler(DeliveryReport<Null, string> res)
        {
            // pass: 우선은 오류가 나면 할 수 있는게 없기 때문
        }

        public void GetPortfolioInfo()
        {
            // 계좌번호를 받아온 다음 요청보내기
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
        }

        public void GetFuturesCodeList()
        {
            foreach (bool index in new List<bool> { true, false }) {
                string futuresCodeListStr;
                string[] futuresCode;

                if (!index)
                {
                    // 주식 선물
                    futuresCodeListStr = axKHOpenAPI1.GetSFutureList("");
                    futuresCode = futuresCodeListStr.Split('|');
                }
                else
                {
                    // 지수 선물
                    futuresCodeListStr = axKHOpenAPI1.GetFutureList();
                    futuresCode = futuresCodeListStr.Split(';');
                }

                List<string> cleanFuturesCode = new List<string>();
                foreach (string code in futuresCode)
                {
                    if (code.Length > 0)
                    {
                        string fCode = code.Split('^')[0];
                        cleanFuturesCode.Add(fCode);
                    }
                }

                List<string> fuCodeSlice = new List<string>();
                foreach (string code in cleanFuturesCode)
                {
                    string fCodeSlice = code.Substring(1, 2);
                    fuCodeSlice.Add(fCodeSlice);
                }
                fuCodeSlice = fuCodeSlice.Select(x => x).Distinct().ToList();

                List<List<string>> totalFuCode = new List<List<string>>();
                foreach (string codeSlice in fuCodeSlice)
                {
                    List<string> tmp = new List<string>();
                    foreach (string code in cleanFuturesCode)
                    {
                        if (codeSlice == code.Substring(1, 2))
                        {
                            tmp.Add(code);
                        }
                    }
                    totalFuCode.Add(tmp.GetRange(0, 3));
                }

                List<string> flattenFuCode = new List<string>();
                foreach (var list in totalFuCode)
                {
                    foreach (string code in list)
                    {
                        flattenFuCode.Add(code);
                    }
                }

                codelist.AddRange(flattenFuCode);
            }
        }

        public void RegisterRealTimeData()
        {
            // 장시작시간 등록
            axKHOpenAPI1.SetRealReg("2001", "", "215", "0");

            int cnt = 0;
            int stockScreenNumber = 3000;

            try
            {
                foreach (string code in codelist)
                {
                    // 주식체결 등록
                    axKHOpenAPI1.SetRealReg(stockScreenNumber.ToString(),
                                            code,
                                            "20;41", // 체결시간
                                            "1");

                    cnt += 1;
                    logTextBox.AppendText($"{cnt.ToString()} 실시간 등록: {code}. Screen Num: {stockScreenNumber.ToString()}\n");

                    if (cnt % 50 == 0)
                    {
                        stockScreenNumber += 1;
                    }
                }

                // 등록 완료 후 label 업데이트해주기
                realtimeReadyLabel.Text = $"실시간 등록 완료 {cnt.ToString()}개 종목";
            }
            catch
            {
                RegisterRealTimeData();
            }

            registered = true;
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

                if (connectedServer == "1")
                {
                    serverType = "모의";
                }
                else
                {
                    serverType = "실서버";
                }

                userIdLabel.Text = $"접속 유저: {userID}";
                userNameLabel.Text = $"접속자 이름: {userName}";
                serverTypeLabel.Text = $"서버 종류: {serverType}";
                accountLabel.Text = $"계좌번호: {accno}";


                GetPortfolioInfo();
                GetCodeList();
                GetFuturesCodeList();
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
            }
            else if (e.sRQName == "예수금상세현황요청")
            {
                // pass
            }
            else if (e.sRQName == "실시간미체결요청")
            {
                // pass
            }
            else if (e.sRQName == "주식분봉차트조회")
            {
                // pass
            }
            else if (e.sRQName == "업종일봉차트조회")
            {
                // pass
            }
        }

        public void onReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            if (registered)
            {
                if (e.sRealType == "장시작시간")
                {
                    GetMarketState(e);
                }

                if ((e.sRealType == "주식호가잔량") || (e.sRealType == "주식선물호가잔량") || (e.sRealType == "선물호가잔량"))
                {
                    GetHoga(e);
                }

                if ((e.sRealType == "주식체결") || (e.sRealType == "선물시세"))
                {
                    GetTick(e);
                }
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
            string code = e.sRealKey;
            string hogaDate = axKHOpenAPI1.GetCommRealData(e.sRealType, 21);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
            string hogaDataString = $"hoga;{code};{hogaDate};{timestamp}";

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

            string totalBuyHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 125); // 매수호가총잔량
            string totalSellHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 121); // 매도호가총잔량
            string netBuyHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 128); // 순매수잔량
            string netSellHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 138); //순매도잔량
            string ratioBuyHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 129); // 매수비율
            string ratioSellHogaAmt = axKHOpenAPI1.GetCommRealData(e.sRealType, 139); // 매도비율
            string agentTicker = axKHOpenAPI1.GetCommRealData(e.sRealType, 216); // 투자자별 ticker (제공해주면)

            hogaDataString += $"{totalBuyHogaAmt};{totalSellHogaAmt};{netBuyHogaAmt};{netSellHogaAmt};{ratioBuyHogaAmt};{ratioSellHogaAmt};{agentTicker};";

            //StreamWriter sw = new StreamWriter("C:\\Users\\simpl\\OneDrive\\바탕 화면\\Projects\\tests\\files\\hogatest.txt", append: true);
            //sw.WriteLine(hogaDataString);
            //sw.Flush();
            //sw.Close();

            Kafka.Produce("kiwoom-data", new Message<Null, string> { Value = hogaDataString }, KafkaErrorHandler);
            Rabbit.BasicPublish(exchange: "", routingKey: "kiwoom-data", basicProperties: null, body: Encoding.UTF8.GetBytes(hogaDataString));
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
             * 고가: 17
             * 시가: 16
             * 저가: 18
             * 거래회전율: 31
             * 거래비용: 32
             */
            string code = e.sRealKey;
            string tradeDate = axKHOpenAPI1.GetCommRealData(e.sRealType, 20);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
            string currentPrice = axKHOpenAPI1.GetCommRealData(e.sRealType, 10);
            string openPrice = axKHOpenAPI1.GetCommRealData(e.sRealType, 16);
            string high = axKHOpenAPI1.GetCommRealData(e.sRealType, 17);
            string low = axKHOpenAPI1.GetCommRealData(e.sRealType, 18);
            string volume = axKHOpenAPI1.GetCommRealData(e.sRealType, 15);
            string cumVolume = axKHOpenAPI1.GetCommRealData(e.sRealType, 13);
            string tradeSellHoga1 = axKHOpenAPI1.GetCommRealData(e.sRealType, 27);
            string tradeBuyHoga1 = axKHOpenAPI1.GetCommRealData(e.sRealType, 28);

            string tickDataString = $"tick;{code};{tradeDate};{timestamp};";
            tickDataString += $"{currentPrice};{openPrice};{high};{low};{volume};{cumVolume};{tradeSellHoga1};{tradeBuyHoga1};";

            //StreamWriter sw = new StreamWriter("C:\\Users\\simpl\\OneDrive\\바탕 화면\\Projects\\tests\\files\\ticktest.txt", append: true);
            //sw.WriteLine(tickDataString);
            //sw.Flush();
            //sw.Close();

            Kafka.Produce("kiwoom-data", new Message<Null, string> { Value = tickDataString }, KafkaErrorHandler);
            Rabbit.BasicPublish(exchange: "", routingKey: "kiwoom-data", basicProperties: null, body: Encoding.UTF8.GetBytes(tickDataString));
        }

        private void ExchangeEngineForm_Load(object sender, EventArgs e)
        {

        }
    }
}
