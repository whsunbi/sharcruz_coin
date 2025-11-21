using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Bitget.Net.Objects.Models.V2;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Net.Http;
using System.Diagnostics;

using System.Speech.Synthesis;
using System.Media;


namespace sharcruz_coin
{

    public partial class Form_main : KryptonForm
    {

        private volatile bool stopThreads = false; // 취소 플래그

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        //////////////////////////////////////////////////////////////////////////////////////////////////

        static public string CONFIG_FILE = "C:\\sharcruz_conf.ini";

        public StringBuilder sbApiKey = new StringBuilder();
        public StringBuilder sbApiSecret = new StringBuilder();
        public StringBuilder sbPassphrase = new StringBuilder();

        public StringBuilder sbChatId = new StringBuilder();
        public StringBuilder sbToken_general = new StringBuilder();
        public StringBuilder sbToken_summary = new StringBuilder();

        public StringBuilder sbInvestMoney = new StringBuilder();
        public StringBuilder sbLeverage = new StringBuilder();
        public StringBuilder sbTargetProfit = new StringBuilder();
        public StringBuilder sbMaxLoss = new StringBuilder();

                                                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                                                                                            //static string _symbol = "";
        static bool _bRealTrade = false; // 실전모드
        static bool _bRealSuccess = false; // 목표달성
        static decimal _investMoney = 7;//36;//72;//7; // $ 215(30만원), 358(50만원), 716(100만원) // 투자금 달러단위 $
        static decimal _investMoney_margin = 7;
        //static decimal _nLeverage = 1;
        static decimal _nLeverage = 5;
        static decimal _RealOutput = 0;
        static decimal _RealPnl = 0;
        static decimal _RealRoi = 0;



        //static AsyncLocal<decimal> _ema10 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _ema15 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _ema50 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lastPrice_beforeCandle = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lastPrice_beforeCandle_old = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<bool> _bLastPrice_beforeCandle = new AsyncLocal<bool> { Value = false };

        static AsyncLocal<int> _decimalPlaces = new AsyncLocal<int> { Value = 0 };
        //static AsyncLocal<decimal> _tickSize = new AsyncLocal<decimal> { Value = 1 };
        static AsyncLocal<decimal> _minOrderQuantity = new AsyncLocal<decimal> { Value = 5 };
        //static AsyncLocal<decimal> _highestHigh = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _highestHigh_15 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lowestLow = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lowestLow_15 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lowestLow_30 = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _lastPrice = new AsyncLocal<decimal> { Value = 0 };

        // 범위 기본값: "Sheet1"
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "BitgetToSheets";

        // 취소 토큰(쓰레드 종료 시 제어용)
        //static CancellationTokenSource cts = null;
        static decimal _mode = 1;
        static AsyncLocal<decimal> _mode_direction = new AsyncLocal<decimal> { Value = 0 }; // 1:up, 2:down

        static DateTime _startTime_coinSearch = DateTime.Now;
        //static string _orderId_trailingStop = "";

        // ✅ 기존 A열의 값들을 DateTime HashSet에 저장    
        static HashSet<DateTime> _existingKeys = new HashSet<DateTime>();

        static AsyncLocal<bool> _bEma10_underEma15 = new AsyncLocal<bool> { Value = false };

        //static AsyncLocal<decimal> _fCutStep = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _totalAveragePrice = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<string> _strMemo = new AsyncLocal<string> { Value = "" };
        //static AsyncLocal<bool> _bEnterStart = new AsyncLocal<bool> { Value = false };

        //static AsyncLocal<DateTime> _timeContract = new AsyncLocal<DateTime> { };
        static AsyncLocal<decimal> _nEnterMode = new AsyncLocal<decimal> { Value = 0 };
        //static AsyncLocal<decimal> _roi_current_max = new AsyncLocal<decimal> { Value = -99 };

        //static AsyncLocal<decimal> _bongsize_max = new AsyncLocal<decimal> { Value = 0 };

        //static AsyncLocal<decimal> _roi_current = new AsyncLocal<decimal> { Value = 0 };





        // 쓰레드 단위 전역변수
        private static AsyncLocal<int> threadId = new AsyncLocal<int>();
        private static AsyncLocal<string> threadSymbol = new AsyncLocal<string> { Value = "" };






        public Form_main()
        {
            InitializeComponent();
        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("sss");

            saveLog(this, 0);
        }

