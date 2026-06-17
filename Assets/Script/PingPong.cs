// 「フロクのいなりずし」の全体シーケンス、フレーム制御
//
// ■プログラムの説明
// 通信の接続、切断、送受信の定型処理をまとめたクラスです.
// 接続全般の処理を OnGUI()、UpdateReady() 関数で行います.
// ゲームは UpdateGame() 関数でを行っています. 
// ゲーム本体のプログラムは GameControllerクラス で処理していますがゲーム内の通信制御は このクラスで処理しています.
// ゲームで使用するデータの送受信は LateUpdate() 関数内の NetworkController.UpdateSync() 関数で処理しています.
// 切断を検知したら NotifyDisconnection() 関数が呼び出されます.
//
/// 役割
/// ・タイトル画面
/// ・ネットワーク接続
/// ・ゲーム開始
/// ・ゲーム進行
/// ・リザルト表示
/// ・切断監視
///
/// また、NetworkControllerを利用して
/// フレーム同期を制御する。
/// 
/// FixedUpdate()
/// ↓
/// ゲーム1フレーム進行
/// ↓
/// Time.timeScale = 0
///

#if true

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;


public class PingPong : MonoBehaviour
{

    // サーバー側プレイヤーのバー
    public GameObject m_serverBarPrefab;
    // クライアント側プレイヤーのバー
    public GameObject m_clientBarPrefab;
    // ゲーム進行管理
    public GameObject m_gameControllerPrefab;
    // リザルト画面管理
    public GameObject m_resultControllerPrefab;

    // 現在のゲーム状態
    GameMode m_gameMode;
    // デフォルトのタイムスケールを記憶しておく.
    float m_timeScale;

    // 接続先IPアドレス
    string m_hostAddress;
    // ネットワーク同期管理
    NetworkController m_networkController = null;

    public enum GameMode
    {
        Ready = 0,  //接続待ち.        
        Game,       //ゲーム中.
        Result,     //結果表示.
    };


    void Awake()
    {
        // デフォルト速度を保存
        m_timeScale = 1;

        // ネットワーク同期完了までは停止
        Time.timeScale = 0;
    }


    // Use this for initialization
    void Start()
    {
        // 接続待ち状態から開始
        m_gameMode = GameMode.Ready;

        // ホスト名を取得します.
        m_hostAddress = GetServerIPAddress();
    }

    void FixedUpdate()
    {

        switch (m_gameMode)
        {
            case GameMode.Ready:
                UpdateReady();
                break;

            case GameMode.Game:
                UpdateGame();
                break;

            case GameMode.Result:
                UpdateResult();
                break;
        }


        /*
      通常のUnityゲームなら

      FixedUpdate
      ↓
      物理更新
      ↓
      次フレーム

      ですが、このゲームでは

      同期確認
      ↓
      同期済みなら1フレーム進める
      ↓
      すぐ停止
      ↓
      同期確認 ・・・Loop

      という制御になっています。

     */
        // フレーム同期を進行してよいかチェックします.
        if (m_networkController != null && m_networkController.IsSync())
        {
            // ネットワーク入力が揃っていたら同期済みフラグを消す。
            m_networkController.ClearSync();

            Time.timeScale = 0; //この周のFixedUpdate関連の更新はfixedDeltaTimｅで更新されるが、2回目以降の呼び出しを防ぐため=0としています.
        }
    }

    // ここで通信処理を行います。
    void LateUpdate()
    {

        if (m_networkController != null)
        {
            if (m_networkController.UpdateSync())
            {
                Resume();
            }
            else
            {
                Suspend();
            }
        }



    }


    // 接続待ち.
    void UpdateReady()
    {

        //通信接続待ちをしてからゲームを始めます.
        if (m_networkController != null)
        {

            // ゲーム開始
            if (m_networkController.IsConnected() == true)
            {

                NetworkController.HostType hostType = m_networkController.GetHostType();

                // GameStart() を呼びます
                GameStart(hostType == NetworkController.HostType.Server);

                // 状態遷移
                m_gameMode = GameMode.Game;
            }
        }
    }


