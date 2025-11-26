using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject StartButton;
    public GameObject SettingButton;
    public GameObject ScrollView;
    public Transform ScrollViewContent;
    public GameObject SettingPanel;

    [Header("リソース設定")]
    public GameObject musicButtonPrefab;
    public Beatmap[] beatmaps;

    void Start()
    {
        // 表示/非表示
        ScrollView.SetActive(false);
        StartButton.SetActive(true);

        // 曲リストの生成
        GenerateMusicList();
    }

    // はじめるボタン押下時
    public void OnStartButtonDown()
    {
        StartButton.SetActive(false);
        SettingButton.SetActive(false);

        ScrollView.SetActive(true);
    }

    // 設定ボタン押下時
    public void OnSettingButtonDown()
    {
        StartButton.SetActive(false);
        SettingButton.SetActive(false);
        
        SettingPanel.SetActive(true);
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