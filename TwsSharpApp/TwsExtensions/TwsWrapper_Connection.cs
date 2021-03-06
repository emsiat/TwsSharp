﻿using IBApi;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TwsSharpApp
{
    public partial class TwsWrapper : EWrapper
    {
        public TwsWrapper() : base()
        {
        }

        //
        // Handling Next Orderd ID
        //
#region Handling Next Orderd ID
        private static int nextOrderId;

        public int NextOrderId
        {
            get { return nextOrderId; }
            set { nextOrderId = value; }
        }

        private readonly object orderId_Lock = new object();
        public void nextValidId(int orderId)
        {
            Debug.WriteLine("Next Valid Id: " + orderId);
            lock (orderId_Lock) { NextOrderId = orderId; }
        }

        public int nextValidId()
        {
            lock (orderId_Lock) { ++NextOrderId; }

            return NextOrderId;
        }
#endregion

        //
        // Handling Client Socket
        //
#region Handling Client Socket and Signal
        public event EventHandler<SocketConnected_EventArgs> SocketConnected_Event;
        static EClientSocket clientSocket = null;
        public EClientSocket ClientSocket
        {
            get
            {
                if (clientSocket == null) clientSocket = new EClientSocket(this, Signal);
                return clientSocket;
            }
            set { clientSocket = value; }
        }

        public void REQ_OpenSocket(string connIp, int connPort, int connClientID)
        {
            if (ClientSocket.IsConnected()) return;

            Semaphore     semaphore = new Semaphore(0, 1);
            EReaderSignal readerSignal = Signal;
            EReader       reader = null;
       
            new Thread(() => 
            {
                try
                {
                    ClientSocket.eConnect(connIp, connPort, connClientID);

                    //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
                    reader = new EReader(ClientSocket, readerSignal);
                    reader.Start();
                }
                catch(Exception ex)
                {
                    error(ex.Message);
                }
                finally
                {
                    semaphore.Release();              
                }

                SocketConnected_Event?.Invoke(this, new SocketConnected_EventArgs(ClientSocket.IsConnected())); 
            }) { IsBackground = true }.Start();
 
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => 
            {
                semaphore.WaitOne();

                try
                {
                    while (ClientSocket.IsConnected())
                    {
                        readerSignal.waitForSignal();
                        reader.processMsgs();

                        // Just for testing catch inside this loop:
                        //EReader reader2 = null;
                        //reader2.processMsgs();
                    }
                }
                catch(Exception ex)
                {
                    error(ex.Message);
                    error("A critical error occured. You need to restart the application!");
                }
                finally
                {
                    ClientSocket.Close();
                }
            }) { IsBackground = true }.Start();
        }

        public void CloseSocket()
        {
            if (ClientSocket.IsConnected()) ClientSocket.eDisconnect();
        }

        public event EventHandler ConnectionClosed_Event;
        public virtual void connectionClosed()
        {
            ConnectionClosed_Event?.Invoke(this, new EventArgs());
        }

        public bool IsConnected { get { return ClientSocket.IsConnected(); } }

        public void connectAck()
        {
            if (ClientSocket.AsyncEConnect)
                ClientSocket.startApi();
        }

        static EReaderSignal signal;
        public EReaderSignal Signal
        {
            get
            {
                if (signal == null) signal = new EReaderMonitorSignal();
                return signal;
            }
        }
        #endregion

        #region Receive Error Events
        public event EventHandler<ErrorRecv_EventArgs> ErrorReceived_Event;

        public virtual void error(string str)
        {
            Debug.WriteLine("Error: " + str + "\n");
            ErrorReceived_Event?.Invoke(this, new ErrorRecv_EventArgs(new TwsError(str)));
        }

        public virtual void error(int id, int errorCode, string errorMsg)
        {
            Debug.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            ErrorReceived_Event?.Invoke(this, new ErrorRecv_EventArgs(new TwsError(id, errorCode, errorMsg)));
        }
        #endregion
    }
}
