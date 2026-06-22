// パケットデータをスレッド間で共有するためのバッファ
//
// ■プログラムの説明
// MemoryStreamを使用してパケットのキューイングを行うモジュールです.
// MemoryStreamはデータを1次元のストリームで管理するクラスです.
// 送受信するパケットはデータサイズがデータの種類により不定になるため効率よくバッファリングするMemoryStreamを使用するとよいでしょう.
// データサイズが不定なためデータの先頭位置とサイズをパケットごとに保存してキューイングします.
// ゲームプログラムの Send() 関数ではキューの最後尾に送信するためのデータを追加します.実際のソケットによる送信時にキューの先頭から取り出して送信します.
// ゲームプログラムの Recieve() 関数でキューに溜まっているデータを先頭から取り出します.実際のソケットによる受信時にキューの最後尾に追加します.
// 

#if true

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// パケットデータをスレッド間で共有するためのバッファ
public class PacketQueue
{

    // パケットを保存するキュー
    // FIFO(先入れ先出し)で管理する
    // Enqueue() で最後尾へ追加
    // Dequeue() で先頭から取得
    private Queue<byte[]> m_queue = new Queue<byte[]>();

    // スレッドセーフ用ロックオブジェクト
    private Object lockObj = new Object();

    /// <summary>
    /// パケットをキューへ追加（送信用バッファに積む）
    /// </summary>
    public int Enqueue(byte[] data, int size)
    {
        // 受け取ったデータサイズ分だけ新しい配列を作成
        // 元の配列をそのまま保持すると、
        // 呼び出し元で内容が変更された場合にキュー内のデータも変化してしまうためコピーする
        byte[] copy = new byte[size];

        // 元データをコピー
        // data[0]からsizeバイト分をcopyへ保存する
        Buffer.BlockCopy( data, 0, copy, 0, size);

        // Queueへのアクセスを排他制御
        lock (lockObj)
        {
            // コピーしたパケットデータをキューの最後尾へ追加
            // Enqueueだと追加していくだけで済むのでoffset管理する必要がない
            m_queue.Enqueue(copy);
        }

        // 追加したデータサイズを返す
        return size;
    }

    // キューの取り出し.(デキュー)
    public int Dequeue(ref byte[] buffer, int size)
    {
        // Queueへのアクセスをロック
        lock (lockObj)
        {
            // キューにデータがない場合関数を終了させる
            if (m_queue.Count <= 0)
            {
                return -1;
            }

            // キューの先頭からパケットを取得
            // Dequeueは取得したデータをQueueから削除する
            byte[] packet = m_queue.Dequeue();

            // 取り出したいパケットが受け取り用バッファより大きい場合
            // コピーできないので失敗とする（復元の手順ミスを想定)
            if (packet.Length > size)
            {
                return -1;
            }

            // キューから取り出したパケットをゲーム側で使用するbufferへコピー
            Buffer.BlockCopy( packet, 0, buffer, 0, packet.Length);

            // 実際に取得したパケットサイズを返す
            return packet.Length;
        }

    }
}

#else

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class PacketQueue
{
    // パケット格納情報.
    struct PacketInfo
    {
        public int offset;
        public int size;
    };

    private MemoryStream m_streamBuffer;

    private List<PacketInfo> m_offsetList;

    // スレッドセーフ用ロックオブジェクト
    private Object lockObj = new Object();

    private int m_offset = 0;

    //  コンストラクタ(ここで初期化を行います).
    public PacketQueue()
    {
        m_streamBuffer = new MemoryStream();
        m_offsetList = new List<PacketInfo>();
    }

    /// <summary>
    /// パケットをキューへ追加（送信用バッファに積む）
    /// </summary>
    public int Enqueue(byte[] data, int size)
    {
        PacketInfo info = new PacketInfo();

        lock (lockObj)
        {

            //現在の書き込み位置を保存
            info.offset = m_offset;

            // パケットサイズを保存
            info.size = size;

            // パケット格納情報を保存します.
            m_offsetList.Add(info);

            // パケットデータを保存します.
            m_streamBuffer.Position = m_offset;
            m_streamBuffer.Write(data, 0, size);
            m_streamBuffer.Flush();

            // 次の書き込み位置を更新
            m_offset += size;
        }

        return size;
    }

    // キューの取り出し.(デキュー)
    public int Dequeue(ref byte[] buffer, int size)
    {

        int recvSize = 0;

        lock (lockObj)
        {
            if (m_offsetList.Count <= 0)
            {
                return -1;
            }

            // 先頭パケット情報を取得
            PacketInfo info = m_offsetList[0];

            // バッファから該当するパケットデータを取得します.
            int dataSize = Math.Min(size, info.size);

            // ストリームから該当位置を読む
            m_streamBuffer.Position = info.offset;
            recvSize = m_streamBuffer.Read(buffer, 0, dataSize);

            // キューデータを取り出したので先頭要素を削除します.
            if (recvSize > 0)
            {
                m_offsetList.RemoveAt(0);
            }

            // すべてのキューデータを取り出したときはストリームをクリアしてメモリを節約します.
            if (m_offsetList.Count == 0)
            {
                Clear();
                m_offset = 0;
            }
        }

        return recvSize;
    }

    // キューをクリア.	
    public void Clear()
    {
        // このロックはほかのロック内で呼ばれるので必要ではないが、単体で呼ばれるとスレッドセーフではなくなるためC#では一応つけておく
        lock (lockObj)
        {
            byte[] buffer = m_streamBuffer.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);

            m_streamBuffer.Position = 0;
            m_streamBuffer.SetLength(0);
        }
    }
}


#endif