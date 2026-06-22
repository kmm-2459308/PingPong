// 「ステージ開始 → プレイ → 制限時間終了またはクリア → ステージ切替 → 次のステージ → 全ステージ終了」
/*
■ 変数の役割

public GameObject[] m_stagePrefabs;

ステージのプレハブを登録する配列。

例:
  Stage1
  Stage2
  Stage3

を登録しておく。


GameObject m_timerObj;

タイマー表示オブジェクト。


float m_gameTime;

現在のゲーム経過時間。


int m_gameCount;

現在何ステージ目か。


const int GAMECOUNT_MAX = 3;

全3ステージ。


const int TIME_LIMIT = 30;

1ステージ30秒。



■ State列挙型

enum State
{
    GameIn,
    Game,
    GameChanging,
    GameOut,
    GameEnd
}

ゲームの状態を表している。

GameIn
  ゲーム開始準備

Game
  プレイ中

GameChanging
  ステージ切替演出中

GameOut
  ステージ終了処理中

GameEnd
  全ゲーム終了



■ Start()

ゲーム開始時に1回呼ばれる。

m_timerObj = GameObject.Find("Timer");

Timerオブジェクト取得。

m_state = State.GameIn;

最初はゲーム開始準備状態。

m_gameTime = 0;
m_gameCount = 0;

時間とステージ番号を初期化。



■ FixedUpdate()

毎フレーム呼ばれる。

状態によって処理を分けている。

switch (m_state)

例えば

State.Game

なら

UpdateGame();

が実行される。



■ UpdateGameIn()

ゲーム開始準備。

【ステージ生成】

GameObject stage = GameObject.Find("Stage");

ステージが存在するか確認。

無ければ

Instantiate(m_stagePrefabs[m_gameCount]);

現在のステージを生成。

例:

m_gameCount = 0

なら

m_stagePrefabs[0]

を生成。


【フェードイン待ち】

if (b.IsFadeIn())
{
    return;
}

ブロックの登場アニメーションが終わるまで待機。


【プレイ開始】

m_state = State.Game;

ゲーム状態へ。

bar.SetShotEnable(true);

プレイヤーがボール発射可能になる。



■ UpdateGame()

プレイ中。

【時間加算】

m_gameTime += Time.fixedDeltaTime;


【ブロック全破壊】

if (blocks.Length == 0)

クリア。


【時間切れ】

if (m_gameTime > TIME_LIMIT)

30秒超えた。


どちらかなら

m_state = State.GameChanging;

へ移行。



■ UpdateGameChanging()

ステージ終了演出。


【ブロックをフェードアウト】

b.FadeOut();


【ボール削除】

Destroy(obj);

画面上の弾を全部消す。


【発射禁止】

bar.SetShotEnable(false);

プレイヤー操作停止。


【次の状態へ】

m_state = State.GameOut;



■ UpdateGameOut()

ステージ終了処理。


【フェードアウト完了待ち】

if (b.IsFadeOut())
{
    return;
}

アニメーション終了待ち。


【ステージ削除】

Destroy(GameObject.Find("Stage"));

現在ステージを消す。


【ステージ数を進める】

++m_gameCount;


【3ステージ終了？】

if (m_gameCount == GAMECOUNT_MAX)

つまり

3 == 3

なら

m_state = State.GameEnd;


そうでなければ

m_state = State.GameIn;

次ステージへ。



■ タイマー表示

毎フレーム

float t = Mathf.Max(TIME_LIMIT - m_gameTime, 0);

残り時間を計算。

例:

経過時間    表示
0秒         30
10秒        20
25秒        5
31秒        0

number.SetNum((int)t);

画面の数字に表示。



■ IsEnd()

public bool IsEnd()
{
    return (m_state == State.GameEnd);
}

ゲーム終了判定。

他のスクリプトから

if(gameController.IsEnd())
{
    // リザルト画面へ
}

のように使える。



■ 全体の流れ

GameIn
 ↓
Game
 ↓
GameChanging
 ↓
GameOut
 ↓
次ステージ
 ↓
GameIn
 ↓
...
 ↓
GameEnd

状態遷移（State Machine）によってゲーム全体を管理している。
 
 */

