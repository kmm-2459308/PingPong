using UnityEngine;
using System.Collections;

/* 寿司の種類を定義 */
public enum SushiType {
    ebi,
    toro,
    tamago,
    ikura,
};

/* ブロック */
public class BlockScript : MonoBehaviour {
    enum State {
        FadeIn,
        Wait,
        FadeOut,
        End,
    };
    State m_state;
    float m_timer;
    const float FADEIN_TIME = 0.3f;
    const float FADEOUT_TIME = 0.3f;

	int m_life = 2;  //[0:2]の範囲.

    public SushiType m_sushiType;
    private GameObject m_sushiModel;

    void Awake() {
        m_state = State.FadeIn;
        m_timer = 0;
        transform.localScale = Vector3.zero;

        m_sushiModel = transform.Find("sushi").gameObject;
    }


    void FixedUpdate() {
        switch (m_state) {
        case State.FadeIn:
            m_timer += Time.fixedDeltaTime;
            {   //徐々に拡大.
                float rate = Mathf.Min(m_timer / FADEIN_TIME, 1.0f);
                transform.localScale = Vector3.one * rate;
            }

            if (m_timer >= FADEIN_TIME) {
                m_state = State.Wait;
            }
            break;

        case State.Wait:
            break;

        case State.FadeOut:
            m_timer += Time.fixedDeltaTime;
            {   //徐々に縮小.
                float rate = Mathf.Min(m_timer / FADEOUT_TIME, 1.0f);
                transform.localScale = Vector3.one * (1.0f - rate);
            }

            if (m_timer >= FADEOUT_TIME) {
                m_state = State.End;
            }
            break;

        case State.End:
            break;

        }
    }


    //ヒット処理.
	void OnCollisionEnter2D(Collision2D col)
	{
        Sushi sushi = m_sushiModel.GetComponent<Sushi>();

		PlayerInfo info = PlayerInfo.GetInstance();
		int ballId = col.gameObject.GetComponent<BallScript>().GetPlayerId();

        m_life--;
        if (m_life <= 0) {
            // スコア加算.
            if (ballId == info.GetPlayerId()) {
                //自分のスコア加算する.
                GameObject score = GameObject.Find("PlayerScore");
                score.GetComponent<UserScore>().PushScore(m_sushiType);
            }
            else {
                //相手プレーヤーのスコア加算する.
                GameObject score = GameObject.Find("OpponentScore");
                score.GetComponent<UserScore>().PushScore(m_sushiType);
            }
            
            // 自分自身を削除.
            Destroy(gameObject);
        }

        //寿司モデルをアニメーションさせる.
        if (m_life == 1) {
            sushi.PlayAnimation(Sushi.AnimationType.jump);
        }
	}


    
    //フェードイン/アウトアニメーション用.
    public bool IsFadeIn() {
        return (m_state == State.FadeIn);
    }
    public bool IsFadeOut() {
        return (m_state == State.FadeOut);
    }
    public void FadeOut() {
        m_state = State.FadeOut;
        m_timer = 0;
    }
    
}
