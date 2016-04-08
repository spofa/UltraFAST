using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RemoteClient
{
    public class TcpServerConnection
    {
        private TcpClient m_socket;

        private List<byte[]> messagesToSend;

        private int attemptCount;

        private Thread m_thread;

        private DateTime m_lastVerifyTime;

        private Encoding m_encoding;

        public Thread CallbackThread
        {
            get
            {
                return this.m_thread;
            }
            set
            {
                if (!this.canStartNewThread())
                {
                    throw new Exception("Cannot override TcpServerConnection Callback Thread. The old thread is still running.");
                }
                this.m_thread = value;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
            set
            {
                this.m_encoding = value;
            }
        }

        public DateTime LastVerifyTime
        {
            get
            {
                return this.m_lastVerifyTime;
            }
        }

        public TcpClient Socket
        {
            get
            {
                return this.m_socket;
            }
            set
            {
                this.m_socket = value;
            }
        }

        public TcpServerConnection(TcpClient sock, Encoding encoding)
        {
            this.m_socket = sock;
            this.messagesToSend = new List<byte[]>();
            this.attemptCount = 0;
            this.m_lastVerifyTime = DateTime.UtcNow;
            this.m_encoding = encoding;
        }

        private bool canStartNewThread()
        {
            if (this.m_thread == null)
            {
                return true;
            }
            if ((this.m_thread.ThreadState & (ThreadState.Stopped | ThreadState.Aborted)) == ThreadState.Running)
            {
                return false;
            }
            return (this.m_thread.ThreadState & ThreadState.Unstarted) == ThreadState.Running;
        }

        public bool connected()
        {
            bool connected;
            try
            {
                connected = this.m_socket.Connected;
            }
            catch (Exception exception)
            {
                connected = false;
            }
            return connected;
        }

        public void forceDisconnect()
        {
            lock (this.m_socket)
            {
                this.m_socket.Close();
            }
        }

        public bool hasMoreWork()
        {
            if (this.messagesToSend.Count > 0)
            {
                return true;
            }
            if (this.Socket.Available <= 0)
            {
                return false;
            }
            return this.canStartNewThread();
        }

        public bool processOutgoing(int maxSendAttempts)
        {
            bool flag;
            lock (this.m_socket)
            {
                if (!this.m_socket.Connected)
                {
                    this.messagesToSend.Clear();
                    flag = false;
                }
                else if (this.messagesToSend.Count != 0)
                {
                    NetworkStream stream = this.m_socket.GetStream();
                    try
                    {
                        stream.Write(this.messagesToSend[0], 0, (int)this.messagesToSend[0].Length);
                        lock (this.messagesToSend)
                        {
                            this.messagesToSend.RemoveAt(0);
                        }
                        this.attemptCount = 0;
                    }
                    catch (IOException oException)
                    {
                        TcpServerConnection tcpServerConnection = this;
                        tcpServerConnection.attemptCount = tcpServerConnection.attemptCount + 1;
                        if (this.attemptCount >= maxSendAttempts)
                        {
                            lock (this.messagesToSend)
                            {
                                this.messagesToSend.RemoveAt(0);
                            }
                            this.attemptCount = 0;
                        }
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        this.m_socket.Close();
                        flag = false;
                        return flag;
                    }
                    return this.messagesToSend.Count != 0;
                }
                else
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void sendData(string data)
        {
            byte[] bytes = this.m_encoding.GetBytes(data);
            lock (this.messagesToSend)
            {
                this.messagesToSend.Add(bytes);
            }
        }

        public bool verifyConnected()
        {
            bool flag = (this.m_socket.Client.Available != 0 || !this.m_socket.Client.Poll(1, SelectMode.SelectRead) ? true : this.m_socket.Client.Available != 0);
            this.m_lastVerifyTime = DateTime.UtcNow;
            return flag;
        }
    }
}
