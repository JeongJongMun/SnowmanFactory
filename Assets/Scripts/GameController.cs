using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private UIMain mainUI = null;

    [SerializeField] private float defectRate = 0.5f;

    private float _totalTime = 0.0f;
    private float _currentTime = 0.0f;
    private bool _isPlaying = false;
    
    private float _railAnimationPlayRate = 1.0f;
    private Snowman _currentSnowman = null;
    
    private Queue<SnowmanType> _snowmanSpawnQueue = new Queue<SnowmanType>();
    private ProductionData _productionData = new ProductionData();
    private ProductionPeriodData _productionPeriodData = new ProductionPeriodData();
    

    private bool _isProcessingGimmick = false;
    
    public event Action OnSnowmanSpawnEvent = null;
    public event Action OnSnowmanArrivalEvent = null;
    public event Action OnSnowmanDeathEvent = null; 
    
    public void Kill()
    {
        if (_isProcessingGimmick || _currentSnowman == null) return;
        
        // TODO : Random Gimmick Activation
        _isProcessingGimmick = true;
        gimmickPress.Activate();
    }

    public void Pass()
    {
        if (_isProcessingGimmick || _currentSnowman == null) return;
        
        _isProcessingGimmick = true;
        _currentSnowman.Stop();
        _currentSnowman.MoveImmediate();
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        CreateRandomMission();
        SpawnSnowman();
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
        _railAnimationPlayRate = 1.0f;
        
        gimmickPress.Init();
        gimmickPress.OnStartActivateEvent += () =>
        {
            _currentSnowman.Stop();
        };
        gimmickPress.OnFinishActivateEvent += () =>
        {
            OnSnowmanDeathEvent?.Invoke();
            Destroy(_currentSnowman.gameObject);
            animatorSnowmanUpperbody.runtimeAnimatorController = null;
        };
        gimmickPress.OnStartDeactivateEvent += () =>
        {
            SpawnSnowman();
        };
        gimmickPress.OnFinishDeactivateEvent += () =>
        {
            _isProcessingGimmick = false;
        };

        OnSnowmanArrivalEvent += () =>
        {
            _isProcessingGimmick = false;
        };
    }

    private void Release()
    {
        _railAnimationPlayRate = 1.0f;
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
        var data = Tables.Instance.GetTable<MissionTd>().GetData(id);
        if (data == null) return;
        
        Debug.LogWarning($"Start Mission ID : {data.id}, Goal : {data.goal}, Time : {data.time}");

        _totalTime = _currentTime = data.time;
        mainUI.SetTimerSlider(0.0f);
        _isPlaying = true;
        
        GenerateSpawnQueue(data);
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

    private void SpawnSnowman()
    {
        if (_snowmanSpawnQueue.Count <= 0 || !_isPlaying)
        {
            Debug.Log("모든 눈사람을 검수했거나, 게임이 종료되었습니다.");
            GameOver();
            return;
        }
        
        var idx = _productionData.totalQuantity - _snowmanSpawnQueue.Count;
    
        var moveDuration = _productionPeriodData.GetMoveDuration(idx);
        var spawnInterval = _productionPeriodData.GetSpawnInterval(idx);
        
        var type = _snowmanSpawnQueue.Dequeue();
        var data = GetSnowmanDataByType(type);
        
        SetupSnowmanAnimator(data);
        
        _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
        _currentSnowman.Init(data);
        _currentSnowman.Move(arrivalPoint.position, moveDuration, () =>
        {
            OnSnowmanArrivalEvent?.Invoke();
            DOVirtual.DelayedCall(spawnInterval, SpawnSnowman);
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
        var path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}";
        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            animatorSnowmanUpperbody.runtimeAnimatorController = controller;
        }
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