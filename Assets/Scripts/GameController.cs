using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Scripts.Manager;
using Core.Scripts;
using Core.Scripts.Table;
using DG.Tweening;
using UnityEngine;

public interface ISnowmanData
{
    int id { get; }
    string upperbody_animator_name { get; }
    string fullbody_animator_name { get; }  
}

public struct ProductionData
{
    public int totalQuantity;
    public int normalQuantity;
    public int defectQuantity;
    public int spyQuantity;
    
    public void Initialize(int goal)
    {
        totalQuantity = goal * 2;
        normalQuantity = (int)(totalQuantity * 0.6f);
        defectQuantity = (int)(totalQuantity * 0.3f);
        spyQuantity = (int)(totalQuantity * 0.1f);
    }
}

public struct ProductionPeriodData
{
    public int productionEarlyEnd;
    public int productionMidEnd;

    public float averagePeriod;
    public float earlyPeriod;
    public float midPeriod;
    public float latePeriod;

    public void SetProductionFlow(int totalQuantity, float totalTime)
    {
        productionEarlyEnd = (int)(totalQuantity * 0.4f);
        productionMidEnd = (int)(totalQuantity * 0.8f);

        averagePeriod = totalTime / totalQuantity;
        earlyPeriod = averagePeriod * 2.5f;
        midPeriod = averagePeriod * 1.67f;
        latePeriod = averagePeriod * 1.0f;
    }
    
    public float GetSpawnInterval(int currentIndex)
    {
        if (currentIndex <= productionEarlyEnd) return earlyPeriod * 0.2f;
        if (currentIndex <= productionMidEnd) return midPeriod * 0.2f;
        return latePeriod * 0.2f;
    }

    public float GetMoveDuration(int currentIndex)
    {
        if (currentIndex <= productionEarlyEnd) return earlyPeriod * 0.8f;
        if (currentIndex <= productionMidEnd) return midPeriod * 0.8f;
        return latePeriod * 0.8f;
    }
}

