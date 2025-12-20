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
    public int targetYield;
    public int totalQuantity;
    public int normalQuantity;
    public int defectQuantity;
    public int spyQuantity;
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

    [SerializeField] private float defectRate = 0.5f;
    [SerializeField] private float snowmanSpawnInterval = 0.2f;

    private float _time = 0.0f;
    private float _currentSnowmanMoveDuration = 3.0f;
    private float _railAnimationPlayRate = 1.0f;
    private Snowman _currentSnowman = null;
    private Queue<SnowmanType> _snowmanSpawnQueue = new Queue<SnowmanType>();

    private bool _isProcessing = false;
    
    public event Action OnSnowmanSpawnEvent = null;
    public event Action OnSnowmanArrivalEvent = null;
    public event Action OnSnowmanDeathEvent = null; 
    
    public void Kill()
    {
        if (_isProcessing || _currentSnowman == null) return;
        
        // TODO : Random Gimmick Activation
        _isProcessing = true;
        gimmickPress.Activate();
    }

    public void Pass()
    {
        if (_isProcessing || _currentSnowman == null) return;
        
        _isProcessing = true;
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
            _isProcessing = false;
        };

        OnSnowmanArrivalEvent += () =>
        {
            _isProcessing = false;
        };
    }

    private void Release()
    {
        _railAnimationPlayRate = 1.0f;
        _isProcessing = false;
    }
    
    private void CreateRandomMission()
    {
        var idCount = Tables.Instance.GetTable<MissionTd>().GetKeys.Count();
        var id = UnityEngine.Random.Range(0, idCount);
        var data = Tables.Instance.GetTable<MissionTd>().GetData(id);
        if (data == null) return;

        _time = data.time;
        GenerateSpawnQueue(data.goal);
    }

    private void GenerateSpawnQueue(int goal)
    {
        var data = new ProductionData();
        data.targetYield = goal * 2;
        data.normalQuantity = (int)(goal * 0.6f);
        data.defectQuantity = (int)(data.normalQuantity * 0.3f);
        data.spyQuantity = (int)(data.normalQuantity * 0.1f);
        
        var list = new List<SnowmanType>();

        for (var i = 0; i < data.normalQuantity; i++) list.Add(SnowmanType.Normal);
        for (var i = 0; i < data.defectQuantity; i++) list.Add(SnowmanType.Defect);
        for (var i = 0; i < data.spyQuantity; i++) list.Add(SnowmanType.Spy);

        for (var i = 0; i < list.Count; i++)
        {
            var rnd = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }

        _snowmanSpawnQueue.Clear();
        foreach (var type in list)
        {
            _snowmanSpawnQueue.Enqueue(type);
        }
    }

    private void SpawnSnowman()
    {
        if (_snowmanSpawnQueue.Count <= 0)
        {
            Debug.Log("모든 눈사람을 검수했습니다! 스테이지 종료.");
            return;
        }
        
        var type = _snowmanSpawnQueue.Dequeue();
        var data = GetSnowmanDataByType(type);
        if (data == null)
        {
            
        }
        var path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}";
        
        _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
        _currentSnowman.Init(data);
        
        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            animatorSnowmanUpperbody.runtimeAnimatorController = controller;
        }
        
        var movementData = new MovementData
        {
            targetPosition = arrivalPoint.position,
            duration = _currentSnowmanMoveDuration,
            doneCallback = () =>
            {
                var seq = DOTween.Sequence();
                seq.AppendCallback(() =>
                {
                    OnSnowmanArrivalEvent?.Invoke();
                });
                seq.AppendInterval(snowmanSpawnInterval);
                seq.AppendCallback(SpawnSnowman);
            }
        };
        _currentSnowman.Move(movementData);
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