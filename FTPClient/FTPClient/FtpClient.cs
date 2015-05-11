using System;
using System.Linq;
using Common.Logging;
using WinSCP;
using System.IO;
using System.Reflection;
using System.ComponentModel;
namespace FTPClient
{
    public class FtpClient :IFtpClient
    {
        #region Field
        private static readonly ILog log = LogManager.GetLogger(typeof(FtpClient));
        private SessionOptions sessionOptions = null;
        private Session session = null;
        private RemoteDirectoryInfo remoteDirectoryInfo = null;
        private TransferOptions transferOptions = null;
        public string currentRemotePath = null;
        public string currentLocalPath = null;
        private string _errorMsg = null;
        private string _proxyHost = null;
        private string _proxyPort = null;
        #endregion
        
        #region Property
        public string ErrorMsg { 
            private set 
            {
                log.Error(this._errorMsg = value);
            }
            get 
            {
                return this._errorMsg;
            }
        }
        public string ProxyHost { get { return this._proxyHost; } set { this._proxyHost = value; } }
        public string ProxyPort { get { return this._proxyPort; } set { this._proxyPort = value; } }
        #endregion
        
        #region Constructor
        public FtpClient()
        {
            this.Init();
        }
        public FtpClient(string proxyHost, string proxyPort)
        {
            this.Init();
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
        }
        #endregion

        #region Event
        void session_FileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
            log.Info(string.Format("檔案傳輸進度: {0}檔案傳輸進度 {1} %", e.FileName, e.FileProgress * 100));
        }
        void session_OutputDataReceived(object sender, OutputDataReceivedEventArgs e)
        {
            log.Info("Session通訊狀態:" + e.Data);
        }

        void session_Failed(object sender, FailedEventArgs e)
        {
            this.ErrorMsg = "Session通訊失敗:" + e.Error;
        }

