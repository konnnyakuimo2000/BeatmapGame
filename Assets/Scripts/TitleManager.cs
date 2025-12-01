using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    public TextMeshProUGUI SETypeLabelText;
    public TextMeshProUGUI SEText;
    public Button SELeftButton;
    public Button SERightButton;
    public AudioClip[] SEClips;
    public string[] SENames;

    [Header("SE音量設定")]
    public TextMeshProUGUI SEVolumeLabelText;
    public Transform SEVolumeMeterParent;

    [Header("BGM音量設定")]
    public TextMeshProUGUI BGMVolumeLabelText;
    public Transform BGMVolumeMeterParent;

    private TextMeshProUGUI instructionText;
    private float flashSpeed = 2.0f;
    private List<Button> musicButtons = new List<Button>();
    private int currentSelectionIndex = 0;
    private int currentSEIndex = 0;
    private AudioSource previewSource;
    private int currentSettingRow = 0;
    private int currentSEVolume = 5;
    private int currentBGMVolume = 5;
    private Image[] seVolumeBars;
    private Image[] bgmVolumeBars;
    private Color activeColor = Color.white;
    private Color inactiveColor = new Color(1, 1, 1, 0.2f);
    private Color selectedLabelColor = Color.yellow;
    private Color normalLabelColor = Color.white;

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

        // SE音量メータの初期化
        seVolumeBars = SEVolumeMeterParent.GetComponentsInChildren<Image>();
        currentSEVolume = 5;
        UpdateSEVolumeMeter();

        // BGM音量メータの初期化
        bgmVolumeBars = BGMVolumeMeterParent.GetComponentsInChildren<Image>();
        currentBGMVolume = 5;
        UpdateBGMVolumeMeter();
    }

    void Update()
    {
        // 指示文を点滅させる
        if (Instruction.activeSelf)
        {
            float alpha = Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f;
            instructionText.color = new Color(instructionText.color.r, instructionText.color.g, instructionText.color.b, alpha);
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

                currentSettingRow = 0;
                UpdateSettingVisual();
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
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
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
            HandleSettingInput();
        }
    }

    // 設定画面の入力をまとめた関数
    void HandleSettingInput()
    {
        // 項目の移動
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSettingRow--;
            if (currentSettingRow < 0) currentSettingRow = 0;
            UpdateSettingVisual();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSettingRow++;
            if (currentSettingRow > 2) currentSettingRow = 2;
            UpdateSettingVisual();
        }

        // 値の変更
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentSettingRow == 0) // SE選択
            {
                StartCoroutine(SEButton(SELeftButton));
                ChangeSE(-1);
            }
            else if (currentSettingRow == 1) // SE音量
            {
                ChangeSEVolume(-1);
            }
            else if (currentSettingRow == 2) // BGM音量
            {
                ChangeBGMVolume(-1);
            }
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentSettingRow == 0) // SE選択
            {
                StartCoroutine(SEButton(SERightButton));
                ChangeSE(1);
            }
            else if (currentSettingRow == 1) // SE音量
            {
                ChangeSEVolume(1);
            }
            else if (currentSettingRow == 2) // BGM音量
            {
                ChangeBGMVolume(1);
            }
        }

        // Enterキーの処理
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 一番下の項目なら戻る、それ以外なら下の項目へ
            if (currentSettingRow == 2)
            {
                StartPanel.SetActive(false);
                SettingPanel.SetActive(false);
                Instruction.SetActive(true);
            }
            else
            {
                currentSettingRow++;
                UpdateSettingVisual();
            }
        }

        // BackSpaceなら即戻る
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StartPanel.SetActive(false);
            SettingPanel.SetActive(false);
            Instruction.SetActive(true);
        }
    }

    // SEを変更する関数
    public void ChangeSE(int direction)
    {
        currentSEIndex += direction;

        // 範囲外になったらループさせる
        if (currentSEIndex < 0) currentSEIndex = SEClips.Length - 1;
        if (currentSEIndex >= SEClips.Length) currentSEIndex = 0;

        // SE名の表示を更新
        SEText.text = SENames[currentSEIndex];

        // 変更したSEをプレビュー再生
        if (direction != 0) previewSource.PlayOneShot(SEClips[currentSEIndex], currentSEVolume / 10.0f);
    }

    // SEの音量を変更する関数
    public void ChangeSEVolume(int direction)
    {
        currentSEVolume += direction;
        // 0 ~ 10 の範囲に制限
        if (currentSEVolume < 0) currentSEVolume = 0;
        if (currentSEVolume > seVolumeBars.Length) currentSEVolume = seVolumeBars.Length;

        // 表示更新
        UpdateSEVolumeMeter();

        // 確認のために音を鳴らす
        previewSource.PlayOneShot(SEClips[currentSEIndex], currentSEVolume / 10.0f);
        
    }

    // BGMの音量を変更する関数
    public void ChangeBGMVolume(int direction)
    {
        currentBGMVolume += direction;
        // 0 ~ 10 の範囲に制限
        if (currentBGMVolume < 0) currentBGMVolume = 0;
        if (currentBGMVolume > bgmVolumeBars.Length) currentBGMVolume = bgmVolumeBars.Length;

        // 表示更新
        UpdateBGMVolumeMeter();
    }

    // SE音量メーターの見た目を更新
    void UpdateSEVolumeMeter()
    {
        for (int i = 0; i < seVolumeBars.Length; i++)
        {
            if (i < currentSEVolume) seVolumeBars[i].color = activeColor;
            else seVolumeBars[i].color = inactiveColor;
        }
    }

    // BGM音量メーターの見た目を更新
    void UpdateBGMVolumeMeter()
    {
        for (int i = 0; i < bgmVolumeBars.Length; i++)
        {
            if (i < currentBGMVolume) bgmVolumeBars[i].color = activeColor;
            else bgmVolumeBars[i].color = inactiveColor;
        }
    }

    // 選択中の行の見た目を更新
    void UpdateSettingVisual()
    {
        SETypeLabelText.color = (currentSettingRow == 0) ? selectedLabelColor : normalLabelColor;
        SEVolumeLabelText.color = (currentSettingRow == 1) ? selectedLabelColor : normalLabelColor;
        BGMVolumeLabelText.color = (currentSettingRow == 2) ? selectedLabelColor : normalLabelColor;
    }

    // ボタン押下時のアニメーション
    private IEnumerator SEButton(Button btn)
    {
        float duration = 0.1f;
        Vector3 pressedScale = new Vector3(0.8f, 0.8f, 1f);

        // 縮む
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            btn.transform.localScale = Vector3.Lerp(Vector3.one, pressedScale, elapsed / duration);
            yield return null;
        }

        // 戻る
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            btn.transform.localScale = Vector3.Lerp(pressedScale, Vector3.one, elapsed / duration);
            yield return null;
        }
        btn.transform.localScale = Vector3.one;
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

        // 設定項目を静的変数に保存
        RhythmGameController.SelectedSEIndex = currentSEIndex;
        RhythmGameController.SelectedSEVolume = currentSEVolume / 10.0f;
        RhythmGameController.SelectedBGMVolume = currentBGMVolume / 10.0f;

        // シーン遷移
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}