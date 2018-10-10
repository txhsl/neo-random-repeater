using Neo;
using Neo.Core;
using Neo.Wallets;
using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using repeater_gui.Properties;
using Neo.SmartContract;
using System.Linq;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;

namespace repeater_gui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread threadTransaction;
        Thread threadInit;
        public bool direction = true;
        static int amount = 10;
        int counter = 0;

        private Account accountA;
        private Account accountB;

        ObservableCollection<RecordInfo> recordInfoList = new ObservableCollection<RecordInfo>();
        internal ObservableCollection<RecordInfo> RecordInfoList
        {
            get { return recordInfoList; }
            set { recordInfoList = value; }
        }

        public MainWindow()
        {
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            InitializeComponent();

            this.threadInit = new Thread(new ThreadStart(this.Init));
            this.threadInit.Name = "MyInitThread";

            this.threadTransaction = new Thread(new ThreadStart(this.Transact));
            this.threadTransaction.Name = "MyTransactionThread";

            DispatcherTimer timeTicker = new DispatcherTimer();
            timeTicker.Interval = new TimeSpan(0, 0, 1); //in Hour, Minutes, Second.
            timeTicker.Tick += time_Tick;
            timeTicker.Start();

            Console.WriteLine(this.RecordInfoList.Count);
            Console.WriteLine(threadTransaction.ThreadState);
        }

        private void time_Tick(object sender, EventArgs e)
        {
            lbl_height.Content = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            lbl_count_node.Content = Program.LocalNode.RemoteNodeCount.ToString();
            switch (threadTransaction.ThreadState)
            {
                case ThreadState.Unstarted:
                    lbl_threads_state.Content = "unstarted";
                    break;
                case ThreadState.Suspended:
                    lbl_threads_state.Content = "suspended";
                    break;
                case ThreadState.Running:
                    lbl_threads_state.Content = "running";
                    break;
                default:
                    lbl_threads_state.Content = "waiting";
                    break;
            }
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Program.LocalNode.Start(Settings.Default.NodePort);
            listView.ItemsSource = RecordInfoList;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (accountA.Wallet != null && accountB.Wallet != null)
                {
                    if (this.RecordInfoList.Count == 0 && threadInit.ThreadState == ThreadState.Unstarted)
                    {
                        threadInit.Start();
                    }

                    if (threadTransaction.ThreadState == ThreadState.Suspended)
                    {
                        threadTransaction.Resume();
                    }
                    else if (threadTransaction.ThreadState == ThreadState.Unstarted)
                    {
                        threadTransaction.Start();
                    }
                }
                else
                {
                    MessageBox.Show("请加载钱包文件！");
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("请加载钱包文件！");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (threadTransaction.ThreadState == ThreadState.Running)
            {
                threadTransaction.Suspend();
            }
        }

        private void btnRebuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (accountA.Wallet != null && accountB.Wallet != null)
                {
                    if (threadTransaction.IsAlive && threadTransaction.ThreadState != ThreadState.Suspended)
                        threadTransaction.Abort();
                    //accountA.Indexer.RebuildIndex();
                    //accountB.Indexer.RebuildIndex();
                }
                else
                {
                    MessageBox.Show("请加载钱包文件！");
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("请加载钱包文件！");
            }
        }

        private void btnWalletFile_Click(object sender, RoutedEventArgs e)
        {
            WalletFile walletfile = new WalletFile();
            if (walletfile.ShowDialog() == true)
            {
                try
                {
                    accountA = new Account(walletfile.walletPathA, walletfile.walletPasswordA, "KxC7fxvBgNNeiFmcp1gRzN6ZfSFXfxrTC6WDXAFjhWDqrknoZUrv");
                    accountB = new Account(walletfile.walletPathB, walletfile.walletPasswordB, "KwPRvCPeoe2y2CvqFypAzv5nVKjziQPStHrFndZQAS5MjQbgrC5C");
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("密码错误！");
                    return;
                }
            }
        }

        private void Init()
        {
            Account[] accountPair = new Account[2];
            accountPair[0] = accountA;
            accountPair[1] = accountB;
            for (int i = 0; i < 2; i++)
            {
                Program.CurrentWallet = accountPair[i].Wallet;
                int curWalletAmount = int.Parse(Program.CurrentWallet.GetAvailable(Blockchain.GoverningToken.Hash).ToString());

                TransactionOutput[] outputs = new TransactionOutput[curWalletAmount / amount];
                for (int j = 0; j < curWalletAmount; j += amount)
                {
                    outputs[j / amount] = new TransactionOutput
                    {
                        AssetId = Blockchain.GoverningToken.Hash,
                        ScriptHash = Wallet.ToScriptHash(accountPair[i].Address),
                        Value = Fixed8.Parse(amount.ToString())
                    };
                }
                if (curWalletAmount > amount)
                {
                    TransactProcess(outputs, Program.CurrentWallet);
                }
                else
                {
                    Thread.Sleep(16000);
                }
            }
        }

        private void Transact()
        {
            WalletAccount bank = accountA.Wallet.GetAccounts().First();
            WalletAccount[] consumers = accountB.Wallet.GetAccounts().ToArray();
            int patch = 100;
                
            while(true)
            {
                if (threadInit.ThreadState == ThreadState.Stopped)
                { //if (amount < 100 || amount > 1000) amount = RandomNumber();

                    Console.WriteLine($"Tx turn {counter}.");
                    ArrayList<WalletAccount> randomConsumers = new ArrayList<WalletAccount>();

                    for (int i = 0; i < patch; i++)
                    {
                        randomConsumers.Add(consumers[RandomNumber(counter)]);
                    }
                                        
                    foreach (WalletAccount consumer in randomConsumers)
                    {
                        TransactAction(consumer, accountA, amount);
                    }

                    TransactBackAction(bank, accountB, patch * amount);
                    counter++;
                }
            }
        }

        
        private void TransactAction(WalletAccount target, Account account, int value)
        {
            Program.CurrentWallet = account.Wallet;
            int curWalletAmount = int.Parse(Program.CurrentWallet.GetAvailable(Blockchain.GoverningToken.Hash).ToString());
            
            TransactionOutput[] outputs = new TransactionOutput[1];

            outputs[0] = new TransactionOutput
            {
                AssetId = Blockchain.GoverningToken.Hash,
                ScriptHash = Wallet.ToScriptHash(target.Address),
                Value = Fixed8.Parse(value.ToString())
            };

            if (curWalletAmount > value)
            {
                TransactProcess(outputs, Program.CurrentWallet);
            }
            else
            {
                Thread.Sleep(16000);
            }
        }

        private void TransactBackAction(WalletAccount target, Account account, int value)
        {
            Program.CurrentWallet = account.Wallet;
            int curWalletAmount = int.Parse(Program.CurrentWallet.GetAvailable(Blockchain.GoverningToken.Hash).ToString());

            TransactionOutput[] outputs = new TransactionOutput[value / amount];

            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = new TransactionOutput
                {
                    AssetId = Blockchain.GoverningToken.Hash,
                    ScriptHash = Wallet.ToScriptHash(target.Address),
                    Value = Fixed8.Parse(amount.ToString())
                };
            }

            if (curWalletAmount > value)
            {
                TransactProcess(outputs, Program.CurrentWallet);
            }
            else
            {
                Thread.Sleep(16000);
            }
        }

        private void TransactProcess(TransactionOutput[] pOutputs, Wallet pWallet)
        {
            //构造交易
            string availableA = accountA.Wallet.GetAvailable(Blockchain.GoverningToken.Hash).ToString();
            string balanceA = accountA.Wallet.GetBalance(Blockchain.GoverningToken.Hash).ToString();
            string availableB = accountB.Wallet.GetAvailable(Blockchain.GoverningToken.Hash).ToString();
            string balanceB = accountB.Wallet.GetBalance(Blockchain.GoverningToken.Hash).ToString();

            ContractTransaction tx = pWallet.MakeTransaction(new ContractTransaction { Outputs = pOutputs }, null, null, Fixed8.Zero);
            if (tx == null)
            {
                //amount = amount * 10;
                //Thread.Sleep(300);
                MessageBox.Show("余额不足以支付系统费用。");
                return;
            }
            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(tx);
            }
            catch (InvalidOperationException)
            {
                //Thread.Sleep(20000);
                MessageBox.Show("钱包余额不足，或区块链未同步完成，无法发送该交易。");
                return;
            }
            Program.CurrentWallet.Sign(context);
            if (context.Completed)
            {
                tx.Scripts = context.GetScripts();
                Program.CurrentWallet.ApplyTransaction(tx);
                Program.LocalNode.Relay(tx);
                string result = "Turn "+ counter + ", 交易已发送, 这是交易编号(TXID)：" + tx.Hash.ToString();

                this.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
                {
                    this.RecordInfoList.Add(new RecordInfo(result, availableA, balanceA, availableB, balanceB));
                    this.listView.ScrollIntoView(this.listView.Items[this.listView.Items.Count - 1]);
                    GridView gv = listView.View as GridView;
                    if (gv != null)
                    {
                        foreach (GridViewColumn gvc in gv.Columns)
                        {
                            gvc.Width = gvc.ActualWidth;
                            gvc.Width = Double.NaN;
                        }
                    }
                }));
                Thread.Sleep(300);
            }
        }

        public static int RandomNumber(int max)
        {
            Random rd = new Random();
            return rd.Next(0, max);
        }

        public void Invert(bool signal)
        {
            if (signal)
            {
                this.direction = false;
            }
            else
            {
                this.direction = true;
            }
        }

    }

    
}
