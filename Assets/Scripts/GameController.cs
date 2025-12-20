using System;
using System.Linq;
using Core.Scripts.Table;
using DG.Tweening;
using UnityEngine;

public struct MissionData
{
    public float time;
    public float amount;
}

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject snowmanPrefab = null; 
        
    [SerializeField] private Animator animatorSnowmanUpperbody = null;
    
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private Transform arrivalPoint = null;
    
    [SerializeField] private GimmickPress gimmickPress = null;

    [SerializeField] private float defectRate = 0.5f;
    
    private float _currentSnowmanMoveDuration = 3.0f;
    private float _railAnimationPlayRate = 1.0f;
    private Snowman _currentSnowman = null;

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
        
    }
    
    private void CreateRandomMission()
    {
        
    }

    private void SpawnSnowman()
    {
        var isFaulty = UnityEngine.Random.value < defectRate;
        string path;
        
        if (isFaulty)
        {
            var idCount = Tables.Instance.GetTable<NPSnowmanTd>().GetKeys.Count();
            var id = UnityEngine.Random.Range(0, idCount);
            var data = Tables.Instance.GetTable<NPSnowmanTd>().GetData(id);
            if (data == null)
            {
                Debug.LogError($"NPSnowmanTd Data Not Found. id:{id}");
                return;
            }
            path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}";
            _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
            _currentSnowman.Init(data);
        }
        else
        {
            var idCount = Tables.Instance.GetTable<PSnowmanTd>().GetKeys.Count();
            var id = UnityEngine.Random.Range(0, idCount);
            var data = Tables.Instance.GetTable<PSnowmanTd>().GetData(id);
            if (data == null)
            {
                Debug.LogError($"PSnowmanTd Data Not Found. id:{id}");
                return;
            }
            path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}";
            _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
            _currentSnowman.Init(data);
        }
        
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
                seq.AppendInterval(0.2f);
                seq.AppendCallback(SpawnSnowman);
            }
        };
        _currentSnowman.Move(movementData);
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