        private void numLeverage_Click(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void numLeverage_Enter(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void kryptonNumericUpDown1_Click(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void kryptonNumericUpDown1_Enter(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void kryptonNumericUpDown2_Click(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void kryptonNumericUpDown2_Enter(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void kryptonTextBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 숫자(0~9), 백스페이스 허용
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // 입력 차단
            }
        }


        // start
        // 시작
        private async void Form_main_Load(object sender, EventArgs e)
        {
            // 모니터(스크린) 정보 가져오기
            var screens = Screen.AllScreens;

            if (screens.Length == 1)
            {
                // 모니터가 1개면 (0,0)에 표시
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(0, 15);
            }
            /*else
            {
                // 여러 개면, 두 번째 모니터 중앙에 표시 (예시)
                var second = screens.ElementAtOrDefault(1);
                if (second != null)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = second.WorkingArea.Location;
                }
            }
            */
            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 모니터 수: {screens.Length}");


            if (loadConfig() == false)
                return;

            //this.Icon = Properties.Resources.sharcruz_coin3;
            tabControl1.SelectedIndex = 1; // 2page

            try
            {
                speech(" 샤크루즈 코인 시작.");
                //await Task.Delay(30000); // 30초 간격
            }
            catch (Exception ex)
            {
                int a = 0;
            }
            // 열 초기화            

            // 스타일
            //dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 240, 250);
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }


            // 전체 그리드 라운딩, 배경색
            dataGridView1.StateCommon.DataCell.Back.Color1 = System.Drawing.Color.White;
            dataGridView1.StateCommon.DataCell.Back.Color2 = System.Drawing.Color.White;
            dataGridView1.StateCommon.DataCell.Back.ColorAngle = 45F;

            dataGridView1.StateCommon.DataCell.Border.Color1 = System.Drawing.Color.LightGray;
            dataGridView1.StateCommon.DataCell.Border.Color2 = System.Drawing.Color.LightGray;
            dataGridView1.StateCommon.DataCell.Border.Rounding = 5; // 라운딩
            dataGridView1.StateCommon.DataCell.Border.Width = 1;
            // 행 높이 조절 금지
            dataGridView1.AllowUserToResizeRows = false;



            //dataGridView1.StateCommon.HeaderColumn.Back.Color1 = System.Drawing.Color.DarkBlue;
            //dataGridView1.StateCommon.HeaderColumn.Back.Color2 = System.Drawing.Color.MediumBlue;
            //dataGridView1.StateCommon.HeaderColumn.Content.Color1 = System.Drawing.Color.White;
            dataGridView1.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 9, FontStyle.Bold);
            // 헤더 텍스트 가운데 정렬            
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns["진입날짜"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["요일"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["진입시간"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["수익률"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["수익금"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["투입금"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["코인명"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["포지션"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["수량"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dataGridView1.Columns["진입날짜"].Width = 80;
            dataGridView1.Columns["요일"].Width = 30;
            dataGridView1.Columns["진입시간"].Width = 65;
            dataGridView1.Columns["수익률"].Width = 60;
            dataGridView1.Columns["수익금"].Width = 50;
            dataGridView1.Columns["투입금"].Width = 60;
            dataGridView1.Columns["코인명"].Width = 90;
            dataGridView1.Columns["포지션"].Width = 40;
            dataGridView1.Columns["수량"].Width = 65;

            // 전체 그리드 라운딩, 배경색
            dataGridView2.StateCommon.DataCell.Back.Color1 = System.Drawing.Color.White;
            dataGridView2.StateCommon.DataCell.Back.Color2 = System.Drawing.Color.White;
            dataGridView2.StateCommon.DataCell.Back.ColorAngle = 45F;

            dataGridView2.StateCommon.DataCell.Border.Color1 = System.Drawing.Color.LightGray;
            dataGridView2.StateCommon.DataCell.Border.Color2 = System.Drawing.Color.LightGray;
            dataGridView2.StateCommon.DataCell.Border.Rounding = 5; // 라운딩
            dataGridView2.StateCommon.DataCell.Border.Width = 1;

            //dataGridView1.StateCommon.HeaderColumn.Back.Color1 = System.Drawing.Color.DarkBlue;
            //dataGridView1.StateCommon.HeaderColumn.Back.Color2 = System.Drawing.Color.MediumBlue;
            //dataGridView1.StateCommon.HeaderColumn.Content.Color1 = System.Drawing.Color.White;
            dataGridView2.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 9, FontStyle.Bold);
            // 헤더 텍스트 가운데 정렬            
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;




            dataGridView2.Columns["진입날짜2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView2.Columns["요일2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView2.Columns["수익률2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView2.Columns["수익금2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView2.Columns["수익금2_원"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            dataGridView2.Columns["진입날짜2"].Width = 80;
            dataGridView2.Columns["요일2"].Width = 30;
            dataGridView2.Columns["수익률2"].Width = 50;
            dataGridView2.Columns["수익금2"].Width = 50;
            dataGridView2.Columns["수익금2_원"].Width = 80;



            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 스레드 시작!!");

            stopThreads = false; // 시작 시 초기화

            var emaWrapper1 = new EmaWrapper();
            var emaWrapper2 = new EmaWrapper();
            var emaWrapper3 = new EmaWrapper();
            var emaWrapper4 = new EmaWrapper();
            var emaWrapper5 = new EmaWrapper();

            var globalWrapper1 = new GlobalWrapper();
            var globalWrapper2 = new GlobalWrapper();
            var globalWrapper3 = new GlobalWrapper();
            var globalWrapper4 = new GlobalWrapper();
            var globalWrapper5 = new GlobalWrapper();


            Thread t1 = new Thread(() => runCoin(this, 1, emaWrapper1, globalWrapper1));
            Thread t2 = new Thread(() => runCoin(this, 2, emaWrapper2, globalWrapper2));
            Thread t3 = new Thread(() => runCoin(this, 3, emaWrapper3, globalWrapper3));

            Thread t4 = new Thread(() => runCoin(this, 4, emaWrapper4, globalWrapper4));
            Thread t5 = new Thread(() => runCoin(this, 5, emaWrapper5, globalWrapper5));

            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 1...");
            t1.Start();
            
            await Task.Delay(1000);
            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 2...");
            t2.Start();
            await Task.Delay(1000);
            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 3...");
            t3.Start();

            await Task.Delay(1000);
            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 4...");
            t4.Start();
            await Task.Delay(1000);
            Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 5...");
            t5.Start();
            

            Logger($"=====================================================================================");

            /*
            // 예: 5초 후 모든 쓰레드 종료 요청
            Task.Delay(7000).ContinueWith(_ =>
            {
                stopThreads = true;
                Logger("쓰레드 종료 요청 완료!!");
            });
            */
        }

        private async Task runCoin(Form_main form, int id, EmaWrapper emaWrapper, GlobalWrapper globalWrapper)
        {

            //var emaWrapper = new EmaWrapper();
            decimal _lastPrice = 0;
            decimal _tickSize = 0;
            decimal _roi_current = 0;
            decimal _roi_current_max = -99m;
            bool _bEnterStart = false;
            decimal _totalAveragePrice = 0;
            string _strMemo = "";


            threadId.Value = id;


            /*
            // get
            await setStopLoss_history(this, apiKey, apiSecret, passphrase);
            return;
            */

            /*
            for (int i = 0; i < 1000; i++) // 큰 수로 반복
            {
                if (stopThreads) break; // 종료 플래그 체크

                string msg = $"[작업 {id}] 실행 중... {i}";
                form.Logger( msg);

                Thread.Sleep(1000);
            }

            form.Logger( $"[작업 {id}] 종료됨.");
            //return;
            */

            //_mode = args.Length > 0 ? decimal.Parse(args[0]) : 1;
            //_mode = 1;
            form.Logger($">>>>> 모드 : {threadId.Value}");

            if (threadId.Value == 2)
                await Task.Delay(2000);
            else if (threadId.Value == 3)
                await Task.Delay(4000);
            else if (threadId.Value == 4)
                await Task.Delay(6000);
            else if (threadId.Value == 5)
                await Task.Delay(8000);

            //return;


            // 콘솔 핸들 가져오기
            IntPtr handle = GetConsoleWindow();

            if (handle != IntPtr.Zero)
            {
                // 콘솔 위치와 크기 지정
                if (threadId.Value == 1)
                    MoveWindow(handle, 2450, 0, 700, 520, true);
                else if (threadId.Value == 2)
                    MoveWindow(handle, 3140, 0, 700, 520, true);
                else
                    MoveWindow(handle, 3140, 510, 700, 520, true);
            }

            form.Logger("콘솔 위치와 크기 변경 완료!");
            //Console.ReadLine();

            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ▶▶▶ 자동매매 시작..!!! ◀◀◀");



            var client = new BitgetRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(apiKey, apiSecret, passphrase);
            });


            // 수익률 조회 : position history
            //await getPosHistory(client);

            var socketClient = new BitgetSocketClient();

            if (threadId.Value == 3) // 마지막에 추가.
                await getPosHistory(this, client, id, _roi_current_max, false, _strMemo, globalWrapper, _nLeverage);




            // 이동평균(산술평균 SMA) 계산 (스레드)
            form.Logger($"이평선 계산 시작.");

            ///cts = null;
            CancellationTokenSource cts_ma = new CancellationTokenSource();
            //Task maTask = null;
            Thread maTask = null;

            while (true)
            {
                // 초기화 다다다다!
                dummy();
                emaWrapper._bFinish = true; // ma 스레드 죽이기!!!!

                emaWrapper._ema2 = 0;
                emaWrapper._ema2_1 = 0;
                emaWrapper._ema5 = 0;
                emaWrapper._ema5_2 = 0;
                emaWrapper._ema5_2 = 0;
                emaWrapper._ema10 = 0;
                emaWrapper._ema15 = 0;
                emaWrapper._ema50 = 0;
                emaWrapper._ema50_1 = 0;
                emaWrapper._lastPrice_beforeCandle = 0;
                emaWrapper._bb_upper = 99999m;
                emaWrapper._bb_lower = -99999m;
                emaWrapper._bb_middle = 0m;

                emaWrapper._bb_upper_2 = 99999m;
                emaWrapper._bb_lower_2 = -99999m;
                emaWrapper._bb_upper_3 = 99999m;
                emaWrapper._bb_lower_3 = -99999m;
                emaWrapper._bb_upper_4 = 99999m;
                emaWrapper._bb_lower_4 = -99999m;
                emaWrapper._bb_upper_5 = 99999m;
                emaWrapper._bb_lower_5 = -99999m;

                emaWrapper._low_1 = 0m;
                emaWrapper._low_2 = 0m;
                emaWrapper._low_3 = 0m;



                //_fCutStep.Value = 0;
                globalWrapper._entryStep = 1;
                globalWrapper._side = OrderSide.Sell; // 기본 숏
                globalWrapper._cutStep = 0;
                globalWrapper._bb_approach = 0;
                globalWrapper._50_approach = 0;
                globalWrapper._createTime = "";
                globalWrapper._detectedPercent = 0;

                _bEma10_underEma15.Value = false;
                _existingKeys.Clear();

                resetMonitoring(form, id);
                threadSymbol.Value = ""; // 초기화



                _startTime_coinSearch = DateTime.Now;

                form.Logger(" ");
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ▷ 종목 탐색 중........");

                // 코인선물 종목 조회 : USDT-M Futures
                var tickersResult = await client.FuturesApiV2.ExchangeData.GetTickersAsync(BitgetProductTypeV2.UsdtFutures);

                if (!tickersResult.Success)
                {
                    form.Logger($"코인선물 조회 실패: {tickersResult.Error}, 5초 대기...");
                    await Task.Delay(5000); // 5초 대기
                    continue;
                }

                // 24시간 등락률 기준 정렬
                // 거래 가능한(online) 티커만 필터링 후 24시간 등락률 기준 정렬            
                var tickers = tickersResult.Data
                    //.Where(t => t.DeliveryStatus.ToString() == "delivery_normal")  // 온라인 상태만
                    //.Where(t => t.DeliveryStatus == DeliveryStatus.Normal)
                    .Select(t => new
                    {
                        QuoteVolume = t.QuoteVolume,
                        Symbol = t.Symbol,
                        LastPrice = t.LastPrice,
                        ChangePercent = t.ChangePercentage24H * 100m
                    })
                    .OrderByDescending(t => t.ChangePercent)
                    .ToList();

                // form.Logger($"=== USDT-M Futures 등락률(24h) 순위 ===");
                int i = 0;
                decimal checkTrading = 0;

                
                foreach (var t in tickers)
                {
                    //if (t.ChangePercent < 10.0m || t.QuoteVolume < 10000000) // 거래대금
                    if (t.ChangePercent < 10.0m || t.QuoteVolume < 4000000) // 거래대금
                        continue;

                    form.Logger(" ");

                    globalWrapper._detectedPercent = Math.Round((decimal)t.ChangePercent, 1);

                    var contractResult = await client.FuturesApiV2.ExchangeData.GetContractsAsync(BitgetProductTypeV2.UsdtFutures, symbol: t.Symbol);
                    if (!contractResult.Success)
                    {
                        form.Logger($"[Error] 코인정보 조회 실패 : {t.Symbol} -> {contractResult.Error}, 5초 대기...");
                        await Task.Delay(5000); // 5초 대기
                        continue;
                    }
                    else
                    {
                        var data = contractResult.Data;

                        // data가 null 또는 비어있는지 체크
                        if (data != null && data.Count() > 0)
                        {
                            // data가 배열이므로 JArray로 변환
                            JArray dataArray = JArray.FromObject(data);

                            foreach (var item in dataArray)
                            {
                                if (item["Symbol"] != null && item["PriceDecimals"] != null)
                                {
                                    _tickSize = GetTickSize(decimal.Parse(item["PriceDecimals"].ToString()));
                                    emaWrapper._tickSize = _tickSize;

                                    //_minOrderQuantity.Value = decimal.Parse(item["MinOrderQuantity"].ToString());
                                    _minOrderQuantity.Value = decimal.Parse(item["MinTradeValue"].ToString());


                                    form.Logger($"* 코인정보 조회 : {t.Symbol}, {item["PriceDecimals"]}, tickSize : {_tickSize}. minOrderQuantity : {_minOrderQuantity.Value}");

                                }
                                else
                                {
                                    // form.Logger($"[Error] {t.Symbol}, Tick Size error :  Symbol, PriceDecimals property does not exist.");
                                    form.Logger($"[Error] {t.Symbol}, Tick Size error :  Symbol, PriceDecimals property does not exist.");
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            form.Logger($"* 코인정보 조회 : {t.Symbol}, UnKnown Coin..!");
                            continue;
                        }

                        // 가격 체크
                        //if (t.LastPrice > 6) // 최대 투입금액
                        if (t.LastPrice > _investMoney) // 최대 투입금액
                        {
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] {t.Symbol}  : 최대주문 금액 초과 : 가격이 주문금액보다 큽니다. ({t.LastPrice}) > {_investMoney}");
                            continue;
                        }
                        else if (_minOrderQuantity.Value >= 20) // 최대 투입금액
                        {
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] {t.Symbol}  : 최소주문 금액 초과 : 투자금보다 최소주문 금액이 큽니다. ({_minOrderQuantity.Value})");
                            continue;
                        }
                        else if(globalWrapper._detectedPercent > 60 && DateTime.Now.Hour >= 10)
                        {
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] {t.Symbol}  : 포착등락률 > 60 and Hour >= 10 : 포착등락률이 너무 높습니다. ({globalWrapper._detectedPercent.ToString()})");
                            continue;
                        }
                        else
                        {
                            //checkTrading = addDataToGoogleSheet_monitoring(id, this, t.Symbol);
                            checkTrading = addMonitoring(this, id, t.Symbol);
                            if (checkTrading > 0)
                            {
                                form.Logger($"  [제외] {t.Symbol} : 이미 {checkTrading}번 프로세스가 모니터링 중입니다.");
                                continue;
                            }
                            else
                            {
                                /*
                                //form.Logger($"모니터링 중으로 등록 : {t.Symbol}");
                                // OK: 계속 아래 로직 진행
                                DateTime now = DateTime.Now; // 로컬시간
                                if (now.Hour >= 7 && (now.Hour < 18 || (now.Hour == 18 && now.Minute <= 30)))
                                {
                                    // 통과 낮시간.
                                }
                                else
                                {

                                    await calctMovingAverage(client, t.Symbol);

                                    if (_ema15.Value > 0 && _ema15.Value > _ema50.Value)
                                    {
                                        form.Logger($"[Error] {t.Symbol} : 상방 제외 (18:30 ~ 06:00)");
                                        continue;
                                    }
                                }
                                */
                            }



                        }
                    }



                    i++;
                    //form.Logger($"{i} : {t.Symbol}: {t.ChangePercent:f1}%  ({t.LastPrice}, {t.QuoteVolume:n0})");
                    form.Logger($"  ☞ {t.Symbol}   {t.ChangePercent:f1}%  ({t.LastPrice}, {t.QuoteVolume:n0})");

                    

                    //if (i == _mode)
                    {
                        threadSymbol.Value = t.Symbol;
                        emaWrapper._symbol = t.Symbol;
                        
                        break;
                    }
                }

                if (threadSymbol.Value == "")
                {
                    resetMonitoring(form, id);

                    form.Logger(" ");
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ■ 알맞은 코인이 없습니다. 5분 대기..");
                    form.Logger(" ");

                    await Task.Delay(300000);
                    continue;
                }


                form.Logger(" ");
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ▶ 종목 탐색 완료 : {threadSymbol.Value}");
                

                

                // 마진모드 설정
                await setMarginMode(this, apiKey, apiSecret, passphrase, "isolated");
                await Task.Delay(500);

                // 레버리지 설정
                await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);
                await Task.Delay(500); // 0.5초 정도            

                /*
                string url = "https://api.bitget.com/api/v2/mix/market/tickers?productType=USDT-FUTURES";
                using HttpClient http_client = new HttpClient();
                HttpResponseMessage response = await http_client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(jsonResponse);

                if (data["code"]?.ToString() == "00000" && data["data"] != null)
                {
                    var tickers = data["data"]
                        .Where(t => t["change24h"] != null)
                        .Select(t => new
                        {
                            Symbol = t["symbol"].ToString(),
                            Change24h = decimal.Parse(t["change24h"].ToString()) * 100 // 퍼센트로 변환
                        })
                        .OrderByDescending(t => t.Change24h)
                        .ToList();

                    form.Logger("24시간 등락률 기준 코인 목록:");
                    foreach (var ticker in tickers)
                    {
                        form.Logger($"{ticker.Symbol}: {ticker.Change24h}%");
                    }
                }
                else
                {
                    form.Logger("API 호출 실패 또는 데이터 없음");
                }

                */


                // 초기화
                //_ema10 = 0;
                //_ema15 = 0;
                //_ema50 = 0;
                //_lastPrice_beforeCandle = 0;
                _bEnterStart = false;
                _nEnterMode.Value = 0;
                _totalAveragePrice = 0m;
                _roi_current = 0m;
                _roi_current_max = -99m;
                _lastPrice = 0;
                emaWrapper._bongsize_max = 0;

                _mode_direction.Value = 0;

                _strMemo = "";

                //

                _investMoney_margin = 7; // 증거금 x1
                //_investMoney = 72;// 7; // Position margin = 증거금(투자금 원금) // 10만원                
                //_nLeverage = 1;
                _nLeverage = 5;

                _investMoney = _investMoney_margin * _nLeverage; // 증거금 * 레버리지 배율 = 실제 가상의 계산된 투입금 결과치!!

                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 설정 : _investMoney_margin = {_investMoney_margin}, _nLeverage = {_nLeverage} => _investMoney = {_investMoney}");
                /*

                try 
{
                    // 이전 Task가 실행 중이면 취소
                    if (maTask != null && !maTask.IsCompleted && maTask.Status != TaskStatus.Created)
                    {
                        cts_ma.Cancel();       // 이전 Task 종료 시도
                        try
                        {
                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Task 안전하게 종료 시작");
                            await maTask;      // 안전하게 대기
                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Task 안전하게 종료 완료");
                        }
                        catch (OperationCanceledException)
                        {
                            // 취소로 인한 정상 종료
                        }
                        finally
                        {                            
                            cts_ma.Dispose();
                            cts_ma = new CancellationTokenSource(); // 새 토큰 생성
                            //form.Logger($"CancellationTokenSource 안전하게 종료 완료");
                        }
                    }

                    // 새 Task 생성 및 시작
                    emaWrapper._bFinish = false;
                    
                    maTask = Task.Run(() => calcMovingAverageThread(this, client, cts_ma.Token, emaWrapper));
                }
                catch (Exception ex)
                {
                    form.Logger($"Task 재시작 오류: {ex.Message}");
                }
                */

                try
                {
                    // 이전 Thread가 실행 중이면 취소
                    if (maTask != null && maTask.IsAlive)
                    {
                        cts_ma.Cancel(); // 취소 신호 전달

                        try
                        {
                            // 안전하게 종료 대기 (최대 3초)
                            int waited = 0;
                            while (maTask.IsAlive && waited < 30)
                            {
                                await Task.Delay(100);
                                waited++;
                            }

                            if (maTask.IsAlive)
                            {
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread 강제 종료 시도");
                                maTask.Abort(); // (비권장) 가능하면 내부에서 token 사용해서 안전종료
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            // 강제 종료 예외 처리
                        }
                        finally
                        {
                            //cts_ma.Dispose();
                            cts_ma.Cancel();
                            cts_ma = new CancellationTokenSource(); // 새 토큰 생성
                        }
                    }

                    // 새 Thread 생성 및 시작
                    emaWrapper._bFinish = false;

                    maTask = new Thread(() => calcMovingAverageThread(form, client, cts_ma.Token, emaWrapper));
                    maTask.IsBackground = true;
                    maTask.Start();

                    //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MA Thread 시작 (ID: {maTask.ManagedThreadId})");
                }
                catch (Exception ex)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MA Thread 재시작 오류: {ex.Message}");
                }


                /*
                // --- 1분봉 데이터 가져오기 ---
                var klines = await client.FuturesApiV2.ExchangeData.GetKlinesAsync(
                    productType: BitgetProductTypeV2.UsdtFutures, // 최신 라이브러리에서 USDT 선물 타입
                    symbol: threadSymbol.Value,
                    interval: BitgetFuturesKlineInterval.OneMinute, // 최신 interval enum
                    startTime: DateTime.UtcNow.AddMinutes(-20),
                    endTime: DateTime.UtcNow,
                    limit: 20
                );

                if (!klines.Success)
                {
                    form.Logger($"캔들 조회 실패: {klines.Error}");
                    return;
                }

                var candles = klines.Data.OrderBy(c => c.OpenTime).ToList();

                // --- 스윙 고점 2개 찾기 ---
                var highs = candles.Select((c, i) => new { Index = i, High = c.HighPrice }).ToList();
                var swingHighs = highs
                    .Where((h, i) => i > 0 && i < highs.Count - 1 &&
                                      h.High > highs[i - 1].High &&
                                      h.High > highs[i + 1].High)
                    .ToList();

                if (swingHighs.Count < 2)
                {
                    form.Logger("고점 부족 → 추세 판단 불가");
                    return;
                }

                var lastTwo = swingHighs.Skip(swingHighs.Count - 2).ToList();
                var high1 = lastTwo[0];
                var high2 = lastTwo[1];

                if (high2.High >= high1.High)
                {
                    form.Logger("하방 추세 아님 → 진입 안함");
                    //return;
                }

                decimal slope = (high2.High - high1.High) / (high2.Index - high1.Index);
                decimal intercept = high1.High - slope * high1.Index;
                int lastIndex = highs.Last().Index;
                decimal trendLine = slope * lastIndex + intercept;



                form.Logger($"현재가: {lastPrice}, 추세선 상단: {trendLine}");

                // if (Math.Abs((lastPrice - trendLine) / trendLine) < 0.002m)
                form.Logger("추세선 근접 → 숏 지정가 진입");
                */


                // 실시간 체결데이터 조회            
                form.Logger($"  ●●● 실시간 진입타점 모니터링 중 . . .");
                form.Logger(" ");
                await setProgress(this, id, true);


                {

                    decimal underPer_ema5 = 0;
                    decimal underPer_ema15 = 0;
                    decimal underPer_ema50 = 0;
                    decimal bong_size = 0;
                    UpdateSubscription subscription = null;
                    bool coinChangeRequested = false;

                    decimal bb_up_max = 0;
                    decimal bb_down_max = 0;
                    decimal bb_up = 0;
                    decimal bb_down = 0;


                    var subscribeResult = await socketClient.FuturesApiV2.SubscribeToTickerUpdatesAsync(
                    BitgetProductTypeV2.UsdtFutures, // productType
                    threadSymbol.Value,                          // symbol
                    data =>                            // 콜백
                    {
                        _lastPrice = data.Data.LastPrice;
                        globalWrapper._lastPrice = _lastPrice;

                        if (_tickSize == 0)
                        {
                            // _tickSize 추정
                            _tickSize = GetTickSize(_lastPrice);

                            form.Logger($"_tickSize: {_tickSize}");
                        }

                        underPer_ema5 = Math.Round((emaWrapper._ema5 - _lastPrice) / _lastPrice * 100m, 2);
                        underPer_ema15 = Math.Round((emaWrapper._ema15 - _lastPrice) / _lastPrice * 100m, 2);
                        underPer_ema50 = Math.Round((emaWrapper._ema50 - _lastPrice) / _lastPrice * 100m, 2);
                        if (emaWrapper._lastPrice_beforeCandle > 0)
                            bong_size = Math.Round((emaWrapper._lastPrice_beforeCandle - _lastPrice) / _lastPrice * 100m, 2);


                        // 볼밴 위 최고값
                        if (DateTime.Now.Second > 3)
                            if (_lastPrice > emaWrapper._bb_middle)
                            {
                                bb_down_max = 0;
                                bb_down = 0;

                                bb_up = Math.Round((_lastPrice - emaWrapper._bb_upper) / emaWrapper._bb_upper * 100m, 2);
                                if (bb_up > 0 && bb_up > bb_up_max)
                                    bb_up_max = bb_up;
                            }
                            else if (_lastPrice < emaWrapper._bb_middle)
                            {
                                bb_up_max = 0;
                                bb_up = 0;

                                bb_down = Math.Round((emaWrapper._bb_lower - _lastPrice) / _lastPrice * 100m, 2);
                                if (bb_down > 0 && bb_down > bb_down_max)
                                    bb_down_max = bb_down;
                            }


                        //form.Logger($"[{DateTime.Now:HH:mm:ss}] abovePer_bb = {abovePer_bb}, abovePer_bb_down_max = {abovePer_bb_down_max}, abovePer_bb_down_max = {abovePer_bb_down_max}");









                        ///////////////////////////////////////////////////////////////////////////
                        // 실시간 로깅, 로그
                        //form.Logger($"[{DateTime.Now:HH:mm:ss}] 실시간 현재가: {data.Data.LastPrice}");
                        //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {threadSymbol.Value} :: {_ema50.Value} > {_ema10.Value} > {_ema15.Value} && {_lastPrice_beforeCandle .Value} > {_ema10.Value} && {lastPrice} < {_ema10.Value} || {underPer_ema15.Value}, {bong_size}");
                        //form.Logger($"\r" + new string(' ', 60)); // 줄 전체 공백으로 지움, Console.WindowWidth                  
                        //form.Logger(output.PadRight(Console.WindowWidth - 1));

                        if (_bEnterStart == true)
                        {
                            // 현재 수익률
                            _roi_current = GetCurrentProfitRate(this, client, globalWrapper, _lastPrice, _totalAveragePrice);

                            form.Invoke(new Action(() =>
                            {
                                form.tbRealOutput_current.Text = _roi_current.ToString();

                                if (globalWrapper._createTime == form.dataGridView1.Rows[form.dataGridView1.Rows.Count - 1].Cells["진입시간"].Value.ToString().Trim())
                                {
                                    form.dataGridView1.Rows[form.dataGridView1.Rows.Count - 1].Cells["수익률"].Value = _roi_current.ToString();
                                    form.dataGridView1.Rows[form.dataGridView1.Rows.Count - 1].Cells["수익금"].Value = (globalWrapper._investMoney * _roi_current / 100m).ToString("F2");

                                    var lastRow = form.dataGridView1.Rows[form.dataGridView1.Rows.Count - 1];
                                    if (_roi_current > 0)
                                    {
                                        // 상승 → 주황/빨강 계열
                                        lastRow.Cells["수익률"].Style.BackColor = System.Drawing.Color.OrangeRed;
                                        lastRow.Cells["수익률"].Style.ForeColor = System.Drawing.Color.White;

                                        //lastRow.Cells["수익금"].Style.BackColor = System.Drawing.Color.OrangeRed;
                                        //lastRow.Cells["수익금"].Style.ForeColor = System.Drawing.Color.White;
                                    }
                                    else if (_roi_current < 0)
                                    {
                                        // 하락 → 파랑 계열
                                        lastRow.Cells["수익률"].Style.BackColor = System.Drawing.Color.CornflowerBlue;
                                        lastRow.Cells["수익률"].Style.ForeColor = System.Drawing.Color.White;

                                        //lastRow.Cells["수익금"].Style.BackColor = System.Drawing.Color.CornflowerBlue;
                                        //lastRow.Cells["수익금"].Style.ForeColor = System.Drawing.Color.White;
                                    }
                                    else
                                    {
                                        // 보합 (0)
                                        lastRow.Cells["수익률"].Style.BackColor = System.Drawing.Color.White;
                                        lastRow.Cells["수익률"].Style.ForeColor = System.Drawing.Color.Black;

                                        //lastRow.Cells["수익금"].Style.BackColor = System.Drawing.Color.White;
                                        //lastRow.Cells["수익금"].Style.ForeColor = System.Drawing.Color.Black;
                                    }
                                }

                                if (_roi_current > 0)
                                {
                                    form.tbRealOutput_current.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                                    form.tbRealOutput_current.StateCommon.Content.Color1 = System.Drawing.Color.White;
                                }
                                else if (_roi_current < 0)
                                {
                                    form.tbRealOutput_current.StateCommon.Back.Color1 = System.Drawing.Color.CornflowerBlue;
                                    form.tbRealOutput_current.StateCommon.Content.Color1 = System.Drawing.Color.White;
                                }
                                else
                                {
                                    form.tbRealOutput_current.StateCommon.Back.Color1 = System.Drawing.Color.White;
                                    form.tbRealOutput_current.StateCommon.Content.Color1 = System.Drawing.Color.Black;
                                }

                            }));

                            if (_roi_current > _roi_current_max)
                                _roi_current_max = _roi_current;
                        }
                        else
                        {
                            //Console.SetCursorPosition(0, Console.CursorTop);
                            //form.Logger("-1");
                            //form.Logger($"\r[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {threadId.Value} {emaWrapper._ema50} | {emaWrapper._ema15}, {emaWrapper._lastPrice_beforeCandle} - {_lastPrice.ToString()}  {underPer_ema15}, {bong_size} -1");
                            //form.Logger($"\r[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {threadId.Value} {emaWrapper._ema50} | {emaWrapper._ema15}, {emaWrapper._lastPrice_beforeCandle} - {_lastPrice.ToString()}  {underPer_ema15}, {bong_size}");


                            if ((DateTime.Now - _startTime_coinSearch) >= TimeSpan.FromMinutes(20 + id))
                            //if ((DateTime.Now - _startTime_coinSearch) >= TimeSpan.FromHours(1))
                            {
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ===== 코인 교체 시간입니다. (*주기 : {20 + id}분) =====");
                                coinChangeRequested = true;

                                return;
                            }


                            //IncrementProgress(form.pbStatus1);
                            /////form.IncrementProgress(form, id, 1);

                        }

                        /*
                        if(Math.Abs(underPer_ema15.Value) > 30 || Math.Abs(bong_size) > 30)
                        {
                            SendMessage(_strChatId, $"[긴급] 오류발생!! %0A%0C {DateTime.Now:yyyy-MM-dd HH:mm:ss}] {threadSymbol.Value} :: {_ema50.Value} | {_ema15.Value}, {_lastPrice_beforeCandle .Value} ~ {data.Data.LastPrice}  {underPer_ema15.Value}, {bong_size}", _strToken_general);
                            coinChangeRequested = true;

                            return;
                        }
                        */


                    });

                    if (!subscribeResult.Success)
                    {
                        form.Logger($"[Error] 실시간 체결데이터 조회 실패: {subscribeResult.Error}");
                        return;
                    }






                    // 진입조건 체크 루프~~~
                    DateTime now = DateTime.Now; // 로컬시간
                    bool restart = false;
                    while (true)
                    {
                        await Task.Delay(100); // 체크 속도 줄이기

                        _nEnterMode.Value = 0;
                        now = DateTime.Now;
                        if (coinChangeRequested == false && subscription == null)
                        {
                            if (subscribeResult != null && subscribeResult.Success && subscribeResult.Data != null)
                            {
                                subscription = subscribeResult.Data;
                            }
                            else
                            {
                                string errMsg = subscribeResult == null
                                    ? "subscribeResult is null"
                                    : subscribeResult.Error?.Message ?? "Unknown error";

                                form.Logger($"[Error] 구독 실패: {errMsg}");
                                subscription = null;
                            }
                        }

                        if (coinChangeRequested == true) // while 문 처음으로.
                        {
                            await subscription.CloseAsync(); // 실시간 종료! 끄기.
                            subscription = null; // 재사용 방지

                            restart = true;
                            await setProgress(this, id, false);
                            break;
                        }

                        // 타점, 진입 타점
                        if (_lastPrice > 0 && emaWrapper._ema50 > 0
                        && emaWrapper._bLastPrice_beforeCandle == true
                        /*&&
                        (
                            // 50선 관통하면 제외
                            // 이전 봉과 현재 봉이 모두 50선 위이거나 아래
                            (_lastPrice_beforeCandle .Value > _ema50.Value && _lastPrice > _ema50.Value) ||
                            (_lastPrice_beforeCandle .Value < _ema50.Value && _lastPrice < _ema50.Value)
                        )
                        */
                        )
                        {
                            /*
                            // 볼밴 정방향
                            if (
                                    
                                    emaWrapper._lastPrice_beforeCandle < emaWrapper._bb_upper
                                    && _lastPrice >= emaWrapper._bb_upper
                                    && DateTime.Now.Second > 50
                                    && (DateTime.Now.Hour >= 17 || DateTime.Now.Hour <= 6)
                                    && ((emaWrapper._bb_upper - emaWrapper._ema50) / emaWrapper._ema50 * 100m) < 3
                            )
                            {
                                _nEnterMode.Value = 41;
                                form.Logger($"  ---> 41 : 볼밴 ▲상방 진입!!!! ");
                            }
                            else if (
                                    
                                    emaWrapper._lastPrice_beforeCandle > emaWrapper._bb_lower
                                        && _lastPrice <= emaWrapper._bb_lower
                                    && DateTime.Now.Second > 50
                                    && (DateTime.Now.Hour >= 17 || DateTime.Now.Hour <= 6)
                                    && ((emaWrapper._ema50 - emaWrapper._bb_lower) / emaWrapper._bb_upper * 100m) < 3
                            )
                            {
                                _nEnterMode.Value = 42;
                                form.Logger($"  ---> 42 : 볼밴 ▼하방 진입!!!! ");

                            }
                            // 볼밴 역방향
                            else */
                            if (

                                        emaWrapper._lastPrice_beforeCandle <= emaWrapper._bb_lower && // 다시 추가                                        
                                        _lastPrice > emaWrapper._bb_lower
                                        &&
                                        (
                                            emaWrapper._ema5 <= emaWrapper._bb_lower
                                            ||
                                            emaWrapper._ema5_2 <= emaWrapper._bb_lower_2
                                            ||
                                            emaWrapper._ema5_3 <= emaWrapper._bb_lower_3
                                        )
                                        //&& DateTime.Now.Second > 50
                                        //&& (DateTime.Now.Hour < 17 && DateTime.Now.Hour > 6)
                                        &&
                                        (
                                            //DateTime.Now.Hour > 6  && DateTime.Now.Hour < 20 
                                            //||
                                            (emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema5 * 100m > 2.5m //4m
                                        )
                                        //&& (DateTime.Now.Hour < 2 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20) || (DateTime.Now.Hour == 20 && DateTime.Now.Minute < 40))
                                        && (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 21)
                                        && DateTime.Now.Second > 50
                                        && 
                                        (
                                            globalWrapper._detectedPercent < 90 ||
                                            DateTime.Now.Hour < 19 // 100% 이상일 때는 저녁에 안하기.
                                        )
                                        /*
                                        &&
                                        (
                                            (
                                                globalWrapper._detectedPercent < 40 &&
                                                DateTime.Now.Hour == 11
                                            )
                                            ||
                                            DateTime.Now.Hour != 11
                                        )
                                        */
                                        && globalWrapper._detectedPercent < 90 // 코인 선정시 60이하 제외됨 (10시~)
                            )
                            {
                                _nEnterMode.Value = 91;
                                form.Logger($"  ---> 91 : 볼밴 ▲상방 진입!!!! ");
                            }
                            else if (

                                        emaWrapper._lastPrice_beforeCandle >= emaWrapper._bb_upper && // 다시 추가
                                        _lastPrice < emaWrapper._bb_upper
                                        &&
                                        (
                                            emaWrapper._ema5 >= emaWrapper._bb_upper // 5선이 볼밴상단 위
                                            ||
                                            emaWrapper._ema5_2 >= emaWrapper._bb_upper_2
                                            ||
                                            emaWrapper._ema5_3 >= emaWrapper._bb_upper_3
                                        )
                                        //&& DateTime.Now.Second > 50
                                        //&& (DateTime.Now.Hour < 17 && DateTime.Now.Hour > 6)
                                        &&
                                        (
                                            //DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20
                                            //||
                                            (emaWrapper._ema5 - emaWrapper._ema50) / emaWrapper._ema50 * 100m > 2.5m //4m
                                        )
                                        //&& (DateTime.Now.Hour < 2 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20) || (DateTime.Now.Hour == 20 && DateTime.Now.Minute < 40))
                                        && (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 21)
                                        && DateTime.Now.Second > 50
                                        &&
                                        (
                                            globalWrapper._detectedPercent < 90 ||
                                            DateTime.Now.Hour < 19 // 100% 이상일 때는 저녁에 안하기.
                                        )
                                        /*
                                        &&
                                        (
                                            (
                                                globalWrapper._detectedPercent < 40 &&
                                                DateTime.Now.Hour == 11
                                            )
                                            ||
                                            DateTime.Now.Hour != 11
                                        )
                                        */
                                        && globalWrapper._detectedPercent < 90
                            )
                            {
                                _nEnterMode.Value = 92;
                                form.Logger($"  ---> 92 : 볼밴 ▼하방 진입!!!! ");

                            }
                            /***
                            // >= 90
                            else if (

                                        emaWrapper._lastPrice_beforeCandle <= emaWrapper._bb_lower && // 다시 추가                                        
                                        _lastPrice > emaWrapper._bb_lower
                                        &&
                                        (
                                            emaWrapper._ema5 <= emaWrapper._bb_lower
                                            ||
                                            emaWrapper._ema5_2 <= emaWrapper._bb_lower_2
                                            ||
                                            emaWrapper._ema5_3 <= emaWrapper._bb_lower_3
                                        )
                                        //&& DateTime.Now.Second > 50
                                        //&& (DateTime.Now.Hour < 17 && DateTime.Now.Hour > 6)
                                        &&
                                        (
                                            //DateTime.Now.Hour > 6  && DateTime.Now.Hour < 20 
                                            //||
                                            (emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema5 * 100m > 6
                                        )
                                        //&& (DateTime.Now.Hour < 2 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20) || (DateTime.Now.Hour == 20 && DateTime.Now.Minute < 40))
                                        && (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20)
                                        && DateTime.Now.Second > 50
                                       
                                        && globalWrapper._detectedPercent >= 90
                            )
                            {
                                _nEnterMode.Value = 41;
                                form.Logger($"  ---> 41 : 볼밴 ▲상방 진입!!!! ");
                            }
                            else if (

                                        emaWrapper._lastPrice_beforeCandle >= emaWrapper._bb_upper && // 다시 추가
                                        _lastPrice < emaWrapper._bb_upper
                                        &&
                                        (
                                            emaWrapper._ema5 >= emaWrapper._bb_upper // 5선이 볼밴상단 위
                                            ||
                                            emaWrapper._ema5_2 >= emaWrapper._bb_upper_2
                                            ||
                                            emaWrapper._ema5_3 >= emaWrapper._bb_upper_3
                                        )
                                        //&& DateTime.Now.Second > 50
                                        //&& (DateTime.Now.Hour < 17 && DateTime.Now.Hour > 6)
                                        &&
                                        (
                                            //DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20
                                            //||
                                            (emaWrapper._ema5 - emaWrapper._ema50) / emaWrapper._ema50 * 100m > 6 //4m
                                        )
                                        //&& (DateTime.Now.Hour < 2 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20) || (DateTime.Now.Hour == 20 && DateTime.Now.Minute < 40))
                                        && (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 20)
                                        && DateTime.Now.Second > 50
                                      
                                        && globalWrapper._detectedPercent >= 90
                            )
                            {
                                _nEnterMode.Value = 42;
                                form.Logger($"  ---> 42 : 볼밴 ▼하방 진입!!!! ");

                            }
                            *///
                            /*
                            else if (                                           
                                _lastPrice < emaWrapper._bb_lower &&
                                bong_size > 1.5m &&
                                (
                                    (
                                        bb_down >= 2
                                        && DateTime.Now.Second >= 50
                                    )
                                    ||
                                    (
                                        bb_down >= 3
                                        //&& DateTime.Now.Second >= 50
                                    )
                                )
                            )
                            {
                                _nEnterMode.Value = 11;
                                form.Logger($"  ---> 11 : 볼밴 ▲상방 진입!!!! ");
                            }
                            else if (
                                 _lastPrice > emaWrapper._bb_upper &&
                                 bong_size > 1.5m &&
                                (
                                    (
                                        bb_up >= 2
                                        && DateTime.Now.Second >= 50
                                    )
                                    ||
                                    (
                                        bb_up >= 3
                                    //&& DateTime.Now.Second >= 50
                                    )
                                )
                            )
                            {
                                _nEnterMode.Value = 12;
                                form.Logger($"  ---> 12 : 볼밴 ▼하방 진입!!!! ");

                            }
                            */
                            /*
                            if (
                                      emaWrapper._lastPrice_beforeCandle < _lastPrice && // 양봉
                                      emaWrapper._lastPrice_beforeCandle <= emaWrapper._ema50 && // 50선 아래에서 시작
                                      (                                                           //_lastPrice > emaWrapper._ema50 && // 50선 위 돌파
                                        emaWrapper._ema2 > emaWrapper._ema50  // 50선 위 돌파
                                        &&
                                            _lastPrice > emaWrapper._highestHigh_3
                                      
                                      //||
                                      //(
                                          //_lastPrice > emaWrapper._ema50 &&
                                        //  underPer_ema50 >= 3 // 0.7m
                                      //)
                                      
                                      )
                                        && emaWrapper._lowestLow_30 <= emaWrapper._ema50  - (emaWrapper._ema50 * 0.07m)  //1.07m &&
                                        //(now.Second > 55 || (_lastPrice != 0 && (underPer_ema50 >= 0.7m)))
                                      
                                  )
                            {
                                _nEnterMode.Value = 61;
                                form.Logger($"  ---> 61 : 50선 ▲상방 진입!!!! ");
                            }
                            */
                            /*
                            if (
                                    emaWrapper._lastPrice_beforeCandle <= emaWrapper._bb_upper
                                    && _lastPrice > emaWrapper._bb_upper
                                    && emaWrapper._close_2 < emaWrapper._bb_upper_3
                                    && emaWrapper._close_3 < emaWrapper._bb_upper_4
                                    && emaWrapper._close_4 < emaWrapper._bb_upper_5
                                    && bong_size < 5
                                    && 
                                    (
                                        DateTime.Now.Second > 50
                                        || 
                                        ((_lastPrice - emaWrapper._bb_upper) / emaWrapper._bb_upper * 100m) > 1.5m
                                    )
                            )
                            {
                                _nEnterMode.Value = 41;
                                form.Logger($"  ---> 41 : 볼밴 ▲상방 진입!!!! ");
                                form.Logger($"  {_lastPrice} > {emaWrapper._bb_upper} ");
                                form.Logger($"  {emaWrapper._close_2} > {emaWrapper._bb_lower_3} ");
                                form.Logger($"  {emaWrapper._close_3} > {emaWrapper._bb_lower_4} ");
                                form.Logger($"  {emaWrapper._close_4} > {emaWrapper._bb_lower_5} ");
                            }
                            else if (
                                    emaWrapper._lastPrice_beforeCandle >= emaWrapper._bb_lower
                                    && _lastPrice < emaWrapper._bb_lower
                                    && emaWrapper._close_2 > emaWrapper._bb_lower_3
                                    && emaWrapper._close_3 > emaWrapper._bb_lower_4
                                    && emaWrapper._close_4 > emaWrapper._bb_lower_5                                    
                                    && bong_size < 5
                                     &&
                                    (
                                        DateTime.Now.Second > 50
                                        ||
                                        ((emaWrapper._bb_lower - _lastPrice) / emaWrapper._bb_lower * 100m) > 1.5m
                                    )
                            )
                            {
                                _nEnterMode.Value = 42;
                                form.Logger($"  ---> 42 : 볼밴 ▼하방 진입!!!! ");
                                form.Logger($"  {_lastPrice} < {emaWrapper._bb_lower} ");
                                form.Logger($"  {emaWrapper._close_2} > {emaWrapper._bb_lower_3} ");
                                form.Logger($"  {emaWrapper._close_3} > {emaWrapper._bb_lower_4} ");
                                form.Logger($"  {emaWrapper._close_4} > {emaWrapper._bb_lower_5} ");

                            }
                            else */
                            /*
                            if (
                                    bb_down_max >= 1.5m && bb_down_max - bb_down >= 0.4m
                                    //abovePer_bb_down_max >= 2.2m && abovePer_bb_down_max - abovePer_bb >= 0.4m
                                    && _lastPrice < emaWrapper._bb_lower

                                    && emaWrapper._close_2 > emaWrapper._bb_lower_3
                                    && emaWrapper._close_3 > emaWrapper._bb_lower_4
                                    && emaWrapper._close_4 > emaWrapper._bb_lower_5
                            )
                            {
                                _nEnterMode.Value = 81;
                                form.Logger($"  ---> 81 : 볼밴 ▲상방 진입!!!! ");                                
                            }
                            else if (
                                    bb_up_max >= 1.5m && bb_up_max - bb_up >= 0.4m
                                    //abovePer_bb_up_max >= 2.2m && abovePer_bb_up_max - abovePer_bb >= 0.4m
                                    && _lastPrice > emaWrapper._bb_upper

                                    && emaWrapper._close_2 < emaWrapper._bb_upper_3
                                    && emaWrapper._close_3 < emaWrapper._bb_upper_4
                                    && emaWrapper._close_4 < emaWrapper._bb_upper_5
                            )
                            {
                                _nEnterMode.Value = 82;
                                form.Logger($"  ---> 82 : 볼밴 ▼하방 진입!!!! ");
                            }
                            */

                            /*****
                            else if (
                                       now.Second > 50 &&
                                       emaWrapper._lastPrice_beforeCandle < _lastPrice && // 양봉
                                       _lastPrice > emaWrapper._ema50 &&
                                       _lastPrice > emaWrapper._highestHigh_15 &&
                                       (
                                             emaWrapper._ema15 < emaWrapper._ema50 && // 15선 < 50선
                                             emaWrapper._ema10 < emaWrapper._ema50 && // 15선 < 50선
                                             emaWrapper._ema5 < emaWrapper._ema50// 15선 < 50선

                                       )
                                       // 시간대

                                   )

                             {
                                 _nEnterMode.Value = 61;
                                 form.Logger($"  ---> 61 : 50선 ▲상방 진입!!!! ");
                             }

                            else if (
                                       now.Second > 50 &&
                                       emaWrapper._lastPrice_beforeCandle > _lastPrice && // 양봉
                                       _lastPrice < emaWrapper._ema50 &&
                                       _lastPrice < emaWrapper._lowestLow_15 &&
                                       (
                                             emaWrapper._ema15 > emaWrapper._ema50 && // 15선 < 50선
                                             emaWrapper._ema10 > emaWrapper._ema50 && // 15선 < 50선
                                             emaWrapper._ema5 > emaWrapper._ema50// 15선 < 50선

                                       )
                                   // 시간대

                                   )
                            {
                                _nEnterMode.Value = 62;
                                form.Logger($"  ---> 62 : 50선 ▼하방 진입!!!! ");
                                //form.Logger($"  {emaWrapper._lastPrice_beforeCandle} > {_lastPrice} < {emaWrapper._lowestLow_3}");
                                //form.Logger($" {emaWrapper._ema2_1} > {emaWrapper._ema50_1}  {emaWrapper._ema2} < {emaWrapper._ema50}");
                                //form.Logger($" {emaWrapper._lastPrice_beforeCandle} >= {emaWrapper._ema50}  {_lastPrice} < {emaWrapper._ema50}   {underPer_ema50} >= 4");
                                //form.Logger($" {emaWrapper._highestHigh_30} >= {emaWrapper._ema50 + (emaWrapper._ema50 * 0.07m)}");
                                //form.Logger($" {emaWrapper._belowEma50}", 0);

                            }
                          *****/

                            /*
                            if (
                                        emaWrapper._ema50 < emaWrapper._ema15 &&
                                        emaWrapper._ema15 < emaWrapper._ema10 &&
                                        emaWrapper._ema10 < emaWrapper._ema5 &&
                                        emaWrapper._lastPrice_beforeCandle > _lastPrice &&
                                        emaWrapper._lastPrice_beforeCandle >= emaWrapper._ema5 &&

                                         ((emaWrapper._ema5 - emaWrapper._ema50) / emaWrapper._ema50 * 100m) >= 2m &&
                                         ((emaWrapper._ema5 - emaWrapper._ema15) / emaWrapper._ema15 * 100m) >= 0.7m && // 추가

                                          _lastPrice < emaWrapper._ema5 &&
                                        (now.Second > 55 || (_lastPrice != 0 && (underPer_ema5 >= 0.3m))) &&
                                        //(now.Hour >= 7) // 새벽 제외!
                                        bong_size >= 1.0m
                                    )
                            {
                                _nEnterMode.Value = 5;
                                form.Logger($"  ---> 5 : 고점 5선 하방 진입!!!! {_lastPrice} < {emaWrapper._ema5}   {underPer_ema5}%");
                            }*/
                            /***
                            else if (emaWrapper._bLastPrice_beforeCandle == true &&
                               (
                                   (
                                       emaWrapper._ema50 < emaWrapper._ema15
                                      
                                       &&
                                       (
                                           Math.Abs((emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema50 * 100m) > 8.0m//9.0m //2.0m                                            
                                       )
                                       &&
                                       (
                                       
                                           //bong_size >= 2.3m
                                           //bong_size >= 2.0m
                                           bong_size >= 1.5m
                                          
                                       )
                                   )
                               )


                           //&& emaWrapper._bongsize_max < 5m // 최근 5% 이하 변동성시.                            
                           )
                            {
                                _nEnterMode.Value = 39;
                                form.Logger("  ---> 39 : 15봉 이탈 급락 진입!!!! ▲▲▲▲▲ 9");
                            }
                            ***/

                            /*
                            else if (emaWrapper._bLastPrice_beforeCandle == true &&
                                //_lastPrice < _lowestLow.Value_15 &&                           
                                (
                                    (
                                        //_mode_direction.Value == 1 && // 상방
                                        emaWrapper._ema50 < emaWrapper._ema15 &&
                                        (
                                             //_lastPrice <= emaWrapper._lowestLow * (1 - 0.003m)
                                             //||
                                             //_lastPrice < emaWrapper._ema5                                            

                                            (
                                                (DateTime.Now.Hour >= 6 || DateTime.Now.Hour < 18) // 낮장
                                                && _lastPrice <= emaWrapper._lowestLow_3 * (1 - 0.003m) // 3봉 아래
                                                && Math.Abs((emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema50 * 100m) > 4.5m //////3.0m //9.0m //2.0m
                                            )
                                            ||
                                            (
                                                (DateTime.Now.Hour < 6 || DateTime.Now.Hour >= 18)
                                                && _lastPrice <= emaWrapper._lowestLow_10 * (1 - 0.003m) // 10봉 아래
                                                && Math.Abs((emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema50 * 100m) > 4.5m /////4.0m //9.0m //2.0m
                                            )
                                        )
                                        

                                        //Math.Abs((emaWrapper._ema50 - emaWrapper._ema15) / emaWrapper._ema50 * 100m) > 2.5m &&
                                        //Math.Abs((emaWrapper._ema15 - emaWrapper._ema5) / emaWrapper._ema15 * 100m) > 2.5m
                                        //&& bong_size >= 1//1.5m // 2.5m//4m
                                        && 
                                        (
                                            (
                                                //bong_size >= 2.3m
                                                //bong_size >= 2.0m
                                                bong_size >= 1.5m &&
                                                (DateTime.Now.Hour < 6 || DateTime.Now.Hour >= 18)
                                            )
                                            ||
                                            _lastPrice <= emaWrapper._lowestLow_15 * (1 - 0.003m)
                                            ||
                                            (
                                                //bong_size >= 1.5m && // 기본
                                                bong_size >= 2.5m && // 기본
                                                (DateTime.Now.Hour < 6 || DateTime.Now.Hour >= 18)
                                            )                                            
                                        )                    
                                    )
                                ) 
                                
                                
                               && emaWrapper._bongsize_max < 5m // 최근 5% 이하 변동성시.
                            ///// && _ema15.Value < _ema50.Value // 1번 한번만 더 보고 아니면 롤백!
                            )
                            {
                                _nEnterMode.Value = 3;
                                form.Logger("  ---> 3 : 15봉 이탈 급락 진입!!!! ");
                            }
                           */
                            /*
                             else if (
                                  _lastPrice >= emaWrapper._highestHigh_15 * (1 + 0.011m)
                                  &&
                                  (

                                         // bong_size >= 0.7m
                                         //bong_size >= 2.5m
                                         //bong_size >= 4m
                                         bong_size >= 2.5m
                                  //bong_size >= 1.4m

                                  )
                                 && emaWrapper._bongsize_max < 5m // 최근 5% 이하 변동성시.                            

                                 /////&& emaWrapper._ema15 < emaWrapper._ema50
                                 //&& Math.Abs((emaWrapper._ema15 - emaWrapper._ema5) / emaWrapper._ema5 * 100m) <= 1m

                                 //&& DateTime.Now.Hour >= 4
                                 //&& (emaWrapper._ema5 - emaWrapper._ema15) / emaWrapper._ema5 * 100m < 1.5m
                                 //&& (emaWrapper._ema5 - emaWrapper._ema50) / emaWrapper._ema5 * 100m < 2.5m//3.0m
                                 && DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 16
                              )
                             {
                                 _nEnterMode.Value = 71;
                                 form.Logger("  ---> 71 : 15봉 이탈 상방 진입!!!!   *5봉 봉길이 : {emaWrapper._bongsize_max} < 5");
                             }
                             else if (//emaWrapper._bLastPrice_beforeCandle == true
                                      //_lastPrice < emaWrapper._ema50 * (1 - 0.003m) // 50선 손절 위해 좀 더 아래에서 잡기.
                                      //_lastPrice <= emaWrapper._lowestLow_15 * (1 - 0.003m)
                                 _lastPrice <= emaWrapper._lowestLow_15 * (1 - 0.011m)
                                 //_lastPrice <= emaWrapper._lowestLow_15 

                                 //&& bong_size >= 2.5m //1.5m // 2.0m // 3.0m
                                 //&& bong_size >= 1.5m
                                 &&
                                 (

                                         //bong_size >= 0.7m
                                         //bong_size >= 2.5m
                                         //bong_size >= 4m
                                         bong_size >= 2.5m
                                 //bong_size >= 1.4m

                                 )
                                && emaWrapper._bongsize_max < 5m // 최근 5% 이하 변동성시.                            

                                /////&& emaWrapper._ema15 < emaWrapper._ema50
                                //&& Math.Abs((emaWrapper._ema15 - emaWrapper._ema5) / emaWrapper._ema5 * 100m) <= 1m

                                //&& DateTime.Now.Hour >= 4
                                //&& (emaWrapper._ema15 - emaWrapper._ema5) / emaWrapper._ema5 * 100m < 1.5m
                                //&& (emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema5 * 100m < 2.5m//3.0m
                                && DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 16
                             )
                             {
                                 _nEnterMode.Value = 72;
                                 form.Logger("  ---> 72 : 15봉 이탈 하방 진입!!!!   *5봉 봉길이 : {emaWrapper._bongsize_max} < 5");
                             }
                             */
                            /*****
                            else if (
                                 emaWrapper._lastPrice_beforeCandle > emaWrapper._ema15
                                &&
                                (
                                    (
                                        _lastPrice < emaWrapper._ema15                                                               
                                        && DateTime.Now.Second > 55
                                    )
                                    ||
                                    (
                                        _lastPrice < emaWrapper._ema15
                                        && Math.Abs((_lastPrice - emaWrapper._ema15) / emaWrapper._ema15 * 100m) > 0.6m
                                    )
                                )
                               && emaWrapper._ema15 > emaWrapper._ema50
                               /////&& Math.Abs((emaWrapper._ema50 - emaWrapper._ema5) / emaWrapper._ema50 * 100m) > 4.0m
                               && Math.Abs((emaWrapper._ema50 - emaWrapper._ema15) / emaWrapper._ema50 * 100m) >= 2.5m
                               /////&& Math.Abs((emaWrapper._ema15 - emaWrapper._ema5) / emaWrapper._ema15 * 100m) > 2.0m

                               && DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 18
                               //////&& _lastPrice <= emaWrapper._highestHigh_3 * (1 - 0.003m)
                            )
                            {
                                _nEnterMode.Value = 5;
                                form.Logger("  ---> 5 : 5선 이탈 진입!!!! ");
                            }
                            *****/
                            /*
                           else if (_bLastPrice_beforeCandle.Value == true &&
                               now.Second < 8 &&
                               _lastPrice < _lowestLow.Value &&
                               (
                                   (
                                       _lastPrice > _ema50.Value && 
                                       bong_size > 8m
                                   )
                                   ||
                                   (
                                       _lastPrice <= _ema50.Value &&
                                       bong_size > 4m
                                   )
                               )
                           )
                           {
                               _nEnterMode.Value = 4;
                               form.Logger("  ---> 4 : 8초내 급락 진입!!!! ");
                           }
                            */
                            /*
                            else if (
                                    emaWrapper._ema50 < emaWrapper._ema10 &&
                                    emaWrapper._ema10 > emaWrapper._ema15 &&
                                    emaWrapper._lastPrice_beforeCandle > emaWrapper._ema15 &&
                                    _lastPrice <= emaWrapper._lowestLow * (1 - 0.003m) &&

                                    _lastPrice < emaWrapper._ema15 &&
                                     ((emaWrapper._ema15 - emaWrapper._ema50) / emaWrapper._ema50 * 100m) >= 4m &&

                                    (now.Second > 57 || (_lastPrice != 0 && (underPer_ema15 >= 1.0m)))

                                    && (now.Hour > 6) // 새벽 제외!
                                )
                            {
                                _nEnterMode.Value = 1;
                                form.Logger("  ---> 1 : 상방 15선 > 50선 3% 이상에 진입!!!! ");
                            }
                            */
                            /*
                            else if ( // 50선 위에서는 10 > 15 배열 안보기
                                     emaWrapper._ema50 < emaWrapper._ema10 &&
                                    emaWrapper._ema10 > emaWrapper._ema15 &&
                                    emaWrapper._lastPrice_beforeCandle > emaWrapper._ema15 && // || ((_ema50.Value -_lastPrice_beforeCandle .Value) / _ema50.Value)  < 0.3m) &&
                                                                        //bong_size >= 0.4m && 
                                    _lastPrice < emaWrapper._ema15 &&
                                    Math.Abs((emaWrapper._ema50 - emaWrapper._ema15) / emaWrapper._ema50 * 100m) > 1.5m &&


                                    //(now.Second > 57 || (lastPrice != 0 && (underPer_ema15.Value >= 0.7m)))
                                    (now.Second > 57 || (_lastPrice != 0 && (underPer_ema15 >= 1.0m)))
                                    //now.Hour >= 7 && (now.Hour < 18 || (now.Hour == 18 && now.Minute <= 30))
                                    && (now.Hour >= 7 && now.Hour < 23)
                                )
                            {
                                _nEnterMode.Value = 11;
                                form.Logger("  ---> 11 : 상방");
                            }
                            */

                            /*
                           else if( // 50선 아래에서는 10 > 15 여야 함.
                               _ema50.Value > _ema10.Value &&
                               (
                                   //(now.Hour >= 7 && now.Hour < 17)  // 낮장에서는 조건 해제하고 많이 진입
                                   //||
                                   _ema10.Value > _ema15.Value

                               )
                               &&
                               _lastPrice_beforeCandle .Value > _ema15.Value && // || ((_ema50.Value - _lastPrice_beforeCandle .Value) / _ema50.Value) < 0.3m) &&
                                                                   //bong_size >= 0.4m &&
                               _lastPrice < _ema15.Value &&

                               Math.Abs((_ema50.Value - _ema15.Value) / _ema50.Value * 100m) < 2m && // 50선 근접 -> 다시 살림 2025-09-13 21:45

                               //(now.Second > 57 || (lastPrice != 0 && (underPer_ema15.Value >= 0.7m)))
                               (now.Second > 57 || (_lastPrice != 0 && (underPer_ema15.Value >= 1.0m)))

                           //   && (now.Hour >= 7 && now.Hour < 17)
                           )
                           {
                               _nEnterMode.Value = 22;
                               form.Logger("  ---> 22 : 하방");
                           }
                          */


                        }



                        // 진입시작
                        if (_nEnterMode.Value > 0)
                        {


                            /*
                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  *** 10이평 하방 돌파...! {lastPrice} < {_ema10.Value}");

                            // 전체 포지션 조회
                            var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                               productType: BitgetProductTypeV2.UsdtFutures,
                               marginAsset: "USDT"
                            );

                            if (positions.Success && positions != null && positions.Data.Count() == 0)
                            {
                            */


                            // OPEN : 미체결 주문 조회
                            var openOrders = await client.FuturesApiV2.Trading.GetOpenOrdersAsync(
                                productType: BitgetProductTypeV2.UsdtFutures,
                                symbol: threadSymbol.Value
                            );

                            if (openOrders.Success && openOrders != null && openOrders.Data.Orders.Count() == 0)
                            {

                                // 로깅만 빼고 실시간 가격 데이터는 필요.
                                _bEnterStart = true;
                                await setProgress(this, id, false);

                                form.Logger(" ");
                                if (_nEnterMode.Value == 61 || _nEnterMode.Value == 41 || _nEnterMode.Value == 81 || _nEnterMode.Value == 71 || _nEnterMode.Value == 91)
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  >>> ▲롱 진입 !!");
                                else
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  >>> ▼숏 진입 !!");
                                //form.Logger($"{_ema50.Value} {_ema15.Value} {_ema10.Value} {_lastPrice_beforeCandle .Value} {lastPrice} :: {now.Hour} {now.Minute}");

                                break;
                            }
                            else
                            {
                                form.Logger(" ");
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  ■■■ {threadSymbol.Value}  이미 다른 미체결(OPEN) 주문 진행 중.... (3분 대기)", 0);
                                string symbol_spell = string.Join(" ", threadSymbol.Value.ToUpper().ToCharArray());
                                speech("띵동, 이미 다른 오픈포지션 진행 중  " + symbol_spell);
                                //await Task.Delay(30000); // 30초 간격
                                await Task.Delay(180000);
                                form.Logger($"  ●●● 실시간 진입타점 모니터링 중 . . .");
                                form.Logger(" ");
                                await setProgress(this, id, true);
                                continue;
                            }

                            /*


                        }
                        else
                        {
                            form.Logger(" ");
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  ■■■ {threadSymbol.Value}  이미 다른 포지션 진행 중....  (3분 대기)", 0);
                            string symbol_spell = string.Join(" ", threadSymbol.Value.ToUpper().ToCharArray());
                            speech("띵동, 이미 다른 포지션 진행 중  " + symbol_spell);
                            //await Task.Delay(30000); // 30초 간격
                            await Task.Delay(180000);
                            form.Logger($"  ●●● 실시간 진입타점 모니터링 중 . . .");
                            form.Logger(" ");
                            await setProgress(this, id, true);
                            continue;
                        }

                        */

                        }
                    }

                    if (restart == true)
                        continue;

                    if (emaWrapper._ema15 > 0m && emaWrapper._ema50 > 0m)
                    {
                        if (emaWrapper._ema50 < emaWrapper._ema15)
                        {
                            _mode_direction.Value = 1;
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ★ ↑↑↑ 상방추세  {emaWrapper._ema50} < {emaWrapper._ema10}");
                        }
                        else
                        {
                            _mode_direction.Value = 2;
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ★ ↓↓↓ 하방추세  {emaWrapper._ema50} > {emaWrapper._ema10}");
                        }
                    }





                    ///////////////////////////////
                    // 실전 진입시작, 투입값 설정, 실투진입
                    ///////////////////////////////

                    if (_bRealSuccess == false)
                    {
                        /*
                        if (_nEnterMode.Value == 3)
                        {
                            if (_mode_direction.Value == 1 && (DateTime.Now.Hour < 2 || DateTime.Now.Hour >= 4))
                            {
                                _bRealTrade = true;
                                _nLeverage = 5;
                                //_investMoney = 215;

                                await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);

                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ◆◆◆◆◆◆◆◆◆◆ 실전 투입!!  3:1 (2:00 ~ 04:00 제외)  (x{_nLeverage}, {_investMoney})", 0);
                            }                           
                        }
                        else if (_nEnterMode.Value == 62)
                        {
                            if (_mode_direction.Value == 1 && (DateTime.Now.Hour >= 13 && DateTime.Now.Hour < 18))
                            {
                                _bRealTrade = true;
                                _nLeverage = 5;
                                //_investMoney = 215;

                                await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);

                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ◆◆◆◆◆◆◆◆◆◆ 실전 투입!!  62 (13:00 ~ 18:00)  (x{_nLeverage}, {_investMoney})", 0);
                            }
                        }
                       */
                       /*
                        if (
                                (
                                    (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 6 && (_nEnterMode.Value == 71 || _nEnterMode.Value == 72))
                                    ||
                                    (_nEnterMode.Value == 11 || _nEnterMode.Value == 12)
                                )
                        )
                        {
                            _bRealTrade = false;

                            _nLeverage = 1;
                            _investMoney = 7;

                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   [{_nEnterMode.Value}] 레버리지 설정 시작.    ({_investMoney}, x{_nLeverage}, {_investMoney})", 0);
                            await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   [{_nEnterMode.Value}] 레버리지 설정 끝.    ({_investMoney}, x{_nLeverage}, {_investMoney})", 0);

                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ◆◆◆◆◆◆◆◆◆◆ 실전 투입!!    (x{_nLeverage}, {_investMoney})", 0);
                        }
                        else
                        */
                        {
                            _bRealTrade = true;

                            if ((_nEnterMode.Value == 91 || _nEnterMode.Value == 92) && (DateTime.Now.Hour >= 11 && DateTime.Now.Hour < 14) && globalWrapper._detectedPercent >= 90)
                            {
                                //_investMoney_margin = 7; // 증거금 x1
                                
                                _nLeverage = 1;
                                _investMoney = _investMoney_margin * _nLeverage; // 증거금 * 레버리지 배율 = 실제 가상의 계산된 투입금 결과치!!
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 설정 : _investMoney_margin = {_investMoney_margin}, _nLeverage = {_nLeverage} => _investMoney = {_investMoney}");
                                await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);
                            }

                            //_nLeverage = 5;
                            //_investMoney = 215;

                            //await setLeverage(this, apiKey, apiSecret, passphrase, _nLeverage);

                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ◆◆◆◆◆◆◆◆◆◆ 실전 투입!!    (x{_nLeverage}, {_investMoney})", 0);
                        }
                    }




                    // 1차 진입

                    if (_minOrderQuantity.Value > _investMoney)
                        _investMoney = _minOrderQuantity.Value;

                    decimal quantity = Math.Ceiling(_investMoney / _lastPrice);
                    if (quantity < 5)
                        quantity = 5;

                    if (_lastPrice > 6)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] 최대주문 금액 초과 : 가격이 주문금액보다 큽니다.");
                        continue;
                    }


                    // 1차 진입
                    //bool bCancel_success = false;
                    BitgetFuturesOrder filledOrder = await entryOrder(this, client, _nEnterMode.Value, quantity, _tickSize, globalWrapper);
                    if (filledOrder == null) // 주문 실패시
                    {
                        await Task.Delay(300000); // 5분 간격
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 5분 대기...");
                        continue;
                    }

                    decimal filledPrice = (decimal)filledOrder.AveragePrice;
                    _totalAveragePrice = filledPrice;


                    //if (bCancel_success == true)
                    //continue;

                    //if (bCancel_success == false) // 취소되지 않고 체결까지 완료되었을시!
                    {
                        //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 10초 대기 후 1차 Short CLOSE 주문...");
                        //Task.Delay(10000); // 10초 대기
                        await Task.Delay(500);



                        //////////////////////////////////////
                        // 1차 Short CLOSE 주문
                        decimal targetPrice = 0m;
                        var side = OrderSide.Buy; // 숏청산 기본 롱잡기!

                        if (globalWrapper._side == OrderSide.Buy)
                            side = OrderSide.Sell;

                        //if (_mode_direction == 1) // 상방
                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.007m);
                        //else

                        //if (_mode_direction == 1)
                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.006m);
                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.01m);
                        //////targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.02m);
                        //else
                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.01m);
                        /////targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.02m);


                        //if (now.Hour >= 7 && now.Hour < 17)
                        if (emaWrapper._ema15 > emaWrapper._ema50 && ((emaWrapper._ema15 - emaWrapper._ema50) / emaWrapper._ema50 * 100 > 8m))
                            targetPrice = filledPrice - (filledPrice * 0.03m);
                        //else if (_ema15.Value > _ema50.Value && ((_ema15.Value - _ema50.Value) / _ema50.Value * 100) >= 2)
                        //targetPrice = filledPrice - (filledPrice * 0.012m);
                        else
                            targetPrice = filledPrice - (filledPrice * 0.008m);


                        if (DateTime.Now.Hour < 7)
                            //targetPrice = filledPrice - (filledPrice * 0.025m);
                            //targetPrice = filledPrice - (filledPrice * 0.008m);
                            targetPrice = filledPrice - (filledPrice * 0.005m);
                        else if (DateTime.Now.Hour >= 16)
                            targetPrice = filledPrice - (filledPrice * 0.008m);

                        if (_nEnterMode.Value == 1) // 고점 하방
                                                    //targetPrice = filledPrice - (filledPrice * 0.02m);
                                                    //targetPrice = filledPrice - (filledPrice * 0.02m);
                            targetPrice = filledPrice - (filledPrice * 0.012m); // 확정!
                        else if (_nEnterMode.Value == 3) // 급락시
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.05m);
                            //targetPrice = filledPrice - (filledPrice * 0.04m);                        
                            //targetPrice = filledPrice - (filledPrice * 0.03m);


                            // 2025-09-21 by whhwang, 최적화
                            if (_mode_direction.Value == 1) // 상방
                            {
                                if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 16) // 15 ??
                                {
                                    targetPrice = filledPrice - (filledPrice * 0.012m);
                                    //targetPrice = filledPrice - (filledPrice * 0.025m);
                                }
                                else
                                {
                                    targetPrice = filledPrice - (filledPrice * 0.07m);
                                }


                                /*
                                if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 18)
                                {
                                    targetPrice = filledPrice - (filledPrice * 0.011m);
                                }
                                */
                            }
                            else
                            {
                                targetPrice = filledPrice - (filledPrice * 0.012m);

                                if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 23) // 14-> 15? => 24
                                {
                                    ///////targetPrice = filledPrice - (filledPrice * 0.005m);
                                    //targetPrice = filledPrice - (filledPrice * 0.011m);
                                    //targetPrice = filledPrice - (filledPrice * 0.015m);
                                    targetPrice = filledPrice - (filledPrice * 0.021m);
                                }
                                else if (DateTime.Now.Hour < 6)
                                {
                                    targetPrice = filledPrice - (filledPrice * 0.04m);
                                }

                            }
                        }
                        else if (_nEnterMode.Value == 39) // 급락시
                        {
                            targetPrice = filledPrice - (filledPrice * 0.07m);
                        }                       
                        else if (_nEnterMode.Value == 61) // 상방
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            //targetPrice = filledPrice + (filledPrice * 0.07m);
                            //targetPrice = filledPrice + (filledPrice * 0.011m); // 일부러 이렇게?
                            //targetPrice = filledPrice + (filledPrice * 0.11m); // 일부러 이렇게?
                            targetPrice = filledPrice + (filledPrice * 0.012m);
                        }
                        else if (_nEnterMode.Value == 62) // 하방
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.07m);
                            //targetPrice = filledPrice - (filledPrice * 0.11m); // 이게 맞나? ㅎㅎ
                            targetPrice = filledPrice - (filledPrice * 0.012m);
                        }
                        else if (_nEnterMode.Value == 71) // 30봉 하방
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            targetPrice = filledPrice + (filledPrice * 0.12m);
                        }
                        else if (_nEnterMode.Value == 72) // 30봉 하방
                        {
                            targetPrice = filledPrice - (filledPrice * 0.12m);
                        }
                        else if (_nEnterMode.Value == 5)
                        {
                            targetPrice = filledPrice - (filledPrice * 0.07m);
                            //targetPrice = filledPrice - (filledPrice * 0.011m);
                        }
                        else if (_nEnterMode.Value == 41) // 상방진입 익절선!
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            //targetPrice = filledPrice + (filledPrice * 0.21m);
                            targetPrice = emaWrapper._bb_upper;
                        }
                        else if (_nEnterMode.Value == 42) // 하방진입
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.21m);
                            targetPrice = emaWrapper._bb_lower;
                        }
                        else if (_nEnterMode.Value == 91) // 상방진입 익절선!/
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            //targetPrice = filledPrice + (filledPrice * 0.03m);
                            //targetPrice = emaWrapper._bb_upper - ((emaWrapper._bb_upper - emaWrapper._ema50) / 2);
                            targetPrice = emaWrapper._bb_upper;

                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] targetPrice = {targetPrice}   upper = {emaWrapper._bb_upper}, ema50 = {emaWrapper._ema50}  gap = {emaWrapper._bb_upper - emaWrapper._ema50}");
                        }
                        else if (_nEnterMode.Value == 92) // 하방진입
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.03m);
                            //targetPrice = emaWrapper._bb_lower + ((emaWrapper._ema50 - emaWrapper._bb_lower) / 2);
                            targetPrice = emaWrapper._bb_lower;


                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] targetPrice = {targetPrice}   ema50 = {emaWrapper._ema50}, lower = {emaWrapper._bb_lower}  gap = {emaWrapper._ema50 - emaWrapper._bb_lower}");
                        }
                        else if (_nEnterMode.Value == 81) // 상방진입 익절선!
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            //targetPrice = filledPrice + (filledPrice * 0.016m);
                            //targetPrice = filledPrice + (filledPrice * 0.07m);
                            targetPrice = filledPrice + (filledPrice * 0.019m);
                        }
                        else if (_nEnterMode.Value == 82) // 하방진입
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.016m);
                            //targetPrice = filledPrice - (filledPrice * 0.07m);
                            targetPrice = filledPrice - (filledPrice * 0.019m);
                        }
                        else if (_nEnterMode.Value == 11) // 상방진입 익절선!/
                        {
                            side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                            //targetPrice = filledPrice + (filledPrice * 0.03m);
                            //targetPrice = emaWrapper._bb_upper - ((emaWrapper._bb_upper - emaWrapper._ema50) / 2);
                            targetPrice = emaWrapper._bb_upper;

                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] targetPrice = {targetPrice}   upper = {emaWrapper._bb_upper}, ema50 = {emaWrapper._ema50}  gap = {emaWrapper._bb_upper - emaWrapper._ema50}");
                        }
                        else if (_nEnterMode.Value == 12) // 하방진입
                        {
                            //targetPrice = filledPrice - (filledPrice * 0.03m);
                            //targetPrice = emaWrapper._bb_lower + ((emaWrapper._ema50 - emaWrapper._bb_lower) / 2);
                            targetPrice = emaWrapper._bb_lower;


                            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] targetPrice = {targetPrice}   ema50 = {emaWrapper._ema50}, lower = {emaWrapper._bb_lower}  gap = {emaWrapper._ema50 - emaWrapper._bb_lower}");
                        }

                        // 익절선 설정

                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.015m);
                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.004m);

                        //targetPrice = filledOrder.Price.Value - (filledOrder.Price.Value * 0.02m);

                        decimal closePrice = Math.Floor(targetPrice / _tickSize) * _tickSize;
                        // 소수점 자리수 제한 (_tickSize에 맞춤)
                        _decimalPlaces.Value = BitConverter.GetBytes(decimal.GetBits(_tickSize)[3])[2];
                        //closePrice = Math.Round(closePrice, _decimalPlaces.Value, MidpointRounding.ToZero);
                        closePrice = Math.Truncate(closePrice * (decimal)Math.Pow(10, _decimalPlaces.Value)) / (decimal)Math.Pow(10, _decimalPlaces.Value);

                        //form.Logger($"체결가: {filledOrder.Price.Value}, 1% 낮춘 가격: {targetPrice}, _tickSize 보정 후: {closePrice}");

                        /////await setTrailingStopopAsync(client, closePrice, filledOrder.Quantity);
                        ///
                        // close 주문 (지정가)
                        var closeOrder2 = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                            productType: BitgetProductTypeV2.UsdtFutures,
                            symbol: threadSymbol.Value,
                            marginAsset: "USDT",
                            //side: OrderSide.Buy,                // ✅ 숏 청산 → 반대 방향 매수
                            side: side,
                            type: OrderType.Limit,              // 지정가
                            marginMode: MarginMode.IsolatedMargin,
                            quantity: filledOrder.Quantity,     // 진입 수량만큼 청산
                            price: closePrice,
                            reduceOnly: true                    // ✅ 청산 주문임을 명시
                        );

                        if (closeOrder2.Success)
                        {
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ◆ 1차 {side.ToString()} 청산(지정가) 주문 성공. {closePrice}, {filledOrder.Quantity}");
                        }
                        else
                        {
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  1차 {side.ToString()} CLOSE 주문 실패: {closeOrder2.Error}", 0);
                        }






                        /*

                   var closeOrder = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                           productType: BitgetProductTypeV2.UsdtFutures,
                           symbol: threadSymbol.Value,
                           marginAsset: "USDT",
                           side: OrderSide.Sell,
                           type: OrderType.Limit,
                           marginMode: MarginMode.IsolatedMargin,
                           quantity: filledOrder.Quantity,
                           price: closePrice,
                           tradeSide: TradeSide.Close
                       );

                    if (roi_current > 1.0m) // 시장가
                    {
                        closeOrder = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                            productType: BitgetProductTypeV2.UsdtFutures,
                            symbol: threadSymbol.Value,
                            marginAsset: "USDT",
                            side: OrderSide.Sell,               // 숏 포지션 청산이라면 Sell
                            type: OrderType.Market,             // 시장가 주문
                            marginMode: MarginMode.IsolatedMargin,
                            quantity: filledOrder.Quantity,     // 청산 수량
                            tradeSide: TradeSide.Close          // 청산 주문
                        );
                    }

                    if (closeOrder.Success)
                    {
                        totalQty = filledOrder.Quantity; // 1차만 할 때.
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ◆ 1차 Short CLOSE 주문 성공. {closePrice}, {filledOrder.Quantity}");
                    }
                    else
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 1차 Short CLOSE 주문 실패: {closeOrder.Error}");

                    */


                        if (_bRealTrade == true)
                        {
                            try
                            {
                                speech("실전모드 진입완료.");
                                //await Task.Delay(30000); // 30초 간격
                            }
                            catch (Exception ex)
                            {
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     speech error : {ex.Message}", 0);
                            }
                        }



                        string orderId_current = "";
                        bool bReturn = false;
                        //if (_mode_direction == 1) // 상방
                        if (true)
                        {

                            if (_nEnterMode.Value != 91 && _nEnterMode.Value != 92 && _nEnterMode.Value != 71 && _nEnterMode.Value != 72)
                            {
                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]                 StopLoss 설정 대기 다음 분의 20초 ...");
                                await setProgress(this, id, true);
                                form.IncrementProgress(form, id, 2);

                                // 적용 딜레이
                                int enterMin = DateTime.Now.Minute;

                                decimal k = 0;
                                bReturn = false;

                                while (true)
                                {
                                    k++;
                                    await Task.Delay(1000); // 초 대기



                                    if (DateTime.Now.Minute != enterMin && DateTime.Now.Second > 20)
                                    {
                                        break;
                                    }

                                    bReturn = await checkPosition(this, client, id, "", threadSymbol.Value);
                                    if (bReturn == true)
                                    {
                                        break;
                                    }

                                    // 청산조건 체크
                                    _strMemo = await CheckExitCondition(this, client, _lastPrice, _roi_current, _roi_current_max, _strMemo, emaWrapper, globalWrapper, _nEnterMode.Value);
                                }
                            }



                            //await Task.Delay(5000); // 5초 대기
                            await Task.Delay(10000); // 10초 대기
                            //if (bReturn == false && (globalWrapper._detectedPercent >= 90 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour <= 8) || (_nEnterMode.Value != 91 && _nEnterMode.Value != 92))) // 2차 진입시 1차는 배제
                            if (bReturn == false && (_nEnterMode.Value != 41 && _nEnterMode.Value != 42) ) // 2차 진입시 1차는 배제
                            //if (bReturn == false)
                            {
                                // 포착시 100% 등락률 이상이면 1차진입하고 스탑로스 걸기! -5%

                                // 스탑로스 설정                    
                                //await SetStopLossAsync(client, filledPrice, filledOrder.QuantityFilled); // 1차 진입 체결가격
                                await setStopLoss(this, apiKey, apiSecret, passphrase, _lastPrice, filledPrice, filledOrder.QuantityFilled, _tickSize, emaWrapper, _totalAveragePrice, globalWrapper);

                                /////orderId_current = closeOrder.Data.OrderId;
                                ///

                            }
                            form.Logger($"  ▶▶▶▶▶ 매매 중 . . . ");

                            // 매매 중
                            bReturn = false;
                            await setProgress(this, id, true);
                            while (true) // 청산 체크 중.................................
                            {


                                now = new DateTime();
                                if (_bEma10_underEma15.Value == false && emaWrapper._ema10 < emaWrapper._ema15)
                                {
                                    _bEma10_underEma15.Value = true;
                                }

                                /*
                                // 현재 수익률
                                decimal roi = 0;
                                // 포지션 조회
                                var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures,
                                    marginAsset: "USDT"
                                );


                                if (positions.Data != null)
                                {
                                    foreach (var pos in positions.Data)
                                    {
                                        if (pos.AverageOpenPrice > 0 && _lastPrice > 0)
                                        {
                                            roi = (pos.AverageOpenPrice - _lastPrice) / pos.AverageOpenPrice * 100m;
                                        }
                                        break;
                                    }
                                }
                                */
                                _strMemo = await CheckExitCondition(this, client, _lastPrice, _roi_current, _roi_current_max, _strMemo, emaWrapper, globalWrapper, _nEnterMode.Value);

                                
                                // 2차 진입  
                                
                                if (globalWrapper._detectedPercent < 90 &&                                    
                                    DateTime.Now - globalWrapper._timeContract >= TimeSpan.FromMinutes(3) &&
                                    /////(_nEnterMode.Value == 91 || _nEnterMode.Value == 92)
                                    /////&& DateTime.Now.Hour > 10
                                    (_nEnterMode.Value == 41 || _nEnterMode.Value == 42)
                                )
                                {
                                    //if (_roi_current < -5 &&
                                    if (_roi_current <= -4 &&
                                    //if (_roi_current < -1 &&
                                        (
                                            (
                                                _nEnterMode.Value == 92 &&
                                                emaWrapper._lastPrice_beforeCandle >= emaWrapper._bb_upper && // 다시 추가
                                                _lastPrice < emaWrapper._bb_upper
                                                &&
                                                (
                                                    emaWrapper._ema5 >= emaWrapper._bb_upper // 5선이 볼밴상단 위
                                                    ||
                                                    emaWrapper._ema5_2 >= emaWrapper._bb_upper_2
                                                    ||
                                                    emaWrapper._ema5_3 >= emaWrapper._bb_upper_3
                                                )
                                                &&
                                                DateTime.Now.Second >= 50 // 추가
                                            )
                                            ||
                                            (
                                                _nEnterMode.Value == 91 &&
                                                emaWrapper._lastPrice_beforeCandle <= emaWrapper._bb_lower && // 다시 추가
                                                _lastPrice > emaWrapper._bb_lower                                                 
                                                &&
                                                (
                                                    emaWrapper._ema5 <= emaWrapper._bb_lower
                                                    ||
                                                    emaWrapper._ema5_2 <= emaWrapper._bb_lower_2
                                                    ||
                                                    emaWrapper._ema5_3 <= emaWrapper._bb_lower_3
                                                )
                                                &&
                                                DateTime.Now.Second >= 50 // 추가
                                            )
                                            ||
                                            _roi_current < -15m //-17m
                                        )                                        
                                        &&
                                        globalWrapper._entryStep < 2 // 5 // 2차까지 진입
                                    )
                                    {
                                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {globalWrapper._entryStep+1}차 진입 : 주문");
                                        //speech($"{globalWrapper._entryStep+1}차 진입");
                                        speech("이차 진입");

                                        // 2차는 2배수.
                                        filledOrder = await entryOrder(this, client, _nEnterMode.Value, quantity * 2, _tickSize, globalWrapper);

                                        if (filledOrder == null) // 주문 실패시
                                        {
                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 5분 대기...");
                                            continue;
                                        }

                                        globalWrapper._entryStep++; // 1, 2, 3, 
                                        filledPrice = (decimal)filledOrder.AveragePrice; // 2차 진입분의 가격!


                                        await Task.Delay(500);



                                        // 포지션 조회
                                        var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                                            productType: BitgetProductTypeV2.UsdtFutures,
                                            marginAsset: "USDT"
                                        );

                                        if (!positions.Success)
                                        {
                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] <2차 진입> 포지션 조회 실패: {positions.Error}", 0);
                                            continue;
                                        }


                                        // 특정 코인 포지션 확인
                                        var targetPosition = positions.Data.FirstOrDefault(p =>
                                        p.Symbol.Equals(threadSymbol.Value, StringComparison.OrdinalIgnoreCase));

                                        if (targetPosition == null)
                                        {
                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{threadSymbol.Value}] 현재 보유 중인 포지션이 없습니다.", 0);
                                        }
                                        else
                                        {
                                            _totalAveragePrice = targetPosition.AverageOpenPrice;    // 전체!! 평균 진입가                                               
                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{threadSymbol.Value}]  *보유수량: {targetPosition.Total}, 평균진입가: {targetPosition.AverageOpenPrice}, 총 개수 : {targetPosition.Total}");


                                            await Task.Delay(200);
                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 기존 청산-지정가 주문 취소.");

                                            // OPEN : 미체결 주문 조회
                                            var openOrders = await client.FuturesApiV2.Trading.GetOpenOrdersAsync(
                                                productType: BitgetProductTypeV2.UsdtFutures,
                                                symbol: threadSymbol.Value
                                            );

                                            if (!openOrders.Success)
                                            {
                                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN : 미체결 주문조회 실패: {openOrders.Error}", 0);
                                                continue;
                                            }
                                            else
                                            {
                                                // 실제 주문 리스트 확인 후 취소
                                                if (openOrders.Data != null)
                                                {
                                                    foreach (var o in openOrders.Data.Orders)
                                                    {
                                                        var cancelResult = await client.FuturesApiV2.Trading.CancelOrderAsync(
                                                            productType: BitgetProductTypeV2.UsdtFutures,
                                                            symbol: threadSymbol.Value,
                                                            orderId: o.OrderId
                                                        );

                                                        if (cancelResult.Success)
                                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN :주문취소 성공, {o.OrderId}");
                                                        else
                                                        {
                                                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN :주문취소 실패... {o.OrderId} : {cancelResult.Error}");

                                                            await Task.Delay(2000);
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }

                                            await Task.Delay(200);


                                            // 익절선 설정
                                            if (_nEnterMode.Value == 91) // 상방진입 익절선!/
                                            {
                                                side = OrderSide.Sell; // 롱일 때 셀로 청산!                                                   
                                                
                                                targetPrice = _totalAveragePrice + (_totalAveragePrice * 0.02m);
                                                
                                                //if (globalWrapper._entryStep >= 2)//3)
                                               // {
                                               //     targetPrice = _totalAveragePrice + (_totalAveragePrice * 0.015m); // 3
                                                //}
                                                
                                                //targetPrice = emaWrapper._ema50;
                                            }
                                            else if (_nEnterMode.Value == 92) // 하방진입
                                            {
                                                
                                                targetPrice = _totalAveragePrice - (_totalAveragePrice * 0.02m);
                                             
                                                //if (globalWrapper._entryStep >= 2)//3)
                                               // {
                                                //    targetPrice = _totalAveragePrice - (_totalAveragePrice * 0.015m);
                                               // }
                                               
                                                //targetPrice = emaWrapper._ema50;                                                                                       
                                            }

                                            closePrice = Math.Floor(targetPrice / _tickSize) * _tickSize;
                                            // 소수점 자리수 제한 (_tickSize에 맞춤)
                                            _decimalPlaces.Value = BitConverter.GetBytes(decimal.GetBits(_tickSize)[3])[2];
                                            //closePrice = Math.Round(closePrice, _decimalPlaces.Value, MidpointRounding.ToZero);
                                            closePrice = Math.Truncate(closePrice * (decimal)Math.Pow(10, _decimalPlaces.Value)) / (decimal)Math.Pow(10, _decimalPlaces.Value);

                                            //form.Logger($"체결가: {filledOrder.Price.Value}, 1% 낮춘 가격: {targetPrice}, _tickSize 보정 후: {closePrice}");

                                            /////await setTrailingStopopAsync(client, closePrice, filledOrder.Quantity);
                                            ///
                                            // close 주문 (지정가)
                                            closeOrder2 = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                                                productType: BitgetProductTypeV2.UsdtFutures,
                                                symbol: threadSymbol.Value,
                                                marginAsset: "USDT",
                                                //side: OrderSide.Buy,                // ✅ 숏 청산 → 반대 방향 매수
                                                side: side,
                                                type: OrderType.Limit,              // 지정가
                                                marginMode: MarginMode.IsolatedMargin,
                                                quantity: targetPosition.Total,     // 진입 수량만큼 청산
                                                price: closePrice,
                                                reduceOnly: true                    // ✅ 청산 주문임을 명시
                                            );

                                            if (closeOrder2.Success)
                                            {
                                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ◆ {globalWrapper._entryStep}차 {side.ToString()} 청산(지정가) 주문 성공. {closePrice}, {targetPosition.Total}");
                                            }
                                            else
                                            {
                                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  {globalWrapper._entryStep}차 {side.ToString()} CLOSE 주문 실패: {closeOrder2.Error}", 0);

                                                await Task.Delay(2000);
                                                continue;
                                            }



                                            // 스탑로스 설정
                                            
                                            if (globalWrapper._entryStep >= 2)//5)
                                            {
                                                // 2차 진입분의 -10% 청산
                                                await setStopLoss(this, apiKey, apiSecret, passphrase, _lastPrice, filledPrice, targetPosition.Total, _tickSize, emaWrapper, filledPrice, globalWrapper);
                                            }
                                            

                                            //////////////////////////////////////////////////////////////////////////////////
                                            await Task.Delay(60000); // 5초 대기, 다음 중복 진입 바로 들어가는 경우 회피용.
                                        }
                                    }
                                }
                                


                                // 종료 체크
                                await Task.Delay(500);
                                bReturn = await checkPosition(this, client, id, orderId_current, threadSymbol.Value);
                                if (bReturn == true)
                                {
                                    await setProgress_reset(this, id);
                                    break;
                                }
                            }
                           

                        }







                    } // if : 60초 내 체결안될 시 다시 시작.


                }/*
            else
            {
                form.Logger("추세선과 거리가 멀어 진입 안함");
            }
            */


                /////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////                
                /// 종료



                // 모든 구독과 연결 종료
                await socketClient.UnsubscribeAllAsync();
                // socketClient.Dispose();

                //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] * MA 스레드 종료신호 보냄!!   {emaWrapper._symbol}", 0);
                emaWrapper._bFinish = true; // ma 스레드 죽이기!!!!

                // 수익률 계산            
                await getPosHistory(this, client, id, _roi_current_max, true, _strMemo, globalWrapper, _nLeverage);
                await Task.Delay(5000);
                // await CloseAllPositions(this, client, "long"); // long 포지션이 혹....시 들어와있으면 청산!!

                _bRealTrade = false;
                //_nLeverage = 1;
                _nLeverage = 5;

                // 현재 프로세스 가져오기
                Process currentProcess = Process.GetCurrentProcess();

                // 각 스레드 정보 출력
                int nThread_count = 0;
                foreach (ProcessThread pt in currentProcess.Threads)
                {
                    if (pt.ThreadState == System.Diagnostics.ThreadState.Running)
                    {
                        nThread_count++;
                        //this.Logger($"ID: {pt.Id}, 상태: {pt.ThreadState}, 시작시간: {pt.StartTime}", 0);
                    }
                }
                //this.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MA 스레드 수 : {nThread_count}", 0);




                //if (_nEnterMode.Value == 61 || _nEnterMode.Value == 62 || _nEnterMode.Value == 71 || _nEnterMode.Value == 72)
                if(true)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 3분 대기 중.........");
                    //await Task.Delay(300000);
                    await Task.Delay(180000);
                }
                else
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 20초 대기 중.........");
                    await Task.Delay(20000);
                }

            } // while : 전체


            socketClient.Dispose();
        }
        /*
        public void Logger(string message)
        {
            if (this.InvokeRequired)
            {
                // Invoke 시에는 message만 전달
                this.Invoke(new Action<string>(Logger), message);
                return;
            }

            int id = threadId?.Value ?? 0; // 쓰레드별 ID, null이면 0

            switch (id)
            {
                case 0:
                    lbMain.Items.Add(message);
                    lbMain.TopIndex = lbMain.Items.Count - 1; // 자동 스크롤
                    break;
                case 1:
                    lbTask1.Items.Add(message);
                    lbTask1.TopIndex = lbTask1.Items.Count - 1;
                    break;
                case 2:
                    lbTask2.Items.Add(message);
                    lbTask2.TopIndex = lbTask2.Items.Count - 1;
                    break;
                case 3:
                    lbTask3.Items.Add(message);
                    lbTask3.TopIndex = lbTask3.Items.Count - 1;
                    break;
            }
        }
        */
        public void Logger(string message, int temp = 1)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Logger(message, temp)));
                return;
            }

            int id = threadId?.Value ?? 0; // 쓰레드별 ID, null이면 0
            KryptonListBox targetListBox;

            // 마지막 줄 교체 모드인지 확인
            bool replaceLast = message.EndsWith("-1");
            if (replaceLast)
            {
                message = message.Substring(0, message.Length - 2).TrimEnd();
            }


            switch (id)
            {               
                case 0:
                    if (replaceLast && lbMain.Items.Count > 0)
                    {
                        lbMain.Items[lbMain.Items.Count - 1] = message; // ← 마지막 줄 덮어쓰기
                    }
                    else
                    {
                        lbMain.Items.Add(message);
                    }
                    lbMain.TopIndex = lbMain.Items.Count - 1;
                    targetListBox = lbMain;
                    break;

                case 1:
                    if (replaceLast && lbTask1.Items.Count > 0)
                    {
                        lbTask1.Items[lbTask1.Items.Count - 1] = message;
                    }
                    else
                    {
                        lbTask1.Items.Add(message);
                    }
                    lbTask1.TopIndex = lbTask1.Items.Count - 1;
                    targetListBox = lbTask1;
                    break;

                case 2:
                    if (replaceLast && lbTask2.Items.Count > 0)
                    {
                        lbTask2.Items[lbTask2.Items.Count - 1] = message;
                    }
                    else
                    {
                        lbTask2.Items.Add(message);
                    }
                    lbTask2.TopIndex = lbTask2.Items.Count - 1;
                    targetListBox = lbTask2;
                    break;

                case 3:
                    if (replaceLast && lbTask3.Items.Count > 0)
                    {
                        lbTask3.Items[lbTask3.Items.Count - 1] = message;
                    }
                    else
                    {
                        lbTask3.Items.Add(message);
                    }
                    lbTask3.TopIndex = lbTask3.Items.Count - 1;
                    targetListBox = lbTask3;
                    break;
                case 4:
                    if (replaceLast && lbTask4.Items.Count > 0)
                    {
                        lbTask4.Items[lbTask4.Items.Count - 1] = message;
                    }
                    else
                    {
                        lbTask4.Items.Add(message);
                    }
                    lbTask4.TopIndex = lbTask4.Items.Count - 1;
                    targetListBox = lbTask4;
                    break;
                case 5:
                    if (replaceLast && lbTask5.Items.Count > 0)
                    {
                        lbTask5.Items[lbTask5.Items.Count - 1] = message;
                    }
                    else
                    {
                        lbTask5.Items.Add(message);
                    }
                    lbTask5.TopIndex = lbTask5.Items.Count - 1;
                    targetListBox = lbTask5;
                    break;

                default:
                    targetListBox = lbMain;
                    break;
            }

            // ✨ 가로 스크롤 자동
            int textWidth = TextRenderer.MeasureText(message, targetListBox.Font).Width;
            if (textWidth > targetListBox.HorizontalExtent)
                targetListBox.HorizontalExtent = textWidth;

            targetListBox.HorizontalScrollbar = true; // 보장



            // 메인에도 쓰기
            if (temp == 0)
            {
                message = $"[{id}]  {message}";
                if (replaceLast && lbMain.Items.Count > 0)
                {
                    lbMain.Items[lbMain.Items.Count - 1] = message; // ← 마지막 줄 덮어쓰기
                }
                else
                {
                    lbMain.Items.Add(message);
                }
                lbMain.TopIndex = lbMain.Items.Count - 1;
                targetListBox = lbMain;


                // ✨ 가로 스크롤 자동
                textWidth = TextRenderer.MeasureText(message, targetListBox.Font).Width;
                if (textWidth > targetListBox.HorizontalExtent)
                    targetListBox.HorizontalExtent = textWidth;

                targetListBox.HorizontalScrollbar = true; // 보장
            }

            if (temp == -1)
            {
                message = $"[{id}]  {message}";
                if (replaceLast && lbError.Items.Count > 0)
                {
                    lbError.Items[lbMain.Items.Count - 1] = message; // ← 마지막 줄 덮어쓰기
                }
                else
                {
                    lbError.Items.Add(message);
                }
                lbError.TopIndex = lbError.Items.Count - 1;
                targetListBox = lbError;


                // ✨ 가로 스크롤 자동
                textWidth = TextRenderer.MeasureText(message, targetListBox.Font).Width;
                if (textWidth > targetListBox.HorizontalExtent)
                    targetListBox.HorizontalExtent = textWidth;

                targetListBox.HorizontalScrollbar = true; // 보장
            }

        }





        // SMA 계산 스레드
        static async Task calcMovingAverageThread(Form_main form, BitgetRestClient client, CancellationToken token, EmaWrapper emaWrapper)
        {
            /*
            decimal ema15 = 0;

            setEma15(9.9999m);
            return;
            */

            decimal ema2 = 0;
            decimal ema5 = 0;
            decimal ema5_2 = 0;
            decimal ema5_3 = 0;
            decimal ema10 = 0;
            decimal ema15 = 0;
            decimal ema50 = 0;
            decimal lastPrice_beforeCandle = 0;
            decimal lastPrice_beforeCandle_old = 0;
            bool bLastPrice_beforeCandle = false;

            int decimalPlaces = 1;// BitConverter.GetBytes(decimal.GetBits(_tickSize)[3])[2];
            decimal tickSize = emaWrapper._tickSize;

            while (!token.IsCancellationRequested)
            {
                if (emaWrapper._bFinish == true)
                    break;

                DateTime now = DateTime.UtcNow;
                if (emaWrapper._ema10 == 0 || ((now.Second >= 0 && now.Second <= 3) && emaWrapper._bLastPrice_beforeCandle == false))  // 매 분 1초대에만 계산
                {
                    try
                    {
                        string symbol = emaWrapper._symbol; // 전역 static
                        decimalPlaces = BitConverter.GetBytes(decimal.GetBits(tickSize)[3])[2];

                        if (symbol == null || symbol == "" || tickSize == 0)
                        {
                            await Task.Delay(1000, token);
                            continue;
                        }

                        var klines = await client.FuturesApiV2.ExchangeData.GetKlinesAsync(
                            productType: BitgetProductTypeV2.UsdtFutures,
                            symbol: symbol,
                            interval: BitgetFuturesKlineInterval.OneMinute,
                            startTime: DateTime.UtcNow.AddMinutes(-60),
                            endTime: DateTime.UtcNow,
                            //limit: 60
                            limit: 180
                        );

                        if (klines.Success)
                        {
                            var candles = klines.Data
                                .OrderBy(c => c.OpenTime)
                                .ToList();

                            // 마지막 봉 제외 (현재 진행 중인 봉)
                            var closedCandles = candles.Take(candles.Count - 1).ToList();
                            var closePrices = closedCandles.Select(c => c.ClosePrice).ToList();

                            // EMA 계산
                            ema2 = CalculateSMA(closePrices, 2);
                            ema5 = CalculateSMA(closePrices, 5);
                            ema5_2 = CalculateSMA(closePrices, 5, 2);
                            ema5_3 = CalculateSMA(closePrices, 5, 3);
                            ema10 = CalculateSMA(closePrices, 10);
                            ema15 = CalculateSMA(closePrices, 15);
                            ema50 = CalculateSMA(closePrices, 50);

                            ema2 = Math.Ceiling(ema2 / tickSize) * tickSize;
                            ema2 = Math.Round(ema2, decimalPlaces, MidpointRounding.AwayFromZero);

                            ema5 = Math.Ceiling(ema5 / tickSize) * tickSize;
                            ema5 = Math.Round(ema5, decimalPlaces, MidpointRounding.AwayFromZero);

                            ema10 = Math.Ceiling(ema10 / tickSize) * tickSize;
                            ema10 = Math.Round(ema10, decimalPlaces, MidpointRounding.AwayFromZero);

                            ema15 = Math.Ceiling(ema15 / tickSize) * tickSize;
                            ema15 = Math.Round(ema15, decimalPlaces, MidpointRounding.AwayFromZero);

                            ema50 = Math.Ceiling(ema50 / tickSize) * tickSize;
                            ema50 = Math.Round(ema50, decimalPlaces, MidpointRounding.AwayFromZero);

                            //emaWrapper._symbol = symbol;

                            if (emaWrapper._ema2 != ema2)
                            {
                                emaWrapper._ema2_1 = emaWrapper._ema2; // 1봉전
                                emaWrapper._ema2 = ema2;
                            }

                            emaWrapper._ema5 = ema5;
                            emaWrapper._ema5_2 = ema5_2;
                            emaWrapper._ema5_3 = ema5_3;
                            emaWrapper._ema10 = ema10;
                            emaWrapper._ema15 = ema15;

                            if (emaWrapper._ema50 != ema50)
                            {
                                emaWrapper._ema50_1 = emaWrapper._ema50; // 1봉전
                                emaWrapper._ema50 = ema50;
                            }


                            // ✅ 최근 3봉 최고가 계산
                            if (closedCandles.Count >= 3)
                            {
                                emaWrapper._highestHigh_3 = closedCandles
                                    .Skip(closedCandles.Count - 3)   // 최근 15봉만
                                    .Max(c => c.HighPrice);          // 최고가
                            }

                            // ✅ 최근 5봉 최고가 계산
                            if (closedCandles.Count >= 5)
                            {
                                emaWrapper._highestHigh = closedCandles
                                    .Skip(closedCandles.Count - 5)   // 최근 15봉만
                                    .Max(c => c.HighPrice);          // 최고가
                            }

                            // ✅ 최근 10봉 최고가 계산
                            if (closedCandles.Count >= 10)
                            {
                                emaWrapper._highestHigh_10 = closedCandles
                                    .Skip(closedCandles.Count - 10)   // 최근 15봉만
                                    .Max(c => c.HighPrice);          // 최고가
                            }

                            // ✅ 최근 15봉 최고가 계산
                            if (closedCandles.Count >= 15)
                            {
                                emaWrapper._highestHigh_15 = closedCandles
                                    .Skip(closedCandles.Count - 15)   // 최근 15봉만
                                    .Max(c => c.HighPrice);          // 최고가
                            }

                            // ✅ 최근 15봉 최고가 계산
                            if (closedCandles.Count >= 30)
                            {
                                emaWrapper._highestHigh_30 = closedCandles
                                    .Skip(closedCandles.Count - 30)   // 최근 15봉만
                                    .Max(c => c.HighPrice);          // 최고가
                            }




                            // ✅ 최근 3봉 (시가/종가 중 낮은 값 기준) 최저가 계산
                            if (closedCandles.Count >= 3)
                            {
                                emaWrapper._lowestLow_3 = closedCandles
                                    .Skip(closedCandles.Count - 3)       // 최근 5봉만
                                    //.Min(c => Math.Min(c.OpenPrice, c.ClosePrice)); // 시가/종가 중 낮은 값
                                    .Min(c => c.LowPrice);
                            }

                            // ✅ 최근 5봉 (시가/종가 중 낮은 값 기준) 최저가 계산
                            if (closedCandles.Count >= 5)
                            {
                                emaWrapper._lowestLow = closedCandles
                                    .Skip(closedCandles.Count - 5)       // 최근 5봉만
                                    //.Min(c => Math.Min(c.OpenPrice, c.ClosePrice)); // 시가/종가 중 낮은 값
                                    .Min(c => c.LowPrice);
                            }

                            // ✅ 최근 10봉 (시가/종가 중 낮은 값 기준) 최저가 계산
                            if (closedCandles.Count >= 10)
                            {
                                emaWrapper._lowestLow_10 = closedCandles
                                    .Skip(closedCandles.Count - 10)       // 최근 10봉만
                                    //.Min(c => Math.Min(c.OpenPrice, c.ClosePrice)); // 시가/종가 중 낮은 값
                                    .Min(c => c.LowPrice);
                            }

                            // ✅ 최근 15봉 (시가/종가 중 낮은 값 기준) 최저가 계산
                            if (closedCandles.Count >= 15)
                            {
                                emaWrapper._lowestLow_15 = closedCandles
                                    .Skip(closedCandles.Count - 15)       // 최근 15봉만
                                    //.Min(c => Math.Min(c.OpenPrice, c.ClosePrice)); // 시가/종가 중 낮은 값
                                    .Min(c => c.LowPrice);
                            }

                            // ✅ 최근 30봉 (시가/종가 중 낮은 값 기준) 최저가 계산
                            if (closedCandles.Count >= 30)
                            {
                                emaWrapper._lowestLow_30 = closedCandles
                                    .Skip(closedCandles.Count - 30)       // 최근 30봉만
                                    //.Min(c => Math.Min(c.OpenPrice, c.ClosePrice)); // 시가/종가 중 낮은 값
                                    .Min(c => c.LowPrice);
                            }

                            //form.Logger( $"{now.Second}sec => EMA10: {_ema10.Value}, EMA15: {_ema15.Value}, EMA50: {_ema50.Value}");

                            lastPrice_beforeCandle = closedCandles.Last().ClosePrice;
                            //emaWrapper._lastPrice_beforeCandle = lastPrice_beforeCandle;
                            //lastPrice_beforeCandle_old = emaWrapper._lastPrice_beforeCandle_old;

                            if (lastPrice_beforeCandle != emaWrapper._lastPrice_beforeCandle) // 기존과 현재 비교
                            {
                                //bLastPrice_beforeCandle = true;
                                emaWrapper._lastPrice_beforeCandle_old2 = emaWrapper._lastPrice_beforeCandle_old;
                                emaWrapper._lastPrice_beforeCandle_old = emaWrapper._lastPrice_beforeCandle; // backup
                                //lastPrice_beforeCandle_old = emaWrapper._lastPrice_beforeCandle;

                                emaWrapper._lastPrice_beforeCandle = lastPrice_beforeCandle;

                                emaWrapper._bLastPrice_beforeCandle = true;

                                //form.Logger($"[{Thread.CurrentThread.ManagedThreadId}] *다름 : {symbol}, {emaWrapper._lastPrice_beforeCandle_old}, {emaWrapper._lastPrice_beforeCandle}");
                            }

                            //form.Logger( $"{now.Second}sec => _lastPrice_beforeCandle .Value = {_lastPrice_beforeCandle.Value}  15 = {_ema15.Value}");
                            /*
                            emaWrapper._low_1 = closedCandles[closedCandles.Count - 1].LowPrice; // 진행 중인 현재봉의 -1 봉
                            emaWrapper._low_2 = closedCandles[closedCandles.Count - 2].LowPrice;
                            emaWrapper._low_3 = closedCandles[closedCandles.Count - 3].LowPrice;
                            emaWrapper._low_4 = closedCandles[closedCandles.Count - 4].LowPrice;

                            // 1봉전~3봉전 최고가
                            emaWrapper._high_1 = closedCandles[closedCandles.Count - 1].HighPrice;
                            emaWrapper._high_2 = closedCandles[closedCandles.Count - 2].HighPrice;
                            emaWrapper._high_3 = closedCandles[closedCandles.Count - 3].HighPrice;
                            emaWrapper._high_4 = closedCandles[closedCandles.Count - 4].HighPrice;
                            */

                            emaWrapper._close_1 = closedCandles[closedCandles.Count - 1].ClosePrice; // 진행 중인 현재봉의 -1 봉
                            emaWrapper._close_2 = closedCandles[closedCandles.Count - 2].ClosePrice;
                            emaWrapper._close_3 = closedCandles[closedCandles.Count - 3].ClosePrice;
                            emaWrapper._close_4 = closedCandles[closedCandles.Count - 4].ClosePrice;

                            // ✅ 최근 5개 봉(직전봉 포함)에서 변동 비율 계산
                            if (closedCandles.Count >= 5)
                            {
                                var last5 = closedCandles.Skip(closedCandles.Count - 5).ToList();

                                decimal maxRatio = 0m;  // 기본값 0 (양봉은 0 처리)
                                foreach (var c in last5)
                                {
                                    if (c.ClosePrice < c.OpenPrice && c.OpenPrice > 0) // ✅ 음봉일 때만
                                    {
                                        decimal ratio = Math.Abs((c.OpenPrice - c.LowPrice) / c.OpenPrice * 100);
                                        if (ratio > maxRatio)
                                            maxRatio = ratio;
                                    }
                                }

                                maxRatio = Math.Round(maxRatio, 2, MidpointRounding.AwayFromZero);
                                emaWrapper._bongsize_max = maxRatio; // 전역 변수 저장

                                // form.Logger( $"최근 5봉 음봉 하락비율 최대값: {emaWrapper._bongsize_max}");
                            }

                            //form.Logger($"[{Thread.CurrentThread.ManagedThreadId}]  {symbol}, {emaWrapper._ema10} {emaWrapper._ema15} {emaWrapper._ema50} ||{emaWrapper._lowestLow} {emaWrapper._lowestLow_15} {emaWrapper._lowestLow_30}, bongsize_max = {emaWrapper._bongsize_max}");



                            ////
                            // --- 볼린저밴드 계산 ---

                            int period = 40;// 15;// 20;
                            decimal multiplier = 2m;

                            //if (DateTime.Now.Hour >= 17 || DateTime.Now.Hour <= 6)
                            //period = 15;


                            //if (closePrices.Count >= 40)
                            //if (closePrices.Count >= 20)
                            if (closePrices.Count >= period)
                            {
                                var last40 = closePrices.Skip(closePrices.Count - period).ToList();

                                decimal sma40 = last40.Average();
                                // 표준편차 계산
                                decimal variance = last40.Sum(c => (c - sma40) * (c - sma40)) / last40.Count;
                                decimal stdDev = (decimal)Math.Sqrt((double)variance);

                                decimal upperBand = sma40 + 2m * stdDev;
                                decimal lowerBand = sma40 - 2m * stdDev;

                                // 반올림 및 틱사이즈 조정
                                upperBand = Math.Round(Math.Ceiling(upperBand / tickSize) * tickSize, decimalPlaces);
                                lowerBand = Math.Round(Math.Floor(lowerBand / tickSize) * tickSize, decimalPlaces);

                                // 전역 저장
                                emaWrapper._bb_upper = upperBand;
                                emaWrapper._bb_lower = lowerBand;
                                emaWrapper._bb_middle = sma40;

                                //form.Logger($"*** BB [{emaWrapper._symbol}] {upperBand}  {lowerBand} ");
                            }


                            /*
                            // 1봉전
                            var closes = closedCandles.Skip(closedCandles.Count - period - 1).Take(period).Select(c => c.ClosePrice).ToList();
                            decimal sma = closes.Average();
                            decimal std = (decimal)Math.Sqrt((double)closes.Average(x => (x - sma) * (x - sma)));
                            emaWrapper._bb_upper_1 = sma + multiplier * std;
                            emaWrapper._bb_lower_1 = sma - multiplier * std;
                            */

                            // 2봉전
                            var closes = closedCandles.Skip(closedCandles.Count - period - 2).Take(period).Select(c => c.ClosePrice).ToList();
                            decimal sma = closes.Average();
                            decimal std = (decimal)Math.Sqrt((double)closes.Average(x => (x - sma) * (x - sma)));
                            emaWrapper._bb_upper_2 = sma + multiplier * std;
                            emaWrapper._bb_lower_2 = sma - multiplier * std;

                            // 3봉전
                            closes = closedCandles.Skip(closedCandles.Count - period - 3).Take(period).Select(c => c.ClosePrice).ToList();
                            sma = closes.Average();
                            std = (decimal)Math.Sqrt((double)closes.Average(x => (x - sma) * (x - sma)));
                            emaWrapper._bb_upper_3 = sma + multiplier * std;
                            emaWrapper._bb_lower_3 = sma - multiplier * std;

                            // 4봉전
                            closes = closedCandles.Skip(closedCandles.Count - period - 4).Take(period).Select(c => c.ClosePrice).ToList();
                            sma = closes.Average();
                            std = (decimal)Math.Sqrt((double)closes.Average(x => (x - sma) * (x - sma)));
                            emaWrapper._bb_upper_4 = sma + multiplier * std;
                            emaWrapper._bb_lower_4 = sma - multiplier * std;

                            // 5봉전
                            closes = closedCandles.Skip(closedCandles.Count - period - 5).Take(period).Select(c => c.ClosePrice).ToList();
                            sma = closes.Average();
                            std = (decimal)Math.Sqrt((double)closes.Average(x => (x - sma) * (x - sma)));
                            emaWrapper._bb_upper_5 = sma + multiplier * std;
                            emaWrapper._bb_lower_5 = sma - multiplier * std;




                            // 앞쪽 50이평 갭 계산용
                            var closeList = closedCandles.Select(c => c.ClosePrice).ToList();
                            bool foundBelow = false;
                            bool foundAbove = false;

                            period = 50; // SMA50 기간

                            // 최근 봉부터 과거로
                            for (int i = closeList.Count - 5; i >= 50 - 1; i--)
                            {
                                // 현재 봉까지의 최근 50봉
                                var subList = closeList.GetRange(i - period + 1, period);

                                // SMA50 계산
                                decimal sma50ForCandle = subList.Average();

                                // SMA50 아래 종가 체크
                                if (!foundBelow && closedCandles[i].ClosePrice < sma50ForCandle && ema10 > ema50)
                                {
                                    emaWrapper._belowEma50 = closedCandles[i].ClosePrice;
                                    //form.Logger($"[{emaWrapper._symbol}] {closeList.Count - i}봉 전: SMA50({sma50ForCandle}) 50 아래 종가 발견 → 가격: {emaWrapper._belowEma50}");
                                    foundBelow = true;
                                }

                                // SMA50 위 종가 체크
                                if (!foundAbove && closedCandles[i].ClosePrice > sma50ForCandle && ema10 < ema50)
                                {
                                    emaWrapper._aboveEma50 = closedCandles[i].ClosePrice;
                                    //form.Logger($"[{emaWrapper._symbol}] {closeList.Count - i}봉 전: SMA50({sma50ForCandle}) 50 위 종가 발견 → 가격: {emaWrapper._aboveEma50}");
                                    foundAbove = true;
                                }

                                // 두 가지 모두 찾으면 종료
                                if (foundBelow && foundAbove)
                                    break;
                            }

                            // 없으면 0 처리
                            if (!foundBelow) emaWrapper._belowEma50 = 0m;
                            if (!foundAbove) emaWrapper._aboveEma50 = 0m;


                        }
                        else
                        {
                            form.Logger($"SMA 조회 실패: {klines.Error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        form.Logger($"SMA 계산 오류: {ex.Message}");

                    }





                    // 다음 분까지 대기
                    //await Task.Delay(50000, token); // 59초 대기 → 다음 분 초 1초대에 실행
                    //await Task.Delay(1000, token); // 59초 대기 → 다음 분 초 1초대에 실행
                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return; // Task 종료
                    }
                }
                else
                {
                    //await Task.Delay(1000, token); // 0.5초마다 시간 체크
                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return; // Task 종료
                    }
                }

                // 초기화
                if (now.Second > 56)
                {
                    bLastPrice_beforeCandle = false;
                    emaWrapper._bLastPrice_beforeCandle = bLastPrice_beforeCandle;
                    emaWrapper._belowEma50 = 0;
                    emaWrapper._aboveEma50 = 0;

                }

            }
            //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] * MA 스레드 종료!!   {emaWrapper._symbol}", 0);
            return;
        }



        // SMA 계산 함수    
        /*
        static decimal CalculateSMA(List<decimal> prices, int period)
        {
            if (prices.Count < period)
                throw new ArgumentException("가격 데이터가 기간보다 적습니다.");

            return prices.Skip(prices.Count - period).Average();
        }
        */
        static decimal CalculateSMA(List<decimal> prices, int period, int offsetFromLast = 0)
        {
            if (prices.Count < period + offsetFromLast)
                throw new ArgumentException("가격 데이터가 필요기간보다 적습니다");

            // 예: offsetFromLast=2 → 2봉 전 기준 SMA
            int endIndex = prices.Count - offsetFromLast;
            var range = prices.Skip(endIndex - period).Take(period);

            return range.Average();
        }

        // tickSize 계산
        static decimal GetTickSize(decimal pricePlace)
        {
            decimal tickSize = 1;

            for (int i = 0; i < pricePlace; i++)
            {
                tickSize = tickSize / 10m;
            }

            return tickSize;
        }

        static decimal dummy()
        {
            return 1m;
        }

        // TTS
        static void speech(string textToRead, int speed = 2)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            // 속도 (기본: 0, 범위: -10 ~ 10)
            synth.Rate = speed;   // 2 → 조금 빠르게, -2 → 조금 느리게

            // 음량 (기본: 100, 범위: 0 ~ 100)
            synth.Volume = 100;
            synth.SelectVoice("Microsoft Heami Desktop"); // 한국어

            synth.SpeakAsync(textToRead);  // <- 여기까지는 실행됨
        } // 함수 끝나면 synth가 GC 대상이 됨

        /*
        static void speech(string textToRead, int speed = 2)
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.Rate = speed;
                synth.Volume = 100;

                // 1️⃣ TTS 음성을 WAV 파일로 저장
                string filePath = "tts_output.wav";
                synth.SetOutputToWaveFile(filePath);
                synth.Speak(textToRead);

                // 2️⃣ 저장된 파일을 재생
                SoundPlayer player = new SoundPlayer(filePath);
                player.Play();
            }
        }
        */
        /*
        static void speech(string text, int speed=2)
        {
            using (var synth = new SpeechSynthesizer())
            {
                synth.Volume = 100;
                synth.Rate = 0;

                string path = Path.Combine(Path.GetTempPath(), "tts.wav");
                synth.SetOutputToWaveFile(path);
                synth.Speak(text);
                synth.SetOutputToNull();

                SoundPlayer player = new SoundPlayer(path);
                player.Play();

                Console.WriteLine($"TTS 실행됨: {path}");
            }
        }
        */

        // Position History
        static async Task<string> getPosHistory(Form_main form, BitgetRestClient client, decimal id, decimal _roi_current_max, bool bSendMessage, string _strMemo, GlobalWrapper globalWrapper, decimal _nLeverage)
        {
            decimal roi = 0;
            decimal roi_total = 0;
            decimal pnl_total = 0;
            string roiStr = "";
            string pnlStr = "";
            string rowData = "";
            string rowData2 = "";

            decimal pnl = 0;
            decimal AverageOpenPrice = 0;
            decimal CloseTotalPosition = 0;
            decimal openPrice = 0;
            decimal closePrice = 0;
            decimal investMoney = 0;
            decimal investMoney_x1 = 0;
            decimal investMoney_margin = 0;
            decimal openFee = 0;
            decimal closeFee = 0;
            decimal feeTotal = 0;

            // 한국시간 (오늘 0시부터 현재까지)
            DateTime startKst = DateTime.Today; // 오늘 0시 (KST)
            DateTime endKst = DateTime.Now;     // 현재 (KST)

            // UTC로 변환 (Bitget API 요구사항)
            DateTime startUtc = startKst.ToUniversalTime();
            DateTime endUtc = endKst.ToUniversalTime();

            var posHistory = await client.FuturesApiV2.Trading.GetPositionHistoryAsync(
                productType: BitgetProductTypeV2.UsdtFutures,
                //symbol: threadSymbol.Value,
                startTime: startUtc,
                endTime: endUtc,
                limit: 500
            );

            if (!posHistory.Success)
            {
                form.Logger($"Position Hitory 실패: {posHistory.Error}");
                return _strMemo;
            }

            decimal dummy = 0m;
            int i = 0;
            int item_count = posHistory.Data.Entries.Count();
            form.Logger($"=== Position Hitory ===");

            if (form.dataGridView1.InvokeRequired)
            {
                form.dataGridView1.Invoke(new Action(() => form.dataGridView1.Rows.Clear()));
            }
            else
            {
                form.dataGridView1.Rows.Clear();
            }

            //_RealRoi = 0;
            //_RealPnl = 0;
            decimal divide = 1;
            foreach (var pos in posHistory.Data.Entries.Reverse())
            {
                // 투자금
                AverageOpenPrice = Convert.ToDecimal(pos.AverageOpenPrice);
                CloseTotalPosition = Convert.ToDecimal(pos.CloseTotalPosition);

                // 총 투입금 (레버리지 x1)
                investMoney = (AverageOpenPrice * CloseTotalPosition);

                if ((_nEnterMode.Value == 91 || _nEnterMode.Value == 92) && (DateTime.Now.Hour > 20 || DateTime.Now.Hour < 2))
                {
                    investMoney_x1 = (AverageOpenPrice * CloseTotalPosition) ;
                    // 증거금
                    investMoney_margin = investMoney ; // 기본 레버리지 5라 치고 과거 서버꺼 계산.

                }
                else
                {
                    investMoney_x1 = (AverageOpenPrice * CloseTotalPosition) / 5;
                    // 증거금
                    investMoney_margin = investMoney / 5; // 기본 레버리지 5라 치고 과거 서버꺼 계산.

                }



                // test  임시 추가
                if (investMoney_margin > 42)
                {
                    divide = Math.Round(investMoney_margin / 36);

                    if(divide > 0)
                        investMoney_margin = investMoney_margin / divide; // 2차 이상 진입시 나누어 1차 진입금 계산해내기
                }

                // fee
                openFee = Convert.ToDecimal(pos.OpenFee);
                closeFee = Convert.ToDecimal(pos.CloseFee);
                feeTotal = openFee + closeFee;

                // 수익금
                pnl = pos.NetProfit; // (openfee, closefee는 계산에 포함된 순수익.)


                // 수익률                
                openPrice = Convert.ToDecimal(pos.AverageOpenPrice);
                closePrice = Convert.ToDecimal(pos.AverageClosePrice);

                //roi = (openPrice / closePrice * 100m) - 100m; // 숏이니까 반대로. (openfee, closefee는 계산 제외)
                roi = pnl / investMoney * 100; //// 증거금 기준으로 하려면 추후 * 5 해줘야 함!!

                //if (investMoney > 9) // 1차 원금대비 보정
                //roi *= 2m;// 1.4m; // 0.7

                roi = Math.Round(roi * 100) / 100;
                roiStr = roi >= 0 ? $" {roi}%" : $"{roi}%";
                roi_total += roi;

                // 수익금, 투자금 보정
                pnl = Math.Round(pnl * 100) / 100;
                pnlStr = pnl >= 0 ? $" {pnl}" : $"{pnl}";
                pnl_total += pnl;

                investMoney = Math.Round(investMoney, 2, MidpointRounding.AwayFromZero);
                investMoney_x1 = Math.Round(investMoney_x1, 2, MidpointRounding.AwayFromZero);
                investMoney_margin = Math.Round(investMoney_margin, 2, MidpointRounding.AwayFromZero);

                i++;
                if (i < item_count)
                    rowData = $"{pos.CreateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}, {pos.CreateTime.ToLocalTime():yyyy-MM-dd}, {pos.CreateTime.ToLocalTime():ddd}, {pos.CreateTime.ToLocalTime():HH:mm:ss}, {pos.UpdateTime.ToLocalTime():HH:mm:ss}, {globalWrapper._entryStep}, {id}, {_mode_direction.Value}, {_nEnterMode.Value}, {globalWrapper._detectedPercent}, {(double)roi}, {dummy}, {(double)pnl}, {(double)investMoney_x1}, {pos.Symbol}, {dummy}, {pos.Side}, {(double)pos.OpenTotalPosition}, {(double)pos.CloseTotalPosition}, {(double)pos.AverageOpenPrice}, {pos.AverageClosePrice}, {openFee}, {closeFee}";
                else
                {
                    rowData = $"{pos.CreateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}, {pos.CreateTime.ToLocalTime():yyyy-MM-dd}, {pos.CreateTime.ToLocalTime():ddd}, {pos.CreateTime.ToLocalTime():HH:mm:ss}, {pos.UpdateTime.ToLocalTime():HH:mm:ss}, {globalWrapper._entryStep}, {id},  {_mode_direction.Value}, {_nEnterMode.Value}, {globalWrapper._detectedPercent}, {(double)roi}, {(double)_roi_current_max}, {(double)pnl}, {(double)investMoney_x1}, {pos.Symbol}, {_strMemo} , {pos.Side},  {(double)pos.OpenTotalPosition}, {(double)pos.CloseTotalPosition}, {(double)pos.AverageOpenPrice}, {pos.AverageClosePrice},  {openFee}, {closeFee}";
                    form.Logger($"[{pos.CreateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}] :: {roiStr}   $ {pnlStr} ($ {investMoney_x1})   {pos.Symbol} | {pos.Side} | {pos.CloseTotalPosition} | {pos.AverageOpenPrice} | {pos.AverageClosePrice}"); // 화면 출력.
                }

                //form.Logger(rowData);



                addDataToGoogleSheet(form, rowData); // 구글 스프레드 시트에 기록.

                rowData2 = $"{pos.CreateTime.ToLocalTime():yyyy-MM-dd}, {pos.CreateTime.ToLocalTime():ddd}, {pos.CreateTime.ToLocalTime():HH:mm:ss}, {(double)roi}, {(double)pnl}, {(double)investMoney}, {pos.Symbol}, {pos.Side}, {(double)pos.CloseTotalPosition}";
                form.addDataToDataGridView1(form, rowData2); // dataGridView1에 중복체크하고 추가.
            }

            form.Logger("--------------------------------------------------------------------------");
            //Console.BackgroundColor = ConsoleColor.Magenta;
            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   *** 당일 총손익 : {roi_total}%, {pnl_total}$ =>  {pnl_total * 1400:N0} 원", 0);
            //Console.ResetColor();
            form.Logger("");
            form.Logger("");




            try
            {
                form.Invoke(new Action(() =>
                {
                    // 현재 수익 표시
                    form.tbRealOutput_roi.Text = roi_total.ToString();
                    form.tbRealOutput_pnl.Text = (pnl_total * 1400).ToString("N0"); // 5배 레버리지

                    // ROI 색상
                    if (roi_total > 0)
                    {
                        form.tbRealOutput_roi.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbRealOutput_roi.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (roi_total < 0)
                    {
                        form.tbRealOutput_roi.StateCommon.Back.Color1 = System.Drawing.Color.CornflowerBlue;
                        form.tbRealOutput_roi.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else
                    {
                        form.tbRealOutput_roi.StateCommon.Back.Color1 = System.Drawing.Color.White;
                        form.tbRealOutput_roi.StateCommon.Content.Color1 = System.Drawing.Color.Black;
                    }

                    // PNL 색상
                    decimal pnlValue = pnl_total * 1400;
                    if (pnlValue > 0)
                    {
                        form.tbRealOutput_pnl.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbRealOutput_pnl.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (pnlValue < 0)
                    {
                        form.tbRealOutput_pnl.StateCommon.Back.Color1 = System.Drawing.Color.CornflowerBlue;
                        form.tbRealOutput_pnl.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else
                    {
                        form.tbRealOutput_pnl.StateCommon.Back.Color1 = System.Drawing.Color.White;
                        form.tbRealOutput_pnl.StateCommon.Content.Color1 = System.Drawing.Color.Black;
                    }


                }));
            }
            catch (Exception ex)
            {
                int a = 0;
            }


            // 텔레그램 전송
            DateTime now = DateTime.Now;
            string strMessage = "";
            if (bSendMessage == true && now.Hour >= 7 && now.Hour < 23)
            {
                string strPrefix;

                if (roi <= -2m)
                    strPrefix = "%F0%9F%8C%9A ";               // 🌚
                else if (roi > -2m && roi < 0m)
                    strPrefix = "%F0%9F%91%8E%F0%9F%8F%BD ";  // 👎🏼
                else if (roi >= 0m && roi < 1m)
                    strPrefix = "%F0%9F%92%9A ";               // 💚
                else if (roi >= 1m && roi < 2m)
                    strPrefix = "%E2%9D%A4%EF%B8%8F ";        // ❤️
                else if (roi >= 2m && roi < 3m)
                    strPrefix = "%F0%9F%94%A5 ";               // 🔥
                else if (roi >= 3m && roi < 5m)
                    strPrefix = "%F0%9F%92%A5 ";               // 💥
                else if (roi >= 5m && roi < 7m)
                    strPrefix = "%E2%9A%A1%EF%B8%8F ";        // ⚡️
                else if (roi >= 7m && roi < 9m)
                    strPrefix = "%E2%AD%90%EF%B8%8F ";        // ⭐️
                else
                    strPrefix = "%F0%9F%8C%9F%F0%9F%91%91%F0%9F%8C%9F "; // 🌟👑🌟 왕관 

                if (_mode_direction.Value == 1) // 상방
                    strPrefix += "  ↗";
                else if (_mode_direction.Value == 2)
                    strPrefix += "  ↘";

                //string strMessage = $"[체결] {strPrefix} {_mode}  {threadSymbol.Value} %0A%0C * 청산 : {roiStr}, ${pnlStr}  => {pnl * 1400:N0} 원 %0A%0C             ({roi_total}%, $ {pnl_total} => {pnl_total * 1400:N0} 원)";
                strMessage = $"[체결] {strPrefix} {id}  {threadSymbol.Value}  {_nEnterMode.Value} x{_nLeverage} %0A%0C ▷ {roiStr} < {_roi_current_max}  ({pnl * 1400:N0}원) | {roi_total}% ({pnl_total * 1400:N0}원)";
                if (_strMemo != "")
                    strMessage += $"%0A%0C → {_strMemo}";

                // form.Logger( $"send message : {strMessage}");
                SendMessage(_strChatId.ToString(), strMessage, _strToken_general); // 일반
            }

            if (_bRealTrade == true) // 실전
            {
                // 해당일 시작 시점체크하여 초기화
                if (item_count == 1)
                {
                    _RealRoi = 0;
                    _RealPnl = 0;
                }

                _RealRoi += roi;
                _RealPnl += pnl;

                //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   ◆실전모드◆ 당일 총손익 : {_RealRoi}%  {_RealPnl * 1400:N0}원", 0);

                //if (_RealRoi >= 10m || (_RealPnl * 1400 > 200000)) // 단위 5% or 20만원
                if (roi_total >= 10m || (pnl_total * 1420 > 200000)) // 단위 5% or 20만원
                {
                    ///// _bRealSuccess = true;

                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]   ◈◈◈실전모드◈◈◈ : {_RealRoi} %  목표달성!! ^0^   단위테스트로 계속 진행..", 0);

                    //strMessage = $"[목표달성] %E2%AD%90%EF%B8%8F %0A%0C ▷ {_RealRoi}%  ({_RealPnl * 1400:N0}원)";
                    strMessage = $"[목표달성] %E2%AD%90%EF%B8%8F %0A%0C ▷ {roi_total}%  ({pnl_total * 1400:N0}원)";
                    form.Logger($"send message : {strMessage}");
                    SendMessage(_strChatId.ToString(), strMessage, _strToken_general); // 일반
                }

            }
            /*
            // 매매 정지, 매매 중지, 손실 제한, 손실제한
            if (now.Hour < 7 && roi_total < -80.0m)//-5.0m)
            {
                form.Logger("손실제한 : 엔터를 누르면 종료됩니다...");
                Console.ReadLine();
            }
            */

            _strMemo = "";
            return _strMemo;
        }

        // Position History
        static async Task getPosHistory_by_period(Form_main form, BitgetRestClient client)
        {
            form.dataGridView2.Rows.Clear();

            form.Logger("=== 지난 30일간 일별 손익 기록 ===");
            string rowData = "";

            try
            {
                // 7일 전 → 오늘 순서로 조회
                //for (int d = 6; d >= 0; d--)
                for (int d = 29; d >= 0; d--)
                {
                    DateTime targetDayKst = DateTime.Today.AddDays(-d);   // d일 전
                    DateTime startKst = targetDayKst;                     // 0시
                    DateTime endKst = targetDayKst.AddDays(1).AddTicks(-1); // 23:59:59

                    DateTime startUtc = startKst.ToUniversalTime();
                    DateTime endUtc = endKst.ToUniversalTime();

                    var posHistory = await client.FuturesApiV2.Trading.GetPositionHistoryAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        startTime: startUtc,
                        endTime: endUtc,
                        limit: 500
                    );

                    if (!posHistory.Success)
                    {
                        form.Logger($"[실패] {targetDayKst:yyyy-MM-dd} 데이터 조회 실패: {posHistory.Error}");
                        continue;
                    }

                    decimal roi_total = 0m;
                    decimal pnl_total = 0m;
                    decimal openPrice = 0m;
                    decimal closePrice = 0m;
                    decimal closeTotalPosition = 0m;
                    decimal roi = 0m;
                    decimal investMoney = 0m;

                    // PNL
                    decimal pnl = 0m;

                    foreach (var pos in posHistory.Data.Entries)
                    {
                        // ROI
                        openPrice = Convert.ToDecimal(pos.AverageOpenPrice);
                        closePrice = Convert.ToDecimal(pos.AverageClosePrice);
                        closeTotalPosition = Convert.ToDecimal(pos.CloseTotalPosition);
                        roi = 0m;
                        investMoney = openPrice * closeTotalPosition;

                        // PNL
                        pnl = Math.Round(pos.NetProfit, 2, MidpointRounding.AwayFromZero);
                        pnl_total += pnl;

                        roi = pnl / investMoney * 100;
                        roi = Math.Round(roi * 100) / 100;
                        roi_total += roi;
                    }

                    // 결과 문자열
                    rowData = $"{targetDayKst:yyyy-MM-dd}|{targetDayKst:ddd}|{roi_total}|{pnl_total}|{pnl_total * 1400:N0}";

                    // DataGridView에 추가
                    form.addDataToDataGridView2(form, rowData);

                    // 로그 출력
                    form.Logger($"[{targetDayKst:yyyy-MM-dd}] 총 손익: {roi_total}% , $ {pnl_total} => {pnl_total * 1400:N0} 원");
                }
            }
            catch (Exception ex)
            {
                form.Logger($"[Error] 조회 오류 : {ex.Message}");
            }

            form.Logger("=== 지난 30일 기록 완료 ===");
        }

        // 마진모드 설정 : isolated, cross
        public static async Task setMarginMode(Form_main form, string apiKey, string apiSecret, string passphrase, string strMarginMode)
        {
            string url = "https://api.bitget.com/api/v2/mix/account/set-margin-mode";
            string method = "POST";
            string requestPath = "/api/v2/mix/account/set-margin-mode";


            string productType = "USDT-FUTURES";
            string marginCoin = "usdt";
            //threadSymbol.Value = "CVXUSDT";

            string body = $"{{\"symbol\":\"{threadSymbol.Value}\",\"productType\":\"{productType}\",\"marginCoin\":\"{marginCoin}\",\"marginMode\":\"{strMarginMode}\"}}";
            //form.Logger( $"{body}");

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string prehash = timestamp + method + requestPath + body;

            string sign = Sign(prehash, apiSecret);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("ACCESS-KEY", apiKey);
            client.DefaultRequestHeaders.Add("ACCESS-SIGN", sign);
            client.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", timestamp);
            client.DefaultRequestHeaders.Add("ACCESS-PASSPHRASE", passphrase);
            client.DefaultRequestHeaders.Add("locale", "en-US");

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            await Task.Delay(500);
            string result = await response.Content.ReadAsStringAsync();

            //form.Logger( $"{result.ToString()}");
            if (result.Contains("success"))
            {
                // JSON 파싱
                var json = JsonConvert.DeserializeObject<JObject>(result);

                form.Logger($"마진모드 설정완료. {(string)json["data"]["marginMode"]}");
            }
            else
            {
                form.Logger($"마진모드 설정 실패.... : {result}");
            }
        }

        // 레버리지 설정
        public static async Task setLeverage(Form_main form, string apiKey, string apiSecret, string passphrase, decimal nLevelage)
        {
            string url = "https://api.bitget.com/api/v2/mix/account/set-leverage";
            string method = "POST";
            string requestPath = "/api/v2/mix/account/set-leverage";


            string productType = "USDT-FUTURES";
            string marginCoin = "usdt";
            //threadSymbol.Value = "CVXUSDT";

            string body = $"{{\"symbol\":\"{threadSymbol.Value}\",\"productType\":\"{productType}\",\"marginCoin\":\"{marginCoin}\",\"leverage\":\"{nLevelage}\",\"holdSide\":\"short\"}}";
            //form.Logger( $"{body}");

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string prehash = timestamp + method + requestPath + body;

            string sign = Sign(prehash, apiSecret);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ACCESS-KEY", apiKey);
                client.DefaultRequestHeaders.Add("ACCESS-SIGN", sign);
                client.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", timestamp);
                client.DefaultRequestHeaders.Add("ACCESS-PASSPHRASE", passphrase);
                client.DefaultRequestHeaders.Add("locale", "en-US");

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                //await Task.Delay(500);
                string result = await response.Content.ReadAsStringAsync();

                //form.Logger( $"{result.ToString()}");
                if (result.Contains("success"))
                {
                    // JSON 파싱
                    var json = JsonConvert.DeserializeObject<JObject>(result);

                    form.Logger($"레버리지 설정완료. x{(string)json["data"]["shortLeverage"]}");
                }
                else
                {
                    form.Logger($"레버리지 설정 실패.... : {result}");
                }
            }
        }


        // stoploss 설정
        public static async Task setStopLoss(Form_main form, string apiKey, string apiSecret, string passphrase, decimal lastPrice, decimal triggerPrice, decimal quantity, decimal _tickSize, EmaWrapper emaWrapper, decimal _totalAveragePrice, GlobalWrapper globalWrapper)
        {

            string side = "buy";
            int _decimalPlaces = BitConverter.GetBytes(decimal.GetBits(_tickSize)[3])[2];

            // 트리거 가격 계산        
            //if(_mode_direction == 1) // 상방
            /////////triggerPrice = triggerPrice + (triggerPrice * 0.02m);
            //triggerPrice = triggerPrice + (triggerPrice * 0.03m);
            //triggerPrice = triggerPrice + (triggerPrice * 0.015m);
            //triggerPrice = triggerPrice + (triggerPrice * 0.01m);
            //else
            //////////triggerPrice = triggerPrice + (triggerPrice * 0.02m);
            //triggerPrice = triggerPrice + (triggerPrice * 0.03m);
            /////triggerPrice = triggerPrice + (triggerPrice * 0.015m);
            //triggerPrice = triggerPrice + (triggerPrice * 0.01m);

            DateTime now = new DateTime();

            /*
            if(now.Hour >= 7 && now.Hour < 17)
                triggerPrice = triggerPrice + (triggerPrice * 0.01m);
            else
                triggerPrice = triggerPrice + (triggerPrice * 0.015m);
            */

            // 해당봉 시가
            //decimal sizePer = lastPrice
            // -5%는 바로 긴급청산 설정되어 있음.
            while (true)
            {
                if (emaWrapper._highestHigh > 0) // 5분
                {
                    // 기준 0.5% 위
                    triggerPrice = emaWrapper._highestHigh * 1.003m; // 0.3위

                    // lastPrice 기준으로 1% 이상 차이가 안 나면 → 최소 1% 위로 보정
                    if (triggerPrice < _totalAveragePrice * 1.015m)
                    {
                        triggerPrice = _totalAveragePrice * 1.015m;
                    }

                    if (triggerPrice > _totalAveragePrice * 1.035m)
                    {
                        triggerPrice = _totalAveragePrice * 1.035m;
                    }

                    break;
                }
                await Task.Delay(1000);
            }
            // 고정
            //triggerPrice = _totalAveragePrice * 1.015m;
            /*
            if (_nEnterMode == 3)
            {
                triggerPrice = _lastPrice_beforeCandle;

                if (triggerPrice > _totalAveragePrice * 1.035m)
                {
                    triggerPrice = _totalAveragePrice * 1.035m;
                }
            }
            */
            //if (_nEnterMode == 1)
            //triggerPrice = _totalAveragePrice * 1.011m;
            triggerPrice = _totalAveragePrice * 1.015m;



            // 2025-09-21 by whhwang, 최적화
            if (_nEnterMode.Value == 3)
            {
                if (_mode_direction.Value == 1) // 상방
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 : {emaWrapper._lastPrice_beforeCandle_old2} {_totalAveragePrice}   {((emaWrapper._lastPrice_beforeCandle_old - _totalAveragePrice) / _totalAveragePrice * 100m).ToString()} %", 0);
                    if (((emaWrapper._lastPrice_beforeCandle_old2 - _totalAveragePrice) / _totalAveragePrice * 100m) > 4)
                    {
                        triggerPrice = _totalAveragePrice * 1.025m;
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - > 4 : {_totalAveragePrice} {triggerPrice}", 0);
                    }
                    else
                    {
                        //triggerPrice = emaWrapper._lastPrice_beforeCandle_old * 1.005m;
                        triggerPrice = emaWrapper._lastPrice_beforeCandle_old2;
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - < 4 : {emaWrapper._lastPrice_beforeCandle_old2}", 0);
                    }

                    /*
                    if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 18) // 낮장
                    {
                        //triggerPrice = _totalAveragePrice * 1.012m;
                        triggerPrice = _totalAveragePrice * 1.02m;


                        //triggerPrice = emaWrapper._highestHigh_3 * 1.003m; // 3선 고점 위 0.3
                    }
                    else
                        //triggerPrice = emaWrapper._highestHigh_15;
                        //triggerPrice = emaWrapper._highestHigh_15 * 1.006m; // 15선 고점 위 0.8 -> 1.2
                        triggerPrice = _totalAveragePrice * 1.009m;
                        */
                }
                else
                    //triggerPrice = _totalAveragePrice * 1.009m;
                    triggerPrice = emaWrapper._highestHigh;

                //triggerPrice = _totalAveragePrice.Value * 1.03m;
                //triggerPrice = _totalAveragePrice * 1.015m;
            }
            else if (_nEnterMode.Value == 39) // 길게 청산
                //triggerPrice = emaWrapper._highestHigh_3 * 1.003m;
                //triggerPrice = _totalAveragePrice * 1.055m;
                triggerPrice = _totalAveragePrice * 1.015m;            
            else if (_nEnterMode.Value == 1)
                //triggerPrice = _totalAveragePrice * 1.01m;
                triggerPrice = _totalAveragePrice * 1.02m;
            else if (_nEnterMode.Value == 61)
            {
                side = "sell";
                //triggerPrice = triggerPrice - (triggerPrice * 0.05m);                
                //triggerPrice = triggerPrice - (triggerPrice * 0.10m);
                /////triggerPrice = emaWrapper._lowestLow_15 * 0.992m; // 15선 저점 밑으로 0.8%
                //triggerPrice = _totalAveragePrice * 0.975m;
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.025m);
                /////triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.015m);

                triggerPrice = emaWrapper._bb_lower;

            }
            else if (_nEnterMode.Value == 62)
            {
                //triggerPrice = triggerPrice + (triggerPrice * 0.05m);
                //triggerPrice = triggerPrice + (triggerPrice * 0.10m);
                //triggerPrice = emaWrapper._highestHigh_15 * 1.008m; // 15선 고점 위 0.8%
                //triggerPrice = _totalAveragePrice * 1.025m;
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.025m);
                /////triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.015m);
                ///

                triggerPrice = emaWrapper._bb_upper;

            }
            else if (_nEnterMode.Value == 71)// 7는 짧게 청산!
            {
                side = "sell";
                //triggerPrice = emaWrapper._ema50 * 1.02m;               
                //triggerPrice = triggerPrice + (triggerPrice * 0.03m);
                //triggerPrice = triggerPrice + (triggerPrice * 0.012m);
                /////triggerPrice = emaWrapper._highestHigh_3 * 1.003m;
                /////triggerPrice = triggerPrice + (triggerPrice * 0.009m);
                ///

                /*
                //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 : {emaWrapper._lastPrice_beforeCandle_old2} {_totalAveragePrice}   {((emaWrapper._lastPrice_beforeCandle_old - _totalAveragePrice) / _totalAveragePrice * 100m).ToString()} %", 0);
                if (((_totalAveragePrice - emaWrapper._lastPrice_beforeCandle_old2) / _totalAveragePrice * 100m) > 4)
                {
                    triggerPrice = _totalAveragePrice * (1 - 0.025m);
                    //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - > 4 : {_totalAveragePrice} {triggerPrice}", 0);
                }
                else
                {
                    //triggerPrice = emaWrapper._lastPrice_beforeCandle_old * 1.005m;
                    triggerPrice = emaWrapper._lastPrice_beforeCandle_old2;
                    //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - < 4 : {emaWrapper._lastPrice_beforeCandle_old2}", 0);
                }
                */

                //triggerPrice = emaWrapper._highestHigh * 1.003m;
                //triggerPrice = _totalAveragePrice * 1.025m;

                triggerPrice = _totalAveragePrice * (1 - 0.05m);

                //triggerPrice = emaWrapper._lastPrice_beforeCandle * 1.005m;

            }
            else if (_nEnterMode.Value == 72)// 7는 짧게 청산!
            {
                //triggerPrice = emaWrapper._ema50 * 1.02m;               
                //triggerPrice = triggerPrice + (triggerPrice * 0.03m);
                //triggerPrice = triggerPrice + (triggerPrice * 0.012m);
                /////triggerPrice = emaWrapper._highestHigh_3 * 1.003m;
                /////triggerPrice = triggerPrice + (triggerPrice * 0.009m);
                ///

                /*
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 : {emaWrapper._lastPrice_beforeCandle_old2} {_totalAveragePrice}   {((emaWrapper._lastPrice_beforeCandle_old - _totalAveragePrice) / _totalAveragePrice * 100m).ToString()} %", 0);
                if (((emaWrapper._lastPrice_beforeCandle_old2 - _totalAveragePrice) / _totalAveragePrice * 100m) > 4)
                {
                    triggerPrice = _totalAveragePrice * 1.025m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - > 4 : {_totalAveragePrice} {triggerPrice}", 0);
                }
                else
                {
                    //triggerPrice = emaWrapper._lastPrice_beforeCandle_old * 1.005m;
                    triggerPrice = emaWrapper._lastPrice_beforeCandle_old2;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 손절설정 - < 4 : {emaWrapper._lastPrice_beforeCandle_old2}", 0);
                }
                */

                //triggerPrice = emaWrapper._highestHigh * 1.003m;
                //triggerPrice = _totalAveragePrice * 1.025m;

                triggerPrice = _totalAveragePrice * (1 + 0.05m);

                //triggerPrice = emaWrapper._lastPrice_beforeCandle * 1.005m;

            }
            else if (_nEnterMode.Value == 5)// 5는 짧게 청산!
            {
                /////triggerPrice = _totalAveragePrice * 1.009m; 
                triggerPrice = emaWrapper._highestHigh_3 * 1.005m;
            }
            else if (_nEnterMode.Value == 41) // 상방진입
            {
                side = "sell";
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.1m);
                //triggerPrice = emaWrapper._bb_middle;
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.05m);
                triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.15m);

            }
            else if (_nEnterMode.Value == 42) // 하방진입
            {
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.1m);
                //triggerPrice = emaWrapper._bb_middle;
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.05m);
                triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.15m);
            }
            else if (_nEnterMode.Value == 91) // 상방진입
            {
                side = "sell";
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.1m);
                //triggerPrice = emaWrapper._bb_middle;

                //if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
                /*
                if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 17)
                        triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.05m);
                else
                    triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.03m);
                    */

                
                triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.07m);
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.027m);

                
                if ((_totalAveragePrice - emaWrapper._lowestLow_10)/ emaWrapper._lowestLow_10 * 100 < 7)
                {
                    triggerPrice = emaWrapper._lowestLow_10 - (_totalAveragePrice * 0.005m);
                }
                
                /*
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.1m);
                triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.15m);

                if (globalWrapper._detectedPercent >= 90 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 11))
                    triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.05m);
                */
            }
            else if (_nEnterMode.Value == 92) // 하방진입
            {
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.1m);
                //triggerPrice = emaWrapper._bb_middle;

                //if(DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
                /*
                if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 17)
                        triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.05m);
                else
                    triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.03m);
                    */

                
                triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.07m);
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.027m);

                if ((emaWrapper._highestHigh_10 - _totalAveragePrice) / _totalAveragePrice * 100 < 7)
                {
                    triggerPrice = emaWrapper._highestHigh_10  + (_totalAveragePrice * 0.005m);
                }
                
                /*
                //triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.1m);
                triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.15m);

                if (globalWrapper._detectedPercent >= 90 || (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 11))
                    triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.05m);
                */
            }//
            else if (_nEnterMode.Value == 81) // 상방진입
            {
                side = "sell";
                //triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.015m);
                triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.015m); // 3
            }
            else if (_nEnterMode.Value == 82) // 하방진입
            {
                triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.015m);
            }
            else if (_nEnterMode.Value == 11) // 상방진입
            {
                side = "sell";              
                triggerPrice = _totalAveragePrice - (_totalAveragePrice * 0.025m);
            }
            else if (_nEnterMode.Value == 12) // 하방진입
            {             
                triggerPrice = _totalAveragePrice + (_totalAveragePrice * 0.025m);
            }

            // 손절선 설정


            if (side != "sell")
            {
                if (triggerPrice < lastPrice * 1.003m)
                {
                    triggerPrice = lastPrice * 1.005m;
                }
            }
            else
            {
                if (triggerPrice > lastPrice * 0.997m)
                {
                    triggerPrice = lastPrice * 0.995m;
                }
            }





            //triggerPrice = triggerPrice + (triggerPrice * 0.002m);
            triggerPrice = Math.Floor(triggerPrice / _tickSize) * _tickSize;
            //triggerPrice = Math.Round(triggerPrice, _decimalPlaces, MidpointRounding.ToZero);
            triggerPrice = Math.Truncate(triggerPrice * (decimal)Math.Pow(10, _decimalPlaces)) / (decimal)Math.Pow(10, _decimalPlaces);



            // 지정가는 의미 없으니 크게 설정 (시장가 방식)
            decimal stoplossTriggerPrice = triggerPrice + (_tickSize + 20); // 비싸게 사겠다.

            string url = "https://api.bitget.com/api/v2/mix/order/place-plan-order";
            string method = "POST";
            string requestPath = "/api/v2/mix/order/place-plan-order";


            string productType = "USDT-FUTURES";
            string marginCoin = "usdt";


            //string body = $"{{ \"planType\":\"normal_plan\", \"symbol\": \"{threadSymbol.Value}\", \"productType\": \"USDT-FUTURES\", \"marginMode\": \"isolated\", \"marginCoin\": \"USDT\", \"size\": \"{quantity}\", \"triggerType\": \"fill_price\", \"side\": \"sell\", \"orderType\":\"market\", \"TriggerPrice\": \"{triggerPrice}\", \"stoplossTriggerPrice\": \"{stoplossTriggerPrice}\", \"stopLossTriggerType\": \"fill_price\" }}";
            //string body = $"{{ \"planType\":\"normal_plan\", \"symbol\": \"{threadSymbol.Value}\", \"productType\": \"USDT-FUTURES\", \"marginMode\": \"isolated\", \"marginCoin\": \"USDT\", \"size\": \"{quantity}\", \"triggerType\": \"fill_price\", \"side\": \"sell\", \"orderType\":\"market\", \"TriggerPrice\": \"{triggerPrice}\", \"stoplossTriggerPrice\": \"{stoplossTriggerPrice}\", \"stopLossTriggerType\": \"fill_price\" }}";
            string body = $"{{ \"planType\":\"normal_plan\", \"symbol\": \"{emaWrapper._symbol}\", \"productType\": \"USDT-FUTURES\", \"marginMode\": \"isolated\", \"marginCoin\": \"USDT\", \"size\": \"{quantity}\", \"triggerType\": \"fill_price\", \"side\": \"{side}\", \"orderType\":\"market\", \"TriggerPrice\": \"{triggerPrice}\", \"stopLossTriggerType\": \"fill_price\" }}";
            //form.Logger( $"{body}");

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string prehash = timestamp + method + requestPath + body;

            string sign = Sign(prehash, apiSecret);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ACCESS-KEY", apiKey);
                client.DefaultRequestHeaders.Add("ACCESS-SIGN", sign);
                client.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", timestamp);
                client.DefaultRequestHeaders.Add("ACCESS-PASSPHRASE", passphrase);
                client.DefaultRequestHeaders.Add("locale", "en-US");

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                await Task.Delay(500);
                string result = await response.Content.ReadAsStringAsync();

                if (result.Contains("success"))
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ▣ StopLoss 주문 성공. {emaWrapper._symbol}, {triggerPrice}, {quantity}");
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     -> {result}");
                }
                else
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StopLoss 주문 실패.... : {triggerPrice}, {result}", 0);
                }
            }
        }

        // stoploss 설정
        public static async Task setStopLoss_history(Form_main form, string apiKey, string apiSecret, string passphrase)
        {
            string symbol = "PIEVERSEUSDT";
            string productType = "USDT-FUTURES";
            string planType = "normal_plan"; // ★ 필수

            string query = $"symbol={symbol}&productType={productType}&planType={planType}";

            // 1) 실제 요청 URL
            string url = $"https://api.bitget.com/api/v2/mix/order/orders-plan-history?{query}";

            // 2) 서명 prehash에 들어갈 requestPath
            string requestPath = $"/api/v2/mix/order/orders-plan-history?{query}";
            //string requestPath = $"/api/v2/mix/order/orders-plan-history";
            

            string method = "GET";
            string body = "";

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // 3) prehash = timestamp + method + requestPath + body
            string prehash = timestamp + method + requestPath + body;

            string sign = Sign(prehash, apiSecret);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ACCESS-KEY", apiKey);
                client.DefaultRequestHeaders.Add("ACCESS-SIGN", sign);
                client.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", timestamp);
                client.DefaultRequestHeaders.Add("ACCESS-PASSPHRASE", passphrase);
                client.DefaultRequestHeaders.Add("locale", "en-US");

                var response = await client.GetAsync(url);

                string result = await response.Content.ReadAsStringAsync();

                if (result.Contains("success"))
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ▣ StopLoss_history 요청 성공.");
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     -> {result}");
                }
                else
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StopLoss 주문 실패.... :  {result}", 0);
                }
            }
        }

        static string Sign(string message, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(hashValue);
            }
        }


        public static SheetsService CreateService(string serviceAccountJsonPath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountJsonPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }



        public static void addDataToGoogleSheet(Form_main form, string rawData)
        {
            //await Task.Delay(500);
            //string serviceAccountJsonPath = @"D:\whsunbi\google_drive_api_key\sharcruz-50cfdffc0bd5.json";
            string serviceAccountJsonPath = @"C:\sharcruz-50cfdffc0bd5.json";
            string spreadsheetId = "1K09q0Wf1nzJyMYjqORRRSYC1Y7gO028QdgshflkBzE4";


            SheetsService service = CreateService(serviceAccountJsonPath);
            string sheetName = "result";


            if (_existingKeys.Count() > 0)
            {
                // by-pass
            }
            else
            {
                // ✅ A열만 읽기 (전체 대신 A열만 조회 → 요청량 절약)
                var readRange = $"{sheetName}!A:A";
                var request = service.Spreadsheets.Values.Get(spreadsheetId, readRange);
                var response = request.Execute();
                var values = response.Values ?? new List<IList<object>>();


                foreach (var r in values)
                {
                    if (r.Count > 0 && DateTime.TryParse(r[0]?.ToString(), out var dt))
                    {
                        _existingKeys.Add(dt);
                    }
                }
            }

            // ✅ 새 데이터 처리
            var rowDataList = rawData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rowData in rowDataList)
            {
                string firstColValueStr = rowData.Split(',')[0].Trim();
                if (!DateTime.TryParse(firstColValueStr, out var firstColDateTime))
                {
                    form.Logger($"⚠️ 날짜 형식 오류: {firstColValueStr}");
                    continue;
                }

                if (!_existingKeys.Contains(firstColDateTime))
                {
                    var columns = rowData.Split(',');
                    var newRow = new ValueRange()
                    {
                        Values = new[] { columns.Cast<object>().ToList() }
                    };

                    var appendRequest = service.Spreadsheets.Values.Append(newRow, spreadsheetId, $"{sheetName}!A:Z");
                    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                    appendRequest.Execute();

                    form.Logger($"▲ 구글시트에 추가완료: {firstColValueStr}");

                    _existingKeys.Add(firstColDateTime); // 중복 방지
                }
                else
                {
                    //form.Logger( $"⚠️ 이미 존재: {firstColValueStr}");
                }
            }
        }

        public static decimal addDataToGoogleSheet_monitoring(decimal _mode, Form_main form, string coinName)
        {
            string serviceAccountJsonPath = @"C:\sharcruz-50cfdffc0bd5.json";
            string spreadsheetId = "1K09q0Wf1nzJyMYjqORRRSYC1Y7gO028QdgshflkBzE4";

            SheetsService service = CreateService(serviceAccountJsonPath);
            string sheetName = "monitoring";

            // 1. trade 시트 A1:A3 읽기
            var range = $"{sheetName}!A1:A3";
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var values = response.Values ?? new List<IList<object>>();

            // 2. 이미 코인명이 존재하는지 확인
            int i = 0;
            foreach (var row in values)
            {
                i++;
                if (threadId.Value == i)
                    continue;

                if (row.Count > 0 && row[0].ToString().Trim().Equals(coinName, StringComparison.OrdinalIgnoreCase))
                {
                    //form.Logger( $"⚠️ 이미 존재: {i.ToString()}, {coinName}");
                    return decimal.Parse(i.ToString());
                }
            }

            // 3. mode행 위치 계산 (mode=1→A1, mode=2→A2, mode=3→A3)
            string targetRange = $"{sheetName}!A{_mode}";

            // 4. 코인명 기록
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { coinName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } }
            };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, targetRange);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            updateRequest.Execute();

            form.Logger($"  ☞ 모니터링 기록 : {targetRange}  {coinName}");
            return 0; // 성공
        }

        public static decimal addMonitoring(Form_main form, decimal id, string coinName)
        {
            decimal result = 0; // 기본값 (성공 시 0 반환)

            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() =>
                {
                    // 이미 등록된 경우 → 해당 id 반환
                    if (coinName == form.tbCoin1.Text) { result = 1; return; }
                    else if (coinName == form.tbCoin2.Text) { result = 2; return; }
                    else if (coinName == form.tbCoin3.Text) { result = 3; return; }
                    else if (coinName == form.tbCoin4.Text) { result = 4; return; }
                    else if (coinName == form.tbCoin5.Text) { result = 5; return; }

                    // 새로 등록
                    if (id == 1) form.tbCoin1.Text = coinName;
                    else if (id == 2) form.tbCoin2.Text = coinName;
                    else if (id == 3) form.tbCoin3.Text = coinName;
                    else if (id == 4) form.tbCoin4.Text = coinName;
                    else if (id == 5) form.tbCoin5.Text = coinName;

                    form.Logger($"  ☞ 모니터링 기록 : {id} -> {coinName}");
                }));
            }
            else
            {
                if (coinName == form.tbCoin1.Text) return 1;
                else if (coinName == form.tbCoin2.Text) return 2;
                else if (coinName == form.tbCoin3.Text) return 3;
                else if (coinName == form.tbCoin4.Text) return 4;
                else if (coinName == form.tbCoin5.Text) return 5;

                if (id == 1) form.tbCoin1.Text = coinName;
                else if (id == 2) form.tbCoin2.Text = coinName;
                else if (id == 3) form.tbCoin3.Text = coinName;
                else if (id == 4) form.tbCoin4.Text = coinName;
                else if (id == 5) form.tbCoin5.Text = coinName;

                form.Logger($"  ☞ 모니터링 기록 : {id} -> {coinName}");
            }

            return result;
        }

        public static decimal resetMonitoring(Form_main form, decimal id)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() =>
                {
                    if (id == 1) form.tbCoin1.Text = "";
                    else if (id == 2) form.tbCoin2.Text = "";
                    else if (id == 3) form.tbCoin3.Text = "";
                    else if (id == 4) form.tbCoin4.Text = "";
                    else if (id == 5) form.tbCoin5.Text = "";

                }));
            }

            return 0; // 성공
        }

        public async static Task SendMessage(string chatId, string message, string token)
        {
            // https://api.telegram.org/bot202902166:AAFLRiEQ9mWu_Cbgis_nTqlJN2LWqSJSafs/sendMessage?chat_id=106224234&text=%F0%9F%91%8D%0A%0C가나다%0A%0C라마바 %F0%9F%92%9B // 크롬에서 URL에 이모티콘 이미지 그대로 붙여넣기 -> 피들러 확인!            
            var url = string.Format("https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}", token, chatId, message);

            using (var client = new HttpClient())
            {
                //await client.PostAsync(url, form);
                await client.GetAsync(url);

            }
        }

        public static async Task<bool> SetStopLossAsync(
        Form_main form,
        BitgetRestClient client,
        decimal filledPrice,
        decimal totalQty,
        decimal _tickSize
        )
        {
            try
            {
                int _decimalPlaces = BitConverter.GetBytes(decimal.GetBits(_tickSize)[3])[2];

                // 트리거 가격 계산
                decimal triggerPrice = filledPrice + (filledPrice * 0.01m);
                triggerPrice = Math.Floor(triggerPrice / _tickSize) * _tickSize;
                //triggerPrice = Math.Round(triggerPrice, _decimalPlaces.Value, MidpointRounding.ToZero);
                triggerPrice = Math.Truncate(triggerPrice * (decimal)Math.Pow(10, _decimalPlaces)) / (decimal)Math.Pow(10, _decimalPlaces);

                // 지정가는 의미 없으니 크게 설정 (시장가 방식)
                decimal limitPrice = triggerPrice + (_tickSize * 20); // 비싸게 사겠다.

                var slOrder = await client.FuturesApiV2.Trading.SetPositionTpSlAsync(
                    productType: BitgetProductTypeV2.UsdtFutures,
                    marginAsset: "USDT",
                    symbol: threadSymbol.Value,
                    holdSide: PositionSide.Short,
                    slTriggerPrice: triggerPrice,
                    slLimitPrice: limitPrice,
                    slTriggerQuantity: totalQty,
                    slTriggerType: TriggerPriceType.LastPrice
                );

                if (slOrder.Success)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ▣ StopLoss 주문 성공. triggerPrice: {triggerPrice}, limitPrice: {limitPrice}");
                    return true;
                }
                else
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StopLoss 주문 실패: {slOrder.Error}", 0);
                    return false;
                }
            }
            catch (Exception ex)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StopLoss 함수 오류: {ex.Message}", 0);
                return false;
            }
        }

        public static async Task<bool> checkPosition(Form_main form, BitgetRestClient client, int id, string orderId, string symbol)
        {
            try
            {
                // 포지션 조회
                var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                    productType: BitgetProductTypeV2.UsdtFutures,
                    marginAsset: "USDT"
                );

                if (!positions.Success)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 포지션 조회 실패: {positions.Error}", 0);
                    return false;
                }


                // 특정 코인 포지션 확인
                var targetPosition = positions.Data.FirstOrDefault(p =>
                p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));




                if (positions.Data == null || !positions.Data.Any() || targetPosition == null)
                {
                    form.Logger("");
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 포지션이 모두 청산되었습니다.");
                    form.Logger("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                    form.Logger("");

                    await Task.Delay(1000);

                    // OPEN : 미체결 주문 조회
                    var openOrders = await client.FuturesApiV2.Trading.GetOpenOrdersAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        symbol: threadSymbol.Value
                    );

                    if (!openOrders.Success)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN : 미체결 주문조회 실패: {openOrders.Error}");
                        return true;
                    }
                    else
                    {

                        // 실제 주문 리스트 확인 후 취소
                        if (openOrders.Data != null)
                        {
                            foreach (var o in openOrders.Data.Orders)
                            {
                                var cancelResult = await client.FuturesApiV2.Trading.CancelOrderAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures,
                                    symbol: threadSymbol.Value,
                                    orderId: o.OrderId
                                );

                                if (cancelResult.Success)
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN :주문취소 성공, {o.OrderId}");
                                else
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN :주문취소 실패... {o.OrderId} : {cancelResult.Error}");
                            }
                        }
                    }

                    await Task.Delay(100);

                    /*
                    // 트레일링 스탑 : 미체결 주문 조회
                    var traillingStopOrders = await client.FuturesApiV2.Trading.GetOpenTriggerOrdersAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        planType: TriggerPlanTypeFilter.TrailingStop
                    );

                    if (!traillingStopOrders.Success)
                    {
                        form.Logger( $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TrailingStop : 미체결 주문조회 실패: {traillingStopOrders.Error}");
                        return true;
                    }
                    else
                    {

                        // 실제 주문 리스트 확인 후 취소
                        if (traillingStopOrders.Data != null && traillingStopOrders.Data.Orders != null)
                        {
                            foreach (var o in traillingStopOrders.Data.Orders)
                            {
                                var cancelResult = await client.FuturesApiV2.Trading.CancelTriggerOrdersAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures
                                //symbol: threadSymbol.Value,
                                //orderId: o.OrderId
                                //planType: CancelTriggerPlanTypeFilter.Trailing
                                );

                                if (cancelResult.Success)
                                    form.Logger( $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TrailingStop : 주문취소 성공, {o.OrderId}");
                                else
                                    form.Logger( $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TrailingStop : 주문취소 실패... {o.OrderId} : {cancelResult.Error}");
                            }
                        }
                    }
                    */

                    await Task.Delay(500);

                    // 스탑로스 : 미체결 주문 조회
                    var stopLossOrders = await client.FuturesApiV2.Trading.GetOpenTriggerOrdersAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        planType: TriggerPlanTypeFilter.Trigger,
                        symbol: symbol
                    );

                    if (!stopLossOrders.Success)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] stopLoss : 미체결 주문조회 실패: {stopLossOrders.Error}");
                        return true;
                    }
                    else
                    {

                        // 실제 주문 리스트 확인 후 취소
                        if (stopLossOrders.Data != null && stopLossOrders.Data.Orders != null)
                        {
                            foreach (var o in stopLossOrders.Data.Orders)
                            {
                                var cancelResult = await client.FuturesApiV2.Trading.CancelTriggerOrdersAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures
                                //symbol: threadSymbol.Value,
                                //orderId: o.OrderId
                                );

                                if (cancelResult.Success)
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] stopLoss : 주문취소 성공, {o.OrderId}");
                                else
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] stopLoss : 주문취소 실패... {o.OrderId} : {cancelResult.Error}");
                            }
                        }
                    }

                    return true; // 포지션이 없어서 주문취소 완료
                }
                else
                {



                    // 아직 포지션 보유 중
                    //Logger(".");
                    //return false;

                    form.IncrementProgress(form, id, 2);
                }
            }
            catch (Exception ex)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 함수 실행 오류: {ex.Message}");
                //return false;
            }

            ///
            if (orderId != "")
            {
                var orderInfo = await client.FuturesApiV2.Trading.GetOrderAsync(
                       productType: BitgetProductTypeV2.UsdtFutures,
                       symbol: threadSymbol.Value,
                       orderId: orderId
                   );

                if (!orderInfo.Success)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Open 체결결과 - 주문조회 실패: {orderInfo.Error}");
                    await Task.Delay(100);
                    return false;
                }

                var status = orderInfo.Data.Status;
                //form.Logger( $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 주문 상태 확인: {status}");
                //Logger($"%");

                if (status == OrderStatus.Filled)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CLOSE 주문 체결 완료! (청산 완료)");

                    // ✅ 혹시 포지션이 남아있으면 모두 청산
                    var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        marginAsset: "USDT"
                    );

                    if (positions.Success && positions.Data != null && positions.Data.Any())
                    {
                        foreach (var pos in positions.Data)
                        {
                            if (pos.Total > 0) // 남아있는 포지션이 있다면
                            {
                                var side = pos.PositionSide == PositionSide.Long ? OrderSide.Sell : OrderSide.Buy;

                                var forceClose = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures,
                                    symbol: pos.Symbol,
                                    marginAsset: "USDT",
                                    side: side,
                                    type: OrderType.Market,
                                    marginMode: MarginMode.IsolatedMargin,
                                    quantity: pos.Total,    // 보유 수량 전부 청산
                                    tradeSide: TradeSide.Close
                                );

                                if (forceClose.Success)
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 남은 포지션 강제 청산 성공, Symbol={pos.Symbol}, Qty={pos.Total}");
                                else
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 남은 포지션 강제 청산 실패, Symbol={pos.Symbol}, Error={forceClose.Error}");
                            }
                        }
                    }

                    return true;
                }
                else if (status == OrderStatus.Canceled || status == OrderStatus.Rejected)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CLOSE 주문 실패 상태 ({status})", 0);
                    //return false;
                    return true;
                }
            }

            //form.Logger( "==================================================================");
            //form.Logger( "");

            //await Task.Delay(100); // 0.5초 대기 후 재확인
            await Task.Delay(500); // 
            return false;
        }
        /*
        public static decimal GetCurrentProfitRate(BitgetRestClient client)
        {
            try
            {
               
   
                //decimal entryPrice = position.AverageOpenPrice; // 평균 진입가
                //decimal markPrice = position.MarkPrice;        // 현재 가격

                if (_totalAveragePrice.Value == 0 || _lastPrice == 0)
                    return -1m;

                // 수익률 계산
                decimal profitRate = 0m;
                profitRate = (_totalAveragePrice.Value - _lastPrice) / _totalAveragePrice.Value * 100m;
                profitRate = Math.Round(profitRate, 2, MidpointRounding.AwayFromZero);

                return profitRate;
            }
            catch (Exception ex)
            {
                //form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 에러 발생: {ex.Message}");
                return -1m;
            }
        }
        */

        public static decimal GetCurrentProfitRate(Form_main form, BitgetRestClient client, GlobalWrapper globalWrapper, decimal _lastPrice, decimal _totalAveragePrice)
        {
            try
            {
                /*
                // 현재 포지션 조회
                var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                    productType: BitgetProductTypeV2.UsdtFutures,
                    marginAsset: "USDT"
                );

                if (!positions.Success || positions.Data == null)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 포지션 조회 실패: {positions.Error}");
                    return -1m;
                }

                // 해당 심볼 포지션 찾기
                var position = positions.Data.FirstOrDefault(p => p.Symbol == threadSymbol.Value);
                if (position == null)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 현재 보유 중인 포지션 없음.");
                    return -1m;
                }
                */
                //decimal entryPrice = position.AverageOpenPrice; // 평균 진입가
                //decimal markPrice = position.MarkPrice;        // 현재 가격

                if (_totalAveragePrice <= 0 || _lastPrice <= 0)
                    return -1m;

                // 수익률 계산
                decimal profitRate = 0m;

                if (globalWrapper._side == OrderSide.Sell) // 숏
                    profitRate = (_totalAveragePrice - _lastPrice) / _totalAveragePrice * 100m;
                else if(globalWrapper._side == OrderSide.Buy) // 롱
                    profitRate = (_lastPrice - _totalAveragePrice) / _totalAveragePrice * 100m;
                else
                    return -1m;

                profitRate = Math.Round(profitRate, 2, MidpointRounding.AwayFromZero);

                if (profitRate >= 50)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 수익률 >= 50   {globalWrapper._side.ToString()} :: {_lastPrice} - {_totalAveragePrice} => {profitRate}", -1);
                    return -1m;
                }

                return profitRate;
            }
            catch (Exception ex)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 에러 발생: {ex.Message}");
                return -1m;
            }
        }

        public static async Task setTrailingStopopAsync(
         Form_main form,
         BitgetRestClient client,
         decimal triggerPrice,   // 발동 기준 가격
         decimal quantity       // 청산 수량
         )
        {
            try
            {
                var result = await client.FuturesApiV2.Trading.PlaceTriggerOrderAsync(
                 productType: BitgetProductTypeV2.UsdtFutures,
                 symbol: threadSymbol.Value,
                 marginAsset: "USDT",
                 planType: TriggerPlanType.TrailingStop,
                 marginMode: MarginMode.IsolatedMargin,
                 side: OrderSide.Buy,
                 orderType: OrderType.Market,

                 quantity: quantity,
                 triggerPrice: triggerPrice, // 감시시작 가격             
                 triggerPriceType: TriggerPriceType.LastPrice,
                 //stopLossTriggerPrice: triggerPrice,
                 trailingStopRate: 0.5m// 0.7m//1.0m//0.5m
                                       //stopLossPriceType: TriggerPriceType.MarkPrice
                                       //triggerPriceType:TriggerPriceType.LastPrice
                                       //tradeSide: TradeSide.Open

             );


                if (result.Success)
                {
                    //_orderId_trailingStop = result.Data.OrderId;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]     ▣ TrailingStop 주문 성공: {threadSymbol.Value}, {triggerPrice}");
                }
                else
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TrailingStop 스탑 주문 실패: {result.Error}", 0);
                }
            }
            catch (Exception ex)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TrailingStop 스탑 주문 에러: {ex.Message}");
            }
        }

        public static async Task CloseAllPositions(Form_main form, BitgetRestClient client, string symbol, string strSide = "")
        {
            // 현재 포지션 전체 조회
            // 전체 포지션 조회
            var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
               productType: BitgetProductTypeV2.UsdtFutures,
               marginAsset: "USDT"
            );

            if (!positions.Success)
            {
                form.Logger("포지션 조회 실패: " + positions.Error);
                return;
            }

            foreach (var pos in positions.Data)
            {
                //if (pos.Available > 0) // 열린 포지션이 있는 경우
                {
                    if (strSide == "" || pos.PositionSide.ToString().ToLower() == strSide)
                    {
                        var result = await client.FuturesApiV2.Trading.ClosePositionsAsync(
                            productType: BitgetProductTypeV2.UsdtFutures,
                            symbol: symbol
                        );

                        if (result.Success)
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {pos.Symbol} 전체 포지션 올청산 완료");
                        else
                            form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {pos.Symbol} 전체 포지션 올청산 실패: {result.Error}");
                    }
                }
            }

            await Task.Delay(300);
        }

        public static async Task setProgress(Form_main form, decimal id, bool bTurn)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => setProgress(form, id, bTurn)));
                //return;
            }

            // UI 스레드에서 안전하게 컨트롤 접근
            if (id == 1)
            {
                //form.pbProgress1.Visible = bTurn;                   
                form.krypton_pgStatus1.Value = 0;
                form.krypton_pgStatus1.Visible = bTurn;

                if (bTurn == true)
                {
                    form.tbCoin1.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.tbCoin1.StateCommon.Content.Color1 = System.Drawing.Color.White;
                }
                else
                {
                    form.tbCoin1.StateCommon.Back.Color1 = System.Drawing.Color.White;
                    form.tbCoin1.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
                }


            }
            else if (id == 2)
            {
                form.krypton_pgStatus2.Value = 0;
                form.krypton_pgStatus2.Visible = bTurn;

                if (bTurn == true)
                {
                    form.tbCoin2.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.tbCoin2.StateCommon.Content.Color1 = System.Drawing.Color.White;
                }
                else
                {
                    form.tbCoin2.StateCommon.Back.Color1 = System.Drawing.Color.White;
                    form.tbCoin2.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
                }
            }
            else if (id == 3)
            {
                form.krypton_pgStatus3.Value = 0;
                form.krypton_pgStatus3.Visible = bTurn;

                if (bTurn == true)
                {
                    form.tbCoin3.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.tbCoin3.StateCommon.Content.Color1 = System.Drawing.Color.White;
                }
                else
                {
                    form.tbCoin3.StateCommon.Back.Color1 = System.Drawing.Color.White;
                    form.tbCoin3.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
                }
                //tbCoin3.Refresh();
            }
            else if (id == 4)
            {
                form.krypton_pgStatus4.Value = 0;
                form.krypton_pgStatus4.Visible = bTurn;

                if (bTurn == true)
                {
                    form.tbCoin4.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.tbCoin4.StateCommon.Content.Color1 = System.Drawing.Color.White;
                }
                else
                {
                    form.tbCoin4.StateCommon.Back.Color1 = System.Drawing.Color.White;
                    form.tbCoin4.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
                }
            }
            else if (id == 5)
            {
                form.krypton_pgStatus5.Value = 0;
                form.krypton_pgStatus5.Visible = bTurn;

                if (bTurn == true)
                {
                    form.tbCoin5.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.tbCoin5.StateCommon.Content.Color1 = System.Drawing.Color.White;
                }
                else
                {
                    form.tbCoin5.StateCommon.Back.Color1 = System.Drawing.Color.White;
                    form.tbCoin5.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
                }
            }
        }

        public static async Task setProgress_reset(Form_main form, decimal id)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => setProgress_reset(form, id)));
                return;
            }

            // UI 스레드에서 안전하게 컨트롤 접근
            if (id == 1)
            {
                //form.pbProgress1.Visible = bTurn;                   
                form.krypton_pgStatus1.Value = 0;
                form.krypton_pgStatus1.Visible = false;
                form.krypton_pgStatus1.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                form.krypton_pgStatus1.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                form.tbCoin1.StateCommon.Back.Color1 = System.Drawing.Color.White;
                form.tbCoin1.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
            }
            else if (id == 2)
            {
                form.krypton_pgStatus2.Value = 0;
                form.krypton_pgStatus2.Visible = false;
                form.krypton_pgStatus2.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                form.krypton_pgStatus2.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                form.tbCoin2.StateCommon.Back.Color1 = System.Drawing.Color.White;
                form.tbCoin2.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
            }
            else if (id == 3)
            {
                form.krypton_pgStatus3.Value = 0;
                form.krypton_pgStatus3.Visible = false;
                form.krypton_pgStatus3.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                form.krypton_pgStatus3.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                form.tbCoin3.StateCommon.Back.Color1 = System.Drawing.Color.White;
                form.tbCoin3.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
            }
            else if (id == 4)
            {
                form.krypton_pgStatus4.Value = 0;
                form.krypton_pgStatus4.Visible = false;
                form.krypton_pgStatus4.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                form.krypton_pgStatus4.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                form.tbCoin4.StateCommon.Back.Color1 = System.Drawing.Color.White;
                form.tbCoin4.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
            }
            else if (id == 5)
            {
                form.krypton_pgStatus5.Value = 0;
                form.krypton_pgStatus5.Visible = false;
                form.krypton_pgStatus5.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                form.krypton_pgStatus5.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                form.tbCoin5.StateCommon.Back.Color1 = System.Drawing.Color.White;
                form.tbCoin5.StateCommon.Content.Color1 = System.Drawing.Color.DimGray;
            }
        }

        public void IncrementProgress(Form_main form, decimal id, decimal step)
        {

            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => IncrementProgress(form, id, step)));
            }
            else
            {
                if (id == 1)
                {
                    form.krypton_pgStatus1.Value++;
                    if (form.krypton_pgStatus1.Value >= form.krypton_pgStatus1.Maximum)
                        form.krypton_pgStatus1.Value = 0;

                    if (step == 1 && form.krypton_pgStatus1.StateCommon.Back.Color1 != System.Drawing.Color.Green)
                    {
                        form.krypton_pgStatus1.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.krypton_pgStatus1.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;
                        //this.krypton_pgStatus1.Text = "포착 중...";
                        //this.krypton_pgStatus1.Values.Text = "포착 중...";                        
                        form.tbCoin1.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.tbCoin1.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (step == 2 && form.krypton_pgStatus1.StateCommon.Back.Color1 != System.Drawing.Color.OrangeRed)
                    {
                        form.krypton_pgStatus1.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.krypton_pgStatus1.StateCommon.Back.Color2 = System.Drawing.Color.Orange;
                        //this.krypton_pgStatus1.Text = "히트...";
                        //this.krypton_pgStatus1.Values.Text = "히트...";
                        form.tbCoin1.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbCoin1.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                }
                else if (id == 2)
                {
                    form.krypton_pgStatus2.Value++;
                    if (form.krypton_pgStatus2.Value >= form.krypton_pgStatus2.Maximum)
                        form.krypton_pgStatus2.Value = 0;

                    form.krypton_pgStatus2.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.krypton_pgStatus2.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                    if (step == 1 && form.krypton_pgStatus2.StateCommon.Back.Color1 != System.Drawing.Color.Green)
                    {
                        form.krypton_pgStatus2.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.krypton_pgStatus2.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;
                        form.tbCoin2.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.tbCoin2.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (step == 2 && form.krypton_pgStatus2.StateCommon.Back.Color1 != System.Drawing.Color.OrangeRed)
                    {
                        form.krypton_pgStatus2.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.krypton_pgStatus2.StateCommon.Back.Color2 = System.Drawing.Color.Orange;
                        form.tbCoin2.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbCoin2.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                }
                else if (id == 3)
                {
                    form.krypton_pgStatus3.Value++;
                    if (form.krypton_pgStatus3.Value >= form.krypton_pgStatus3.Maximum)
                        form.krypton_pgStatus3.Value = 0;

                    form.krypton_pgStatus3.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.krypton_pgStatus3.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                    if (step == 1 && form.krypton_pgStatus3.StateCommon.Back.Color1 != System.Drawing.Color.Green)
                    {
                        form.krypton_pgStatus3.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.krypton_pgStatus3.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;
                        form.tbCoin3.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.tbCoin3.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (step == 2 && form.krypton_pgStatus3.StateCommon.Back.Color1 != System.Drawing.Color.OrangeRed)
                    {
                        form.krypton_pgStatus3.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.krypton_pgStatus3.StateCommon.Back.Color2 = System.Drawing.Color.Orange;
                        form.tbCoin3.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbCoin3.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                }
                else if (id == 4)
                {
                    form.krypton_pgStatus4.Value++;
                    if (form.krypton_pgStatus4.Value >= form.krypton_pgStatus3.Maximum)
                        form.krypton_pgStatus4.Value = 0;

                    form.krypton_pgStatus4.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.krypton_pgStatus4.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                    if (step == 1 && form.krypton_pgStatus4.StateCommon.Back.Color1 != System.Drawing.Color.Green)
                    {
                        form.krypton_pgStatus4.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.krypton_pgStatus4.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;
                        form.tbCoin4.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.tbCoin4.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (step == 2 && form.krypton_pgStatus4.StateCommon.Back.Color1 != System.Drawing.Color.OrangeRed)
                    {
                        form.krypton_pgStatus4.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.krypton_pgStatus4.StateCommon.Back.Color2 = System.Drawing.Color.Orange;
                        form.tbCoin4.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbCoin4.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                }
                else if (id == 5)
                {
                    form.krypton_pgStatus5.Value++;
                    if (form.krypton_pgStatus5.Value >= form.krypton_pgStatus5.Maximum)
                        form.krypton_pgStatus5.Value = 0;

                    form.krypton_pgStatus5.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                    form.krypton_pgStatus5.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;

                    if (step == 1 && form.krypton_pgStatus5.StateCommon.Back.Color1 != System.Drawing.Color.Green)
                    {
                        form.krypton_pgStatus5.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.krypton_pgStatus5.StateCommon.Back.Color2 = System.Drawing.Color.LimeGreen;
                        form.tbCoin5.StateCommon.Back.Color1 = System.Drawing.Color.Green;
                        form.tbCoin5.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                    else if (step == 2 && form.krypton_pgStatus5.StateCommon.Back.Color1 != System.Drawing.Color.OrangeRed)
                    {
                        form.krypton_pgStatus5.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.krypton_pgStatus5.StateCommon.Back.Color2 = System.Drawing.Color.Orange;
                        form.tbCoin5.StateCommon.Back.Color1 = System.Drawing.Color.OrangeRed;
                        form.tbCoin5.StateCommon.Content.Color1 = System.Drawing.Color.White;
                    }
                }
            }
        }

        public void addDataToDataGridView1(Form_main form, string rowData)
        {
            string[] arrRowData = rowData.Split(',');
            //form.dataGridView1.Rows.Add(arrRowData);

            // 중복 여부 확인 (첫 번째 열 기준)
            bool exists = false;
            /*
            foreach (DataGridViewRow row in form.dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[0].Value != null &&
                    row.Cells[2].Value.ToString() == arrRowData[2])
                {
                    exists = true;
                    break;
                }
            }
            */


            if (!exists)
            {
                try
                {
                    if (dataGridView1.InvokeRequired)
                    {
                        dataGridView1.Invoke(new Action(() => addDataToDataGridView1(form, rowData)));
                        return;
                    }

                    dataGridView1.Rows.Add(arrRowData);

                    // 추가 후 맨 아래로 스크롤
                    if (dataGridView1.Rows.Count > 0)
                    {
                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                    }

                }
                catch (Exception ex)
                {
                    //
                    int a = 0;
                }
                //form.Logger($"▲ dataGridView1에 추가완료: {rowData}");
            }
            else
            {
                //MessageBox.Show($"이미 존재하는 항목입니다: {values[0]}");
            }
        }
        public void addDataToDataGridView2(Form_main form, string rowData)
        {
            string[] arrRowData = rowData.Split('|');
            //form.dataGridView1.Rows.Add(arrRowData);

            // 중복 여부 확인 (첫 번째 열 기준)
            bool exists = false;
            /*
            foreach (DataGridViewRow row in form.dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[0].Value != null &&
                    row.Cells[2].Value.ToString() == arrRowData[2])
                {
                    exists = true;
                    break;
                }
            }
            */


            if (!exists)
            {
                try
                {
                    if (dataGridView2.InvokeRequired)
                    {
                        dataGridView2.Invoke(new Action(() => addDataToDataGridView2(form, rowData)));
                        return;
                    }

                    dataGridView2.Rows.Add(arrRowData);

                    // 추가 후 맨 아래로 스크롤
                    if (dataGridView2.Rows.Count > 0)
                    {
                        dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView2.Rows.Count - 1;
                    }

                }
                catch (Exception ex)
                {
                    //
                    int a = 0;
                }
                //form.Logger($"▲ dataGridView1에 추가완료: {rowData}");
            }
            else
            {
                //MessageBox.Show($"이미 존재하는 항목입니다: {values[0]}");
            }
        }

        // 청산전략
        public static async Task<string> CheckExitCondition(Form_main form, BitgetRestClient client, decimal _lastPrice, decimal _roi_current, decimal _roi_current_max, string _strMemo, EmaWrapper emaWrapper, GlobalWrapper globalWrapper, decimal _nEnterMode)
        {
            try
            {
                if (globalWrapper._cutStep < 1.0m && _roi_current >= 1.0m)
                {
                    globalWrapper._cutStep = 1.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 1% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "1% 돌파";

                    speech("일");
                }
                else if (globalWrapper._cutStep < 1.5m && _roi_current >= 1.5m)
                {
                    globalWrapper._cutStep = 1.5m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 1.5% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "1.5% 돌파";
                    speech("일쩜오");
                }
                else if (globalWrapper._cutStep < 2.0m && _roi_current >= 2.0m)
                {
                    globalWrapper._cutStep = 2.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 2% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "2% 돌파";
                    speech("이");
                }
                else if (globalWrapper._cutStep < 3.0m && _roi_current >= 3.0m)
                {
                    globalWrapper._cutStep = 3.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 3% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "3% 돌파";
                    speech("삼");
                }
                else if (globalWrapper._cutStep < 4.0m && _roi_current >= 4.0m)
                {
                    globalWrapper._cutStep = 4.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 4% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "4% 돌파";
                    speech("사");
                }
                else if (globalWrapper._cutStep < 5.0m && _roi_current >= 5.0m)
                {
                    globalWrapper._cutStep = 5.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 5% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "5% 돌파";
                    speech("오");
                }
                else if (globalWrapper._cutStep < 6.0m && _roi_current >= 6.0m)
                {
                    globalWrapper._cutStep = 6.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 6% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "6% 돌파";
                    speech("육");
                }

                else if (globalWrapper._cutStep < 8.0m && _roi_current >= 8.0m)
                {
                    globalWrapper._cutStep = 8.0m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [목표도달] ♣♣♣ 8% 돌파!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "8% 돌파";
                    speech("팔");
                }


                if (globalWrapper._bb_approach != 100m
                    &&
                    (
                        (
                            _nEnterMode == 91 && // 저점에서 상방 진입
                            (
                              _lastPrice > emaWrapper._bb_upper
                                ||
                                Math.Abs(_lastPrice - emaWrapper._bb_upper) / emaWrapper._bb_upper * 100m < 0.5m //1m
                            )
                        )
                        ||
                        (
                            _nEnterMode == 92 &&
                            (
                               _lastPrice < emaWrapper._bb_lower
                                ||
                                Math.Abs(_lastPrice - emaWrapper._bb_lower) / emaWrapper._bb_lower * 100m < 0.5m //1m
                            )
                        )
                    )
                )
                {
                    globalWrapper._bb_approach = 100m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [근접] ▼▼▼ 볼밴근접!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "볼밴근접";
                    speech("볼밴근접", 0);
                }


                // 50선 
                if (globalWrapper._bb_approach != 100m
                    &&
                    (
                        (
                            _nEnterMode == 61 && // 저점에서 상방 진입
                            (
                              _lastPrice > emaWrapper._bb_upper                             
                            )
                        )
                        ||
                        (
                            _nEnterMode == 61 &&
                            (
                               _lastPrice < emaWrapper._bb_lower                                
                            )
                        )
                    )
                )
                {
                    globalWrapper._bb_approach = 100m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [근접] ▼▼▼ 볼밴근접!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "볼밴근접";
                    speech("볼밴근접", 0);
                }

                if (globalWrapper._50_approach != 100m
                    && DateTime.Now.Second > 50
                    &&
                    (
                        (
                            _nEnterMode == 91 && // 저점에서 상방 진입
                            (
                                _lastPrice > emaWrapper._ema50                                
                            )
                        )
                        ||
                        (
                            _nEnterMode == 92 &&
                            (
                               _lastPrice < emaWrapper._ema50
                            )
                        )
                    )
                )
                {
                    globalWrapper._50_approach = 100m;
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [근접] ▼▼▼ 50선 근접!!!!!", 0);


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "50근접";
                    speech("오십근접", 0);
                }


                //if (_bEma10_underEma15.Value == true && lastPrice > _ema15.Value && DateTime.Now.Second > 50) // 10선 < 15선일 때 15선 실시간 돌파시 청산.
                if (_nEnterMode == 3 && _mode_direction.Value == 2 && DateTime.Now - globalWrapper._timeContract >= TimeSpan.FromMinutes(2) && DateTime.Now.Second > 55 && (_lastPrice > emaWrapper._ema15 * 1.002m) && _strMemo.IndexOf("15 이평 이탈") < 0) // 10선 < 15선일 때 15선 실시간 돌파시 청산.
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15 이평 이탈 청산!");


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "15 이평 이탈";

                    await CloseAllPositions(form, client, emaWrapper._symbol);
                }
                else if (DateTime.Now.Hour == globalWrapper._timeContract.Hour
                    && DateTime.Now.Minute == globalWrapper._timeContract.Minute
                    && int.Parse(globalWrapper._createTime.Split(':')[2]) > 5
                    && DateTime.Now.Second > 50
                    && _lastPrice > emaWrapper._lastPrice_beforeCandle
                    //&& ((_nEnterMode != 61 && _lastPrice > emaWrapper._lastPrice_beforeCandle) || (_nEnterMode == 61 && _lastPrice < emaWrapper._lastPrice_beforeCandle)) 
                    && _nEnterMode != 61 && _nEnterMode != 62 //&& _nEnterMode != 39
                    && _nEnterMode != 91 && _nEnterMode != 92
                    && _nEnterMode != 41 && _nEnterMode != 42
                    && _nEnterMode != 81 && _nEnterMode != 82
                    && _nEnterMode != 11 && _nEnterMode != 12
                    && _strMemo.IndexOf("진입봉 이탈") < 0
                    ) // 진입봉 이탈
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 진입봉 이탈 청산!");


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "진입봉 이탈";

                    await CloseAllPositions(form, client, emaWrapper._symbol);
                }
                else if (_nEnterMode == 39
                    && _lastPrice < emaWrapper._ema50
                    && _strMemo.IndexOf("50선 이탈") < 0) // 진입봉 이탈
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50 이평 이탈 청산!");


                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "50선 이탈";

                    await CloseAllPositions(form, client, emaWrapper._symbol);
                }


                /*
                if (globalWrapper._cutStep == 1.0m && _roi_current <= 0.6m && _nEnterMode != ) // 2% 달성시 1.1% 보존!
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 1% 수익 후 0.6% 보존 청산!", 0);

                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "0.6% 청산";

                    await CloseAllPositions(form, client);
                }
                else 
                */


                //if(_nEnterMode != 41 && _nEnterMode != 42)
                {
                    /*
                    if (globalWrapper._cutStep == 1.5m && _roi_current <= 1.1m
                        && _nEnterMode != 61 && _nEnterMode != 62
                        && _nEnterMode != 41 && _nEnterMode != 42
                        && _nEnterMode != 81 && _nEnterMode != 82
                        && _nEnterMode != 91 && _nEnterMode != 92
                        && _nEnterMode != 71 && _nEnterMode != 72
                        && _nEnterMode != 11 && _nEnterMode != 12) // 1.5% 달성시 1.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 1.5% 수익 후 1.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "1.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                    /*
                    else if (globalWrapper._cutStep == 1.5m && _roi_current <= 0.3m
                        && (_nEnterMode == 91 || _nEnterMode == 92)) // 1.5% 달성시 0.3% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 1.5% 수익 후 1.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "0.3% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */

                    if (globalWrapper._cutStep == 2.0m && _roi_current <= 1.1m
                        && _nEnterMode != 41 && _nEnterMode != 42
                        //&& ((_nEnterMode != 91 && _nEnterMode != 92) || globalWrapper._entryStep >= 2)
                        && (_nEnterMode != 91 && _nEnterMode != 92)
                        && _nEnterMode != 11 && _nEnterMode != 12
                    ) // 2% 달성시 1.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 2% 수익 후 1.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "1.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (globalWrapper._cutStep == 2.0m && _roi_current <= 1.1m
                        && globalWrapper._bb_approach == 100m
                        && (_nEnterMode == 91 || _nEnterMode == 92)
                    ) // 2% 달성시 0.5% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 2% 수익 후 1.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "1.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    } 
                    */
                    else if (globalWrapper._bb_approach == 100m
                        && _roi_current > 1.5m // 추가
                        &&
                        (
                            (
                                (
                                    _nEnterMode == 91 ||
                                    _nEnterMode == 61
                                )
                                &&
                                _lastPrice < emaWrapper._ema50
                            )
                            ||
                            (
                                (
                                    _nEnterMode == 92 ||
                                    _nEnterMode == 62
                                )
                                &&
                                _lastPrice > emaWrapper._ema50
                            )
                        )
                    ) // 볼밴 상하단 근접 후 50선 이탈
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 볼밴 상/하단 근접 후 50선 이탈 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._50_approach == 100m
                        && _roi_current > 1.5m // 추가
                        &&
                        (
                            (
                                _nEnterMode == 91 &&
                                emaWrapper._lastPrice_beforeCandle >= emaWrapper._ema50 &&
                                _lastPrice < emaWrapper._ema50
                            )
                            ||
                            (
                            _nEnterMode == 92 &&
                                emaWrapper._lastPrice_beforeCandle >= emaWrapper._ema50 &&
                                _lastPrice > emaWrapper._ema50
                            )
                        )
                    ) // 볼밴 상하단 근접 후 50선 이탈
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50 상/하단 근접 후 50선 이탈 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._cutStep == 3.0m && _roi_current <= 2.1m) // 3% 달성시 2.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 3% 수익 후 2.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "2.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._cutStep == 4.0m && _roi_current <= 3.1m) // 4% 달성시 3.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 4% 수익 후 3.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "3.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._cutStep == 5.0m && _roi_current <= 4.1m) // 5% 달성시 4.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 5% 수익 후 4.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "4.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._cutStep == 6.0m && _roi_current <= 5.3m) // 6% 달성시 5.3% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 6% 수익 후 5.3% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "5.3% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (globalWrapper._cutStep == 8.0m && _roi_current <= 7.1m) // 8% 달성시 7.1% 보존!
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 8% 수익 후 7.1% 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "7.1% 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }


                    if (globalWrapper._bb_approach == 100m &&
                        (
                            (_nEnterMode == 91 && _lastPrice < emaWrapper._ema50)
                            ||
                            (_nEnterMode == 92 && _lastPrice > emaWrapper._ema50)
                        )
                        && _roi_current >= 2
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 볼밴 근접 후 50선 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }


                    // 60선 손절
                    if (globalWrapper._bb_approach == 100m &&
                        (
                            (_nEnterMode == 61 && _lastPrice < emaWrapper._ema50)
                            ||
                            (_nEnterMode == 62 && _lastPrice > emaWrapper._ema50)
                        )                        
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 볼밴 근접 후 50선 보존 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 청산";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }




                    // 스탑로스나 트레일링 스탑 긴급청산 to do 
                    else if (_roi_current > 15 && _nEnterMode != 41 && _nEnterMode != 42)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15% 이상 수익 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "15% 익절";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (_roi_current < -1.5m && (_nEnterMode == 81 || _nEnterMode == 82))//-3)
                    //else if (_roi_current < -10 && _nEnterMode != 61 && _nEnterMode != 62)//-3)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] -1.5% 이하 손실 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "-1.5% 손절";


                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_roi_current < -3 && _nEnterMode != 61 && _nEnterMode != 62 && _nEnterMode != 91 && _nEnterMode != 92)//-3)
                    //else if (_roi_current < -10 && _nEnterMode != 61 && _nEnterMode != 62)//-3)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] -3% 이하 손실 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "-3% 손절";


                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                }




                if (_nEnterMode == 41 && emaWrapper._bb_middle != 0 && DateTime.Now - globalWrapper._timeContract >= TimeSpan.FromMinutes(1))
                {
                    if (
                            (
                                emaWrapper._ema2 < emaWrapper._bb_middle
                            )
                            ||
                            (
                                ((emaWrapper._bb_middle - _lastPrice) / emaWrapper._bb_middle * 100m) > 3m
                            )
                        )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 볼밴 중심선 하향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "중심선 하향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (
                            _lastPrice > emaWrapper._ema15 &&
                            //DateTime.Now.Second > 55 &&
                            _roi_current_max >= 10m
                        )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15선 하향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "15하향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                }
                else if (_nEnterMode == 42 && emaWrapper._bb_middle != 0 && DateTime.Now - globalWrapper._timeContract >= TimeSpan.FromMinutes(1))
                {
                    if (
                            (
                                emaWrapper._ema2 > emaWrapper._bb_middle
                            )
                            ||
                            (
                                ((_lastPrice - emaWrapper._bb_middle) / emaWrapper._bb_middle * 100m) > 3m
                            )
                        )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 볼밴 중심선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "중심선 상향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    else if (
                            _lastPrice < emaWrapper._ema15 &&
                            //DateTime.Now.Second > 55 &&
                            _roi_current_max >= 10m
                        )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "15하향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }

                }
                else if (_nEnterMode == 61)
                {/*
                if(emaWrapper._ema2 > emaWrapper._ema50 &&
                    DateTime.Now.Second > 55
                )
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50선 2선 상향돌파 청산!");

                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "50상향 돌파";

                    await CloseAllPositions(form, client);
                }
                else*/

                    if (_lastPrice < emaWrapper._ema10 &&
                        DateTime.Now.Second > 55 &&
                        _roi_current >= 7m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 10선 하향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "10하향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_lastPrice < emaWrapper._ema15 &&
                        DateTime.Now.Second > 55
                    //_roi_current >= 7m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15선 하향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "15하향 돌파";

                        await CloseAllPositions(form, client);
                    }
                    */
                }
                else if (_nEnterMode == 62)
                {/*
                if(emaWrapper._ema2 > emaWrapper._ema50 &&
                    DateTime.Now.Second > 55
                )
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50선 2선 상향돌파 청산!");

                    if (_strMemo != "")
                        _strMemo += "| ";
                    _strMemo += "50상향 돌파";

                    await CloseAllPositions(form, client);
                }
                else*/

                    if (_lastPrice > emaWrapper._ema10 &&
                        DateTime.Now.Second > 55 &&
                        _roi_current >= 7m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 10선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "10상향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_lastPrice > emaWrapper._ema15 &&
                        DateTime.Now.Second > 55
                    //_roi_current >= 7m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 15선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "15상향 돌파";

                        await CloseAllPositions(form, client);
                    }
                    */
                }


                else if (_nEnterMode == 91)
                {
                    /*
                    if (emaWrapper._ema50 < emaWrapper._ema10 &&
                        _lastPrice < emaWrapper._ema10
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 10선 하향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "10하향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                    if (emaWrapper._ema5 > emaWrapper._bb_upper &&
                        _roi_current >= 1.0m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 상단선 이탈 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "상단선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_lastPrice > emaWrapper._ema50)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50선 이탈", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                }
                else if (_nEnterMode == 92)
                {
                    /*
                    if (emaWrapper._ema50 > emaWrapper._ema10 &&
                        _lastPrice > emaWrapper._ema10
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 10선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "10상향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                    if (emaWrapper._ema5 < emaWrapper._bb_lower &&
                        _roi_current >= 1.0m
                    )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 하단선 이탈 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "하단선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_lastPrice < emaWrapper._ema50)
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50선 이탈", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50선 이탈";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    */
                }

                else if (_nEnterMode == 7)
                {
                    if (_lastPrice > emaWrapper._ema15 &&
                       DateTime.Now.Second > 55 &&
                       _roi_current_max >= 2m
                   )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 10선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "10상향 돌파";

                        await CloseAllPositions(form, client, emaWrapper._symbol);
                    }
                    /*
                    else if (_lastPrice > emaWrapper._ema50 &&
                       DateTime.Now.Second > 55
                       //_roi_current >= 1m
                   )
                    {
                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [긴급청산] 50선 상향돌파 청산!", 0);

                        if (_strMemo != "")
                            _strMemo += "| ";
                        _strMemo += "50상향 돌파";

                        await CloseAllPositions(form, client);
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] closeall Error = {ex.Message}", 0);

                speech("클로즈 올 오류발생", 0);
            }

            return _strMemo;
        }

        public static void setRealOutput(Form_main form, decimal id, decimal realRoi, decimal realPnl)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() =>
                {
                    form.tbRealOutput_roi.Text = realRoi.ToString();
                    form.Text = realPnl.ToString();
                }));
            }
            else
            {
                form.tbRealOutput_roi.Text = realRoi.ToString();
                form.tbRealOutput_pnl.Text = realPnl.ToString();
            }
        }




        private void Form_main_Shown(object sender, EventArgs e)
        {

        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            var client = new BitgetRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(apiKey, apiSecret, passphrase);
            });
            await getPosHistory_by_period(this, client);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // "수익률" 컬럼만 처리
            if (dataGridView1.Columns[e.ColumnIndex].Name == "수익률" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal roi))
                {
                    if (roi > 0)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.OrangeRed;
                        e.CellStyle.ForeColor = System.Drawing.Color.White;
                    }
                    else if (roi < 0)
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.CornflowerBlue;
                        e.CellStyle.ForeColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        e.CellStyle.BackColor = System.Drawing.Color.White;
                        e.CellStyle.ForeColor = System.Drawing.Color.Black;
                    }

                    // 선택돼도 원래 색 유지
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                }
            }
        }

        // INI 설정파일 로딩
        public bool loadConfig()
        {
            Logger("[" + DateTime.Now.ToLongTimeString() + "] # INI 설정파일 로딩 시작...");

            string strID = "whsunbi_coin";

            //ini 읽기
            try
            {
                GetPrivateProfileString(strID, "apiKey", "", sbApiKey, 64, CONFIG_FILE);
                GetPrivateProfileString(strID, "apiSecret", "", sbApiSecret, 128, CONFIG_FILE);
                GetPrivateProfileString(strID, "passphrase", "", sbPassphrase, 32, CONFIG_FILE);

                GetPrivateProfileString(strID, "ChatId", "", sbChatId, 32, CONFIG_FILE);
                GetPrivateProfileString(strID, "Token_general", "", sbToken_general, 64, CONFIG_FILE);
                //GetPrivateProfileString(strID, "Token_summary", "", sbToken_summary, 64, CONFIG_FILE);

                GetPrivateProfileString(strID, "Leverage", "", sbLeverage, 32, CONFIG_FILE);
                GetPrivateProfileString(strID, "InvestMoney", "", sbInvestMoney, 64, CONFIG_FILE);
                GetPrivateProfileString(strID, "TargetProfit", "", sbTargetProfit, 32, CONFIG_FILE);
                GetPrivateProfileString(strID, "MaxLoss", "", sbMaxLoss, 64, CONFIG_FILE);


                tbApiKey.Text = sbApiKey.ToString().Trim();
                tbApiSecret.Text = sbApiSecret.ToString().Trim();
                tbPassPhase.Text = sbPassphrase.ToString().Trim();

                tbChatId.Text = sbChatId.ToString().Trim();
                tbToken_general.Text = sbToken_general.ToString().Trim();
                //tbToken_summary.Text = sbToken_summary.ToString().Trim();

                numLeverage.Value = Convert.ToDecimal(sbLeverage.ToString());
                tbInvestMoney.Text = sbInvestMoney.ToString().Trim();
                numTargetProfit.Value = Convert.ToDecimal(sbTargetProfit.ToString());
                numMaxLoss.Value = Convert.ToDecimal(sbMaxLoss.ToString());

                if (sbChatId.ToString() == "")
                {
                    MessageBox.Show("#설정파일 로딩 오류 : 해당 파일이 없거나 해당 ID에 대한 설정이 없습니다.\n\n확인 후 다시 시도해 주십시오.\n감사합니다.", "샤크루즈 오류");
                    Logger("[" + DateTime.Now.ToLongTimeString() + "] [Error] INI 설정파일 로딩 실패!! ");
                    return false;
                }
                Logger("[" + DateTime.Now.ToLongTimeString() + "] # INI 설정파일 로딩 끝!");

            }
            catch (Exception ex)
            {
                Logger("[" + DateTime.Now.ToLongTimeString() + "] [Error] INI 설정파일 로딩 실패!! (" + ex.Message.ToString() + ")");
                Logger("[" + DateTime.Now.ToLongTimeString() + "] [Error] INI 설정파일 로딩 실패!! (" + ex.Message.ToString() + ")");
            }
            Logger("------------------------------------------------------");

            Thread.Sleep(200);
            return true;

            // MessageBox.Show("ID :" + sbId.ToString() + ", charid: " + sbChatId.ToString() );
        }

        public async Task<BitgetFuturesOrder> entryOrder(Form_main form, BitgetRestClient client, decimal _nEnterMode, decimal quantity, decimal _tickSize, GlobalWrapper globalWrapper)
        {
            BitgetFuturesOrder filledOrder = null;



            // 방향에 따라 OrderSide 설정
            //globalWrapper._side = OrderSide.Sell;


            if (_nEnterMode == 61 || _nEnterMode == 41 || _nEnterMode == 81 || _nEnterMode == 71 || _nEnterMode == 91 || _nEnterMode == 11)
                globalWrapper._side = OrderSide.Buy; // 롱

            // form.Logger($"숏 주문 지정가 : {entryPrice}");
            var order = await client.FuturesApiV2.Trading.PlaceOrderAsync(
                 productType: BitgetProductTypeV2.UsdtFutures,
                 symbol: threadSymbol.Value,
                 marginAsset: "USDT",
                 side: globalWrapper._side,
                 //type: OrderType.Limit,             // 여기 필수!
                 type: OrderType.Market,             // 여기 필수!
                 marginMode: MarginMode.IsolatedMargin,
                 quantity: quantity//,
                                   //price: entryPrice//,   // 지정가 주문일 때만 필요
                                   //////tradeSide: TradeSide.Open // 단방향 모드일 때 반드시 설정                 
             );

            if (order.Success)
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ▷ {globalWrapper._side} 진입(시장가) 주문 성공. {quantity}개");
            }
            else
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN Short 주문 실패: {quantity}개, {order.Error}", 0);
                return null;
            }

            while (true)
            {
                if (order.Data != null)
                {
                    break;
                }
                await Task.Delay(1000); // 1초 간격
            }

            decimal totalQty = 0;           // 전체 계약 수량
            decimal totalPrice = 0;// 전체 평균 진입가

            // 1차 진입 주문 체결확인
            int nSec = 0;
            decimal filledPrice = 0;

            bool bCancel_success = false;
            BitgetFuturesOrder orderInfo = null;  // 루프 밖에서 선언

            while (true)
            {
                await Task.Delay(1000); // 1초 간격

                var checkOrder = await client.FuturesApiV2.Trading.GetOrderAsync(
                   productType: BitgetProductTypeV2.UsdtFutures,
                   symbol: threadSymbol.Value,
                   orderId: order.Data.OrderId
               );

                if (!checkOrder.Success)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 주문 상태 조회 실패: {checkOrder.Error}");
                    continue;
                }
                else
                {
                    // 주문 ID 받아오기
                    var orderId = order.Data.OrderId;
                    //form.Logger($"주문 접수 완료: OrderId={orderId}");

                    // 2. 주문 상세 조회
                    var orderResult = await client.FuturesApiV2.Trading.GetOrderAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        symbol: threadSymbol.Value,
                        orderId: orderId
                    );

                    if (!orderResult.Success)
                    {
                        form.Logger($"주문 조회 실패: {orderResult.Error}");
                    }
                    else
                    {
                        // WebCallResult에서 실제 주문 객체 추출
                        orderInfo = orderResult.Data;
                        /*
                        form.Logger("===== 주문 상세 =====");
                        form.Logger($"주문 수량: {orderInfo.Quantity}");
                        form.Logger($"체결 수량: {orderInfo.QuantityFilled}");
                        form.Logger($"남은 수량: {orderInfo.Quantity - orderInfo.QuantityFilled}");
                        form.Logger($"상태: {orderInfo.Status}");
                        */

                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]                 {orderInfo.QuantityFilled}/{orderInfo.Quantity} ");

                        if (orderInfo.QuantityFilled <= 0)
                        {
                            await Task.Delay(500);
                            continue;
                        }


                        globalWrapper._createTime = $"{orderInfo.CreateTime.ToLocalTime():HH:mm:ss}";
                        globalWrapper._investMoney = (decimal)(orderInfo.AveragePrice * orderInfo.QuantityFilled);
                        string rowData2 = $"{orderInfo.CreateTime.ToLocalTime():yyyy-MM-dd}, {orderInfo.CreateTime.ToLocalTime():ddd}, {orderInfo.CreateTime.ToLocalTime():HH:mm:ss}, 0, 0, {(double)(orderInfo.AveragePrice * orderInfo.QuantityFilled)}, {orderInfo.Symbol}, {orderInfo.Side}, {(double)orderInfo.QuantityFilled}";
                        form.addDataToDataGridView1(form, rowData2); // dataGridView1에 중복체크하고 추가.
                    }
                }


                filledOrder = orderInfo;
                if (filledOrder.Status.ToString().ToLower() == "filled" && filledOrder.Quantity == filledOrder.QuantityFilled)
                {
                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]    ▶ {globalWrapper._side} POSITION 체결됨. {filledOrder.AveragePrice}, {filledOrder.QuantityFilled}");
                    //filledPrice = (decimal)filledOrder.AveragePrice;
                    //_totalAveragePrice = filledPrice;

                    globalWrapper._timeContract = DateTime.Now;

                    break;
                }
                else if (filledOrder.Status.ToString().ToLower() == "partially_filled") // 부분 체결
                {
                    // 0.5% 이상 수익시 기존 OPEN 해제
                    //filledPrice = (decimal)filledOrder.AveragePrice;
                    //_totalAveragePrice = filledPrice;

                    globalWrapper._timeContract = DateTime.Now;

                    decimal roi = 0;
                    // 포지션 조회
                    var positions = await client.FuturesApiV2.Trading.GetPositionsAsync(
                        productType: BitgetProductTypeV2.UsdtFutures,
                        marginAsset: "USDT"
                    );


                    foreach (var pos in positions.Data)
                    {
                        if (pos.AverageOpenPrice > 0 && globalWrapper._lastPrice > 0)
                        {
                            roi = (pos.AverageOpenPrice - globalWrapper._lastPrice) / pos.AverageOpenPrice * 100m;
                            if (roi >= 0.5m)
                            {
                                // OPEN : 미체결 주문 조회
                                var openOrders = await client.FuturesApiV2.Trading.GetOpenOrdersAsync(
                                    productType: BitgetProductTypeV2.UsdtFutures,
                                    symbol: threadSymbol.Value
                                );

                                if (!openOrders.Success)
                                {
                                    form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN : 미체결 주문조회 실패: {openOrders.Error}");

                                }
                                else
                                {

                                    // 실제 주문 리스트 확인 후 취소
                                    if (openOrders.Data != null)
                                    {
                                        foreach (var o in openOrders.Data.Orders)
                                        {
                                            var cancelResult = await client.FuturesApiV2.Trading.CancelOrderAsync(
                                                productType: BitgetProductTypeV2.UsdtFutures,
                                                symbol: threadSymbol.Value,
                                                orderId: o.OrderId
                                            );

                                            if (cancelResult.Success)
                                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN : 미체결 남은 물량,주문취소 성공, {o.OrderId}");
                                            else
                                                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OPEN :미체결 남은 물량, 주문취소 실패... {o.OrderId} : {cancelResult.Error}");
                                        }
                                    }

                                    await Task.Delay(2000);
                                    // 기존꺼 취소하고 체결된 물량은 계속 진행!!
                                }

                            }
                        }
                    }
                }
                else
                {
                    //form.Logger($"체결 대기 중... 현재 상태: {filledOrder.Status}");
                    //form.Logger("&");

                    nSec++;
                    if (nSec > 60) // 60초 동안 1차 주문이 체결되지 않으면 취소시키기.
                    {
                        // 취소 주문
                        try
                        {
                            if (order.Data != null && order.Data.OrderId != null)
                            {
                                while (true)
                                {
                                    var cancelResult = await client.FuturesApiV2.Trading.CancelOrderAsync(
                                        productType: BitgetProductTypeV2.UsdtFutures, // USDT 선물
                                        symbol: threadSymbol.Value,
                                        orderId: order.Data.OrderId   // 취소할 주문 ID
                                    );

                                    await Task.Delay(1000);
                                    if (cancelResult.Success)
                                    {
                                        form.Logger(" ");
                                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 1차 {globalWrapper._side.ToString()} - OPEN 주문 취소 성공");
                                        bCancel_success = true;
                                        break;
                                    }
                                    else
                                    {
                                        form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 1차 {globalWrapper._side.ToString()} - OPEN 주문 취소 실패.... 재시도!");
                                        continue;
                                    }
                                }

                                if (bCancel_success == true)
                                {
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            //
                        }

                    }
                }
            }

            return filledOrder;
        }

        public string getDateTime(bool bMode)
        {
            string strTime = " ";
            strTime += DateTime.Now.ToString("yyyy-MM-dd");
            strTime += "(";
            DayOfWeek DayOfWeek = DateTime.Now.DayOfWeek;
            switch (DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    strTime += "일";
                    break;
                case DayOfWeek.Monday:
                    strTime += "월";
                    break;
                case DayOfWeek.Tuesday:
                    strTime += "화";
                    break;
                case DayOfWeek.Wednesday:
                    strTime += "수";
                    break;
                case DayOfWeek.Thursday:
                    strTime += "목";
                    break;
                case DayOfWeek.Friday:
                    strTime += "금";
                    break;
                case DayOfWeek.Saturday:
                    strTime += "토";
                    break;
            }
            strTime += ") ";

            if (bMode)
                return strTime;

            //strTime += DateTime.Now.ToString("HH:mm:ss");
            strTime += DateTime.Now.ToLongTimeString();
            return strTime;
        }

        public string getContent(KryptonListBox lbTemp)
        {
            string strTemp = "";
            int nCount = lbTemp.Items.Count;
            for (int i = 0; i < nCount; i++)
            {
                strTemp += lbTemp.Items[i] + "\r\n";
            }
            return strTemp;
        }


        public void openLog(string LOG_FILE)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "notepad.exe";
            startInfo.Arguments = LOG_FILE;
            //startInfo.Arguments = "/C " + ftpbatFile; // 절대경로를 포함한 BAT 파일명       

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;

            Process ftp = new Process();
            ftp.StartInfo = startInfo;
            ftp.Start();
        }

        public void saveLog(Form_main form, int nMode = 1)
        {

            try
            {
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [" + DateTime.Now.ToLongTimeString() + "] [INFO] 로그저장(텍스트) 시작...");
                /////////////////////////////////////////////////////////////////
                string strTime = getDateTime(false).Trim();
                strTime = strTime.Replace(":", "_");
                //string strFile = string.Format("D:\\whsunbi\\sharcruz\\log\\sharcruz_log_" +lbl아이디.Text + "_"+ DateTime.Now.ToString("yyyy-MM-dd") + ".log");


                //LOG_FILE = string.Format(sbLogPath.ToString() + "sharcruz_log_" + lbl아이디.Text + "_" + _nRunMode.ToString() + "_" + strTime + ".log");

                string LOG_FILE = string.Format($"C:\\whsunbi\\sharcruz\\log_coin\\sharcruz_log_whsunbi_1_" + strTime + ".log");
                //string LOG_FILE = string.Format($"D:\\whsunbi\\sharcruz\\log_coin\\sharcruz_log_whsunbi_1_" + strTime + ".log");

                FileInfo fi = new FileInfo(LOG_FILE);
                StreamWriter sw = new StreamWriter(LOG_FILE);
                sw.WriteLine("=============================================================================================================================================");
                sw.WriteLine("=============================================================================================================================================");
                sw.WriteLine("");
                sw.WriteLine(">>>>>>> 샤크루즈 코카야 매매로그");
                sw.WriteLine("### ID : whsunbi");
                sw.WriteLine("### 최종 로그 저장시간 : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToLongTimeString());
                sw.WriteLine("");
                sw.WriteLine(" ● 메인 ##########################################################################################");
                sw.WriteLine(getContent(lbMain));
                sw.WriteLine("");
                sw.WriteLine(" ● Task1 ##########################################################################################");
                sw.WriteLine(getContent(lbTask1));
                sw.WriteLine("");
                sw.WriteLine(" ● Task2 ##########################################################################################");
                sw.WriteLine(getContent(lbTask2));
                sw.WriteLine("");
                sw.WriteLine(" ● Task3 ##########################################################################################");
                sw.WriteLine(getContent(lbTask3));
                sw.WriteLine("");
                sw.WriteLine(" ● Task4 ##########################################################################################");
                sw.WriteLine(getContent(lbTask4));
                sw.WriteLine("");
                sw.WriteLine(" ● Task5 ##########################################################################################");
                sw.WriteLine(getContent(lbTask5));
                sw.WriteLine("");
                sw.WriteLine("");               
                sw.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------");
                sw.Close();

                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [" + DateTime.Now.ToLongTimeString() + "] [INFO] 로그저장(텍스트) 끝!");

                if (nMode == 0)
                {
                    MessageBox.Show("로그 저장 성공!! ^^ (" + LOG_FILE + ")");

                    openLog(LOG_FILE);
                }


            }
            catch (Exception ex)
            {                
                form.Logger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] Log 로그저장 실패(__)(" + ex.Message.ToString() + ")");
                MessageBox.Show("로그 저장 실패(Log)... : " + ex.Message.ToString());
            }
}

        private void Form_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveLog(this);
        }
    } // class

    public class EmaWrapper
    {
        public string _symbol { get; set; }
        public decimal _tickSize { get; set; }
        public decimal _ema2 { get; set; }
        public decimal _ema2_1 { get; set; }
        public decimal _ema5 { get; set; }
        public decimal _ema5_2 { get; set; }
        public decimal _ema5_3 { get; set; }
        public decimal _ema10 { get; set; }
        public decimal _ema15 { get; set; }
        public decimal _ema50 { get; set; }
        public decimal _ema50_1 { get; set; }
        public decimal _lastPrice_beforeCandle { get; set; }
        public decimal _lastPrice_beforeCandle_old { get; set; }
        public decimal _lastPrice_beforeCandle_old2 { get; set; }
        public bool _bLastPrice_beforeCandle { get; set; }
        public decimal _bongsize_max { get; set; }

        public decimal _highestHigh_3 { get; set; }
        public decimal _highestHigh { get; set; } // 5
        public decimal _highestHigh_10 { get; set; }
        public decimal _highestHigh_15 { get; set; }
        public decimal _highestHigh_30 { get; set; }
        public decimal _lowestLow_3 { get; set; }
        public decimal _lowestLow { get; set; }
        public decimal _lowestLow_10 { get; set; }
        public decimal _lowestLow_15 { get; set; }
        public decimal _lowestLow_30 { get; set; }
        public bool _bFinish { get; set; }

        public decimal _bb_upper { get; set; }
        public decimal _bb_lower { get; set; }
        public decimal _bb_middle { get; set; }

        public decimal _low_1 { get; set; }
        public decimal _low_2 { get; set; }
        public decimal _low_3 { get; set; }
        public decimal _low_4 { get; set; }

        public decimal _high_1 { get; set; }
        public decimal _high_2 { get; set; }
        public decimal _high_3 { get; set; }
        public decimal _high_4 { get; set; }

        public decimal _close_1 { get; set; }
        public decimal _close_2 { get; set; }
        public decimal _close_3 { get; set; }
        public decimal _close_4 { get; set; }

        public decimal _bb_upper_2 { get; set; }
        public decimal _bb_lower_2 { get; set; }
        public decimal _bb_upper_3 { get; set; }
        public decimal _bb_lower_3 { get; set; }
        public decimal _bb_upper_4 { get; set; }
        public decimal _bb_lower_4 { get; set; }
        public decimal _bb_upper_5 { get; set; }
        public decimal _bb_lower_5 { get; set; }

        public decimal _belowEma50 { get; set; }
        public decimal _aboveEma50 { get; set; }



    }

    public class GlobalWrapper
    {
        public decimal _detectedPercent { get; set; }
        public decimal _entryStep { get; set; }
        public OrderSide _side { get; set; }
        public decimal _cutStep { get; set; }
        public decimal _bb_approach { get; set; }
        public decimal _50_approach { get; set; }

        public DateTime _timeContract { get; set; }
        public string _createTime { get; set; } // string

        public decimal _investMoney { get; set; }
        public decimal _lastPrice { get; set; }

        
        





    }
}
