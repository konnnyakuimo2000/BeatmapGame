using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBeatmap", menuName = "Beatmap")]
public class Beatmap : ScriptableObject
{
    [Header("曲の基本設定")]
    public AudioClip audioClip;
    public double bpm = 140.0;
    public int beatsPerMeasure = 4; // 拍子 (4/4なら 4)
    public double firstBeatOffsetSec = 0.0; // 最初の拍までのオフセット(秒)

    [Header("エディタ設定")]
    // 1小節あたりのグリッド分割数
    public int stepsPerMeasure = 16; 

    [Header("譜面データ")]
    public List<NoteData> notes = new List<NoteData>();
}