    // ゲーム中.
    void UpdateGame()
    {

        // m_gameControllerPrefab を検索して変数に確保
        GameObject gameController = GameObject.Find(m_gameControllerPrefab.name);
        if (gameController == null)
        {
            // もし存在しなければ自分で生成
            gameController = Instantiate(m_gameControllerPrefab) as GameObject;
            gameController.name = m_gameControllerPrefab.name;
            GameObject.Find("BGM").GetComponent<AudioSource>().Play();    //BGM再生.
            return;
        }

        // 勝敗判定 
        if (gameController.GetComponent<GameController>().IsEnd())
        {

            // 相手へ ゲーム終了したので切断したい とメッセージを送る 0x01 (0001)
            m_networkController.SuspendSync();

            // 相手も両状済みのメッセージが届いた 0x03 (0011)
            if (m_networkController.IsSuspned() == true)
            {

                // リザルトへ移行
                m_gameMode = GameMode.Result;
            }
        }
    }


    // 結果表示.
    void UpdateResult()
    {
        //結果表示して勝ち負けを出す.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);

        if (resultController == null)
        {
            // m_resultControllerPrefab を検索して存在しなかったので自分で生成
            resultController = Instantiate(m_resultControllerPrefab) as GameObject;
            resultController.name = m_resultControllerPrefab.name;
            GameObject.Find("BGM").SendMessage("FadeOut");    //BGMフェードアウト.
            return;
        }
    }


    // ゲーム再開
    public void Resume()
    {
        Time.timeScale = m_timeScale;
    }

    // ゲーム停止
    public void Suspend()
    {
        Time.timeScale = 0;
    }


    // ゲーム開始.
    void GameStart(bool isServer)
    {

        // プレイヤー生成 (バーを生成します)
        GameObject serverBar = Instantiate(m_serverBarPrefab) as GameObject;
        serverBar.GetComponent<BarScript>().SetBarId(0);
        serverBar.name = m_serverBarPrefab.name;
        GameObject clientBar = Instantiate(m_clientBarPrefab) as GameObject;
        clientBar.GetComponent<BarScript>().SetBarId(1);
        clientBar.name = m_clientBarPrefab.name;


        // クライアントの場合は2P用のカメラにします (画面上下反転処理)
        if (isServer == false)
        {
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos.y *= -1;
            cameraPos.x *= -1;
            Camera.main.transform.position = cameraPos;

            Vector3 cameraRot = Camera.main.transform.rotation.eulerAngles;
            cameraRot.x *= -1;
            cameraRot.y *= -1;
            cameraRot.z += 180; // カメラ180度回転
            Camera.main.transform.rotation = Quaternion.Euler(cameraRot);

            GameObject light = GameObject.Find("Directional light");
            Vector3 lightRot = light.transform.rotation.eulerAngles;
            lightRot.x *= -1;
            light.transform.rotation = Quaternion.Euler(lightRot);
        }
    }



    void OnGUI()
    {

        // ボタンが押されたら通信をスタートします.
        if (m_networkController == null)
        {
            PlayerInfo info = PlayerInfo.GetInstance();

            int x = 50;
            int y = 650;

            // クライアントを選択した時の接続するサーバのアドレスを入力します.
            GUIStyle style = new GUIStyle();
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(x, y - 25, 200.0f, 50.0f), "対戦相手のIPアドレス", style);
            m_hostAddress = GUI.TextField(new Rect(x, y, 200, 20), m_hostAddress);
            y += 25;

            if (GUI.Button(new Rect(x, y, 150, 20), "対戦相手を待ちます"))
            {
                // << サーバー >> として起動します.
                m_networkController = new NetworkController(m_hostAddress, true);
                info.SetPlayerId(0);  // プレイヤーIDを設定します.

                GameObject.Find("Title").SetActive(false); // タイトル表示OFF.
            }

            if (GUI.Button(new Rect(x + 160, y, 150, 20), "対戦相手と接続します"))
            {
                // << クライアント >> として起動します.
                m_networkController = new NetworkController(m_hostAddress, false);
                info.SetPlayerId(1);  // プレイヤーIDを設定します.

                GameObject.Find("Title").SetActive(false); // タイトル表示OFF.
            }
        }

