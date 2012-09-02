using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace vkaudiosync
{
    class HelpersImpl
    {
        private WebBrowser browser;

        private Dictionary<int, CancellationTokenSource> downloads;

        internal HelpersImpl(WebBrowser browser)
        {
            this.browser = browser;
            this.downloads = new Dictionary<int, CancellationTokenSource>();
        }

        private void resolveCallback(int cbId, string error, object result) {
            browser.Document.InvokeScript("$resolveCallback", new object[] { cbId, error, result });
        }

        internal void resolveCallback(int cbId, object result)
        {
            resolveCallback(cbId, null, result);
        }

        internal void errorCallback(int cbId, Exception ex)
        {
            resolveCallback(cbId, ex.Message, null);
        }

        internal async void ajaxGet(string url, int cbId)
        {
            var req = createHTTPRequest(url);
            var res = await req.GetResponseAsync();
            resolveCallback(cbId, getResponse(res));
        }

        private static HttpWebRequest createHTTPRequest(string url)
        {
            HttpWebRequest result = (HttpWebRequest)WebRequest.Create(url);
            result.Proxy = null;
            result.Timeout = 10000;
            return result;
        }

        private static string getResponse(WebResponse res)
        {
            return new StreamReader(res.GetResponseStream()).ReadToEnd();
        }

        internal void auth(string url, int cb)
        {
            var authForm = new AuthForm();
            authForm.init(url, (id, token) =>
            {
                resolveCallback(cb, id + ":" + token);
            }, (err, desc) =>
            {
                resolveCallback(cb, err, desc);
            });
            authForm.Show();
        }

        internal string browseFolder(string old)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = old;
            dialog.Description = "Выберите папку для синхронизации.";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else
            {
                return null;
            }
        }

        internal string markExistingFiles(string json)
        {
            return toJson(_markExistingFiles(fromJson<MarkExistingFilesRq>(json)));
        }

        public class MarkExistingFilesRq
        {
            public string folder;
            public dynamic[] files;
        };

        private dynamic _markExistingFiles(MarkExistingFilesRq p)
        {
            if (!isFolderOk(p.folder)) return null;
            saveFolder(p.folder);
            var res = new List<string>();
            foreach (dynamic f in p.files)
            {
                string name = cleanFilename(HttpUtility.HtmlDecode(f["artist"] + " - " + f["title"] + ".mp3"));
                string fullName = Path.Combine(p.folder, name);
                if (File.Exists(fullName))
                {
                    res.Add(f["aid"].ToString());
                }
            }
            return new { d = res };
        }

        private void saveFolder(string p)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("last_folder");
            config.AppSettings.Settings.Add("last_folder", p);
            config.Save(ConfigurationSaveMode.Minimal);
        }

        private string cleanFilename(string filename)
        {
            var sb = new StringBuilder();
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in filename)
            {
                if (invalid.Contains(c)) sb.Append('_');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        public bool isFolderOk(string p)
        {
            return Directory.Exists(p);
        }

        private T fromJson<T>(string json)
        {
            var dec = new JavaScriptSerializer();
            return dec.Deserialize<T>(json);
        }

        private string toJson(object obj)
        {
            var enc = new JavaScriptSerializer();
            return enc.Serialize(obj);
        }

        internal void startDownload(int id, string url, string folder, string fileName)
        {
            var source = new CancellationTokenSource();
            downloads[id] = source;
            fileName = Path.Combine(folder, cleanFilename(fileName));
            startDownloadAsync(id, url, fileName, source.Token);
        }

        private const int bufSize = 4096;

        private async void startDownloadAsync(int id, string url, string fileName, CancellationToken cancelToken)
        {
            try
            {
                var req = createHTTPRequest(url);
                var res = await req.GetResponseAsync();
                cancelToken.ThrowIfCancellationRequested();
                var stream = res.GetResponseStream();
                var totalLength = (int)res.ContentLength;
                var ms = new MemoryStream(totalLength);
                var buf = new Byte[bufSize];
                var totalRead = 0;
                var watch = new Stopwatch();
                watch.Start();
                while (stream.CanRead)
                {
                    var readBytes = await stream.ReadAsync(buf, 0, bufSize, cancelToken);
                    cancelToken.ThrowIfCancellationRequested();
                    if (readBytes == 0) break;
                    ms.Write(buf, 0, readBytes);
                    totalRead += readBytes;
                    if (watch.Elapsed > new TimeSpan(TimeSpan.TicksPerSecond / 2))
                    {
                        sendDownloadProgress(id, totalRead, totalLength);
                        watch.Restart();
                    }
                }
                var fs = new FileStream(fileName, FileMode.Create);
                ms.WriteTo(fs);
                fs.Close();
                sendDownloadEnd(id);
                downloads.Remove(id);
            }
            catch (Exception ex)
            {
                sendDownloadError(id, ex.Message);
            }
        }

        private void sendDownloadEnd(int id)
        {
            browser.Document.InvokeScript("downloadEnd", new object[] { id });
        }

        private void sendDownloadProgress(int id, int totalRead, int totalLength)
        {
            browser.Document.InvokeScript("downloadProgress", new object[] { id, totalRead, totalLength });
        }

        private void sendDownloadError(int downloadId, string err)
        {
            browser.Document.InvokeScript("downloadError", new object[] { downloadId, err });
        }

        internal void cancelDownloads()
        {
            foreach (var it in downloads)
            {
                it.Value.Cancel();
            }
            downloads.Clear();
        }

        internal string getLastFolder()
        {
            return ConfigurationManager.AppSettings["last_folder"];
        }
    }
}
