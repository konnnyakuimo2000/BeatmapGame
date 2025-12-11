using UnityEngine;

public class NoteObject : MonoBehaviour
{

    /// <summary>
    /// 消える位置
    /// </summary>
    private float DespawnZ = -5f;

    /// <summary>
    /// 速さ
    /// </summary>
    public float Speed;

    /// <summary>
    /// 自身を管理するコントローラ
    /// </summary>
    public GameManager Controller;

    /// <summary>
    /// 自身のレーン番号 (0-3)
    /// </summary>
    public int Lane;

    /// <summary>
    /// 自分が長押しノーツか
    /// </summary>
    public bool IsLongNote = false;

    /// <summary>
    /// 自分が現在押さえられているか
    /// </summary>
    private bool isHolding = false;

    /// <summary>
    /// 自身のレンダラ
    /// </summary>
    private SpriteRenderer objRenderer;

    void Awake()
    {
        // 自分のレンダラを起動時に取得
        objRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        // 奥から手前に移動
        transform.Translate(Vector3.back * Speed * Time.deltaTime, Space.World);

        // ノーツの上端のZ座標を計算
        float noteFrontZ = transform.position.z + (transform.localScale.z / 2.0f);

        // 画面外に出たら自身を破棄する
        if (noteFrontZ < DespawnZ)
        {
            if (Controller != null && !isHolding)
            {
                Controller.NoteMissed(this);
            }
            Destroy(gameObject);
        }

        // 押さえられていてかつノーツの上端が判定ラインを通過したら
        if (isHolding && noteFrontZ < Controller.JudgeZ)
        {
            // 成功として自動で消滅
            Controller.AutoRelease(Lane);
            Hit(); // 自分を消す
        }
    }

    /// <summary>
    /// 叩かれた時
    /// </summary>
    public void Hit()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 長押し開始時
    /// </summary>
    public void Hold()
    {
        isHolding = true;

        // 色を濃くする
        Color.RGBToHSV(objRenderer.material.color, out float h, out float s, out float v);
        objRenderer.material.color = Color.gray4;
    }
}