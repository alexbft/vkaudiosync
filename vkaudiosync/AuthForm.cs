using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace vkaudiosync
{
    public partial class AuthForm : Form
    {
        private Action<string, string> onSuccess;
        private Action<string, string> onError;
        
        public AuthForm()
        {
            InitializeComponent();
        }

        internal void init(string url, Action<string, string> onSuccess, Action<string, string> onError)
        {
            webBrowser1.Url = new Uri(url);
            this.onSuccess = onSuccess;
            this.onError = onError;
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                Uri url = e.Url;
                if (url.AbsolutePath == "/blank.html")
                {
                    var q = HttpUtility.ParseQueryString(url.Fragment.Substring(1)); 
                    if (q["error"] != null)
                    {
                        onError(q["error"], q["error_description"]);
                    }
                    else
                    {
                        onSuccess(q["user_id"], q["access_token"]);
                    }
                    Hide();
                    var timer = new System.Threading.Timer(_ =>
                    {
                        Close();
                    }, null, 10000, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                onError(ex.Message, "");
            }
        }

        private void AuthForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.onError != null)
            {
                this.onError("cancel", "Пользователь закрыл окно авторизации");
            }
        }
    }
}
