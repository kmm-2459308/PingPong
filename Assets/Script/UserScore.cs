using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserScore : MonoBehaviour {
    public enum UserType {
        Player,     //自分.
        Opponent,   //対戦相手.
    }
    public UserType m_userType;
    public GameObject[] m_scoreSushiPrefabs; //スコア表示用の寿司をいれる.

    Dictionary<SushiType, int> m_fixedScore;   //獲得した寿司をカウントアップ(こちらは獲得済み扱い).
    List<GameObject> m_scoreSushi;  //獲得した寿司(こちらは獲得保留扱いなので増減する).
    const int FIXED_NUM = 7;        //一定数獲得したらfixedの方に移す.

	// Use this for initialization
	void Start () {
        m_fixedScore = new Dictionary<SushiType, int>();
	    m_scoreSushi = new List<GameObject>();
        
        //スコア初期化.
        foreach( SushiType s in Enum.GetValues(typeof(SushiType)) ){
            m_fixedScore[s] = 0;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate() {

        //FIXED_NUM個ごとに確定&表示リセットさせる.
        if (m_scoreSushi.Count >= FIXED_NUM) {
            if (m_scoreSushi[0].GetComponent<ScoreSushi>().IsFadeOut()) {
                //演出待ち-------------------------------------------
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    if (sushi.IsFadeOutEnd() == false) {
                        return;
                    }
                }

                //SE.
                GetComponent<AudioSource>().Play();
                //消化アニメーション終わりなので消す.
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    m_fixedScore[sushi.m_sushiType]++;
                    Destroy(m_scoreSushi[i]);
                }
                m_scoreSushi.RemoveRange(0, FIXED_NUM);

                //残っている表示物の位置合わせ.
                for (int i = 0; i < m_scoreSushi.Count; ++i) {
                    Vector3 pos = MakePosition(m_userType, i);
                    m_scoreSushi[i].transform.position = pos;
                }

                return;
            }
            else {
                //消化準備-------------------------------------------
                //消化アニメーションが終わってから削除してください.
                GameObject dog = GameObject.Find(m_userType.ToString() + "Dog");
                Vector3 target = dog.transform.Find("target").position;
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    sushi.StartFadeOut(target); //演出開始.
                }

            }
        }
	}



    //スコア追加.
    public void PushScore(SushiType sushiType) {
        Vector3 pos = MakePosition(m_userType, m_scoreSushi.Count);
        
        //スコア用の寿司を表示させる.
        GameObject obj = Instantiate(
            m_scoreSushiPrefabs[(int)sushiType],
            pos, Quaternion.identity * Quaternion.Euler(0,0,15)
        ) as GameObject;

        obj.transform.parent = transform;
        m_scoreSushi.Add(obj);
    }

    //スコア取り出し(削除).
    public void PopScore() {
        if (m_scoreSushi.Count == 0) {
            return; //消すものがない.
        }

        int last = m_scoreSushi.Count - 1;
        GameObject obj = m_scoreSushi[last];
        if (obj.GetComponent<ScoreSushi>().IsFadeOut()) {
            return; //フェードアウト中のものは消さない
        }

        Destroy(obj);
        m_scoreSushi.RemoveAt(last);
    }

    //スコア取得.
    public int GetCount(SushiType sushiType) {
        //獲得済みでないもの(表示分)をカウント.
        int count = 0;
        foreach(GameObject obj in m_scoreSushi){
            ScoreSushi sushi = obj.GetComponent<ScoreSushi>();
            SushiType type = sushi.m_sushiType;
            if(type == sushiType){
                count++;
            }
        }

        //獲得済みのものと足し合わせて返す.
        return m_fixedScore[sushiType] + count;
    }


    //スコア表示用寿司の座標を決める.
    //Playerなら右上に、Opponentなら左下に伸びていく.
    Vector3 MakePosition(UserType userType, int index) {
        Vector3 pos = transform.position;
        const float DISTANCE = 0.6f;
        switch (userType) {
        case UserType.Player:
            pos += Vector3.up * DISTANCE * index;
            pos -= Vector3.forward * -DISTANCE * index;
            return pos;
        case UserType.Opponent:
            pos -= Vector3.up * DISTANCE * index;
            pos -= Vector3.forward * DISTANCE * index;
            return pos;
        }
        return pos;
    }
}

