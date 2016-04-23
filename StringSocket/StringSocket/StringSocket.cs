// Written by Joe Zachary for CS 3500, November 2012
// Revised by Joe Zachary April 2016
// Implemented by Morgan Empey and Braden Klunker for CS 3500, Spring 2016

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CustomNetworking
{
    /// <summary> 
    /// A StringSocket is a wrapper around a Socket.  It provides methods that
    /// asynchronously read lines of text (strings terminated by newlines) and 
    /// write strings. (As opposed to Sockets, which read and write raw bytes.)  
    ///
    /// StringSockets are thread safe.  This means that two or more threads may
    /// invoke methods on a shared StringSocket without restriction.  The
    /// StringSocket takes care of the synchronization.
    /// 
    /// Each StringSocket contains a Socket object that is provided by the client.  
    /// A StringSocket will work properly only if the client refrains from calling
    /// the contained Socket's read and write methods.
    /// 
    /// If we have an open Socket s, we can create a StringSocket by doing
    /// 
    ///    StringSocket ss = new StringSocket(s, new UTF8Encoding());
    /// 
    /// We can write a string to the StringSocket by doing
    /// 
    ///    ss.BeginSend("Hello world", callback, payload);
    ///    
    /// where callback is a SendCallback (see below) and payload is an arbitrary object.
    /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
    /// successfully written the string to the underlying Socket, or failed in the 
    /// attempt, it invokes the callback.  The parameters to the callback are a
    /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
    /// the Exception that caused the send attempt to fail.
    /// 
    /// We can read a string from the StringSocket by doing
    /// 
    ///     ss.BeginReceive(callback, payload)
    ///     
    /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
    /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
    /// string of text terminated by a newline character from the underlying Socket, or
    /// failed in the attempt, it invokes the callback.  The parameters to the callback are
    /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
    /// string or the Exception will be non-null, but nor both.  If the string is non-null, 
    /// it is the requested string (with the newline removed).  If the Exception is non-null, 
    /// it is the Exception that caused the send attempt to fail.
    /// </summary>

    public class StringSocket
    {
        /// <summary>
        /// The type of delegate that is called when a send has completed.
        /// </summary>
        public delegate void SendCallback(Exception e, object payload);

        /// <summary>
        /// The type of delegate that is called when a receive has completed.
        /// </summary>
        public delegate void ReceiveCallback(string s, Exception e, object payload);

        // Underlying socket
        private Socket socket;

        // The character encoding used by this StringSocket
        private Encoding encoding;

        // For decoding incoming byte streams.
        private Decoder decoder;

        // Buffer size for reading incoming bytes
        private const int BUFFER_SIZE = 1024;

        // Text that has been received from the client but not yet dealt with
        private StringBuilder incoming = new StringBuilder();

        // Text that needs to be sent to the client but which we have not yet started sending
        private StringBuilder outgoing = new StringBuilder();

        // Buffers that will contain incoming bytes and characters
        private byte[] incomingBytes = new byte[BUFFER_SIZE];
        private char[] incomingChars = new char[BUFFER_SIZE];

        // Object used for locking the representation during receiving
        private readonly object syncReceive = new object();

        // Object used for locking the representation during sending
        private readonly object syncSend = new object();

        // Indicates that a async send is currently going on
        private bool sendIsOngoing = false;

        // An array of bytes we are going to send
        private byte[] pendingBytes = new byte[0];

        // The index where pendingBytes is currently at
        private int pendingIndex = 0;

        // Thread safe queue to process the send callbacks in the correct order.
        private ConcurrentQueue<SendState> sendCallbackQueue = new ConcurrentQueue<SendState>();

        // Thread safe queue to process the recieve callbacks in the correct order.
        private ConcurrentQueue<ReceiveState> receiveStateQueue = new ConcurrentQueue<ReceiveState>();
        int tempCount = 1; // TODO remove this (for debugging receive only)

        /// <summary>
        /// Creates a StringSocket from a regular Socket, which should already be connected.  
        /// The read and write methods of the regular Socket must not be called after the
        /// StringSocket is created.  Otherwise, the StringSocket will not behave properly.  
        /// The encoding to use to convert between raw bytes and strings is also provided.
        /// </summary>
        public StringSocket(Socket s, Encoding e)
        {
            socket = s;
            encoding = e;
            decoder = encoding.GetDecoder();
        }

        /// <summary>
        /// Shuts down and closes the socket.  No need to change this.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// We can write a string to a StringSocket ss by doing
        /// 
        ///    ss.BeginSend("Hello world", callback, payload);
        ///    
        /// where callback is a SendCallback (see above) and payload is an arbitrary object.
        /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
        /// successfully written the string to the underlying Socket, or failed in the 
        /// attempt, it invokes the callback.  The parameters to the callback are a
        /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
        /// the Exception that caused the send attempt to fail. 
        /// 
        /// This method is non-blocking.  This means that it does not wait until the string
        /// has been sent before returning.  Instead, it arranges for the string to be sent
        /// and then returns.  When the send is completed (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginSend
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginSend must take care of synchronization instead.  On a given StringSocket, each
        /// string arriving via a BeginSend method call must be sent (in its entirety) before
        /// a later arriving string can be sent.
        /// </summary>
        public void BeginSend(string s, SendCallback callback, object payload)
        {
            lock (syncSend)
            {
                // Enqueue the current callback and payload in a thread safe queue. 
                sendCallbackQueue.Enqueue(new SendState(callback, payload));
                SendMessage(s);
            }
        }

        /// <summary>
        /// Sends a string to the client.
        /// </summary>
        private void SendMessage(string lines)
        {
            lock (syncSend)
            {
                outgoing.Append(lines);

                if (!sendIsOngoing)
                {
                    sendIsOngoing = true;
                    SendBytes();
                }
            }

        }

        // TODO: Modify summary of SendBytes to be different than JOEs
        /// <summary>
        /// Attempts to send the entire outgoing string.
        /// This method should not be called unless sendSync has been acquired.
        /// </summary>
        private void SendBytes()
        {
            // If pending index is less than the length of pendingBytes use the underlying socket and call begin send.
            if (pendingIndex < pendingBytes.Length)
            {
                socket.BeginSend(pendingBytes, 0, pendingBytes.Length - pendingIndex, SocketFlags.None, MessageSent, null);
            }
            else if (outgoing.Length > 0)
            {
                pendingBytes = encoding.GetBytes(outgoing.ToString());
                pendingIndex = 0;
                outgoing.Clear();
                socket.BeginSend(pendingBytes, 0, pendingBytes.Length, SocketFlags.None, MessageSent, null);
            }
            else
            {
                sendIsOngoing = false;
            }

            SendState temp;

            // Dequeue the first SendState object from the thread safe queue
            sendCallbackQueue.TryDequeue(out temp);

            // Run the callback on a new thread
            Task.Run(() => temp.Callback(null, temp.Payload));
        }

        /// <summary>
        /// Called when a message has been successfully sent.
        /// </summary>
        private void MessageSent(IAsyncResult result)
        {
            int byteSent = socket.EndSend(result);

            lock (syncSend)
            {
                if (byteSent == 0)
                {
                    socket.Close();
                }
                else
                {
                    pendingIndex += byteSent;
                    SendBytes();
                }
            }
        }

        /// <summary>
        /// We can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload)
        ///     
        /// where callback is a ReceiveCallback (see above) and payload is an arbitrary object.
        /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
        /// string of text terminated by a newline character from the underlying Socket, or
        /// failed in the attempt, it invokes the callback.  The parameters to the callback are
        /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
        /// string or the Exception will be null, or possibly both.  If the string is non-null, 
        /// it is the requested string (with the newline removed).  If the Exception is non-null, 
        /// it is the Exception that caused the send attempt to fail.  If both are null, this
        /// indicates that the sending end of the remote socket has been shut down.
        ///  
        /// Alternatively, we can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload, length)
        ///     
        /// If length is negative or zero, this behaves identically to the first case.  If length
        /// is length, then instead of sending the next complete line in the callback, it sends
        /// the next length characters.  In other respects, it behaves analogously to the first case.
        /// 
        /// This method is non-blocking.  This means that it does not wait until a line of text
        /// has been received before returning.  Instead, it arranges for a line to be received
        /// and then returns.  When the line is actually received (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginReceive
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginReceive must take care of synchronization instead.  On a given StringSocket, each
        /// arriving line of text must be passed to callbacks in the order in which the corresponding
        /// BeginReceive call arrived.
        /// 
        /// Note that it is possible for there to be incoming bytes arriving at the underlying Socket
        /// even when there are no pending callbacks.  StringSocket implementations should refrain
        /// from buffering an unbounded number of incoming bytes beyond what is required to service
        /// the pending callbacks.
        /// </summary>
        public void BeginReceive(ReceiveCallback callback, object payload, int length = 0)
        {
            lock (syncReceive)
            {
                //receiveStateQueue.Enqueue(new ReceiveState(callback, payload));
                socket.BeginReceive(incomingBytes, 0, incomingBytes.Length, SocketFlags.None, DataReceived, new ReceiveState(callback, payload));
            }
        }

        /// <summary>
        /// Called when some bytes have been recieved on the socket.
        /// </summary>
        private void DataReceived(IAsyncResult result)
        {
            lock (syncReceive)
            {
                // Read the data
                int bytesRead = 0;
                try // need a try-catch here because sometimes the socket is disposed in the tests when we get here
                {
                    bytesRead = socket.EndReceive(result);
                }
                catch (ObjectDisposedException) { }

                if (bytesRead > 0)
                {
                    // Decode the bytes and add them to incoming
                    int charsRead = decoder.GetChars(incomingBytes, 0, bytesRead, incomingChars, 0, true);
                    Array.Clear(incomingBytes, 0, incomingBytes.Length);
                    incoming.Append(incomingChars, 0, charsRead);
                    //System.Diagnostics.Debug.Write(tempCount++ + ". Incoming Chars: " + new string(incomingChars));

                    for (int i = 0; i < incoming.Length; i++)
                    {
                        if (incoming[i] == '\n')
                        {
                            var line = incoming.ToString(0, i);
                            incoming.Remove(0, i + 1);

                            ReceiveState state = (ReceiveState)result.AsyncState;
                            Task.Run(() => state.Callback(line, null, state.Payload)); // fire off callback on another thread
                            //System.Diagnostics.Debug.WriteLine("Line: " + line + " Payload: " + state.Payload);                            
                        }
                    }

                    // Get more data
                    socket.BeginReceive(incomingBytes, 0, incomingBytes.Length, SocketFlags.None, DataReceived, result.AsyncState);
                }
                else
                {
                    socket.Close();
                }
            }
        }

        /// <summary>
        /// Provides an object to hold the state of a receive operation.  Specifically, it holds the callback to be used
        /// and the payload of the Receive.
        /// </summary>
        private class ReceiveState
        {
            /// <summary>
            /// The callback to be used.
            /// </summary>
            public ReceiveCallback Callback { get; set; }

            /// <summary>
            /// The payload to be passed through.
            /// </summary>
            public object Payload { get; set; }

            /// <summary>
            /// Creates a new ReceiveState with the given callback and payload.
            /// </summary>
            public ReceiveState(ReceiveCallback cb, object py)
            {
                Callback = cb;
                Payload = py;
            }
        }

        /// <summary>
        /// Provides an object to hold the state of a send operation.  Specifically, it holds the callback to be used
        /// and the payload of the send.
        /// </summary>
        private class SendState
        {
            /// <summary>
            /// The callback to be used.
            /// </summary>
            public SendCallback Callback { get; set; }

            /// <summary>
            /// The payload to be passed through.
            /// </summary>
            public object Payload { get; set; }

            /// <summary>
            /// Creates a new SendState with the given callback and payload.
            /// </summary>
            public SendState(SendCallback cb, object py)
            {
                Callback = cb;
                Payload = py;
            }
        }

    }
}