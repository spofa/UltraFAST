/*****************************************************************
 *
 * BSD LICENCE (http://www.opensource.org/licenses/bsd-license.php)
 *
 * Copyright (c) 2010, Cory Thomas
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   * Redistributions of source code must retain the above copyright notice,
 *     this list of conditions and the following disclaimer.
 *   * Redistributions in binary form must reproduce the above copyright notice,
 *     this list of conditions and the following disclaimer in the documentation
 *     and/or other materials provided with the distribution.
 *   * Neither the name of the <ORGANIZATION> nor the names of its contributors
 *     may be used to endorse or promote products derived from this software
 *     without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 ****************************************************************/

#pragma once

#include "Message.h"
#include "TraceInfo.h"
#include "SocketOptionName.h"
#include "SocketEvents.h"
#include "SocketState.h"
#include "SocketException.h"
#include "StdFileStream.h"
#include <udt.h>

namespace Udt
{
	interface class ICongestionControlFactory;

	/// <summary>
	/// Interface to a UDT socket.
	/// </summary>
	public ref class Socket
	{
	private:
		UDTSOCKET _socket;
		bool _isDisposed;
		System::Net::Sockets::AddressFamily _addressFamily;
		System::Net::Sockets::SocketType _socketType;
		ICongestionControlFactory^ _congestionControl;
		bool _blockingSend;

		void AssertNotDisposed(void)
		{
			if (_isDisposed)
				throw gcnew System::ObjectDisposedException(this->ToString());
		}

		Socket(UDTSOCKET socket, System::Net::Sockets::AddressFamily family, System::Net::Sockets::SocketType type, ICongestionControlFactory^ congestionControl);

		static Socket(void)
		{
			if (UDT::ERROR == UDT::startup())
				throw SocketException::GetLastError("Error in UDT startup");
			
			System::AppDomain::CurrentDomain->DomainUnload += gcnew System::EventHandler(DomainUnloaded);
		}
		
		static void DomainUnloaded(System::Object^ source, System::EventArgs^ args)
		{
			if (UDT::ERROR == UDT::cleanup())
				throw SocketException::GetLastError("Error in UDT cleanup");
		}

		int GetSocketOptionInt32(SocketOptionName name);
		__int64 GetSocketOptionInt64(SocketOptionName name);
		bool GetSocketOptionBoolean(SocketOptionName name);

		void SetSocketOptionInt32(SocketOptionName name, int value);
		void SetSocketOptionInt64(SocketOptionName name, __int64 value);
		void SetSocketOptionBoolean(SocketOptionName name, bool value);

		static UDT::UDSET* CreateUDSet(System::String^ paramName, System::Collections::Generic::ICollection<Udt::Socket^>^ fds);
		static void FillSocketList(const std::vector<UDTSOCKET>* list, System::Collections::Generic::Dictionary<UDTSOCKET, Udt::Socket^>^ sockets, System::Collections::Generic::ICollection<Udt::Socket^>^ fds);
		static void Filter(UDT::UDSET* set, System::Collections::Generic::ICollection<Udt::Socket^>^ fds);

	internal:

		property UDTSOCKET Handle
		{
			UDTSOCKET get(void) { return _socket; }
		}

	public:

		/// <summary>
		/// Timeout value that indicates infinite.
		/// </summary>
		static initonly System::TimeSpan InfiniteTimeout = System::TimeSpan(-1L);

		/// <summary>
		/// Initialize a new instance using the specified address family and
		/// socket type.
		/// </summary>
		/// <param name="family">Address family.</param>
		/// <param name="type">Socket type.</param>
		/// <exception cref="System::ArgumentException">
		/// <paramref name="family"/> is not either <c>InterNetwork</c> or <c>InterNetworkV6</c><br/>
		/// <b>- or -</b><br/>
		/// <paramref name="type"/> is not either <c>Dgram</c> or <c>Stream</c>
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs creating the socket.</exception>
		Socket(System::Net::Sockets::AddressFamily family, System::Net::Sockets::SocketType type);

		/// <summary>
		/// Closes the socket.
		/// </summary>
		~Socket(void);

		/// <summary>
		/// Close the socket and release any associated resources.
		/// </summary>
		void Close(void);

		/// <summary>
		/// Associate the socket with a local end point.
		/// </summary>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="address"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// If the type of <paramref name="address"/> is not compatible with the
		/// <b>AddressFamily</b> passed to the <b>Socket(AddressFamily,SocketType)</b>
		/// constructor.
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="port"/> is less than <see cref="System::Net::IPEndPoint::MinPort"/>
		/// or greater than <see cref="System::Net::IPEndPoint::MaxPort"/>.
		/// </exception>
		/// <exception cref="Udt::SocketException">
		/// If an error occurs binding the socket (i.e. the socket is already bound, etc).
		/// </exception>
		void Bind(System::Net::IPAddress^ address, int port);

		/// <summary>
		/// Associate the socket with a local end point.
		/// </summary>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="endPoint"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// If the type of address in <paramref name="endPoint"/> is not compatible with the
		/// <b>AddressFamily</b> passed to the <b>Socket(AddressFamily,SocketType)</b>
		/// constructor.
		/// </exception>
		/// <exception cref="Udt::SocketException">
		/// If an error occurs binding the socket (i.e. the socket is already bound, etc).
		/// </exception>
		[System::Diagnostics::CodeAnalysis::SuppressMessageAttribute(
			"Microsoft.Naming",
			"CA1702:CompoundWordsShouldBeCasedCorrectly",
			Justification = "EndPoint is the casing used in IPEndPoint")]
		void Bind(System::Net::IPEndPoint^ endPoint);

		/// <summary>
		/// Bind directly to an existing UDP socket.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is useful for firewall traversing in certain situations:
		/// <list type="number">
		/// <item><description>
		/// A UDP socket is created and its address is learned from a name server,
		/// there is no need to close the UDP socket and open a UDT socket on the
		/// same address again
		/// </description></item>
		/// <item><description>
		/// For certain firewalls, especially some on local system, the port mapping
		/// may be changed or the "hole" may be closed when a UDP socket is closed
		/// and reopened, thus it is necessary to use the UDP socket directly in UDT.
		/// </description></item>
		/// </list>
		/// </para>
		/// <para>
		/// Use this form of bind with caution, as it violates certain programming
		/// rules regarding code robustness. Once <paramref name="udpSocket"/> is
		/// passed to UDT, it MUST NOT be touched again. DO NOT use this unless
		/// you clearly understand how the related systems work.
		/// </para>
		/// </remarks>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="udpSocket"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// <see cref="System::Net::Sockets::Socket::ProtocolType"/> is not
		/// <c>Udp</c> for <paramref name="udpSocket"/>
		/// </exception>
		/// <exception cref="Udt::SocketException">
		/// If an error occurs binding the socket (i.e. the socket is already bound, etc).
		/// </exception>
		void Bind(System::Net::Sockets::Socket^ udpSocket);

		/// <summary>
		/// Places the socket in a listening state.
		/// </summary>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="backlog"/> is less than 1.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		void Listen(int backlog);

		/// <summary>
		/// Creates a <see cref="Socket"/> for a newly created connection.
		/// </summary>
		/// <remarks>
		/// <b>Accept</b> synchronously extracts the first pending connection
		/// request from the connection request queue of the listening socket,
		/// and then creates and returns a new <see cref="Socket"/>.
		/// </remarks>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		Socket^ Accept();

		/// <summary>
		/// Establishes a connection to a remote host.
		/// </summary>
		/// <param name="host">Name of the host to connect to.</param>
		/// <param name="port">Port to connect to.</param>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="host"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="port"/> is less than <see cref="System::Net::IPEndPoint::MinPort"/>
		/// or greater than <see cref="System::Net::IPEndPoint::MaxPort"/>.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		void Connect(System::String^ host, int port);

		/// <summary>
		/// Establishes a connection to a remote host.
		/// </summary>
		/// <param name="address">Address of the host to connect to.</param>
		/// <param name="port">Port to connect to.</param>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="address"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="port"/> is less than <see cref="System::Net::IPEndPoint::MinPort"/>
		/// or greater than <see cref="System::Net::IPEndPoint::MaxPort"/>.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		void Connect(System::Net::IPAddress^ address, int port);

		/// <summary>
		/// Establishes a connection to a remote host.
		/// </summary>
		/// <param name="addresses">Addresses of the host to connect to.</param>
		/// <param name="port">Port to connect to.</param>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="addresses"/> is a null reference
		/// </exception>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="addresses"/> is empty
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="port"/> is less than <see cref="System::Net::IPEndPoint::MinPort"/>
		/// or greater than <see cref="System::Net::IPEndPoint::MaxPort"/>.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		void Connect(cli::array<System::Net::IPAddress^>^ addresses, int port);

		/// <summary>
		/// Establishes a connection to a remote host.
		/// </summary>
		/// <param name="endPoint">Remote end point to connect to.</param>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="endPoint"/> is a null reference
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		[System::Diagnostics::CodeAnalysis::SuppressMessageAttribute(
			"Microsoft.Naming",
			"CA1702:CompoundWordsShouldBeCasedCorrectly",
			Justification = "EndPoint is the casing used in IPEndPoint")]
		void Connect(System::Net::IPEndPoint^ endPoint);

		/// <summary>
		/// Determines the status of one or more sockets.
		/// </summary>
		/// <remarks>
		/// Note that, currently, <paramref name="checkError"/> is ignored in UDT4.
		/// </remarks>
		/// <param name="checkRead">Socket instances to check for readability.</param>
		/// <param name="checkWrite">Socket instances to check for writeability.</param>
		/// <param name="checkError">Socket instances to check for errors.</param>
		/// <param name="timeout">Timeout value or <see cref="InfiniteTimeout"/>.</param>
		/// <exception cref="System::ArgumentException">
		/// <paramref name="checkRead"/> is a null reference or empty
		/// - and -
		/// <paramref name="checkWrite"/> is a null reference or empty
		/// - and -
		/// <paramref name="checkError"/> is a null reference or empty
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// If <paramref name="checkRead"/>, <paramref name="checkWrite"/>, or
		/// <paramref name="checkError"/> contains a null reference.
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="timeout"/> is not <see cref="InfiniteTimeout"/> and
		/// is less than 0.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		static void Select(
			System::Collections::Generic::ICollection<Socket^>^ checkRead,
			System::Collections::Generic::ICollection<Socket^>^ checkWrite,
			System::Collections::Generic::ICollection<Socket^>^ checkError,
			System::TimeSpan timeout);

