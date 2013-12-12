using System;
using System.Runtime.InteropServices;

// 12 bytes
[StructLayout(LayoutKind.Sequential)]
[Serializable]
public struct Voxel
{
	public byte id; // 1 byte
	public byte attr1; // 1 byte
	public byte attr2; // 1 byte
	public byte attr3; // 1 byte
	public short attr4; // 2 bytes
	public short attr5; // 2 bytes
	public int attr6; // 4 bytes
	
}
