// ライブラリで使用する共通の定義
//

using UnityEngine;
using System.Collections;
using System.Net;

// 最大プレイヤー人数.
public class NetConfig
{
	public static int PLAYER_MAX = 4;

	public static int SERVER_PORT = 50764;
	public static int GAME_PORT = 50765;
}

// イベントの種類.
public enum NetEventType 
{
	Connect = 0,	// 接続イベント.
	Disconnect,		// 切断イベント.
	SendError,		// 送信エラー.
	ReceiveError,	// 受信エラー.
}

// イベントの結果.
public enum NetEventResult
{
	Failure = -1,	// 失敗.
	Success = 0,	// 成功.
}

// イベントの状態通知.
public class NetEventState
{
    public NetEventType     type;		// イベントタイプ.
    public NetEventResult   result;		// イベントの結果.
	public IPEndPoint		endPoint;	// 接続先のエンドポイント.
}
