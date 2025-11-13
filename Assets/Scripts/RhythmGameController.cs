using System.Collections.Generic;
using UnityEngine;

public class RhythmGameController : MonoBehaviour
{
    [Header("譜面データ")]
    public Beatmap currentBeatmap;

    [Header("レーン設定")]
    // 4レーンのX座標 (Quadの幅が1の場合)
    public float[] laneXPositions = new float[] { -1.5f, -0.5f, 0.5f, 1.5f };

    [Header("オブジェクト参照")]
    public AudioSource musicSource; // 音楽再生用
    public GameObject notePrefab;   // ステップ1で作ったノーツのPrefab

    [Header("ゲーム設定")]
    public float noteSpeed = 10f;   // ノーツが流れる速さ (単位/秒)
    public float spawnY = 10f;        // ノーツが生成されるY座標
    public float judgementY = -3f;   // 判定ラインのY座標 

    [Header("判定設定")]
    // 判定ラインからの許容距離
    public float hitTolerance = 0.5f;

    // 判定に使用するキー (左から D, F, S, K)
    private KeyCode[] keys = new KeyCode[] {
        KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K
    };

    // 4つのレーンそれぞれで、ノーツを順番に管理するキュー
    private List<Queue<NoteObject>> laneQueues = new List<Queue<NoteObject>>();
    // --- 内部変数 ---
    private double gameTime = 0; // ゲームの経過時間（秒）
    private int nextNoteIndex = 0;  // 次に生成すべきノーツのインデックス

    // ノーツが生成されてから判定ラインに到達するまでの時間（秒）
    private double noteTravelTimeInSeconds;

    void Start()
    {
        if (currentBeatmap == null)
        {
            Debug.LogError("譜面が設定されていません。");
            this.enabled = false;
            return;
        }

        // 4レーン分のキューを初期化
        for (int i = 0; i < laneXPositions.Length; i++)
        {
            laneQueues.Add(new Queue<NoteObject>());
        }

        // 1. 音楽をセットして再生
        musicSource.clip = currentBeatmap.audioClip;
        musicSource.Play();

        // 2. ノーツの移動時間を計算
        // (距離) = (生成Y) - (判定Y)
        float distance = spawnY - judgementY;
        // (時間) = (距離) / (速さ)
        noteTravelTimeInSeconds = (double)distance / noteSpeed;

        Debug.Log($"ノーツの移動時間: {noteTravelTimeInSeconds} 秒");

        // 3. 譜面データを時間順にソートしておく (エディタ側で保証されていても念のため)
        currentBeatmap.notes.Sort((a, b) => a.step.CompareTo(b.step));

        gameTime = 0;
        nextNoteIndex = 0;
    }

