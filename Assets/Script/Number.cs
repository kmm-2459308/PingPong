/*
 * ■ Number の役割

3桁の数字を画面に表示するクラス。

例

  0
  5
  25
  123
  999

などを表示できる。

このクラス自身は数字の計算をせず、

AsciiCharacter クラスに

「何の数字を表示するか」

を指示している。



■ 変数の役割

GameObject[] m_asciiObj;

数字表示用オブジェクトを保存する配列。

3桁分保持する。

配列の内容

m_asciiObj[0] → 1の位
m_asciiObj[1] → 10の位
m_asciiObj[2] → 100の位



■ Start()

ゲーム開始時に1回呼ばれる。


----------------------------------
配列作成
----------------------------------

m_asciiObj = new GameObject[3];

3桁分の配列を作成。



----------------------------------
子オブジェクト取得
----------------------------------

string[] names =
{
    "num1",
    "num2",
    "num3"
};

ループで取得。

for (int i = 0; i < 3; ++i)
{
    Transform num =
        transform.Find(names[i]);

    m_asciiObj[i] =
        num.gameObject;
}

取得対象

num1
num2
num3

Hierarchy例

Number
 ├─ num1
 ├─ num2
 └─ num3

これらを配列へ保存する。



■ SetNum()

数字表示を更新する関数。

public void SetNum(int num)

引数

num

表示したい数字。



例

SetNum(123);

なら

123

を表示する。



■ 処理の流れ

int div = 1;

桁を取り出すための値。

最初は

1

つまり1の位。



----------------------------------
1回目のループ
----------------------------------

int n = (num / div) % 10;

例

num = 123

div = 1

計算

123 / 1 = 123

123 % 10 = 3

結果

n = 3

1の位を取得。



----------------------------------
数字表示
----------------------------------

AsciiCharacter ac =
    m_asciiObj[i]
    .GetComponent<AsciiCharacter>();

取得した数字表示オブジェクト。


ac.SetNumber(n);

数字を表示。


例

SetNumber(3)

→ 「3」を表示。



----------------------------------
次の桁へ
----------------------------------

div *= 10;

1
↓
10
↓
100

となる。



■ 例①

SetNum(123)

1回目

123 / 1 % 10

= 3

num1 → 3


2回目

123 / 10 % 10

= 2

num2 → 2


3回目

123 / 100 % 10

= 1

num3 → 1


結果

num3 = 1
num2 = 2
num1 = 3

画面表示

123



■ 例②

SetNum(5)

1回目

5 / 1 % 10

= 5

num1 → 5


2回目

5 / 10 % 10

= 0

num2 → 0


3回目

5 / 100 % 10

= 0

num3 → 0


結果

005

または

表示側の設定によっては

5

として見える。



■ 例③

SetNum(87)

1回目

87 / 1 % 10

= 7

num1 → 7


2回目

87 / 10 % 10

= 8

num2 → 8


3回目

87 / 100 % 10

= 0

num3 → 0


結果

087

または

87



■ 使用例

Number number =
    GetComponent<Number>();

number.SetNum(150);

画面表示

150



タイマー表示

number.SetNum(30);

画面表示

30



スコア表示

number.SetNum(245);

画面表示

245



■ このクラスの特徴

・最大3桁まで表示可能
・各桁を個別のオブジェクトで管理
・数字の描画は AsciiCharacter に任せる
・スコアやタイマー表示に利用できる



■ 処理のイメージ

SetNum(456)
      ↓

456 % 10
↓
6

45 % 10
↓
5

4 % 10
↓
4

結果

num3 → 4
num2 → 5
num1 → 6

画面表示

456
 * 
 */

using UnityEngine;
using System.Collections;
using System;

/** 3桁の数字を表示 */
public class Number : MonoBehaviour {
    GameObject[] m_asciiObj;


	// Use this for initialization
	void Start () {
        m_asciiObj = new GameObject[3];

        string[] names = { "num1", "num2", "num3" };
        for (int i = 0; i < 3; ++i) {
            Transform num = transform.Find(names[i]);
            m_asciiObj[i] = num.gameObject;
        }
	}
	
	
    
    public void SetNum(int num){
        int div = 1;
        for (int i = 0; i < m_asciiObj.Length; ++i) {
            int n = (num / div) % 10;

            AsciiCharacter ac = m_asciiObj[i].GetComponent<AsciiCharacter>();
            if (ac) {
                ac.SetNumber(n);
            }
            
            div *= 10;
        }
    }
}
