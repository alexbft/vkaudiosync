using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace vkaudiosync
{
    [ComVisible(true)]
    public class Helpers
    {
        private HelpersImpl impl;
        
        public Helpers(WebBrowser browser)
        {
            this.impl = new HelpersImpl(browser);
        }

        public void ajaxGet(string url, int cbId)
        {
            try
            {
                impl.ajaxGet(url, cbId);
            }
            catch (Exception e)
            {
                impl.errorCallback(cbId, e);
            }
        }

        public void auth(string url, int cb)
        {
            try
            {
                impl.auth(url, cb);
            }
            catch (Exception e)
            {
                impl.errorCallback(cb, e);
            }
        }

        public string browseFolder(string old)
        {
            return impl.browseFolder(old);
        }

        public string markExistingFiles(string json)
        {
            return impl.markExistingFiles(json);
        }

        public bool isFolderOk(string folder)
        {
            return impl.isFolderOk(folder);
        }

        public void startDownload(int id, string url, string folder, string fileName)
        {
            impl.startDownload(id, url, folder, fileName);
        }

        public void cancelDownloads()
        {
            impl.cancelDownloads();
        }

        public string getLastFolder()
        {
            return impl.getLastFolder();
        }
    }
}
