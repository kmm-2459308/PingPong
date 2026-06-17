// マウスの入力および送信データのシリアライザー
//
// ■プログラムの説明
// Serialize クラスを継承してマウスの入力データをシリアライズします.
// 受信した対戦相手のバイナリーデータをマウスの入力データにデシリアライズします.
// Serialize() 関数にパケットの構造をバイナリデータにシリアライズする処理を記述しています.
// Deserialize() 関数にバイナリデータをパケットの構造にシリアライズする処理を記述しています.
//

#if true

using UnityEngine;
using System.Collections;


// マウスの入力データのシリアライザー.
public class MouseSerializer : Serializer
{

	// 各要素を順番にシリアライズします
	//・・・P.158
	public bool Serialize(MouseData packet)
	{
		bool ret = true;
		ret &= Serialize(packet.frame);
		ret &= Serialize(packet.mouseButtonLeft);
		ret &= Serialize(packet.mouseButtonRight);
		ret &= Serialize(packet.mousePositionX);
		ret &= Serialize(packet.mousePositionY);
		ret &= Serialize(packet.mousePositionZ);
		return ret;
	}
    // データの要素ごとにデシリアライズします.
    //・・・P.159
	public bool Deserialize(byte[] data,ref MouseData serialized)
	{
        bool ret = SetDeserializedData(data);
        if (ret == false)
        {
            return false;
        }

        ret &= Deserialize(ref serialized.frame);
        ret &= Deserialize(ref serialized.mouseButtonLeft);
        ret &= Deserialize(ref serialized.mouseButtonRight);
        ret &= Deserialize(ref serialized.mousePositionX);
        ret &= Deserialize(ref serialized.mousePositionY);
        ret &= Deserialize(ref serialized.mousePositionZ);
        return ret;
    }

}


// 複数の入力データをまとめた1回分の送信データのシリアライザー.
public class InputSerializer : Serializer
{
	public bool Serialize(InputData data)
	{
		//・・・P.173
		Clear();
		bool ret = true;
		ret &= Serialize(data.count);
		ret &= Serialize(data.flag);
		MouseSerializer mouse = new MouseSerializer();
		for(int i = 0;i<data.datum.Length;++i)
		{
			mouse.Clear();

			bool ans = mouse.Serialize(data.datum[i]);
			if(ans == false)
			{
				return false;
			}
			byte[] buffer = mouse.GetSerializedData();
			ret &= Serialize(buffer, buffer.Length);
		}

		return ret;
	}

	public bool Deserialize(byte[] data, ref InputData serialized)
	{
		// デシリアライズするデータを設定します.
		//・・・ P.175
		bool ret = SetDeserializedData(data);
		if(ret == false)
		{
			return false;
		}
		ret &= Deserialize(ref serialized.count);
		ret &= Deserialize(ref serialized.flag);
		MouseSerializer mouse = new MouseSerializer();
		MouseData md = new MouseData();
		mouse.Serialize(md);
		byte[] buf = mouse.GetSerializedData();
		int size = buf.Length;

		serialized.datum = new MouseData[serialized.count];
		for(int i = 0;i<serialized.count;++i)
		{
			serialized.datum[i] = new MouseData();
		}
        for (int i = 0; i < serialized.count; ++i)
        {
			byte[] buffer = new byte[size];
			bool ans = Deserialize(ref buffer, size);
        }








        return ret;
	}
}

#else

using UnityEngine;
using System.Collections;


// マウスの入力データのシリアライザー.
public class MouseSerializer : Serializer
{

	// 各要素を順番にシリアライズします
	public bool Serialize(MouseData packet)
	{
		// 各要素を順番にシリアライズします.
		bool ret = true;
		ret &= Serialize(packet.frame);	
		ret &= Serialize(packet.mouseButtonLeft);
		ret &= Serialize(packet.mouseButtonRight);
		ret &= Serialize(packet.mousePositionX);
		ret &= Serialize(packet.mousePositionY);
		ret &= Serialize(packet.mousePositionZ);
		
		return ret;
	}

	public bool Deserialize(byte[] data, ref MouseData serialized)
	{
		// データの要素ごとにデシリアライズします.
		// デシリアライズするデータを設定します.
		bool ret = SetDeserializedData(data);
		if (ret == false) {
			return false;
		}
		
		// データの要素ごとにデシリアライズします.
		ret &= Deserialize(ref serialized.frame);
		ret &= Deserialize(ref serialized.mouseButtonLeft);
		ret &= Deserialize(ref serialized.mouseButtonRight);
		ret &= Deserialize(ref serialized.mousePositionX);
		ret &= Deserialize(ref serialized.mousePositionY);
		ret &= Deserialize(ref serialized.mousePositionZ);
		
		return ret;
	}
}


// 複数の入力データをまとめた1回分の送信データのシリアライザー.
public class InputSerializer : Serializer
{
	public bool Serialize(InputData data)
	{
		// 既存のデータをクリアします.
		Clear();
		
		// 各要素を順番にシリアライズします.
		bool ret = true;
		ret &= Serialize(data.count);	
		ret &= Serialize(data.flag);

		MouseSerializer mouse = new MouseSerializer();
		
		for (int i = 0; i < data.datum.Length; ++i) {

			mouse.Clear();

			bool ans = mouse.Serialize(data.datum[i]);
			if (ans == false) {
				return false;
			}

            // 生成されたバイナリを取り出して本体バッファへ追加
            byte[] buffer = mouse.GetSerializedData();
			ret &= Serialize(buffer, buffer.Length);
		}
		
		return ret;
	}

	public bool Deserialize(byte[] data, ref InputData serialized)
	{
		// デシリアライズするデータを設定します.
		bool ret = SetDeserializedData(data);
		if (ret == false) {
			return false;
		}
		
		// データの要素ごとにデシリアライズします.
		ret &= Deserialize(ref serialized.count);
		ret &= Deserialize(ref serialized.flag);

		// デシリアライズ後のバッファサイズを取得します.
		MouseSerializer mouse = new MouseSerializer();
		MouseData md = new MouseData();
		mouse.Serialize(md);
		byte[] buf= mouse.GetSerializedData();
		int size = buf.Length;
		
		serialized.datum = new MouseData[serialized.count];
		for (int i = 0; i < serialized.count; ++i) {
			serialized.datum[i] = new MouseData();
		}
		
		// 1パケットに含まれるすべてのmouseDataを取り出します.
		for (int i = 0; i < serialized.count; ++i) {
			byte[] buffer = new byte[size];
			
			// mouseDataの1フレーム分のデータを取り出します.
			bool ans = Deserialize(ref buffer, size);
			if (ans == false) {
				return false;
			}

			ret &= mouse.Deserialize(buffer, ref md);
			if (ret == false) {
				return false;
			}
			
			serialized.datum[i] = md;
		}
		
		return ret;
	}
}

#endif