        // リザルト終了時はボタンで << リセット >> できるようにします.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);
        if (resultController && resultController.GetComponent<ResultController>().IsEnd())
        {
            // 終了ボタンを表示します.
            if (GUI.Button(new Rect(20, Screen.height - 100, 80, 80), "RESET"))
            {
                SceneManager.LoadScene("PingPong");
                m_networkController.Disconnect();
                m_networkController = null;
            }
            return;
        }

        // 切断検知 (ゲーム中に相手が落ちた場合)
        if (m_networkController != null &&
m_networkController.IsConnected() == false &&
    m_networkController.IsSuspned() == false &&
    m_networkController.GetSyncState() != NetworkController.SyncState.NotStarted)
        {
            // 切断画面表示 (切断しました)
            NotifyDisconnection();
        }
    }


    // 切断通知.
    void NotifyDisconnection()
    {
        GUISkin skin = GUI.skin;
        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
        style.normal.textColor = Color.white;
        style.fontSize = 25;

        float sx = 450;
        float sy = 200;
        float px = Screen.width / 2 - sx * 0.5f;
        float py = Screen.height / 2 - sy * 0.5f;

        string message = "回線が切断しました.\n\nぼたんをおしてね.";

        // 終了ボタンを表示します.  
        if (GUI.Button(new Rect(px, py, sx, sy), message, style))
        {
            // ゲームが終了しました.
            SceneManager.LoadScene("PingPong");
        }
    }


    // 端末のIPアドレスを取得 (最初の章の SampleScene でもあったので、Library化してもいいかも)
    public string GetServerIPAddress()
    {

        string hostAddress = "";

        // ホスト名取得
        string hostname = Dns.GetHostName();

        // ホスト名からIPアドレスを取得します.
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);

        // IPアドレスメイキング
        for (int i = 0; i < adrList.Length; ++i)
        {
            string addr = adrList[i].ToString();
            string[] c = addr.Split('.');

            // IPv4選択
            if (c.Length == 4)
            {
                hostAddress = addr;
                break;
            }
        }

        return hostAddress;
    }
}


#else

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;


public class PingPong : MonoBehaviour {

    // サーバー側プレイヤーのバー
    public GameObject m_serverBarPrefab;
    // クライアント側プレイヤーのバー
    public GameObject m_clientBarPrefab;
    // ゲーム進行管理
    public GameObject m_gameControllerPrefab;
    // リザルト画面管理
    public GameObject m_resultControllerPrefab;

    // 現在のゲーム状態
    GameMode m_gameMode;
    // デフォルトのタイムスケールを記憶しておく.
    float m_timeScale;

    // 接続先IPアドレス
    string m_hostAddress;
    // ネットワーク同期管理
    NetworkController m_networkController = null;

  public enum GameMode {
    Ready = 0,  //接続待ち.        
        Game,       //ゲーム中.
        Result,     //結果表示.
  };


  void Awake()
  {
        // デフォルト速度を保存
        m_timeScale = 1;

        // ネットワーク同期完了までは停止
        Time.timeScale = 0;
    }


  // Use this for initialization
  void Start()
  {
        // 接続待ち状態から開始
        m_gameMode = GameMode.Ready;

        // ホスト名を取得します.
    m_hostAddress = GetServerIPAddress();
  }

