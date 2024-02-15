#pragma once

class CHIDPacket
{
public:
	CHIDPacket(UCHAR* buff, DWORD size);
	~CHIDPacket();

	UCHAR* GetData() { return data; }
	UINT32 GetJoyId() const { return joyId;	}
	DWORD GetSize() const { return size; }
private:
	UCHAR* data = nullptr;
	UINT32 joyId = 0;
	DWORD size = 0;

};