		/// <summary>
		/// Determines the status of one or more sockets.
		/// </summary>
		/// <param name="checkSockets">Sockets to check the status of.</param>
		/// <param name="readSockets">Sockets that are ready for receive.</param>
		/// <param name="writeSockets">Socket that are ready to write.</param>
		/// <param name="errorSockets">Sockets that are closed or with a broken connection.</param>
		/// <param name="timeout">Timeout value or <see cref="InfiniteTimeout"/>.</param>
		/// <exception cref="System::ArgumentNullException">
		/// If <paramref name="checkSockets"/> is a null reference.
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// If <paramref name="checkSockets"/> contains a null reference.
		/// </exception>
		/// <exception cref="System::ArgumentException">
		/// <paramref name="readSockets"/>, <paramref name="writeSockets"/>, and <paramref name="errorSockets"/> are null references.
		/// </exception>
		/// <exception cref="System::ArgumentOutOfRangeException">
		/// If <paramref name="timeout"/> is not <see cref="InfiniteTimeout"/> and
		/// is less than 0.
		/// </exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		static void Select(
			System::Collections::Generic::ICollection<Socket^>^ checkSockets,
			System::Collections::Generic::ICollection<Socket^>^ readSockets,
			System::Collections::Generic::ICollection<Socket^>^ writeSockets,
			System::Collections::Generic::ICollection<Socket^>^ errorSockets,
			System::TimeSpan timeout);

		int Receive(cli::array<System::Byte>^ buffer);
		int Receive(cli::array<System::Byte>^ buffer, int offset, int size);

		/// <summary>
		/// Send the specified bytes.
		/// </summary>
		/// <remarks>
		/// If the socket is in blocking mode, the call will block until the
		/// entire buffer is sent. In non-blocking mode, the call may return
		/// a value less than the length of the buffer (even zero) if the socket
		/// send queue limit has been reached. See <see cref="BlockingSend"/>.
		/// </remarks>
		/// <param name="buffer">Bytes to send.</param>
		/// <returns>The total number of bytes sent.</returns>
		/// <exception cref="System::ArgumentNullException">If <paramref name="buffer"/> is null.</exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		int Send(cli::array<System::Byte>^ buffer);

		/// <summary>
		/// Send the specified bytes.
		/// </summary>
		/// <remarks>
		/// If the socket is in blocking mode, the call will block until the
		/// entire buffer is sent. In non-blocking mode, the call may return
		/// a value less than the length of the buffer (even zero) if the socket
		/// send queue limit has been reached. See <see cref="BlockingSend"/>.
		/// </remarks>
		/// <param name="buffer">Bytes to send.</param>
		/// <param name="offset">Offset into <paramref name="buffer"/> to start sending.</param>
		/// <param name="size">Number of bytes to send.</param>
		/// <returns>The total number of bytes sent.</returns>
		/// <exception cref="System::ArgumentNullException">If <paramref name="buffer"/> is null.</exception>
		/// <exception cref="System::ArgumentOutOfRangeException">If <paramref name="offset"/> or <paramref name="size"/> is less than zero.</exception>
		/// <exception cref="System::ArgumentException">If <paramref name="size"/> is greater than the length of the <paramref name="buffer"/> minus the <paramref name="offset"/>.</exception>
		/// <exception cref="Udt::SocketException">If an error occurs.</exception>
		int Send(cli::array<System::Byte>^ buffer, int offset, int size);

		/// <summary>
		/// Send the contents of a file on this socket.
		/// </summary>
		/// <remarks>
		/// Does not send a file size.
		/// </remarks>
		/// <param name="fileName">Name of the local file to send.</param>
		/// <returns>The total number of bytes sent.</returns>
		/// <exception cref="System::ArgumentNullException">If <paramref name="fileName"/> is null.</exception>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket or the file.</exception>
		__int64 SendFile(System::String^ fileName);

		/// <summary>
		/// Send the contents of a file on this socket.
		/// </summary>
		/// <remarks>
		/// Does not send a file size.
		/// </remarks>
		/// <param name="fileName">Name of the local file to send.</param>
		/// <param name="offset">Offset in the file to start sending.</param>
		/// <returns>The total number of bytes sent.</returns>
		/// <exception cref="System::ArgumentNullException">If <paramref name="fileName"/> is null.</exception>
		/// <exception cref="System::ArgumentOutOfRangeException">If <paramref name="offset"/> is less than 0.</exception>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket or the file.</exception>
		__int64 SendFile(System::String^ fileName, __int64 offset);

		/// <summary>
		/// Send the contents of a file on this socket.
		/// </summary>
		/// <remarks>
		/// Does not send a file size.
		/// </remarks>
		/// <param name="fileName">Name of the local file to send.</param>
		/// <param name="offset">Offset in the file to start sending.</param>
		/// <param name="count">Number of bytes to send or -1 to send until the end of the file is reached.</param>
		/// <returns>The total number of bytes sent.</returns>
		/// <exception cref="System::ArgumentNullException">If <paramref name="fileName"/> is null.</exception>
		/// <exception cref="System::ArgumentOutOfRangeException">If <paramref name="offset"/> is less than 0 or <paramref name="count"/> is less than -1.</exception>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket or the file.</exception>
		__int64 SendFile(System::String^ fileName, __int64 offset, __int64 count);

		__int64 SendFile(StdFileStream^ file);
		__int64 SendFile(StdFileStream^ file, __int64 count);

		/// <summary>
		/// Receive data on this socket and store it in a local file.
		/// </summary>
		/// <param name="fileName">Name of the local file to write the data to.</param>
		/// <param name="length">Number of bytes to read from the socket into <paramref name="fileName"/></param>
		/// <returns>The total number of bytes received.</returns>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket or the file.</exception>
		__int64 ReceiveFile(System::String^ fileName, __int64 length);

		__int64 ReceiveFile(StdFileStream^ file, __int64 length);

		int SendMessage(cli::array<System::Byte>^ buffer);
		int SendMessage(cli::array<System::Byte>^ buffer, int offset, int size);
		int SendMessage(Message^ message);

		int ReceiveMessage(cli::array<System::Byte>^ buffer);
		int ReceiveMessage(cli::array<System::Byte>^ buffer, int offset, int size);

		/// <summary>
		/// Retrieve internal protocol parameters and performance trace.
		/// </summary>
		/// <remarks>
		/// Same as <c>GetPerformanceInfo(true)</c>.
		/// </remarks>
		/// <returns>UDT socket performance trace information.</returns>
		TraceInfo^ GetPerformanceInfo();

		/// <summary>
		/// Retrieve internal protocol parameters and performance trace.
		/// </summary>
		/// <param name="clear">True to clear local trace information and counts.</param>
		/// <returns>UDT socket performance trace information.</returns>
		TraceInfo^ GetPerformanceInfo(bool clear);

		void SetSocketOption(SocketOptionName name, int value);
		void SetSocketOption(SocketOptionName name, __int64 value);
		void SetSocketOption(SocketOptionName name, bool value);
		void SetSocketOption(SocketOptionName name, System::Object^ value);

		System::Object^ GetSocketOption(SocketOptionName name);

		/// <summary>
		/// Gets the local end point.
		/// </summary>
		/// <value>
		/// The local end point that the socket is using for communications.
		/// </value>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket.</exception>
		[System::Diagnostics::CodeAnalysis::SuppressMessageAttribute(
			"Microsoft.Naming",
			"CA1702:CompoundWordsShouldBeCasedCorrectly",
			Justification = "EndPoint is the casing used in IPEndPoint")]
		property System::Net::IPEndPoint^ LocalEndPoint
		{
			System::Net::IPEndPoint^ get(void);
		}

		/// <summary>
		/// Gets the address family of the socket.
		/// </summary>
		property System::Net::Sockets::AddressFamily AddressFamily
		{
			System::Net::Sockets::AddressFamily get(void);
		}

		/// <summary>
		/// Gets the type of the socket.
		/// </summary>
		property System::Net::Sockets::SocketType SocketType
		{
			System::Net::Sockets::SocketType get(void);
		}

		/// <summary>
		/// Gets the remote end point.
		/// </summary>
		/// <value>
		/// The remote end point that the socket is using for communications.
		/// </value>
		/// <exception cref="Udt::SocketException">If an error occurs accessing the socket.</exception>
		[System::Diagnostics::CodeAnalysis::SuppressMessageAttribute(
			"Microsoft.Naming",
			"CA1702:CompoundWordsShouldBeCasedCorrectly",
			Justification = "EndPoint is the casing used in IPEndPoint")]
		property System::Net::IPEndPoint^ RemoteEndPoint
		{
			System::Net::IPEndPoint^ get(void);
		}

		property int SendBufferSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::SendBuffer); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::SendBuffer, value); }
		}

		property int UdpSendBufferSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::UdpSendBuffer); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::UdpSendBuffer, value); }
		}

		property int SendTimeout
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::SendTimeout); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::SendTimeout, value); }
		}

		property bool BlockingSend
		{
			bool get(void) { return _blockingSend; }
			void set(bool value) { SetSocketOptionBoolean(Udt::SocketOptionName::BlockingSend, value); }
		}

		property int ReceiveBufferSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::ReceiveBuffer); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::ReceiveBuffer, value); }
		}

		property int UdpReceiveBufferSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::UdpReceiveBuffer); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::UdpReceiveBuffer, value); }
		}

		property int ReceiveTimeout
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::ReceiveTimeout); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::ReceiveTimeout, value); }
		}

		property bool BlockingReceive
		{
			bool get(void) { return GetSocketOptionBoolean(Udt::SocketOptionName::BlockingReceive); }
			void set(bool value) { SetSocketOptionBoolean(Udt::SocketOptionName::BlockingReceive, value); }
		}

		property bool Rendezvous
		{
			bool get(void) { return GetSocketOptionBoolean(Udt::SocketOptionName::Rendezvous); }
			void set(bool value) { SetSocketOptionBoolean(Udt::SocketOptionName::Rendezvous, value); }
		}

		property bool ReuseAddress
		{
			bool get(void) { return GetSocketOptionBoolean(Udt::SocketOptionName::ReuseAddress); }
			void set(bool value) { SetSocketOptionBoolean(Udt::SocketOptionName::ReuseAddress, value); }
		}

		property __int64 MaxBandwidth
		{
			__int64 get(void) { return GetSocketOptionInt64(Udt::SocketOptionName::MaxBandwidth); }
			void set(__int64 value) { SetSocketOptionInt64(Udt::SocketOptionName::MaxBandwidth, value); }
		}

		property System::Net::Sockets::LingerOption^ LingerState
		{
			System::Net::Sockets::LingerOption^ get(void)
			{
				return (System::Net::Sockets::LingerOption^)GetSocketOption(Udt::SocketOptionName::Linger);
			}

			void set(System::Net::Sockets::LingerOption^ value)
			{
				SetSocketOption(Udt::SocketOptionName::Linger, value);
			}
		}

		property int MaxPacketSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::MaxPacketSize); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::MaxPacketSize, value); }
		}

		property int MaxWindowSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::MaxWindowSize); }
			void set(int value) { SetSocketOptionInt32(Udt::SocketOptionName::MaxWindowSize, value); }
		}

		property int SendDataSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::SendData); }
		}

		property int ReceiveDataSize
		{
			int get(void) { return GetSocketOptionInt32(Udt::SocketOptionName::ReceiveData); }
		}

		property Udt::SocketEvents Events
		{
			Udt::SocketEvents get(void) { return (Udt::SocketEvents)GetSocketOptionInt32(Udt::SocketOptionName::Events); }
		}

		property Udt::SocketState State
		{
			Udt::SocketState get(void)
			{
				if (_isDisposed)
				{
					return Udt::SocketState::Closed;
				}

				Udt::SocketState state = (Udt::SocketState)GetSocketOptionInt32(Udt::SocketOptionName::State);

				if ((int)state == NONEXIST)
				{
					state = Udt::SocketState::Closed;
				}

				return state;
			}
		}

		/// <summary>
		/// Get or set the custom congestion control algorithm for this socket
		/// or null to use the default.
		/// </summary>
		/// <remarks>
		/// The custom congestion control algorithm will be passed to any
		/// sockets accepted by this socket.
		/// </remarks>
		property ICongestionControlFactory^ CongestionControl
		{
			ICongestionControlFactory^ get(void)
			{
				return (ICongestionControlFactory^)GetSocketOption(Udt::SocketOptionName::CongestionControl);
			}

			void set(ICongestionControlFactory^ value)
			{
				SetSocketOption(Udt::SocketOptionName::CongestionControl, value);
			}
		}

		/// <summary>
		/// Get true or false if this socket has been closed.
		/// </summary>
		property bool IsDisposed
		{
			bool get(void) { return _isDisposed; }
		}
	};
}