  void FixedUpdate() {

        switch (m_gameMode) {
    case GameMode.Ready:
      UpdateReady();
      break;

    case GameMode.Game:
      UpdateGame();
      break;

    case GameMode.Result:
      UpdateResult();
      break;
    }


        /*
      通常のUnityゲームなら

      FixedUpdate
      ↓
      物理更新
      ↓
      次フレーム

      ですが、このゲームでは

      同期確認
      ↓
      同期済みなら1フレーム進める
      ↓
      すぐ停止
      ↓
      同期確認 ・・・Loop

      という制御になっています。

     */
        // フレーム同期を進行してよいかチェックします.
        if (m_networkController != null && m_networkController.IsSync()) {
            // ネットワーク入力が揃っていたら同期済みフラグを消す。
            m_networkController.ClearSync();

            Time.timeScale = 0; //この周のFixedUpdate関連の更新はfixedDeltaTimｅで更新されるが、2回目以降の呼び出しを防ぐため=0としています.
    }
  }

    // ここで通信処理を行います。
    void LateUpdate(){

        if (m_networkController != null) {

            // UpdateSync() で 入力取得 → 送信 → 受信 → 同期判定 をやっています
            if (m_networkController.UpdateSync()) {
                // 停止状態を解除します
                Resume(); // Time.timeScale = 1; になり FixedUpdate が可能になります
            }
            else {
                // 入力情報を受信していないため次のフレームを処理することができません.
                Suspend(); // Time.timeScale = 0; のままゲームは停止
            }
    }
  }


  // 接続待ち.
  void UpdateReady(){

        //通信接続待ちをしてからゲームを始めます.
        if (m_networkController != null) {

            // ゲーム開始
            if (m_networkController.IsConnected() == true) {

                NetworkController.HostType hostType = m_networkController.GetHostType();

                // GameStart() を呼びます
                GameStart(hostType == NetworkController.HostType.Server);

                // 状態遷移
                m_gameMode = GameMode.Game;
            }
        }
  }


    // ゲーム中.
    void UpdateGame() {

        // m_gameControllerPrefab を検索して変数に確保
        GameObject gameController = GameObject.Find(m_gameControllerPrefab.name);
        if (gameController == null) {
            // もし存在しなければ自分で生成
            gameController = Instantiate(m_gameControllerPrefab) as GameObject;
            gameController.name = m_gameControllerPrefab.name;
            GameObject.Find("BGM").GetComponent<AudioSource>().Play();    //BGM再生.
            return;
        }

        // 勝敗判定 
        if (gameController.GetComponent<GameController>().IsEnd()) {

            // 相手へ ゲーム終了したので切断したい とメッセージを送る 0x01 (0001)
            m_networkController.SuspendSync();

            // 相手も両状済みのメッセージが届いた 0x03 (0011)
            if (m_networkController.IsSuspned() == true) {

        // リザルトへ移行
        m_gameMode = GameMode.Result;
      }
        }
    }


  // 結果表示.
  void UpdateResult(){
        //結果表示して勝ち負けを出す.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);

        if (resultController == null) {
            // m_resultControllerPrefab を検索して存在しなかったので自分で生成
            resultController = Instantiate(m_resultControllerPrefab) as GameObject;
            resultController.name = m_resultControllerPrefab.name;
            GameObject.Find("BGM").SendMessage("FadeOut");    //BGMフェードアウト.
            return;
        }
  }


    // ゲーム再開
    public void Resume(){
        Time.timeScale = m_timeScale;
  }

    // ゲーム停止
    public void Suspend(){
    Time.timeScale = 0;
  }


  // ゲーム開始.
  void GameStart(bool isServer){

        // プレイヤー生成 (バーを生成します)
        GameObject serverBar = Instantiate(m_serverBarPrefab) as GameObject;
        serverBar.GetComponent<BarScript>().SetBarId(0);
        serverBar.name = m_serverBarPrefab.name;
        GameObject clientBar = Instantiate(m_clientBarPrefab) as GameObject;
        clientBar.GetComponent<BarScript>().SetBarId(1);
        clientBar.name = m_clientBarPrefab.name;


        // クライアントの場合は2P用のカメラにします (画面上下反転処理)
        if (isServer == false) {
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos.y *= -1;
            cameraPos.x *= -1;
            Camera.main.transform.position = cameraPos;

            Vector3 cameraRot = Camera.main.transform.rotation.eulerAngles;
            cameraRot.x *= -1;
            cameraRot.y *= -1;
            cameraRot.z += 180; // カメラ180度回転
            Camera.main.transform.rotation = Quaternion.Euler(cameraRot);

            GameObject light = GameObject.Find("Directional light");
            Vector3 lightRot = light.transform.rotation.eulerAngles;
            lightRot.x *= -1;
            light.transform.rotation = Quaternion.Euler(lightRot);
        }
  }



