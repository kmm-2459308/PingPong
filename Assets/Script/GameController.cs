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
