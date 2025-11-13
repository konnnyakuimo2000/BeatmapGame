using UnityEngine;

public class NoteObject : MonoBehaviour
{

    // 消える位置
    private float despawnY = -20f;

    // 速さ
    public float speed;

    // 自身を管理するコントローラー
    public RhythmGameController controller;
    
    // 自身のレーン番号 (0-3)
    public int lane;

    void Update()
    {
        // 毎フレーム、真下に移動させる
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        
        // ノーツの上端のY座標を計算
        float noteTopY = transform.position.y + (transform.localScale.y / 2.0f);
        // 画面外に出たら自身を破棄する
        if (noteTopY < despawnY)
        {
            if (controller != null)
            {
                controller.NoteMissed(this);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 叩かれた（Hit）時にコントローラーから呼ばれる
    /// </summary>
    public void Hit()
    {
        // （ここにエフェクト再生などを追加できる）
        Destroy(gameObject); // 自身を破棄する
    }
}