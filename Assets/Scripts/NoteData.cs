using System;

[Serializable]
public class NoteData
{
    // どのタイミング（ステップ）で始まるか
    // (例: 16分音符のグリッドなら、1小節目の1拍目=0, 1拍目ウラ=2, 2拍目=4...)
    public int step; 

    // ノーツの長さ（ステップ単位）
    // (例: 1ステップの長さが16分音符なら、
    //  length_in_steps = 1 -> 16分音符の長さ
    //  length_in_steps = 2 -> 8分音符の長さ
    //  length_in_steps = 4 -> 4分音符の長さ)
    public int length_in_steps;

    // どのレーンか
    public int lane;
    
    // コンストラクタ
    public NoteData(int step, int length_in_steps, int lane = 0)
    {
        this.step = step;
        this.length_in_steps = length_in_steps;
        this.lane = lane;
    }
}