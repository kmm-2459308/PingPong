// UDP通信を行う通信モジュール
//
// ■プログラムの説明
// UDPプロトコルを用いたデータの送受信を行うモジュールです.
// データを受信するための動作をサーバーのように処理しています.
// 通信相手のデータを受信するためのサーバー処理を実行して待ち受けを行います.
// 通信相手が Connect() 関数を呼び出して接続したらお互いに送受信できる状態になります.
// 通信相手と Send() 関数、Recieve() 関数で双方向のデータ通信を行います.
// 
// ●待ち受けの動作
//   受信用ソケットを生成し、クライアントと送受信を行う送受信関数をゲームスレッドとは別のスレッド(通信スレッド)として起動します.
//   通信相手から送信されたメッセージを受信してゲームスレッドに渡すためにキューにバッファリングします.
//   待ち受けの動作は次の関数で処理しています.
//     StartServer() 関数でクライアントとの通信を開始します.
//     StopServer() 関数でクライアントとの通信を終了します.
//
// ●通信相手への接続の動作
//   Connect() 関数で待ち受けをしてる通信相手に接続を行います.
//   Disconnect() 関数で通信相手と切断します.
//
// ●送受信処理
//　　ゲームプログラムからデータを送受信を行うために　Send() 関数、Receive() 関数を使用します.
//   これらの関数はソケットによる送受信をしていません.
//   実際の送受信は通信スレッド側で行います.これはデータの祖受信によるオーバーヘッドでゲームの処理時間を消費しないようにしています.
//   ソケットによる送受信は DispatchSend() 関数、DispatchReceive() 関数で処理しています.
//
// ●イベント処理
//	  通信相手の接続や切断をゲームプログラムに通知するための仕組みをイベントハンドラーとして登録、削除をします.
//    ゲームプログラム側に EventHandler 型の関数 [例： void foo(NetEventState state) { ... } ]を定義します.
//    定義した関数を RegisterEventHandler() 関数、UnregisterEventHandler() 関数に渡してイベントハンドラーに登録、解除をします.
//    通信相手が接続、切断した時にイベントハンドラーに登録した関数が呼び出され、ゲームプログラムの関数がデリゲートからが呼び出されます.
//    イベント発生時の関数呼び出しは AcceptClient() 関数、Connect() 関数、Disconnect() 関数を参照してください.
//
// ■備考
// TransportTCPクラスとTransportUDPクラスのメソッド(関数)の書式は両方のクラスで一致しています.
// このように書式を合わせておくと送受信を行うプロトコル(TCPまたはUDP)をクラスの宣言を切り替えるだけ変更できます.
// 

