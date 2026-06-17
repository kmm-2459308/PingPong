#if true

using UnityEngine;
using System.Collections;

/// <summary>
/// マウス入力1フレーム分のデータ
/// ネットワーク送受信用のデータ構造体
/// </summary>
public struct MouseData
{
    // 入力取得時のフレーム番号
    //・・・P.164
    public int frame;
    public bool mouseButtonLeft;
    public bool mouseButtonRight;

    public float mousePositionX;
    public float mousePositionY;
    public float mousePositionZ;

    // デバッグ表示用
    public override string ToString()
    {
        string str = "";

        str += "frame:" + frame;
        str += " mouseButtonLeft:" + mouseButtonLeft;
        str += " mouseButtonRight:" + mouseButtonRight;
        str += " mousePositionX:" + mousePositionX;
        str += " mousePositionY:" + mousePositionY;
        str += " mousePositionZ:" + mousePositionZ;

        return str;
    }

};

/// <summary>
/// 入力データ群
/// （複数フレーム分をまとめて送る場合などに利用）
/// </summary>
public struct InputData
{
    //・・・P.167
    public int count;
    public int flag;
    public MouseData[] datum;
};

/// <summary>
/// 入力管理クラス
/// ローカル入力の取得と同期済み入力の管理を行う
/// </summary>
public class InputManager : MonoBehaviour {

    private static int playerNum = 2;

    // プレイヤーごとの同期済み入力データ
    // [0] サーバ
    // [1] クライアント
    MouseData[] m_syncedInputs = new MouseData[playerNum]; //同期済みの入力値.

    //現在の入力値(これを送信させる).
    MouseData m_localInput;


    /// <summary>
    /// FixedUpdate
    /// 物理演算更新タイミングで入力を取得
    /// （同期処理向け）
    /// </summary>
    void FixedUpdate() {
        //Debug.Log(gameObject.name + Time.frameCount.ToString() + " scale:" + Time.timeScale.ToString());

        // 現在のマウスボタン状態を取得

        // 左クリック中か
        m_localInput.mouseButtonLeft = Input.GetMouseButton(0);

        // 右クリック中か
        m_localInput.mouseButtonRight = Input.GetMouseButton(1);


        // スクリーン座標取得
        Vector3 pos = Input.mousePosition;
        // カメラからマウス位置へ向かうRayを生成
        Ray ray = Camera.main.ScreenPointToRay(pos);

        // y=0 の平面を作成（XZ平面）
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float depth;
        // Rayと平面の交点距離を取得

        plane.Raycast(ray, out depth);

        // Ray上の交点座標を計算
        Vector3 worldPos = ray.origin + ray.direction * depth;

        // ワールド座標を保存
        m_localInput.mousePositionX = worldPos.x;
        m_localInput.mousePositionY = worldPos.y;
        m_localInput.mousePositionZ = worldPos.z;
    }

    /// <summary>
    /// 現在のローカル入力を取得
    /// 主に送信用
    /// </summary>
    public MouseData GetLocalMouseData() {
        return m_localInput;
    }


    /// <summary>
    /// 同期済み入力データ取得
    /// id:
    /// 0 = サーバ
    /// 1 = クライアント
    /// </summary>
    public MouseData GetMouseData(int id) {
        //		Debug.Log("id:" + id + "' " + inputData.Length);
        return m_syncedInputs[id];
    }

    /// <summary>
    /// 同期済み入力データセット用
    /// ネットワーク受信後に呼び出す
    /// </summary>
    public void SetInputData(int id, MouseData data) {
        m_syncedInputs[id] = data;
    }
}
#else

using UnityEngine;
using System.Collections;

/// <summary>
/// マウス入力1フレーム分のデータ
/// ネットワーク送受信用のデータ構造体
/// </summary>
public struct MouseData
{
    // 入力取得時のフレーム番号
    public int frame;

    // 左クリック状態
    public bool mouseButtonLeft;

    // 右クリック状態
    public bool mouseButtonRight;

    // マウス座標（ワールド座標）
    public float mousePositionX;
    public float mousePositionY;
    public float mousePositionZ;

    // デバッグ表示用
    public override string ToString()
    {
        string str = "";

        str += "frame:" + frame;
        str += " mouseButtonLeft:" + mouseButtonLeft;
        str += " mouseButtonRight:" + mouseButtonRight;
        str += " mousePositionX:" + mousePositionX;
        str += " mousePositionY:" + mousePositionY;
        str += " mousePositionZ:" + mousePositionZ;

        return str;
    }

};

/// <summary>
/// 入力データ群
/// （複数フレーム分をまとめて送る場合などに利用）
/// </summary>
public struct InputData
{   
	public int 			count;		// データ数.
	public int			flag;		// 各種フラグ.
	public MouseData[] 	datum;		// キー入力情報.
};

/// <summary>
/// 入力管理クラス
/// ローカル入力の取得と同期済み入力の管理を行う
/// </summary>
public class InputManager : MonoBehaviour {

    private static int playerNum = 2;

    // プレイヤーごとの同期済み入力データ
    // [0] サーバ
    // [1] クライアント
    MouseData[] m_syncedInputs = new MouseData[playerNum]; //同期済みの入力値.

    //現在の入力値(これを送信させる).
    MouseData m_localInput;


    /// <summary>
    /// FixedUpdate
    /// 物理演算更新タイミングで入力を取得
    /// （同期処理向け）
    /// </summary>
    void FixedUpdate() {
        //Debug.Log(gameObject.name + Time.frameCount.ToString() + " scale:" + Time.timeScale.ToString());

        // 現在のマウスボタン状態を取得

        // 左クリック中か
        m_localInput.mouseButtonLeft = Input.GetMouseButton(0);

        // 右クリック中か
        m_localInput.mouseButtonRight = Input.GetMouseButton(1);


        // スクリーン座標取得
        Vector3 pos = Input.mousePosition;
        // カメラからマウス位置へ向かうRayを生成
        Ray ray = Camera.main.ScreenPointToRay(pos);

        // y=0 の平面を作成（XZ平面）
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float depth;
        // Rayと平面の交点距離を取得

        plane.Raycast(ray, out depth);

        // Ray上の交点座標を計算
        Vector3 worldPos = ray.origin + ray.direction * depth;

        // ワールド座標を保存
        m_localInput.mousePositionX = worldPos.x;
        m_localInput.mousePositionY = worldPos.y;
        m_localInput.mousePositionZ = worldPos.z;
    }

    /// <summary>
    /// 現在のローカル入力を取得
    /// 主に送信用
    /// </summary>
    public MouseData GetLocalMouseData() {
        return m_localInput;
    }


    /// <summary>
    /// 同期済み入力データ取得
    /// id:
    /// 0 = サーバ
    /// 1 = クライアント
    /// </summary>
    public MouseData GetMouseData(int id) {
        //		Debug.Log("id:" + id + "' " + inputData.Length);
        return m_syncedInputs[id];
    }

    /// <summary>
    /// 同期済み入力データセット用
    /// ネットワーク受信後に呼び出す
    /// </summary>
    public void SetInputData(int id, MouseData data) {
        m_syncedInputs[id] = data;
    }
}

#endif
