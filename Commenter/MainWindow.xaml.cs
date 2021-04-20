using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Commenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FlowComments comments = new FlowComments();
        List<string> messages = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task ConnectToServer()
        {
            //クライアント側のWebSocketを定義
            ClientWebSocket ws = new ClientWebSocket();

            //接続先エンドポイントを指定
            var uri = new Uri("ws://ik1-431-47779.vs.sakura.ne.jp:25252");

            //サーバに対し、接続を開始
            await ws.ConnectAsync(uri, CancellationToken.None);
            var buffer = new byte[1024];

            //情報取得待ちループ
            while (true)
            {
                //所得情報確保用の配列を準備
                var segment = new ArraySegment<byte>(buffer);

                //サーバからのレスポンス情報を取得
                var result = await ws.ReceiveAsync(segment, CancellationToken.None);

                //エンドポイントCloseの場合、処理を中断
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK",
                      CancellationToken.None);
                    return;
                }

                //バイナリの場合は、当処理では扱えないため、処理を中断
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                      "I don't do binary", CancellationToken.None);
                    return;
                }

                //メッセージの最後まで取得
                int count = result.Count;
                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                          "That's too long", CancellationToken.None);
                        return;
                    }
                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);

                    count += result.Count;
                }

                //メッセージを取得
                var message = Encoding.UTF8.GetString(buffer, 0, count);
                messages.Add(message);
                FlowComment(message);
            }
        }

        private void FlowComment(string comment)
        {
            //if (!comment.Contains("mode=\"1\""))
              //  return;

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(10);
            Random rm = new Random();
            TextBlock tb = new TextBlock()
            {
                Text = comment,
                FontSize = 20,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            };
            tb.Margin = new Thickness(0, rm.Next(0, 720), 15, 0);
            comments.mainGrid.Children.Add(tb);
            int x = 15;
            dt.Tick += (ss, ee) =>
            {
                if (tb.Margin.Left < 0)
                    dt.Stop();
                tb.Margin = new Thickness(0, tb.Margin.Top, x += 5, 0);
            };
            dt.Start();
        }

        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            ((Button)sender).Content = "待機中...";
            comments.Topmost = true;
            comments.Show();
            await ConnectToServer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string cmts = "";
            messages.ForEach(x => cmts += x + Environment.NewLine);
            MessageBox.Show("コピーしました。");
        }
    }
}
