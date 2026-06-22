// プリミティブなデータとバイナリデータを相互変換するシリアライザー
//
// ■プログラムの説明
// プリミティブのデータ(システムで定義されているデータ)を送信するための1次元のバイナリデータに変換します.
// 受信した1次元のバイナリデータをプリミティブのデータに変換します.
// Serializer() 関数(コンストラクタ)で使用中の端末のエンディアンを判別しています.
// Serialize() 関数でプリミティブな型の変数をバイナリデータに変換しています.
// 各型をバイナリに変換した後に、1次元のデータにするために WriteBuffer() 関数で配列の最後尾にデータを追加しています.
// バイナリデータからプリミティブな型に変換するには SetDeserializedData() 関数に変換するバイナリデータを設定し、
// Deserialize() 関数でプリミティブな型に変換しています.
//

#if false

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Serializer
{
    // バイナリデータ保存用ストリーム P.157
    //・・・

    // 追加位置のオフセット P.157
    //・・・

    // 実行している端末のエンディアンの種類. P.161
    //・・・

    // エンディアン. P.161
    //・・・



    //
    // コンストラクタ.
    //
    public Serializer()
    {
        // シリアライズ用バッファを作成します.
        m_buffer = new MemoryStream();

        // エンディアンを判定します.
        int val = 1;
        byte[] conv = BitConverter.GetBytes(val);
        m_endianness = (conv[0] == 1) ? Endianness.LittleEndian : Endianness.BigEndian;
    }

    //
    // シリアライズされたバイナリデータを取得.
    //
    public byte[] GetSerializedData()
    {
        return m_buffer.ToArray();
    }

    //
    // データをクリア.
    //
    public void Clear()
    {
        // 内部バッファをクリア
        byte[] buffer = m_buffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        // ストリーム位置と長さをリセット
        m_buffer.Position = 0;
        m_buffer.SetLength(0);

        // オフセットもリセット
        m_offset = 0;
    }

    //
    // デシリアライズするデータをバッファに設定.
    //
    public bool SetDeserializedData(byte[] data)
    {
        // 設定するバッファをクリアします.
        Clear();

        try
        {
            // デシリアライズするデータを設定します.
            m_buffer.Write(data, 0, data.Length);
        }
        catch
        {
            return false;
        }

        return true;
    }

    //
    // bool型のデータをシリアライズ
    //
    protected bool Serialize(bool element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(bool));
    }

    //
    // char型のデータをシリアライズ.
    //
    protected bool Serialize(char element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(char));
    }

    //
    // float型のデータをシリアライズ.
    //
    protected bool Serialize(float element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(float));
    }

    //
    // double型のデータをシリアライズ.
    //
    protected bool Serialize(double element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(double));
    }

    //
    // short型のデータをシリアライズ.
    //
    protected bool Serialize(short element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(short));
    }

    //
    // ushort型のデータをシリアライズ.
    //
    protected bool Serialize(ushort element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(ushort));
    }

    //
    // int型のデータをシリアライズ. P.157
    //
    //・・・

    //
    // uint型のデータをシリアライズ.
    //
    protected bool Serialize(uint element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(uint));
    }

    //
    // long型のデータをシリアライズ.
    //
    protected bool Serialize(long element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(long));
    }

    //
    // ulong型のデータをシリアライズ.
    //
    protected bool Serialize(ulong element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(ulong));
    }

    //
    // byte[]型のデータをシリアライズ.
    //
    protected bool Serialize(byte[] element, int length)
    {
        // byte列はデータの塊として設定するのでエンディアン変換しない
        // ためバッファ保存先でもとに戻るようにします.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(element);
        }

        return WriteBuffer(element, length);
    }

    //
    // string型のデータをシリアライズ.
    //
    protected bool Serialize(string element, int length)
    {
        byte[] data = new byte[length];

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(element);
        int size = Math.Min(buffer.Length, data.Length);
        Buffer.BlockCopy(buffer, 0, data, 0, size);

        // byte列はデータの塊として設定するのでエンディアン変換しない
        // ためバッファ保存先でもとに戻るようにします.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(data);
        }

        return WriteBuffer(data, data.Length);
    }

    //
    // データをbool型へデシリアライズ.
    //
    protected bool Deserialize(ref bool element)
    {
        int size = sizeof(bool);
        byte[] data = new byte[size];

        // bool型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をbool型へ変換します.
            element = BitConverter.ToBoolean(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをchar型へデシリアライズ.
    //
    protected bool Deserialize(ref char element)
    {
        int size = sizeof(char);
        byte[] data = new byte[size];

        // char型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をchar型へ変換します.
            element = BitConverter.ToChar(data, 0);
            return true;
        }

        return false;
    }


    //
    // データをfloat型へデシリアライズ.
    //
    protected bool Deserialize(ref float element)
    {
        int size = sizeof(float);
        byte[] data = new byte[size];

        // float型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をfloat型へ変換します.
            element = BitConverter.ToSingle(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをdouble型へデシリアライズ.
    //
    protected bool Deserialize(ref double element)
    {
        int size = sizeof(double);
        byte[] data = new byte[size];

        // double型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をdouble型へ変換します.
            element = BitConverter.ToDouble(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをshort型へデシリアライズ.
    //
    protected bool Deserialize(ref short element)
    {
        int size = sizeof(short);
        byte[] data = new byte[size];

        // short型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をshort型へ変換します.
            element = BitConverter.ToInt16(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをushort型へデシリアライズ.
    //
    protected bool Deserialize(ref ushort element)
    {
        int size = sizeof(ushort);
        byte[] data = new byte[size];

        // ushort型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をushort型へ変換します.
            element = BitConverter.ToUInt16(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをint型へデシリアライズ.
    //
    protected bool Deserialize(ref int element)
    {
        int size = sizeof(int);
        byte[] data = new byte[size];

        // int型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をint型へ変換します.
            element = BitConverter.ToInt32(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをuint型へデシリアライズ.
    //
    protected bool Deserialize(ref uint element)
    {
        int size = sizeof(uint);
        byte[] data = new byte[size];

        // uint型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をuint型へ変換します.
            element = BitConverter.ToUInt32(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをlong型へデシリアライズ.
    //
    protected bool Deserialize(ref long element)
    {
        int size = sizeof(long);
        byte[] data = new byte[size];

        // long型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をlong型へ変換します.
            element = BitConverter.ToInt64(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをulong型へデシリアライズ.
    //
    protected bool Deserialize(ref ulong element)
    {
        int size = sizeof(ulong);
        byte[] data = new byte[size];

        // ulong型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をulong型へ変換します.
            element = BitConverter.ToUInt64(data, 0);
            return true;
        }

        return false;
    }

    //
    // byte[]型のデータへデシリアライズ.
    //
    protected bool Deserialize(ref byte[] element, int length)
    {
        // 配列のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref element, length);

        // byte列はデータの塊として保存されてのでエンディアン変換しない
        // ためバッファここででもとに戻します.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(element);
        }

        if (ret == true)
        {
            return true;
        }

        return false;
    }

    //
    // string型のデータへデシリアライズ.
    //
    protected bool Deserialize(ref string element, int length)
    {
        byte[] data = new byte[length];

        // 文字列のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // byte列はデータの塊として保存されてのでエンディアン変換しない
            // ためバッファここででもとに戻します.
            if (m_endianness == Endianness.LittleEndian)
            {
                Array.Reverse(data);
            }
            string str = System.Text.Encoding.UTF8.GetString(data);
            element = str.Trim('\0');

            return true;
        }

        return false;
    }

    //
    // 指定サイズをデータの先頭からホストバイトオーダーに変換して読み出す.
    //
    protected bool ReadBuffer(ref byte[] data, int size)
    {
        // 現在のオフセットからデータを読み出します.
        try
        {
            m_buffer.Position = m_offset;
            m_buffer.Read(data, 0, size);
            m_offset += size;
        }
        catch
        {
            return false;
        }

        // 読みだした値をホストバイトオーダーに変換します.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(data);
        }

        return true;
    }

    //
    // バイトオーダー変更後のデータをストリームに追加. P.157
    //
    //・・・

    //
    // エンディアンを取得.
    //
    public Endianness GetEndianness()
    {
        return m_endianness;
    }

    //
    // データのサイズを取得.
    //
    public long GetDataSize()
    {
        return m_buffer.Length;
    }
}



#else

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Serializer
{
    // バイナリデータ保存用ストリーム.
    private MemoryStream m_buffer = null;

    // 追加位置のオフセット
    private int m_offset = 0;

    // 実行している端末のエンディアンの種類.
    private Endianness m_endianness;

    // エンディアン.
    public enum Endianness
    {
        BigEndian = 0,      // ビッグエンディアン.
        LittleEndian,       // リトルエンディアン.
    }

    //
    // コンストラクタ.
    //
    public Serializer()
    {
        // シリアライズ用バッファを作成します.
        m_buffer = new MemoryStream();

        // エンディアンを判定します.
        int val = 1;
        byte[] conv = BitConverter.GetBytes(val);
        m_endianness = (conv[0] == 1) ? Endianness.LittleEndian : Endianness.BigEndian;
    }

    //
    // シリアライズされたバイナリデータを取得.
    //
    public byte[] GetSerializedData()
    {
        return m_buffer.ToArray();
    }

    //
    // データをクリア.
    //
    public void Clear()
    {
        // 内部バッファをクリア
        byte[] buffer = m_buffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        // ストリーム位置と長さをリセット
        m_buffer.Position = 0;
        m_buffer.SetLength(0);

        // オフセットもリセット
        m_offset = 0;
    }

    //
    // デシリアライズするデータをバッファに設定.
    //
    public bool SetDeserializedData(byte[] data)
    {
        // 設定するバッファをクリアします.
        Clear();

        try
        {
            // デシリアライズするデータを設定します.
            m_buffer.Write(data, 0, data.Length);
        }
        catch
        {
            return false;
        }

        return true;
    }

    //
    // bool型のデータをシリアライズ.
    //
    protected bool Serialize(bool element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(bool));
    }

    //
    // char型のデータをシリアライズ.
    //
    protected bool Serialize(char element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(char));
    }

    //
    // float型のデータをシリアライズ.
    //
    protected bool Serialize(float element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(float));
    }

    //
    // double型のデータをシリアライズ.
    //
    protected bool Serialize(double element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(double));
    }

    //
    // short型のデータをシリアライズ.
    //
    protected bool Serialize(short element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(short));
    }

    //
    // ushort型のデータをシリアライズ.
    //
    protected bool Serialize(ushort element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(ushort));
    }

    //
    // int型のデータをシリアライズ.
    //
    protected bool Serialize(int element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(int));
    }

    //
    // uint型のデータをシリアライズ.
    //
    protected bool Serialize(uint element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(uint));
    }

    //
    // long型のデータをシリアライズ.
    //
    protected bool Serialize(long element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(long));
    }

    //
    // ulong型のデータをシリアライズ.
    //
    protected bool Serialize(ulong element)
    {
        byte[] data = BitConverter.GetBytes(element);

        return WriteBuffer(data, sizeof(ulong));
    }

    //
    // byte[]型のデータをシリアライズ.
    //
    protected bool Serialize(byte[] element, int length)
    {
        // byte列はデータの塊として設定するのでエンディアン変換しない
        // ためバッファ保存先でもとに戻るようにします.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(element);
        }

        return WriteBuffer(element, length);
    }

    //
    // string型のデータをシリアライズ.
    //
    protected bool Serialize(string element, int length)
    {
        byte[] data = new byte[length];

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(element);
        int size = Math.Min(buffer.Length, data.Length);
        Buffer.BlockCopy(buffer, 0, data, 0, size);

        // byte列はデータの塊として設定するのでエンディアン変換しない
        // ためバッファ保存先でもとに戻るようにします.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(data);
        }

        return WriteBuffer(data, data.Length);
    }

    //
    // データをbool型へデシリアライズ.
    //
    protected bool Deserialize(ref bool element)
    {
        int size = sizeof(bool);
        byte[] data = new byte[size];

        // bool型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をbool型へ変換します.
            element = BitConverter.ToBoolean(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをchar型へデシリアライズ.
    //
    protected bool Deserialize(ref char element)
    {
        int size = sizeof(char);
        byte[] data = new byte[size];

        // char型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をchar型へ変換します.
            element = BitConverter.ToChar(data, 0);
            return true;
        }

        return false;
    }


    //
    // データをfloat型へデシリアライズ.
    //
    protected bool Deserialize(ref float element)
    {
        int size = sizeof(float);
        byte[] data = new byte[size];

        // float型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をfloat型へ変換します.
            element = BitConverter.ToSingle(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをdouble型へデシリアライズ.
    //
    protected bool Deserialize(ref double element)
    {
        int size = sizeof(double);
        byte[] data = new byte[size];

        // double型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をdouble型へ変換します.
            element = BitConverter.ToDouble(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをshort型へデシリアライズ.
    //
    protected bool Deserialize(ref short element)
    {
        int size = sizeof(short);
        byte[] data = new byte[size];

        // short型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をshort型へ変換します.
            element = BitConverter.ToInt16(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをushort型へデシリアライズ.
    //
    protected bool Deserialize(ref ushort element)
    {
        int size = sizeof(ushort);
        byte[] data = new byte[size];

        // ushort型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をushort型へ変換します.
            element = BitConverter.ToUInt16(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをint型へデシリアライズ.
    //
    protected bool Deserialize(ref int element)
    {
        int size = sizeof(int);
        byte[] data = new byte[size];

        // int型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をint型へ変換します.
            element = BitConverter.ToInt32(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをuint型へデシリアライズ.
    //
    protected bool Deserialize(ref uint element)
    {
        int size = sizeof(uint);
        byte[] data = new byte[size];

        // uint型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をuint型へ変換します.
            element = BitConverter.ToUInt32(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをlong型へデシリアライズ.
    //
    protected bool Deserialize(ref long element)
    {
        int size = sizeof(long);
        byte[] data = new byte[size];

        // long型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をlong型へ変換します.
            element = BitConverter.ToInt64(data, 0);
            return true;
        }

        return false;
    }

    //
    // データをulong型へデシリアライズ.
    //
    protected bool Deserialize(ref ulong element)
    {
        int size = sizeof(ulong);
        byte[] data = new byte[size];

        // ulong型のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // 読みだした値をulong型へ変換します.
            element = BitConverter.ToUInt64(data, 0);
            return true;
        }

        return false;
    }

    //
    // byte[]型のデータへデシリアライズ.
    //
    protected bool Deserialize(ref byte[] element, int length)
    {
        // 配列のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref element, length);

        // byte列はデータの塊として保存されてのでエンディアン変換しない
        // ためバッファここででもとに戻します.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(element);
        }

        if (ret == true)
        {
            return true;
        }

        return false;
    }

    //
    // string型のデータへデシリアライズ.
    //
    protected bool Deserialize(ref string element, int length)
    {
        byte[] data = new byte[length];

        // 文字列のサイズ分データを読み込みます.
        bool ret = ReadBuffer(ref data, data.Length);
        if (ret == true)
        {
            // byte列はデータの塊として保存されてのでエンディアン変換しない
            // ためバッファここででもとに戻します.
            if (m_endianness == Endianness.LittleEndian)
            {
                Array.Reverse(data);
            }
            string str = System.Text.Encoding.UTF8.GetString(data);
            element = str.Trim('\0');

            return true;
        }

        return false;
    }

    //
    // 指定サイズをデータの先頭からホストバイトオーダーに変換して読み出す.
    //
    protected bool ReadBuffer(ref byte[] data, int size)
    {
        // 現在のオフセットからデータを読み出します.
        try
        {
            m_buffer.Position = m_offset;
            m_buffer.Read(data, 0, size);
            m_offset += size;
        }
        catch
        {
            return false;
        }

        // 読みだした値をホストバイトオーダーに変換します.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(data);
        }

        return true;
    }

    //
    // バイトオーダー変更後のデータをストリームに追加.
    //
    protected bool WriteBuffer(byte[] data, int size)
    {
        // 書き込む値をネットワークバイトオーダーに変換します.
        if (m_endianness == Endianness.LittleEndian)
        {
            Array.Reverse(data);
        }

        // 現在のオフセットからデータを書き込みます.
        try
        {
            m_buffer.Position = m_offset;
            m_buffer.Write(data, 0, size);
            m_offset += size;
        }
        catch
        {
            return false;
        }

        return true;
    }

    //
    // エンディアンを取得.
    //
    public Endianness GetEndianness()
    {
        return m_endianness;
    }

    //
    // データのサイズを取得.
    //
    public long GetDataSize()
    {
        return m_buffer.Length;
    }
}



#endif