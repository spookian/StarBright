// reimplements exhal
public static class HALDecompressor
{
    enum Command
    {
        UNCOMPRESSED, // uncompressed
        RLE_8, // run length encoding 8 bit
        RLE_16, // rle 16 bit
        INC_8, // 8 bit increasing sequence
        BACKREF_NORMAL, // backreference, big endian offset
        BACKREF_ROTATION,
        BACKREF_REVERSE,
        BACKREF_AGAIN
    }
    const int UNPACK_FAILED = -1;

    private static byte rotate(byte i) // i considered using a different method but decided to pilfer this function from the source of exhal
    {                                   // because it is faster
        byte j = 0;
        if ((i & 0x01) == 0x01) j |= 0x80;
	    if ((i & 0x02) == 0x02) j |= 0x40;
	    if ((i & 0x04) == 0x04) j |= 0x20;
	    if ((i & 0x08) == 0x08) j |= 0x10;
	    if ((i & 0x10) == 0x10) j |= 0x08;
	    if ((i & 0x20) == 0x20) j |= 0x04;
	    if ((i & 0x40) == 0x40) j |= 0x02;
	    if ((i & 0x80) == 0x80) j |= 0x01;
        return j; 
    }

    private static int decompressBytes(byte[] compressedData, int offset, int length, Command command, List<byte> unpackedData) // no easy way to array split BRUH
    {
        int nOffset;
        int returnLength = 0;

        switch (command)
        {
            case Command.UNCOMPRESSED:
                unpackedData.AddRange(compressedData);
                returnLength = length;
                break;

            case Command.RLE_8:
                if (length < 1) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( compressedData[offset] );
                }
                returnLength = 1;
                break;

            case Command.RLE_16:
                if (length < 2) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( compressedData[offset] );
                    unpackedData.Add( compressedData[offset + 1] );
                }
                returnLength = 2;
                break;

            case Command.INC_8:
                if (length < 1) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( (byte)(compressedData[offset] + i) );
                }
                returnLength = 1;
                break;

            case Command.BACKREF_NORMAL:
            case Command.BACKREF_AGAIN:
                if (length < 2) return UNPACK_FAILED;
                nOffset = (compressedData[offset] << 8) | compressedData[offset + 1];

                if (nOffset + length > ushort.MaxValue) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( unpackedData[nOffset + i] );
                }

                returnLength = 2;
                break;

            case Command.BACKREF_ROTATION:
                if (length < 2) return UNPACK_FAILED;
                
                nOffset = (compressedData[offset] << 8) | compressedData[offset + 1];
                if (nOffset + length > ushort.MaxValue) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( rotate(unpackedData[nOffset + i]) );
                }

                returnLength = 2;
                break;

            case Command.BACKREF_REVERSE:
                if (length < 2) return UNPACK_FAILED;
                
                nOffset = (compressedData[offset] << 8) | compressedData[offset + 1];
                if (nOffset < length - 1) return UNPACK_FAILED;
                for (int i = 0; i < length; i++)
                {
                    unpackedData.Add( unpackedData[nOffset - i] );
                }

                returnLength = 2;
                break;

        }

        return returnLength;
    }

    public static List<byte> Decompress(byte[] compressedData)
    {
        List<byte> unpackedData = new List<byte>();
        for (int i = 0; i < ushort.MaxValue; i++)
        {
            byte cur = compressedData[i];
            if (cur == 0xFF) break;

            // check for long command, and get command + size
            Command command;
            int length;

            if ((cur & 0xE0) == 0xE0)
            {
                command = (Command)((cur >> 2) & 0x07);
                length = (((cur * 0x03) << 8) | compressedData[i]) + 1;
            }
            else
            {
                command = (Command)(cur >> 5);
                length = (cur & 0x1F) + 1;
            }
            i++;

            // failsafe seen in exhal, but i'm not sure if i need it
            int outpos = unpackedData.Count;
            if ( ((command == Command.RLE_16) && (outpos + 2 * length > ushort.MaxValue)) || (outpos + length > ushort.MaxValue)) return unpackedData;

            byte[] subArray = new byte[length];
            
            int l = decompressBytes(compressedData, i, length, command, unpackedData);
            if (l == UNPACK_FAILED) return unpackedData;
            i += l;
        }

        return unpackedData;
    }
}
