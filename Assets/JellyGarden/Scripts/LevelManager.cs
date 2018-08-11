using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Assets.Plugins.SmartLevelsMap.Scripts;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SquareBlocks
{
    public SquareTypes block;
    public SquareTypes obstacle;

}

public enum GameState
{
    Map,
    PrepareGame,
    Playing,
    Highscore,
    GameOver,
    Pause,
    PreWinAnimations,
    Win,
    WaitForPopup,
    WaitAfterClose,
    BlockedGame,
    Tutorial,
    PreTutorial,
    WaitForPotion,
    PreFailed,
    RegenLevel
}


public class LevelManager : MonoBehaviour
{

    public static LevelManager THIS;
    public static LevelManager Instance;
    public GameObject itemPrefab;
    public GameObject squarePrefab;
    public Sprite squareSprite;
    public Sprite squareSprite1;
    public Sprite outline1;
    public Sprite outline2;
    public Sprite outline3;
    public GameObject blockPrefab;
    public GameObject wireBlockPrefab;
    public GameObject solidBlockPrefab;
    public GameObject undesroyableBlockPrefab;
    public GameObject thrivingBlockPrefab;
    public LifeShop lifeShop;
    public Transform GameField;
    public bool enableInApps;
    public int maxRows = 9;
    public int maxCols = 9;
    public int opponentRows;//对手所占的行数
    public int spaceRows;//敌我隔离区域占的行数
    public int mineRows;//自己所占的行数 maxRows = opponentRows + spaceRows + mineRows;

    public float squareWidth = 1.2f;
    public float squareHeight = 1.2f;
    public Vector2 firstSquarePosition;
    public Square[] squaresArray;
    List<List<Item>> combinedItems = new List<List<Item>>();
    public Item lastDraggedItem;
    public Item lastSwitchedItem;
    public List<Item> destroyAnyway = new List<Item>();
    public GameObject popupScore;
    public int scoreForItem = 10;
    public int scoreForBlock = 100;
    public int scoreForWireBlock = 100;
    public int scoreForSolidBlock = 100;
    public int scoreForThrivingBlock = 100;
    public LIMIT limitType;
    public int Limit = 30;
    public int TargetScore = 1000;
    public int currentLevel = 1;
    public int FailedCost;
    public int ExtraFailedMoves = 5;
    public int ExtraFailedSecs = 30;
    public List<GemProduct> gemsProducts = new List<GemProduct>();
    public string[] InAppIDs;
    public string GoogleLicenseKey;
    LineRenderer line;
    public bool thrivingBlockDestroyed;
    List<List<Item>> newCombines;
    private bool dragBlocked;
    public int BoostColorfullBomb;
    public int BoostPackage;
    public int BoostStriped;
    public bool BoostHandActivated;
    public bool BoostBombActivated;
    public bool BoostReplacingActivated;
    public BoostIcon emptyBoostIcon;
    public BoostIcon AvctivatedBoostView;
    public BoostIcon activatedBoost;
    public string androidSharingPath;
    public string iosSharingPath;

    public CLevelConfig levelConfig;
    public BoostIcon ActivatedBoost
    {
        get
        {
            if (activatedBoost == null)
            {
                //BoostIcon bi = new BoostIcon();
                //bi.type = BoostType.None;
                return emptyBoostIcon;
            }
            else
                return activatedBoost;
        }
        set
        {
            if (value == null)
            {
                if (activatedBoost != null && gameStatus == GameState.Playing)
                    InitScript.Instance.SpendBoost(activatedBoost.type);
                UnLockBoosts();
            }
            //        if (activatedBoost != null) return;
            activatedBoost = value;

            if (value != null)
            {
                LockBoosts();
            }

            if (activatedBoost != null)
            {
                if (activatedBoost.type == BoostType.ExtraMoves || activatedBoost.type == BoostType.ExtraTime)
                {
                    if (LevelManager.Instance.limitType == LIMIT.MOVES)
                        LevelManager.THIS.Limit += 5;
                    else
                        LevelManager.THIS.Limit += 30;

                    ActivatedBoost = null;
                }

            }
        }
    }

    SquareBlocks[] levelSquaresFile = new SquareBlocks[81];
    public int targetBlocks;

    public GameObject[] itemExplPool = new GameObject[20];
    public static int Score;
    public int stars;
    private int linePoint;
    public int star1;
    public int star2;
    public int star3;
    public bool showPopupScores;

    public GameObject stripesEffect;
    public GameObject snowParticle;
    public Color[] scoresColors;
    public Color[] scoresColorsOutline;
    public int colorLimit;
    public int[] ingrCountTarget = new int[2];
    public Ingredients[] ingrTarget = new Ingredients[2];
    public CollectItems[] collectItems = new CollectItems[2];
    public Sprite[] ingrediendSprites;
    public string[] targetDiscriptions;
    private bool matchesGot;
    bool ingredientFly;
    public GameObject[] gratzWords;

    public GameObject Level;
    public GameObject LevelsMap;

    public BoostIcon[] InGameBoosts;
    public int passLevelCounter;

    public Target target;

    public GameObject SuccessImage;

    public int TargetBlocks
    {
        get
        {
            return targetBlocks;
        }
        set
        {
            if (targetBlocks < 0)
                targetBlocks = 0;
            targetBlocks = value;
        }
    }

    public bool DragBlocked
    {
        get
        {
            return dragBlocked;
        }
        set
        {
            if (value)
            {
                List<Item> items = GetItems();
                foreach (Item item in items)
                {
                    //if (item != null)
                    //    item.anim.SetBool("stop", true);
                }
            }
            else
            {
                //  StartCoroutine( StartIdleCor());
            }
            dragBlocked = value;
        }
    }

    private GameState GameStatus;
    public bool itemsHided;
    public int moveID;
    public int lastRandColor;
    public bool onlyFalling;
    public bool levelLoaded;
    public Hashtable countedSquares;
    public Sprite doubleBlock;
    public bool FacebookEnable;
    internal int latstMatchColor;
    public CombineManager combineManager;

    #region EVENTS

    public delegate void GameStateEvents();

    public static event GameStateEvents OnMapState;
    public static event GameStateEvents OnEnterGame;
    public static event GameStateEvents OnLevelLoaded;
    public static event GameStateEvents OnMenuPlay;
    public static event GameStateEvents OnMenuComplete;
    public static event GameStateEvents OnStartPlay;
    public static event GameStateEvents OnWin;
    public static event GameStateEvents OnLose;

