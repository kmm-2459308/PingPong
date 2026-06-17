using UnityEngine;
using System.Collections;

public class BarScript : MonoBehaviour {
    public GameObject m_ballPrefab; //発射できるボール.
    float m_shotTime;           //発射した時間.
    bool m_shotEnable;          //trueなら弾を撃ってok.
    int m_id = 0;               //サーバー/クライアントの判定用.

	// Use this for initialization
	void Start()
	{
        m_shotTime = 1.0f;
        m_shotEnable = false;
	}


	// Update is called once per frame
	void FixedUpdate () {
        // IDに対応した入力値を得る.
		GameObject manager = GameObject.Find("InputManager");
        MouseData data = manager.GetComponent<InputManager>().GetMouseData(m_id);

        // 移動させる.
        Vector3 pos = transform.position;
        if (data.mouseButtonLeft) {
            //ドラッグ中は移動できる.
            pos.x = data.mousePositionX;
        }


		// 両脇の壁の間だけ移動するように制限する.
		if (pos.x < -3.5f) {
			pos.x = -3.5f;
		} else if (pos.x > 3.5f) {
			pos.x = 3.5f;
		}

		// 移動後の位置を再設定する.
		transform.position = pos;

        //ボタン押しでボール発射.
        m_shotTime += Time.fixedDeltaTime;
        if(m_shotEnable && data.mouseButtonRight){
            if(m_shotTime > 1.0f){
                GameObject ball = Instantiate(m_ballPrefab, transform.position + transform.up*0.8f, transform.rotation) as GameObject;
                ball.GetComponent<BallScript>().SetPlayerId(m_id);
                m_shotTime = 0;
            }
        }
	}
	
	//
    public int GetBarId() {
        return m_id;
    }
    public void SetBarId(int id) {
        m_id = id;
    }

    //弾の発射ができるかどうかを調整.
    public void SetShotEnable(bool enable){
        m_shotEnable = enable;
    }

}