    void OnGUI() {

        // ボタンが押されたら通信をスタートします.
        if (m_networkController == null) {
            PlayerInfo info = PlayerInfo.GetInstance();

      int x = 50;
      int y = 650;

      // クライアントを選択した時の接続するサーバのアドレスを入力します.
      GUIStyle style = new GUIStyle();
      style.fontSize = 18;
      style.fontStyle = FontStyle.Bold;
      style.normal.textColor = Color.black;
      GUI.Label(new Rect(x, y-25, 200.0f, 50.0f), "対戦相手のIPアドレス", style);
      m_hostAddress = GUI.TextField(new Rect(x, y, 200, 20), m_hostAddress);
      y += 25;

      if (GUI.Button(new Rect(x, y, 150, 20), "対戦相手を待ちます")) {
        // << サーバー >> として起動します.
        m_networkController = new NetworkController(m_hostAddress, true);
                info.SetPlayerId( 0 );  // プレイヤーIDを設定します.

                GameObject.Find("Title").SetActive(false); // タイトル表示OFF.
            }

      if (GUI.Button(new Rect(x+160, y, 150, 20), "対戦相手と接続します")) {
        // << クライアント >> として起動します.
        m_networkController = new NetworkController(m_hostAddress, false);
                info.SetPlayerId( 1 );  // プレイヤーIDを設定します.

                GameObject.Find("Title").SetActive(false); // タイトル表示OFF.
            }
        }

        // リザルト終了時はボタンで << リセット >> できるようにします.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);
        if (resultController && resultController.GetComponent<ResultController>().IsEnd()) {
            // 終了ボタンを表示します.
            if (GUI.Button(new Rect(20, Screen.height - 100, 80, 80), "RESET")) {
                SceneManager.LoadScene("PingPong");
        m_networkController.Disconnect();
        m_networkController = null;
            } 
      return;
        }

        // 切断検知 (ゲーム中に相手が落ちた場合)
        if (m_networkController != null &&
      m_networkController.IsConnected() == false &&
        m_networkController.IsSuspned() == false &&
        m_networkController.GetSyncState() != NetworkController.SyncState.NotStarted) {
            // 切断画面表示 (切断しました)
            NotifyDisconnection(); 
    }
    }


  // 切断通知.
  void NotifyDisconnection()
  {
    GUISkin skin = GUI.skin;
    GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
    style.normal.textColor = Color.white;
    style.fontSize = 25;

    float sx = 450;
    float sy = 200;
    float px = Screen.width / 2 - sx * 0.5f;
    float py = Screen.height / 2 - sy * 0.5f;

    string message = "回線が切断しました.\n\nぼたんをおしてね.";

    // 終了ボタンを表示します.  
    if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
      // ゲームが終了しました.
      SceneManager.LoadScene("PingPong");
    }
  }


  // 端末のIPアドレスを取得 (最初の章の SampleScene でもあったので、Library化してもいいかも)
  public string GetServerIPAddress() {

    string hostAddress = "";

        // ホスト名取得
        string hostname = Dns.GetHostName();

    // ホスト名からIPアドレスを取得します.
    IPAddress[] adrList = Dns.GetHostAddresses(hostname);

        // IPアドレスメイキング
    for (int i = 0; i < adrList.Length; ++i) {
      string addr = adrList[i].ToString();
      string [] c = addr.Split('.');

            // IPv4選択
            if (c.Length == 4) {
        hostAddress = addr;
        break;
      }
    }

    return hostAddress;
  }
}


#endif