public enum SnowmanType
{
    Normal = 0,
    Defect = 1,
    Spy = 2,
    Invalid = 3,
}

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject snowmanPrefab = null; 
        
    [SerializeField] private Animator animatorSnowmanUpperbody = null;
    
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private Transform arrivalPoint = null;
    
    [SerializeField] private GimmickPress gimmickPress = null;
    [SerializeField] private GimmickFire gimmickFire = null;
    
    [SerializeField] private Animator animatorRail = null;

    [SerializeField] private UIMain mainUI = null;

    [SerializeField] private float defectRate = 0.5f;

    private float _totalTime = 0.0f;
    private float _currentTime = 0.0f;
    private bool _isPlaying = false;
    private int _score = 0;
    
    private Snowman _currentSnowman = null;
    
    private Queue<SnowmanType> _snowmanSpawnQueue = new Queue<SnowmanType>();
    private ProductionData _productionData = new ProductionData();
    private ProductionPeriodData _productionPeriodData = new ProductionPeriodData();
    private MissionTdData _currentMissionData = null;

    private bool _isProcessingGimmick = false;
    
    public event Action<SnowmanType> OnSnowmanSpawnEvent = null;
    public event Action<SnowmanType> OnSnowmanArrivalEvent = null;
    public event Action<SnowmanType> OnSnowmanDeathEvent = null; 
    
    public void Kill()
    {
        if (_isProcessingGimmick || _currentSnowman == null) return;
        
        // TODO : Random Gimmick Activation
        _isProcessingGimmick = true;
        gimmickPress.Activate();

        //Sonud
        Managers.Sound.Play("SFX_press", Define.ESoundType.SFX, volume: 1f);
    }

    public void Pass()
    {
        if (_isProcessingGimmick || _currentSnowman == null) return;
        
        _isProcessingGimmick = true;
        _currentSnowman.Stop();
        _currentSnowman.MoveImmediate();

        //Sonud
        Managers.Sound.Play("SFX_button", Define.ESoundType.SFX, volume: 2f);
        Managers.Sound.Play("SFX_swoosh", Define.ESoundType.SFX, volume: 1f);
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        CreateRandomMission();
        StartCoroutine(SpawnSnowman());
    }

    private void Update()
    {
        if (_isPlaying)
        {
            _currentTime -= Time.deltaTime;
            var elapsedRatio = 1.0f - (_currentTime / _totalTime);
            mainUI.SetTimerSlider(elapsedRatio);
            if (_currentTime < 0.0f)
            {
                GameOver();
            }
        }
    }

    private void Init()
    {
        gimmickPress.Init();
        gimmickPress.OnStartActivateEvent += () =>
        {
            _currentSnowman.Stop();
        };
        gimmickPress.OnFinishActivateEvent += () =>
        {
            animatorSnowmanUpperbody.runtimeAnimatorController = null;
            OnSnowmanDeathEvent?.Invoke(_currentSnowman.Type);
            _currentSnowman.Die();
        };
        gimmickPress.OnStartDeactivateEvent += () =>
        {
            
        };
        gimmickPress.OnFinishDeactivateEvent += () =>
        {
            _isProcessingGimmick = false;
            StartCoroutine(SpawnSnowman());
        };

        
        OnSnowmanSpawnEvent += (type) =>
        {
            HandleSnowmanSpawn(type);
        };
        OnSnowmanArrivalEvent += (type) =>
        {
            HandleSnowmanArrival(type);
            
            _isProcessingGimmick = false;
            StartCoroutine(SpawnSnowman());
        };
        OnSnowmanDeathEvent += (type) =>
        {
            HandleSnowmanDeath(type);
        };
    }

    private void Release()
    {
        _score = 0;
        _isProcessingGimmick = false;
    }
    
    private void GameOver()
    {
        Debug.LogError("GameOver");
        _isPlaying = false;
    }
    
    private void CreateRandomMission()
    {
        var idCount = Tables.Instance.GetTable<MissionTd>().GetKeys.Count();
        var id = UnityEngine.Random.Range(0, idCount);
        _currentMissionData = Tables.Instance.GetTable<MissionTd>().GetData(id);
        if (_currentMissionData == null) return;
        
        Debug.LogWarning($"Start Mission ID : {_currentMissionData.id}, Goal : {_currentMissionData.goal}, Time : {_currentMissionData.time}");
        
        mainUI.SetScore(0, _currentMissionData.goal);
        mainUI.SetTimerSlider(0.0f);
        
        _totalTime = _currentTime = _currentMissionData.time;
        _isPlaying = true;
        
        GenerateSpawnQueue(_currentMissionData);
    }
    
    private void GenerateSpawnQueue(MissionTdData missionData)
    {
        _productionData.Initialize(missionData.goal);
    
        var typeList = CreateSnowmanTypeList(_productionData);
    
        ShuffleList(typeList);

        SetSpawnQueue(typeList);

        _productionPeriodData.SetProductionFlow(_productionData.totalQuantity, missionData.time);
    }

    private List<SnowmanType> CreateSnowmanTypeList(ProductionData data)
    {
        var list = new List<SnowmanType>();
        for (var i = 0; i < data.normalQuantity; i++) list.Add(SnowmanType.Normal);
        for (var i = 0; i < data.defectQuantity; i++) list.Add(SnowmanType.Defect);
        for (var i = 0; i < data.spyQuantity; i++) list.Add(SnowmanType.Spy);
        return list;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var rnd = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
    
    private void SetSpawnQueue(List<SnowmanType> list)
    {
        _snowmanSpawnQueue.Clear();
        foreach (var type in list)
        {
            _snowmanSpawnQueue.Enqueue(type);
        }
    }

    private IEnumerator SpawnSnowman()
    {
        if (_snowmanSpawnQueue.Count <= 0 || !_isPlaying)
        {
            Debug.Log("모든 눈사람을 검수했거나, 게임이 종료되었습니다.");
            GameOver();
            yield break;
        }
        
        var idx = _productionData.totalQuantity - _snowmanSpawnQueue.Count;
        UpdateRailSpeed(idx);
        var moveDuration = _productionPeriodData.GetMoveDuration(idx);
        var spawnInterval = _productionPeriodData.GetSpawnInterval(idx);

        yield return new WaitForSeconds(spawnInterval);
        
        var type = _snowmanSpawnQueue.Dequeue();
        var data = GetSnowmanDataByType(type);
        
        SetupSnowmanAnimator(data);
        
        _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
        _currentSnowman.Init(data, type);
        _currentSnowman.Move(arrivalPoint.position, moveDuration, () =>
        {
            OnSnowmanArrivalEvent?.Invoke(type);
        });
    }
    
    private ISnowmanData GetSnowmanDataByType(SnowmanType type)
    {
        switch (type)
        {
            case SnowmanType.Normal:
                var pTable = Tables.Instance.GetTable<PSnowmanTd>();
                return GetRandomFromList(pTable.DataList);

            case SnowmanType.Spy:
                // var spyTable = Tables.Instance.GetTable<SpySnowmanTd>();
                // return GetRandomFromList(spyTable.DataList);
            case SnowmanType.Defect:
                var npTable = Tables.Instance.GetTable<NPSnowmanTd>();
                return GetRandomFromList(npTable.DataList);
            default:
                return null;
        }
    }
    
    private T GetRandomFromList<T>(List<T> list) where T : ISnowmanData
    {
        if (list == null || list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    
    private void SetupSnowmanAnimator(ISnowmanData data)
    {
        var path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}_000";
        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            animatorSnowmanUpperbody.runtimeAnimatorController = controller;
        }
    }

    private void UpdateRailSpeed(int idx)
    {
        var standardDuration = _productionPeriodData.GetMoveDuration(0);
        var currentDuration = _productionPeriodData.GetMoveDuration(idx);
        
        var playRate = standardDuration / currentDuration;

        if (animatorRail)
        {
            animatorRail.speed = playRate;
            Debug.Log($"Rail Speed Updated : {playRate}, Index : {idx}, standardDuration : {standardDuration}, currentDuration : {currentDuration}");
        }
    }
    
    private void HandleSnowmanSpawn(SnowmanType type)
    {
        
    }

    private void HandleSnowmanArrival(SnowmanType type)
    {
        switch (type)
        {
            case SnowmanType.Normal:
                mainUI.SetScore(++_score, _currentMissionData.goal);
                Managers.Sound.Play("SFX_santa", Define.ESoundType.SFX, volume: 1f);
                break;
            case SnowmanType.Defect:
                Managers.Sound.Play("SFX_wrong", Define.ESoundType.SFX, volume: 1f);
                break;
            case SnowmanType.Spy:
                Managers.Sound.Play("SFX_spy", Define.ESoundType.SFX, volume: 1f);
                break;
            case SnowmanType.Invalid:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    private void HandleSnowmanDeath(SnowmanType type)
    {
        
    }

    private void OnDrawGizmos() 
    {
        if (spawnPoint == null || arrivalPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPoint.position, 30.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(arrivalPoint.position, 30.0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(spawnPoint.position, arrivalPoint.position);
    }
}