using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;

namespace SampleHttpServer
{
    class Server
    {
        private HttpListener listener = new HttpListener();
        private readonly object locker = new object();
        private readonly string[] accept_urls = new[] { "/control", "/information" };
        public Action<string> OnLogWrite;// ログ出力用
        #region Start/Stop/IsListening
        public void Start()
        {
            lock (this.locker)
            {
                this.listener.Prefixes.Add(ConfigurationManager.AppSettings["prefix"]);
                this.listener.Start();
                this.listener.BeginGetContext(this.OnRequested, this.listener);
            }
        }
        public void Stop()
        {
            lock (this.locker)
            {
                this.listener.Close();
                this.listener = new HttpListener();
            }
        }
        public bool IsListening
        {
            get { return this.listener.IsListening; }
        }
        #endregion
        // 受信イベント
        public void OnRequested(System.IAsyncResult result)
        {
            if (!this.IsListening)
            {
                // 受信開始→終了でOnRequestedイベントが発火するため、受信待機状態でない時はSkip
                return;
            }
            try
            {
                HttpListenerContext context = this.listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                using (HttpListenerResponse response = context.Response)
                {
                    OnLogWrite(string.Format("time:{0},url:{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), request.RawUrl));
                    this.requestParser(context);
                    OnLogWrite(response.StatusCode.ToString());
                    //Debug.Assert();
                }
            }
            finally
            {
                this.listener.BeginGetContext(this.OnRequested, this.listener);
            }
        }
        // 受信内容を解析
        private void requestParser(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            // この箇所に送信元のIPアドレスを元にした送信リクエストチェックがほぼ必要。
            OnLogWrite(request.RemoteEndPoint.ToString());
            // 受け入れるURL
            if (!accept_urls.Any(a => a.Equals(request.RawUrl, StringComparison.OrdinalIgnoreCase)))
            {
                // todo: /favicon.icoの扱い
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            // /control
            if (request.RawUrl.StartsWith(accept_urls[0]))
            {
                string s = GetRequestPostData(request);
                OnLogWrite(s);
                if(s.Length == 0)
                {
                    // postデータなし。
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                // 受信した信号を元にこの部分にスレイブ側の処理を記述！！
                response.StatusCode = GetRandomStatusCode();
                return;
            }
            // /information
            if (request.RawUrl.StartsWith(accept_urls[1]))
            {
                response.StatusCode = GetRandomStatusCode();
                return;
            }
        }
        #region static
        // ランダムにHTTPステータスコードを返す。
        private static int GetRandomStatusCode()
        {
            var rnd = new System.Random();
            var code = new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.BadRequest };

            // 今はステータスコード:200固定
            return (int)HttpStatusCode.OK;
            //return (int)code[rnd.Next(code.Length)];
        }
        // POSTデータを取得
        private static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return string.Empty;
            }
            using (System.IO.Stream body = request.InputStream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion
    }
}
