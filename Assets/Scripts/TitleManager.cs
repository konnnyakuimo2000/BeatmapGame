using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    public GameObject BackgroundFrame;
    [Header("オブジェクト参照")]
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
    public AudioClip StartPanelSE;
    public AudioClip SettingPanelSE;
    public AudioClip SelectSE;

    [Header("SE音量設定")]
    public TextMeshProUGUI SEVolumeLabelText;
    public Transform SEVolumeMeterParent;

    [Header("BGM音量設定")]
    public TextMeshProUGUI BGMVolumeLabelText;
    public Transform BGMVolumeMeterParent;

    [Header("トランジション")]
    public Image TransitionPanel;

    private TextMeshProUGUI instructionText;
    private float flashSpeed = 2.0f;
    private List<Button> musicButtons = new List<Button>();
    private int currentSelectionIndex = 0;
    private int currentSEIndex = 0;
    private AudioSource previewSource;
    private int currentSettingRow = 0;
    private int currentSEVolume = 10;
    private int currentBGMVolume = 10;
    private Image[] seVolumeBars;
    private Image[] bgmVolumeBars;
    private Color activeColor = Color.mediumSpringGreen;
    private Color inactiveColor = new Color(1, 1, 1, 0.4f);
    private Color selectedLabelColor = Color.yellow;
    private Color normalLabelColor = Color.white;

    void Start()
    {
        // 表示/非表示
        StartPanel.SetActive(false);
        SettingPanel.SetActive(false);
        TransitionPanel.gameObject.SetActive(false);
        
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
        UpdateSEVolumeMeter();

        // BGM音量メータの初期化
        bgmVolumeBars = BGMVolumeMeterParent.GetComponentsInChildren<Image>();
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
            // 背景フレームを見せる
            BackgroundFrame.SetActive(true);

            // Enterキーではじめる
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Instruction.SetActive(false);
                StartPanel.SetActive(true);
                previewSource.PlayOneShot(StartPanelSE, currentSEVolume / 10.0f);

                // 一番上を選択状態にする
                currentSelectionIndex = 0;
                UpdateSelectionVisual();
            }

            // Escapeキーで設定
            if (Input.GetKeyDown(KeyCode.Escape) && !StartPanel.activeSelf && !SettingPanel.activeSelf)
            {
                Instruction.SetActive(false);
                SettingPanel.SetActive(true);
                previewSource.PlayOneShot(SettingPanelSE, currentSEVolume / 10.0f);

                currentSettingRow = 0;
                UpdateSettingVisual();
            }

        }

        // スタートパネルが開いている時
        else if (StartPanel.activeSelf)
        {
            // 背景フレームを見せない
            BackgroundFrame.SetActive(false);
            
            // 上移動
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelectionIndex--;
                if (currentSelectionIndex < 0) currentSelectionIndex = 0;
                previewSource.PlayOneShot(SelectSE, currentSEVolume / 10.0f);
                UpdateSelectionVisual();
            }

            // 下移動
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelectionIndex++;
                if (currentSelectionIndex >= musicButtons.Count) currentSelectionIndex = musicButtons.Count - 1;
                previewSource.PlayOneShot(SelectSE, currentSEVolume / 10.0f);
                UpdateSelectionVisual();
            }

            // 決定
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 現在選択中のボタンのクリックイベントを実行
                if (musicButtons.Count > 0)
                {
                    previewSource.PlayOneShot(StartPanelSE);
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
        // 背景フレームを見せない
        BackgroundFrame.SetActive(false);
    
        // 項目の移動
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSettingRow--;
            if (currentSettingRow < 0) currentSettingRow = 0;
            UpdateSettingVisual();
            previewSource.PlayOneShot(SelectSE, currentSEVolume / 10.0f);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSettingRow++;
            if (currentSettingRow > 2) currentSettingRow = 2;
            UpdateSettingVisual();
            previewSource.PlayOneShot(SelectSE, currentSEVolume / 10.0f);
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
                previewSource.PlayOneShot(SelectSE, currentSEVolume / 10.0f);
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

        // シーンのトランジションを開始
        StartCoroutine(TitleTransition());
    }

    /// <summary>
    /// ゲームシーンへのトランジション
    /// </summary>
    /// <returns></returns>
    private IEnumerator TitleTransition()
    {
        TransitionPanel.gameObject.SetActive(true);
        
        // 最初はサイズ0
        TransitionPanel.rectTransform.localScale = Vector3.zero;

        // 演出のために複製
        Transform parent = TransitionPanel.transform.parent;
        Image panel0 = TransitionPanel;
        Image panel1 = Instantiate(TransitionPanel, parent);
        Image panel2 = Instantiate(TransitionPanel, parent);
        Image panel3 = Instantiate(TransitionPanel, parent);
        Image panel4 = Instantiate(TransitionPanel, parent);
        
        // 色の設定
        panel0.color = Color.white;
        panel1.color = Color.orangeRed;
        panel2.color = Color.lightGray;
        panel3.color = Color.dimGray;
        panel4.color = Color.black;

        float duration = 0.8f;
        float maxScale = 30.0f;
        float time = 0f;

        while (time < duration + 0.8f)
        {
            time += Time.deltaTime;

            // 進捗率を時差付きで計算
            float t0 = Mathf.Clamp01((time - 0f) / duration);
            float t1 = Mathf.Clamp01((time - 0.4f) / duration);
            float t2 = Mathf.Clamp01((time - 0.6f) / duration);
            float t3 = Mathf.Clamp01((time - 0.75f) / duration);
            float t4 = Mathf.Clamp01((time - 0.8f) / duration);

            // 4次関数でイージング
            float scale0 = Mathf.Pow(t0, 4.0f) * maxScale;
            float scale1 = Mathf.Pow(t1, 4.0f) * maxScale;
            float scale2 = Mathf.Pow(t2, 4.0f) * maxScale;
            float scale3 = Mathf.Pow(t3, 4.0f) * maxScale;
            float scale4 = Mathf.Pow(t4, 4.0f) * maxScale;

            // サイズ適用
            TransitionPanel.rectTransform.localScale = new Vector3(scale0, scale0, 1f);
            panel1.rectTransform.localScale = new Vector3(scale1, scale1, 1f);
            panel2.rectTransform.localScale = new Vector3(scale2, scale2, 1f);
            panel3.rectTransform.localScale = new Vector3(scale3, scale3, 1f);
            panel4.rectTransform.localScale = new Vector3(scale4, scale4, 1f);

            yield return null;
        }

        // シーン遷移
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}