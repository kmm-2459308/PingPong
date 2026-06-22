// リザルト表現
/*
 ■ ResultController の役割

リザルト画面（結果発表画面）の演出を管理するクラス。

主な流れ

1. リザルト背景表示
2. 寿司ごとの得点表示
3. 合計得点表示
4. 勝敗表示（WIN / LOSE）
5. リザルト終了



■ 変数の役割

public GameObject m_winPrefab;

勝利表示（WIN）のプレハブ。


public GameObject m_losePrefab;

敗北表示（LOSE）のプレハブ。


GameObject m_winlose;

現在表示中の WIN または LOSE オブジェクト。


GameObject m_playerScore;
GameObject m_opponentScore;

プレイヤーと相手のスコア情報を持つオブジェクト。


GameObject m_resultback;

リザルト背景。


GameObject m_resultPlayer;
GameObject m_resultOpponent;

合計得点表示用オブジェクト。


GameObject[] m_playerIcons;
GameObject[] m_opponentIcons;

寿司ごとのスコア表示アイコン。

配列の中身

[0] tamago
[1] ebi
[2] ikura
[3] toro


int m_resultAnimationIndex;

現在何番目の寿司スコアを表示中か管理。



■ State列挙型

enum State
{
    In,
    ScoreWait,
    TotalScore,
    WinLose,
    End
}

リザルト画面の状態管理。


In

リザルト画面入場。


ScoreWait

寿司ごとのスコア表示中。


TotalScore

合計得点表示中。


WinLose

勝敗表示中。


End

リザルト終了。



■ Start()

開始時に呼ばれる。

m_state = State.In;

最初は入場状態。


GameObject.Find()

で必要なオブジェクトを取得。

・resultback
・result_player
・result_opponent
・PlayerScore
・OpponentScore


寿司アイコン取得

string[] names =
{
    "tamago",
    "ebi",
    "ikura",
    "toro"
};

ループで

tamago_player
tamago_opponent

などを取得して配列へ保存。



■ プレイヤー位置調整

PlayerInfo playerInfo =
    PlayerInfo.GetInstance();

if(playerInfo.GetPlayerId() != 0)

クライアント側なら

server_icon
client_icon

の位置を入れ替える。


最初はアイコン非表示

enabled = false;



■ FixedUpdate()

状態ごとに処理を実行。

switch(m_state)



==================================
State.In
==================================

リザルト背景のフェードイン待ち。

if(animation.isPlaying == false)

アニメーション終了。


サーバー・クライアントアイコン表示。

enabled = true


カウントアップSE再生。

Play()


次状態へ。

m_state = State.ScoreWait



==================================
State.ScoreWait
==================================

UpdateScoreWait()

を実行。


最後の寿司スコア表示が終了したら

prs.IsEnd()
ors.IsEnd()

両方 true。


合計得点を計算して表示。

m_resultPlayer.GetComponent<Number>()
    .SetNum(...);

m_resultOpponent.GetComponent<Number>()
    .SetNum(...);


合計得点アニメーション開始。

Play("ResultScore")


SE再生。

PlayDelayed(0.75f)


カウントアップSE停止。

Stop()


次状態へ。

m_state = State.TotalScore



==================================
State.TotalScore
==================================

合計得点アニメーション終了待ち。

if(pAnim.isPlaying == false &&
   oAnim.isPlaying == false)

終了したら

m_state = State.WinLose



==================================
State.WinLose
==================================

まだWIN/LOSEが生成されていない場合

if(m_winlose == null)

勝敗判定。


プレイヤー得点 < 相手得点

なら

Instantiate(m_losePrefab)

負け。


それ以外

Instantiate(m_winPrefab)

勝ち。


生成したオブジェクト名

winlose


アニメーション終了待ち。

if(animation.isPlaying == false)

表示終了なら

Destroy(m_winlose)

削除。


次状態へ。

m_state = State.End



==================================
State.End
==================================

リザルト終了。



■ UpdateScoreWait()

寿司ごとの得点演出を順番に表示する。


----------------------------------
1回目
----------------------------------

m_resultAnimationIndex == 0

なら

たまご寿司の表示開始。


取得数

GetCount(SushiType.tamago)

得点

個数 × 8


FadeIn()

で表示開始。


表示開始後

m_resultAnimationIndex = 1



----------------------------------
2回目以降
----------------------------------

前の寿司の表示終了待ち。

if(prs.IsEnd() && ors.IsEnd())

終了したら次の寿司へ。


寿司種類

tamago
ebi
ikura
toro


点数

8点
10点
12点
15点


例

えびを3個

3 × 10

= 30点


FadeIn()

で表示開始。


次のインデックスへ。

m_resultAnimationIndex++



■ GetResultScore()

合計得点計算。


寿司ごとの点数

tamago = 8点
ebi    = 10点
ikura  = 12点
toro   = 15点


例

tamago 2個 = 16点
ebi    3個 = 30点
ikura  1個 = 12点
toro   2個 = 30点

合計

16 + 30 + 12 + 30

= 88点


return result;

で返す。



■ IsEnd()

public bool IsEnd()
{
    return (m_state == State.End);
}

リザルト終了判定。


他スクリプトから

if(resultController.IsEnd())
{
    // タイトルへ戻る
}

のように使用できる。



■ リザルト全体の流れ

In
↓
ScoreWait
↓
TotalScore
↓
WinLose
↓
End


画面表示の流れ

背景表示
↓
寿司ごとの得点表示
↓
合計得点表示
↓
WIN / LOSE表示
↓
終了
 */


