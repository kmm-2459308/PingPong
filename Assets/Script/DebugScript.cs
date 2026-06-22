// 通信内容などのデバッグログなど

/*
 ■ DebugScript の役割

デバッグ用ログをファイルへ出力するクラス。

主な用途

・通信データ確認
・プレイヤー位置確認
・ブロック配置確認
・ゲーム中の状態記録

実行中の情報を

DebugScript.log

へ書き出している。



■ 変数の役割

System.IO.StreamWriter m_writer;

ログファイルへ書き込むためのオブジェクト。


GameObject m_inputManager;

InputManagerオブジェクト。


GameObject m_serverBar;

サーバー側のバー。


GameObject m_clientBar;

クライアント側のバー。



■ Awake()

ゲーム開始時に呼ばれる。

Start() より先に実行される。


----------------------------------
ログファイル作成
----------------------------------

string filename =
    Application.dataPath +
    "/DebugScript.log";

例

C:/MyGame/Assets/DebugScript.log


ログファイルを開く。

m_writer =
    new StreamWriter(filename);


開始ログを書き込む。

m_writer.WriteLine("DebugStart");


----------------------------------
オブジェクト取得
----------------------------------

m_inputManager =
    GameObject.Find("InputManager");

InputManager取得。


バーはまだ生成されていない可能性があるため

m_serverBar = null;
m_clientBar = null;



■ FixedUpdate()

一定時間ごとに実行される。

毎回ログを書き出している。



----------------------------------
バーの取得
----------------------------------

if(m_serverBar == null)
{
    m_serverBar =
        GameObject.Find("ServerBar");
}

if(m_clientBar == null)
{
    m_clientBar =
        GameObject.Find("ClientBar");
}

まだ取得できていなければ検索する。



----------------------------------
InputManager取得
----------------------------------

InputManager im =
    m_inputManager.GetComponent<InputManager>();



----------------------------------
プレイヤー0の入力取得
----------------------------------

MouseData data =
    im.GetMouseData(0);

取得したデータ例

frame = 120

など。


ログ文字列へ追加

str +=
    "data0.frame:" +
    data.frame;



----------------------------------
プレイヤー1の入力取得
----------------------------------

data =
    im.GetMouseData(1);

str +=
    "data1.frame:" +
    data.frame;



----------------------------------
サーバーバー位置取得
----------------------------------

if(m_serverBar)

{
    str +=
        "server.pos:" +
        m_serverBar.transform.position;
}

例

server.pos:(1.2,3.4,0)



----------------------------------
クライアントバー位置取得
----------------------------------

if(m_clientBar)

{
    str +=
        "client.pos:" +
        m_clientBar.transform.position;
}

例

client.pos:(-1.5,3.4,0)



----------------------------------
ブロック情報取得
----------------------------------

GameObject[] objs =
    GameObject.FindGameObjectsWithTag("Block");

現在存在する全ブロックを取得。



ブロック数を記録

str +=
    "Block:" +
    objs.Length;

例

Block:15



----------------------------------
ブロック座標記録
----------------------------------

foreach(GameObject o in objs)
{
    str +=
        o.transform.position;
}

例

(0,0,0)
(1,0,0)
(2,0,0)
・・・



----------------------------------
ログ出力
----------------------------------

m_writer.WriteLine(str);

ファイルへ書き込み。


m_writer.Flush();

即保存。

Flush()を行うことで

ゲームが異常終了しても

途中までのログが残る。



■ 出力されるログ例

DebugStart

--
data0.frame:120
data1.frame:118
server.pos:(0.0, -3.5, 0.0)
client.pos:(0.0, 3.5, 0.0)
Block:12
(0,1,0)
(1,1,0)
(2,1,0)

--
data0.frame:121
data1.frame:119
server.pos:(0.1,-3.5,0)
client.pos:(0.0,3.5,0)
Block:12
...



■ OnDestroy()

オブジェクト破棄時に呼ばれる。

ゲーム終了時などに実行される。



----------------------------------
終了ログ出力
----------------------------------

m_writer.WriteLine("DebugEnd");



----------------------------------
ファイルを閉じる
----------------------------------

m_writer.Close();

ファイル保存完了。



■ このスクリプトで確認できる情報

① 入力フレーム番号

data0.frame
data1.frame

通信遅延や同期ズレ確認用。



② サーバーバー位置

server.pos

サーバープレイヤーの位置確認。



③ クライアントバー位置

client.pos

クライアントプレイヤーの位置確認。



④ ブロック数

Block:○

ブロックが正しく消えているか確認。



⑤ ブロック位置

(x,y,z)

ブロック配置が一致しているか確認。



■ 処理の流れ

Awake()
 ↓
DebugStart出力
 ↓
FixedUpdate()
 ↓
入力情報取得
 ↓
バー位置取得
 ↓
ブロック情報取得
 ↓
ログ出力
 ↓
繰り返し
 ↓
OnDestroy()
 ↓
DebugEnd出力
 ↓
ファイルクローズ


■ このスクリプトの目的

ネットワーク対戦ゲームで

・入力同期が取れているか
・バー位置が一致しているか
・ブロック状態が一致しているか

を確認するためのデバッグログ収集ツール。
 */

using UnityEngine;
using System.Collections;

public class DebugScript : MonoBehaviour {
    System.IO.StreamWriter m_writer;
    GameObject m_inputManager;
    GameObject m_serverBar;
    GameObject m_clientBar;

    void Awake() {
        string filename = Application.dataPath + "/DebugScript.log";
        m_writer = new System.IO.StreamWriter(filename);
        m_writer.WriteLine("DebugStart");

        m_inputManager = GameObject.Find("InputManager");
        m_serverBar = null;
        m_clientBar = null;
    }

    void FixedUpdate() {
        if(m_serverBar == null){
            m_serverBar = GameObject.Find("ServerBar");
        }
        if(m_clientBar == null){
            m_clientBar = GameObject.Find("ClientBar");
        }

        InputManager im = m_inputManager.GetComponent<InputManager>();
        MouseData data = im.GetMouseData(0);
        
        string str = "--\n";
        str += "data0.frame:" + data.frame + "\n";

        data = im.GetMouseData(1);
        str += "data1.frame:" + data.frame + "\n";

        if (m_serverBar) {
            str += "server.pos:" + m_serverBar.transform.position.ToString() + "\n";
        }
        if (m_clientBar) {
            str += "client.pos:" + m_clientBar.transform.position.ToString() + "\n";
        }

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Block");
        str += "Block:" + objs.Length;
        foreach (GameObject o in objs) {
            str += o.transform.position.ToString();
        }

		if (m_writer != null) {
	        m_writer.WriteLine(str);
    	    m_writer.Flush();
		}
    }

    void OnDestroy() {
		if (m_writer != null) {
	        m_writer.WriteLine("DebugEnd");
    	    m_writer.Close();
		}
    }
}
