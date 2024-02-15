#include "../framework.h"
#include "CHIDPacket.h"

CHIDPacket::CHIDPacket(UCHAR* buff, DWORD size)
{
	data = new UCHAR[size - sizeof(UINT32)];
	joyId = *(UINT32*)buff;
	RtlCopyMemory(data, buff + sizeof(UINT32), size - sizeof(UINT32));
	this->size = size - sizeof(UINT32);
}

CHIDPacket::~CHIDPacket()
{
	delete[] data; data = nullptr;
}
