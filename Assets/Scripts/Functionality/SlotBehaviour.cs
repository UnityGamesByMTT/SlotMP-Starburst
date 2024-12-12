using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine.UI.Extensions;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> images;     //class to store total images
    [SerializeField] private List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField] private List<SlotImage> ResultMatrix;     //class to store the result matrix

    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private Button TotalBetMinus_Button;
    [SerializeField] private Button LineBetPlus_Button;
    [SerializeField] private Button LineBetMinus_Button;
    [SerializeField] private Button MaxBet_Button;
    [SerializeField] private Sprite BigWin_Sprite;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;

    [Header("Animated Sprites")]
    [SerializeField] private Sprite[] BlueGem_Sprites;
    [SerializeField] private Sprite[] BlueGemEffect_Sprites;
    [SerializeField] private Sprite[] GreenGem_Sprites;
    [SerializeField] private Sprite[] GreenGemEffect_Sprites;
    [SerializeField] private Sprite[] PurpleGem_Sprites;
    [SerializeField] private Sprite[] PurpleGemEffect_Sprites;
    [SerializeField] private Sprite[] YellowGem_Sprites;
    [SerializeField] private Sprite[] YellowGemEffect_Sprites;
    [SerializeField] private Sprite[] OrangeGem_Sprites;
    [SerializeField] private Sprite[] OrangeGemEffect_Sprites;
    [SerializeField] private Sprite[] Seven_Sprites;
    [SerializeField] private Sprite[] Bar_Sprites;
    [SerializeField] private Sprite[] Rainbow_Sprites;
    [SerializeField] private Sprite[] comboSprites;
    [SerializeField] private Sprite[] BigWinAnimationSprites;
    [SerializeField] private Sprite[] HugeWinAnimationSprites;
    [SerializeField] private Sprite[] MegaWinAnimationSprites;


    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text LineBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text BigWin_Text;
    [SerializeField] private TMP_Text BonusWin_Text;
    [SerializeField] private Image[] Fill_Images;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;

    [SerializeField] private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField] private BonusController _bonusManager;
    
    [SerializeField] private TMP_Text FSnum_text;
    [SerializeField] private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField] private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField] private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool WinAnimationFin = true;

    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;

    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 10;
    [SerializeField] private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    private ImageAnimation BaseImageAnimation;
    private bool isBaseAnimationRunning;
    private Coroutine BaseAnimationCoroutine;
    private Coroutine RotationLineTween;
    private Coroutine ComboAnimationCoroutine;

    private Tween WinningsTextAnimationTween;
    private float BigWinAnimationTime;

    private Coroutine PaylinesCoroutine;

    private bool WaitForAnim;

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StartSlots();
        });

        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.RemoveAllListeners();
        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.RemoveAllListeners();
        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (LineBetPlus_Button) LineBetPlus_Button.onClick.RemoveAllListeners();
        if (LineBetPlus_Button) LineBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (LineBetMinus_Button) LineBetMinus_Button.onClick.RemoveAllListeners();
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            AutoSpin();
        });

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StopAutoSpin();
        });

        if(MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if(MaxBet_Button) MaxBet_Button.onClick.AddListener(()=> {
            uiManager.CanCloseMenu();
            SetMaxBet();
        });
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        { 
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        if (AutoSpinStop_Button) AutoSpinStop_Button.interactable = false;
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            AutoSpinStop_Button.interactable = true;
            tweenroutine = null;
            AutoSpinRoutine = null;
            IsAutoSpin = false;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        int j = spinchances;
        while (i < spinchances)
        {
            j -= 1;
            if (FSnum_text) FSnum_text.text = j.ToString();

            StartSlots(false);

            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
        StartCoroutine(_bonusManager.BonusGameEndRoutine());
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            SlotStart_Button.interactable = true;
        }
    }

    #region LinesCalculation

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        if(y_string.Count>0){
            y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
            PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
        }
    }
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }
    #endregion

    void SetMaxBet(){
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.Bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1; // Loop to the last bet
            }
        }
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void SetFillImage(){
        int fillBetCounter = BetCounter + 1;
        float fillAmount = Mathf.Clamp(1f/SocketManager.initialData.Bets.Count * fillBetCounter, 0, 1);
        // Debug.Log(fillAmount);
        foreach(Image i in Fill_Images){
            i.fillAmount= fillAmount;
        }
    } 

    #region InitialFunctions
    internal void ShuffleInitialMatrix()
    {
        for (int i = 0; i < images.Count; i++)
        {
            for (int j = 0; j < images[i].slotImages.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length-1);
                images[i].slotImages[j].sprite = myImages[randomIndex];
                
                if (j >= 8 && j <= 10)
                {
                    var imageAnimation = images[i].slotImages[j].GetComponent<ImageAnimation>();
                    if (imageAnimation != null)
                    {
                        PopulateBaseAnimationSprites(imageAnimation, randomIndex);
                    }
                }
            }
        }

        StartCoroutine(AnimationLoop());
    }

    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            if (!IsSpinning && !IsAutoSpin && !IsFreeSpin && !isBaseAnimationRunning)
            {
                if (BaseImageAnimation == null)
                {
                    int randomColumn = UnityEngine.Random.Range(0, 5);
                    int randomRow = UnityEngine.Random.Range(0, 3);

                    BaseImageAnimation = Tempimages[randomColumn].slotImages[randomRow].GetComponent<ImageAnimation>();
                    
                    if (BaseImageAnimation != null)
                    {
                        BaseAnimationCoroutine = StartCoroutine(BaseAnimation());
                    }
                }
            }
            else if(IsSpinning && IsAutoSpin && IsFreeSpin && isBaseAnimationRunning && BaseImageAnimation!=null){
                StopBaseAnimation();
            }

            yield return new WaitForSeconds(0.5f); // Add a delay to prevent overloading the game loop
        }
    }

    private IEnumerator BaseAnimation()
    {
        isBaseAnimationRunning = true;

        if (BaseImageAnimation != null)
        {
            if (BaseImageAnimation.textureArray.Count > 0)
            {
                BaseImageAnimation.StartAnimation();
                yield return new WaitUntil(() => BaseImageAnimation.textureArray[^1] == BaseImageAnimation.rendererDelegate.sprite);
                BaseImageAnimation.StopAnimation();
            }
            BaseImageAnimation = null;
        }
        isBaseAnimationRunning = false;
    }

    void StopBaseAnimation()
    {
        if (BaseImageAnimation != null)
        {
            StopCoroutine(BaseAnimationCoroutine);
            BaseImageAnimation.StopAnimation();
            BaseImageAnimation = null;
            isBaseAnimationRunning = false;
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateBaseAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.id=val.ToString();
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                animScript.textureArray.AddRange(PurpleGem_Sprites);
                animScript.AnimationSpeed = PurpleGem_Sprites.Length-8;
                break;

            case 1:
                animScript.textureArray.AddRange(BlueGem_Sprites);
                animScript.AnimationSpeed = BlueGem_Sprites.Length-8;
                break;
            case 2:
                animScript.textureArray.AddRange(OrangeGem_Sprites);
                animScript.AnimationSpeed = OrangeGem_Sprites.Length-8;
                break;
            case 3:
                animScript.textureArray.AddRange(GreenGem_Sprites);
                animScript.AnimationSpeed = GreenGem_Sprites.Length-8;
                break;
            case 4:
                animScript.textureArray.AddRange(YellowGem_Sprites);
                animScript.AnimationSpeed = YellowGem_Sprites.Length-8;
                break;
            case 5:
                animScript.textureArray.AddRange(Seven_Sprites);
                animScript.AnimationSpeed = Seven_Sprites.Length;
                break;
            case 6:
                animScript.textureArray.AddRange(Bar_Sprites);
                animScript.AnimationSpeed = Bar_Sprites.Length;
                break;
            // case 7:
            //     for(int i=0;i<Rainbow_Sprites.Length; i++)
            //     {
            //         animScript.textureArray.Add(Rainbow_Sprites[i]);
            //     }
            //     animScript.AnimationSpeed = 15f;
            //     break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        //WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;

        StopBaseAnimation();
        StopGameAnimation();
        PayCalculator.ResetLines();
        uiManager.StopWinAnimation();

        tweenroutine = StartCoroutine(TweenRoutine());

        if (TotalWin_text) TotalWin_text.text = "0.00";
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine(bool bonus = false)
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) // Check if balance is sufficient to place the bet
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }

        CheckSpinAudio = true;
        IsSpinning = true;
        ToggleButtonGrp(false);

        for (int i = 0; i < numberOfSlots; i++) // Initialize tweening for slot animations
        {
            InitializeTweening(Slot_Transform[i]);
        }

        BalanceDeduction(); //test

        yield return new WaitForSeconds(1f);

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        currentBalance = SocketManager.playerdata.Balance;

        for (int j = 0; j < SocketManager.resultData.ResultSymbols.Count; j++) // Update slot images based on the results
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (ResultMatrix[j].slotImages[i]) ResultMatrix[j].slotImages[i].sprite = myImages[resultnum[i]];
                string loc = i.ToString()+j.ToString();
                if(resultnum[i] <= 4 && SocketManager.resultData.FinalsymbolsToEmit.Contains(loc)){
                    // Debug.Log(ResultMatrix[j].slotImages[i].transform.GetChild(0).name);
                    PopulateWinningsAnimationSprites(ResultMatrix[j].slotImages[i].transform.GetChild(0).GetComponent<ImageAnimation>(), resultnum[i]);
                }
                if(resultnum[i] <= 6){
                    PopulateBaseAnimationSprites(ResultMatrix[j].slotImages[i].GetComponent<ImageAnimation>(), resultnum[i]);
                }
            }
        }

        // yield return new WaitForSeconds(2f);

        for (int i = 0; i < numberOfSlots; i++) // Stop tweening for each slot
        {
            yield return StopTweening(Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(1f);
        KillAllTweens();

        // if(!IsAutoSpin && !IsFreeSpin){
        //     PaylinesCoroutine = StartCoroutine(CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot));
        // }
        // else{
        // }
        PaylinesCoroutine = StartCoroutine(CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot));
        yield return PaylinesCoroutine;

        if (SocketManager.playerdata.currentWining>0){
            if(!IsAutoSpin && !IsFreeSpin){
                WinningsTextAnimation(SocketManager.playerdata.currentWining, CheckWinPopups()); //Trigger winnings animation if applicable
            }
            else{
                WaitForAnim = true;
                WinningsTextAnimation(SocketManager.playerdata.currentWining, CheckWinPopups()); //Trigger winnings animation if applicable
                yield return new WaitUntil(()=> !WaitForAnim);
            }
        }

        if(SocketManager.playerdata.currentWining <= 0) //add condition if its not starburst
        {
            audioController.PlayWLAudio("lose");
        }

        if (!IsAutoSpin && !IsFreeSpin) // Reset spinning state and toggle buttons
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
            yield return new WaitForSeconds(2f);
        }
    }

    private void PopulateWinningsAnimationSprites(ImageAnimation imageAnimation, int v)
    {
        imageAnimation.id=v.ToString();
        imageAnimation.textureArray.Clear();
        imageAnimation.textureArray.TrimExcess();
        imageAnimation.doLoopAnimation = false;
        switch(v){
            case 7:
                imageAnimation.textureArray.AddRange(Rainbow_Sprites);
                imageAnimation.AnimationSpeed=Rainbow_Sprites.Length;
                break;
            case 4:
                imageAnimation.textureArray.AddRange(YellowGemEffect_Sprites);
                imageAnimation.AnimationSpeed=YellowGemEffect_Sprites.Length;
                break;
            case 3:
                imageAnimation.textureArray.AddRange(GreenGemEffect_Sprites);
                imageAnimation.AnimationSpeed=GreenGemEffect_Sprites.Length;
                break;
            case 2:
                imageAnimation.textureArray.AddRange(OrangeGemEffect_Sprites);
                imageAnimation.AnimationSpeed=OrangeGemEffect_Sprites.Length;
                break;
            case 1:
                imageAnimation.textureArray.AddRange(BlueGemEffect_Sprites);
                imageAnimation.AnimationSpeed=BlueGemEffect_Sprites.Length;
                break;
            case 0:
                imageAnimation.textureArray.AddRange(PurpleGemEffect_Sprites);
                imageAnimation.AnimationSpeed=PurpleGemEffect_Sprites.Length;
                break;
        }
    }
    #endregion

    internal bool CheckWinPopups()
    {
        if (SocketManager.playerdata.currentWining >= currentTotalBet * 5 && SocketManager.playerdata.currentWining < currentTotalBet * 10)
        {
            BigWinAnimationTime = 3f;
            return true;
        }
        else if (SocketManager.playerdata.currentWining >= currentTotalBet * 10 && SocketManager.playerdata.currentWining < currentTotalBet * 15)
        {
            BigWinAnimationTime = 4f;
            return true;
        }
        else if (SocketManager.playerdata.currentWining >= currentTotalBet * 15)
        {
            BigWinAnimationTime = 5f;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void WinningsTextAnimation(double amount, bool BigWin)
    {
        float time=0.8f;
        if(BigWin){
            time = BigWinAnimationTime;
            StartCoroutine(uiManager.BigWinStartAnim());
            audioController.PlayWLAudio("bigwin");
        }
        double winnings = 0;
        if(!double.TryParse(Balance_text.text, out double balance)){
            Debug.Log("Error while conversion");
        }
        double newBalance = balance + amount;
        bool bigWinTriggered = false;
        bool hugeWinTriggered = false;
        bool megaWinTriggered = false;

        WinningsTextAnimationTween = DOTween.To(() => winnings, val => winnings = val, amount, time).OnUpdate(() =>
        {
            if (TotalWin_text)
            {
                TotalWin_text.text = winnings.ToString("f2");

                if(BigWin){
                    // Monitor and switch animations dynamically
                    if (!bigWinTriggered && winnings >= currentTotalBet * 5 && winnings < currentTotalBet * 10)
                    {
                        bigWinTriggered = true;
                        uiManager.StartWinAnimation(BigWin_Sprite, BigWinAnimationSprites);
                    }
                    else if (!hugeWinTriggered && winnings >= currentTotalBet * 10 && winnings < currentTotalBet * 15)
                    {
                        hugeWinTriggered = true;
                        uiManager.StartWinAnimation(HugeWin_Sprite, HugeWinAnimationSprites);
                    }
                    else if (!megaWinTriggered && winnings >= currentTotalBet * 15)
                    {
                        megaWinTriggered = true;
                        uiManager.StartWinAnimation(MegaWin_Sprite, MegaWinAnimationSprites);
                    }
                }
            }
        })
        .OnComplete(()=>{
            if(!BigWin){
                WaitForAnim = false;
            }
            DOVirtual.DelayedCall(3f, ()=> {
                uiManager.StopWinAnimation();
                WaitForAnim = false;
            });
        });
        DOTween.To(() => balance, (val) => balance = val, newBalance, time).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = balance.ToString("f2");
        });
    }

    private void BalanceDeduction()
    {
        if(!double.TryParse(TotalBet_text.text, out double bet)){
            Debug.Log("Error while conversion");
        }
        if(!double.TryParse(Balance_text.text, out double balance)){
            Debug.Log("Error while conversion");
        }
        double initAmount = balance;
        balance -= bet;
        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    //generate the payout lines generated 
    private IEnumerator CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            bool allIdsAreSame = true;
            string firstId = Tempimages[2].slotImages[0].GetComponent<ImageAnimation>().id;

            for (int i = 1; i < Tempimages[2].slotImages.Count; i++)
            {
                string currentId = Tempimages[2].slotImages[i].GetComponent<ImageAnimation>().id;
                if (currentId != firstId)
                {
                    allIdsAreSame = false;
                    break; // Exit the loop early if a mismatch is found
                }
            }
            if(allIdsAreSame){
                for(int i=Tempimages[2].slotImages.Count-1;i>=0;i--){
                    Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StartAnimation();
                    yield return new WaitForSeconds(1f);
                }
            }

            if (audioController) audioController.PlayWLAudio("win");
            FadeOutImages();
        
            List<Transform> transforms = new();
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    // Debug.Log(Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].name);
                    if (points_anim[k] >= 10)
                    {
                        Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        transforms.Add(Tempimages[points_anim[k] / 10 % 10].slotImages[points_anim[k] % 10].transform.GetChild(0));
                    }
                    else
                    {
                        Tempimages[0].slotImages[points_anim[k]].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        transforms.Add(Tempimages[0].slotImages[points_anim[k]].transform.GetChild(0));
                    }
                }
            }

            bool CanPlayComboAnim = !CheckWinPopups();

            for (int i = 0; i < transforms.Count; i++)
            {
                StartGameAnimation(transforms[i]);
                yield return new WaitForSeconds(0.2f);

                if(CanPlayComboAnim){
                    if ((i + 1) % 3 == 0)
                    {
                        Sprite comboSprite = null;

                        if ((i + 1) / 3 == 1 && LineId.Count >= 2)
                        {
                            comboSprite = comboSprites[0];
                        }
                        else if ((i + 1) / 3 == 2 && LineId.Count >= 3)
                        {
                            comboSprite = comboSprites[1];
                        }
                        else if ((i + 1) / 3 == 3 && LineId.Count >= 4)
                        {
                            comboSprite = comboSprites[2];
                        }
                        if (comboSprite != null)
                        {
                            while (uiManager.isComboSpritesAnimating)
                            {
                                yield return null;
                            }
                            ComboAnimationCoroutine = StartCoroutine(uiManager.AnimateSprite(comboSprite));
                        }
                    }
                } 
            }

            if(TempList.Count>0){
                if(TempList[^1].textureArray.Count==0){
                    for(int j=TempList.Count-1;j>=0;j--){
                        if(TempList[j].textureArray.Count>0){
                            yield return new WaitUntil(()=> TempList[j].textureArray[^1]==TempList[j].rendererDelegate.sprite);
                            break;
                        }
                    }
                }
                else{
                    yield return new WaitUntil(()=> TempList[^1].textureArray[^1]==TempList[^1].rendererDelegate.sprite);
                }
            }

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }

        // if (LineId.Count > 0)
        // {
        //     RotationLineTween = StartCoroutine(RotateLines(LineId));
        // }

        CheckSpinAudio = false;
    }

    private IEnumerator RotateLines(List<int> LineIDs)
    {
        yield return new WaitForSeconds(2f);
        // WinAnimationFin = false;
        // while (true)
        // {
        //     for (int i = 0; i < LineIDs.Count; i++)
        //     {
        //         PayCalculator.GeneratePayoutLinesBackend(LineIDs[i]);
        //         PayCalculator.DontDestroyLines.Add(LineIDs[i]);
        //         for (int s = 0; s < 5; s++)
        //         {
        //             if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim)
        //             {
        //                 TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].SetBG(Box_Sprites[LineIDs[i]]);
        //             }
        //         }
        //         if (LineIDs.Count < 2)
        //         {
        //             WinAnimationFin = true;
        //             yield return new WaitForSeconds(2f);
        //             yield break;
        //         }
        //         yield return new WaitForSeconds(2f);
        //         for (int s = 0; s < 5; s++)
        //         {
        //             if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim)
        //             {
        //                 TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].ResetBG();
        //             }
        //         }
        //         PayCalculator.DontDestroyLines.Clear();
        //         PayCalculator.DontDestroyLines.TrimExcess();
        //         PayCalculator.ResetStaticLine();
        //     }
        //     for (int i = 0; i < LineIDs.Count; i++)
        //     {
        //         PayCalculator.GeneratePayoutLinesBackend(LineIDs[i]);
        //         PayCalculator.DontDestroyLines.Add(LineIDs[i]);
        //     }
        //     yield return new WaitForSeconds(3f);
        //     PayCalculator.DontDestroyLines.Clear();
        //     PayCalculator.DontDestroyLines.TrimExcess();
        //     PayCalculator.ResetStaticLine();
        //     WinAnimationFin = true;
        // }
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button && !IsAutoSpin) AutoSpin_Button.interactable = toggle;

        if (LineBetMinus_Button) LineBetMinus_Button.interactable = toggle;
        if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = toggle;
        if (LineBetPlus_Button) LineBetPlus_Button.interactable = toggle;
        if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;
    }

    //Start the icons animation
    private void StartGameAnimation(Transform animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        if (temp.textureArray.Count > 0)
        {
            temp.StartAnimation();
            TempList.Add(temp);
            temp.IsAnim = true;
        }
    }

    private void FadeOutImages()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].GetComponent<Image>().color = new Color(0.33f, 0.33f, 0.33f, 1);
            }
        }
    }

    //Stop the icons animation
    internal void StopGameAnimation()
    {
        if(PaylinesCoroutine!=null){
            StopCoroutine(PaylinesCoroutine);
        }
        for(int i=0;i<ResultMatrix.Count;i++){
            for(int j=0;j<ResultMatrix[i].slotImages.Count;j++){
                ImageAnimation BaseAnim = ResultMatrix[i].slotImages[j].GetComponent<ImageAnimation>();
                ImageAnimation WinningsAnim = ResultMatrix[i].slotImages[j].transform.GetChild(0).GetComponent<ImageAnimation>();
                if(BaseAnim.textureArray.Count>0){
                    BaseAnim.StopAnimation();
                }
                BaseAnim.textureArray.Clear();
                BaseAnim.textureArray.TrimExcess();
                if(WinningsAnim.textureArray.Count>0){
                    WinningsAnim.StopAnimation();
                }
                WinningsAnim.textureArray.Clear();
                WinningsAnim.textureArray.TrimExcess();
                ResultMatrix[i].slotImages[j].color=new Color(1, 1, 1, 1);
            }
        }

        for(int i=0;i<Tempimages[2].slotImages.Count;i++){
            if(Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().currentAnimationState==ImageAnimation.ImageState.PLAYING){
                Tempimages[2].slotImages[i].transform.GetChild(1).GetComponent<ImageAnimation>().StopAnimation();
            }
        }

        if(uiManager.isComboSpritesAnimating){
            StopCoroutine(ComboAnimationCoroutine);
            uiManager.isComboSpritesAnimating = false;
        }

        PayCalculator.ResetStaticLine();
        PayCalculator.ResetLines();
        WinningsTextAnimationTween.Kill();

        
        // if (SkipWinAnimation_Button) SkipWinAnimation_Button.gameObject.SetActive(false);
        // if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.gameObject.SetActive(false);

        // if (TempList.Count > 0)
        // {
        //     for (int i = 0; i < TempList.Count; i++)
        //     {
        //         TempList[i].StopAnimation();
        //     }
        //     TempList.Clear();
        //     TempList.TrimExcess();
        // }

        // PayCalculator.DontDestroyLines.Clear();
        // PayCalculator.DontDestroyLines.TrimExcess();
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        Tweener tweener = null;
        slotTransform.DOLocalMoveY(-3221f, .7f)
        .SetEase(Ease.InBack)
        .OnComplete(()=> {
            slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -959f);

            tweener = slotTransform.DOLocalMoveY(-3221f, .7f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
            alltweens.Add(tweener);
        });
    }

    private IEnumerator StopTweening(Transform slotTransform, int index)
    {
        bool IsRegister = false;
        yield return alltweens[index].OnStepComplete(delegate { IsRegister = true; });
        yield return new WaitUntil(() => IsRegister);

        alltweens[index].Kill();
        slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -1372f);
        alltweens[index] = slotTransform.DOLocalMoveY(-1691f+323.195f, .7f).SetEase(Ease.OutQuint).OnComplete( ()=> {alltweens[index].Kill();} );
        if (audioController) audioController.PlayWLAudio("spinStop");
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}