using UnityEngine;
using System.Collections;

/** ゲームシーケンス担当 */
public class GameController : MonoBehaviour {
    public GameObject[] m_stagePrefabs;   //ステージ登録しておく.

    GameObject m_timerObj;  //タイマー表示物.
    float m_gameTime;       //ゲームの時間制御用.
    int m_gameCount;        //何ゲーム目かをカウントする.
    const int GAMECOUNT_MAX = 3;
    const int TIME_LIMIT = 30;  //1ゲームの制限時間.

    enum State {
        GameIn,     //ゲーム開始準備.
        Game,       //ゲーム中.
        GameChanging,//終了間際の演出.
        GameOut,    //ゲーム終了準備.
        GameEnd,    //ゲーム終了.
    };
    State m_state;


	// Use this for initialization
	void Start () {
        m_timerObj = GameObject.Find("Timer");
        m_state = State.GameIn;

        m_gameTime = 0;
        m_gameCount = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.GameIn:
            UpdateGameIn();
            break;
        case State.Game:
            UpdateGame();
            break;
        case State.GameChanging:
            UpdateGameChanging();
            break;
        case State.GameOut:
            UpdateGameOut();
            break;
        case State.GameEnd:
            //UpdateGameEnd();
            break;
        }

        //タイマー表示.
        Number number = m_timerObj.GetComponent<Number>();
        float t = Mathf.Max(TIME_LIMIT - m_gameTime, 0);
        number.SetNum((int)t);
	}



    //ゲーム開始準備.
    void UpdateGameIn() {
        //ステージ構築.
        GameObject stage = GameObject.Find("Stage");
        if (stage == null) {
            stage = Instantiate(m_stagePrefabs[m_gameCount]) as GameObject;
            stage.name = "Stage";
            return;
        }

        //フェードインを待つ.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            if (b.IsFadeIn()) {
                return;
            }
        }

        //ゲーム開始へ遷移.
        m_state = State.Game;
        m_gameTime = 0;

        //発射できるようにする.
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        foreach (GameObject obj in bars) {
            BarScript bar = obj.GetComponent<BarScript>();
            bar.SetShotEnable(true);       //発射機能OFF.
        }
    }


    //ゲーム中.
    void UpdateGame() {
        //終了間際の演出に行ってもいいかの判定をする.
        m_gameTime += Time.fixedDeltaTime;
        bool isNext = false;

        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        if (blocks.Length == 0) {   //ブロックが全部なくなった.
            isNext = true;
        }
        if (m_gameTime > TIME_LIMIT) {
            isNext = true;
        }

        if (isNext) {
            //次の状態へ遷移.
            m_state = State.GameChanging;
        }
    }
    

    //ステージチェンジする演出.
    void UpdateGameChanging() {            
        //寿司フェードアウト開始.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            b.FadeOut();
        }

        //弾消去.
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject obj in balls) {
            Destroy(obj);
        }

        //発射できなくする.
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        foreach (GameObject obj in bars) {
            BarScript bar = obj.GetComponent<BarScript>();
            bar.SetShotEnable(false);       //発射機能OFF.
        }


        //次の状態へ遷移.
        m_state = State.GameOut;
    }


    //ゲーム終了準備.
    void UpdateGameOut() {
        //フェードアウト待ち.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            if (b.IsFadeOut()) {
                return;
            }
        }

        //ステージ消す.
        Destroy(GameObject.Find("Stage"));


        // 1ゲーム終了.
        ++m_gameCount;
        //Debug.Log("GameCount:" + m_gameCount);
        if (m_gameCount == GAMECOUNT_MAX) {
            m_state = State.GameEnd; // 既定のゲーム数に達したのでリザルトに遷移する.
        }
        else {
            m_state = State.GameIn; // 次のゲームに進みます.
        }
    }



    //ゲーム終了ならtrue.
    public bool IsEnd() {
        return (m_state == State.GameEnd);
    }

}