using UnityEngine;
using System.Collections;

public class ResultController : MonoBehaviour {
    public GameObject m_winPrefab;  //[勝ち]の表示.
    public GameObject m_losePrefab; //[負け]の表示.
    GameObject m_winlose;

    GameObject m_playerScore;
    GameObject m_opponentScore;

    
    //表示物を捕まえておく.
    GameObject m_resultback;
    GameObject m_resultPlayer;
    GameObject m_resultOpponent;
    
    GameObject[] m_playerIcons;    //寿司アイコンとスコア.
    GameObject[] m_opponentIcons;  //寿司アイコンとスコア.
    int m_resultAnimationIndex; //表示物のアニメーション管理.

    enum State {
        In,         //入場.
        ScoreWait,  //スコアアニメーション待ち.
        TotalScore, //合計スコア表示.
        WinLose,    //勝ち負け出す.
        End,        //終わり.
    }
    State m_state;


	// Use this for initialization
	void Start () {
        m_state = State.In;
        m_resultback = GameObject.Find("resultback");
        m_resultPlayer = GameObject.Find("result_player");
        m_resultOpponent = GameObject.Find("result_opponent");

        m_playerScore = GameObject.Find("PlayerScore");
        m_opponentScore = GameObject.Find("OpponentScore");

        //表示物を捕まえておく.
        m_playerIcons = new GameObject[4];
        m_opponentIcons = new GameObject[4];
        string[] names = { "tamago", "ebi", "ikura", "toro" };
        for (int i = 0; i < names.Length; ++i) {
            string name = names[i];
            m_playerIcons[i] = transform.Find(name + "_player").gameObject;
            m_opponentIcons[i] = transform.Find(name + "_opponent").gameObject;
        }

        //イナリ・かっぱ巻きのアイコン.
        GameObject serverIcon = GameObject.Find("server_icon");
        GameObject clientIcon = GameObject.Find("client_icon");
        PlayerInfo playerInfo = PlayerInfo.GetInstance();
        if (playerInfo.GetPlayerId() != 0) {
            //クライアント起動の場合は、クライアントのアイコンを左側に表示させる.
            Vector3 pos = serverIcon.transform.position;
            serverIcon.transform.position = clientIcon.transform.position;
            clientIcon.transform.position = pos;
        }
        serverIcon.GetComponent<SpriteRenderer>().enabled = false; //最初は表示を切っておく.
        clientIcon.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.In:
            //背景のフェードイン.
            if (m_resultback.GetComponent<Animation>().isPlaying == false) {
                //イナリ・かっぱ巻きのアイコンの表示をONにする.
                GameObject.Find("server_icon").GetComponent<SpriteRenderer>().enabled = true;
                GameObject.Find("client_icon").GetComponent<SpriteRenderer>().enabled = true;

                //SE カウントアップ音再生.
                GetComponent<AudioSource>().Play();

                m_state = State.ScoreWait;
            }
            break;

        case State.ScoreWait:
            UpdateScoreWait();  //スコア表示.

            ResultScore prs = m_playerIcons[3].GetComponent<ResultScore>();
            ResultScore ors = m_opponentIcons[3].GetComponent<ResultScore>();
            if (prs.IsEnd() && ors.IsEnd()) {
                //表示終わりで合計得点を出す.
                m_resultPlayer.GetComponent<Number>().SetNum( GetResultScore(m_playerScore) );
                m_resultOpponent.GetComponent<Number>().SetNum( GetResultScore(m_opponentScore) );
                m_resultPlayer.GetComponent<Animation>().Play("ResultScore");
                m_resultOpponent.GetComponent<Animation>().Play("ResultScore");
                //SE.
                m_resultPlayer.GetComponent<AudioSource>().PlayDelayed(0.75f);
                GetComponent<AudioSource>().Stop(); //カウントアップ音は停止.

                m_state = State.TotalScore;
            }
            break;

        case State.TotalScore:
            //合計得点の表示待ち.
            Animation pAnim = m_resultPlayer.GetComponent<Animation>();
            Animation oAnim = m_resultOpponent.GetComponent<Animation>();
            if (pAnim.isPlaying == false && oAnim.isPlaying == false) {
                m_state = State.WinLose;
            }
            break;

        case State.WinLose:
            if (m_winlose == null) {
                //win/loseの表示開始.
                if (GetResultScore(m_playerScore) < GetResultScore(m_opponentScore)) {
                    m_winlose = Instantiate(m_losePrefab) as GameObject;  //負け.
                }
                else {
                    m_winlose = Instantiate(m_winPrefab) as GameObject;   //勝ち.
                }
                m_winlose.name = "winlose";
                return;
            }

            if (m_winlose.GetComponent<Animation>().isPlaying == false) {
                Destroy(m_winlose);
                m_state = State.End;
            }
            break;

        case State.End:
            break;
        }
    }

    
    //スコア表示中.
    void UpdateScoreWait(){
        if (m_resultAnimationIndex >= m_playerIcons.Length) {
            return;
        }
        if (m_resultAnimationIndex == 0) {
            //表示開始.
            int pCount = m_playerScore.GetComponent<UserScore>().GetCount(SushiType.tamago);
            int oCount = m_opponentScore.GetComponent<UserScore>().GetCount(SushiType.tamago);
            m_playerIcons[0].GetComponent<ResultScore>().FadeIn(pCount, pCount * 8);
            m_opponentIcons[0].GetComponent<ResultScore>().FadeIn(oCount, oCount * 8);
            m_resultAnimationIndex = 1;
            
            return;
        }


	    //スコア表示する.
        ResultScore prs = m_playerIcons[m_resultAnimationIndex - 1].GetComponent<ResultScore>();
        ResultScore ors = m_opponentIcons[m_resultAnimationIndex - 1].GetComponent<ResultScore>();
        
        //アニメーションが終わったら次のアニメーションを再生.
        if(prs.IsEnd() && ors.IsEnd()){
            if (m_resultAnimationIndex >= m_playerIcons.Length) {
                return;
            }

            SushiType[] typeList = { SushiType.tamago, SushiType.ebi, SushiType.ikura, SushiType.toro };
            int[] pointList = { 8, 10, 12, 15 };  //寿司タイプ毎の得点定義.

            SushiType type = typeList[m_resultAnimationIndex];
            int point = pointList[m_resultAnimationIndex];
            int pCount = m_playerScore.GetComponent<UserScore>().GetCount(type);
            int oCount = m_opponentScore.GetComponent<UserScore>().GetCount(type);

            //得点表示スタート.
            m_playerIcons[m_resultAnimationIndex].GetComponent<ResultScore>().FadeIn(pCount, pCount * point);
            m_opponentIcons[m_resultAnimationIndex].GetComponent<ResultScore>().FadeIn(oCount, oCount * point);

            m_resultAnimationIndex++;
        }
	}


    //リザルト終了ならtrue.
    public bool IsEnd() {
        return (m_state == State.End);
    }


    //合計得点の計算.
    int GetResultScore(GameObject userScore) {
        SushiType[] typeList = { SushiType.tamago, SushiType.ebi, SushiType.ikura, SushiType.toro };
        int[] pointList = { 8, 10, 12, 15 };  //寿司タイプ毎の得点定義.

        int result = 0;
        for (int i = 0; i < 4; ++i) {
            SushiType type = typeList[i];
            int point = pointList[i];
            int count = userScore.GetComponent<UserScore>().GetCount(type);

            result += count * point;
        }

        return result;
    }
    
}
