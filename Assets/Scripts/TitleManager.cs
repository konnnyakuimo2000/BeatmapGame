using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TitleManager : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject Instruction;
    public GameObject StartPanel;
    public Transform ScrollViewContent;
    public GameObject SettingPanel;
    public ScrollRect scrollRect;

    [Header("リソース設定")]
    public GameObject musicButtonPrefab;
    public Beatmap[] beatmaps;

    [Header("SE設定")]
    public TextMeshProUGUI SEText;
    public Button SELeftButton;
    public Button SERightButton;
    public AudioClip[] SEClips;
    public string[] SENames;

    private TextMeshProUGUI instructionText;
    private float flashSpeed = 2.0f;
    private List<Button> musicButtons = new List<Button>();
    private int currentSelectionIndex = 0;
    private int currentSEIndex = 0;
    private AudioSource previewSource;

    void Start()
    {
        // 表示/非表示
        StartPanel.SetActive(false);
        SettingPanel.SetActive(false);

        // 曲リストの生成
        GenerateMusicList();

        // 変数割り当て
        instructionText = Instruction.GetComponent<TextMeshProUGUI>();

        // プレビュー用のAudioSourceを作成
        previewSource = gameObject.AddComponent<AudioSource>();

        // ボタンにクリックイベントを登録
        SELeftButton.onClick.AddListener(() => ChangeSE(-1));
        SERightButton.onClick.AddListener(() => ChangeSE(1));

        // SE名の表示を更新
        SEText.text = SENames[currentSEIndex];
    }

    void Update()
    {
        // 指示文を点滅させる
        if (Instruction.activeSelf)
        {
            float alpha = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
            instructionText.color = new Color (instructionText.color.r, instructionText.color.g, instructionText.color.b, alpha);
        }
        
        // スタートパネルも設定パネルも開いていない時
        if (!StartPanel.activeSelf && !SettingPanel.activeSelf)
        {
            // Enterキーではじめる
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Instruction.SetActive(false);
                StartPanel.SetActive(true);

                // 一番上を選択状態にする
                currentSelectionIndex = 0;
                UpdateSelectionVisual();
            }

            // Escapeキーで設定
            if (Input.GetKeyDown(KeyCode.Escape) && !StartPanel.activeSelf && !SettingPanel.activeSelf)
            {
                Instruction.SetActive(false);
                SettingPanel.SetActive(true);
            }
            
        }

        // スタートパネルが開いている時
        else if (StartPanel.activeSelf)
        {
            // 上移動
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelectionIndex--;
                if (currentSelectionIndex < 0) currentSelectionIndex = 0;
                UpdateSelectionVisual();
            }

            // 下移動
            if (Input.GetKeyDown(KeyCode.S)|| Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelectionIndex++;
                if (currentSelectionIndex >= musicButtons.Count) currentSelectionIndex = musicButtons.Count - 1;
                UpdateSelectionVisual();
            }

            // 決定
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 現在選択中のボタンのクリックイベントを実行
                if (musicButtons.Count > 0)
                {
                    musicButtons[currentSelectionIndex].onClick.Invoke();
                }
            }

            // BackSpaceで戻る
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                StartPanel.SetActive(false);
                Instruction.SetActive(true);
            }
        }

        // 設定パネルが開いている時
        else if (SettingPanel.activeSelf)
        {
            // A, DでSE切替
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) ChangeSE(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) ChangeSE(1);

            // Enterキーで戻る(TODO:設定項目が２個以上あるなら一個下に行く。最下なら戻る)
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartPanel.SetActive(false);
                SettingPanel.SetActive(false);
                Instruction.SetActive(true);
            }       
        }
    }

    // SEを変更する関数
    public void ChangeSE(int direction)
    {
        if (SEClips == null || SEClips.Length == 0) return;

        currentSEIndex += direction;

        // 範囲外になったらループさせる
        if (currentSEIndex < 0) currentSEIndex = SEClips.Length - 1;
        if (currentSEIndex >= SEClips.Length) currentSEIndex = 0;

        // SE名の表示を更新
        SEText.text = SENames[currentSEIndex];
        
        // 変更したSEをプレビュー再生
        previewSource.PlayOneShot(SEClips[currentSEIndex]);
    }

    // 譜面リストからボタンを生成する
    void GenerateMusicList()
    {
        musicButtons.Clear();

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

            // 曲選択リストに追加
            musicButtons.Add(btn);
        }
    }

    // 選択状態の見た目を更新する関数
    void UpdateSelectionVisual()
    {
        if (musicButtons.Count == 0) return;

        // EventSystemにより選択状態にする
        musicButtons[currentSelectionIndex].Select();

        // スクロール処理
        AutoScrollToSelection();
    }

    // 選択項目が画面内に収まるようにスクロールさせる
    void AutoScrollToSelection()
    {
        if (scrollRect == null) return;

        // ターゲットとなるボタンのRectTransform
        RectTransform target = musicButtons[currentSelectionIndex].GetComponent<RectTransform>();
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        // ビューポートの高さとコンテンツ全体の高さ
        float viewportHeight = viewport.rect.height;
        float contentHeight = content.rect.height;
        
        // スクロール可能な余地がない場合は何もしない
        if (contentHeight <= viewportHeight) return;

        // ターゲットの中心位置を取得
        float targetY = -target.anchoredPosition.y; 

        // 現在のスクロール位置を上からの距離に変換
        float scrollRange = contentHeight - viewportHeight;
        float currentScrollY = (1f - scrollRect.verticalNormalizedPosition) * scrollRange;

        // ターゲットを表示するために必要な範囲
        float itemHalfHeight = target.rect.height / 2f;
        float targetTop = targetY - itemHalfHeight;
        float targetBottom = targetY + itemHalfHeight;

        // 画面外にはみ出ている場合修正
        if (targetTop < currentScrollY)
        {
            float newScrollY = targetTop;
            scrollRect.verticalNormalizedPosition = 1f - (newScrollY / scrollRange);
        }
        else if (targetBottom > currentScrollY + viewportHeight)
        {
            float newScrollY = targetBottom - viewportHeight;
            scrollRect.verticalNormalizedPosition = 1f - (newScrollY / scrollRange);
        }
    }

    // 曲が押されたとき
    void OnMusicSelected(Beatmap beatmap)
    {
        // 選んだ曲を静的変数に保存
        RhythmGameController.SelectedBeatmap = beatmap;

        // 選んだSEの番号も静的変数に保存
        RhythmGameController.SelectedSeIndex = currentSEIndex;

        // シーン遷移
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}