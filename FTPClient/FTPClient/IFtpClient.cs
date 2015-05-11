using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    public interface IFtpClient : IDisposable
    {
        /// <summary>
        /// 開啟 ftp 或是 ftp proxy 連線
        /// </summary>
        /// <param name="host">ftp host</param>
        /// <param name="port">ftp port</param>
        void Open(string host, int port);

        /// <summary>
        ///  使用指定的 ftp user/password 登入ftp server
        /// </summary>
        /// <param name="user">ftp user</param>
        /// <param name="pwd">ftp password</param>
        void Logon(string user, string pwd);

        /// <summary>
        ///  使用 proxy 登入指定的 ftp server user after logon
        /// </summary>
        /// <param name="proxyUser">proxy user id</param>
        /// <param name="proxyPasswd">proxy user password</param>
        /// <param name="destUser">destination user id(user@host:port)</user></param>
        /// <param name="destPasswd">destination user password</param>
        void Logon(string proxyUser, string proxyPwd, string destUser, string destPwd);

        /// <summary>
        ///    Delete file from ftp server
        /// </summary>
        /// <param name="sName">file name</param>
        /// <param name="fIgnoreNotExist">Fire exceiption if the file does not exist when the flag set to false.</param>
        void DeleteFile(string sName, bool fIgnoreNotExist);

        /// <summary>
        ///   Get the file list of current directory
        /// </summary>
        /// <returns>string array of files</returns>
        string[] GetDirectoryList();

        /// <summary>
        /// Send file by stream with name
        /// </summary>
        /// <param name="name">name of file to send to ftp server</param>
        /// <param name="stream">steam of source file</param>
        void SendFile(String name, String sourcePath);

        /// <summary>
        ///   get a single file from server
        /// </summary>
        /// <param name="sName">file name</param>
        /// <param name="stream">stream of file</param>
        void GetFile(string sName, String destPath);


        /// <summary>
        /// Close ftp connection
        /// </summary>
        void Close();

        bool FileExists(string remoteFile);
    }

}