        void session_FileTransferred(object sender, TransferEventArgs e)
        {
            if (e.Error == null)
            {
                log.Info(string.Format("檔案傳輸成功: {0} ", e.FileName));
            }
            else
            {
                this.ErrorMsg = "檔案傳輸失敗: 檔案名稱:" + e.FileName + "\r錯誤訊息:" + e.Error.StackTrace;
                throw new Exception("Upload of " + e.FileName + " failded: " + e.Error.Message);
            }
            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    log.Info("檔案權限: " + e.Chmod.FileName + "設定 \r 權限訊息:" + e.Chmod.FilePermissions);
                }
                else
                {
                    this.ErrorMsg = "檔案權限錯誤:  檔案名稱:" + e.Chmod.FileName + "\r權限錯誤訊息:" + e.Chmod.Error.StackTrace;
                    throw new Exception("Permision of " + e.Chmod.FileName + " failed" + e.Chmod.Error.Message);
                }
            }
            else
            {
                log.Info(string.Format("檔案權限: {0} 維持預設值", e.Destination));
            }
            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {
                    log.Info(string.Format("時間戳設置: {0} set to {1}", e.Touch.FileName, e.Touch.LastWriteTime));
                }
                else
                {
                    this.ErrorMsg = string.Format("時間戳設置錯誤: {0} \r 錯誤訊息: {1}", e.Touch.FileName, e.Touch.Error.StackTrace);
                    throw new Exception("Setting timestamp of " + e.Touch.FileName + " failed: " + e.Touch.Error.Message);
                }
            }
            else
            {
                log.Info(string.Format("時間戳設置: {0} 維持預設值(current time)", e.Destination));
            }
        } 
        #endregion

        #region Method
        protected virtual void Init()
        {
            this.sessionOptions = new SessionOptions()
            {
                Protocol = Protocol.Ftp,
                FtpMode = FtpMode.Passive,
                FtpSecure = FtpSecure.None,
                TimeoutInMilliseconds = 36000
            };
            
            this.transferOptions = new TransferOptions()
            {
                TransferMode = TransferMode.Binary
            };

            this.session = new Session() { Timeout = new TimeSpan(0,5,0) };
            this.session.FileTransferProgress += session_FileTransferProgress;
            this.session.FileTransferred += session_FileTransferred;
            this.session.Failed += session_Failed;
            this.session.OutputDataReceived += session_OutputDataReceived;

            this.currentRemotePath = @"/";
            this.currentLocalPath = this.CreateLocalFolder("FTP\\", false);
        }

        public void Open(string host, int port)
        {
            if (this.session == null)
            {
                this.Init();
            }
            this.sessionOptions.HostName = host;
            this.sessionOptions.PortNumber = port;
        }

        public void Logon(string user, string pwd)
        {
            if (this.session != null)
            {
                this.sessionOptions.UserName = user;
                this.sessionOptions.Password = pwd;

                try
                {
                    if (this.session.Opened)
                    {
                        this.session.Dispose();
                        this.Open(this.sessionOptions.HostName, this.sessionOptions.PortNumber);
                        this.ErrorMsg = "連線錯誤: Session is already opened and reStart Session";
                    }
                    this.session.Open(this.sessionOptions);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = "連線錯誤:" + ex.StackTrace;
                    throw new Exception("Session connection failed:" + ex.Message);
                }
            }
            else
            {
                this.Open(this.sessionOptions.HostName, this.sessionOptions.PortNumber);
                this.Logon(user, pwd);
            }
        }

        public void Logon(string proxyUser, string proxyPwd, string destUser, string destPwd)
        {
            this.sessionOptions.AddRawSettings("ProxyHost",ProxyHost);
            this.sessionOptions.AddRawSettings("ProxyPort",ProxyPort);
            this.sessionOptions.AddRawSettings("FtpProxyLogonType", "2");
            this.sessionOptions.AddRawSettings("ProxyUsername", proxyUser);
            this.sessionOptions.AddRawSettings("ProxyPassword", proxyPwd);
            this.sessionOptions.UserName = destUser;
            this.sessionOptions.Password = destPwd;
            if (this.session != null)
            {
                try
                {
                    if (this.session.Opened)
                    {
                        this.session.Dispose();
                        this.Open(this.sessionOptions.HostName, this.sessionOptions.PortNumber);
                        this.ErrorMsg = "連線錯誤: Session is already opened and reStart Session";
                    }
                    this.session.Open(this.sessionOptions);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = "連線錯誤:" + ex.StackTrace;
                    throw new Exception("Session connection failed:" + ex.Message);
                }
            }
            else
            {
                this.Open(this.sessionOptions.HostName, this.sessionOptions.PortNumber);
                this.Logon(proxyUser, proxyPwd, destUser, destPwd);
            }
        }

        public void DeleteFile(string sName, bool fIgnoreNotExist)
        {
            string fullFilePath = Path.Combine(this.currentRemotePath,sName);
            if (!this.FileExists(sName))
            {
                if (fIgnoreNotExist)
                {
                    return;
                }
                this.ErrorMsg = "刪除檔案失敗: 檔案名稱:" + fullFilePath + "不存在!";
                throw new Exception("Delete " + fullFilePath + " failed: not exists");
            }
            else
            {
                try
                {
                    RemovalOperationResult removeResult = this.session.RemoveFiles(fullFilePath);
                    removeResult.Check();
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = "刪除檔案失敗: 檔案名稱:" + fullFilePath + "\r錯誤訊息:" + ex.StackTrace;
                    throw new Exception("Delete " + fullFilePath + " failed: " + ex.Message);
                }
            }
        }

        public string[] GetDirectoryList()
        {
            try
            {
                //判斷是否為資料夾
                if (!Path.HasExtension(this.currentRemotePath))
                {
                    remoteDirectoryInfo = this.session.ListDirectory(this.currentRemotePath);
                }
                else
                {
                    this.currentRemotePath = this.GetParantDirectory(this.currentRemotePath, 1);//若為檔案則當前目錄改成上一層目錄
                    remoteDirectoryInfo = this.session.ListDirectory(this.currentRemotePath);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMsg = "取得遠端目錄失敗: 遠端路徑:" + this.currentRemotePath + "\r錯訊訊息:" + ex.StackTrace;
                throw new Exception("Get directory " + this.currentRemotePath + "\r failed: " + ex.Message);
            }
            string[] directoryList = new string[remoteDirectoryInfo.Files.Count()];
            for (int i = 0; i < remoteDirectoryInfo.Files.Count(); i++)
            {
                directoryList[i] = string.Format("{0}",Path.Combine(path1:this.currentRemotePath,
                                                                    path2:remoteDirectoryInfo.Files[i].Name));
            }
            return directoryList;
        }

        public void ExecuteCommand(string command)
        {
            if (this.session != null)
            {
                try
                {
                    this.session.ExecuteCommand(command);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = "執行命令失敗: " + ex.StackTrace;
                    throw new Exception("ExecuteCommand failed:" + ex.StackTrace);
                }
            }
        }

        public string[] GetFullDirectoryList()
        {            
            try
            {
                remoteDirectoryInfo = this.session.ListDirectory(this.currentRemotePath);
            }
            catch (Exception ex) 
            { 
                this.ErrorMsg = "取得詳細目錄失敗: " + ex.StackTrace;
                throw new Exception("Get full directory list failed: " + ex.Message);
            }
            string[] directoryList = new string[remoteDirectoryInfo.Files.Count];
            for (int i = 0; i < remoteDirectoryInfo.Files.Count; i++)
            {
                directoryList[i] = string.Format("{0} | {1} | {2} | {3} | {4} | {5}",
                    remoteDirectoryInfo.Files[i].Name,
                    remoteDirectoryInfo.Files[i].Length,
                    remoteDirectoryInfo.Files[i].FileType,
                    remoteDirectoryInfo.Files[i].LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"),
                    remoteDirectoryInfo.Files[i].IsDirectory,
                    remoteDirectoryInfo.Files[i].FilePermissions.Text);
            }
            return directoryList;
        }

        public void SendFile(string name, string sourcePath)
        {
            if (Path.HasExtension(sourcePath))
            {
                sourcePath = Path.Combine(this.GetParantDirectory(sourcePath, 1), name);
            }
            try
            {
                string fullLoaclPath = Path.Combine(sourcePath, name);
                if (!this.FileExists(name) && File.Exists(fullLoaclPath))
                {
                    TransferOperationResult transferResult = this.session.PutFiles(fullLoaclPath, this.currentRemotePath, false, this.transferOptions);
                    transferResult.Check();
                    foreach (TransferEventArgs fileTrans in transferResult.Transfers)
                    {
                        if (fileTrans.Error == null)
                        {
                            log.Info("傳送檔案成功: " + fileTrans.FileName);
                        }
                        else
                        {
                            this.ErrorMsg = "傳送檔案失敗: " + fileTrans.FileName + "\r錯誤訊息: " + fileTrans.Error.StackTrace;
                        }
                    }
                }
                else
                {
                    this.ErrorMsg = "傳送檔案失敗: " + name + "檔案已存在";
                }
            }
            catch (Exception ex)
            {
                this.ErrorMsg = "傳送檔案失敗: " + name + "\r錯誤訊息:" + ex.StackTrace;
                throw new Exception("Send file " + name + " failed:" + ex.Message);
            }
        }

        public void GetFile(string sName, string destPath)
        {
            string remoteFullPath = this.currentRemotePath + "/" + sName;
            try
            {
                if (this.session.FileExists(remoteFullPath))
                {
                    bool download = false;
                    if (Path.HasExtension(destPath) && File.Exists(destPath))
                    {
                        DateTime remoteWriteTime = this.session.GetFileInfo(remoteFullPath).LastWriteTime;
                        DateTime localWriteTime = File.GetLastWriteTime(Path.Combine(destPath,sName));
                        if (remoteWriteTime > localWriteTime)
                        {
                            download = true;
                        }
                        else
                        {
                            download = false;
                        }
                    }
                    else
                    {
                        download = true;
                    }
                    if (download)
                    {
                        try { 
                            this.session.GetFiles(remoteFullPath, destPath).Check(); 
                        }
                        catch (Exception ex) {
                            this.ErrorMsg = "下載檔案失敗: 遠端檔案名稱:" + remoteFullPath + "\r錯誤訊息:" + ex.StackTrace;
                            throw new Exception("download " + remoteFullPath + " file failed:" + ex.Message);
                        }
                    }
                }
                else
                {
                    this.ErrorMsg = "下載檔案失敗: 遠端檔案名稱:" + sName + " 不存在!";
                }
            }
            catch (Exception ex) {
                this.ErrorMsg = "下載檔案失敗: 遠端檔案名稱:" + sName + "\r 錯誤訊息:" + ex.StackTrace;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public bool FileExists(string remoteFile)
        {
            string[] fileList = this.GetDirectoryList();
            foreach (string file in fileList)
            {
                if (Path.GetFileName(file).Equals(remoteFile))
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveSessionEvent(Session session)
        {
            FieldInfo f = typeof(Session).GetField("_events", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = f.GetValue(session);
            PropertyInfo pi = session.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            EventHandlerList list = (EventHandlerList)pi.GetValue(session, null);
            list.RemoveHandler(obj, list[obj]);
        }

        //check folder is exist , delete all files in folder if true
        private string CreateLocalFolder(string folderName, bool deleteFolderAllFiles = false)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, folderName);
            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = "Client端目錄建立失敗: " + ex.StackTrace;
                    return basePath;
                }
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
                else
                {
                    return basePath;
                }
            }
            if (deleteFolderAllFiles)
            {
                this.DeleteAllFiles(new DirectoryInfo(fullPath));//刪除目錄內所有層數非目錄的檔案
            }
            return fullPath;

        }

        /// <summary>
        /// 刪除Client端目錄內所有資料夾的檔案(包含目錄)
        /// </summary>
        /// <param name="directoryInfo">current directory information</param>
        private void DeleteAllFiles(DirectoryInfo directoryInfo)
        {
            try
            {
                directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(file => file.Delete());
            }
            catch (Exception ex)
            {
                log.Error("刪除檔案失敗: 刪除本機檔案錯誤訊息:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 返回當前目錄的上幾層目錄
        /// </summary>
        /// <param name="currentPath">current Path</param>
        /// <param name="parentCount">return parant path layer count</param>
        /// <returns>("/a/b/c/",1)-->/a/b/</returns>
        private string GetParantDirectory(string currentPath, int parentCount)
        {
            if (string.IsNullOrEmpty(currentPath) || parentCount < 1)
            {
                return currentPath;
            }
            string parentPath = System.IO.Path.GetDirectoryName(currentPath);
            if (--parentCount > 0)
            {
                return GetParantDirectory(parentPath, parentCount);
            }
            return parentPath;
        }

        public void Dispose()
        {
            if (this.session != null)
            {
                //RemoveSessionEvent(session);
                this.session.Dispose();
                this.session = null;
            }
            this.sessionOptions = null;
            this.currentLocalPath = null;
            this.currentRemotePath = null;
            this.ErrorMsg = null;
            this.ProxyHost = null;
            this.ProxyPort = null;
            this.remoteDirectoryInfo = null;
            this.sessionOptions = null;
            this.transferOptions = null;
        }
        #endregion
    }
}
