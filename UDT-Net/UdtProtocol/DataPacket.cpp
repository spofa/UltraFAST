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

#include "StdAfx.h"
#include "DataPacket.h"

#include <udt.h>
#include <packet.h>

using namespace Udt;
using namespace System;

DataPacket::DataPacket(const CPacket* packet)
	: Packet(packet), _capacity(packet->getLength())
{
}

DataPacket::DataPacket(void)
	: _capacity(0)
{
}

DataPacket::~DataPacket(void)
{
}

void DataPacket::FreePacketData()
{
	delete [] _packet->m_pcData;
}

Udt::MessageBoundary DataPacket::MessageBoundary::get(void)
{
	AssertNotDisposed();
	return (Udt::MessageBoundary)_packet->getMsgBoundary();
}

void DataPacket::MessageBoundary::set(Udt::MessageBoundary value)
{
	AssertIsMutable();

	int iValue = (int)value;
	if (iValue < 0 || iValue > 3) throw gcnew ArgumentOutOfRangeException("value", value, "Invalid message boundary flag.");

	_packet->m_iMsgNo = (_packet->m_iMsgNo & 0x3FFFFFFF) | (iValue << 30);
}

bool DataPacket::InOrder::get(void)
{
	AssertNotDisposed();
	return _packet->getMsgOrderFlag();
}

void DataPacket::InOrder::set(bool value)
{
	AssertIsMutable();

	if (value)
		_packet->m_iMsgNo |= 0x20000000;
	else
		_packet->m_iMsgNo &= 0xDFFFFFFF;
}

int DataPacket::MessageNumber::get(void)
{
	AssertNotDisposed();
	return _packet->getMsgSeq();
}

void DataPacket::MessageNumber::set(int value)
{
	AssertIsMutable();
	if (value < 0 || value > MaxMessageNumber) throw gcnew ArgumentOutOfRangeException("value", value, String::Concat("Value must be between 0 and ", MaxMessageNumber, "."));

	_packet->m_iMsgNo = (_packet->m_iMsgNo & ~MaxMessageNumber) | value;
}

int DataPacket::PacketNumber::get(void)
{
	AssertNotDisposed();
	return _packet->m_iSeqNo;
}

void DataPacket::PacketNumber::set(int value)
{
	AssertIsMutable();
	if (value < 0) throw gcnew ArgumentOutOfRangeException("value", value, "Value must be greater than or equal to 0.");
	_packet->m_iSeqNo = value;
}

int DataPacket::DataLength::get(void)
{
	AssertNotDisposed();
	return _packet->getLength();
}

void DataPacket::DataLength::set(int value)
{
	AssertIsMutable();
	if (value < 0) throw gcnew ArgumentOutOfRangeException("value", value, "Value must be greater than or equal to 0.");

	EnsureCapacity(value);
	_packet->setLength(value);
}

int DataPacket::DataCapacity::get(void)
{
	AssertNotDisposed();
	return _capacity;
}

void DataPacket::EnsureCapacity(int value)
{
	if (value > _capacity)
	{
		int newCapacity = value;

		if (newCapacity < 256)
			newCapacity = 256;
		else if (newCapacity < _capacity * 2)
			newCapacity = _capacity * 2;

		char* newBuf = new char[newCapacity];
		memcpy(newBuf, _packet->m_pcData, _packet->getLength());
		delete [] _packet->m_pcData;
		_packet->m_pcData = newBuf;
		_capacity = newCapacity;
	}
}

int DataPacket::Read(int dataOffset, cli::array<unsigned char>^ buffer, int bufferOffset, int bufferCount)
{
	AssertNotDisposed();

	int packetLen = _packet->getLength();

	if (dataOffset < 0 || dataOffset > packetLen) throw gcnew ArgumentOutOfRangeException("dataOffset", dataOffset, String::Format("Value must be greater than or equal to 0 and less than the packet length ({0})", packetLen));
	if (buffer == nullptr) throw gcnew ArgumentNullException("buffer");
	if (bufferOffset < 0) throw gcnew ArgumentOutOfRangeException("bufferOffset", bufferOffset, "Value must be greater than or equal to 0.");
	if (bufferCount < 0) throw gcnew ArgumentOutOfRangeException("bufferCount", bufferCount, "Value must be greater than or equal to 0.");
	if (bufferOffset + bufferCount > buffer->Length) throw gcnew ArgumentException("Invalid buffer offset and count.");

	int readCount = min(packetLen - dataOffset, bufferCount);

	if (readCount > 0)
	{
		pin_ptr<unsigned char> bufferPin = &buffer[0];
		memcpy(bufferPin + bufferOffset, _packet->m_pcData + dataOffset, readCount);
	}

	return readCount;
}

void DataPacket::Write(int dataOffset, cli::array<unsigned char>^ buffer, int bufferOffset, int bufferCount)
{
	AssertNotDisposed();

	if (dataOffset < 0) throw gcnew ArgumentOutOfRangeException("dataOffset", dataOffset, "Value must be greater than or equal to 0");
	if (buffer == nullptr) throw gcnew ArgumentNullException("buffer");
	if (bufferOffset < 0) throw gcnew ArgumentOutOfRangeException("bufferOffset", bufferOffset, "Value must be greater than or equal to 0.");
	if (bufferCount < 0) throw gcnew ArgumentOutOfRangeException("bufferCount", bufferCount, "Value must be greater than or equal to 0.");
	if (bufferOffset + bufferCount > buffer->Length) throw gcnew ArgumentException("Invalid buffer offset and count.");

	int requiredLength = dataOffset + bufferCount;

	if (requiredLength > 0)
	{
		EnsureCapacity(requiredLength);

		if (requiredLength > _packet->getLength())
			_packet->setLength(requiredLength);

		pin_ptr<unsigned char> bufferPin = &buffer[0];
		memcpy(_packet->m_pcData + dataOffset, bufferPin + bufferOffset, bufferCount);
	}
}