    public GameState gameStatus
    {
        get
        {
            return GameStatus;
        }
        set
        {
            GameStatus = value;
            InitScript.Instance.CheckAdsEvents(value);

            if (value == GameState.PrepareGame)
            {
                passLevelCounter++;

                MusicBase.Instance.GetComponent<AudioSource>().Stop();
                MusicBase.Instance.GetComponent<AudioSource>().loop = true;
                MusicBase.Instance.GetComponent<AudioSource>().clip = MusicBase.Instance.music[1];
                MusicBase.Instance.GetComponent<AudioSource>().Play();
                PrepareGame();
            }
            else if (value == GameState.WaitForPopup)
            {

                InitLevel();
                OnLevelLoaded();


            }
            else if (value == GameState.PreFailed)
            {
                GameObject.Find("CanvasGlobal").transform.Find("PreFailed").gameObject.SetActive(true);

            }
            else if (value == GameState.Map)
            {
                if (PlayerPrefs.GetInt("OpenLevelTest") <= 0)
                {
                    MusicBase.Instance.GetComponent<AudioSource>().Stop();
                    MusicBase.Instance.GetComponent<AudioSource>().loop = true;
                    MusicBase.Instance.GetComponent<AudioSource>().clip = MusicBase.Instance.music[0];
                    MusicBase.Instance.GetComponent<AudioSource>().Play();
                    EnableMap(true);
                    OnMapState();
                }
                else
                {
                    LevelManager.THIS.gameStatus = GameState.PrepareGame;
                    PlayerPrefs.SetInt("OpenLevelTest", 0);
                    PlayerPrefs.Save();
                }
                if (passLevelCounter > 0 && InitScript.Instance.ShowRateEvery > 0)
                {
                    if (passLevelCounter % InitScript.Instance.ShowRateEvery == 0 && InitScript.Instance.ShowRateEvery > 0 && PlayerPrefs.GetInt("Rated", 0) == 0)
                        InitScript.Instance.ShowRate();
                }
            }
            else if (value == GameState.Pause)
            {
                Time.timeScale = 0;

            }
            else if (value == GameState.Playing)
            {
                Time.timeScale = 1;
                StartCoroutine(AI.THIS.CheckPossibleCombines());
            }
            else if (value == GameState.GameOver)
            {
                GameObject.Find("CanvasGlobal").transform.Find("MenuFailed").gameObject.SetActive(true);
                OnLose();
            }
            else if (value == GameState.PreWinAnimations)
            {
                StartCoroutine(PreWinAnimationsCor());
            }
            else if (value == GameState.Win)
            {
                //InitScript.THIS.AddGems(1);//Add coins after win
                OnMenuComplete();
                if (PlayerPrefs.GetInt(string.Format("Level.{0:000}.StarsCount", currentLevel), 0) < stars)
                    PlayerPrefs.SetInt(string.Format("Level.{0:000}.StarsCount", currentLevel), stars);
                if (Score > PlayerPrefs.GetInt("Score" + currentLevel))
                {
                    PlayerPrefs.SetInt("Score" + currentLevel, Score);
                }

#if PLAYFAB || GAMESPARKS
                NetworkManager.dataManager.SetPlayerScore(currentLevel, Score);
                NetworkManager.dataManager.SetPlayerLevel(currentLevel + 1);
                NetworkManager.dataManager.SetStars();
#endif
                GameObject.Find("CanvasGlobal").transform.Find("MenuComplete").gameObject.SetActive(true);
                SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.complete[1]);
                OnWin();
            }


        }
    }

    public void MenuPlayEvent()
    {
        OnMenuPlay();
    }


    #endregion

    void LockBoosts()
    {
        foreach (BoostIcon item in InGameBoosts)
        {
            if (item != ActivatedBoost)
                item.LockBoost();
        }
    }

    public void UnLockBoosts()
    {
        foreach (BoostIcon item in InGameBoosts)
        {
            item.UnLockBoost();
        }
    }


    public void LoadLevel()
    {
        currentLevel = PlayerPrefs.GetInt("OpenLevel");// TargetHolder.level;
        if (currentLevel == 0)
            currentLevel = 1;
        LoadDataFromLocal(currentLevel);



    }

    public void EnableMap(bool enable)
    {
        float aspect = (float)Screen.height / (float)Screen.width;//2.1.4
        GetComponent<Camera>().orthographicSize = 5.3f;
        aspect = (float)Math.Round(aspect, 2);
        if (enable)
        {
            if (aspect == 1.6f)
                GetComponent<Camera>().orthographicSize = 6.25f;                    //16:10
            else if (aspect == 1.78f)
                GetComponent<Camera>().orthographicSize = 7f;    //16:9
            else if (aspect == 1.5f)
                GetComponent<Camera>().orthographicSize = 5.9f;                  //3:2
            else if (aspect == 1.33f)
                GetComponent<Camera>().orthographicSize = 5.25f;                  //4:3
            else if (aspect == 1.67f)
                GetComponent<Camera>().orthographicSize = 6.6f;                  //5:3
            else if (aspect == 1.25f)
                GetComponent<Camera>().orthographicSize = 4.9f;                  //5:4
            else if (aspect == 2.06f)
                GetComponent<Camera>().orthographicSize = 8.2f;                  //2960:1440
            else if (aspect == 2.17f)
                GetComponent<Camera>().orthographicSize = 8.7f;                  //iphone x
            GetComponent<Camera>().GetComponent<MapCamera>().SetPosition(new Vector2(0, GetComponent<Camera>().transform.position.y));
        }
        else
        {
            InitScript.DateOfExit = DateTime.Now.ToString();  //1.4

            LevelManager.THIS.latstMatchColor = -1;

            GetComponent<Camera>().orthographicSize = 6.5f;
            if (aspect == 2.06f)
                GetComponent<Camera>().orthographicSize = 7.6f;                  //2960:1440
            else if (aspect == 2.17f)
                GetComponent<Camera>().orthographicSize = 8.1f;                  //iphone x
            GameObject.Find("CanvasGlobal").GetComponent<GraphicRaycaster>().enabled = false;
            GameObject.Find("CanvasGlobal").GetComponent<GraphicRaycaster>().enabled = true;
            Level.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = false;
            Level.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = true;

        }
        Camera.main.GetComponent<MapCamera>().enabled = enable;
        LevelsMap.SetActive(!enable);
        LevelsMap.SetActive(enable);
        Level.SetActive(!enable);

        if (enable)
            GameField.gameObject.SetActive(false);

        if (!enable)
            Camera.main.transform.position = new Vector3(0, 0, -10);
        foreach (Transform item in GameField.transform)
        {
            Destroy(item.gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
#if FACEBOOK
        FacebookEnable = true;//1.6.2
        if (FacebookEnable)
            FacebookManager.THIS.CallFBInit();
#else
        FacebookEnable = false;

#endif
#if UNITY_INAPPS

        gameObject.AddComponent<UnityInAppsIntegration>();
        enableInApps = true;//1.6.1
#else
        enableInApps = false;

#endif
        combineManager = new CombineManager();
        THIS = this;
        Instance = this;
        if (!LevelManager.THIS.enableInApps)
            GameObject.Find("CanvasMap/Gems").gameObject.SetActive(false);//2.1.2

        gameStatus = GameState.Map;
        for (int i = 0; i < 20; i++)
        {
            itemExplPool[i] = Instantiate(Resources.Load("Prefabs/Effects/ItemExpl"), transform.position, Quaternion.identity) as GameObject;
            itemExplPool[i].GetComponent<SpriteRenderer>().enabled = false;

            // itemExplPool[i].SetActive(false);
        }
        passLevelCounter = 0;

        SuccessImage.SetActive(false);
    }

    void InitLevel()
    {

        GenerateLevel();
#if OLD_LOGIC
        GenerateOutline();
#endif
        ReGenLevel();
        if (limitType == LIMIT.TIME)
        {
            StopCoroutine(TimeTick());
            StartCoroutine(TimeTick());
        }
        GameField.gameObject.SetActive(true);

    }

    void PrepareGame()
    {
        ActivatedBoost = null;
        Score = 0;
        stars = 0;
        moveID = 0;

        collectItems[0] = CollectItems.None;
        collectItems[1] = CollectItems.None;

        ingrTarget[0] = Ingredients.None;
        ingrTarget[1] = Ingredients.None;

        ingrCountTarget[0] = 0;
        ingrCountTarget[1] = 0;

        TargetBlocks = 0;
        EnableMap(false);


        GameField.transform.position = Vector3.zero;
        firstSquarePosition = GameField.transform.position;

        squaresArray = new Square[maxCols * maxRows];
        LoadLevel();
        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if (levelSquaresFile[row * maxCols + col].block == SquareTypes.BLOCK)
                    TargetBlocks++;
                else if (levelSquaresFile[row * maxCols + col].block == SquareTypes.DOUBLEBLOCK)
                    TargetBlocks += 2;
            }
        }
        //float getSize = maxCols - 9;
        //if (getSize < maxRows - 9)
        //    getSize = maxRows - 9;
        //if (getSize > 0)
        //    camera.orthographicSize = 6.5f + getSize * 0.5f;
        GameObject.Find("Canvas").transform.Find("PrePlay").gameObject.SetActive(true);
        if (limitType == LIMIT.MOVES)
        {
            InGameBoosts[0].gameObject.SetActive(true);
            InGameBoosts[1].gameObject.SetActive(false);
        }
        else
        {
            InGameBoosts[0].gameObject.SetActive(false);
            InGameBoosts[1].gameObject.SetActive(true);

        }
        OnEnterGame();
    }

    public void CheckCollectedTarget(GameObject _item)
    {
        for (int i = 0; i < 2; i++)
        {
            if (ingrCountTarget[i] > 0)
            {
                if (_item.GetComponent<Item>() != null)
                {
                    if (_item.GetComponent<Item>().currentType == ItemsTypes.NONE)
                    {
                        if (_item.GetComponent<Item>().color == (int)collectItems[i] - 1)
                        {
                            GameObject item = new GameObject();
                            item.transform.position = _item.transform.position;
                            item.transform.localScale = Vector3.one / 2f;
                            SpriteRenderer spr = item.AddComponent<SpriteRenderer>();
                            spr.sprite = _item.GetComponent<Item>().GetItemSprites()[_item.GetComponent<Item>().color];
                            spr.sortingLayerName = "UI";
                            spr.sortingOrder = 1;
                        }
                    }
                    else if (_item.GetComponent<Item>().currentType == ItemsTypes.INGREDIENT)
                    {
                        if (_item.GetComponent<Item>().color == (int)ingrTarget[i] + 1000)
                        {
                            GameObject item = new GameObject();
                            item.transform.position = _item.transform.position;
                            item.transform.localScale = Vector3.one / 2f;
                            SpriteRenderer spr = item.AddComponent<SpriteRenderer>();
                            spr.sprite = _item.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                            spr.sortingLayerName = "UI";
                            spr.sortingOrder = 1;
                        }
                    }

                }
            }
        }
        if (targetBlocks > 0)
        {
            if (_item.GetComponent<Square>() != null)
            {
                GameObject item = new GameObject();
                item.transform.position = _item.transform.position;
                // item.transform.localScale = Vector3.one / 2f;
                SpriteRenderer spr = item.AddComponent<SpriteRenderer>();
                spr.sprite = _item.GetComponent<SpriteRenderer>().sprite;
                spr.sortingLayerName = "UI";
                spr.sortingOrder = 1;
            }
        }


    }

    public GameObject GetExplFromPool()
    {
        for (int i = 0; i < itemExplPool.Length; i++)
        {
            if (!itemExplPool[i].GetComponent<SpriteRenderer>().enabled)
            {
                // itemExplPool[i].SetActive(true);
                itemExplPool[i].GetComponent<SpriteRenderer>().enabled = true;
                StartCoroutine(HideDelayed(itemExplPool[i]));
                return itemExplPool[i];
            }

        }
        return null;
    }

    IEnumerator HideDelayed(GameObject gm)
    {
        yield return new WaitForSeconds(1f);
        gm.GetComponent<Animator>().SetTrigger("stop");
        gm.GetComponent<Animator>().SetInteger("color", 10);
        gm.GetComponent<SpriteRenderer>().enabled = false;
        //gm.SetActive(false);
    }


    public void CheckWinLose()
    {
        //		print ("check win lose");
        if (Limit <= 0)
        {
            bool lose = false;
            Limit = 0;

            if (LevelManager.THIS.target == Target.BLOCKS && LevelManager.THIS.TargetBlocks > 0)
            {
                lose = true;
            }
            else if (LevelManager.THIS.target == Target.COLLECT && (LevelManager.THIS.ingrCountTarget[0] > 0 || LevelManager.THIS.ingrCountTarget[1] > 0))
            {
                lose = true;
            }
            else if (LevelManager.THIS.target == Target.INGREDIENT && (LevelManager.THIS.ingrCountTarget[0] > 0 || LevelManager.THIS.ingrCountTarget[1] > 0))
            {
                lose = true;
            }
            if (LevelManager.Score < LevelManager.THIS.star1)
            {
                lose = true;

            }
            if (lose)
                gameStatus = GameState.PreFailed;
            else if (LevelManager.Score >= LevelManager.THIS.star1 && LevelManager.THIS.target == Target.SCORE)
            {
                gameStatus = GameState.PreWinAnimations;

            }
            else if (LevelManager.Score >= LevelManager.THIS.star1 && LevelManager.THIS.target == Target.BLOCKS && LevelManager.THIS.TargetBlocks <= 0)
            {
                gameStatus = GameState.PreWinAnimations;

            }
            else if (LevelManager.Score >= LevelManager.THIS.star1 && LevelManager.THIS.target == Target.COLLECT && (LevelManager.THIS.ingrCountTarget[0] <= 0 && LevelManager.THIS.ingrCountTarget[1] <= 0))
            {
                gameStatus = GameState.PreWinAnimations;

            }
            else if (LevelManager.Score >= LevelManager.THIS.star1 && LevelManager.THIS.target == Target.INGREDIENT && (LevelManager.THIS.ingrCountTarget[0] <= 0 && LevelManager.THIS.ingrCountTarget[1] <= 0))
            {
                gameStatus = GameState.PreWinAnimations;

            }


        }
        else
        {
            bool win = false;

            if (LevelManager.THIS.target == Target.BLOCKS && LevelManager.THIS.TargetBlocks <= 0)
            {
                win = true;
            }
            else if (LevelManager.THIS.target == Target.COLLECT && (LevelManager.THIS.ingrCountTarget[0] <= 0 && LevelManager.THIS.ingrCountTarget[1] <= 0))
            {
                win = true;
            }
            else if (LevelManager.THIS.target == Target.INGREDIENT && (LevelManager.THIS.ingrCountTarget[0] <= 0 && LevelManager.THIS.ingrCountTarget[1] <= 0))
            {
                win = true;
                //			} else if (LevelManager.Score >= LevelManager.THIS.star1 && LevelManager.THIS.target == Target.SCORE) { // win if get 1 start and still have moves
                //				win = true;
            }
            if (LevelManager.Score < LevelManager.THIS.star1)
            {
                win = false;

            }
            //else if (LevelManager.THIS.target == Target.SCORE && LevelManager.Score >= LevelManager.THIS.star1)
            //    win = true;
            if (win)
            {
                if (currentLevel >= 1 && currentLevel <= 10)
                    gameStatus = GameState.Win;
                else
                    gameStatus = GameState.PreWinAnimations;
            }


        }
    }

    IEnumerator PreWinAnimationsCor()
    {
        if (!InitScript.Instance.losingLifeEveryGame)
            InitScript.Instance.AddLife(1);
        GameObject.Find("Canvas").transform.Find("CompleteLabel").gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        List<Item> items = GetRandomItems(limitType == LIMIT.MOVES ? Limit : 8);
        foreach (Item item in items)
        {
            if (limitType == LIMIT.MOVES)
                Limit--;
            item.NextType = (ItemsTypes)UnityEngine.Random.Range(1, 3);
            item.ChangeType();
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(0.3f);
        while (GetAllExtaItems().Count > 0 && gameStatus != GameState.Win)
        { //1.6
            Item item = GetAllExtaItems()[0];
            item.DestroyItem();
            dragBlocked = true;
            yield return new WaitForSeconds(0.1f);
            FindMatches();
            yield return new WaitForSeconds(1f);

            //           GenerateNewItems();
            while (dragBlocked)
                yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1f);
        while (dragBlocked || GetMatches().Count > 0)
            yield return new WaitForSeconds(0.2f);

        GameObject.Find("Canvas").transform.Find("CompleteLabel").gameObject.SetActive(false);
        SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.complete[0]);

        GameObject.Find("Canvas").transform.Find("PreCompleteBanner").gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        GameObject.Find("Canvas").transform.Find("PreCompleteBanner").gameObject.SetActive(false);


        gameStatus = GameState.Win;
    }

    void Update()
    {
        //  AvctivatedBoostView = ActivatedBoost;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NoMatches();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            gameStatus = GameState.PreWinAnimations;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Limit = 1;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {   //save items state
            print("Saving items...");
            int[] items = new int[99];

            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxCols; col++)
                {
                    if (GetSquare(col, row, false) != null)
                    {
                        if (GetSquare(col, row, false).item != null)
                            items[row * maxCols + col] = GetSquare(col, row, false).item.color;
                    }
                    else
                        items[row * maxCols + col] = -1;

                }
            }
            LevelDebugger.SaveMap(items, maxCols, maxRows);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {   //load items state
            print("load items...");

            int[] items = new int[99];
            items = LevelDebugger.LoadMap(maxCols, maxRows);
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxCols; col++)
                {
                    if (items[row * maxCols + col] > -1)
                    {
                        if (GetSquare(col, row).item != null)
                            GetSquare(col, row).item.SetColor(items[row * maxCols + col]);
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (LevelManager.THIS.gameStatus == GameState.Playing)
                GameObject.Find("CanvasGlobal").transform.Find("MenuPause").gameObject.SetActive(true);
            else if (LevelManager.THIS.gameStatus == GameState.Map)
                Application.Quit();

        }


        if (LevelManager.THIS.gameStatus == GameState.Playing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnStartPlay();
                Collider2D hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (hit != null)
                {
                    Item item = hit.gameObject.GetComponent<Item>();
                    if (!LevelManager.THIS.DragBlocked && LevelManager.THIS.gameStatus == GameState.Playing)
                    {
                        if (LevelManager.THIS.ActivatedBoost.type == BoostType.Bomb && item.currentType != ItemsTypes.BOMB && item.currentType != ItemsTypes.INGREDIENT)
                        {
                            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.boostBomb);
                            LevelManager.THIS.DragBlocked = true;
                            GameObject obj = Instantiate(Resources.Load("Prefabs/Effects/bomb"), item.transform.position, item.transform.rotation) as GameObject;
                            obj.GetComponent<SpriteRenderer>().sortingOrder = 4;
                            obj.GetComponent<BoostAnimation>().square = item.square;
                            LevelManager.THIS.ActivatedBoost = null;
                        }
                        else if (LevelManager.THIS.ActivatedBoost.type == BoostType.Random_color && item.currentType != ItemsTypes.BOMB)
                        {
                            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.boostColorReplace);
                            LevelManager.THIS.DragBlocked = true;
                            GameObject obj = Instantiate(Resources.Load("Prefabs/Effects/random_color_item"), item.transform.position, item.transform.rotation) as GameObject;
                            obj.GetComponent<BoostAnimation>().square = item.square;
                            obj.GetComponent<SpriteRenderer>().sortingOrder = 4;
                            LevelManager.THIS.ActivatedBoost = null;
                        }
                        else if (item.square.type != SquareTypes.WIREBLOCK)
                        {
                            item.dragThis = true;
                            item.mousePos = item.GetMousePosition();
                            item.deltaPos = Vector3.zero;
                        }
                    }
                }

            }
            else if (Input.GetMouseButtonUp(0))
            {
                Collider2D hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (hit != null)
                {
                    Item item = hit.gameObject.GetComponent<Item>();
                    item.dragThis = false;
                    item.switchDirection = Vector3.zero;
                }

            }
        }


        //判断游戏是否结束：玩家死亡活着怪物被杀光
        BattleSystem.Instance.Update();
    }

    IEnumerator TimeTick()
    {
        while (true)
        {
            if (gameStatus == GameState.Playing)
            {
                if (LevelManager.Instance.limitType == LIMIT.TIME)
                {
                    LevelManager.THIS.Limit--;
                    if (IsAllItemsFallDown())
                        CheckWinLose();
                }
            }
            if (gameStatus == GameState.Map)
                yield break;
            yield return new WaitForSeconds(1);
        }
    }

    private void GenerateLevel()
    {

        GenerateMineSolider();
        GenerateSpace();
        GenerateEnemySolider();
        float scale = 0.4f;
        Vector3 fieldPos = new Vector3(-maxCols * scale, maxRows * scale, -10);
        AnimateField(fieldPos);

    }
    void GenerateMineSolider()
    {
        bool chessColor = false;
        int beginMineRow = maxRows - opponentRows - spaceRows;
        for (int row = beginMineRow; row < maxRows; row++)
        {
            if (maxCols % 2 == 0)
                chessColor = !chessColor;
            for (int col = 0; col < maxCols; col++)
            {
                CreateSquare(col, row, chessColor);
                chessColor = !chessColor;
            }

        }
    }
    void GenerateEnemySolider()
    {
        bool chessColor = false;
        for (int row = 0; row < opponentRows; row++)
        {
            if (maxCols % 2 == 0)
                chessColor = !chessColor;
            for (int col = 0; col < maxCols; col++)
            {
                CreateSquare(col, row, chessColor);
                chessColor = !chessColor;
            }

        }
    }
    void GenerateSpace()
    {
        bool chessColor = false;
        for (int row = opponentRows; row < opponentRows + spaceRows; row++)
        {
            if (maxCols % 2 == 0)
                chessColor = !chessColor;
            for (int col = 0; col < maxCols; col++)
            {
                CreateSquare(col, row, chessColor);
                chessColor = !chessColor;
            }

        }
    }
    void AnimateField(Vector3 pos)
    {

        float yOffset = 0;
        if (target == Target.INGREDIENT)
            yOffset = 0.3f;
        Animation anim = GameField.GetComponent<Animation>();
        AnimationClip clip = new AnimationClip();
        float xOffset = 15;
        AnimationCurve curveX = new AnimationCurve(new Keyframe(0, pos.x + xOffset), new Keyframe(0.7f, pos.x - 0.2f), new Keyframe(0.8f, pos.x));
        AnimationCurve curveY = new AnimationCurve(new Keyframe(0, pos.y + yOffset), new Keyframe(1, pos.y + yOffset));
        //#if UNITY_5 || UNITY_2017 
        clip.legacy = true;
        //#endif
        clip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
        clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        clip.AddEvent(new AnimationEvent() { time = 1, functionName = "EndAnimGamField" });
        anim.AddClip(clip, "appear");
        anim.Play("appear");
        GameField.transform.position = new Vector2(pos.x + xOffset, pos.y + yOffset);

    }

    void CreateSquare(int col, int row, bool chessColor = false)
    {
        GameObject square = null;
        square = Instantiate(squarePrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
        if (chessColor)
        {
            square.GetComponent<SpriteRenderer>().sprite = squareSprite1;
        }
        square.GetComponent<SpriteRenderer>().enabled = false;
        square.transform.SetParent(GameField);
        square.transform.localPosition = firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight);

        squaresArray[row * maxCols + col] = square.GetComponent<Square>();
        square.GetComponent<Square>().row = row;
        square.GetComponent<Square>().col = col;
        square.GetComponent<Square>().type = SquareTypes.EMPTY;
        if (row < LevelManager.THIS.opponentRows)
        {
            square.GetComponent<Square>().gravity = Vector3.down;
        }
        else if (row < LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows)
        {
            square.GetComponent<Square>().gravity = Vector3.zero;
        }
        else
        {
            square.GetComponent<Square>().gravity = Vector3.up;
        }
        if (levelSquaresFile[row * maxCols + col].block == SquareTypes.EMPTY)
        {
            CreateObstacles(col, row, square, SquareTypes.NONE);
        }
        else if (levelSquaresFile[row * maxCols + col].block == SquareTypes.NONE)
        {
            square.GetComponent<SpriteRenderer>().enabled = false;
            square.GetComponent<Square>().type = SquareTypes.NONE;

        }
        else if (levelSquaresFile[row * maxCols + col].block == SquareTypes.BLOCK)
        {
            GameObject block = Instantiate(blockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;

            // TargetBlocks++;
            CreateObstacles(col, row, square, SquareTypes.NONE);
        }
        else if (levelSquaresFile[row * maxCols + col].block == SquareTypes.DOUBLEBLOCK)
        {
            GameObject block = Instantiate(blockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;

            //  TargetBlocks++;
            block = Instantiate(blockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.01f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.BLOCK;
            block.GetComponent<SpriteRenderer>().sprite = doubleBlock;
            block.GetComponent<SpriteRenderer>().sortingOrder = 1;
            //  TargetBlocks++;
            CreateObstacles(col, row, square, SquareTypes.NONE);
        }

    }
#if OLD_LOGIC
    void GenerateOutline()
    {
        int row = 0;
        int col = 0;
        for (row = 0; row < maxRows; row++)
        { //down
            SetOutline(col, row, 0);
        }
        row = maxRows - 1;
        for (col = 0; col < maxCols; col++)
        { //right
            SetOutline(col, row, 90);
        }
        col = maxCols - 1;
        for (row = maxRows - 1; row >= 0; row--)
        { //up
            SetOutline(col, row, 180);
        }
        row = 0;
        for (col = maxCols - 1; col >= 0; col--)
        { //left
            SetOutline(col, row, 270);
        }
        col = 0;
        for (row = 1; row < maxRows - 1; row++)
        {
            for (col = 1; col < maxCols - 1; col++)
            {
                //  if (GetSquare(col, row).type == SquareTypes.NONE)
                SetOutline(col, row, 0);
            }
        }
    }


    void SetOutline(int col, int row, float zRot)
    {
        Square square = GetSquare(col, row, true);
        if (!square)
        {
            return;
        }
        if (square.type != SquareTypes.NONE)
        {
            if (row == 0 || col == 0 || col == maxCols - 1 || row == maxRows - 1)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                outline.transform.localRotation = Quaternion.Euler(0, 0, zRot);
                if (zRot == 0)
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.425f;
                if (zRot == 90)
                    outline.transform.localPosition = Vector3.zero + Vector3.down * 0.425f;
                if (zRot == 180)
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.425f;
                if (zRot == 270)
                    outline.transform.localPosition = Vector3.zero + Vector3.up * 0.425f;
                if (row == 0 && col == 0)
                {   //top left
                    spr.sprite = outline3;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 180);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.up * 0.015f;
                }
                if (row == 0 && col == maxCols - 1)
                {   //top right
                    spr.sprite = outline3;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.up * 0.015f;
                }
                if (row == maxRows - 1 && col == 0)
                {   //bottom left
                    spr.sprite = outline3;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.down * 0.015f;
                }
                if (row == maxRows - 1 && col == maxCols - 1)
                {   //bottom right
                    spr.sprite = outline3;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.down * 0.015f;
                }
            }
            else
            {
                //top left
                if (GetSquare(col - 1, row - 1, true).type == SquareTypes.NONE && GetSquare(col, row - 1, true).type == SquareTypes.NONE && GetSquare(col - 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    spr.sprite = outline3;
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.up * 0.015f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 180);
                }
                //top right
                if (GetSquare(col + 1, row - 1, true).type == SquareTypes.NONE && GetSquare(col, row - 1, true).type == SquareTypes.NONE && GetSquare(col + 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    spr.sprite = outline3;
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.up * 0.015f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                //bottom left
                if (GetSquare(col - 1, row + 1, true).type == SquareTypes.NONE && GetSquare(col, row + 1, true).type == SquareTypes.NONE && GetSquare(col - 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    spr.sprite = outline3;
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.015f + Vector3.down * 0.015f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 270);
                }
                //bottom right
                if (GetSquare(col + 1, row + 1, true).type == SquareTypes.NONE && GetSquare(col, row + 1, true).type == SquareTypes.NONE && GetSquare(col + 1, row, true).type == SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    spr.sprite = outline3;
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.015f + Vector3.down * 0.015f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }


            }
        }
        else
        {
            bool corner = false;
            if (GetSquare(col - 1, row, true).type != SquareTypes.NONE && GetSquare(col, row - 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                spr.sprite = outline2;
                outline.transform.localPosition = Vector3.zero;
                outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                corner = true;
            }
            if (GetSquare(col + 1, row, true).type != SquareTypes.NONE && GetSquare(col, row + 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                spr.sprite = outline2;
                outline.transform.localPosition = Vector3.zero;
                outline.transform.localRotation = Quaternion.Euler(0, 0, 180);
                corner = true;
            }
            if (GetSquare(col + 1, row, true).type != SquareTypes.NONE && GetSquare(col, row - 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                spr.sprite = outline2;
                outline.transform.localPosition = Vector3.zero;
                outline.transform.localRotation = Quaternion.Euler(0, 0, 270);
                corner = true;
            }
            if (GetSquare(col - 1, row, true).type != SquareTypes.NONE && GetSquare(col, row + 1, true).type != SquareTypes.NONE)
            {
                GameObject outline = CreateOutline(square);
                SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                spr.sprite = outline2;
                outline.transform.localPosition = Vector3.zero;
                outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                corner = true;
            }


            if (!corner)
            {
                if (GetSquare(col, row - 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.up * 0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                if (GetSquare(col, row + 1, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.down * 0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                if (GetSquare(col - 1, row, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.left * 0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                if (GetSquare(col + 1, row, true).type != SquareTypes.NONE)
                {
                    GameObject outline = CreateOutline(square);
                    SpriteRenderer spr = outline.GetComponent<SpriteRenderer>();
                    outline.transform.localPosition = Vector3.zero + Vector3.right * 0.395f;
                    outline.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
        //Vector3 pos = GameField.transform.TransformPoint((Vector3)firstSquarePosition + new Vector3(col * squareWidth - squareWidth / 2, -row * squareHeight, 10));
        //line.SetVertexCount(linePoint + 1);
        //line.SetPosition(linePoint++, pos);

    }

    GameObject CreateOutline(Square square)
    {
        GameObject outline = new GameObject();
        outline.name = "outline";
        outline.transform.SetParent(square.transform);
        outline.transform.localPosition = Vector3.zero;
        SpriteRenderer spr = outline.AddComponent<SpriteRenderer>();
        spr.sprite = outline1;
        spr.sortingOrder = 1;
        return outline;
    }
#endif
    void CreateObstacles(int col, int row, GameObject square, SquareTypes type)
    {
        if ((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK && type == SquareTypes.NONE) || type == SquareTypes.WIREBLOCK)
        {
            GameObject block = Instantiate(wireBlockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.WIREBLOCK;
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            //   TargetBlocks++;
        }
        else if ((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK && type == SquareTypes.NONE) || type == SquareTypes.SOLIDBLOCK)
        {
            GameObject block = Instantiate(solidBlockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            square.GetComponent<Square>().type = SquareTypes.SOLIDBLOCK;

            //  TargetBlocks++;
        }
        else if ((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.UNDESTROYABLE && type == SquareTypes.NONE) || type == SquareTypes.UNDESTROYABLE)
        {
            GameObject block = Instantiate(undesroyableBlockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.UNDESTROYABLE;

            //  TargetBlocks++;
        }
        else if ((levelSquaresFile[row * maxCols + col].obstacle == SquareTypes.THRIVING && type == SquareTypes.NONE) || type == SquareTypes.THRIVING)
        {
            GameObject block = Instantiate(thrivingBlockPrefab, firstSquarePosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
            block.transform.SetParent(square.transform);
            block.transform.localPosition = new Vector3(0, 0, -0.5f);
            block.GetComponent<SpriteRenderer>().sortingOrder = 3;
            if (square.GetComponent<Square>().item != null)
                Destroy(square.GetComponent<Square>().item.gameObject);
            square.GetComponent<Square>().block.Add(block);
            square.GetComponent<Square>().type = SquareTypes.THRIVING;

            //   TargetBlocks++;
        }

    }
    //生成敌人
    public void GenerateNewEnemys(bool falling = true)
    {
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = 0; row < opponentRows; row++)
            {
                if (GetSquare(col, row) != null)
                {
                    if (!GetSquare(col, row).IsNone() && GetSquare(col, row).CanGoInto() && GetSquare(col, row).item == null)
                    {
                        if ((GetSquare(col, row).item == null && !GetSquare(col, row).IsHaveSolidAbove()) || !falling)
                        {
                            GetSquare(col, row).GenItem(falling);
                        }
                    }
                }
            }
        }
    }
    //生成士兵
    void GenerateNewItems(bool falling = true)
    {
#if OLD_LOGIC
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = maxRows - 1; row >= 0; row--)
            {
                if (GetSquare(col, row) != null)
                {
                    if (!GetSquare(col, row).IsNone() && GetSquare(col, row).CanGoInto() && GetSquare(col, row).item == null)
                    {
                        if ((GetSquare(col, row).item == null && !GetSquare(col, row).IsHaveSolidAbove()) || !falling)
                        {
                            GetSquare(col, row).GenItem(falling);
                        }
                    }
                }
            }
        }
#else
        int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
        //beginMineRow = 0;
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = beginMineRow; row < maxRows; row++)
            {
                if (GetSquare(col, row) != null)
                {
                    if (!GetSquare(col, row).IsNone() && GetSquare(col, row).CanGoInto() && GetSquare(col, row).item == null)
                    {
                        if ((GetSquare(col, row).item == null && !GetSquare(col, row).IsHaveSolidAbove()) || !falling)
                        {
                            GetSquare(col, row).GenItem(falling);
                        }
                    }
                }
            }
        }
#endif
    }
    public void NoMatches()
    {
        StartCoroutine(NoMatchesCor());
    }

    IEnumerator NoMatchesCor()
    {
        if (gameStatus == GameState.Playing)
        {
            SoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(SoundBase.Instance.noMatch);

            GameObject.Find("Canvas").transform.Find("NoMoreMatches").gameObject.SetActive(true);
            gameStatus = GameState.RegenLevel;
            yield return new WaitForSeconds(1);
            ReGenLevel();
        }
    }

    public void ReGenLevel()
    {
        itemsHided = false;
        DragBlocked = true;
        if (gameStatus != GameState.Playing && gameStatus != GameState.RegenLevel)
            DestroyItems();
        else if (gameStatus == GameState.RegenLevel)
            DestroyItems(true);
        OnLevelLoaded();
        StartCoroutine(RegenMatches());
        OnLevelLoaded();
    }

    IEnumerator RegenMatches(bool onlyFalling = false)
    {
        if (gameStatus == GameState.RegenLevel)
        {
            //while (!itemsHided)
            //{
            yield return new WaitForSeconds(0.5f);
            //}
        }
        if (!onlyFalling)
        {
            GenerateNewItems(false);
            GenerateNewEnemys(false);
        }
        else
            LevelManager.THIS.onlyFalling = true;
        //   yield return new WaitForSeconds(1f);
        yield return new WaitForFixedUpdate();

        List<List<Item>> combs = GetMatches();
        //while (!matchesGot)
        //{
        //    yield return new WaitForFixedUpdate();

        //}
        //combs = newCombines;
        //matchesGot = false;
        do
        {
            foreach (List<Item> comb in combs)
            {
                int colorOffset = 0;
                foreach (Item item in comb)
                {
                    item.GenColor(item.color + colorOffset);
                    colorOffset++;
                }
            }
            combs = GetMatches();
            //while (!matchesGot)
            //{
            //    yield return new WaitForFixedUpdate();

            //}
            //combs = newCombines;
            //matchesGot = false;

            //     yield return new WaitForFixedUpdate();
        } while (combs.Count > 0);
        yield return new WaitForFixedUpdate();
        SetPreBoosts();
        LevelManager.THIS.onlyFalling = false;
        if (gameStatus == GameState.RegenLevel)
            gameStatus = GameState.Playing;
        if (!onlyFalling)//2.1.5 prevents early move
            DragBlocked = false;
        StartCoroutine(AI.THIS.CheckPossibleCombines());//2.1.5 prevents early move

        //StartCoroutine(CheckFallingAtStart());

    }

    void SetPreBoosts()
    {
        if (BoostPackage > 0)
        {
            InitScript.Instance.SpendBoost(BoostType.Packages);
            foreach (Item item in GetRandomItems(BoostPackage))
            {
                item.NextType = ItemsTypes.PACKAGE;
                item.ChangeType();
                item.boost = true;
            }
            BoostPackage = 0;
        }
        if (BoostColorfullBomb > 0)
        {
            InitScript.Instance.SpendBoost(BoostType.Colorful_bomb);
            foreach (Item item in GetRandomItems(BoostColorfullBomb))
            {
                item.NextType = ItemsTypes.BOMB;
                item.ChangeType();
                item.boost = true;
            }
            BoostColorfullBomb = 0;
        }
        if (BoostStriped > 0)
        {
            InitScript.Instance.SpendBoost(BoostType.Stripes);
            foreach (Item item in GetRandomItems(BoostStriped))
            {
                item.NextType = (ItemsTypes)UnityEngine.Random.Range(1, 3);
                item.ChangeType();
                item.boost = true;
            }
            BoostStriped = 0;
        }
    }


    public List<Item> GetRandomItems(int count)
    {
        List<Item> list = new List<Item>();
        List<Item> list2 = new List<Item>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        if (items.Length < count)
            count = items.Length;
        foreach (GameObject item in items)
        {
            if (item.GetComponent<Item>().currentType == ItemsTypes.NONE && item.GetComponent<Item>().NextType == ItemsTypes.NONE && !item.GetComponent<Item>().destroying)
            {
                list.Add(item.GetComponent<Item>());
            }
        }

        while (list2.Count < count)
        {

            try
            {
                Item newItem = list[UnityEngine.Random.Range(0, list.Count)];
                if (list2.IndexOf(newItem) < 0)
                {
                    list2.Add(newItem);
                }
            }
            catch (Exception ex)
            {
                gameStatus = GameState.Win;
            }
        }
        return list2;
    }

    List<Item> GetAllExtaItems()
    {
        List<Item> list = new List<Item>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item.GetComponent<Item>().currentType != ItemsTypes.NONE)
            {
                list.Add(item.GetComponent<Item>());
            }
        }

        return list;
    }

    public List<Item> GetIngredients(int i)
    {
        List<Item> list = new List<Item>();
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item.GetComponent<Item>().currentType == ItemsTypes.INGREDIENT && item.GetComponent<Item>().color == 1000 + (int)LevelManager.THIS.ingrTarget[i])
            {
                list.Add(item.GetComponent<Item>());
            }
        }
        return list;
    }


    public void DestroyItems(bool withoutEffects = false)
    {

        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item != null)
            {
                if (item.GetComponent<Item>().currentType != ItemsTypes.INGREDIENT && item.GetComponent<Item>().currentType == ItemsTypes.NONE)
                {
                    if (!withoutEffects)
                        item.GetComponent<Item>().DestroyItem();
                    else
                        item.GetComponent<Item>().SmoothDestroy();
                }
            }
        }

    }

    public IEnumerator FindMatchDelay()
    {
        yield return new WaitForSeconds(0.2f);
        LevelManager.THIS.FindMatches();

    }

    public void FindMatches()
    {
        StartCoroutine(FallingDown());
    }

    public List<List<Item>> GetMatches(FindSeparating separating = FindSeparating.NONE, int matches = 3)
    {
        newCombines = new List<List<Item>>();
        //       List<Item> countedSquares = new List<Item>();
        countedSquares = new Hashtable();
        countedSquares.Clear();
        int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = beginMineRow; row < maxRows; row++)
            {
                if (GetSquare(col, row) != null)
                {
                    if (!countedSquares.ContainsValue(GetSquare(col, row).item))
                    {
                        List<Item> newCombine = GetSquare(col, row).FindMatchesAround(separating, matches, countedSquares);
                        if (newCombine.Count >= matches)
                            newCombines.Add(newCombine);
                    }
                }
            }
        }
        //print("global " + countedSquares.Count);
        //  Debug.Break();
        return newCombines;
    }

    IEnumerator GetMatchesCor(FindSeparating separating = FindSeparating.NONE, int matches = 3, bool Smooth = true)
    {
        Hashtable countedSquares = new Hashtable();
        int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
        for (int col = 0; col < maxCols; col++)
        {
            //if (Smooth)
            //                    yield return new WaitForFixedUpdate();
            for (int row = beginMineRow; row < maxRows; row++)
            {

                if (GetSquare(col, row) != null)
                {
                    if (!countedSquares.ContainsValue(GetSquare(col, row).item))
                    {
                        List<Item> newCombine = GetSquare(col, row).FindMatchesAround(separating, matches, countedSquares);
                        if (newCombine.Count >= matches)
                            newCombines.Add(newCombine);
                    }
                }
            }
        }
        matchesGot = true;
        yield return new WaitForFixedUpdate();

    }

    IEnumerator CheckFallingAtStart()
    {
        yield return new WaitForSeconds(0.5f);
        while (!IsAllItemsFallDown())
        {
            yield return new WaitForSeconds(0.1f);
        }
        FindMatches();
    }

    public bool CheckExtraPackage(List<List<Item>> rowItems)
    {
        //     print("set package");
        foreach (List<Item> items in rowItems)
        {
            foreach (Item item in items)
            {
                if (item.square.FindMatchesAround(FindSeparating.VERTICAL).Count > 2)
                {
                    if (LevelManager.THIS.lastDraggedItem == null)
                        LevelManager.THIS.lastDraggedItem = item;
                    LevelManager.THIS.latstMatchColor = item.color;
                    //           print(LevelManager.THIS.latstMatchColor);
                    return true;
                }
            }
        }
        return false;
    }
    void fallingDownSolider()
    {
        //falling down solider
        int beginMineRow = opponentRows + spaceRows;

        for (int i = 0; i < 20; i++)
        {   //just for testing
            for (int col = 0; col < maxCols; col++)
            {
                for (int row = maxRows - 1; row >= beginMineRow; row--)
                {   //need to enumerate rows from bottom to top
                    if (GetSquare(col, row) != null)
                        GetSquare(col, row).FallOut();
                }
            }
            // yield return new WaitForFixedUpdate();
        }
    }

    void fallingDownEnemy()
    {
        //falling down solider
        int beginMineRow = 0;

        for (int i = 0; i < 20; i++)
        {   //just for testing
            for (int col = 0; col < maxCols; col++)
            {
                for (int row = opponentRows - 1; row >= beginMineRow; row--)
                {   //need to enumerate rows from bottom to top
                    if (GetSquare(col, row) != null)
                        GetSquare(col, row).FallOut();
                }
            }
            // yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator FallingDown()
    {
        bool nearEmptySquareDetected = false;
        int combo = 0;
        AI.THIS.allowShowTip = false;
        List<Item> it = GetItems();
        for (int i = 0; i < it.Count; i++)
        {
            Item item = it[i];
            if (item != null)
            {
                //AI.THIS.StopAllCoroutines();
                item.anim.StopPlayback();
            }
        }
        //        combineManager.GetCombine();

        while (true)
        {

            //find matches
            yield return new WaitForSeconds(0.1f);

            combinedItems.Clear();
            combinedItems = combineManager.GetCombineItems(); //GetMatches();  //1.6
                                                              //StartCoroutine(GetMatchesCor());
                                                              //while (!matchesGot)
                                                              //    yield return new WaitForFixedUpdate();
                                                              //combinedItems = newCombines;
                                                              //matchesGot = false;
                                                              //   print(LevelManager.THIS.latstMatchColor);
                                                              //			if (LevelManager.THIS.CheckExtraPackage (GetMatches (FindSeparating.HORIZONTAL)) && lastSwitchedItem != null) {
                                                              //
                                                              //				if (LevelManager.THIS.latstMatchColor == lastDraggedItem.color && LevelManager.THIS.lastDraggedItem.NextType == ItemsTypes.NONE)
                                                              //					LevelManager.THIS.lastDraggedItem.NextType = ItemsTypes.PACKAGE;
                                                              //				else if (LevelManager.THIS.latstMatchColor == lastSwitchedItem.color && LevelManager.THIS.lastDraggedItem.NextType == ItemsTypes.NONE)
                                                              //					LevelManager.THIS.lastSwitchedItem.NextType = ItemsTypes.PACKAGE;
                                                              //				lastDraggedItem.ChangeType ();
                                                              //				lastSwitchedItem.ChangeType ();
                                                              //
                                                              //			}
#if OLD_LOGIC
            if (combinedItems.Count > 0)
                combo++;

            foreach (List<Item> desrtoyItems in combinedItems)
            {

                //				if (lastDraggedItem == null) {  //1.6
                //					if (desrtoyItems.Count == 4) {
                //						if (lastDraggedItem == null)
                //							lastDraggedItem = desrtoyItems [UnityEngine.Random.Range (0, desrtoyItems.Count)];
                //						lastDraggedItem.NextType = (ItemsTypes)UnityEngine.Random.Range (1, 3);
                //						//lastDraggedItem.ChangeType();
                //					}
                //					if (desrtoyItems.Count >= 5) {
                //						if (lastDraggedItem == null)
                //							lastDraggedItem = desrtoyItems [UnityEngine.Random.Range (0, desrtoyItems.Count)];
                //						lastDraggedItem.NextType = ItemsTypes.BOMB;
                //						//lastDraggedItem.ChangeType();
                //					}
                //
                //				}
                // if (desrtoyItems.Count > 0) PopupScore(scoreForItem * desrtoyItems.Count, desrtoyItems[(int)desrtoyItems.Count / 2].transform.position, Color.black);
                foreach (Item item in desrtoyItems)
                {//TODO items not destroy

                    if (item.currentType != ItemsTypes.NONE)
                        yield return new WaitForSeconds(0.1f);
                    item.DestroyItem(true);  //destroy items safely
                    if (item.currentType != ItemsTypes.NONE)
                    {
                        //while (!item.animationFinished)
                        //{
                        //    yield return new WaitForFixedUpdate();
                        //}
                    }

                }
            }
#else
            yield return StartCoroutine(BattleSystem.Instance.onCombine(combineManager.GetCombines()));
#endif
            foreach (Item item in destroyAnyway)
            {
                //  if(item.sprRenderer.enabled)
                item.DestroyItem(true, "", true);  //destroy items safely
                                                   //yield return new WaitForSeconds(0.2f);
            }
            //          if (destroyAnyway.Count > 0) PopupScore(scoreForItem * destroyAnyway.Count, destroyAnyway[(int)destroyAnyway.Count / 2].transform.position);
            destroyAnyway.Clear();

            if (lastDraggedItem != null)
            {
                //
                //				if (LevelManager.THIS.CheckExtraPackage (GetMatches (FindSeparating.HORIZONTAL))) {  //1.6
                //					if (LevelManager.THIS.latstMatchColor == lastDraggedItem.color)
                //						LevelManager.THIS.lastDraggedItem.NextType = ItemsTypes.PACKAGE;
                //					else if (LevelManager.THIS.latstMatchColor == lastSwitchedItem.color)
                //						LevelManager.THIS.lastSwitchedItem.NextType = ItemsTypes.PACKAGE;
                //					lastDraggedItem.ChangeType ();
                //					lastSwitchedItem.ChangeType ();
                //
                //				}
                if (lastDraggedItem.NextType != ItemsTypes.NONE)
                {
                    //lastDraggedItem.ChangeType();
                    yield return new WaitForSeconds(0.5f);

                }
                lastDraggedItem = null;
            }

            while (!IsAllDestoyFinished())
            {
                yield return new WaitForSeconds(0.1f);
            }

            //falling down solider
            fallingDownSolider();
            if (!nearEmptySquareDetected)
                yield return new WaitForSeconds(0.2f);

            CheckIngredient();
#if OLD_LOGIC
            for (int col = 0; col < maxCols; col++)
            {
                for (int row = maxRows - 1; row >= 0; row--)
                {
                    if (GetSquare(col, row) != null)
                    {
                        if (!GetSquare(col, row).IsNone())
                        {
                            if (GetSquare(col, row).item != null)
                            {
                                GetSquare(col, row).item.StartFalling();
                                //if (row == maxRows - 1 && GetSquare(col, row).item.currentType == ItemsTypes.INGREDIENT)
                                //{
                                //    destroyAnyway.Add(GetSquare(col, row).item);
                                //}
                            }
                        }
                    }
                }
            }
#else
            int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
            for (int col = 0; col < maxCols; col++)
            {
                for (int row = beginMineRow; row < maxRows; ++row)
                {
                    if (GetSquare(col, row) != null)
                    {
                        if (!GetSquare(col, row).IsNone())
                        {
                            if (GetSquare(col, row).item != null)
                            {
                                GetSquare(col, row).item.StartFalling();
                                //if (row == maxRows - 1 && GetSquare(col, row).item.currentType == ItemsTypes.INGREDIENT)
                                //{
                                //    destroyAnyway.Add(GetSquare(col, row).item);
                                //}
                            }
                        }
                    }
                }
            }

#endif
            yield return new WaitForSeconds(0.2f);
            GenerateNewItems();
            StartCoroutine(RegenMatches(true));
            yield return new WaitForSeconds(0.1f);
            while (!IsAllSoliderItemsFallDown())
            {
                yield return new WaitForSeconds(0.1f);
            }

            //detect near empty squares to fall into
            nearEmptySquareDetected = false;

            for (int col = 0; col < maxCols; col++)
            {
                for (int row = maxRows - 1; row >= 0; row--)
                {
                    if (GetSquare(col, row) != null)
                    {
                        if (!GetSquare(col, row).IsNone())
                        {
                            if (GetSquare(col, row).item != null)
                            {
                                if (GetSquare(col, row).item.GetNearEmptySquares())
                                    nearEmptySquareDetected = true;

                            }
                        }
                    }
                    // if (nearEmptySquareDetected) break;
                }
                //   if (nearEmptySquareDetected) break;
            }
            //StartCoroutine(GetMatchesCor());
            //while (!matchesGot)
            //    yield return new WaitForFixedUpdate();
            //matchesGot = false;
            //CheckIngredient();
            while (!IsAllSoliderItemsFallDown())
            {//2.0
                yield return new WaitForSeconds(0.1f);
            }

            if (destroyAnyway.Count > 0)
                nearEmptySquareDetected = true;
            if (GetMatches().Count <= 0 && !nearEmptySquareDetected)
                break;
        }

        List<Item> item_ = GetItems();
        for (int i = 0; i < item_.Count; i++)//2.1.5 
        {
            Item item1 = item_[i];
            if (item1 != null)
            {
                if (item1 != item1.square.item)
                {
                    Destroy(item1.gameObject);
                }
            }
        }

        //thrive thriving blocks
        if (!thrivingBlockDestroyed)
        {
            bool thrivingBlockSelected = false;
            for (int col = 0; col < maxCols; col++)
            {
                if (thrivingBlockSelected)
                    break;
                for (int row = maxRows - 1; row >= 0; row--)
                {
                    if (thrivingBlockSelected)
                        break;
                    if (GetSquare(col, row) != null)
                    {
                        if (GetSquare(col, row).type == SquareTypes.THRIVING)
                        {
                            List<Square> sqList = GetSquare(col, row).GetAllNeghbors();
                            foreach (Square sq in sqList)
                            {
                                if (sq.CanGoInto() && UnityEngine.Random.Range(0, 1) == 0 && sq.type == SquareTypes.EMPTY)
                                {
                                    if (sq.item != null)
                                    {//1.6.1
                                        if (sq.item.currentType == ItemsTypes.NONE)
                                        {//1.6.1
                                         //GetSquare(col, row).GenThriveBlock(sq);
                                            CreateObstacles(sq.col, sq.row, sq.gameObject, SquareTypes.THRIVING);

                                            thrivingBlockSelected = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        thrivingBlockDestroyed = false;

        if (gameStatus == GameState.Playing && !ingredientFly)
            LevelManager.THIS.CheckWinLose();
        //如果一次消除的组合数>2，则提示夸赞语句、音效
        if (combo > 2 && gameStatus == GameState.Playing)
        {
            gratzWords[UnityEngine.Random.Range(0, gratzWords.Length)].SetActive(true);
            combo = 0;
        }
        LevelManager.THIS.latstMatchColor = -1;
        CheckItemsPositions();//1.6.1
        DragBlocked = false;//2.1.5 prevents early move
        if (gameStatus == GameState.Playing)
            StartCoroutine(AI.THIS.CheckPossibleCombines());




    }

    void CheckItemsPositions()
    {//1.6.1
        List<Item> items = GetItems();
        foreach (var item in items)
        {
            if (item)
                item.transform.position = item.square.transform.position + Vector3.back * 0.2f;
        }
    }


    public void DestroyDoubleBomb(int col)
    {
        StartCoroutine(DestroyDoubleBombCor(col));
        StartCoroutine(DestroyDoubleBombCorBack(col));
    }

    IEnumerator DestroyDoubleBombCor(int col)
    {
        for (int i = col; i < maxCols; i++)
        {
            List<Item> list = GetColumn(i);
            foreach (Item item in list)
            {
                if (item != null)
                    item.DestroyItem(true, "", true);
            }
            yield return new WaitForSeconds(0.3f);
            //GenerateNewItems();
            //yield return new WaitForSeconds(0.3f);
        }
        if (col <= maxCols - col - 1)
            FindMatches();
    }

    IEnumerator DestroyDoubleBombCorBack(int col)
    {
        for (int i = col - 1; i >= 0; i--)
        {
            List<Item> list = GetColumn(i);
            foreach (Item item in list)
            {
                if (item != null)
                    item.DestroyItem(true, "", true);
            }
            yield return new WaitForSeconds(0.3f);
            //GenerateNewItems();
            //yield return new WaitForSeconds(0.3f);
        }
        if (col > maxCols - col - 1)
            FindMatches();
    }


    public Square GetSquare(int col, int row, bool safe = false)
    {
        if (!safe)
        {
            if (row >= maxRows || col >= maxCols)
                return null;
            return squaresArray[row * maxCols + col];
        }
        else
        {
            row = Mathf.Clamp(row, 0, maxRows - 1);
            col = Mathf.Clamp(col, 0, maxCols - 1);
            return squaresArray[row * maxCols + col];
        }
    }

    bool IsAllDestoyFinished()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent == null)
            {
                return false;
            }
            if (itemComponent.destroying && !itemComponent.animationFinished)
                return false;
        }
        return true;
    }

    bool IsIngredientFalling()
    {//1.6.1
        if (gameStatus == GameState.PreWinAnimations)
            return true;
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent != null)
            {
                if (itemComponent.falling && itemComponent.currentType == ItemsTypes.INGREDIENT)
                    return true;
            }
        }
        return false;

    }

    bool IsAllItemsFallDown()
    {
        if (gameStatus == GameState.PreWinAnimations)
            return true;
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent == null)
            {
                return false;
            }
            if (itemComponent.falling)
                return false;
        }
        return true;
    }
    bool IsAllSoliderItemsFallDown()
    {
        if (gameStatus == GameState.PreWinAnimations)
            return true;
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent == null)
            {
                return false;
            }
            if (itemComponent.square.row > opponentRows)
            {
                if (itemComponent.falling)
                    return false;
            }

        }
        return true;
    }
    public Vector2 GetPosition(Item item)
    {
        return GetPosition(item.square);
    }

    public Vector2 GetPosition(Square square)
    {
        return new Vector2(square.col, square.row);
    }

    public List<Item> GetRow(int row)
    {
        List<Item> itemsList = new List<Item>();
        for (int col = 0; col < maxCols; col++)
        {
            itemsList.Add(GetSquare(col, row, true).item);
        }
        return itemsList;
    }

    public List<Item> GetColumn(int col)
    {
        List<Item> itemsList = new List<Item>();
        for (int row = 0; row < maxRows; row++)
        {
            itemsList.Add(GetSquare(col, row, true).item);
        }
        return itemsList;
    }

    public List<Square> GetColumnSquaresObstacles(int col)
    {
        List<Square> itemsList = new List<Square>();
        for (int row = 0; row < maxRows; row++)
        {
            if (GetSquare(col, row, true).IsHaveDestroybleObstacle())
                itemsList.Add(GetSquare(col, row, true));
        }
        return itemsList;
    }

    public List<Square> GetRowSquaresObstacles(int row)
    {
        List<Square> itemsList = new List<Square>();
        for (int col = 0; col < maxCols; col++)
        {
            if (GetSquare(col, row, true).IsHaveDestroybleObstacle())
                itemsList.Add(GetSquare(col, row, true));
        }
        return itemsList;
    }

    public List<Item> GetItemsAround(Square square)
    {
        int col = square.col;
        int row = square.row;
        List<Item> itemsList = new List<Item>();
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                itemsList.Add(GetSquare(c, r, true).item);
            }
        }
        return itemsList;
    }

    public List<Item> GetItemsCross(Square square, List<Item> exceptList = null, int COLOR = -1)
    {
        if (exceptList == null)
            exceptList = new List<Item>();
        int c = square.col;
        int r = square.row;
        List<Item> itemsList = new List<Item>();
        Item item = null;
        item = GetSquare(c - 1, r, true).item;
        if (exceptList.IndexOf(item) <= -1)
        {
            if (item.color == COLOR || COLOR == -1)
                itemsList.Add(item);
        }
        item = GetSquare(c + 1, r, true).item;
        if (exceptList.IndexOf(item) <= -1)
        {
            if (item.color == COLOR || COLOR == -1)
                itemsList.Add(item);
        }
        item = GetSquare(c, r - 1, true).item;
        if (exceptList.IndexOf(item) <= -1)
        {
            if (item.color == COLOR || COLOR == -1)
                itemsList.Add(item);
        }
        item = GetSquare(c, r + 1, true).item;
        if (exceptList.IndexOf(item) <= -1)
        {
            if (item.color == COLOR || COLOR == -1)
                itemsList.Add(item);
        }

        return itemsList;
    }

    public List<Item> GetItems()
    {
        List<Item> itemsList = new List<Item>();
        int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
        beginMineRow = 0;
        for (int row = beginMineRow; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if (GetSquare(col, row) != null)
                    itemsList.Add(GetSquare(col, row, true).item);
            }
        }
        return itemsList;
    }

    public List<Item> GetEnemyItems()
    {
        List<Item> itemsList = new List<Item>();
        for (int row = 0; row < opponentRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                Square square = GetSquare(col, row);
                if (square != null && square.item != null)
                    itemsList.Add(square.item);
            }
        }
        return itemsList;
    }
    public void SetTypeByColor(int p, ItemsTypes nextType)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item.GetComponent<Item>().color == p)
            {
                if (nextType == ItemsTypes.HORIZONTAL_STRIPPED || nextType == ItemsTypes.VERTICAL_STRIPPED)
                    item.GetComponent<Item>().NextType = (ItemsTypes)UnityEngine.Random.Range(1, 3);
                else
                    item.GetComponent<Item>().NextType = nextType;

                item.GetComponent<Item>().ChangeType();
                if (nextType == ItemsTypes.NONE)
                    destroyAnyway.Add(item.GetComponent<Item>());
            }
        }
    }

    public void CheckIngredient()
    {
        int row = maxRows;
        List<Square> sqList = GetBottomRow();
        foreach (Square sq in sqList)
        {
            if (sq.item != null)
            {
                if (sq.item.currentType == ItemsTypes.INGREDIENT)
                {
                    destroyAnyway.Add(sq.item);
                }
            }
        }
    }

    public List<Square> GetBottomRow()
    {
        List<Square> itemsList = new List<Square>();
        int listCounter = 0;
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = maxRows - 1; row >= 0; row--)
            {
                Square square = GetSquare(col, row, true);
                if (square.type != SquareTypes.NONE)
                {
                    itemsList.Add(square);
                    listCounter++;
                    break;
                }
            }
        }
        return itemsList;
    }

    IEnumerator StartIdleCor()
    {
        int beginMineRow = LevelManager.THIS.opponentRows + LevelManager.THIS.spaceRows;
        for (int col = 0; col < maxCols; col++)
        {
            for (int row = beginMineRow; row < maxRows; row++)
            {
                // GetSquare(col, row, true).item.anim.SetBool("stop", false);
                if (GetSquare(col, row, true).item != null)
                    GetSquare(col, row, true).item.StartIdleAnim();
            }

        }

        yield return new WaitForFixedUpdate();
    }

    public void StrippedShow(GameObject obj, bool horrizontal)
    {
        GameObject effect = Instantiate(stripesEffect, obj.transform.position, Quaternion.identity) as GameObject;
        if (!horrizontal)
            effect.transform.Rotate(Vector3.back, 90);
        Destroy(effect, 1);
    }

    public void PopupScore(int value, Vector3 pos, int color)
    {
        Score += value;
        if (showPopupScores)
        {
            Transform parent = GameObject.Find("CanvasScore").transform;
            GameObject poptxt = Instantiate(popupScore, pos, Quaternion.identity) as GameObject;
            poptxt.transform.GetComponentInChildren<Text>().text = "" + value;
            if (color <= scoresColors.Length - 1)
            {
                poptxt.transform.GetComponentInChildren<Text>().color = scoresColors[color];
                poptxt.transform.GetComponentInChildren<Outline>().effectColor = scoresColorsOutline[color];
            }
            poptxt.transform.SetParent(parent);
            //   poptxt.transform.position += Vector3.right * 1;
            poptxt.transform.localScale = Vector3.one / 1.5f;
            Destroy(poptxt, 0.3f);
        }
    }


    public void LoadDataFromLocal(int currentLevel)
    {
        levelLoaded = false;
        //Read data from text file
        TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        if (mapText == null)
        {
            mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
        }
        ProcessGameDataFromString(mapText.text);
        GameData.Instance.init();
#if false
        //加载怪物生成数据
        TextAsset levelText = Resources.Load("level_" + currentLevel) as TextAsset;
        //Debug.Log("levelText = " + levelText + "; levelText.text = " + levelText.text);
        levelConfig = JsonConvert.DeserializeObject<CLevelConfig>(levelText.text);
        //Debug.Log("levelConfig = " + levelConfig);
#endif
    }

    void ProcessGameDataFromString(string mapText)
    {
        string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        int mapLine = 0;
        foreach (string line in lines)
        {
            //check if line is game mode line
            if (line.StartsWith("MODE"))
            {
                //Replace GM to get mode number, 
                string modeString = line.Replace("MODE", string.Empty).Trim();
                //then parse it to interger
                target = (Target)int.Parse(modeString);
                //Assign game mode
            }
            else if (line.StartsWith("SIZE "))
            {
                string blocksString = line.Replace("SIZE", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                maxCols = int.Parse(sizes[0]);
                maxRows = int.Parse(sizes[1]);
                if (sizes.Length >= 4)
                {
                    opponentRows = int.Parse(sizes[2]);
                    spaceRows = int.Parse(sizes[3]);
                    mineRows = maxRows - opponentRows - spaceRows;
                }
                else
                {
                    opponentRows = spaceRows = mineRows = 0;
                }
                squaresArray = new Square[maxCols * maxRows];
                levelSquaresFile = new SquareBlocks[maxRows * maxCols];
                for (int i = 0; i < levelSquaresFile.Length; i++)
                {

                    SquareBlocks sqBlocks = new SquareBlocks();
                    sqBlocks.block = SquareTypes.EMPTY;
                    sqBlocks.obstacle = SquareTypes.NONE;

                    levelSquaresFile[i] = sqBlocks;
                }
            }
            else if (line.StartsWith("LIMIT"))
            {
                string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                limitType = (LIMIT)int.Parse(sizes[0]);
                Limit = int.Parse(sizes[1]);
            }
            else if (line.StartsWith("COLOR LIMIT "))
            {
                string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                colorLimit = int.Parse(blocksString);
            }

            //check third line to get missions
            else if (line.StartsWith("STARS"))
            {
                string blocksString = line.Replace("STARS", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                star1 = int.Parse(blocksNumbers[0]);
                star2 = int.Parse(blocksNumbers[1]);
                star3 = int.Parse(blocksNumbers[2]);
                if (ProgressBarScript.Instance != null)//2.1.2
                    ProgressBarScript.Instance.InitBar();//2.1.2
            }
            else if (line.StartsWith("COLLECT COUNT "))
            {
                string blocksString = line.Replace("COLLECT COUNT", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < blocksNumbers.Length; i++)
                {
                    ingrCountTarget[i] = int.Parse(blocksNumbers[i]);
                }
            }
            else if (line.StartsWith("COLLECT ITEMS "))
            {
                string blocksString = line.Replace("COLLECT ITEMS", string.Empty).Trim();
                string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < blocksNumbers.Length; i++)
                {
                    if (target == Target.INGREDIENT)
                        ingrTarget[i] = (Ingredients)int.Parse(blocksNumbers[i]);
                    else if (target == Target.COLLECT)
                        collectItems[i] = (CollectItems)int.Parse(blocksNumbers[i]);

                }
            }
            else
            { //Maps
              //Split lines again to get map numbers
                string[] st = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < st.Length; i++)
                {
                    levelSquaresFile[mapLine * maxCols + i].block = (SquareTypes)int.Parse(st[i][0].ToString());
                    levelSquaresFile[mapLine * maxCols + i].obstacle = (SquareTypes)int.Parse(st[i][1].ToString());
                }
                mapLine++;
            }
        }
        levelLoaded = true;
    }

}

[System.Serializable]
public class GemProduct
{
    public int count;
    public float price;
}