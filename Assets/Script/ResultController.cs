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