    void Update()
    {
        if (currentBeatmap == null || !musicSource.isPlaying) return;

        // 1. 現在の音楽再生時間を取得 (高精度)
        gameTime = musicSource.time;

        while (nextNoteIndex < currentBeatmap.notes.Count)
        {
            NoteData noteToSpawn = currentBeatmap.notes[nextNoteIndex];
            double noteHitTime = BeatmapUtility.GetTimeFromStep(currentBeatmap, noteToSpawn.step);
            double noteSpawnTime = noteHitTime - noteTravelTimeInSeconds;

            // 2. 現在のゲーム時間が、生成時間を超えたか？
            if (gameTime >= noteSpawnTime)
            {
                // 3. (重要) ノーツがどれだけ「遅れて」生成されたかを計算
                //    (例: gameTimeが0.1, noteSpawnTimeが-0.2 の場合、0.3秒遅れている)
                double timeSinceSpawn = gameTime - noteSpawnTime;

                // 4. SpawnNote に「遅れた時間」を渡す
                SpawnNote(noteToSpawn, timeSinceSpawn);

                nextNoteIndex++; // 次のノーツへ
            }
            else
            {
                // 5. (重要) まだ生成時間ではないノーツに到達したら、
                //    このフレームの処理を終わり、次のフレームを待つ
                break;
            }
        }

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                // 対応するレーンの判定処理を呼ぶ
                CheckHit(i);
            }
        }
    }

    /// <summary>
    /// ノーツを実際にシーンに生成する関数
    /// </summary>
    void SpawnNote(NoteData noteData, double timeSinceSpawn)
    {
        // 1. レーンのX座標
        float xPos = laneXPositions[noteData.lane];

        // 2. ノーツの長さ（Yスケール）を計算
        double noteStartTime = BeatmapUtility.GetTimeFromStep(currentBeatmap, noteData.step);
        double noteEndTime = BeatmapUtility.GetTimeFromStep(currentBeatmap, noteData.step + noteData.length_in_steps);
        double noteDurationInSeconds = noteEndTime - noteStartTime;
        float noteLengthInUnits = noteSpeed * (float)noteDurationInSeconds;

        // 3. (重要) ノーツの「正しい」Y座標を計算

        // 3a. 本来スポーンすべきだったY座標（中心）
        float baseCenterY = spawnY + (noteLengthInUnits / 2f);

        // 3b. 「遅れた時間」の分だけ、下に移動させる
        //     (移動距離) = (速さ) * (遅れた時間)
        float distanceToMove = noteSpeed * (float)timeSinceSpawn;

        // 3c. 最終的なY座標（中心）
        float yPos = baseCenterY - distanceToMove;

        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        // Prefabをインスタンス化
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        // NoteObject スクリプトのコンポーネントを取得
        NoteObject noteScript = noteObj.GetComponent<NoteObject>();

        // 必要な情報を NoteObject に渡す
        noteScript.speed = noteSpeed;
        noteScript.controller = this; // コントローラー（自分自身）への参照
        noteScript.lane = noteData.lane; // レーン番号

        // スケール変更
        noteObj.transform.localScale = new Vector3(
            noteObj.transform.localScale.x,
            noteLengthInUnits,
            noteObj.transform.localScale.z);

        // 対応するレーンのキューに、今作ったノーツを追加
        laneQueues[noteData.lane].Enqueue(noteScript);
    }

    /// <summary>
    /// 指定されたレーンのキーが押された時の判定処理
    /// </summary>
    /// <param name="laneIndex">判定するレーン番号 (0-3)</param>
    private void CheckHit(int laneIndex)
    {
        // そのレーンに判定すべきノーツが存在するか？
        if (laneQueues[laneIndex].Count > 0)
        {
            // キューの先頭にあるノーツ（一番古いノーツ）を取得
            NoteObject note = laneQueues[laneIndex].Peek();

            // ノーツの下端のY座標を計算
            float noteBottomY = note.transform.position.y - (note.transform.localScale.y / 2.0f);

            //　その下端と判定ラインとの距離を計算
            float distance = Mathf.Abs(noteBottomY - judgementY);

            // 距離が許容範囲内か
            if (distance <= hitTolerance)
            {
                // ヒット
                Debug.Log($"HIT! Lane {laneIndex} (Distance: {distance})");

                // キューからノーツを削除
                laneQueues[laneIndex].Dequeue();

                // ノーツにHit()を伝え、消滅させる
                note.Hit();
            }
            else
            {
                // キーは押したが、ノーツが遠すぎる (BAD)
                Debug.Log($"BAD. Lane {laneIndex} (Distance: {distance})");
            }
        }
        else
        {
            // キーは押したが、そのレーンにノーツがなかった (EMPTY)
            Debug.Log($"EMPTY. Lane {laneIndex}");
        }
    }

    /// <summary>
    /// NoteObjectから呼ばれ、ノーツが判定ラインを過ぎた（Miss）ことを処理する
    /// </summary>
    public void NoteMissed(NoteObject note)
    {
        // ノーツがdespawnYを通過してMissedとして報告された時、
        // それがまだ叩かれていない場合
        if (laneQueues[note.lane].Count > 0 && laneQueues[note.lane].Peek() == note)
        {
            Debug.Log($"MISS. Lane {note.lane}");

            // キューからそのノーツを削除
            laneQueues[note.lane].Dequeue();
        }
    }
}