using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject Instruction;
    public GameObject ScrollView;
    public Transform ScrollViewContent;
    public GameObject SettingPanel;

    [Header("リソース設定")]
    public GameObject musicButtonPrefab;
    public Beatmap[] beatmaps;

    private TextMeshProUGUI instructionText;
    private float flashSpeed = 2.0f;

    void Start()
    {
        // 表示/非表示
        ScrollView.SetActive(false);
        SettingPanel.SetActive(false);

        // 曲リストの生成
        GenerateMusicList();

        // 変数割り当て
        instructionText = Instruction.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // 指示文を点滅させる
        if (Instruction.activeSelf)
        {
            float alpha = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
            instructionText.color = new Color (instructionText.color.r, instructionText.color.g, instructionText.color.b, alpha);
        }
        
        // Enterキーではじめる
        if (Input.GetKeyDown(KeyCode.Return) && !ScrollView.activeSelf && !SettingPanel.activeSelf)
        {
            Instruction.SetActive(false);
            ScrollView.SetActive(true);
        }

        // Escapeキーで設定
        if (Input.GetKeyDown(KeyCode.Escape) && !ScrollView.activeSelf && !SettingPanel.activeSelf)
        {
            Instruction.SetActive(false);
            SettingPanel.SetActive(true);
        }

        // BackSpaceで戻る
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ScrollView.SetActive(false);
            SettingPanel.SetActive(false);
            Instruction.SetActive(true);
        }

    }

    // 譜面リストからボタンを生成する
    void GenerateMusicList()
    {
        foreach (Beatmap beatmap in beatmaps)
        {
            // ボタンをContentの子として生成
            GameObject btnObj = Instantiate(musicButtonPrefab, ScrollViewContent);

            // 曲名を設定
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = beatmap.title;
            }

            // ボタンクリック時の動作を登録しておく
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnMusicSelected(beatmap));
        }
    }

    // 曲が押されたとき
    void OnMusicSelected(Beatmap beatmap)
    {
        // 選んだ曲を静的変数に保存
        RhythmGameController.SelectedBeatmap = beatmap;

        // シーン遷移
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}