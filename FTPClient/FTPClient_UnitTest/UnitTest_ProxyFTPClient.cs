using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FTPClient;
using Common.Logging;
using System.IO;
namespace FTPClient_UnitTest
{
    [TestClass]
    public class UnitTest_ProxyFTPClient
    {
        private IFtpClient proxyftpClient = null;
        private static readonly ILog log2 = LogManager.GetLogger(typeof(UnitTest_ProxyFTPClient));
        private string host = null;
        private int port = -1;
        private string user = null;
        private string pwd = null;

        [TestInitialize]
        public void Init()
        {
            proxyftpClient = new FtpClient("10.27.68.155", "8021");
            host = "10.27.68.155";
            port = 21;
            user ="icftp";
            pwd = "icftp";
            proxyftpClient.Open(host, port);
            proxyftpClient.Logon(user, pwd);
            log2.Debug("開啟FTP " + ", host:" + host + ", port:" + port + ", user:" + user + ", pwd:" + pwd);
        }
        
        [TestMethod]
        public void TestMethod_GetDirectory()
        {
            string[] fileList = proxyftpClient.GetDirectoryList();
            foreach (string fileInfo in fileList)
            {
                log2.Debug("檔案夾列表: " + fileInfo);
            }
            Assert.IsTrue(fileList.Length > 0);
        }

        [TestMethod]
        public void TestMethod_SendFile()
        {
            string file = @"r5.jpg";
            string sourcePath = @"D:\FTP\";
            bool expected = true;
            bool actual = false;
            bool uploadfailed = true;
            if (!proxyftpClient.FileExists(file) && File.Exists(sourcePath + file))
            {
                proxyftpClient.SendFile(file, sourcePath);
            }
            string[] fileList = proxyftpClient.GetDirectoryList();
            foreach (string fileInfo in fileList)
            {
                if (Path.GetFileName(fileInfo) == file)
                {
                    log2.Debug("上傳檔案成功: " + fileInfo);
                    actual = true;
                    uploadfailed = false;
                    break;
                }
            }
            if (uploadfailed)
            {
                log2.Debug("上傳檔案失敗: " + file);
            }
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_DeleteFile()
        {
            string file = @"r5.jpg";
            
            bool IsDelete = false;
            string[] fileList = proxyftpClient.GetDirectoryList();
            foreach (string fileInfo in fileList)
            {
                if (Path.GetFileName(fileInfo) == file)
                {
                    proxyftpClient.DeleteFile(file, false);
                    IsDelete = true;
                    break;
                }
            }
            if (IsDelete)
            {
                string[] refreshList = proxyftpClient.GetDirectoryList();
                bool hasFile = false;
                foreach (string fileInfo in refreshList)
                {
                    if (Path.GetFileName(fileInfo) == file)
                    {
                        hasFile = true;
                        break;
                    }
                }
                if (hasFile)
                {
                    log2.Debug("檔案刪除失敗:" + file);
                }
                else
                {
                    log2.Debug("檔案刪除成功:" + file);
                }
                Assert.IsFalse(hasFile);
            }
            else
            {
                log2.Debug("遠端檔案:" + file + "不存在,無法刪除");
                Assert.IsTrue(IsDelete);
            }
        }

        [TestMethod]
        public void TestMethod_GetFile()
        {
            string file = @"bunny_20140725_12345678.jpg";
            string localPath = @"D:\FTP\";
            if (File.Exists(Path.Combine(localPath, file)))
            {
                File.Delete(Path.Combine(localPath, file));
            }
            proxyftpClient.GetFile(file, Path.Combine(localPath, file));
            if (File.Exists(Path.Combine(localPath, file)))
            {
                log2.Debug("下載檔案成功: " + Path.Combine(localPath, file));
            }
            else
            {
                log2.Debug("下載檔案失敗: " + Path.Combine(localPath, file));
            }
            Assert.IsTrue(File.Exists(Path.Combine(localPath, file)));
        }

        [TestCleanup]
        public void Finalizer()
        {
            proxyftpClient.Dispose();
        }
    }
}
