using UnityEngine;
using System;
using System.Collections;

public class BallScript : MonoBehaviour {
    public GameObject m_hitEffectPrefab; //ヒット時のエフェクト.
    int m_playerId;
	//
	float m_velocity;
	Vector2 m_direction;

	//
	bool m_isMissed = false;
    const float DEADLINE = 6.0f;

	// Use this for initialization
	void Start()
	{
        m_velocity = 4.0f;
        m_direction = new Vector2(1.0f, 1.0f).normalized;
        if (m_playerId == 1) {
            m_direction *= -1.0f;
		}
        GetComponent<Rigidbody2D>().velocity = m_velocity * m_direction.normalized;
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		CheckMissBall();
        if (IsMissed()) {
            Destroy(gameObject);
        }
	}

	//
	public void SetPlayerId(int id)
	{
        m_playerId = id;

        Transform model = transform.Find("sara");
        
        if (id == 0) {
            model.GetComponent<Renderer>().material.color = Color.blue * 0.3f + Color.white * 0.7f;
        }
        else {
            model.GetComponent<Renderer>().material.color = Color.red * 0.3f + Color.white * 0.7f;
        }
	}

	public int GetPlayerId()
	{
        return m_playerId;
	}

	//
	public bool IsMissed()
	{
        return m_isMissed;
	}

	//
	void OnCollisionEnter2D(Collision2D col)
	{
        //エフェクト生成.  //そのままだと地面にめり込むのでカメラ方向に位置を寄せてます.
        Vector3 effectPos = transform.position - 3 * Camera.main.transform.forward;
        Instantiate(m_hitEffectPrefab, effectPos, transform.rotation);
        //SE
        GetComponent<AudioSource>().Play();


        if (col.gameObject.tag == "Bar") {
            BarScript bar = col.gameObject.GetComponent<BarScript>();
            SetPlayerId( bar.GetBarId() );
            m_velocity += 0.3f;
        }

       
		if (col.gameObject.tag == "Ball") {
			// ボール同士の衝突は逆方向に跳ね返るようにします.
            m_direction *= -1.0f;
		} else {
            // ボール以外の衝突は衝突面の法線のむきに反転します.
            float range = Mathf.Sin( Mathf.Deg2Rad * 3 ); //3度程度は許容する.

            Vector2 normal = col.contacts[0].normal;
            if (normal.x > range || normal.x < -range) {
                m_direction.x *= -1.0f;
            }
            if (normal.y > range || normal.y < -range) {
                m_direction.y *= -1.0f;
            }
		}

		// 反転した方向に現在の速度を合わせて設定します.
        GetComponent<Rigidbody2D>().velocity = m_velocity * m_direction;
    }


    //ミスチェック.
	void CheckMissBall()
	{
		Vector3 pos = transform.position;

		if (pos.y > DEADLINE || pos.y < -DEADLINE) {
            m_isMissed = true;    // ミスした.
		}
        else {
            return;
        }

        //どっちのプレイヤーがミスしたのか、idを決める.
        int missPlayerId = 0;
        if (pos.y > DEADLINE) {
            missPlayerId = 1;
        }
        
        PlayerInfo info = PlayerInfo.GetInstance();
        GameObject scoreObj;
        if (missPlayerId == info.GetPlayerId()) {
            scoreObj = GameObject.Find("PlayerScore");  //自分のスコアを減らしたい.            
        }
        else {            
            scoreObj = GameObject.Find("OpponentScore"); //相手プレーヤーのスコアを減らしたい.
        }
        //点数減らす.
        UserScore score = scoreObj.GetComponent<UserScore>();
        score.PopScore();
        if (missPlayerId != m_playerId) { //相手のボールでミス.
            score.PopScore();
            score.PopScore();
        }
            
	}
}