using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TransportUDP : MonoBehaviour {

	private Socket			m_socket = null;

	private Thread			m_thread = null;

	private	bool			m_isStarted = false;

	// サーバーフラグ.	
	private bool 			m_isServer = false;
	
	// 接続フラグ.
	private	bool			m_isConnected = false;

	// 送信バッファ.
	private PacketQueue		m_sendQueue;
	
	// 受信バッファ.
	private PacketQueue		m_recvQueue;
	

	// 送受信用のパケットの最大サイズ.
	// バッファのサイズはMTUの設定によって決まります.(MTU:1回に送信できる最大のデータサイズ)
	// イーサネットの最大MTUは1500bytesです.
	// この値はOSや端末などで異なるものですのでバッファのサイズは
	// 動作させる環境のMTUを調べて設定しましょう.
	private const int		m_packetSize = 1400;


	// タイムアウト時間.
	private const int 		m_timeOutSec = 5;

	private DateTime		m_ticker;

	//
	// イベント関連のメンバ変数.
	//
	
	// イベント通知のデリゲート.
	public delegate void 	EventHandler(NetEventState state);
	// イベントハンドラー.
	private EventHandler	m_handler;


	// Use this for initialization
	void Start()
	{
		// 送受信バッファを作成します.
		m_sendQueue = new PacketQueue();
		m_recvQueue = new PacketQueue();
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
	
	}

	// アプリケーション終了時の処理.
	void OnApplicationQuit()
	{
		if (m_isStarted == true) {
			StopServer();
		}
	}

	// 待ち受け開始.
	public bool StartServer(int port)
	{
		Debug.Log("Start server called[Port:" + port + "]");
		
		// リスニングソケットを生成します.
		try {
			if (m_socket == null) {
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			m_socket.Bind(new IPEndPoint(IPAddress.Any, port));
		}
		catch {
			return false;
		}

		m_isServer = true;

		return LaunchThread();
	}

	// 待ち受け終了.
	public void StopServer()
	{
		m_isStarted = false;
		if (m_thread != null) {
			m_thread.Join();
			m_thread = null;
		}

		Disconnect ();

		if (m_socket != null) {
			m_socket.Close();
			m_socket = null;
		}		

		m_isServer = false;
		m_isStarted = false;

		Debug.Log("Server stopped.");
	}

    // 接続処理.
	public bool Connect(string address, int port)
	{
		Debug.Log("TransportUdp::Connect called.[Port:" + port + "]");

		try {
			if (m_socket == null) {
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			// ※UDPでも通信相手とConnect関数を呼び出して接続して通信することもできます.
			//   接続して通信する場合は送信時にIPアドレスを指定しないSend関数を使用して送信することもできます.
			//   ここではTransportTCPと同じ関数で動作させるためConnect関数を使用しています.
			//   IPアドレスとポート番号をこのクラスで管理することでConnect関数を使用しないで通信することもできます.   
			m_socket.Connect(address, port);
		}
		catch {
			Debug.Log("TransportUdp::Connect failed.");
			return false;
		}

		bool ret = true;
		if (m_isStarted == false) {
			ret = LaunchThread();
			if (ret == true) {
				m_isConnected = true;
			}
		}

		// 接続を通知します.
		if (m_handler != null) {
	        // ゲームアプリケーションは他のプレイヤーが入室したときにユーザーへ通知するのがよいでしょう.
	        // そのため入室したことをアプリケーションがわかるようにアプリケーション側の関数を呼び出すようにします.
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = NetEventResult.Success;
			m_handler(state);
		}

		Debug.Log("TransportUdp::Connect success.");

		return ret;
	}

	// 切断処理.
	public bool Disconnect()
	{
		if (m_socket != null) {
            // ソケットをクローズします.
			try {
				m_socket.Close();
				m_socket = null;
			}
			catch (SocketException e) {
				Debug.Log(e.Message);
			}
				
			// 切断を通知します.
			if (m_handler != null) {
		        // ゲームアプリケーションは他のプレイヤーが切断したときにユーザーに通知するのがよいでしょう.
   				// そのため切断したことをアプリケーションがわかるようにアプリケーション側の関数を呼び出すようにします.
				NetEventState state = new NetEventState();
				state.type = NetEventType.Disconnect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}

		m_isStarted = false;
		m_isConnected = false;
		Debug.Log("TransportTcp::Disconnect called.");

		return true;
	}

    // 送信処理.
	public int Send(byte[] data, int size)
	{
		// 送信データは一旦キューにバッファリングするだけで送信はしていません.
		// 実際の送信は通信スレッド側(DispatchSend() 関数)で行います.
		// ゲームスレッド側の処理をできるだけ軽くするために直接 Send() 関数で送信していません.
		return m_sendQueue.Enqueue(data, size);
	}
	
    // 受信処理.
	public int Receive(ref byte[] buffer, int size) 
	{
		// 実際の受信は通信スレッド側(DispatchReceive() 関数)で行います.
		// ゲームスレッド側の処理をできるだけ軽くするために直接　Receive() 関数で受信していません.
		return m_recvQueue.Dequeue(ref buffer, size);
	}

	// 接続確認.
	public bool IsConnected()
	{
		return	m_isConnected;
	}

	// サーバー動作設定確認.
	public bool IsServer()
	{
		return	m_isServer;
	}

	// イベント通知関数登録.
	public void RegisterEventHandler(EventHandler handler)
	{
		m_handler += handler;
	}
	
	// イベント通知関数削除.
	public void UnregisterEventHandler(EventHandler handler)
	{
		m_handler -= handler;
	}

	// スレッド起動関数.
	bool LaunchThread()
	{
		try {
			// Dispatch用のスレッドを起動します.
			m_thread = new Thread(new ThreadStart(Dispatch));
			m_isStarted = true;	
			m_thread.Start();
		}
		catch {
			Debug.Log("Cannot launch thread.");
			return false;
		}
		
		return true;
	}

	// 通信スレッド側の送受信処理.
	void Dispatch()
	{
		while (m_isStarted == true) {
			// クライアントからの接続を待ちます.
			AcceptClient();

			// クライアントとの送受信を処理します.
			if (m_socket != null) {			
				// 送信処理.
				DispatchSend();
				
				// 受信処理.
				DispatchReceive();

				// タイムアウト処理.
				CheckTimeout();
			}

			Thread.Sleep(3);
		}
	}

	// 通信相手の待ち受け.
	void AcceptClient()
	{
		if (m_isConnected == false &&
			m_socket != null && 
		    m_socket.Poll(0, SelectMode.SelectRead)) {
			// クライアントから接続されました.
			m_isConnected = true;
			// 通信開始時刻を記録します.
			m_ticker = DateTime.Now;

			// 接続を通知します.
	        // ゲームなどのアプリケーションは他のプレイヤーが入室したときにユーザーに通知するのがよいでしょう.
	        // そのため入室したことをアプリケーションがわかるようにアプリケーション側の関数を呼び出すようにします.
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}
	}

	// スレッド側の送信処理.
    void DispatchSend()
	{
		if (m_socket == null) {
			return;
		}

		try {
			// 送信処理.
			if (m_socket.Poll(0, SelectMode.SelectWrite)) {
				byte[] buffer = new byte[m_packetSize];
				
				// Send関数でバッファリングされたデータを取り出して送信を行います.
				int sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
                // 送信データがなくなるまで送信を続けます.
				while (sendSize > 0) {
					m_socket.Send(buffer, sendSize, SocketFlags.None);	
					sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				}
			}
		}
		catch {
			return;
		}
	}
	// スレッド側の受信処理.
	void DispatchReceive()
	{
		if (m_socket == null) {
			return;
		}

		// 受信処理.
		try {
			while (m_socket.Poll(0, SelectMode.SelectRead)) {
				byte[] buffer = new byte[m_packetSize];

				int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);
                // 通信相手と切断したことにReceive関数の関数値は0が返されます.
				if (recvSize == 0) {
					// 切断します.
					Disconnect();
				}
				else if (recvSize > 0) {
	               	// ゲームスレッド側に受信したデータを渡すために受信データをキューに追加します.
 					m_recvQueue.Enqueue(buffer, recvSize);
					// 受信時刻を更新します.
					m_ticker = DateTime.Now;
				}
			}
		}
		catch {
			return;
		}
	}

	// タイムアウトチェック.
	void CheckTimeout()
	{
		TimeSpan ts = DateTime.Now - m_ticker;

		if (m_isConnected && ts.Seconds > m_timeOutSec) {
			Debug.Log("Disconnect because of timeout.");
			// タイムアウトする時間までにデータが届かなかった.
			// 理解を簡単にするために,あえて通信スレッドからメインスレッドを呼び出しています.
			// 本来ならば切断リクエストを発行して,メインスレッド側でリクエストを監視して.
			// メインスレッド側の処理で切断を行うようにしましょう.
			Disconnect();
		}
	}
}
